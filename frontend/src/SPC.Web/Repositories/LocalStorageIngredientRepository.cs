using SPC.Core.Models;
using SPC.Core.Repositories;
using SPC.Core.Services;
using SPC.Web.Services;

namespace SPC.Web.Repositories;

/// <summary>Stopgap nutrition library in browser localStorage. Not linked to recipes.</summary>
public sealed class LocalStorageIngredientRepository(IBrowserLocalStorage storage) : IIngredientRepository
{
    private const string StorageKey = "spc.ingredients.v1";

    private List<IngredientDto>? _cache;

    public async Task<IReadOnlyList<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await LoadAsync(cancellationToken);
        return items.Select(i => i.Clone()).ToList();
    }

    public async Task<IReadOnlyList<IngredientDto>> SearchAsync(
        string query,
        IReadOnlyList<string>? occupiedNames = null,
        CancellationToken cancellationToken = default)
    {
        var items = await LoadAsync(cancellationToken);
        return IngredientLibrary.Search(items, query, occupiedNames: occupiedNames).Select(i => i.Clone()).ToList();
    }

    public async Task<PagedResult<IngredientDto>> GetPageAsync(
        int page,
        int pageSize,
        string? nameQuery = null,
        CancellationToken cancellationToken = default)
    {
        var items = await LoadAsync(cancellationToken);
        var result = IngredientList.Page(items, page, pageSize, nameQuery);
        return new PagedResult<IngredientDto>
        {
            Items = result.Items.Select(i => i.Clone()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
        };
    }

    public async Task SaveAsync(IngredientDto ingredient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        var items = (await LoadAsync(cancellationToken)).Select(i => i.Clone()).ToList();
        var toSave = ingredient.Clone();
        toSave.Name = toSave.Name.Trim();

        var index = items.FindIndex(i => i.Id == toSave.Id);
        if (index < 0)
        {
            var existing = IngredientLibrary.FindExact(items, toSave.Name);
            if (existing is not null)
            {
                toSave.Id = existing.Id;
                index = items.FindIndex(i => i.Id == existing.Id);
            }
        }

        if (index >= 0)
        {
            items[index] = toSave;
        }
        else
        {
            items.Add(toSave);
        }

        await storage.SetItemAsync(StorageKey, items, cancellationToken);
        _cache = items;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var items = (await LoadAsync(cancellationToken))
            .Where(i => i.Id != id)
            .Select(i => i.Clone())
            .ToList();

        await storage.SetItemAsync(StorageKey, items, cancellationToken);
        _cache = items;
    }

    private async Task<List<IngredientDto>> LoadAsync(CancellationToken cancellationToken)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var stored = await storage.GetItemAsync<List<IngredientDto>>(StorageKey, cancellationToken);
        _cache = stored ?? [];
        return _cache;
    }
}
