using SPC.Core.Models;
using SPC.Core.Services;
using SPC.Core.Validation;
using Xunit;

namespace SPC.Core.Tests.Services;

public class EnergyCalculatorTests
{
    private readonly EnergyCalculator _calculator = new();

    [Fact]
    public void Calculate_MaleModerate_MatchesMifflinFixture()
    {
        var profile = ValidMale();

        var result = _calculator.Calculate(profile, MealType.Lunch);

        Assert.Equal(1780m, result.BmrKcal);
        Assert.Equal(2760m, result.TdeeKcal);
        Assert.Equal(30m, result.MealPercent);
        Assert.Equal(830m, result.MealKcal);
    }

    [Fact]
    public void Calculate_FemaleLight_MatchesMifflinFixture()
    {
        var profile = new UserProfileDto
        {
            Name = "Ada",
            Sex = Sex.Female,
            WeightKg = 65,
            HeightCm = 165,
            AgeYears = 35,
            ActivityLevel = ActivityLevel.Light,
        };

        var result = _calculator.Calculate(profile, MealType.Lunch);

        Assert.Equal(1350m, result.BmrKcal);
        Assert.Equal(1850m, result.TdeeKcal);
        Assert.Equal(560m, result.MealKcal);
    }

    [Fact]
    public void Calculate_CustomLunchPercent_ChangesMealKcalOnly()
    {
        var profile = ValidMale();
        profile.MealSplit.LunchPercent = 40;
        profile.MealSplit.DinnerPercent = 25;

        var result = _calculator.Calculate(profile, MealType.Lunch);

        Assert.Equal(2760m, result.TdeeKcal);
        Assert.Equal(1100m, result.MealKcal);
    }

    [Fact]
    public void PalFor_MapsUsActivityFactors()
    {
        Assert.Equal(1.2m, EnergyCalculator.PalFor(ActivityLevel.Sedentary));
        Assert.Equal(1.55m, EnergyCalculator.PalFor(ActivityLevel.Moderate));
        Assert.Equal(1.9m, EnergyCalculator.PalFor(ActivityLevel.VeryActive));
    }

    [Fact]
    public void PalFor_Profile_UsesCustomIndex()
    {
        var profile = ValidMale();
        profile.ActivityLevel = ActivityLevel.Custom;
        profile.CustomPal = 1.7m;

        Assert.Equal(1.7m, EnergyCalculator.PalFor(profile));
    }

    [Fact]
    public void Estimate_IncludesEveryMeal()
    {
        var result = _calculator.Estimate(ValidMale());

        Assert.Equal(1780m, result.BmrKcal);
        Assert.Equal(1.55m, result.Pal);
        Assert.Equal(2760m, result.TdeeKcal);
        Assert.Equal(550m, result.BreakfastKcal);
        Assert.Equal(830m, result.LunchKcal);
        Assert.Equal(970m, result.DinnerKcal);
        Assert.Equal(410m, result.SnackKcal);
    }

    [Fact]
    public void Estimate_CustomPal_ScalesTdee()
    {
        var profile = ValidMale();
        profile.ActivityLevel = ActivityLevel.Custom;
        profile.CustomPal = 1.7m;

        var result = _calculator.Estimate(profile);

        Assert.Equal(1.7m, result.Pal);
        Assert.Equal(3030m, result.TdeeKcal);
        Assert.Equal(910m, result.LunchKcal);
    }

    [Fact]
    public void Calculate_Throws_WhenProfileInvalid()
    {
        var profile = new UserProfileDto { Name = "x" };

        Assert.Throws<ArgumentException>(() => _calculator.Calculate(profile, MealType.Lunch));
    }

    private static UserProfileDto ValidMale() => new()
    {
        Name = "Jan",
        Sex = Sex.Male,
        WeightKg = 80,
        HeightCm = 180,
        AgeYears = 30,
        ActivityLevel = ActivityLevel.Moderate,
    };
}

public class ProfileValidatorTests
{
    [Fact]
    public void ValidateProfile_AcceptsCompleteProfile()
    {
        var profile = new UserProfileDto
        {
            Name = "Jan",
            Sex = Sex.Male,
            WeightKg = 80,
            HeightCm = 180,
            AgeYears = 30,
        };

        Assert.True(ProfileValidator.IsProfileValid(profile));
    }

    [Fact]
    public void ValidateMealSplit_RequiresOneHundredPercent()
    {
        var split = new MealSplitDto
        {
            BreakfastPercent = 20,
            LunchPercent = 30,
            DinnerPercent = 35,
            SnackPercent = 10,
        };

        var errors = ProfileValidator.ValidateMealSplit(split);

        Assert.Contains(errors, e => e.Contains("100%"));
    }

    [Fact]
    public void ValidateProfile_RequiresCustomPal_WhenActivityIsCustom()
    {
        var profile = new UserProfileDto
        {
            Name = "Jan",
            Sex = Sex.Male,
            WeightKg = 80,
            HeightCm = 180,
            AgeYears = 30,
            ActivityLevel = ActivityLevel.Custom,
        };

        var errors = ProfileValidator.ValidateProfile(profile);

        Assert.Contains(errors, e => e.Contains("custom activity index", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateProfile_AcceptsCustomPalInRange()
    {
        var profile = new UserProfileDto
        {
            Name = "Jan",
            Sex = Sex.Male,
            WeightKg = 80,
            HeightCm = 180,
            AgeYears = 30,
            ActivityLevel = ActivityLevel.Custom,
            CustomPal = 1.65m,
        };

        Assert.True(ProfileValidator.IsProfileValid(profile));
    }
}
