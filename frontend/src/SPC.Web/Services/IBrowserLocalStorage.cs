namespace SPC.Web.Services;

/// <summary>
/// Browser key-value storage. Implementations live in SPC.Web only.
/// Swap for an HTTP-backed store when the backend arrives.
/// </summary>
public interface IBrowserLocalStorage
{
    Task<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    Task RemoveItemAsync(string key, CancellationToken cancellationToken = default);
}
