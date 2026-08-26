using SPC.Core.Models;
using SPC.Core.Validation;
using Xunit;

namespace SPC.Core.Tests.Validation;

public class IngredientValidatorTests
{
    [Fact]
    public void Validate_RequiresNameAndPositiveKcal()
    {
        var errors = IngredientValidator.Validate(new IngredientDto());

        Assert.Contains(errors, e => e.Contains("Name", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("kcal", StringComparison.Ordinal));
    }

    [Fact]
    public void IsValid_AcceptsNamedFoodWithKcal()
    {
        var item = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };

        Assert.True(IngredientValidator.IsValid(item));
    }
}
