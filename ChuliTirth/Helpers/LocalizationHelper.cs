using System.Globalization;

namespace ChuliTirth.Helpers;

// Picks the right DB-driven translation (English/Gujarati/Hindi) for the current request culture.
public static class LocalizationHelper
{
    public static string Pick(CultureInfo culture, string english, string? gujarati, string? hindi)
    {
        var lang = culture.TwoLetterISOLanguageName;
        return lang switch
        {
            "gu" => string.IsNullOrWhiteSpace(gujarati) ? english : gujarati,
            "hi" => string.IsNullOrWhiteSpace(hindi) ? english : hindi,
            _ => english
        };
    }
}
