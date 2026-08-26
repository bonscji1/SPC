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
            MealType = MealType.Dinner,
            Ingredients = [new RecipeIngredientDto { Name = "carrot", Grams = 100, CaloriesPer100g = 41 }],
            Spices = [new SpiceDto { Name = "salt" }],
            ActualDishWeightG = 1234m,
            Instructions =
            [
                new InstructionStepDto
                {
                    Tokens = [new InstructionTokenDto { Text = "mix flour" }],
                },
            ],
        };

        var clone = original.Clone();
        clone.Name = "Changed";
        clone.MealType = MealType.Breakfast;
        clone.Ingredients[0].Grams = 999;
        clone.ActualDishWeightG = 50m;
        clone.Instructions[0].Tokens[0].Text = "changed";

        Assert.Equal("Stew", original.Name);
        Assert.Equal(MealType.Dinner, original.MealType);
        Assert.Equal(MealType.Breakfast, clone.MealType);
        Assert.Equal(100, original.Ingredients[0].Grams);
        Assert.Equal(1234m, original.ActualDishWeightG);
        Assert.Equal(50m, clone.ActualDishWeightG);
        Assert.Equal("mix flour", original.Instructions[0].Tokens[0].Text);
    }
}
