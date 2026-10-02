namespace ChuliTirth.Models.Entities;

// A physical room. Deliberately NOT tied to a RoomType — every room is the same physical space
// and can be booked under either rate tier (AC/Non-AC); which tier a given stay used is recorded
// per-booking on BookingRoom.RoomTypeId, not here. See RoomAvailabilityService for how this keeps
// the same physical room from being double-booked once as AC and once as Non-AC.
public class Room : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
