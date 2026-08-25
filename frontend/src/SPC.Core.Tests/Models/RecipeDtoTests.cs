using SPC.Core.Models;
using Xunit;

namespace SPC.Core.Tests.Models;

public class RecipeDtoTests
{
    [Fact]
    public void RecipeDto_DefaultsToEmptyIngredients()
    {
        var recipe = new RecipeDto { Name = "Test stew" };

        Assert.Equal("Test stew", recipe.Name);
        Assert.Empty(recipe.Ingredients);
        Assert.NotEqual(Guid.Empty, recipe.Id);
    }
}
