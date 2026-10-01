namespace ChuliTirth.Helpers;

public static class BookingNumberGenerator
{
    // CT-{year}-{6 digit sequence derived from the new row's identity}
    public static string Generate(int sequentialId, DateTime nowUtc) =>
        $"CT-{nowUtc.Year}-{sequentialId:D6}";
}
