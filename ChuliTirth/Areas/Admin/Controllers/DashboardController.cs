using ChuliTirth.Data;
using ChuliTirth.Models.Entities;
using ChuliTirth.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var vm = new AdminDashboardViewModel
        {
            TodayCheckIns = await _db.Bookings.CountAsync(b => b.CheckIn == today && b.Status == BookingStatus.Confirmed),
            TodayCheckOuts = await _db.Bookings.CountAsync(b => b.CheckOut == today && b.Status == BookingStatus.CheckedIn),
            CurrentGuests = await _db.Bookings.Where(b => b.Status == BookingStatus.CheckedIn).SumAsync(b => (int?)(b.Adults + b.Children)) ?? 0,
            AvailableRooms = await _db.Rooms.CountAsync(r => r.IsActive && r.Status == RoomStatus.Available),
            OccupiedRooms = await _db.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied),
            PendingBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.PaymentPending),
            ConfirmedBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed),
            CancelledBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled),
            RevenueThisMonth = await _db.Bookings
                .Where(b => b.CreatedAtUtc >= monthStart.ToDateTime(TimeOnly.MinValue) && b.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0,
            RecentBookings = await _db.Bookings.OrderByDescending(b => b.CreatedAtUtc).Take(10).ToListAsync()
        };

        return View(vm);
    }
}
