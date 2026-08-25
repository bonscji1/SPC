using SPC.Core.Models;
using SPC.Core.Validation;
using Xunit;

namespace SPC.Core.Tests.Validation;

public class RecipeValidatorTests
{
    [Fact]
    public void ValidateRecipe_ReturnsError_WhenNameMissing()
    {
        var recipe = new RecipeDto
        {
            Ingredients = [new IngredientDto { Name = "carrot", Grams = 100, CaloriesPer100g = 41 }],
        };

        var errors = RecipeValidator.ValidateRecipe(recipe);

        Assert.Contains("Recipe name is required.", errors);
    }

    [Fact]
    public void ValidateRecipe_ReturnsError_WhenNoIngredients()
    {
        var recipe = new RecipeDto { Name = "Stew" };

        var errors = RecipeValidator.ValidateRecipe(recipe);

        Assert.Contains("Add at least one ingredient.", errors);
    }

    [Fact]
    public void ValidateIngredient_ReturnsError_WhenGramsNotPositive()
    {
        var ingredient = new IngredientDto { Name = "carrot", Grams = 0, CaloriesPer100g = 41 };

        var errors = RecipeValidator.ValidateIngredient(ingredient);

        Assert.Contains("carrot: grams must be greater than zero.", errors);
    }

    [Fact]
    public void ValidateIngredient_ReturnsError_WhenCaloriesNegative()
    {
        var ingredient = new IngredientDto { Name = "carrot", Grams = 100, CaloriesPer100g = -1 };

        var errors = RecipeValidator.ValidateIngredient(ingredient);

        Assert.Contains("carrot: calories per 100 g cannot be negative.", errors);
    }

    [Fact]
    public void IsRecipeValid_ReturnsTrue_ForCompleteRecipe()
    {
        var recipe = new RecipeDto
        {
            Name = "Stew",
            Ingredients =
            [
                new IngredientDto { Name = "carrot", Grams = 200, CaloriesPer100g = 41 },
                new IngredientDto { Name = "potato", Grams = 150, CaloriesPer100g = 77 },
            ],
        };

        Assert.True(RecipeValidator.IsRecipeValid(recipe));
    }

    [Fact]
    public void GetTotalGrams_SumsIngredientWeights()
    {
        var recipe = new RecipeDto
        {
            Ingredients =
            [
                new IngredientDto { Grams = 200 },
                new IngredientDto { Grams = 150.5m },
            ],
        };

        Assert.Equal(350.5m, RecipeValidator.GetTotalGrams(recipe));
    }

    [Fact]
    public void GetTotalCalories_CalculatesFromIngredients()
    {
        var recipe = new RecipeDto
        {
            Ingredients = [new IngredientDto { Grams = 200, CaloriesPer100g = 41 }],
        };

        Assert.Equal(82m, RecipeValidator.GetTotalCalories(recipe));
    }

    [Fact]
    public void ValidateSpice_ReturnsNoErrors_WhenRowEmpty()
    {
        var spice = new SpiceDto();

        Assert.Empty(RecipeValidator.ValidateSpice(spice));
    }

    [Fact]
    public void ValidateSpice_AllowsNameOnly()
    {
        var spice = new SpiceDto { Name = "salt" };

        Assert.Empty(RecipeValidator.ValidateSpice(spice));
    }

    [Fact]
    public void ValidateSpice_ReturnsError_WhenNameMissingButGramsProvided()
    {
        var spice = new SpiceDto { Grams = 5 };

        var errors = RecipeValidator.ValidateSpice(spice);

        Assert.Contains("Spice: name is required.", errors);
    }

    [Fact]
    public void GetTotalGrams_IncludesSpiceGrams()
    {
        var recipe = new RecipeDto
        {
            Ingredients = [new IngredientDto { Grams = 200 }],
            Spices = [new SpiceDto { Name = "salt", Grams = 5 }],
        };

        Assert.Equal(205m, RecipeValidator.GetTotalGrams(recipe));
    }
}
