using SPC.Core.Models;

namespace SPC.Core.Validation;

public static class RecipeValidator
{
    public static IReadOnlyList<string> ValidateRecipe(RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            errors.Add("Recipe name is required.");
        }

        if (recipe.Ingredients.Count == 0)
        {
            errors.Add("Add at least one ingredient.");
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            errors.AddRange(ValidateIngredient(ingredient));
        }

        foreach (var spice in recipe.Spices)
        {
            errors.AddRange(ValidateSpice(spice));
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateIngredient(IngredientDto ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        var errors = new List<string>();
        var label = string.IsNullOrWhiteSpace(ingredient.Name) ? "Ingredient" : ingredient.Name.Trim();

        if (string.IsNullOrWhiteSpace(ingredient.Name))
        {
            errors.Add($"{label}: name is required.");
        }

        if (ingredient.Grams <= 0)
        {
            errors.Add($"{label}: grams must be greater than zero.");
        }

        if (ingredient.CaloriesPer100g < 0)
        {
            errors.Add($"{label}: calories per 100 g cannot be negative.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateSpice(SpiceDto spice)
    {
        ArgumentNullException.ThrowIfNull(spice);

        if (!IsSpiceRowTouched(spice))
        {
            return [];
        }

        var errors = new List<string>();
        var label = string.IsNullOrWhiteSpace(spice.Name) ? "Spice" : spice.Name.Trim();

        if (string.IsNullOrWhiteSpace(spice.Name))
        {
            errors.Add($"{label}: name is required.");
        }

        if (spice.Grams is < 0)
        {
            errors.Add($"{label}: grams cannot be negative.");
        }

        if (spice.CaloriesPer100g is < 0)
        {
            errors.Add($"{label}: calories per 100 g cannot be negative.");
        }

        return errors;
    }

    public static bool IsSpiceRowTouched(SpiceDto spice) =>
        !string.IsNullOrWhiteSpace(spice.Name)
        || spice.Grams is > 0
        || spice.CaloriesPer100g is > 0;

    public static decimal GetTotalGrams(RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var ingredientGrams = recipe.Ingredients.Sum(i => i.Grams);
        var spiceGrams = recipe.Spices.Sum(s => s.Grams ?? 0);
        return ingredientGrams + spiceGrams;
    }

    public static decimal GetIngredientCalories(IngredientDto ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        return ingredient.Grams / 100m * ingredient.CaloriesPer100g;
    }

    public static decimal GetSpiceCalories(SpiceDto spice)
    {
        ArgumentNullException.ThrowIfNull(spice);

        if (spice.Grams is not > 0 || spice.CaloriesPer100g is not > 0)
        {
            return 0;
        }

        return spice.Grams.Value / 100m * spice.CaloriesPer100g.Value;
    }

    public static decimal GetTotalCalories(RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return recipe.Ingredients.Sum(GetIngredientCalories)
            + recipe.Spices.Sum(GetSpiceCalories);
    }

    public static bool IsRecipeValid(RecipeDto recipe) => ValidateRecipe(recipe).Count == 0;
}
