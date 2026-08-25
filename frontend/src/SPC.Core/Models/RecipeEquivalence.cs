using SPC.Core.Models;

namespace SPC.Core.Models;

public static class RecipeEquivalence
{
    public static bool AreEquivalent(RecipeDto left, RecipeDto right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (!string.Equals(left.Name, right.Name, StringComparison.Ordinal)
            || left.ActualDishWeightG != right.ActualDishWeightG)
        {
            return false;
        }

        if (left.Ingredients.Count != right.Ingredients.Count
            || left.Spices.Count != right.Spices.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Ingredients.Count; i++)
        {
            if (!AreEquivalent(left.Ingredients[i], right.Ingredients[i]))
            {
                return false;
            }
        }

        for (var i = 0; i < left.Spices.Count; i++)
        {
            if (!AreEquivalent(left.Spices[i], right.Spices[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreEquivalent(IngredientDto left, IngredientDto right) =>
        left.Name == right.Name
        && left.Grams == right.Grams
        && left.CaloriesPer100g == right.CaloriesPer100g;

    private static bool AreEquivalent(SpiceDto left, SpiceDto right) =>
        left.Name == right.Name
        && left.Grams == right.Grams
        && left.CaloriesPer100g == right.CaloriesPer100g;
}
