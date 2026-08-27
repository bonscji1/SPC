namespace SPC.Core.Models;

/// <summary>Public login identity. Not a calorie <see cref="UserProfileDto"/>.</summary>
public sealed class AccountDto
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;
}
