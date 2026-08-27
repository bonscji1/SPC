using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SPC.Api.Data.Entities;
using SPC.Core.Auth;

namespace SPC.Api.Data;

public static class SeedData
{
    public static async Task EnsureDefaultAccountAsync(
        AppDbContext db,
        IPasswordHasher<AccountEntity> hasher,
        CancellationToken cancellationToken = default)
    {
        var normalized = DefaultAccount.Username.ToLowerInvariant();
        var exists = await db.Accounts.AnyAsync(
            a => a.NormalizedUsername == normalized,
            cancellationToken);
        if (exists)
        {
            return;
        }

        var account = new AccountEntity
        {
            Id = Guid.NewGuid(),
            Username = DefaultAccount.Username,
            NormalizedUsername = normalized,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        account.PasswordHash = hasher.HashPassword(account, DefaultAccount.Password);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
    }
}
