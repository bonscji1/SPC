using SPC.Core.Models;
using SPC.Core.Repositories;
using SPC.Web.Services;

namespace SPC.Web.Repositories;

/// <summary>
/// Stopgap persistence in browser localStorage. Replace with ApiRecipeRepository for backend.
/// </summary>
public sealed class LocalStorageRecipeRepository(IBrowserLocalStorage storage) : IRecipeRepository
{
    private const string StorageKey = "spc.recipes.v1";

    public async Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var recipes = await storage.GetItemAsync<List<RecipeDto>>(StorageKey, cancellationToken);
        return recipes ?? [];
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recipes = await GetAllAsync(cancellationToken);
        return recipes.FirstOrDefault(r => r.Id == id);
    }

    public async Task<RecipeSaveResult> SaveAsync(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        try
        {
            var recipes = (await GetAllAsync(cancellationToken)).Select(r => r.Clone()).ToList();
            var toSave = recipe.Clone();
            toSave.UpdatedAt = DateTimeOffset.UtcNow;

            var index = recipes.FindIndex(r => r.Id == toSave.Id);
            if (index >= 0)
            {
                recipes[index] = toSave;
            }
            else
            {
                recipes.Add(toSave);
            }

            await storage.SetItemAsync(StorageKey, recipes, cancellationToken);
            return RecipeSaveResult.Succeeded("Recipe saved successfully.");
        }
        catch (Exception ex)
        {
            return RecipeSaveResult.Failed($"Could not save recipe: {ex.Message}");
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recipes = (await GetAllAsync(cancellationToken))
            .Where(r => r.Id != id)
            .Select(r => r.Clone())
            .ToList();

        await storage.SetItemAsync(StorageKey, recipes, cancellationToken);
    }
}
