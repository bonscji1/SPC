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
        app.MapPost("/api/auth/signup", SignUpAsync);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AppDbContext db,
        IPasswordHasher<AccountEntity> hasher,
        TokenService tokens,
        CancellationToken cancellationToken)
    {
        if (!AccountRules.TryNormalizeUsername(request.Username, out _, out var normalized))
        {
            return Results.Unauthorized();
        }

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

        return TokenResult(account, tokens);
    }

    private static async Task<IResult> SignUpAsync(
        LoginRequest request,
        AppDbContext db,
        IPasswordHasher<AccountEntity> hasher,
        TokenService tokens,
        CancellationToken cancellationToken)
    {
        if (!AccountRules.TryNormalizeUsername(request.Username, out var username, out var normalized)
            || !AccountRules.IsPasswordAcceptable(request.Password))
        {
            return Results.BadRequest();
        }

        var taken = await db.Accounts.AnyAsync(
            a => a.NormalizedUsername == normalized,
            cancellationToken);
        if (taken)
        {
            return Results.Conflict();
        }

        var account = new AccountEntity
        {
            Id = Guid.NewGuid(),
            Username = username,
            NormalizedUsername = normalized,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        // Unique salt is stored inside PasswordHasher's payload; login compares hashes, never the plaintext.
        account.PasswordHash = hasher.HashPassword(account, request.Password);

        db.Accounts.Add(account);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict();
        }

        return TokenResult(account, tokens);
    }

    private static IResult TokenResult(AccountEntity account, TokenService tokens) =>
        Results.Ok(new LoginResponse
        {
            AccessToken = tokens.CreateToken(account),
            Account = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
            },
        });
}
