namespace SPC.Core.Models;

public static class RecipeCloneExtensions
{
    public static RecipeDto Clone(this RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return new RecipeDto
        {
            Id = recipe.Id,
            FamilyId = recipe.FamilyId,
            VariantLabel = recipe.VariantLabel,
            Name = recipe.Name,
            MealType = recipe.MealType,
            UpdatedAt = recipe.UpdatedAt,
            Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientDto
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
            Instructions = (recipe.Instructions ?? []).Select(CloneStep).ToList(),
            Notes = CloneStep(recipe.Notes),
            ActualDishWeightG = recipe.ActualDishWeightG,
        };
    }

    private static InstructionStepDto CloneStep(InstructionStepDto? step)
    {
        var source = step ?? new InstructionStepDto();
        return new InstructionStepDto
        {
            Id = source.Id,
            EditorJson = source.EditorJson,
            Tokens = (source.Tokens ?? []).Select(t => new InstructionTokenDto
            {
                Id = t.Id,
                Kind = t.Kind,
                Text = t.Text,
                ItemId = t.ItemId,
            }).ToList(),
        };
    }
}
