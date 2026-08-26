namespace SPC.Core.Models;

public static class IngredientList
{
    public static PagedResult<IngredientDto> Page(
        IReadOnlyList<IngredientDto> items,
        int page,
        int pageSize,
        string? nameQuery = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        IEnumerable<IngredientDto> filtered = items;
        var needle = nameQuery?.Trim();
        if (!string.IsNullOrEmpty(needle))
        {
            filtered = items.Where(item => Matches(item, needle));
        }

        var ordered = filtered
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Paging.Slice(ordered, page, pageSize);
    }

    private static bool Matches(IngredientDto item, string needle) =>
        item.Name.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
