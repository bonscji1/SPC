using SPC.Core.Models;

namespace SPC.Core.Repositories;

public interface IRecipeRepository
{
    Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Families grouped by <see cref="RecipeDto.FamilyId"/> (missing family id treated as the recipe id).
    /// Newest family activity first, then primary name. Page size is 10, 25, or 50.
    /// Name query matches recipe name or variant label. Meal type matches if any member has that type.
    /// </summary>
    Task<PagedResult<RecipeFamilyGroup>> GetPageAsync(
        int page,
        int pageSize,
        MealType? mealType = null,
        string? nameQuery = null,
        CancellationToken cancellationToken = default);

    Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>All rows that share a family, default variant first.</summary>
    Task<IReadOnlyList<RecipeDto>> GetByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);

    Task<RecipeSaveResult> SaveAsync(RecipeDto recipe, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Deletes every row in the family (the recipe on Home).</summary>
    Task DeleteFamilyAsync(Guid familyId, CancellationToken cancellationToken = default);
}
