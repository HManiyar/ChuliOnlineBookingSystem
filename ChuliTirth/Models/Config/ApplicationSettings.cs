namespace ChuliTirth.Models.Config;

public class ApplicationSettings
{
    public const string SectionName = "ApplicationSettings";

    public string DharamshalaName { get; set; } = "Chuli Tirth Dharamshala";
    public string DefaultCulture { get; set; } = "gu-IN";
    public List<string> SupportedCultures { get; set; } = new() { "gu-IN", "hi-IN", "en-US" };
    public string Currency { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public bool BookingEnabled { get; set; } = true;
    public bool PaymentEnabled { get; set; }
    public bool PaymentBypassInDevelopment { get; set; } = true;
    public bool EmailEnabled { get; set; }
    public bool WhatsAppEnabled { get; set; }
    public bool ExternalJainCalendarApiEnabled { get; set; }
    public string DefaultCheckInTime { get; set; } = "12:00";
    public string DefaultCheckOutTime { get; set; } = "10:00";
    public string TimeZoneId { get; set; } = "India Standard Time";

    // Booking window limits — enforced in BookingController (for a friendly error before the
    // guest fills the whole form) and again in BookingService.CreateBookingAsync (the
    // authoritative, un-bypassable gate).
    public int MaxBookingNights { get; set; } = 3;
    public int MaxAdvanceBookingMonths { get; set; } = 6;
}

public class SeedAdminSettings
{
    public const string SectionName = "SeedAdmin";
    public string Email { get; set; } = "admin@chulitirth.local";
    public string Password { get; set; } = "Admin@12345";
    public string FullName { get; set; } = "Dharamshala Administrator";
}

// KeyId is public (used client-side by Razorpay Checkout.js) and fine in appsettings.json.
// KeySecret and WebhookSecret are credentials — set only via `dotnet user-secrets` (dev) or
// environment variables / a secret store (production). Never commit them.
public class RazorpaySettings
{
    public const string SectionName = "Razorpay";
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(KeySecret);
}
