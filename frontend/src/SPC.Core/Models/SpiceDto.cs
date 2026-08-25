namespace SPC.Core.Models;

public sealed class SpiceDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Amount in grams. Optional for spices.</summary>
    public decimal? Grams { get; set; }

    /// <summary>Calories per 100 g. Optional for spices.</summary>
    public decimal? CaloriesPer100g { get; set; }
}
