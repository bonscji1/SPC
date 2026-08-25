using SPC.Core.Models;

namespace SPC.Core.Repositories;

public interface IRecipeRepository
{
    Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RecipeSaveResult> SaveAsync(RecipeDto recipe, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
