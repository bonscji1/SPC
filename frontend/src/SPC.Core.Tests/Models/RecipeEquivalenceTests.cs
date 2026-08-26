using SPC.Core.Models;
using SPC.Core.Services;
using Xunit;

namespace SPC.Core.Tests.Models;

public class RecipeEquivalenceTests
{
    [Fact]
    public void AreEquivalent_ReturnsTrue_ForMatchingRecipes()
    {
        var left = CreateSample();
        var right = left.Clone();

        Assert.True(RecipeEquivalence.AreEquivalent(left, right));
    }

    [Fact]
    public void AreEquivalent_ReturnsFalse_WhenNameDiffers()
    {
        var left = CreateSample();
        var right = left.Clone();
        right.Name = "Different";

        Assert.False(RecipeEquivalence.AreEquivalent(left, right));
    }

    [Fact]
    public void AreEquivalent_IgnoresUpdatedAt()
    {
        var left = CreateSample();
        var right = left.Clone();
        right.UpdatedAt = DateTimeOffset.UtcNow;

        Assert.True(RecipeEquivalence.AreEquivalent(left, right));
    }

    [Fact]
    public void AreEquivalent_ReturnsFalse_WhenActualWeightDiffers()
    {
        var left = CreateSample();
        var right = left.Clone();
        right.ActualDishWeightG = 800;

        Assert.False(RecipeEquivalence.AreEquivalent(left, right));
    }

    [Fact]
    public void AreEquivalent_ReturnsFalse_WhenInstructionsDiffer()
    {
        var left = CreateSample();
        var right = left.Clone();
        right.Instructions.Add(InstructionEditor.NewStep());
        right.Instructions[^1].Tokens[0].Text = "bake";

        Assert.False(RecipeEquivalence.AreEquivalent(left, right));
    }

    [Fact]
    public void AreEquivalent_ReturnsFalse_WhenMealTypeDiffers()
    {
        var left = CreateSample();
        var right = left.Clone();
        right.MealType = MealType.Dinner;

        Assert.False(RecipeEquivalence.AreEquivalent(left, right));
    }

    private static RecipeDto CreateSample() => new()
    {
        Name = "Stew",
        Ingredients = [new RecipeIngredientDto { Name = "carrot", Grams = 100, CaloriesPer100g = 41 }],
        Spices = [new SpiceDto { Name = "salt", Grams = 2 }],
    };
}
