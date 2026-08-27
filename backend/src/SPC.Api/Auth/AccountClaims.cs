using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SPC.Api.Auth;

public static class AccountClaims
{
    public static Guid RequireAccountId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (value is null || !Guid.TryParse(value, out var id))
        {
            throw new InvalidOperationException("Authenticated request is missing an account id.");
        }

        return id;
    }
}
