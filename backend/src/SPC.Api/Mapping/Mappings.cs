using SPC.Api.Data.Entities;
using SPC.Core.Models;
using SPC.Core.Services;

namespace SPC.Api.Mapping;

public static class RecipeMapping
{
    public static RecipeDto ToDto(this RecipeEntity entity) => new()
    {
        Id = entity.Id,
        FamilyId = entity.FamilyId == Guid.Empty ? entity.Id : entity.FamilyId,
        VariantLabel = entity.VariantLabel,
        Name = entity.Name,
        MealType = entity.MealType,
        UpdatedAt = entity.UpdatedAt,
        ActualDishWeightG = entity.ActualDishWeightG,
        Ingredients = entity.Ingredients.Select(CloneIngredient).ToList(),
        Spices = entity.Spices.Select(CloneSpice).ToList(),
        Instructions = entity.Instructions.Select(CloneStep).ToList(),
        Notes = CloneStep(entity.Notes),
    };

    public static void Apply(this RecipeEntity entity, RecipeDto dto, Guid accountId)
    {
        entity.Id = dto.Id;
        entity.AccountId = accountId;
        entity.FamilyId = dto.FamilyId == Guid.Empty ? dto.Id : dto.FamilyId;
        entity.VariantLabel = dto.VariantLabel ?? string.Empty;
        entity.Name = dto.Name ?? string.Empty;
        entity.MealType = dto.MealType;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.ActualDishWeightG = dto.ActualDishWeightG;
        entity.Ingredients = dto.Ingredients.Select(CloneIngredient).ToList();
        entity.Spices = dto.Spices.Select(CloneSpice).ToList();
        entity.Instructions = (dto.Instructions ?? []).Select(CloneStep).ToList();
        entity.Notes = CloneStep(dto.Notes);
    }

    private static RecipeIngredientDto CloneIngredient(RecipeIngredientDto item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Grams = item.Grams,
        CaloriesPer100g = item.CaloriesPer100g,
    };

    private static SpiceDto CloneSpice(SpiceDto item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Grams = item.Grams,
        CaloriesPer100g = item.CaloriesPer100g,
    };

    private static InstructionStepDto CloneStep(InstructionStepDto? step)
    {
        var source = step ?? new InstructionStepDto();
        return new InstructionStepDto
        {
            Id = source.Id,
            EditorJson = source.EditorJson,
            Tokens = (source.Tokens ?? []).Select(t => new InstructionTokenDto
            {
                Id = t.Id,
                Kind = t.Kind,
                Text = t.Text,
                ItemId = t.ItemId,
            }).ToList(),
        };
    }
}

public static class IngredientMapping
{
    public static IngredientDto ToDto(this IngredientEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        CaloriesPer100g = entity.CaloriesPer100g,
    };

    public static void Apply(this IngredientEntity entity, IngredientDto dto, Guid accountId)
    {
        entity.Id = dto.Id;
        entity.AccountId = accountId;
        entity.Name = dto.Name.Trim();
        entity.NormalizedName = IngredientLibrary.NormalizeName(dto.Name);
        entity.CaloriesPer100g = dto.CaloriesPer100g;
    }
}

public static class ProfileMapping
{
    public static UserProfileDto ToDto(this ProfileEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Sex = entity.Sex,
        WeightKg = entity.WeightKg,
        HeightCm = entity.HeightCm,
        AgeYears = entity.AgeYears,
        ActivityLevel = entity.ActivityLevel,
        CustomPal = entity.CustomPal,
        MealSplit = CloneSplit(entity.MealSplit),
    };

    public static void Apply(this ProfileEntity entity, UserProfileDto dto, Guid accountId)
    {
        entity.Id = dto.Id;
        entity.AccountId = accountId;
        entity.Name = dto.Name ?? string.Empty;
        entity.Sex = dto.Sex;
        entity.WeightKg = dto.WeightKg;
        entity.HeightCm = dto.HeightCm;
        entity.AgeYears = dto.AgeYears;
        entity.ActivityLevel = dto.ActivityLevel;
        entity.CustomPal = dto.CustomPal;
        entity.MealSplit = CloneSplit(dto.MealSplit);
    }

    private static MealSplitDto CloneSplit(MealSplitDto? split)
    {
        var source = split ?? new MealSplitDto();
        return new MealSplitDto
        {
            BreakfastPercent = source.BreakfastPercent,
            LunchPercent = source.LunchPercent,
            DinnerPercent = source.DinnerPercent,
            SnackPercent = source.SnackPercent,
        };
    }
}
