using SPC.Core.Models;
using Xunit;

namespace SPC.Core.Tests.Models;

public class RecipeCloneExtensionsTests
{
    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new RecipeDto
        {
            Name = "Stew",
            Ingredients = [new IngredientDto { Name = "carrot", Grams = 100, CaloriesPer100g = 41 }],
            Spices = [new SpiceDto { Name = "salt" }],
            ActualDishWeightG = 1234m,
        };

        var clone = original.Clone();
        clone.Name = "Changed";
        clone.Ingredients[0].Grams = 999;
        clone.ActualDishWeightG = 50m;

        Assert.Equal("Stew", original.Name);
        Assert.Equal(100, original.Ingredients[0].Grams);
        Assert.Equal(1234m, original.ActualDishWeightG);
        Assert.Equal(50m, clone.ActualDishWeightG);
    }
}
