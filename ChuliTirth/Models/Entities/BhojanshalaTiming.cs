namespace ChuliTirth.Models.Entities;

public class BhojanshalaTiming : BaseEntity
{
    public string MealName { get; set; } = string.Empty;
    public string? MealNameGujarati { get; set; }
    public string? MealNameHindi { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Note { get; set; }
    public int DisplayOrder { get; set; }
}
