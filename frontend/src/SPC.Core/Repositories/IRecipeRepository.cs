using SPC.Core.Models;

namespace SPC.Core.Repositories;

public interface IRecipeRepository
{
    Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Newest <see cref="RecipeDto.UpdatedAt"/> first, then name.
    /// Page size is 10, 25, or 50; other values fall back to 10. Out-of-range pages clamp.
    /// Pass <paramref name="mealType"/> to keep one recipe type only.
    /// Pass <paramref name="nameQuery"/> for a case-insensitive name contains filter.
    /// </summary>
    Task<PagedResult<RecipeDto>> GetPageAsync(
        int page,
        int pageSize,
        MealType? mealType = null,
        string? nameQuery = null,
        CancellationToken cancellationToken = default);

    Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RecipeSaveResult> SaveAsync(RecipeDto recipe, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
