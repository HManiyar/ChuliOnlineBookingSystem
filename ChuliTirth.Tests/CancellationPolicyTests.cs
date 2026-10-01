using ChuliTirth.Helpers;
using Xunit;

namespace ChuliTirth.Tests;

public class CancellationPolicyTests
{
    private const string CheckInTime = "12:00";

    [Fact]
    public void FullRefund_WhenCancelledSevenOrMoreDaysOut()
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var cancelledAtUtc = DateTime.UtcNow;

        var result = CancellationPolicy.CalculateRefund(checkIn, CheckInTime, cancelledAtUtc);

        Assert.Equal(100m, result.RefundPercent);
    }

    [Fact]
    public void HalfRefund_WhenCancelledBetween48HoursAndSevenDaysOut()
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4));
        var cancelledAtUtc = DateTime.UtcNow;

        var result = CancellationPolicy.CalculateRefund(checkIn, CheckInTime, cancelledAtUtc);

        Assert.Equal(50m, result.RefundPercent);
    }

    [Fact]
    public void NoRefund_WhenCancelledWithin48Hours()
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(20));
        var cancelledAtUtc = DateTime.UtcNow;

        var result = CancellationPolicy.CalculateRefund(checkIn, CheckInTime, cancelledAtUtc);

        Assert.Equal(0m, result.RefundPercent);
    }

    [Fact]
    public void NoRefund_WhenCancelledAfterCheckIn()
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var cancelledAtUtc = DateTime.UtcNow;

        var result = CancellationPolicy.CalculateRefund(checkIn, CheckInTime, cancelledAtUtc);

        Assert.Equal(0m, result.RefundPercent);
    }
}
