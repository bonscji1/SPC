namespace SPC.Core.Auth;

public static class AccountRules
{
    public const int MaxUsernameLength = 128;

    public static bool TryNormalizeUsername(string? username, out string display, out string normalized)
    {
        display = username?.Trim() ?? string.Empty;
        normalized = display.ToLowerInvariant();
        return display.Length is > 0 and <= MaxUsernameLength;
    }

    public static bool IsPasswordAcceptable(string? password) =>
        !string.IsNullOrEmpty(password);
}
