using Microsoft.EntityFrameworkCore;
using SPC.Api.Auth;
using SPC.Api.Data;
using SPC.Api.Data.Entities;
using SPC.Api.Mapping;
using SPC.Core.Models;
using SPC.Core.Services;

namespace SPC.Api.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapGet("/families/{familyId:guid}", GetFamilyAsync);
        group.MapPut("/", SaveAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapDelete("/families/{familyId:guid}", DeleteFamilyAsync);
    }

    private static async Task<IResult> ListAsync(
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken,
        int? page = null,
        int? pageSize = null,
        MealType? mealType = null,
        string? nameQuery = null)
    {
        var accountId = http.User.RequireAccountId();
        var recipes = await LoadAccountRecipesAsync(db, accountId, cancellationToken);
        return Results.Ok(RecipeList.Page(recipes, page ?? 1, pageSize ?? 10, mealType, nameQuery));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var entity = await db.Recipes.AsNoTracking()
            .SingleOrDefaultAsync(r => r.AccountId == accountId && r.Id == id, cancellationToken);
        return entity is null ? Results.NotFound() : Results.Ok(entity.ToDto());
    }

    private static async Task<IResult> GetFamilyAsync(
        Guid familyId,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var recipes = await LoadAccountRecipesAsync(db, accountId, cancellationToken);
        var members = recipes.Where(r => RecipeScaler.FamilyKey(r) == familyId).ToList();
        if (members.Count == 0)
        {
            return Results.Ok(Array.Empty<RecipeDto>());
        }

        return Results.Ok(RecipeList.BuildFamily(members).AllMembers);
    }

    private static async Task<IResult> SaveAsync(
        RecipeDto recipe,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        var accountId = http.User.RequireAccountId();

        var entity = await db.Recipes
            .SingleOrDefaultAsync(r => r.AccountId == accountId && r.Id == recipe.Id, cancellationToken);
        if (entity is null)
        {
            entity = new RecipeEntity();
            db.Recipes.Add(entity);
        }

        entity.Apply(recipe, accountId);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(RecipeSaveResult.Succeeded("Recipe saved successfully."));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var entity = await db.Recipes
            .SingleOrDefaultAsync(r => r.AccountId == accountId && r.Id == id, cancellationToken);
        if (entity is not null)
        {
            db.Recipes.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteFamilyAsync(
        Guid familyId,
        AppDbContext db,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var accountId = http.User.RequireAccountId();
        var members = await db.Recipes
            .Where(r => r.AccountId == accountId && r.FamilyId == familyId)
            .ToListAsync(cancellationToken);
        if (members.Count == 0)
        {
            var all = await LoadAccountRecipesAsync(db, accountId, cancellationToken);
            var trackedIds = all
                .Where(r => RecipeScaler.FamilyKey(r) == familyId)
                .Select(r => r.Id)
                .ToList();
            members = await db.Recipes
                .Where(r => r.AccountId == accountId && trackedIds.Contains(r.Id))
                .ToListAsync(cancellationToken);
        }

        db.Recipes.RemoveRange(members);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<List<RecipeDto>> LoadAccountRecipesAsync(
        AppDbContext db,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var entities = await db.Recipes.AsNoTracking()
            .Where(r => r.AccountId == accountId)
            .ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDto()).ToList();
    }
}
