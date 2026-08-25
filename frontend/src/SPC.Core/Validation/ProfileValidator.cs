using SPC.Core.Formatting;
using SPC.Core.Models;

namespace SPC.Core.Validation;

public static class ProfileValidator
{
    public const int MinAgeYears = 18;
    public const int MaxAgeYears = 80;
    public const decimal MinWeightKg = 30;
    public const decimal MaxWeightKg = 250;
    public const decimal MinHeightCm = 120;
    public const decimal MaxHeightCm = 220;
    public const decimal MinCustomPal = 1.0m;
    public const decimal MaxCustomPal = 2.4m;

    public static IReadOnlyList<string> ValidateProfile(UserProfileDto profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            errors.Add("Profile name is required.");
        }

        if (profile.Sex is null)
        {
            errors.Add("Sex is required for the calorie estimate.");
        }

        if (profile.AgeYears < MinAgeYears || profile.AgeYears > MaxAgeYears)
        {
            errors.Add($"Age must be between {MinAgeYears} and {MaxAgeYears}.");
        }

        if (profile.WeightKg < MinWeightKg || profile.WeightKg > MaxWeightKg)
        {
            errors.Add($"Weight must be between {MinWeightKg:0} and {MaxWeightKg:0} kg.");
        }

        if (profile.HeightCm < MinHeightCm || profile.HeightCm > MaxHeightCm)
        {
            errors.Add($"Height must be between {MinHeightCm:0} and {MaxHeightCm:0} cm.");
        }

        if (profile.ActivityLevel == ActivityLevel.Custom)
        {
            if (profile.CustomPal is null)
            {
                errors.Add("Enter a custom activity index.");
            }
            else if (profile.CustomPal < MinCustomPal || profile.CustomPal > MaxCustomPal)
            {
                errors.Add($"Custom activity index must be between {NumberFormat.Format(MinCustomPal)} and {NumberFormat.Format(MaxCustomPal)}.");
            }
        }

        errors.AddRange(ValidateMealSplit(profile.MealSplit));
        return errors;
    }

    public static IReadOnlyList<string> ValidateMealSplit(MealSplitDto split)
    {
        ArgumentNullException.ThrowIfNull(split);

        var errors = new List<string>();

        if (split.BreakfastPercent < 0 || split.LunchPercent < 0
            || split.DinnerPercent < 0 || split.SnackPercent < 0)
        {
            errors.Add("Meal percents cannot be negative.");
        }

        if (decimal.Abs(split.TotalPercent - 100m) > 0.05m)
        {
            errors.Add($"Meal percents must add up to 100% (currently {NumberFormat.Format(split.TotalPercent)}%).");
        }

        return errors;
    }

    public static bool IsProfileValid(UserProfileDto profile) => ValidateProfile(profile).Count == 0;
}
