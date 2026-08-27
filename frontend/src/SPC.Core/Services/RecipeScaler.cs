using SPC.Core.Models;
using SPC.Core.Validation;

namespace SPC.Core.Services;

public enum RecipeScaleSizeKind
{
    GramsPerPortion,
    CaloriesPerPortion,
}

public sealed class RecipeScaleResult
{
    public bool Success { get; init; }

    public string? Error { get; init; }

    public RecipeDto? Recipe { get; init; }

    public decimal ScaleFactor { get; init; }

    public decimal TheoreticalWeightG { get; init; }

    public decimal TheoreticalCalories { get; init; }

    public static RecipeScaleResult Fail(string error) => new() { Success = false, Error = error };
}

public static class RecipeScaler
{
    public static Guid FamilyKey(RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return recipe.FamilyId == Guid.Empty ? recipe.Id : recipe.FamilyId;
    }

    public static RecipeScaleResult Scale(
        RecipeDto source,
        decimal portions,
        RecipeScaleSizeKind sizeKind,
        decimal portionSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (portions <= 0)
        {
            return RecipeScaleResult.Fail("Number of portions must be greater than zero.");
        }

        if (portionSize <= 0)
        {
            return RecipeScaleResult.Fail("Portion size must be greater than zero.");
        }

        var theoreticalWeightG = RecipeValidator.GetTotalGrams(source);
        var theoreticalCalories = RecipeValidator.GetTotalCalories(source);

        decimal scaleFactor;
        switch (sizeKind)
        {
            case RecipeScaleSizeKind.GramsPerPortion:
                if (theoreticalWeightG <= 0)
                {
                    return RecipeScaleResult.Fail("Dish weight must be greater than zero.");
                }

                scaleFactor = portions * portionSize / theoreticalWeightG;
                break;
            case RecipeScaleSizeKind.CaloriesPerPortion:
                if (theoreticalCalories <= 0)
                {
                    return RecipeScaleResult.Fail("Dish calories must be greater than zero.");
                }

                scaleFactor = portions * portionSize / theoreticalCalories;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sizeKind), sizeKind, null);
        }

        var scaled = source.Clone();
        scaled.ActualDishWeightG = null;

        foreach (var ingredient in scaled.Ingredients)
        {
            ingredient.Grams = RoundGrams(ingredient.Grams * scaleFactor);
        }

        foreach (var spice in scaled.Spices)
        {
            if (spice.Grams is > 0)
            {
                spice.Grams = RoundGrams(spice.Grams.Value * scaleFactor);
            }
        }

        return new RecipeScaleResult
        {
            Success = true,
            Recipe = scaled,
            ScaleFactor = scaleFactor,
            TheoreticalWeightG = RecipeValidator.GetTotalGrams(scaled),
            TheoreticalCalories = RecipeValidator.GetTotalCalories(scaled),
        };
    }

    public static RecipeDto AsNewRecipe(RecipeDto scaled, string name)
    {
        ArgumentNullException.ThrowIfNull(scaled);

        var copy = scaled.Clone();
        copy.Id = Guid.NewGuid();
        copy.FamilyId = copy.Id;
        copy.VariantLabel = string.Empty;
        copy.Name = name.Trim();
        copy.UpdatedAt = null;
        copy.ActualDishWeightG = null;
        return copy;
    }

    public static RecipeDto AsVariant(RecipeDto scaled, Guid familyId, string variantLabel)
    {
        ArgumentNullException.ThrowIfNull(scaled);

        var copy = scaled.Clone();
        copy.Id = Guid.NewGuid();
        copy.FamilyId = familyId;
        copy.VariantLabel = variantLabel.Trim();
        copy.UpdatedAt = null;
        copy.ActualDishWeightG = null;
        return copy;
    }

    private static decimal RoundGrams(decimal grams) =>
        decimal.Round(grams, 2, MidpointRounding.AwayFromZero);
}
