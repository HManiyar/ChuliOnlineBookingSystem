using System.Globalization;
using ChuliTirth.Helpers;
using Xunit;

namespace ChuliTirth.Tests;

public class LocalizationHelperTests
{
    [Theory]
    [InlineData("en-US", "Hello")]
    [InlineData("gu-IN", "Namaste")]
    [InlineData("hi-IN", "Namaskar")]
    public void Pick_ReturnsTranslationForCulture(string cultureName, string expected)
    {
        var culture = new CultureInfo(cultureName);
        var result = LocalizationHelper.Pick(culture, "Hello", "Namaste", "Namaskar");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Pick_FallsBackToEnglish_WhenTranslationMissing()
    {
        var culture = new CultureInfo("gu-IN");
        var result = LocalizationHelper.Pick(culture, "Hello", null, "Namaskar");
        Assert.Equal("Hello", result);
    }
}
