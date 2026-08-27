namespace SPC.Core.Models;

public sealed class RecipeDto
{
    public const string DefaultVariantLabel = "Default";

    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Shared by every variation of one dish. Empty on old rows means <see cref="Id"/>.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>Short name for a variation (extra onion, turkey). Empty is the default row in the family.</summary>
    public string VariantLabel { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Which meal this dish is for. Drives the profile kcal suggestion; default lunch.</summary>
    public MealType MealType { get; set; } = MealType.Lunch;

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<RecipeIngredientDto> Ingredients { get; set; } = [];

    public List<SpiceDto> Spices { get; set; } = [];

    public List<InstructionStepDto> Instructions { get; set; } = [];

    /// <summary>Free-form notes for this variant (same editor as a single instruction step).</summary>
    public InstructionStepDto Notes { get; set; } = new();

    /// <summary>Cooked dish weight in grams, if weighed. Used for yield and future planning.</summary>
    public decimal? ActualDishWeightG { get; set; }

    public static bool IsUnnamedVariant(string? label) =>
        string.IsNullOrWhiteSpace(label)
        || string.Equals(label.Trim(), DefaultVariantLabel, StringComparison.OrdinalIgnoreCase);

    public static string NormalizeVariantLabel(string? label)
    {
        if (IsUnnamedVariant(label))
        {
            return string.Empty;
        }

        return label!.Trim();
    }

    public string DisplayVariantLabel() =>
        IsUnnamedVariant(VariantLabel) ? DefaultVariantLabel : VariantLabel.Trim();

    public string DisplayTitle()
    {
        var name = string.IsNullOrWhiteSpace(Name) ? "Untitled recipe" : Name.Trim();
        return IsUnnamedVariant(VariantLabel) ? name : $"{name} ({VariantLabel.Trim()})";
    }
}
