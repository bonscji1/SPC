using SPC.Core.Models;
using SPC.Core.Services;
using Xunit;

namespace SPC.Core.Tests.Services;

public class PortionCalculatorTests
{
    private readonly PortionCalculator _calculator = new();

    [Fact]
    public void Calculate_WholePortions_FromCaloriesPerPortion()
    {
        var recipe = Recipe(grams: 500, kcalPer100g: 100);

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, 250);

        Assert.True(result.HasPortions);
        Assert.Equal(500m, result.TheoreticalWeightG);
        Assert.Equal(500m, result.TheoreticalCalories);
        Assert.Equal(500m, result.DishWeightG);
        Assert.Equal(2m, result.Portions);
        Assert.Equal(250m, result.KcalPerPortion);
        Assert.Equal(250m, result.GramsPerPortion);
        Assert.Equal(100m, result.KcalPer100gCooked);
        Assert.Equal(2, result.FullPortions);
        Assert.Equal(0m, result.LeftoverPortions);
        Assert.Equal(0m, result.LeftoverGrams);
        Assert.Equal(0m, result.LeftoverCalories);
    }

    [Fact]
    public void Calculate_CookedWeight_DoesNotChangeBatchCalories()
    {
        var recipe = Recipe(grams: 500, kcalPer100g: 100);

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, 250, actualDishWeightG: 400);

        Assert.Equal(500m, result.TheoreticalCalories);
        Assert.Equal(400m, result.DishWeightG);
        Assert.Equal(2m, result.Portions);
        Assert.Equal(250m, result.KcalPerPortion);
        Assert.Equal(200m, result.GramsPerPortion);
        Assert.Equal(125m, result.KcalPer100gCooked);
    }

    [Fact]
    public void Calculate_FromPortionCount_DerivesKcalPerPortion()
    {
        var recipe = Recipe(grams: 500, kcalPer100g: 100);

        var result = _calculator.Calculate(recipe, PortionTargetKind.Portions, 4);

        Assert.Equal(4m, result.Portions);
        Assert.Equal(125m, result.KcalPerPortion);
        Assert.Equal(125m, result.GramsPerPortion);
        Assert.Equal(500m, result.TheoreticalCalories);
    }

    [Fact]
    public void Calculate_FromGramsPerPortion_DerivesCountAndKcal()
    {
        var recipe = Recipe(grams: 500, kcalPer100g: 100);

        var result = _calculator.Calculate(recipe, PortionTargetKind.GramsPerPortion, 200);

        Assert.Equal(2.5m, result.Portions);
        Assert.Equal(200m, result.GramsPerPortion);
        Assert.Equal(200m, result.KcalPerPortion);
        Assert.Equal(2, result.FullPortions);
        Assert.Equal(100m, result.LeftoverGrams);
        Assert.Equal(100m, result.LeftoverCalories);
    }

    [Fact]
    public void Calculate_IncludesSpiceWeightAndCalories()
    {
        var recipe = new RecipeDto
        {
            Ingredients =
            [
                new IngredientDto { Name = "carrot", Grams = 200, CaloriesPer100g = 41 },
            ],
            Spices =
            [
                new SpiceDto { Name = "salt", Grams = 5 },
                new SpiceDto { Name = "paprika", Grams = 10, CaloriesPer100g = 280 },
            ],
        };

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, 50);

        Assert.Equal(215m, result.TheoreticalWeightG);
        Assert.Equal(110m, result.TheoreticalCalories);
        Assert.Equal(2.2m, result.Portions);
        Assert.Equal(215m / 2.2m, result.GramsPerPortion);
    }

    [Fact]
    public void Calculate_FractionalPortionsLessThanOne()
    {
        var recipe = Recipe(grams: 100, kcalPer100g: 50);

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, 200);

        Assert.Equal(50m, result.TheoreticalCalories);
        Assert.Equal(0.25m, result.Portions);
        Assert.Equal(0, result.FullPortions);
        Assert.Equal(0.25m, result.LeftoverPortions);
        Assert.Equal(50m, result.LeftoverCalories);
        Assert.Equal(100m, result.LeftoverGrams);
        Assert.Equal(400m, result.GramsPerPortion);
    }

    [Fact]
    public void Calculate_ReturnsTotalsOnly_WhenIndependentValueMissing()
    {
        var recipe = Recipe(grams: 200, kcalPer100g: 41);

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, null);

        Assert.True(result.HasTotals);
        Assert.False(result.HasPortions);
        Assert.Equal(82m, result.TheoreticalCalories);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Calculate_ReturnsError_WhenWeightIsZero()
    {
        var recipe = new RecipeDto();

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, 250);

        Assert.False(result.HasTotals);
        Assert.False(result.HasPortions);
        Assert.Contains("Dish weight must be greater than zero.", result.Errors);
    }

    [Fact]
    public void Calculate_ReturnsError_WhenCookedWeightIsZero()
    {
        var recipe = Recipe(grams: 200, kcalPer100g: 41);

        var result = _calculator.Calculate(recipe, PortionTargetKind.CaloriesPerPortion, 50, actualDishWeightG: 0);

        Assert.Contains("Cooked weight must be greater than zero.", result.Errors);
        Assert.Equal(200m, result.DishWeightG);
        Assert.Equal(82m, result.TheoreticalCalories);
    }

    private static RecipeDto Recipe(decimal grams, decimal kcalPer100g) =>
        new()
        {
            Name = "Fixture",
            Ingredients = [new IngredientDto { Name = "item", Grams = grams, CaloriesPer100g = kcalPer100g }],
        };
}
