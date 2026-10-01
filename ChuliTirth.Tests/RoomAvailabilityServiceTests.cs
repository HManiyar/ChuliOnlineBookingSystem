using ChuliTirth.Models.Entities;
using ChuliTirth.Services;
using Xunit;

namespace ChuliTirth.Tests;

public class RoomAvailabilityServiceTests
{
    private static RoomType MakeRoomType(int rooms, out ChuliTirth.Data.ApplicationDbContext db)
    {
        db = TestDbContextFactory.Create();
        var rt = new RoomType { Name = "AC Room", Capacity = 3, BedCount = 2, Price = 700, IsActive = true };
        db.RoomTypes.Add(rt);
        db.SaveChanges();

        for (var i = 1; i <= rooms; i++)
        {
            db.Rooms.Add(new Room { RoomTypeId = rt.Id, RoomNumber = $"10{i}", Status = RoomStatus.Available, IsActive = true });
        }
        db.SaveChanges();
        return rt;
    }

    [Fact]
    public async Task GetAvailableRoomCount_ReturnsAllRooms_WhenNoBookings()
    {
        var roomType = MakeRoomType(5, out var db);
        var sut = new RoomAvailabilityService(db);

        var count = await sut.GetAvailableRoomCountAsync(roomType.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 3));

        Assert.Equal(5, count);
    }

    [Fact]
    public async Task GetAvailableRoomCount_ExcludesOverlappingConfirmedBooking()
    {
        var roomType = MakeRoomType(2, out var db);
        var room = db.Rooms.First();

        var booking = new Booking
        {
            BookingNumber = "CT-2026-000001", Status = BookingStatus.Confirmed,
            CheckIn = new DateOnly(2026, 10, 1), CheckOut = new DateOnly(2026, 10, 5),
            FirstName = "A", LastName = "B", Mobile = "1", Email = "a@b.com"
        };
        booking.BookingRooms.Add(new BookingRoom { RoomId = room.Id, RoomTypeId = roomType.Id, CheckIn = booking.CheckIn, CheckOut = booking.CheckOut, RatePerNight = 700 });
        db.Bookings.Add(booking);
        db.SaveChanges();

        var sut = new RoomAvailabilityService(db);

        // Overlaps the existing booking (2 nights within 1-5 Oct) -> only 1 of 2 rooms free.
        var overlapping = await sut.GetAvailableRoomCountAsync(roomType.Id, new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 4));
        Assert.Equal(1, overlapping);

        // Entirely after the existing booking -> both rooms free.
        var afterward = await sut.GetAvailableRoomCountAsync(roomType.Id, new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 7));
        Assert.Equal(2, afterward);
    }

    [Fact]
    public async Task GetAvailableRoomCount_IgnoresCancelledBookings()
    {
        var roomType = MakeRoomType(1, out var db);
        var room = db.Rooms.First();

        var booking = new Booking
        {
            BookingNumber = "CT-2026-000002", Status = BookingStatus.Cancelled,
            CheckIn = new DateOnly(2026, 10, 1), CheckOut = new DateOnly(2026, 10, 5),
            FirstName = "A", LastName = "B", Mobile = "1", Email = "a@b.com"
        };
        booking.BookingRooms.Add(new BookingRoom { RoomId = room.Id, RoomTypeId = roomType.Id, CheckIn = booking.CheckIn, CheckOut = booking.CheckOut, RatePerNight = 700 });
        db.Bookings.Add(booking);
        db.SaveChanges();

        var sut = new RoomAvailabilityService(db);
        var count = await sut.GetAvailableRoomCountAsync(roomType.Id, new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 4));

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetAvailableRoomCount_ExcludesBlockedAndMaintenanceRooms()
    {
        var roomType = MakeRoomType(3, out var db);
        var rooms = db.Rooms.ToList();
        rooms[0].Status = RoomStatus.Maintenance;
        rooms[1].Status = RoomStatus.Blocked;
        db.SaveChanges();

        var sut = new RoomAvailabilityService(db);
        var count = await sut.GetAvailableRoomCountAsync(roomType.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 2));

        Assert.Equal(1, count);
    }
}
