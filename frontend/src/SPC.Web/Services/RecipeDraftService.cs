using SPC.Core.Models;

namespace SPC.Web.Services;

/// <summary>In-memory recipe being edited. Persist via <see cref="SPC.Core.Repositories.IRecipeRepository"/>.</summary>
public sealed class RecipeDraftService
{
    private RecipeDto _baseline = new();

    public RecipeDto Recipe { get; private set; } = new();

    /// <summary>Session-only. Last edit of kcal, portion count, or grams per portion is independent.</summary>
    public PortionTargetKind PortionTargetKind { get; set; } = PortionTargetKind.CaloriesPerPortion;

    public decimal? PortionTargetValue { get; set; }

    public bool HasUnsavedChanges => !RecipeEquivalence.AreEquivalent(Recipe, _baseline);

    public void Load(RecipeDto recipe)
    {
        Recipe = recipe.Clone();
        ResetPortionSession();
        MarkClean();
    }

    public void NewRecipe()
    {
        Recipe = new RecipeDto();
        ResetPortionSession();
    }

    public void ResetPortionSession()
    {
        PortionTargetKind = PortionTargetKind.CaloriesPerPortion;
        PortionTargetValue = null;
    }

    public void MarkClean() => _baseline = Recipe.Clone();

    public void AddIngredient() => Recipe.Ingredients.Add(new IngredientDto());

    public void RemoveIngredient(Guid id) => Recipe.Ingredients.RemoveAll(i => i.Id == id);

    public void AddSpice() => Recipe.Spices.Add(new SpiceDto());

    public void RemoveSpice(Guid id) => Recipe.Spices.RemoveAll(s => s.Id == id);
}
