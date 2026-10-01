namespace ChuliTirth.Models.Entities;

public class BookingRule : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? TitleGujarati { get; set; }
    public string? TitleHindi { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? DescriptionGujarati { get; set; }
    public string? DescriptionHindi { get; set; }

    public string Category { get; set; } = "General";
    public int DisplayOrder { get; set; }
}
