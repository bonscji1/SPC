using Microsoft.EntityFrameworkCore;
using SPC.Api.Auth;
using SPC.Api.Data;
using SPC.Api.Data.Entities;
using SPC.Api.Mapping;
using SPC.Core.Models;

namespace SPC.Api.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPut("/", SaveAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var entities = await db.Profiles.AsNoTracking()
            .Where(p => p.AccountId == accountId)
            .ToListAsync(cancellationToken);
        return Results.Ok(entities.Select(e => e.ToDto()).ToList());
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var entity = await db.Profiles.AsNoTracking()
            .SingleOrDefaultAsync(p => p.AccountId == accountId && p.Id == id, cancellationToken);
        return entity is null ? Results.NotFound() : Results.Ok(entity.ToDto());
    }

    private static async Task<IResult> SaveAsync(
        UserProfileDto profile,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var accountId = http.User.RequireAccountId();
        var entity = await db.Profiles
            .SingleOrDefaultAsync(p => p.AccountId == accountId && p.Id == profile.Id, cancellationToken);
        if (entity is null)
        {
            entity = new ProfileEntity();
            db.Profiles.Add(entity);
        }

        entity.Apply(profile, accountId);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var entity = await db.Profiles
            .SingleOrDefaultAsync(p => p.AccountId == accountId && p.Id == id, cancellationToken);
        if (entity is not null)
        {
            db.Profiles.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }
}
