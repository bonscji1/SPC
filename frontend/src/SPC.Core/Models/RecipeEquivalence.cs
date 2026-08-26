using SPC.Core.Models;

namespace SPC.Core.Models;

public static class RecipeEquivalence
{
    public static bool AreEquivalent(RecipeDto left, RecipeDto right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (!string.Equals(left.Name, right.Name, StringComparison.Ordinal)
            || left.MealType != right.MealType
            || left.ActualDishWeightG != right.ActualDishWeightG)
        {
            return false;
        }

        var leftSteps = left.Instructions ?? [];
        var rightSteps = right.Instructions ?? [];

        if (left.Ingredients.Count != right.Ingredients.Count
            || left.Spices.Count != right.Spices.Count
            || leftSteps.Count != rightSteps.Count)
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

        for (var i = 0; i < leftSteps.Count; i++)
        {
            if (!AreEquivalent(leftSteps[i], rightSteps[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreEquivalent(InstructionStepDto left, InstructionStepDto right)
    {
        if (!string.Equals(left.EditorJson, right.EditorJson, StringComparison.Ordinal))
        {
            return false;
        }

        if (left.Tokens.Count != right.Tokens.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Tokens.Count; i++)
        {
            var a = left.Tokens[i];
            var b = right.Tokens[i];
            if (a.Kind != b.Kind || a.ItemId != b.ItemId
                || !string.Equals(a.Text, b.Text, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreEquivalent(RecipeIngredientDto left, RecipeIngredientDto right) =>
        left.Name == right.Name
        && left.Grams == right.Grams
        && left.CaloriesPer100g == right.CaloriesPer100g;

    private static bool AreEquivalent(SpiceDto left, SpiceDto right) =>
        left.Name == right.Name
        && left.Grams == right.Grams
        && left.CaloriesPer100g == right.CaloriesPer100g;
}
