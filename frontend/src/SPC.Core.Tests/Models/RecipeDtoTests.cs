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
        Assert.Equal(MealType.Lunch, recipe.MealType);
        Assert.NotEqual(Guid.Empty, recipe.Id);
        Assert.Equal(string.Empty, recipe.VariantLabel);
        Assert.Equal(RecipeDto.DefaultVariantLabel, recipe.DisplayVariantLabel());
        Assert.NotNull(recipe.Notes);
    }

    [Fact]
    public void DisplayVariantLabel_UsesDefaultWhenEmpty()
    {
        var recipe = new RecipeDto { Name = "Stew", VariantLabel = "  " };

        Assert.Equal("Default", recipe.DisplayVariantLabel());
        Assert.Equal("Stew", recipe.DisplayTitle());
    }

    [Fact]
    public void DisplayVariantLabel_KeepsNamedVariant()
    {
        var recipe = new RecipeDto { Name = "Stew", VariantLabel = " extra onion " };

        Assert.Equal("extra onion", recipe.DisplayVariantLabel());
        Assert.Equal("Stew (extra onion)", recipe.DisplayTitle());
    }

    [Fact]
    public void IsUnnamedVariant_TreatsDefaultAsBase()
    {
        Assert.True(RecipeDto.IsUnnamedVariant(null));
        Assert.True(RecipeDto.IsUnnamedVariant(""));
        Assert.True(RecipeDto.IsUnnamedVariant("Default"));
        Assert.False(RecipeDto.IsUnnamedVariant("turkey"));
    }

    [Fact]
    public void NormalizeVariantLabel_ClearsDefaultAndWhitespace()
    {
        Assert.Equal(string.Empty, RecipeDto.NormalizeVariantLabel(null));
        Assert.Equal(string.Empty, RecipeDto.NormalizeVariantLabel("  "));
        Assert.Equal(string.Empty, RecipeDto.NormalizeVariantLabel("Default"));
        Assert.Equal("extra onion", RecipeDto.NormalizeVariantLabel(" extra onion "));
    }
}
