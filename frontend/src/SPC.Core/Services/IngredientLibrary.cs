using SPC.Core.Formatting;
using SPC.Core.Models;

namespace SPC.Core.Services;

/// <summary>
/// Nutrition library matching and save-time sync. Copy-on-use: recipes store their own
/// kcal; the library is a lookup, not a live link.
/// </summary>
public static class IngredientLibrary
{
    public const int SearchLimit = 8;

    public static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var parts = name.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts).ToLowerInvariant();
    }

    public static bool HasNutrition(decimal? kcalPer100g) => kcalPer100g is > 0;

    public static IReadOnlyList<IngredientDto> Search(
        IReadOnlyList<IngredientDto> library,
        string? query,
        int limit = SearchLimit,
        IEnumerable<string>? occupiedNames = null)
    {
        ArgumentNullException.ThrowIfNull(library);

        var needle = NormalizeName(query);
        if (needle.Length == 0)
        {
            return [];
        }

        var occupied = OccupiedSet(occupiedNames);

        return library
            .Select(item => (Item: item, Rank: Rank(item, needle)))
            .Where(x => x.Rank >= 0 && !IsOccupied(x.Item, occupied))
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .Select(x => x.Item)
            .ToList();
    }

    public static IngredientDto? FindExact(IReadOnlyList<IngredientDto> library, string? name)
    {
        ArgumentNullException.ThrowIfNull(library);

        var needle = NormalizeName(name);
        if (needle.Length == 0)
        {
            return null;
        }

        return library.FirstOrDefault(item => NormalizeName(item.Name) == needle);
    }

    public static IEnumerable<(string Name, decimal? CaloriesPer100g)> LinesFrom(RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        foreach (var ingredient in recipe.Ingredients)
        {
            yield return (ingredient.Name, ingredient.CaloriesPer100g);
        }

        foreach (var spice in recipe.Spices)
        {
            yield return (spice.Name, spice.CaloriesPer100g);
        }
    }

    public static IngredientLibrarySync ProposeSync(
        IEnumerable<(string Name, decimal? CaloriesPer100g)> lines,
        IReadOnlyList<IngredientDto> library)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(library);

        var lastByKey = new Dictionary<string, (string DisplayName, decimal Kcal)>(StringComparer.Ordinal);
        foreach (var (name, kcal) in lines)
        {
            if (kcal is not > 0)
            {
                continue;
            }

            var key = NormalizeName(name);
            if (key.Length == 0)
            {
                continue;
            }

            lastByKey[key] = (name.Trim(), kcal.Value);
        }

        var toAdd = new List<IngredientDto>();
        var toUpdate = new List<IngredientLibraryUpdate>();

        foreach (var (_, (displayName, kcal)) in lastByKey)
        {
            var existing = FindExact(library, displayName);
            if (existing is null)
            {
                toAdd.Add(new IngredientDto
                {
                    Name = displayName,
                    CaloriesPer100g = kcal,
                });
                continue;
            }

            if (existing.CaloriesPer100g != kcal)
            {
                toUpdate.Add(new IngredientLibraryUpdate
                {
                    Existing = existing,
                    NewCaloriesPer100g = kcal,
                });
            }
        }

        return new IngredientLibrarySync
        {
            ToAdd = toAdd,
            ToUpdate = toUpdate,
        };
    }

    public static string FormatUpdatePrompt(IReadOnlyList<IngredientLibraryUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var parts = updates.Select(update =>
            $"{update.Existing.Name}: {NumberFormat.Format(update.Existing.CaloriesPer100g)} → {NumberFormat.Format(update.NewCaloriesPer100g)}");

        return "Recipe saved. Update library kcal? Other recipes stay as they are. "
            + string.Join("; ", parts)
            + ".";
    }

    private static int Rank(IngredientDto item, string needle)
    {
        var canonical = NormalizeName(item.Name);
        if (canonical == needle)
        {
            return 0;
        }

        if (canonical.StartsWith(needle, StringComparison.Ordinal))
        {
            return 1;
        }

        if (canonical.Length >= 2 && needle.StartsWith(canonical, StringComparison.Ordinal))
        {
            return 2;
        }

        if (HasWordPrefix(canonical, needle))
        {
            return 3;
        }

        return -1;
    }

    private static bool HasWordPrefix(string name, string needle)
    {
        foreach (var word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.StartsWith(needle, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static HashSet<string> OccupiedSet(IEnumerable<string>? occupiedNames)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (occupiedNames is null)
        {
            return set;
        }

        foreach (var name in occupiedNames)
        {
            var key = NormalizeName(name);
            if (key.Length > 0)
            {
                set.Add(key);
            }
        }

        return set;
    }

    private static bool IsOccupied(IngredientDto item, HashSet<string> occupied)
    {
        if (occupied.Count == 0)
        {
            return false;
        }

        return occupied.Contains(NormalizeName(item.Name));
    }
}

public sealed class IngredientLibrarySync
{
    public IReadOnlyList<IngredientDto> ToAdd { get; init; } = [];

    public IReadOnlyList<IngredientLibraryUpdate> ToUpdate { get; init; } = [];
}

public sealed class IngredientLibraryUpdate
{
    public required IngredientDto Existing { get; init; }

    public required decimal NewCaloriesPer100g { get; init; }
}
