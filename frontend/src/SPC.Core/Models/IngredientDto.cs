namespace SPC.Core.Models;

/// <summary>Reusable nutrition library entry. Not tied to a recipe amount.</summary>
public sealed class IngredientDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Calories per 100 g.</summary>
    public decimal CaloriesPer100g { get; set; }

    public IngredientDto Clone() => new()
    {
        Id = Id,
        Name = Name,
        CaloriesPer100g = CaloriesPer100g,
    };
}
