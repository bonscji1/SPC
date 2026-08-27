using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SPC.Core.Auth;
using SPC.Core.Models;
using SPC.Web.Repositories;
using SPC.Web.Services;

namespace SPC.Web.Auth;

public sealed class AuthService : IAuthService
{
    public const string StorageKey = "spc.auth.v1";

    private readonly HttpClient _http;
    private readonly AuthSession _session;
    private readonly IJSRuntime _js;
    private readonly RecipeDraftService _draft;
    private readonly IIngredientLibraryCache _ingredients;
    private readonly ActiveProfileService _profiles;

    public AuthService(
        HttpClient http,
        AuthSession session,
        IJSRuntime js,
        RecipeDraftService draft,
        IIngredientLibraryCache ingredients,
        ActiveProfileService profiles)
    {
        _http = http;
        _session = session;
        _js = js;
        _draft = draft;
        _ingredients = ingredients;
        _profiles = profiles;
        _session.Changed += OnSessionChanged;
    }

    public async Task<bool> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequest { Username = username.Trim(), Password = password },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Login response was empty.");

        await BeginSessionAsync(body.AccessToken, body.Account, cancellationToken);
        return true;
    }

    public async Task<SignUpStatus> SignUpAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!AccountRules.TryNormalizeUsername(username, out _, out _)
            || !AccountRules.IsPasswordAcceptable(password))
        {
            return SignUpStatus.InvalidInput;
        }

        var response = await _http.PostAsJsonAsync(
            "api/auth/signup",
            new LoginRequest { Username = username.Trim(), Password = password },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return SignUpStatus.UsernameTaken;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return SignUpStatus.InvalidInput;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Sign-up response was empty.");

        await BeginSessionAsync(body.AccessToken, body.Account, cancellationToken);
        return SignUpStatus.Succeeded;
    }

    public async Task LogoutAsync()
    {
        _session.Clear();
        await _js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
    }

    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", cancellationToken, StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        StoredAuth? stored;
        try
        {
            stored = System.Text.Json.JsonSerializer.Deserialize<StoredAuth>(json, JsonOptions);
        }
        catch (System.Text.Json.JsonException)
        {
            await LogoutAsync();
            return;
        }

        if (stored is null
            || string.IsNullOrWhiteSpace(stored.AccessToken)
            || stored.Account is null
            || stored.Account.Id == Guid.Empty)
        {
            await LogoutAsync();
            return;
        }

        _session.Set(stored.AccessToken, stored.Account);
        await LoadAccountDataAsync(cancellationToken);
    }

    private void OnSessionChanged()
    {
        if (_session.IsAuthenticated)
        {
            return;
        }

        _draft.Clear();
        _ingredients.Clear();
        _profiles.Clear();
    }

    private async Task BeginSessionAsync(
        string accessToken,
        AccountDto account,
        CancellationToken cancellationToken)
    {
        _session.Set(accessToken, account);
        var json = System.Text.Json.JsonSerializer.Serialize(
            new StoredAuth { AccessToken = accessToken, Account = account },
            JsonOptions);
        await _js.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, StorageKey, json);

        _draft.Clear();
        _ingredients.Clear();
        _profiles.Clear();
        await LoadAccountDataAsync(cancellationToken);
    }

    private async Task LoadAccountDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _ingredients.HydrateAsync(cancellationToken);
            await _profiles.RefreshAsync(cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            await LogoutAsync();
        }
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private sealed class StoredAuth
    {
        public string AccessToken { get; set; } = string.Empty;

        public AccountDto? Account { get; set; }
    }
}

public sealed class BearerTokenHandler(AuthSession session, IJSRuntime js) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsLogin(request))
        {
            session.Clear();
            try
            {
                await js.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, AuthService.StorageKey);
            }
            catch (JSException)
            {
                // Session already gone or JS unavailable during teardown.
            }
        }

        return response;
    }

    private static bool IsLogin(HttpRequestMessage request) =>
        request.RequestUri?.AbsolutePath.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase) == true;
}

public sealed class SpcAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly AuthSession _session;

    public SpcAuthenticationStateProvider(AuthSession session)
    {
        _session = session;
        _session.Changed += OnSessionChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(Create());

    public void Dispose() => _session.Changed -= OnSessionChanged;

    private void OnSessionChanged() =>
        NotifyAuthenticationStateChanged(Task.FromResult(Create()));

    private AuthenticationState Create()
    {
        if (!_session.IsAuthenticated || _session.Account is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, _session.Account.Id.ToString()),
            new(ClaimTypes.Name, _session.Account.Username),
        ];
        var identity = new ClaimsIdentity(claims, authenticationType: "spc");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
