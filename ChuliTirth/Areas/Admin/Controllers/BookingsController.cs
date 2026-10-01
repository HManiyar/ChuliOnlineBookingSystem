using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

public class BookingsController : AdminControllerBase
{
    private readonly IBookingService _bookingService;
    public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

    public async Task<IActionResult> Index(string? bookingNumber, string? guestName, string? mobile,
        DateOnly? checkInFrom, DateOnly? checkInTo, BookingStatus? status, PaymentStatus? paymentStatus, int? roomTypeId)
    {
        var filter = new BookingFilter
        {
            BookingNumber = bookingNumber,
            GuestName = guestName,
            Mobile = mobile,
            CheckInFrom = checkInFrom,
            CheckInTo = checkInTo,
            Status = status,
            PaymentStatus = paymentStatus,
            RoomTypeId = roomTypeId
        };
        ViewBag.Filter = filter;
        return View(await _bookingService.GetAllBookingsAsync(filter));
    }

    public async Task<IActionResult> Details(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking is null) return NotFound();

        if (booking.PaymentStatus == PaymentStatus.Paid)
        {
            var (percent, amount, note) = await _bookingService.PreviewCancellationRefundAsync(id);
            ViewBag.CancellationPreview = $"If cancelled now: ₹{amount:N2} refund ({percent:0}%). {note}";
        }

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await _bookingService.ConfirmBookingAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Booking confirmed." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var result = await _bookingService.RejectBookingAsync(id, reason);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Booking rejected." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var result = await _bookingService.CancelBookingAsync(id, reason, null);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? $"Booking cancelled. {result.Message}".TrimEnd()
            : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    // Asks the gateway directly whether this booking's payment actually went through, for when
    // the client-side callback and the webhook both missed it (closed browser, webhook not
    // configured, etc.) despite the guest's money having been captured.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPayment(int id)
    {
        var result = await _bookingService.ReconcilePaymentWithGatewayAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "Gateway confirms this payment was captured — booking updated to Paid/Confirmed."
            : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int id, string reason)
    {
        var result = await _bookingService.RefundBookingAsync(id, reason, null);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Refund issued." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id)
    {
        var result = await _bookingService.CheckInAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Guest checked in." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id)
    {
        var result = await _bookingService.CheckOutAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Guest checked out." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NoShow(int id)
    {
        var result = await _bookingService.MarkNoShowAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Booking marked as no-show." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }
}
