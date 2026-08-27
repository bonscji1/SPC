using System.Net;
using System.Net.Http.Json;
using SPC.Core.Models;
using SPC.Core.Repositories;

namespace SPC.Web.Repositories;

public sealed class ApiUserProfileRepository(HttpClient http) : IUserProfileRepository
{
    public async Task<IReadOnlyList<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await http.GetFromJsonAsync<List<UserProfileDto>>("api/profiles", cancellationToken);
        return profiles ?? [];
    }

    public async Task<UserProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"api/profiles/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken);
    }

    public async Task SaveAsync(UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        using var response = await http.PutAsJsonAsync("api/profiles", profile, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/profiles/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
