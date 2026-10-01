using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;

namespace ChuliTirth.Services;

// Development-mode notification services. They log instead of sending real
// email/WhatsApp so the app runs with zero external credentials. Swap the
// INotificationService registration in Program.cs for a real sender when ready.
public class MockEmailNotificationService : INotificationService
{
    private readonly ILogger<MockEmailNotificationService> _logger;

    public MockEmailNotificationService(ILogger<MockEmailNotificationService> logger) => _logger = logger;

    public Task NotifyAsync(NotificationEvent evt, Booking booking)
    {
        _logger.LogInformation("[MockEmail] {Event} -> {Email} for booking {BookingNumber}", evt, booking.Email, booking.BookingNumber);
        return Task.CompletedTask;
    }
}

public class MockWhatsAppNotificationService : INotificationService
{
    private readonly ILogger<MockWhatsAppNotificationService> _logger;

    public MockWhatsAppNotificationService(ILogger<MockWhatsAppNotificationService> logger) => _logger = logger;

    public Task NotifyAsync(NotificationEvent evt, Booking booking)
    {
        _logger.LogInformation("[MockWhatsApp] {Event} -> {Mobile} for booking {BookingNumber}", evt, booking.Mobile, booking.BookingNumber);
        return Task.CompletedTask;
    }
}

// Development-mode stand-in for a real mailer — logs the reset link instead of emailing it,
// same convention as MockEmailNotificationService above.
public class MockAuthMailer : IAuthMailer
{
    private readonly ILogger<MockAuthMailer> _logger;
    public MockAuthMailer(ILogger<MockAuthMailer> logger) => _logger = logger;

    public Task SendPasswordResetAsync(User user, string resetUrl)
    {
        _logger.LogInformation("[MockEmail] Password reset for {Email} -> {ResetUrl}", user.Email, resetUrl);
        return Task.CompletedTask;
    }
}

// Fans a single event out to every registered channel (email, WhatsApp, ...).
public class CompositeNotificationService : INotificationService
{
    private readonly IEnumerable<INotificationService> _channels;

    public CompositeNotificationService(IEnumerable<INotificationService> channels) => _channels = channels;

    public async Task NotifyAsync(NotificationEvent evt, Booking booking)
    {
        foreach (var channel in _channels)
        {
            await channel.NotifyAsync(evt, booking);
        }
    }
}
