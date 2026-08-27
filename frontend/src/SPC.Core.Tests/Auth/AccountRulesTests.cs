using SPC.Core.Auth;
using Xunit;

namespace SPC.Core.Tests.Auth;

public sealed class AccountRulesTests
{
    [Theory]
    [InlineData("spc", "spc", "spc")]
    [InlineData("  Alice  ", "Alice", "alice")]
    public void TryNormalizeUsername_trims_and_lowercases(string input, string display, string normalized)
    {
        Assert.True(AccountRules.TryNormalizeUsername(input, out var actualDisplay, out var actualNormalized));
        Assert.Equal(display, actualDisplay);
        Assert.Equal(normalized, actualNormalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryNormalizeUsername_rejects_blank(string? input)
    {
        Assert.False(AccountRules.TryNormalizeUsername(input, out _, out _));
    }

    [Fact]
    public void TryNormalizeUsername_rejects_too_long()
    {
        var tooLong = new string('a', AccountRules.MaxUsernameLength + 1);
        Assert.False(AccountRules.TryNormalizeUsername(tooLong, out _, out _));
    }

    [Fact]
    public void IsPasswordAcceptable_rejects_empty()
    {
        Assert.False(AccountRules.IsPasswordAcceptable(null));
        Assert.False(AccountRules.IsPasswordAcceptable(""));
        Assert.True(AccountRules.IsPasswordAcceptable("x"));
    }
}
