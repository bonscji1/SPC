using Microsoft.EntityFrameworkCore;
using SPC.Api.Auth;
using SPC.Api.Data;
using SPC.Api.Data.Entities;
using SPC.Api.Mapping;
using SPC.Core.Models;
using SPC.Core.Services;

namespace SPC.Api.Endpoints;

public static class IngredientEndpoints
{
    public static void MapIngredientEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/search", SearchAsync);
        group.MapGet("/page", GetPageAsync);
        group.MapPut("/", SaveAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var items = await LoadAsync(db, accountId, cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> SearchAsync(
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken,
        string? query = null,
        string[]? occupied = null)
    {
        var accountId = http.User.RequireAccountId();
        var items = await LoadAsync(db, accountId, cancellationToken);
        return Results.Ok(IngredientLibrary.Search(items, query, occupiedNames: occupied));
    }

    private static async Task<IResult> GetPageAsync(
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken,
        int? page = null,
        int? pageSize = null,
        string? nameQuery = null)
    {
        var accountId = http.User.RequireAccountId();
        var items = await LoadAsync(db, accountId, cancellationToken);
        return Results.Ok(IngredientList.Page(items, page ?? 1, pageSize ?? 10, nameQuery));
    }

    private static async Task<IResult> SaveAsync(
        IngredientDto ingredient,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        var accountId = http.User.RequireAccountId();
        var normalized = IngredientLibrary.NormalizeName(ingredient.Name);
        if (normalized.Length == 0)
        {
            return Results.BadRequest("Ingredient name is required.");
        }

        var entity = await db.Ingredients
            .SingleOrDefaultAsync(i => i.AccountId == accountId && i.Id == ingredient.Id, cancellationToken);

        if (entity is null)
        {
            entity = await db.Ingredients.SingleOrDefaultAsync(
                i => i.AccountId == accountId && i.NormalizedName == normalized,
                cancellationToken);
        }
        else
        {
            var clash = await db.Ingredients.AnyAsync(
                i => i.AccountId == accountId
                    && i.Id != entity.Id
                    && i.NormalizedName == normalized,
                cancellationToken);
            if (clash)
            {
                return Results.BadRequest("An ingredient with that name already exists.");
            }
        }

        if (entity is null)
        {
            entity = new IngredientEntity { Id = ingredient.Id == Guid.Empty ? Guid.NewGuid() : ingredient.Id };
            db.Ingredients.Add(entity);
        }

        var id = entity.Id;
        entity.Apply(ingredient, accountId);
        entity.Id = id;

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(entity.ToDto());
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var entity = await db.Ingredients
            .SingleOrDefaultAsync(i => i.AccountId == accountId && i.Id == id, cancellationToken);
        if (entity is not null)
        {
            db.Ingredients.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }

    private static async Task<List<IngredientDto>> LoadAsync(
        AppDbContext db,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var entities = await db.Ingredients.AsNoTracking()
            .Where(i => i.AccountId == accountId)
            .ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDto()).ToList();
    }
}
