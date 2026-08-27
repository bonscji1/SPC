using SPC.Core.Models;
using SPC.Core.Repositories;

namespace SPC.Web.Services;

/// <summary>
/// Which calorie profile is selected in the UI. Independent of the login account.
/// In-memory only; the list comes from the API for the signed-in account.
/// </summary>
public sealed class ActiveProfileService(IUserProfileRepository profiles)
{
    public IReadOnlyList<UserProfileDto> All { get; private set; } = [];

    public UserProfileDto? Active { get; private set; }

    public event Action? Changed;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var previousId = Active?.Id;
        All = await profiles.GetAllAsync(cancellationToken);
        Active = previousId is Guid id ? All.FirstOrDefault(p => p.Id == id) : null;
        Changed?.Invoke();
    }

    public async Task SelectAsync(Guid? id, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        Active = id is Guid selected ? All.FirstOrDefault(p => p.Id == selected) : null;
        Changed?.Invoke();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default) =>
        await InitializeAsync(cancellationToken);

    public void Clear()
    {
        All = [];
        Active = null;
        Changed?.Invoke();
    }
}
