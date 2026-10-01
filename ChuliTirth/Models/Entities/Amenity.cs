namespace ChuliTirth.Models.Entities;

public class Amenity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameGujarati { get; set; }
    public string? NameHindi { get; set; }
    public string? Icon { get; set; }

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}

public class RoomAmenity
{
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;
}
