namespace SPC.Core.Models;

/// <summary>One dish on Home: the default row plus any variations that share <see cref="FamilyId"/>.</summary>
public sealed class RecipeFamilyGroup
{
    public required Guid FamilyId { get; init; }

    public required RecipeDto Primary { get; init; }

    /// <summary>Other rows in the family, not including <see cref="Primary"/>.</summary>
    public required IReadOnlyList<RecipeDto> Variants { get; init; }

    /// <summary>Default row first, then other variants.</summary>
    public IReadOnlyList<RecipeDto> AllMembers => [Primary, .. Variants];
}
