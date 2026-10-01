namespace ChuliTirth.Models.Entities;

// Lightweight custom auth (dev/testing mode) — not ASP.NET Core Identity.
// Passwords hashed with Microsoft.AspNetCore.Identity.PasswordHasher<User>.
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Guest;

    // Failed-login lockout (see AuthService) — a simple counter + expiry, not a full Identity
    // lockout store, but enough to blunt password-guessing against a public login form.
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }

    // Password reset — only a hash of the token is stored (never the raw token, which only ever
    // exists in the emailed link), with a short expiry.
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresUtc { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
