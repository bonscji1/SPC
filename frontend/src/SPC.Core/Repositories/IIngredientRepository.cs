using SPC.Core.Models;

namespace SPC.Core.Repositories;

/// <summary>
/// Nutrition library (canonical name + kcal/100 g). Shared by ingredient and
/// spice rows. Distinct from recipe lines (<see cref="RecipeIngredientDto"/>).
/// </summary>
public interface IIngredientRepository
{
    Task<IReadOnlyList<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngredientDto>> SearchAsync(
        string query,
        IReadOnlyList<string>? occupiedNames = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Name order. Page size is 10, 25, or 50; other values fall back to 10.
    /// Pass <paramref name="nameQuery"/> for a case-insensitive contains match on name.
    /// </summary>
    Task<PagedResult<IngredientDto>> GetPageAsync(
        int page,
        int pageSize,
        string? nameQuery = null,
        CancellationToken cancellationToken = default);

    Task SaveAsync(IngredientDto ingredient, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
