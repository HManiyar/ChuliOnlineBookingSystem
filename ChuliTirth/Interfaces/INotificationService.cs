using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public enum NotificationEvent
{
    BookingCreated,
    BookingConfirmed,
    BookingCancelled,
    PaymentSuccessful,
    CheckInReminder,
    CheckOutReminder
}

public interface INotificationService
{
    Task NotifyAsync(NotificationEvent evt, Booking booking);
}
