using System.ComponentModel.DataAnnotations;

namespace ChuliTirth.Models.ViewModels;

public class RegisterViewModel
{
    [Required, Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, Display(Name = "Mobile Number")]
    public string Mobile { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class LoginViewModel
{
    [Required, Display(Name = "Email or Mobile")]
    public string EmailOrMobile { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6), Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Confirm New Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
