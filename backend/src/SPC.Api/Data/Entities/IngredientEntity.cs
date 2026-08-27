namespace SPC.Api.Data.Entities;

public sealed class IngredientEntity
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public decimal CaloriesPer100g { get; set; }
}
