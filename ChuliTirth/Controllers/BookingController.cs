using ChuliTirth.Helpers;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Config;
using ChuliTirth.Models.DTOs;
using ChuliTirth.Models.Entities;
using ChuliTirth.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace ChuliTirth.Controllers;

public class BookingController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IRoomAvailabilityService _availabilityService;
    private readonly IBookingService _bookingService;
    private readonly IPaymentService _paymentService;
    private readonly RazorpaySettings _razorpaySettings;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<BookingController> _logger;

    public BookingController(IRoomService roomService, IRoomAvailabilityService availabilityService,
        IBookingService bookingService, IPaymentService paymentService, IOptions<RazorpaySettings> razorpaySettings,
        IOptions<ApplicationSettings> appSettings, ILogger<BookingController> logger)
    {
        _roomService = roomService;
        _availabilityService = availabilityService;
        _bookingService = bookingService;
        _paymentService = paymentService;
        _razorpaySettings = razorpaySettings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    // "Too far in the future" cutoff, inclusive — a guest can book exactly MaxAdvanceBookingMonths
    // out, not one day more.
    private DateOnly MaxAdvanceCheckInDate =>
        DateOnly.FromDateTime(IndianTimeHelper.NowIst()).AddMonths(_appSettings.MaxAdvanceBookingMonths);

    [HttpGet]
    public async Task<IActionResult> Availability(DateOnly checkIn, DateOnly checkOut, int guests = 1, int rooms = 1, int? roomTypeId = null)
    {
        var vm = new AvailabilityResultsViewModel { CheckIn = checkIn, CheckOut = checkOut, Guests = guests, Rooms = rooms };

        if (checkIn == default || checkOut == default)
        {
            ModelState.AddModelError(string.Empty, "Please select check-in and check-out dates.");
            return View(vm);
        }
        if (checkIn >= checkOut)
        {
            ModelState.AddModelError(string.Empty, "Check-out date must be after check-in date.");
            return View(vm);
        }
        if (checkIn < DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5).Date))
        {
            ModelState.AddModelError(string.Empty, "Check-in date cannot be in the past.");
            return View(vm);
        }
        if (checkOut.DayNumber - checkIn.DayNumber > _appSettings.MaxBookingNights)
        {
            ModelState.AddModelError(string.Empty, $"Bookings are limited to a maximum of {_appSettings.MaxBookingNights} nights.");
            return View(vm);
        }
        if (checkIn > MaxAdvanceCheckInDate)
        {
            ModelState.AddModelError(string.Empty, $"Bookings can only be made up to {_appSettings.MaxAdvanceBookingMonths} months in advance.");
            return View(vm);
        }

        vm.Results = await _availabilityService.SearchAvailabilityAsync(checkIn, checkOut, guests, rooms, roomTypeId);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GuestDetails(int roomTypeId, DateOnly checkIn, DateOnly checkOut, int roomsNeeded = 1, int adults = 1, int children = 0)
    {
        var roomType = await _roomService.GetRoomTypeByIdAsync(roomTypeId);
        if (roomType is null) return NotFound();

        if (checkOut.DayNumber - checkIn.DayNumber > _appSettings.MaxBookingNights || checkIn > MaxAdvanceCheckInDate)
        {
            TempData["BookingError"] = $"Bookings are limited to a maximum of {_appSettings.MaxBookingNights} nights and must be within {_appSettings.MaxAdvanceBookingMonths} months.";
            return RedirectToAction(nameof(Availability), new { checkIn, checkOut, guests = adults + children, rooms = roomsNeeded });
        }
        if (adults < roomsNeeded)
        {
            TempData["BookingError"] = "Number of adults must be at least the number of rooms booked — each room needs at least one adult.";
            return RedirectToAction(nameof(Availability), new { checkIn, checkOut, guests = adults + children, rooms = roomsNeeded });
        }

        var available = await _availabilityService.GetAvailableRoomCountAsync(roomTypeId, checkIn, checkOut);
        if (available < roomsNeeded)
        {
            TempData["BookingError"] = "Sorry, that room is no longer available for the selected dates.";
            return RedirectToAction(nameof(Availability), new { checkIn, checkOut, guests = adults + children, rooms = roomsNeeded });
        }

        var vm = new GuestDetailsViewModel
        {
            RoomTypeId = roomTypeId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            RoomsNeeded = roomsNeeded,
            Adults = adults,
            Children = children,
            RoomType = roomType,
            Country = "India"
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuestDetails(GuestDetailsViewModel model)
    {
        model.RoomType = await _roomService.GetRoomTypeByIdAsync(model.RoomTypeId);
        if (model.RoomType is null) return NotFound();

        if (model.Nights > _appSettings.MaxBookingNights)
        {
            ModelState.AddModelError(string.Empty, $"Bookings are limited to a maximum of {_appSettings.MaxBookingNights} nights.");
        }
        if (model.CheckIn > MaxAdvanceCheckInDate)
        {
            ModelState.AddModelError(string.Empty, $"Bookings can only be made up to {_appSettings.MaxAdvanceBookingMonths} months in advance.");
        }
        if (model.Adults < model.RoomsNeeded)
        {
            ModelState.AddModelError(nameof(model.Adults), $"Number of adults must be at least the number of rooms booked ({model.RoomsNeeded}).");
        }

        if (!ModelState.IsValid) return View(model);

        var available = await _availabilityService.GetAvailableRoomCountAsync(model.RoomTypeId, model.CheckIn, model.CheckOut);
        if (available < model.RoomsNeeded)
        {
            ModelState.AddModelError(string.Empty, "Sorry, that room is no longer available for the selected dates. Please go back and search again.");
            return View(model);
        }

        return View("Summary", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(GuestDetailsViewModel model)
    {
        model.RoomType = await _roomService.GetRoomTypeByIdAsync(model.RoomTypeId);
        if (model.RoomType is null) return NotFound();

        if (!model.AcceptedRules)
        {
            ModelState.AddModelError(nameof(model.AcceptedRules), "You must agree to the Dharamshala rules and booking policies.");
        }

        if (!ModelState.IsValid) return View("Summary", model);

        int? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var parsedId)) userId = parsedId;

        var request = new CreateBookingRequest
        {
            RoomTypeId = model.RoomTypeId,
            CheckIn = model.CheckIn,
            CheckOut = model.CheckOut,
            RoomsNeeded = model.RoomsNeeded,
            Adults = model.Adults,
            Children = model.Children,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Mobile = model.Mobile,
            Email = model.Email,
            Address = model.Address,
            City = model.City,
            State = model.State,
            Country = model.Country,
            SpecialRequirements = model.SpecialRequirements,
            IdType = model.IdType,
            IdNumber = model.IdNumber,
            UserId = userId,
            AcceptedRules = model.AcceptedRules
        };

        var result = await _bookingService.CreateBookingAsync(request);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Booking could not be completed.");
            return View("Summary", model);
        }

        var booking = result.Booking!;
        var order = await _paymentService.CreateOrderAsync(booking, booking.TotalAmount);
        if (!order.Success)
        {
            ModelState.AddModelError(string.Empty, order.Message ?? "Could not start payment for this booking.");
            return View("Summary", model);
        }

        await _bookingService.RecordPaymentOrderAsync(booking.Id, order, order.RequiresCheckout ? "Razorpay" : "Mock");

        if (!order.RequiresCheckout)
        {
            // Mock gateway / payment bypass — settle immediately, same as the old synchronous flow.
            await _bookingService.ApplyPaymentResultAsync(order.GatewayOrderId!, null, success: true);
            _logger.LogInformation("Booking {BookingNumber} created and payment simulated.", booking.BookingNumber);
            return RedirectToAction(nameof(Confirmation), new { bookingNumber = booking.BookingNumber });
        }

        // Real gateway — send the guest through the interactive checkout.
        return RedirectToAction(nameof(Pay), new { bookingNumber = booking.BookingNumber });
    }

    [HttpGet]
    public async Task<IActionResult> Pay(string bookingNumber)
    {
        var booking = await _bookingService.GetByBookingNumberAsync(bookingNumber);
        if (booking is null) return NotFound();

        if (booking.PaymentStatus == PaymentStatus.Paid)
            return RedirectToAction(nameof(Confirmation), new { bookingNumber });

        // Most recent order for this booking — reused on retry even if an earlier attempt on it
        // failed (Razorpay allows multiple payment attempts against the same unpaid order).
        var payment = booking.Payments.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Status != PaymentStatus.Paid);
        if (payment is null || string.IsNullOrWhiteSpace(payment.GatewayOrderId))
        {
            TempData["BookingError"] = "This booking has no payment in progress. Please start a new booking.";
            return RedirectToAction(nameof(Availability));
        }

        var vm = new PayViewModel
        {
            Booking = booking,
            KeyId = payment.Method == "Razorpay" ? _razorpaySettings.KeyId : null,
            GatewayOrderId = payment.GatewayOrderId,
            AmountInPaise = (long)(payment.Amount * 100)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PaymentCallback(string razorpay_order_id, string razorpay_payment_id, string razorpay_signature)
    {
        var verification = await _paymentService.VerifyPaymentAsync(new PaymentVerificationRequest
        {
            GatewayOrderId = razorpay_order_id,
            GatewayPaymentId = razorpay_payment_id,
            Signature = razorpay_signature
        });

        var result = await _bookingService.ApplyPaymentResultAsync(razorpay_order_id, razorpay_payment_id, verification.Success, verification.Message);
        if (!result.Success || result.Booking is null)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Payment could not be verified." });
        }

        return Json(new { success = verification.Success, redirectUrl = Url.Action(nameof(Confirmation), new { bookingNumber = result.Booking.BookingNumber }) });
    }

    // Razorpay calls this server-to-server — it's the authoritative confirmation, independent of
    // whether the guest's browser stayed open for the client-side callback above. No antiforgery
    // token (external caller) and the body must be read raw since the signature covers the exact bytes.
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync();
        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

        var result = await _paymentService.ProcessWebhookAsync(rawBody, signature);
        if (!result.SignatureValid)
        {
            _logger.LogWarning("Razorpay webhook rejected: invalid signature.");
            return Unauthorized();
        }

        if (result.Handled && !string.IsNullOrWhiteSpace(result.GatewayOrderId))
        {
            await _bookingService.ApplyPaymentResultAsync(result.GatewayOrderId, result.GatewayPaymentId, result.PaymentSucceeded);
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string bookingNumber)
    {
        var booking = await _bookingService.GetByBookingNumberAsync(bookingNumber);
        if (booking is null) return NotFound();

        // Guards against landing here (e.g. browser back/forward) before a real gateway payment
        // has actually settled — send the guest back to finish paying instead of showing "Confirmed".
        if (booking.PaymentStatus != PaymentStatus.Paid)
            return RedirectToAction(nameof(Pay), new { bookingNumber });

        var payment = booking.Payments.OrderByDescending(p => p.Id).FirstOrDefault();
        var vm = new BookingConfirmationViewModel
        {
            Booking = booking,
            IsSimulatedPayment = payment?.IsSimulated ?? true,
            PaymentMessage = payment?.IsSimulated == true
                ? "Payment simulated (development mode) — no real charge was made."
                : null
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Track(string? bookingNumber)
    {
        if (string.IsNullOrWhiteSpace(bookingNumber)) return View((Booking?)null);
        var booking = await _bookingService.GetByBookingNumberAsync(bookingNumber);
        return View(booking);
    }
}
