using SPC.Core.Models;

namespace SPC.Core.Repositories;

public interface IIngredientRepository
{
    Task<IReadOnlyList<IngredientDto>> SearchAsync(string query, CancellationToken cancellationToken = default);

    Task SaveAsync(IngredientDto ingredient, CancellationToken cancellationToken = default);
}
