namespace ChuliTirth.Helpers;

// Tiered refund policy, measured against the configured check-in time (not just the date) so
// "48 hours" means something precise. Keep this in sync with the "Cancellation Policy" text
// seeded in BookingRules (Data/Seed/DataSeeder.cs) — the displayed policy and the enforced one
// must match, or guests get a nasty surprise.
public static class CancellationPolicy
{
    public record Result(decimal RefundPercent, string Note);

    public static Result CalculateRefund(DateOnly checkIn, string defaultCheckInTime, DateTime cancelledAtUtc)
    {
        var checkInTime = TimeOnly.TryParse(defaultCheckInTime, out var t) ? t : new TimeOnly(12, 0);
        var checkInAtIst = checkIn.ToDateTime(checkInTime);
        var cancelledAtIst = cancelledAtUtc.ToIst();
        var hoursRemaining = (checkInAtIst - cancelledAtIst).TotalHours;

        if (hoursRemaining >= 168) // 7+ days out
            return new Result(100m, "Cancelled 7 or more days before check-in — full refund.");
        if (hoursRemaining >= 48) // 2–7 days out
            return new Result(50m, "Cancelled between 48 hours and 7 days before check-in — 50% refund.");
        return new Result(0m, "Cancelled within 48 hours of check-in (or after) — no refund.");
    }
}
