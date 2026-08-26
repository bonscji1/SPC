namespace SPC.Core.Models;

public enum InstructionTokenKind
{
    Text,
    Ingredient,
    Spice,
}

public sealed class InstructionTokenDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public InstructionTokenKind Kind { get; set; } = InstructionTokenKind.Text;

    public string Text { get; set; } = string.Empty;

    public Guid? ItemId { get; set; }
}

public sealed class InstructionStepDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public List<InstructionTokenDto> Tokens { get; set; } = [new()];

    /// <summary>TipTap document JSON for rich text and mention chips.</summary>
    public string? EditorJson { get; set; }
}

/// <summary>An ingredient or spice that can be inserted as a chip.</summary>
public sealed class InstructionLinkChoice
{
    public Guid ItemId { get; init; }

    public InstructionTokenKind Kind { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;
}

/// <summary>Resolved token for display (editor chips and summary).</summary>
public sealed class InstructionTokenView
{
    public Guid TokenId { get; init; }

    public InstructionTokenKind Kind { get; init; }

    public string Text { get; init; } = string.Empty;

    public string? Hover { get; init; }

    public bool IsMissing { get; init; }
}
