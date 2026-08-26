using SPC.Core.Models;

namespace SPC.Core.Validation;

public static class IngredientValidator
{
    public static IReadOnlyList<string> Validate(IngredientDto ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ingredient.Name))
        {
            errors.Add("Name is required.");
        }

        if (ingredient.CaloriesPer100g <= 0)
        {
            errors.Add("kcal per 100 g must be greater than zero.");
        }

        return errors;
    }

    public static bool IsValid(IngredientDto ingredient) => Validate(ingredient).Count == 0;
}
