namespace ChuliTirth.Models.Entities;

public class JainQuote : BaseEntity
{
    public string TextEnglish { get; set; } = string.Empty;
    public string? TextGujarati { get; set; }
    public string? TextHindi { get; set; }

    // Only populate when attribution is verifiable.
    public string? Attribution { get; set; }
    public int DisplayOrder { get; set; }
}
