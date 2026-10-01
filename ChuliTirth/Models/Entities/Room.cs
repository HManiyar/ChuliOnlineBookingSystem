namespace ChuliTirth.Models.Entities;

public class Room : BaseEntity
{
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public string RoomNumber { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
