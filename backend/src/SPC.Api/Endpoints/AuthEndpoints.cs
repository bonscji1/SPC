using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SPC.Api.Auth;
using SPC.Api.Data;
using SPC.Api.Data.Entities;
using SPC.Core.Auth;
using SPC.Core.Models;

namespace SPC.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", LoginAsync);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AppDbContext db,
        IPasswordHasher<AccountEntity> hasher,
        TokenService tokens,
        CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        var normalized = username.ToLowerInvariant();
        var account = await db.Accounts.SingleOrDefaultAsync(
            a => a.NormalizedUsername == normalized,
            cancellationToken);

        if (account is null)
        {
            return Results.Unauthorized();
        }

        var result = hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password ?? string.Empty);
        if (result == PasswordVerificationResult.Failed)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new LoginResponse
        {
            AccessToken = tokens.CreateToken(account),
            Account = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
            },
        });
    }
}
