using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

// Separate from INotificationService because that interface is Booking-bound — auth emails
// (password reset) aren't tied to a booking. Swap the registration in Program.cs for a real
// sender alongside INotificationService when ready.
public interface IAuthMailer
{
    Task SendPasswordResetAsync(User user, string resetUrl);
}
