using SPC.Core.Models;
using SPC.Core.Validation;

namespace SPC.Core.Services;

/// <summary>
/// Mifflin–St Jeor BMR × PAL (preset or custom). Meal kcal = TDEE × profile meal percent.
/// </summary>
public sealed class EnergyCalculator : IEnergyCalculator
{
    public const decimal PalSedentary = 1.4m;
    public const decimal PalLight = 1.5m;
    public const decimal PalModerate = 1.6m;
    public const decimal PalActive = 1.8m;
    public const decimal PalVeryActive = 2.0m;

    public EnergyEstimateDto Estimate(UserProfileDto profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!ProfileValidator.IsProfileValid(profile))
        {
            throw new ArgumentException("Profile is not valid.", nameof(profile));
        }

        var bmr = ComputeBmr(profile);
        var pal = PalFor(profile);
        var tdee = RoundToTen(bmr * pal);

        return new EnergyEstimateDto
        {
            BmrKcal = RoundToTen(bmr),
            Pal = pal,
            TdeeKcal = tdee,
            BreakfastKcal = MealKcal(tdee, profile.MealSplit.BreakfastPercent),
            LunchKcal = MealKcal(tdee, profile.MealSplit.LunchPercent),
            DinnerKcal = MealKcal(tdee, profile.MealSplit.DinnerPercent),
            SnackKcal = MealKcal(tdee, profile.MealSplit.SnackPercent),
        };
    }

    public EnergyTargetDto Calculate(UserProfileDto profile, MealType meal)
    {
        var estimate = Estimate(profile);
        return new EnergyTargetDto
        {
            BmrKcal = estimate.BmrKcal,
            TdeeKcal = estimate.TdeeKcal,
            MealKcal = estimate.KcalFor(meal),
            MealType = meal,
            MealPercent = profile.MealSplit.PercentFor(meal),
        };
    }

    public static decimal PalFor(UserProfileDto profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.ActivityLevel == ActivityLevel.Custom)
        {
            return profile.CustomPal
                ?? throw new ArgumentException("Custom activity index is required.", nameof(profile));
        }

        return PalFor(profile.ActivityLevel);
    }

    public static decimal PalFor(ActivityLevel activity) => activity switch
    {
        ActivityLevel.Sedentary => PalSedentary,
        ActivityLevel.Light => PalLight,
        ActivityLevel.Moderate => PalModerate,
        ActivityLevel.Active => PalActive,
        ActivityLevel.VeryActive => PalVeryActive,
        ActivityLevel.Custom => throw new ArgumentException("Use PalFor(UserProfileDto) for a custom index.", nameof(activity)),
        _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, null),
    };

    public static decimal ComputeBmr(UserProfileDto profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var sexOffset = profile.Sex switch
        {
            Sex.Male => 5m,
            Sex.Female => -161m,
            _ => throw new ArgumentException("Sex is required.", nameof(profile)),
        };

        return 10m * profile.WeightKg
            + 6.25m * profile.HeightCm
            - 5m * profile.AgeYears
            + sexOffset;
    }

    public static decimal RoundToTen(decimal kcal) =>
        decimal.Round(kcal / 10m, 0, MidpointRounding.AwayFromZero) * 10m;

    private static decimal MealKcal(decimal tdee, decimal percent) =>
        RoundToTen(tdee * percent / 100m);
}
