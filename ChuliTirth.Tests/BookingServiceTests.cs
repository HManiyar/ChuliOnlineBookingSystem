using ChuliTirth.Models.Config;
using ChuliTirth.Models.DTOs;
using ChuliTirth.Models.Entities;
using ChuliTirth.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ChuliTirth.Tests;

public class BookingServiceTests
{
    private static (BookingService sut, RoomType roomType) MakeSut(int rooms = 1)
    {
        var db = TestDbContextFactory.Create();
        var rt = new RoomType { Name = "AC Room", Capacity = 3, BedCount = 2, Price = 700, IsActive = true };
        db.RoomTypes.Add(rt);
        db.SaveChanges();

        for (var i = 1; i <= rooms; i++)
        {
            db.Rooms.Add(new Room { RoomNumber = $"10{i}", Status = RoomStatus.Available, IsActive = true });
        }
        db.SaveChanges();

        var availability = new RoomAvailabilityService(db);
        var payments = new MockPaymentService(NullLogger<MockPaymentService>.Instance);
        var appSettings = Options.Create(new ApplicationSettings());
        var sut = new BookingService(db, availability, new NoopNotificationService(), payments, appSettings, NullLogger<BookingService>.Instance);
        return (sut, rt);
    }

    private static CreateBookingRequest ValidRequest(RoomType rt, DateOnly checkIn, DateOnly checkOut) => new()
    {
        RoomTypeId = rt.Id,
        CheckIn = checkIn,
        CheckOut = checkOut,
        RoomsNeeded = 1,
        Adults = 2,
        FirstName = "Test",
        LastName = "Pilgrim",
        Mobile = "9999999999",
        Email = "test@example.com",
        AcceptedRules = true
    };

    [Fact]
    public async Task CreateBooking_Fails_WhenCheckOutNotAfterCheckIn()
    {
        var (sut, rt) = MakeSut();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var request = ValidRequest(rt, date, date);

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
        Assert.Contains("Check-out", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBooking_Fails_WhenCheckInInPast()
    {
        var (sut, rt) = MakeSut();
        var request = ValidRequest(rt, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
        Assert.Contains("past", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBooking_Fails_WhenRulesNotAccepted()
    {
        var (sut, rt) = MakeSut();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(2));
        request.AcceptedRules = false;

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateBooking_Fails_WhenStayExceedsMaxNights()
    {
        var (sut, rt) = MakeSut();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(4)); // default max is 3 nights

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
        Assert.Contains("nights", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBooking_Fails_WhenCheckInTooFarInAdvance()
    {
        var (sut, rt) = MakeSut();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(7)); // default max is 6 months
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(2));

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
        Assert.Contains("advance", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBooking_Fails_WhenRoomsExceedAdults()
    {
        var (sut, rt) = MakeSut(rooms: 2);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(2));
        request.RoomsNeeded = 2;
        request.Adults = 1; // 2 rooms but only 1 adult — invalid, each room needs an adult

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
        Assert.Contains("adult", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBooking_Fails_WhenGuestsExceedCapacity()
    {
        var (sut, rt) = MakeSut();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(2));
        request.Adults = 10; // room capacity is 3

        var result = await sut.CreateBookingAsync(request);

        Assert.False(result.Success);
        Assert.Contains("capacity", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBooking_Succeeds_AndGeneratesBookingNumber()
    {
        var (sut, rt) = MakeSut();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(2));

        var result = await sut.CreateBookingAsync(request);

        Assert.True(result.Success);
        Assert.StartsWith("CT-", result.Booking!.BookingNumber);
        Assert.Equal(700 * 2, result.Booking.TotalAmount);
        Assert.Single(result.Booking.BookingRooms);
    }

    [Fact]
    public async Task CreateBooking_PreventsDoubleBooking_WhenOnlyOneRoomLeft()
    {
        var (sut, rt) = MakeSut(rooms: 1);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = ValidRequest(rt, checkIn, checkIn.AddDays(2));

        var first = await sut.CreateBookingAsync(request);
        Assert.True(first.Success);

        var second = await sut.CreateBookingAsync(request);

        Assert.False(second.Success);
        Assert.Contains("booked", second.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
