using SPC.Core.Services;
using Xunit;

namespace SPC.Core.Tests.Services;

public class PortionFormatterTests
{
    [Theory]
    [InlineData(2, "2")]
    [InlineData(0.5, "½")]
    [InlineData(3.5, "3½")]
    [InlineData(1.25, "1¼")]
    [InlineData(0.75, "¾")]
    [InlineData(1.64, "1.64")]
    public void FormatPortions_UsesSimpleFractions(decimal portions, string expected)
    {
        Assert.Equal(expected, PortionFormatter.FormatPortions(portions));
    }
}
