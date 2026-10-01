using ChuliTirth.Models.DTOs;
using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public class BookingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    // Optional success-path info for the caller to surface (e.g. the refund amount/percentage a
    // cancellation worked out to) — distinct from ErrorMessage, which is failure-only.
    public string? Message { get; set; }
    public Booking? Booking { get; set; }

    public static BookingResult Ok(Booking booking, string? message = null) => new() { Success = true, Booking = booking, Message = message };
    public static BookingResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

public interface IBookingService
{
    Task<BookingResult> CreateBookingAsync(CreateBookingRequest request);
    Task<Booking?> GetByBookingNumberAsync(string bookingNumber);
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetBookingsForUserAsync(int userId);
    Task<List<Booking>> GetAllBookingsAsync(BookingFilter? filter = null);

    // Records a Payment row for an order just opened with the gateway (status Pending).
    Task<BookingResult> RecordPaymentOrderAsync(int bookingId, PaymentOrderResult order, string method);

    // Applies a (verified) payment outcome — from either the checkout callback or the webhook —
    // to the Payment/Booking matching the given gateway order id. Idempotent: a Payment already
    // marked Paid is left untouched, so the callback and webhook can safely race.
    Task<BookingResult> ApplyPaymentResultAsync(string gatewayOrderId, string? gatewayPaymentId, bool success, string? message = null);

    // Asks the gateway for ground truth on a booking's payment and reconciles our local record
    // to match — for when the checkout callback and the webhook both missed a payment that
    // actually succeeded (closed browser, webhook not configured yet, etc.).
    Task<BookingResult> ReconcilePaymentWithGatewayAsync(int bookingId);

    // Manual full-refund override — for anything CancelBookingAsync's automatic policy-based
    // refund doesn't cover (e.g. a goodwill refund on a booking that isn't being cancelled).
    Task<BookingResult> RefundBookingAsync(int bookingId, string reason, int? actingUserId);

    Task<BookingResult> ConfirmBookingAsync(int bookingId);
    Task<BookingResult> RejectBookingAsync(int bookingId, string reason);

    // Cancels the booking and, if it was Paid, automatically refunds it per CancellationPolicy
    // (100% at 7+ days out, 50% at 48h–7d, 0% inside 48h) — see Helpers/CancellationPolicy.cs.
    // The result's Message carries the refund outcome for the caller to show the guest.
    Task<BookingResult> CancelBookingAsync(int bookingId, string reason, int? actingUserId);

    // Preview only, doesn't change anything — "if you cancel now, you'd get back ₹X (Y%)" for the UI.
    Task<(decimal RefundPercent, decimal RefundAmount, string Note)> PreviewCancellationRefundAsync(int bookingId);
    Task<BookingResult> CheckInAsync(int bookingId);
    Task<BookingResult> CheckOutAsync(int bookingId);
    Task<BookingResult> MarkNoShowAsync(int bookingId);
}

public class BookingFilter
{
    public string? BookingNumber { get; set; }
    public string? GuestName { get; set; }
    public string? Mobile { get; set; }
    public DateOnly? CheckInFrom { get; set; }
    public DateOnly? CheckInTo { get; set; }
    public BookingStatus? Status { get; set; }
    public int? RoomTypeId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
}
