namespace SPC.Web.Components;

public sealed class RecipeItemRowChange
{
    public string Name { get; init; } = string.Empty;

    public decimal? Grams { get; init; }

    public decimal? CaloriesPer100g { get; init; }
}
