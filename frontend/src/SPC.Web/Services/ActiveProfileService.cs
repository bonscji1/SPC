using SPC.Core.Models;
using SPC.Core.Repositories;

namespace SPC.Web.Services;

/// <summary>
/// Which profile is selected in the UI. Independent of recipes.
/// </summary>
public sealed class ActiveProfileService(
    IUserProfileRepository profiles,
    IBrowserLocalStorage storage)
{
    private const string ActiveIdKey = "spc.activeProfileId.v1";

    public IReadOnlyList<UserProfileDto> All { get; private set; } = [];

    public UserProfileDto? Active { get; private set; }

    public event Action? Changed;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        All = await profiles.GetAllAsync(cancellationToken);

        var stored = await storage.GetItemAsync<string>(ActiveIdKey, cancellationToken);
        Guid? activeId = Guid.TryParse(stored, out var parsed) ? parsed : null;
        Active = activeId is Guid id ? All.FirstOrDefault(p => p.Id == id) : null;

        Changed?.Invoke();
    }

    public async Task SelectAsync(Guid? id, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        if (id is Guid selected)
        {
            Active = All.FirstOrDefault(p => p.Id == selected);
            if (Active is not null)
            {
                await storage.SetItemAsync(ActiveIdKey, selected.ToString(), cancellationToken);
            }
            else
            {
                await storage.RemoveItemAsync(ActiveIdKey, cancellationToken);
            }
        }
        else
        {
            Active = null;
            await storage.RemoveItemAsync(ActiveIdKey, cancellationToken);
        }

        Changed?.Invoke();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default) =>
        await InitializeAsync(cancellationToken);
}
