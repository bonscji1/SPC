using SPC.Core.Formatting;
using Xunit;

namespace SPC.Core.Tests.Formatting;

public class NumberFormatTests
{
    public static TheoryData<decimal, string> Cases => new()
    {
        { 1m, "1" },
        { 100m, "100" },
        { 543m, "543" },
        { 0m, "0" },
        { 4.32m, "4.32" },
        { 4.3m, "4.30" },
        { 0.5m, "0.50" },
        { 4.325m, "4.33" },
        { 1.001m, "1" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Format_UsesTwoDecimalsOnlyWhenNeeded(decimal value, string expected)
    {
        Assert.Equal(expected, NumberFormat.Format(value));
    }

    [Fact]
    public void WithUnit_AppendsGrams()
    {
        Assert.Equal("4.30 g", NumberFormat.WithUnit(4.3m, "g"));
        Assert.Equal("200 g", NumberFormat.WithUnit(200m, "g"));
    }
}
