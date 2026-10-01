using System.Security.Cryptography;
using System.Text;
using ChuliTirth.Data;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Services;

// Lightweight cookie-based auth for development/testing — not ASP.NET Core Identity.
// Uses Identity's PasswordHasher<T> only (no UserManager/EF stores).
public class AuthService : IAuthService
{
    // After this many consecutive failed attempts, the account is locked for LockoutDuration.
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(ApplicationDbContext db) => _db = db;

    public async Task<AuthResult> RegisterAsync(string fullName, string email, string mobile, string password)
    {
        email = email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return new AuthResult { Success = false, ErrorMessage = "An account with this email already exists." };

        var user = new User { FullName = fullName, Email = email, Mobile = mobile, Role = UserRole.Guest };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return new AuthResult { Success = true, User = user };
    }

    public async Task<AuthResult> ValidateCredentialsAsync(string emailOrMobile, string password)
    {
        var key = emailOrMobile.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == key || u.Mobile == emailOrMobile);
        if (user is null || !user.IsActive)
            return new AuthResult { Success = false, ErrorMessage = "Invalid credentials." };

        if (user.LockoutEndUtc is { } lockoutEnd && lockoutEnd > DateTime.UtcNow)
        {
            var minutesLeft = Math.Ceiling((lockoutEnd - DateTime.UtcNow).TotalMinutes);
            return new AuthResult { Success = false, ErrorMessage = $"Too many failed attempts. Try again in {minutesLeft:0} minute(s)." };
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEndUtc = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
            }
            await _db.SaveChangesAsync();
            return new AuthResult { Success = false, ErrorMessage = "Invalid credentials." };
        }

        if (user.FailedLoginAttempts > 0 || user.LockoutEndUtc.HasValue)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEndUtc = null;
            await _db.SaveChangesAsync();
        }

        return new AuthResult { Success = true, User = user };
    }

    public Task<User?> GetByIdAsync(int id) => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<(User? User, string? Token)> GeneratePasswordResetTokenAsync(string email)
    {
        var key = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == key);
        if (user is null || !user.IsActive) return (null, null);

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        user.PasswordResetTokenHash = Hash(rawToken);
        user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.Add(ResetTokenLifetime);
        await _db.SaveChangesAsync();

        return (user, rawToken);
    }

    public async Task<AuthResult> ResetPasswordAsync(int userId, string token, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null || string.IsNullOrEmpty(user.PasswordResetTokenHash) || user.PasswordResetTokenExpiresUtc is null)
            return new AuthResult { Success = false, ErrorMessage = "This password reset link is invalid or has expired." };

        if (user.PasswordResetTokenExpiresUtc < DateTime.UtcNow)
            return new AuthResult { Success = false, ErrorMessage = "This password reset link is invalid or has expired." };

        var expectedHash = Encoding.UTF8.GetBytes(user.PasswordResetTokenHash);
        var actualHash = Encoding.UTF8.GetBytes(Hash(token));
        if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
            return new AuthResult { Success = false, ErrorMessage = "This password reset link is invalid or has expired." };

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresUtc = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        await _db.SaveChangesAsync();

        return new AuthResult { Success = true, User = user };
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexStringLower(bytes);
    }
}
