namespace ChuliTirth.Helpers;

// DB stores UTC; UI always displays India Standard Time regardless of host OS.
public static class IndianTimeHelper
{
    private static readonly TimeZoneInfo IstZone = ResolveIst();

    private static TimeZoneInfo ResolveIst()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); }
        catch (TimeZoneNotFoundException)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"); }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromHours(5.5), "India Standard Time", "IST");
            }
        }
    }

    public static DateTime ToIst(this DateTime utc)
    {
        var u = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(u, IstZone);
    }

    public static DateTime NowIst() => ToIst(DateTime.UtcNow);
}
