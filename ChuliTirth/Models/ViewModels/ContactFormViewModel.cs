using System.ComponentModel.DataAnnotations;

namespace ChuliTirth.Models.ViewModels;

public class ContactFormViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Mobile { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;
}
