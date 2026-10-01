namespace ChuliTirth.Models.Entities;

public class Facility : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameGujarati { get; set; }
    public string? NameHindi { get; set; }

    public string? Description { get; set; }
    public string? DescriptionGujarati { get; set; }
    public string? DescriptionHindi { get; set; }

    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
}
