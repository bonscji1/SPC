using SPC.Core.Services;

namespace SPC.Core.Models;

public static class RecipeList
{
    public static PagedResult<RecipeFamilyGroup> Page(
        IReadOnlyList<RecipeDto> recipes,
        int page,
        int pageSize,
        MealType? mealType = null,
        string? nameQuery = null)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        var families = recipes
            .GroupBy(RecipeScaler.FamilyKey)
            .Select(BuildFamily)
            .Where(family => Matches(family, mealType, nameQuery))
            .OrderByDescending(family => FamilyUpdatedAt(family))
            .ThenBy(family => family.Primary.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Paging.Slice(families, page, pageSize);
    }

    public static RecipeFamilyGroup BuildFamily(IEnumerable<RecipeDto> members)
    {
        var list = members.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("A recipe family must have at least one recipe.", nameof(members));
        }

        var familyId = RecipeScaler.FamilyKey(list[0]);
        var primary = PickPrimary(list, familyId);
        var variants = list
            .Where(r => r.Id != primary.Id)
            .OrderBy(r => r.VariantLabel, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new RecipeFamilyGroup
        {
            FamilyId = familyId,
            Primary = primary,
            Variants = variants,
        };
    }

    public static bool VariantLabelIsTaken(IEnumerable<RecipeDto> members, Guid exceptId, string normalizedLabel)
    {
        ArgumentNullException.ThrowIfNull(members);

        return members.Any(m =>
            m.Id != exceptId
            && string.Equals(
                RecipeDto.NormalizeVariantLabel(m.VariantLabel),
                normalizedLabel,
                StringComparison.OrdinalIgnoreCase));
    }

    private static RecipeDto PickPrimary(IReadOnlyList<RecipeDto> members, Guid familyId)
    {
        return members.FirstOrDefault(m => string.IsNullOrWhiteSpace(m.VariantLabel) && m.Id == familyId)
            ?? members.FirstOrDefault(m => string.IsNullOrWhiteSpace(m.VariantLabel))
            ?? members.FirstOrDefault(m => m.Id == familyId)
            ?? members.OrderBy(m => m.UpdatedAt ?? DateTimeOffset.MinValue).First();
    }

    private static bool Matches(RecipeFamilyGroup family, MealType? mealType, string? nameQuery)
    {
        var members = family.AllMembers;

        if (mealType is MealType type && members.All(r => r.MealType != type))
        {
            return false;
        }

        var needle = nameQuery?.Trim();
        if (string.IsNullOrEmpty(needle))
        {
            return true;
        }

        return members.Any(r =>
            r.Name.Contains(needle, StringComparison.OrdinalIgnoreCase)
            || r.VariantLabel.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static DateTimeOffset FamilyUpdatedAt(RecipeFamilyGroup family)
    {
        var dates = family.Variants.Select(r => r.UpdatedAt ?? DateTimeOffset.MinValue)
            .Append(family.Primary.UpdatedAt ?? DateTimeOffset.MinValue);
        return dates.Max();
    }
}
