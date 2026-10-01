using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public record RoomTypeAvailability(RoomType RoomType, int TotalRooms, int AvailableRooms);

public interface IRoomAvailabilityService
{
    /// Returns room types with at least one free room for the given date range, guest count and room quantity.
    Task<List<RoomTypeAvailability>> SearchAvailabilityAsync(DateOnly checkIn, DateOnly checkOut, int guests, int roomsNeeded, int? roomTypeId = null);

    /// Number of physical rooms of a type still free for the date range (excludes overlapping bookings and blocked/maintenance/inactive rooms).
    Task<int> GetAvailableRoomCountAsync(int roomTypeId, DateOnly checkIn, DateOnly checkOut);

    /// Concrete free room ids for a type/date range, used to allocate rooms to a booking.
    Task<List<int>> GetAvailableRoomIdsAsync(int roomTypeId, DateOnly checkIn, DateOnly checkOut, int take);

    /// Day-by-day availability status for a room type, used to render the calendar.
    Task<Dictionary<DateOnly, string>> GetCalendarAsync(int roomTypeId, DateOnly fromDate, DateOnly toDate);
}
