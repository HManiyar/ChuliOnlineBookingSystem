namespace ChuliTirth.Models.Entities;

public class Booking : BaseEntity
{
    public string BookingNumber { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Nights { get; set; }

    public int Adults { get; set; } = 1;
    public int Children { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }

    public decimal RoomAmount { get; set; }
    public decimal AdditionalCharges { get; set; }
    public decimal TotalAmount { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CheckedInAtUtc { get; set; }
    public DateTime? CheckedOutAtUtc { get; set; }

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
