namespace SPC.Core.Models;

public sealed class RecipeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<IngredientDto> Ingredients { get; set; } = [];

    public List<SpiceDto> Spices { get; set; } = [];

    /// <summary>Cooked dish weight in grams, if weighed. Used for yield and future planning.</summary>
    public decimal? ActualDishWeightG { get; set; }
}
