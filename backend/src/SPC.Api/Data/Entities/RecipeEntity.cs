using SPC.Core.Models;

namespace SPC.Api.Data.Entities;

public sealed class RecipeEntity
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public Guid FamilyId { get; set; }

    public string VariantLabel { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public MealType MealType { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public decimal? ActualDishWeightG { get; set; }

    public List<RecipeIngredientDto> Ingredients { get; set; } = [];

    public List<SpiceDto> Spices { get; set; } = [];

    public List<InstructionStepDto> Instructions { get; set; } = [];

    public InstructionStepDto Notes { get; set; } = new();
}
