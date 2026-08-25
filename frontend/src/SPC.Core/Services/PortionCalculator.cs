using SPC.Core.Models;
using SPC.Core.Validation;

namespace SPC.Core.Services;

/// <summary>
/// Ingredient-sum + recipe yield. Batch calories stay theoretical;
/// cooked weight only changes grams per portion and cooked density.
/// </summary>
public sealed class PortionCalculator : IPortionCalculator
{
    public PortionResultDto Calculate(
        RecipeDto recipe,
        PortionTargetKind independentKind,
        decimal? independentValue,
        decimal? actualDishWeightG = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var errors = new List<string>();
        var theoreticalWeightG = RecipeValidator.GetTotalGrams(recipe);
        var theoreticalCalories = RecipeValidator.GetTotalCalories(recipe);

        var result = new PortionResultDto
        {
            TheoreticalWeightG = theoreticalWeightG,
            TheoreticalCalories = theoreticalCalories,
        };

        if (theoreticalWeightG <= 0)
        {
            errors.Add("Dish weight must be greater than zero.");
            result.Errors = errors;
            return result;
        }

        decimal dishWeightG;
        if (actualDishWeightG is null)
        {
            dishWeightG = theoreticalWeightG;
        }
        else if (actualDishWeightG <= 0)
        {
            errors.Add("Cooked weight must be greater than zero.");
            dishWeightG = theoreticalWeightG;
        }
        else
        {
            dishWeightG = actualDishWeightG.Value;
            result.ActualDishWeightG = actualDishWeightG;
        }

        result.HasTotals = true;
        result.DishWeightG = dishWeightG;
        result.TheoreticalKcalPer100g = theoreticalCalories / theoreticalWeightG * 100m;
        result.KcalPer100gCooked = theoreticalCalories / dishWeightG * 100m;

        if (independentValue is not > 0)
        {
            result.Errors = errors;
            return result;
        }

        decimal portions;
        decimal kcalPerPortion;
        decimal gramsPerPortion;

        switch (independentKind)
        {
            case PortionTargetKind.CaloriesPerPortion:
                kcalPerPortion = independentValue.Value;
                portions = theoreticalCalories / kcalPerPortion;
                gramsPerPortion = portions > 0 ? dishWeightG / portions : 0;
                break;
            case PortionTargetKind.Portions:
                portions = independentValue.Value;
                kcalPerPortion = theoreticalCalories / portions;
                gramsPerPortion = dishWeightG / portions;
                break;
            case PortionTargetKind.GramsPerPortion:
                gramsPerPortion = independentValue.Value;
                portions = dishWeightG / gramsPerPortion;
                kcalPerPortion = portions > 0 ? theoreticalCalories / portions : 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(independentKind), independentKind, null);
        }

        result.Portions = portions;
        result.KcalPerPortion = kcalPerPortion;
        result.GramsPerPortion = gramsPerPortion;
        result.HasPortions = portions > 0;

        if (result.HasPortions)
        {
            result.FullPortions = (int)decimal.Floor(portions);
            result.LeftoverPortions = portions - result.FullPortions;
            result.LeftoverCalories = theoreticalCalories - (result.FullPortions * kcalPerPortion);
            result.LeftoverGrams = dishWeightG - (result.FullPortions * gramsPerPortion);
        }

        result.Errors = errors;
        return result;
    }
}
