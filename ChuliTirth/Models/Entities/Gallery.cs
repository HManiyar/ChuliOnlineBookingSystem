namespace ChuliTirth.Models.Entities;

public class GalleryCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameGujarati { get; set; }
    public string? NameHindi { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<GalleryImage> Images { get; set; } = new List<GalleryImage>();
}

public class GalleryImage : BaseEntity
{
    public int GalleryCategoryId { get; set; }
    public GalleryCategory GalleryCategory { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}
