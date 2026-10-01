using System.ComponentModel.DataAnnotations;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;

namespace ChuliTirth.Models.ViewModels;

public class AvailabilityResultsViewModel
{
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Guests { get; set; }
    public int Rooms { get; set; }
    public List<RoomTypeAvailability> Results { get; set; } = new();
}

public class GuestDetailsViewModel
{
    public int RoomTypeId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int RoomsNeeded { get; set; } = 1;

    [Required, Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, Phone, Display(Name = "Mobile Number")]
    public string Mobile { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; } = "India";

    [Range(1, 30, ErrorMessage = "Please enter a valid number of adults.")]
    public int Adults { get; set; } = 1;

    [Range(0, 20)]
    public int Children { get; set; }

    public string? SpecialRequirements { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }

    public RoomType? RoomType { get; set; }
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;
    public decimal EstimatedTotal => (RoomType?.Price ?? 0) * Nights * RoomsNeeded;

    // Validated manually in BookingController.Confirm (not a data annotation) — the checkbox
    // only exists on the Summary page, so it must not block ModelState on the earlier
    // GuestDetails step, which posts the same view model without that field.
    public bool AcceptedRules { get; set; }
}

public class BookingConfirmationViewModel
{
    public Booking Booking { get; set; } = null!;
    public bool IsSimulatedPayment { get; set; }
    public string? PaymentMessage { get; set; }
}

public class PayViewModel
{
    public Booking Booking { get; set; } = null!;
    public string? KeyId { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public long AmountInPaise { get; set; }
}
