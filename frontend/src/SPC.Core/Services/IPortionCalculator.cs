using SPC.Core.Models;

namespace SPC.Core.Services;

/// <summary>Computes dish totals and portion sizes (ingredient-sum + yield).</summary>
public interface IPortionCalculator
{
    PortionResultDto Calculate(
        RecipeDto recipe,
        PortionTargetKind independentKind,
        decimal? independentValue,
        decimal? actualDishWeightG = null);
}
