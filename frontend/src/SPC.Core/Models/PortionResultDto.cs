namespace SPC.Core.Models;

/// <summary>Dish totals and portion breakdown. Batch calories never scale with cooked weight.</summary>
public sealed class PortionResultDto
{
    public IReadOnlyList<string> Errors { get; set; } = [];

    public bool HasTotals { get; set; }

    public bool HasPortions { get; set; }

    public decimal TheoreticalWeightG { get; set; }

    public decimal TheoreticalCalories { get; set; }

    /// <summary>Cooked weight if provided, otherwise theoretical weight.</summary>
    public decimal DishWeightG { get; set; }

    public decimal? ActualDishWeightG { get; set; }

    public decimal KcalPer100gCooked { get; set; }

    /// <summary>kcal / 100 g from theoretical weight (ignores cooked yield).</summary>
    public decimal TheoreticalKcalPer100g { get; set; }

    /// <summary>Fractional portion count, e.g. 3.5.</summary>
    public decimal Portions { get; set; }

    public decimal KcalPerPortion { get; set; }

    public decimal GramsPerPortion { get; set; }

    public int FullPortions { get; set; }

    public decimal LeftoverPortions { get; set; }

    public decimal LeftoverCalories { get; set; }

    public decimal LeftoverGrams { get; set; }
}
