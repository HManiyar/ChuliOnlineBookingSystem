using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;

namespace ChuliTirth.Tests;

public class NoopNotificationService : INotificationService
{
    public Task NotifyAsync(NotificationEvent evt, Booking booking) => Task.CompletedTask;
}
