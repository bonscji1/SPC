namespace SPC.Core.Models;

public static class RecipeCloneExtensions
{
    public static RecipeDto Clone(this RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return new RecipeDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            UpdatedAt = recipe.UpdatedAt,
            Ingredients = recipe.Ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Grams = i.Grams,
                CaloriesPer100g = i.CaloriesPer100g,
            }).ToList(),
            Spices = recipe.Spices.Select(s => new SpiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Grams = s.Grams,
                CaloriesPer100g = s.CaloriesPer100g,
            }).ToList(),
            ActualDishWeightG = recipe.ActualDishWeightG,
        };
    }
}
