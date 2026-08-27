using System.Net;
using System.Net.Http.Json;
using SPC.Core.Models;
using SPC.Core.Repositories;

namespace SPC.Web.Repositories;

public sealed class ApiRecipeRepository(HttpClient http) : IRecipeRepository
{
    public async Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = new List<RecipeDto>();
        var page = 1;
        PagedResult<RecipeFamilyGroup> result;
        do
        {
            result = await GetPageAsync(page, 50, cancellationToken: cancellationToken);
            foreach (var family in result.Items)
            {
                all.AddRange(family.AllMembers);
            }

            page++;
        } while (result.HasNext);

        return all;
    }

    public async Task<PagedResult<RecipeFamilyGroup>> GetPageAsync(
        int page,
        int pageSize,
        MealType? mealType = null,
        string? nameQuery = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/recipes?page={page}&pageSize={pageSize}";
        if (mealType is { } meal)
        {
            url += $"&mealType={meal}";
        }

        if (!string.IsNullOrWhiteSpace(nameQuery))
        {
            url += $"&nameQuery={Uri.EscapeDataString(nameQuery)}";
        }

        using var response = await http.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new PagedResult<RecipeFamilyGroup>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 1,
            };
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RecipeFamilyGroup>>(cancellationToken);
        return result ?? new PagedResult<RecipeFamilyGroup>
        {
            Items = [],
            Page = page,
            PageSize = pageSize,
            TotalCount = 0,
            TotalPages = 1,
        };
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"api/recipes/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RecipeDto>(cancellationToken);
    }

    public async Task<IReadOnlyList<RecipeDto>> GetByFamilyIdAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        var members = await http.GetFromJsonAsync<List<RecipeDto>>(
            $"api/recipes/families/{familyId}",
            cancellationToken);
        return members ?? [];
    }

    public async Task<RecipeSaveResult> SaveAsync(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        try
        {
            using var response = await http.PutAsJsonAsync("api/recipes", recipe, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RecipeSaveResult>(cancellationToken)
                ?? RecipeSaveResult.Succeeded("Recipe saved successfully.");
        }
        catch (Exception ex)
        {
            return RecipeSaveResult.Failed($"Could not save recipe: {ex.Message}");
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/recipes/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteFamilyAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/recipes/families/{familyId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
