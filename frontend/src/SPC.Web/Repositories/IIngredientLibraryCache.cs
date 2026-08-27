namespace SPC.Web.Repositories;

public interface IIngredientLibraryCache
{
    Task HydrateAsync(CancellationToken cancellationToken = default);

    void Clear();
}
