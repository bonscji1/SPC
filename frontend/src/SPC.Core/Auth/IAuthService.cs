namespace SPC.Core.Auth;

/// <summary>Login and sign-up against the API. Does not own recipes, library, or calorie profiles.</summary>
public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<SignUpStatus> SignUpAsync(string username, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync();

    Task RestoreAsync(CancellationToken cancellationToken = default);
}
