using SPC.Core.Models;

namespace SPC.Core.Auth;

/// <summary>In-memory login session. HttpClient reads the token from here.</summary>
public sealed class AuthSession
{
    public string? AccessToken { get; private set; }

    public AccountDto? Account { get; private set; }

    public bool IsAuthenticated =>
        Account is not null && !string.IsNullOrEmpty(AccessToken);

    public event Action? Changed;

    public void Set(string accessToken, AccountDto account)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentNullException.ThrowIfNull(account);

        AccessToken = accessToken;
        Account = account;
        Changed?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        Account = null;
        Changed?.Invoke();
    }
}
