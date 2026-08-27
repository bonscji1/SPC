namespace SPC.Api.Auth;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "spc";

    public string Audience { get; set; } = "spc";

    public int ExpiryHours { get; set; } = 8;
}
