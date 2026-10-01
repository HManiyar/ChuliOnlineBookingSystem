namespace ChuliTirth.Models.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? TitleGujarati { get; set; }
    public string? TitleHindi { get; set; }

    public string Body { get; set; } = string.Empty;
    public string? BodyGujarati { get; set; }
    public string? BodyHindi { get; set; }

    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
    public DateTime? StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
}
