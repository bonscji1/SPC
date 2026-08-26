using SPC.Core.Models;
using SPC.Core.Services;
using Xunit;

namespace SPC.Core.Tests.Services;

public class IngredientLibraryTests
{
    [Fact]
    public void NormalizeName_TrimsAndLowercases()
    {
        Assert.Equal("olive oil", IngredientLibrary.NormalizeName("  Olive   Oil "));
        Assert.Equal(string.Empty, IngredientLibrary.NormalizeName("   "));
    }

    [Fact]
    public void Search_PrefixAndOnionsMatchOnion()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };
        var carrot = new IngredientDto { Name = "carrot", CaloriesPer100g = 41 };

        var oni = IngredientLibrary.Search([onion, carrot], "oni");
        Assert.Equal(["onion"], oni.Select(i => i.Name));

        var onions = IngredientLibrary.Search([onion, carrot], "onions");
        Assert.Equal(["onion"], onions.Select(i => i.Name));
    }

    [Fact]
    public void Search_MatchesWordPrefix()
    {
        var oil = new IngredientDto { Name = "olive oil", CaloriesPer100g = 884 };

        var result = IngredientLibrary.Search([oil], "oil");

        Assert.Equal(["olive oil"], result.Select(i => i.Name));
    }

    [Fact]
    public void Search_EmptyQuery_ReturnsNothing()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };

        Assert.Empty(IngredientLibrary.Search([onion], "  "));
    }

    [Fact]
    public void Search_HidesOccupiedNames()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };
        var carrot = new IngredientDto { Name = "carrot", CaloriesPer100g = 41 };

        var withoutOnion = IngredientLibrary.Search([onion, carrot], "o", occupiedNames: ["Onion"]);
        Assert.Empty(withoutOnion);

        var all = IngredientLibrary.Search([onion, carrot], "o");
        Assert.Equal(["onion"], all.Select(i => i.Name));
    }

    [Fact]
    public void FindExact_MatchesNameIgnoringCase()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };

        Assert.Same(onion, IngredientLibrary.FindExact([onion], "Onion"));
        Assert.Null(IngredientLibrary.FindExact([onion], "onions"));
    }

    [Fact]
    public void ProposeSync_AddsNewAndSkipsUnchanged()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };
        var recipe = new RecipeDto
        {
            Ingredients =
            [
                new RecipeIngredientDto { Name = "onion", Grams = 100, CaloriesPer100g = 40 },
                new RecipeIngredientDto { Name = "carrot", Grams = 80, CaloriesPer100g = 41 },
            ],
        };

        var sync = IngredientLibrary.ProposeSync(IngredientLibrary.LinesFrom(recipe), [onion]);

        Assert.Empty(sync.ToUpdate);
        Assert.Equal(["carrot"], sync.ToAdd.Select(i => i.Name));
        Assert.Equal(41, sync.ToAdd[0].CaloriesPer100g);
    }

    [Fact]
    public void ProposeSync_FlagsKcalChangeAndIgnoresZeroKcalSpice()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };
        var recipe = new RecipeDto
        {
            Ingredients =
            [
                new RecipeIngredientDto { Name = "onion", Grams = 100, CaloriesPer100g = 32 },
            ],
            Spices =
            [
                new SpiceDto { Name = "salt" },
            ],
        };

        var sync = IngredientLibrary.ProposeSync(IngredientLibrary.LinesFrom(recipe), [onion]);

        Assert.Empty(sync.ToAdd);
        Assert.Single(sync.ToUpdate);
        Assert.Equal(32, sync.ToUpdate[0].NewCaloriesPer100g);
        Assert.Equal("onion", sync.ToUpdate[0].Existing.Name);
    }

    [Fact]
    public void ProposeSync_LastRowWinsForSameName()
    {
        var recipe = new RecipeDto
        {
            Ingredients =
            [
                new RecipeIngredientDto { Name = "onion", CaloriesPer100g = 40 },
                new RecipeIngredientDto { Name = "Onion", CaloriesPer100g = 32 },
            ],
        };

        var sync = IngredientLibrary.ProposeSync(IngredientLibrary.LinesFrom(recipe), []);

        Assert.Single(sync.ToAdd);
        Assert.Equal(32, sync.ToAdd[0].CaloriesPer100g);
    }

    [Fact]
    public void FormatUpdatePrompt_ListsChanges()
    {
        var update = new IngredientLibraryUpdate
        {
            Existing = new IngredientDto { Name = "onion", CaloriesPer100g = 40 },
            NewCaloriesPer100g = 32,
        };

        var text = IngredientLibrary.FormatUpdatePrompt([update]);

        Assert.Contains("onion: 40 → 32", text);
        Assert.Contains("Other recipes stay as they are", text);
    }
}
