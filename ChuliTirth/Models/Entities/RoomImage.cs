namespace ChuliTirth.Models.Entities;

public class RoomImage : BaseEntity
{
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
}
