using System.Net;
using System.Net.Http.Json;
using SPC.Core.Models;
using SPC.Core.Repositories;
using SPC.Core.Services;

namespace SPC.Web.Repositories;

/// <summary>
/// Account library in memory after login. Search and paging stay local; writes go to the API.
/// </summary>
public sealed class CachedIngredientRepository(HttpClient http) : IIngredientRepository, IIngredientLibraryCache
{
    private List<IngredientDto> _items = [];
    private bool _hydrated;

    public async Task HydrateAsync(CancellationToken cancellationToken = default)
    {
        var items = await http.GetFromJsonAsync<List<IngredientDto>>("api/ingredients", cancellationToken);
        _items = items ?? [];
        _hydrated = true;
    }

    public void Clear()
    {
        _items = [];
        _hydrated = false;
    }

    public async Task<IReadOnlyList<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureHydratedAsync(cancellationToken);
        return _items.Select(i => i.Clone()).ToList();
    }

    public async Task<IReadOnlyList<IngredientDto>> SearchAsync(
        string query,
        IReadOnlyList<string>? occupiedNames = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureHydratedAsync(cancellationToken);
        return IngredientLibrary.Search(_items, query, occupiedNames: occupiedNames)
            .Select(i => i.Clone())
            .ToList();
    }

    public async Task<PagedResult<IngredientDto>> GetPageAsync(
        int page,
        int pageSize,
        string? nameQuery = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureHydratedAsync(cancellationToken);
        var result = IngredientList.Page(_items, page, pageSize, nameQuery);
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

        using var response = await http.PutAsJsonAsync("api/ingredients", ingredient, cancellationToken);
        response.EnsureSuccessStatusCode();
        var saved = await response.Content.ReadFromJsonAsync<IngredientDto>(cancellationToken)
            ?? ingredient.Clone();

        await EnsureHydratedAsync(cancellationToken);
        var index = _items.FindIndex(i => i.Id == saved.Id);
        if (index >= 0)
        {
            _items[index] = saved.Clone();
        }
        else
        {
            _items.Add(saved.Clone());
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/ingredients/{id}", cancellationToken);
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }

        _items.RemoveAll(i => i.Id == id);
    }

    private async Task EnsureHydratedAsync(CancellationToken cancellationToken)
    {
        if (!_hydrated)
        {
            await HydrateAsync(cancellationToken);
        }
    }
}
