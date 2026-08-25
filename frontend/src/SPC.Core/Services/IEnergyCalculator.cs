using SPC.Core.Models;

namespace SPC.Core.Services;

/// <summary>Estimates BMR, TDEE, and meal calorie budgets from a profile.</summary>
public interface IEnergyCalculator
{
    EnergyEstimateDto Estimate(UserProfileDto profile);

    EnergyTargetDto Calculate(UserProfileDto profile, MealType meal);
}
