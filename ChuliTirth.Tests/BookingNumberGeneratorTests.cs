using ChuliTirth.Helpers;
using Xunit;

namespace ChuliTirth.Tests;

public class BookingNumberGeneratorTests
{
    [Fact]
    public void Generate_ProducesExpectedFormat()
    {
        var result = BookingNumberGenerator.Generate(123, new DateTime(2026, 1, 1));
        Assert.Equal("CT-2026-000123", result);
    }

    [Fact]
    public void Generate_PadsSequenceToSixDigits()
    {
        var result = BookingNumberGenerator.Generate(7, new DateTime(2026, 6, 1));
        Assert.Equal("CT-2026-000007", result);
    }
}
