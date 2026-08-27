using SPC.Core.Models;

namespace SPC.Api.Data.Entities;

public sealed class ProfileEntity
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Sex? Sex { get; set; }

    public decimal WeightKg { get; set; }

    public decimal HeightCm { get; set; }

    public int AgeYears { get; set; }

    public ActivityLevel ActivityLevel { get; set; }

    public decimal? CustomPal { get; set; }

    public MealSplitDto MealSplit { get; set; } = new();
}
