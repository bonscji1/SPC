using SPC.Core.Auth;
using SPC.Core.Models;
using Xunit;

namespace SPC.Core.Tests.Auth;

public sealed class AuthSessionTests
{
    [Fact]
    public void Set_marks_session_authenticated()
    {
        var session = new AuthSession();
        var account = new AccountDto { Id = Guid.NewGuid(), Username = "spc" };

        session.Set("token", account);

        Assert.True(session.IsAuthenticated);
        Assert.Equal("token", session.AccessToken);
        Assert.Equal("spc", session.Account?.Username);
    }

    [Fact]
    public void Clear_drops_token_and_account()
    {
        var session = new AuthSession();
        session.Set("token", new AccountDto { Id = Guid.NewGuid(), Username = "spc" });

        session.Clear();

        Assert.False(session.IsAuthenticated);
        Assert.Null(session.AccessToken);
        Assert.Null(session.Account);
    }
}
