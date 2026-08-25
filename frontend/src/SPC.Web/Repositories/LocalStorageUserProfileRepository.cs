using SPC.Core.Models;
using SPC.Core.Repositories;
using SPC.Web.Services;

namespace SPC.Web.Repositories;

/// <summary>Stopgap persistence in browser localStorage. Not linked to recipes.</summary>
public sealed class LocalStorageUserProfileRepository(IBrowserLocalStorage storage) : IUserProfileRepository
{
    private const string StorageKey = "spc.profiles.v1";

    public async Task<IReadOnlyList<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await storage.GetItemAsync<List<UserProfileDto>>(StorageKey, cancellationToken);
        return profiles ?? [];
    }

    public async Task<UserProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profiles = await GetAllAsync(cancellationToken);
        return profiles.FirstOrDefault(p => p.Id == id);
    }

    public async Task SaveAsync(UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var profiles = (await GetAllAsync(cancellationToken)).Select(p => p.Clone()).ToList();
        var toSave = profile.Clone();
        var index = profiles.FindIndex(p => p.Id == toSave.Id);
        if (index >= 0)
        {
            profiles[index] = toSave;
        }
        else
        {
            profiles.Add(toSave);
        }

        await storage.SetItemAsync(StorageKey, profiles, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profiles = (await GetAllAsync(cancellationToken))
            .Where(p => p.Id != id)
            .Select(p => p.Clone())
            .ToList();

        await storage.SetItemAsync(StorageKey, profiles, cancellationToken);
    }
}
