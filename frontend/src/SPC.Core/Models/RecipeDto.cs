namespace SPC.Core.Models;

public sealed class RecipeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Which meal this dish is for. Drives the profile kcal suggestion; default lunch.</summary>
    public MealType MealType { get; set; } = MealType.Lunch;

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<RecipeIngredientDto> Ingredients { get; set; } = [];

    public List<SpiceDto> Spices { get; set; } = [];

    public List<InstructionStepDto> Instructions { get; set; } = [];

    /// <summary>Cooked dish weight in grams, if weighed. Used for yield and future planning.</summary>
    public decimal? ActualDishWeightG { get; set; }
}
