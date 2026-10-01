namespace ChuliTirth.Models.Entities;

public class SiteSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = "General";
    public DateTime? UpdatedAtUtc { get; set; }
}
