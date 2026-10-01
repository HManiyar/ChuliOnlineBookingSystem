using ChuliTirth.Models.Entities;

namespace ChuliTirth.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int CurrentGuests { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<Booking> RecentBookings { get; set; } = new();
}

public class ContactMessageAdminViewModel
{
    public List<ContactMessage> Messages { get; set; } = new();
}
