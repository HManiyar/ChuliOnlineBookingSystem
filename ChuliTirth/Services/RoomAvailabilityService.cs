using ChuliTirth.Data;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Services;

public class RoomAvailabilityService : IRoomAvailabilityService
{
    private readonly ApplicationDbContext _db;

    // Booking statuses that still hold a room's inventory.
    private static readonly BookingStatus[] BlockingStatuses =
    {
        BookingStatus.Pending, BookingStatus.PaymentPending, BookingStatus.Confirmed,
        BookingStatus.CheckedIn, BookingStatus.CheckedOut, BookingStatus.Completed
    };

    // Room statuses that never accept a booking regardless of date overlap.
    private static readonly RoomStatus[] UnbookableRoomStatuses =
    {
        RoomStatus.Blocked, RoomStatus.Maintenance, RoomStatus.Inactive
    };

    public RoomAvailabilityService(ApplicationDbContext db) => _db = db;

    public async Task<List<RoomTypeAvailability>> SearchAvailabilityAsync(DateOnly checkIn, DateOnly checkOut, int guests, int roomsNeeded, int? roomTypeId = null)
    {
        var query = _db.RoomTypes.Where(rt => rt.IsActive);
        if (roomTypeId.HasValue) query = query.Where(rt => rt.Id == roomTypeId.Value);
        if (guests > 0) query = query.Where(rt => rt.Capacity >= guests);

        var roomTypes = await query.Include(rt => rt.Images).OrderBy(rt => rt.DisplayOrder).AsNoTracking().ToListAsync();

        var results = new List<RoomTypeAvailability>();
        foreach (var rt in roomTypes)
        {
            var totalRooms = await _db.Rooms.CountAsync(r => r.RoomTypeId == rt.Id && r.IsActive && !UnbookableRoomStatuses.Contains(r.Status));
            var available = await GetAvailableRoomCountAsync(rt.Id, checkIn, checkOut);
            if (available >= roomsNeeded)
            {
                results.Add(new RoomTypeAvailability(rt, totalRooms, available));
            }
        }
        return results;
    }

    public async Task<int> GetAvailableRoomCountAsync(int roomTypeId, DateOnly checkIn, DateOnly checkOut)
    {
        var bookableRoomIds = await _db.Rooms
            .Where(r => r.RoomTypeId == roomTypeId && r.IsActive && !UnbookableRoomStatuses.Contains(r.Status))
            .Select(r => r.Id)
            .ToListAsync();

        if (bookableRoomIds.Count == 0) return 0;

        var occupiedRoomIds = await _db.BookingRooms
            .Where(br => bookableRoomIds.Contains(br.RoomId)
                && br.CheckIn < checkOut && checkIn < br.CheckOut
                && BlockingStatuses.Contains(br.Booking.Status))
            .Select(br => br.RoomId)
            .Distinct()
            .ToListAsync();

        return bookableRoomIds.Count - occupiedRoomIds.Count;
    }

    public async Task<List<int>> GetAvailableRoomIdsAsync(int roomTypeId, DateOnly checkIn, DateOnly checkOut, int take)
    {
        var bookableRoomIds = await _db.Rooms
            .Where(r => r.RoomTypeId == roomTypeId && r.IsActive && !UnbookableRoomStatuses.Contains(r.Status))
            .Select(r => r.Id)
            .ToListAsync();

        var occupiedRoomIds = await _db.BookingRooms
            .Where(br => bookableRoomIds.Contains(br.RoomId)
                && br.CheckIn < checkOut && checkIn < br.CheckOut
                && BlockingStatuses.Contains(br.Booking.Status))
            .Select(br => br.RoomId)
            .Distinct()
            .ToListAsync();

        return bookableRoomIds.Except(occupiedRoomIds).Take(take).ToList();
    }

    public async Task<Dictionary<DateOnly, string>> GetCalendarAsync(int roomTypeId, DateOnly fromDate, DateOnly toDate)
    {
        var result = new Dictionary<DateOnly, string>();
        var totalRooms = await _db.Rooms.CountAsync(r => r.RoomTypeId == roomTypeId && r.IsActive && !UnbookableRoomStatuses.Contains(r.Status));

        for (var day = fromDate; day <= toDate; day = day.AddDays(1))
        {
            if (totalRooms == 0)
            {
                result[day] = "blocked";
                continue;
            }
            var available = await GetAvailableRoomCountAsync(roomTypeId, day, day.AddDays(1));
            result[day] = available <= 0 ? "full" : (available < totalRooms ? "partial" : "available");
        }
        return result;
    }
}
