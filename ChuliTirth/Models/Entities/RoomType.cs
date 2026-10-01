namespace ChuliTirth.Models.Entities;

public class RoomType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameGujarati { get; set; }
    public string? NameHindi { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? DescriptionGujarati { get; set; }
    public string? DescriptionHindi { get; set; }

    public int Capacity { get; set; }
    public int BedCount { get; set; }
    public decimal Price { get; set; }

    public bool IsAC { get; set; }
    public bool HasAttachedBathroom { get; set; } = true;
    public bool HasHotWater { get; set; } = true;
    public bool HasWifi { get; set; }
    public bool HasParking { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
