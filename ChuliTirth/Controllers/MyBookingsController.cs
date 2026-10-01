using System.Security.Claims;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

[Authorize]
public class MyBookingsController : Controller
{
    private readonly IBookingService _bookingService;

    public MyBookingsController(IBookingService bookingService) => _bookingService = bookingService;

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bookings = await _bookingService.GetBookingsForUserAsync(userId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        var previews = new Dictionary<int, string>();
        foreach (var b in bookings.Where(b => b.Status is BookingStatus.Pending or BookingStatus.PaymentPending or BookingStatus.Confirmed && b.CheckIn > today))
        {
            var (percent, amount, note) = await _bookingService.PreviewCancellationRefundAsync(b.Id);
            previews[b.Id] = b.PaymentStatus == PaymentStatus.Paid
                ? $"If cancelled now: ₹{amount:N0} refund ({percent:0}%). {note}"
                : "Not yet paid — cancelling now has nothing to refund.";
        }
        ViewBag.CancellationPreviews = previews;

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking is null || booking.UserId != userId) return Forbid();

        var result = await _bookingService.CancelBookingAsync(id, reason ?? "Cancelled by guest", userId);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? $"Your booking has been cancelled. {result.Message}".TrimEnd()
            : result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
