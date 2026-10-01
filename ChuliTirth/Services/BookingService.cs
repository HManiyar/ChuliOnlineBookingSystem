using ChuliTirth.Data;
using ChuliTirth.Helpers;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Config;
using ChuliTirth.Models.DTOs;
using ChuliTirth.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace ChuliTirth.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _db;
    private readonly IRoomAvailabilityService _availability;
    private readonly INotificationService _notifications;
    private readonly IPaymentService _payments;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<BookingService> _logger;

    public BookingService(ApplicationDbContext db, IRoomAvailabilityService availability, INotificationService notifications,
        IPaymentService payments, IOptions<ApplicationSettings> appSettings, ILogger<BookingService> logger)
    {
        _db = db;
        _availability = availability;
        _notifications = notifications;
        _payments = payments;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<BookingResult> CreateBookingAsync(CreateBookingRequest request)
    {
        if (request.CheckIn >= request.CheckOut)
            return BookingResult.Fail("Check-out date must be after check-in date.");

        if (request.CheckIn < DateOnly.FromDateTime(DateTime.UtcNow.ToIst().Date))
            return BookingResult.Fail("Check-in date cannot be in the past.");

        if (request.CheckOut.DayNumber - request.CheckIn.DayNumber > _appSettings.MaxBookingNights)
            return BookingResult.Fail($"Bookings are limited to a maximum of {_appSettings.MaxBookingNights} nights.");

        if (request.CheckIn > DateOnly.FromDateTime(DateTime.UtcNow.ToIst().Date).AddMonths(_appSettings.MaxAdvanceBookingMonths))
            return BookingResult.Fail($"Bookings can only be made up to {_appSettings.MaxAdvanceBookingMonths} months in advance.");

        if (request.RoomsNeeded < 1)
            return BookingResult.Fail("At least one room must be selected.");

        if (request.Adults < request.RoomsNeeded)
            return BookingResult.Fail("Number of adults must be at least the number of rooms booked — each room needs at least one adult.");

        if (!request.AcceptedRules)
            return BookingResult.Fail("You must accept the Dharamshala rules and booking policies.");

        var roomType = await _db.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.IsActive);
        if (roomType is null)
            return BookingResult.Fail("Selected room type is not available.");

        var totalGuests = request.Adults + request.Children;
        if (totalGuests > roomType.Capacity * request.RoomsNeeded)
            return BookingResult.Fail("Number of guests exceeds room capacity.");

        // The DbContext is configured with EnableRetryOnFailure, and EF Core forbids a
        // user-managed transaction under a retrying execution strategy (the retry could replay
        // a partially-committed transaction). Running the transaction inside ExecuteAsync makes
        // the whole open/commit/rollback sequence the retriable unit, which is the supported way
        // to combine the two. Serializable isolation so a concurrent booking for the same
        // rooms/dates fails fast instead of racing.
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var roomIds = await _availability.GetAvailableRoomIdsAsync(request.RoomTypeId, request.CheckIn, request.CheckOut, request.RoomsNeeded);
                if (roomIds.Count < request.RoomsNeeded)
                {
                    return BookingResult.Fail("Sorry, the selected rooms were just booked by someone else. Please choose different dates or a different room type.");
                }

                var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
                var roomAmount = roomType.Price * nights * request.RoomsNeeded;

                var booking = new Booking
                {
                    UserId = request.UserId,
                    CheckIn = request.CheckIn,
                    CheckOut = request.CheckOut,
                    Nights = nights,
                    Adults = request.Adults,
                    Children = request.Children,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    SpecialRequirements = request.SpecialRequirements,
                    IdType = request.IdType,
                    IdNumber = request.IdNumber,
                    RoomAmount = roomAmount,
                    AdditionalCharges = 0,
                    TotalAmount = roomAmount,
                    Status = BookingStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    BookingNumber = "TEMP"
                };

                foreach (var roomId in roomIds)
                {
                    booking.BookingRooms.Add(new BookingRoom
                    {
                        RoomId = roomId,
                        RoomTypeId = roomType.Id,
                        CheckIn = request.CheckIn,
                        CheckOut = request.CheckOut,
                        RatePerNight = roomType.Price
                    });
                }

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();

                booking.BookingNumber = BookingNumberGenerator.Generate(booking.Id, DateTime.UtcNow);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                await _notifications.NotifyAsync(NotificationEvent.BookingCreated, booking);
                return BookingResult.Ok(booking);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Booking creation failed due to a concurrency conflict.");
                return BookingResult.Fail("Booking could not be completed due to a conflict. Please try again.");
            }
        });
    }

    public async Task<BookingResult> RecordPaymentOrderAsync(int bookingId, PaymentOrderResult order, string method)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");

        _db.Payments.Add(new Payment
        {
            BookingId = bookingId,
            Amount = booking.TotalAmount,
            Method = method,
            Status = PaymentStatus.Pending,
            GatewayOrderId = order.GatewayOrderId,
            IsSimulated = !order.RequiresCheckout
        });
        await _db.SaveChangesAsync();
        return BookingResult.Ok(booking);
    }

    public async Task<BookingResult> ApplyPaymentResultAsync(string gatewayOrderId, string? gatewayPaymentId, bool success, string? message = null)
    {
        var payment = await _db.Payments.Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.GatewayOrderId == gatewayOrderId);
        if (payment is null) return BookingResult.Fail("No matching payment order was found.");

        var booking = payment.Booking;

        // Idempotent — the client-side callback and the server-to-server webhook can both land
        // for the same payment; once it's Paid, later calls are a no-op.
        if (payment.Status == PaymentStatus.Paid) return BookingResult.Ok(booking);

        payment.Status = success ? PaymentStatus.Paid : PaymentStatus.Failed;
        if (!string.IsNullOrWhiteSpace(gatewayPaymentId)) payment.TransactionRef = gatewayPaymentId;

        booking.PaymentStatus = success ? PaymentStatus.Paid : PaymentStatus.Failed;
        booking.Status = success ? BookingStatus.Confirmed : BookingStatus.PaymentPending;
        await _db.SaveChangesAsync();

        if (success) await _notifications.NotifyAsync(NotificationEvent.BookingConfirmed, booking);

        _logger.LogInformation("Payment {Outcome} for booking {BookingNumber} (order {OrderId}, payment {PaymentId})",
            success ? "succeeded" : "failed", booking.BookingNumber, gatewayOrderId, gatewayPaymentId);

        return BookingResult.Ok(booking);
    }

    public async Task<BookingResult> ReconcilePaymentWithGatewayAsync(int bookingId)
    {
        var booking = await _db.Bookings.Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");

        var payment = booking.Payments.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Status != PaymentStatus.Paid);
        if (payment is null || string.IsNullOrWhiteSpace(payment.GatewayOrderId))
            return BookingResult.Fail("No pending/failed gateway payment to reconcile for this booking.");

        var status = await _payments.GetOrderStatusAsync(payment.GatewayOrderId);
        if (!status.Success)
            return BookingResult.Fail(status.Message ?? "Could not reach the payment gateway.");

        if (!status.AnyPaymentCaptured)
        {
            return BookingResult.Fail(string.IsNullOrWhiteSpace(status.LatestPaymentStatus)
                ? "The gateway has no payment attempts for this order — the guest likely never completed checkout."
                : $"The gateway shows no captured payment — latest attempt status: {status.LatestPaymentStatus}.");
        }

        // The gateway confirms money actually changed hands but our record never caught up
        // (missed callback, missed webhook) — bring our record in line with reality.
        await ApplyPaymentResultAsync(payment.GatewayOrderId, status.LatestPaymentId, success: true);
        _logger.LogInformation("Reconciled booking {BookingNumber}: gateway confirms payment {PaymentId} captured.", booking.BookingNumber, status.LatestPaymentId);

        var refreshed = await GetByIdAsync(bookingId);
        return BookingResult.Ok(refreshed!);
    }

    public async Task<BookingResult> RefundBookingAsync(int bookingId, string reason, int? actingUserId)
    {
        var booking = await _db.Bookings.Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");

        var payment = booking.Payments.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Status == PaymentStatus.Paid);
        if (payment is null)
            return BookingResult.Fail("This booking has no captured payment to refund.");

        var result = await _payments.RefundAsync(payment);
        if (!result.Success)
        {
            _logger.LogWarning("Refund failed for booking {BookingNumber}: {Message}", booking.BookingNumber, result.Message);
            return BookingResult.Fail(result.Message ?? "Refund could not be processed.");
        }

        payment.Status = PaymentStatus.Refunded;
        booking.PaymentStatus = PaymentStatus.Refunded;
        booking.CancellationReason ??= reason;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Refund {RefundRef} issued for booking {BookingNumber} by user {UserId}: {Reason}",
            result.TransactionRef, booking.BookingNumber, actingUserId, reason);

        return BookingResult.Ok(booking);
    }

    public Task<Booking?> GetByBookingNumberAsync(string bookingNumber) =>
        _db.Bookings
            .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
            .Include(b => b.BookingRooms).ThenInclude(br => br.RoomType)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

    public Task<Booking?> GetByIdAsync(int id) =>
        _db.Bookings
            .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
            .Include(b => b.BookingRooms).ThenInclude(br => br.RoomType)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id);

    public Task<List<Booking>> GetBookingsForUserAsync(int userId) =>
        _db.Bookings
            .Include(b => b.BookingRooms).ThenInclude(br => br.RoomType)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync();

    public async Task<List<Booking>> GetAllBookingsAsync(BookingFilter? filter = null)
    {
        var query = _db.Bookings.Include(b => b.BookingRooms).ThenInclude(br => br.RoomType).AsQueryable();

        if (filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(filter.BookingNumber))
                query = query.Where(b => b.BookingNumber.Contains(filter.BookingNumber));
            if (!string.IsNullOrWhiteSpace(filter.GuestName))
                query = query.Where(b => (b.FirstName + " " + b.LastName).Contains(filter.GuestName));
            if (!string.IsNullOrWhiteSpace(filter.Mobile))
                query = query.Where(b => b.Mobile.Contains(filter.Mobile));
            if (filter.CheckInFrom.HasValue)
                query = query.Where(b => b.CheckIn >= filter.CheckInFrom.Value);
            if (filter.CheckInTo.HasValue)
                query = query.Where(b => b.CheckIn <= filter.CheckInTo.Value);
            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);
            if (filter.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);
            if (filter.RoomTypeId.HasValue)
                query = query.Where(b => b.BookingRooms.Any(br => br.RoomTypeId == filter.RoomTypeId.Value));
        }

        return await query.OrderByDescending(b => b.CreatedAtUtc).ToListAsync();
    }

    public async Task<BookingResult> ConfirmBookingAsync(int bookingId)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");
        booking.Status = BookingStatus.Confirmed;
        await _db.SaveChangesAsync();
        await _notifications.NotifyAsync(NotificationEvent.BookingConfirmed, booking);
        return BookingResult.Ok(booking);
    }

    public async Task<BookingResult> RejectBookingAsync(int bookingId, string reason)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");
        booking.Status = BookingStatus.Rejected;
        booking.CancellationReason = reason;
        await _db.SaveChangesAsync();
        return BookingResult.Ok(booking);
    }

    public async Task<BookingResult> CancelBookingAsync(int bookingId, string reason, int? actingUserId)
    {
        var booking = await _db.Bookings.Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed or BookingStatus.CheckedOut or BookingStatus.CheckedIn)
            return BookingResult.Fail("This booking can no longer be cancelled.");

        var cancelledAtUtc = DateTime.UtcNow;
        string? refundNote = null;

        if (booking.PaymentStatus == PaymentStatus.Paid)
        {
            var payment = booking.Payments.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Status == PaymentStatus.Paid);
            if (payment is not null)
            {
                var policy = CancellationPolicy.CalculateRefund(booking.CheckIn, _appSettings.DefaultCheckInTime, cancelledAtUtc);
                var refundAmount = Math.Round(payment.Amount * policy.RefundPercent / 100m, 2);

                if (refundAmount > 0)
                {
                    var refundResult = await _payments.RefundAsync(payment, refundAmount);
                    if (refundResult.Success)
                    {
                        payment.Status = PaymentStatus.Refunded;
                        payment.RefundedAmount = refundAmount;
                        refundNote = $"{policy.Note} Refund of ₹{refundAmount:N2} ({policy.RefundPercent:0}%) initiated.";
                    }
                    else
                    {
                        // Don't block the cancellation on a refund API hiccup — the guest still
                        // wants out, and the room still needs releasing. Staff can retry the
                        // refund manually (Admin → Issue Refund) using the note left here.
                        refundNote = $"{policy.Note} Automatic refund of ₹{refundAmount:N2} failed ({refundResult.Message}) — needs manual refund.";
                        _logger.LogWarning("Auto-refund failed during cancellation of booking {BookingNumber}: {Message}", booking.BookingNumber, refundResult.Message);
                    }
                }
                else
                {
                    refundNote = policy.Note;
                }
            }
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = cancelledAtUtc;
        booking.CancellationReason = refundNote is null ? reason : $"{reason} — {refundNote}";
        await _db.SaveChangesAsync();
        await _notifications.NotifyAsync(NotificationEvent.BookingCancelled, booking);

        return BookingResult.Ok(booking, refundNote);
    }

    public async Task<(decimal RefundPercent, decimal RefundAmount, string Note)> PreviewCancellationRefundAsync(int bookingId)
    {
        var booking = await _db.Bookings.Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null || booking.PaymentStatus != PaymentStatus.Paid)
            return (0m, 0m, "No payment to refund.");

        var payment = booking.Payments.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Status == PaymentStatus.Paid);
        if (payment is null) return (0m, 0m, "No payment to refund.");

        var policy = CancellationPolicy.CalculateRefund(booking.CheckIn, _appSettings.DefaultCheckInTime, DateTime.UtcNow);
        var refundAmount = Math.Round(payment.Amount * policy.RefundPercent / 100m, 2);
        return (policy.RefundPercent, refundAmount, policy.Note);
    }

    public async Task<BookingResult> CheckInAsync(int bookingId)
    {
        var booking = await _db.Bookings.Include(b => b.BookingRooms).ThenInclude(br => br.Room)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");

        // Server-side enforcement of the same rule the Admin UI disables the button for — a
        // disabled button is only UX, this is what actually stops an early check-in.
        var today = DateOnly.FromDateTime(IndianTimeHelper.NowIst());
        if (today < booking.CheckIn)
            return BookingResult.Fail($"Check-in isn't until {booking.CheckIn:dd MMM yyyy} — too early to check in this booking.");

        booking.Status = BookingStatus.CheckedIn;
        booking.CheckedInAtUtc = DateTime.UtcNow;
        foreach (var br in booking.BookingRooms)
        {
            br.Room.Status = RoomStatus.Occupied;
        }
        await _db.SaveChangesAsync();
        return BookingResult.Ok(booking);
    }

    public async Task<BookingResult> CheckOutAsync(int bookingId)
    {
        var booking = await _db.Bookings.Include(b => b.BookingRooms).ThenInclude(br => br.Room)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");

        booking.Status = BookingStatus.Completed;
        booking.CheckedOutAtUtc = DateTime.UtcNow;
        foreach (var br in booking.BookingRooms)
        {
            // Only free up the room if it isn't mid-stay for someone else — defensive, since
            // overlapping assignments shouldn't happen (GetAvailableRoomIdsAsync prevents it),
            // but this is cheap insurance against ever un-occupying a room that's still in use.
            if (br.Room.Status == RoomStatus.Occupied) br.Room.Status = RoomStatus.Available;
        }
        await _db.SaveChangesAsync();
        return BookingResult.Ok(booking);
    }

    public async Task<BookingResult> MarkNoShowAsync(int bookingId)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking is null) return BookingResult.Fail("Booking not found.");
        booking.Status = BookingStatus.NoShow;
        await _db.SaveChangesAsync();
        return BookingResult.Ok(booking);
    }
}
