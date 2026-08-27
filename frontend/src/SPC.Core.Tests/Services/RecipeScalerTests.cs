using SPC.Core.Models;
using SPC.Core.Services;
using Xunit;

namespace SPC.Core.Tests.Services;

public class RecipeScalerTests
{
    [Fact]
    public void Scale_GramsPath_MultipliesWeighedLines()
    {
        var recipe = new RecipeDto
        {
            Ingredients =
            [
                new RecipeIngredientDto { Name = "carrot", Grams = 300, CaloriesPer100g = 41 },
                new RecipeIngredientDto { Name = "beef", Grams = 300, CaloriesPer100g = 250 },
            ],
            Spices =
            [
                new SpiceDto { Name = "salt", Grams = 6, CaloriesPer100g = 0 },
                new SpiceDto { Name = "bay leaf" },
            ],
        };

        var result = RecipeScaler.Scale(recipe, portions: 8, RecipeScaleSizeKind.GramsPerPortion, portionSize: 100);

        Assert.True(result.Success);
        Assert.Equal(800m / 606m, result.ScaleFactor);
        Assert.Equal(396.04m, result.Recipe!.Ingredients[0].Grams);
        Assert.Equal(396.04m, result.Recipe.Ingredients[1].Grams);
        Assert.Equal(7.92m, result.Recipe.Spices[0].Grams);
        Assert.Null(result.Recipe.Spices[1].Grams);
        Assert.Null(result.Recipe.ActualDishWeightG);
        Assert.Equal(recipe.Ingredients[0].Id, result.Recipe.Ingredients[0].Id);
        Assert.Equal(300m, recipe.Ingredients[0].Grams);
    }

    [Fact]
    public void Scale_SixToEightPortions_AtSameGramsPerPortion()
    {
        var recipe = new RecipeDto
        {
            Ingredients = [new RecipeIngredientDto { Name = "mix", Grams = 600, CaloriesPer100g = 100 }],
        };

        var result = RecipeScaler.Scale(recipe, portions: 8, RecipeScaleSizeKind.GramsPerPortion, portionSize: 100);

        Assert.True(result.Success);
        Assert.Equal(8m / 6m, result.ScaleFactor);
        Assert.Equal(800m, result.Recipe!.Ingredients[0].Grams);
    }

    [Fact]
    public void Scale_CaloriesPath_UsesTheoreticalCalories()
    {
        var recipe = new RecipeDto
        {
            Ingredients = [new RecipeIngredientDto { Name = "rice", Grams = 200, CaloriesPer100g = 350 }],
        };

        var result = RecipeScaler.Scale(recipe, portions: 4, RecipeScaleSizeKind.CaloriesPerPortion, portionSize: 175);

        Assert.True(result.Success);
        Assert.Equal(1m, result.ScaleFactor);
        Assert.Equal(200m, result.Recipe!.Ingredients[0].Grams);
    }

    [Fact]
    public void Scale_ZeroWeight_Fails()
    {
        var recipe = new RecipeDto();

        var result = RecipeScaler.Scale(recipe, 2, RecipeScaleSizeKind.GramsPerPortion, 100);

        Assert.False(result.Success);
        Assert.Equal("Dish weight must be greater than zero.", result.Error);
    }

    [Fact]
    public void AsNewRecipe_GetsOwnFamily()
    {
        var scaled = new RecipeDto
        {
            Id = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            Name = "Stew",
            Ingredients = [new RecipeIngredientDto { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "carrot", Grams = 100, CaloriesPer100g = 41 }],
        };

        var created = RecipeScaler.AsNewRecipe(scaled, "Stew (scaled)");

        Assert.NotEqual(scaled.Id, created.Id);
        Assert.Equal(created.Id, created.FamilyId);
        Assert.Equal("Stew (scaled)", created.Name);
        Assert.Equal(string.Empty, created.VariantLabel);
        Assert.Equal(scaled.Ingredients[0].Id, created.Ingredients[0].Id);
    }

    [Fact]
    public void AsVariant_KeepsFamily_NewId()
    {
        var familyId = Guid.NewGuid();
        var scaled = new RecipeDto
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Name = "Stew",
            Ingredients = [new RecipeIngredientDto { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "carrot", Grams = 100, CaloriesPer100g = 41 }],
        };

        var variant = RecipeScaler.AsVariant(scaled, familyId, " extra onion ");

        Assert.NotEqual(scaled.Id, variant.Id);
        Assert.Equal(familyId, variant.FamilyId);
        Assert.Equal("extra onion", variant.VariantLabel);
        Assert.Equal(scaled.Ingredients[0].Id, variant.Ingredients[0].Id);
    }
}
