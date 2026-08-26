namespace SPC.Core.Models;

public static class RecipeList
{
    public static PagedResult<RecipeDto> Page(
        IReadOnlyList<RecipeDto> recipes,
        int page,
        int pageSize,
        MealType? mealType = null,
        string? nameQuery = null)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        IEnumerable<RecipeDto> filtered = mealType is MealType type
            ? recipes.Where(r => r.MealType == type)
            : recipes;

        var needle = nameQuery?.Trim();
        if (!string.IsNullOrEmpty(needle))
        {
            filtered = filtered.Where(r => r.Name.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = filtered
            .OrderByDescending(r => r.UpdatedAt ?? DateTimeOffset.MinValue)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Paging.Slice(ordered, page, pageSize);
    }
}
