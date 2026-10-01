using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string fullName, string email, string mobile, string password);
    Task<AuthResult> ValidateCredentialsAsync(string emailOrMobile, string password);
    Task<User?> GetByIdAsync(int id);

    // Always "succeeds" from the caller's point of view (even for an unknown email) so the
    // ForgotPassword page can't be used to enumerate registered accounts. User/Token are null
    // when no matching active account exists — the caller is responsible for emailing the token.
    Task<(User? User, string? Token)> GeneratePasswordResetTokenAsync(string email);

    Task<AuthResult> ResetPasswordAsync(int userId, string token, string newPassword);
}
