using ChuliTirth.Models.Entities;

namespace ChuliTirth.Models.ViewModels;

public class HomeViewModel
{
    public List<RoomType> RoomTypes { get; set; } = new();
    public List<Facility> Facilities { get; set; } = new();
    public List<Announcement> Announcements { get; set; } = new();
    public List<GalleryImage> GalleryPreview { get; set; } = new();
    public JainTithi? TodayTithi { get; set; }
    public Dictionary<string, string?> Settings { get; set; } = new();
    public BookingSearchViewModel Search { get; set; } = new();
}

public class BookingSearchViewModel
{
    public DateOnly? CheckIn { get; set; }
    public DateOnly? CheckOut { get; set; }
    public int Guests { get; set; } = 2;
    public int Rooms { get; set; } = 1;
    public int? RoomTypeId { get; set; }
}
