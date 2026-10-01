namespace ChuliTirth.Models.Entities;

// Sample/demo Jain calendar data — not authoritative. See IJainCalendarService.
public class JainTithi : BaseEntity
{
    public DateOnly GregorianDate { get; set; }

    public string TithiName { get; set; } = string.Empty;
    public string? TithiNameGujarati { get; set; }
    public string? TithiNameHindi { get; set; }

    public string? Paksha { get; set; }
    public string? PakshaGujarati { get; set; }
    public string? PakshaHindi { get; set; }

    public string? Nakshatra { get; set; }

    public string? SpecialOccasion { get; set; }
    public string? SpecialOccasionGujarati { get; set; }
    public string? SpecialOccasionHindi { get; set; }
}
