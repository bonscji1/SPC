namespace SPC.Core.Models;

public enum Sex
{
    Male,
    Female,
}

public enum ActivityLevel
{
    Sedentary,
    Light,
    Moderate,
    Active,
    VeryActive,
    Custom,
}

/// <summary>Percent of daily calories per meal. Must sum to 100.</summary>
public sealed class MealSplitDto
{
    public decimal BreakfastPercent { get; set; } = 20;

    public decimal LunchPercent { get; set; } = 30;

    public decimal DinnerPercent { get; set; } = 35;

    public decimal SnackPercent { get; set; } = 15;

    public decimal TotalPercent =>
        BreakfastPercent + LunchPercent + DinnerPercent + SnackPercent;

    public decimal PercentFor(MealType meal) => meal switch
    {
        MealType.Breakfast => BreakfastPercent,
        MealType.Lunch => LunchPercent,
        MealType.Dinner => DinnerPercent,
        MealType.Snack => SnackPercent,
        _ => throw new ArgumentOutOfRangeException(nameof(meal), meal, null),
    };
}

/// <summary>A person for calorie targets. Not linked to recipes.</summary>
public sealed class UserProfileDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public Sex? Sex { get; set; }

    public decimal WeightKg { get; set; }

    public decimal HeightCm { get; set; }

    public int AgeYears { get; set; }

    public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Moderate;

    /// <summary>PAL when <see cref="ActivityLevel"/> is Custom.</summary>
    public decimal? CustomPal { get; set; }

    public MealSplitDto MealSplit { get; set; } = new();

    public UserProfileDto Clone() => new()
    {
        Id = Id,
        Name = Name,
        Sex = Sex,
        WeightKg = WeightKg,
        HeightCm = HeightCm,
        AgeYears = AgeYears,
        ActivityLevel = ActivityLevel,
        CustomPal = CustomPal,
        MealSplit = new MealSplitDto
        {
            BreakfastPercent = MealSplit.BreakfastPercent,
            LunchPercent = MealSplit.LunchPercent,
            DinnerPercent = MealSplit.DinnerPercent,
            SnackPercent = MealSplit.SnackPercent,
        },
    };
}

/// <summary>BMR, maintenance TDEE, and kcal for every meal split.</summary>
public sealed class EnergyEstimateDto
{
    public decimal BmrKcal { get; set; }

    public decimal Pal { get; set; }

    public decimal TdeeKcal { get; set; }

    public decimal BreakfastKcal { get; set; }

    public decimal LunchKcal { get; set; }

    public decimal DinnerKcal { get; set; }

    public decimal SnackKcal { get; set; }

    public decimal KcalFor(MealType meal) => meal switch
    {
        MealType.Breakfast => BreakfastKcal,
        MealType.Lunch => LunchKcal,
        MealType.Dinner => DinnerKcal,
        MealType.Snack => SnackKcal,
        _ => throw new ArgumentOutOfRangeException(nameof(meal), meal, null),
    };
}

/// <summary>BMR, TDEE, and one meal budget from a profile.</summary>
public sealed class EnergyTargetDto
{
    public decimal BmrKcal { get; set; }

    public decimal TdeeKcal { get; set; }

    public decimal MealKcal { get; set; }

    public MealType MealType { get; set; }

    public decimal MealPercent { get; set; }
}
