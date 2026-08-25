namespace SPC.Core.Models;

public sealed class IngredientDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Amount used in the recipe, in grams.</summary>
    public decimal Grams { get; set; }

    /// <summary>Calories per 100 g.</summary>
    public decimal CaloriesPer100g { get; set; }
}
