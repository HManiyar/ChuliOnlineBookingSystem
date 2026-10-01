namespace ChuliTirth.Models.Entities;

// One row per physical room allocated to a booking (supports multi-room bookings).
public class BookingRoom
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public decimal RatePerNight { get; set; }
}
