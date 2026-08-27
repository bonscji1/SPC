using SPC.Core.Models;

namespace SPC.Core.Auth;

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    public required AccountDto Account { get; init; }
}
