using System.Text.Json;
using SPC.Core.Formatting;
using SPC.Core.Models;

namespace SPC.Core.Services;

/// <summary>
/// Chip-based instruction steps: text plus links to ingredient/spice ids.
/// </summary>
public static class InstructionEditor
{
    public static InstructionStepDto NewStep() => new()
    {
        Tokens = [new InstructionTokenDto()],
    };

    public static bool StepHasContent(InstructionStepDto step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.Tokens.Any(t =>
            t.Kind != InstructionTokenKind.Text
            || !string.IsNullOrWhiteSpace(t.Text));
    }

    public static IReadOnlyList<InstructionLinkChoice> AvailableLinks(RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var links = new List<InstructionLinkChoice>();

        foreach (var ingredient in recipe.Ingredients)
        {
            links.Add(new InstructionLinkChoice
            {
                ItemId = ingredient.Id,
                Kind = InstructionTokenKind.Ingredient,
                Name = DisplayName(ingredient.Name, "Ingredient"),
                Detail = NumberFormat.WithUnit(ingredient.Grams, "g"),
            });
        }

        foreach (var spice in recipe.Spices)
        {
            var detail = spice.Grams is decimal grams
                ? NumberFormat.WithUnit(grams, "g")
                : "no amount set";

            links.Add(new InstructionLinkChoice
            {
                ItemId = spice.Id,
                Kind = InstructionTokenKind.Spice,
                Name = DisplayName(spice.Name, "Spice"),
                Detail = detail,
            });
        }

        return links;
    }

    public static IReadOnlyList<InstructionLinkChoice> FilterLinks(
        IEnumerable<InstructionLinkChoice> links,
        string? query)
    {
        ArgumentNullException.ThrowIfNull(links);

        if (string.IsNullOrWhiteSpace(query))
        {
            return links.ToList();
        }

        var needle = query.Trim();
        return links
            .Where(link => link.Name.Contains(needle, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static IReadOnlyList<InstructionTokenView> Resolve(InstructionStepDto step, RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(recipe);

        return step.Tokens.Select(token => ResolveToken(token, recipe)).ToList();
    }

    public static Guid InsertLink(
        InstructionStepDto step,
        Guid textTokenId,
        int caret,
        InstructionTokenKind kind,
        Guid itemId)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (kind is not InstructionTokenKind.Ingredient and not InstructionTokenKind.Spice)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Link must be an ingredient or spice.");
        }

        EnsureTextEnds(step);

        var index = step.Tokens.FindIndex(t => t.Id == textTokenId);
        if (index < 0 || step.Tokens[index].Kind != InstructionTokenKind.Text)
        {
            throw new ArgumentException("Text token not found.", nameof(textTokenId));
        }

        var text = step.Tokens[index].Text ?? string.Empty;
        caret = Math.Clamp(caret, 0, text.Length);
        var before = text[..caret];
        var after = text[caret..];

        var afterToken = new InstructionTokenDto { Kind = InstructionTokenKind.Text, Text = after };
        var linkToken = new InstructionTokenDto { Kind = kind, ItemId = itemId };

        step.Tokens[index].Text = before;
        step.Tokens.Insert(index + 1, linkToken);
        step.Tokens.Insert(index + 2, afterToken);
        return afterToken.Id;
    }

    public static Guid? RemoveToken(InstructionStepDto step, Guid tokenId)
    {
        ArgumentNullException.ThrowIfNull(step);

        var index = step.Tokens.FindIndex(t => t.Id == tokenId);
        if (index < 0)
        {
            return null;
        }

        step.Tokens.RemoveAt(index);
        MergeAdjacentText(step);
        EnsureTextEnds(step);

        if (index > 0 && index - 1 < step.Tokens.Count)
        {
            return step.Tokens[index - 1].Id;
        }

        return step.Tokens.FirstOrDefault()?.Id;
    }

    public static void SetText(InstructionStepDto step, Guid textTokenId, string text)
    {
        ArgumentNullException.ThrowIfNull(step);

        var token = step.Tokens.FirstOrDefault(t => t.Id == textTokenId)
            ?? throw new ArgumentException("Text token not found.", nameof(textTokenId));

        if (token.Kind != InstructionTokenKind.Text)
        {
            throw new ArgumentException("Token is not text.", nameof(textTokenId));
        }

        token.Text = text ?? string.Empty;
    }

    public static string SerializeStepToTiptap(InstructionStepDto step, RecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(recipe);

        if (!string.IsNullOrWhiteSpace(step.EditorJson))
        {
            return step.EditorJson;
        }

        var paragraphContent = new List<object>();

        foreach (var token in step.Tokens)
        {
            if (token.Kind == InstructionTokenKind.Text)
            {
                if (!string.IsNullOrEmpty(token.Text))
                {
                    paragraphContent.Add(new { type = "text", text = token.Text });
                }

                continue;
            }

            if (token.Kind is InstructionTokenKind.Ingredient or InstructionTokenKind.Spice)
            {
                var view = ResolveToken(token, recipe);
                paragraphContent.Add(new
                {
                    type = "ingredientMention",
                    attrs = new
                    {
                        id = token.ItemId?.ToString() ?? string.Empty,
                        label = view.Text,
                        kind = token.Kind == InstructionTokenKind.Ingredient ? "ingredient" : "spice",
                        detail = DetailFromHover(view.Hover),
                    },
                });
            }
        }

        if (paragraphContent.Count == 0)
        {
            paragraphContent.Add(new { type = "text", text = string.Empty });
        }

        var doc = new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    content = paragraphContent.ToArray(),
                },
            },
        };

        return JsonSerializer.Serialize(doc);
    }

    public static void ApplyTiptapJson(InstructionStepDto step, string json)
    {
        ArgumentNullException.ThrowIfNull(step);

        step.EditorJson = json;
        step.Tokens.Clear();

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.GetProperty("type").GetString() != "doc"
            || !root.TryGetProperty("content", out var blocks))
        {
            step.Tokens.Add(new InstructionTokenDto());
            return;
        }

        var firstParagraph = true;
        foreach (var block in blocks.EnumerateArray())
        {
            var blockType = block.GetProperty("type").GetString();
            if (blockType == "bulletList" || blockType == "orderedList")
            {
                AppendListText(step, block, firstParagraph);
                firstParagraph = false;
                continue;
            }

            if (blockType != "paragraph")
            {
                continue;
            }

            if (!firstParagraph)
            {
                AppendTextToken(step, "\n");
            }

            firstParagraph = false;

            if (!block.TryGetProperty("content", out var inlines))
            {
                continue;
            }

            foreach (var inline in inlines.EnumerateArray())
            {
                AppendInline(step, inline);
            }
        }

        if (step.Tokens.Count == 0)
        {
            step.Tokens.Add(new InstructionTokenDto());
        }

        MergeAdjacentText(step);
        EnsureTextEnds(step);
    }

    public static IReadOnlyList<object> MentionItemsForRecipe(RecipeDto recipe)
    {
        return AvailableLinks(recipe)
            .Select(link => (object)new
            {
                id = link.ItemId.ToString(),
                name = link.Name,
                kind = link.Kind == InstructionTokenKind.Ingredient ? "ingredient" : "spice",
                detail = link.Detail,
            })
            .ToList();
    }

    private static void AppendInline(InstructionStepDto step, JsonElement inline)
    {
        var type = inline.GetProperty("type").GetString();
        if (type == "text")
        {
            AppendTextToken(step, inline.GetProperty("text").GetString() ?? string.Empty);
            return;
        }

        if (type == "ingredientMention" && inline.TryGetProperty("attrs", out var attrs))
        {
            var idText = attrs.GetProperty("id").GetString();
            if (!Guid.TryParse(idText, out var itemId))
            {
                return;
            }

            var kindText = attrs.GetProperty("kind").GetString();
            var kind = kindText == "spice"
                ? InstructionTokenKind.Spice
                : InstructionTokenKind.Ingredient;

            step.Tokens.Add(new InstructionTokenDto
            {
                Kind = kind,
                ItemId = itemId,
            });
        }
    }

    private static void AppendListText(InstructionStepDto step, JsonElement list, bool firstParagraph)
    {
        if (!list.TryGetProperty("content", out var items))
        {
            return;
        }

        var index = 0;
        foreach (var item in items.EnumerateArray())
        {
            if (item.GetProperty("type").GetString() != "listItem"
                || !item.TryGetProperty("content", out var itemBlocks))
            {
                continue;
            }

            if (!firstParagraph || index > 0)
            {
                AppendTextToken(step, index == 0 && !firstParagraph ? "\n" : "\n• ");
            }
            else
            {
                AppendTextToken(step, "• ");
            }

            foreach (var block in itemBlocks.EnumerateArray())
            {
                if (block.GetProperty("type").GetString() != "paragraph"
                    || !block.TryGetProperty("content", out var inlines))
                {
                    continue;
                }

                foreach (var inline in inlines.EnumerateArray())
                {
                    AppendInline(step, inline);
                }
            }

            index++;
        }
    }

    private static void AppendTextToken(InstructionStepDto step, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (step.Tokens.Count > 0
            && step.Tokens[^1].Kind == InstructionTokenKind.Text)
        {
            step.Tokens[^1].Text += text;
            return;
        }

        step.Tokens.Add(new InstructionTokenDto { Text = text });
    }

    private static string DetailFromHover(string? hover)
    {
        if (string.IsNullOrWhiteSpace(hover))
        {
            return string.Empty;
        }

        var separator = hover.IndexOf(':');
        return separator < 0 ? hover.Trim() : hover[(separator + 1)..].Trim();
    }

    private static InstructionTokenView ResolveToken(InstructionTokenDto token, RecipeDto recipe)
    {
        if (token.Kind == InstructionTokenKind.Text)
        {
            return new InstructionTokenView
            {
                TokenId = token.Id,
                Kind = InstructionTokenKind.Text,
                Text = token.Text ?? string.Empty,
            };
        }

        if (token.Kind == InstructionTokenKind.Ingredient)
        {
            var ingredient = recipe.Ingredients.FirstOrDefault(i => i.Id == token.ItemId);
            if (ingredient is null)
            {
                return MissingView(token, "Ingredient removed from the list");
            }

            return new InstructionTokenView
            {
                TokenId = token.Id,
                Kind = InstructionTokenKind.Ingredient,
                Text = DisplayName(ingredient.Name, "Ingredient"),
                Hover = $"{DisplayName(ingredient.Name, "Ingredient")}: {NumberFormat.WithUnit(ingredient.Grams, "g")}",
            };
        }

        if (token.Kind == InstructionTokenKind.Spice)
        {
            var spice = recipe.Spices.FirstOrDefault(s => s.Id == token.ItemId);
            if (spice is null)
            {
                return MissingView(token, "Spice removed from the list");
            }

            var amount = spice.Grams is decimal grams
                ? NumberFormat.WithUnit(grams, "g")
                : "no amount set";

            return new InstructionTokenView
            {
                TokenId = token.Id,
                Kind = InstructionTokenKind.Spice,
                Text = DisplayName(spice.Name, "Spice"),
                Hover = $"{DisplayName(spice.Name, "Spice")}: {amount}",
            };
        }

        return MissingView(token, "Unknown");
    }

    private static InstructionTokenView MissingView(InstructionTokenDto token, string hover) => new()
    {
        TokenId = token.Id,
        Kind = token.Kind,
        Text = token.Kind == InstructionTokenKind.Spice ? "Missing spice" : "Missing ingredient",
        Hover = hover,
        IsMissing = true,
    };

    private static void EnsureTextEnds(InstructionStepDto step)
    {
        if (step.Tokens.Count == 0)
        {
            step.Tokens.Add(new InstructionTokenDto());
            return;
        }

        if (step.Tokens[0].Kind != InstructionTokenKind.Text)
        {
            step.Tokens.Insert(0, new InstructionTokenDto());
        }

        if (step.Tokens[^1].Kind != InstructionTokenKind.Text)
        {
            step.Tokens.Add(new InstructionTokenDto());
        }
    }

    private static void MergeAdjacentText(InstructionStepDto step)
    {
        for (var i = 0; i < step.Tokens.Count - 1;)
        {
            var current = step.Tokens[i];
            var next = step.Tokens[i + 1];
            if (current.Kind == InstructionTokenKind.Text && next.Kind == InstructionTokenKind.Text)
            {
                current.Text = (current.Text ?? string.Empty) + (next.Text ?? string.Empty);
                step.Tokens.RemoveAt(i + 1);
                continue;
            }

            i++;
        }
    }

    private static string DisplayName(string? name, string fallback) =>
        string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();
}
