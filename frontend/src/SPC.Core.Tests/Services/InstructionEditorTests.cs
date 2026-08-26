using SPC.Core.Models;
using SPC.Core.Services;
using Xunit;

namespace SPC.Core.Tests.Services;

public class InstructionEditorTests
{
    [Fact]
    public void StepHasContent_False_ForEmptyText()
    {
        Assert.False(InstructionEditor.StepHasContent(InstructionEditor.NewStep()));
    }

    [Fact]
    public void InsertLink_SplitsTextAroundChip()
    {
        var flour = new RecipeIngredientDto { Name = "flour", Grams = 200, CaloriesPer100g = 364 };
        var recipe = new RecipeDto { Ingredients = [flour] };
        var step = InstructionEditor.NewStep();
        var textId = step.Tokens[0].Id;
        step.Tokens[0].Text = "mix  well";

        var afterId = InstructionEditor.InsertLink(
            step, textId, caret: 4, InstructionTokenKind.Ingredient, flour.Id);

        Assert.Equal("mix ", step.Tokens[0].Text);
        Assert.Equal(InstructionTokenKind.Ingredient, step.Tokens[1].Kind);
        Assert.Equal(flour.Id, step.Tokens[1].ItemId);
        Assert.Equal(" well", step.Tokens[2].Text);
        Assert.Equal(afterId, step.Tokens[2].Id);

        var views = InstructionEditor.Resolve(step, recipe);
        Assert.Equal("flour", views[1].Text);
        Assert.Contains("200 g", views[1].Hover);
    }

    [Fact]
    public void Resolve_MarksMissingIngredient()
    {
        var step = InstructionEditor.NewStep();
        var textId = step.Tokens[0].Id;
        InstructionEditor.InsertLink(
            step, textId, 0, InstructionTokenKind.Ingredient, Guid.NewGuid());

        var views = InstructionEditor.Resolve(step, new RecipeDto());

        Assert.True(views.Single(v => v.Kind == InstructionTokenKind.Ingredient).IsMissing);
    }

    [Fact]
    public void RemoveToken_MergesAdjacentText()
    {
        var flour = new RecipeIngredientDto { Name = "flour", Grams = 200, CaloriesPer100g = 364 };
        var step = InstructionEditor.NewStep();
        step.Tokens[0].Text = "mix  in";
        var textId = step.Tokens[0].Id;
        InstructionEditor.InsertLink(step, textId, 4, InstructionTokenKind.Ingredient, flour.Id);
        var linkId = step.Tokens[1].Id;

        InstructionEditor.RemoveToken(step, linkId);

        Assert.Single(step.Tokens);
        Assert.Equal("mix  in", step.Tokens[0].Text);
    }

    [Fact]
    public void FilterLinks_MatchesName()
    {
        var links = InstructionEditor.AvailableLinks(new RecipeDto
        {
            Ingredients =
            [
                new RecipeIngredientDto { Name = "flour", Grams = 200 },
                new RecipeIngredientDto { Name = "water", Grams = 300 },
            ],
        });

        var filtered = InstructionEditor.FilterLinks(links, "fl");

        Assert.Single(filtered);
        Assert.Equal("flour", filtered[0].Name);
    }

    [Fact]
    public void ApplyTiptapJson_SyncsTokensFromMention()
    {
        var flour = new RecipeIngredientDto { Name = "flour", Grams = 200, CaloriesPer100g = 364 };
        var recipe = new RecipeDto { Ingredients = [flour] };
        var step = InstructionEditor.NewStep();
        var json = """
            {"type":"doc","content":[{"type":"paragraph","content":[
            {"type":"text","text":"Mix "},
            {"type":"ingredientMention","attrs":{"id":"%s","label":"flour","kind":"ingredient","detail":"200 g"}},
            {"type":"text","text":" well."}
            ]}]}
            """.Replace("%s", flour.Id.ToString());

        InstructionEditor.ApplyTiptapJson(step, json);

        Assert.Equal(3, step.Tokens.Count);
        Assert.Equal("Mix ", step.Tokens[0].Text);
        Assert.Equal(InstructionTokenKind.Ingredient, step.Tokens[1].Kind);
        Assert.Equal(flour.Id, step.Tokens[1].ItemId);
        Assert.Equal(" well.", step.Tokens[2].Text);
        Assert.Equal(json, step.EditorJson);
    }

    [Fact]
    public void SerializeStepToTiptap_UsesEditorJsonWhenPresent()
    {
        var step = InstructionEditor.NewStep();
        step.EditorJson = """{"type":"doc","content":[{"type":"paragraph"}]}""";

        var json = InstructionEditor.SerializeStepToTiptap(step, new RecipeDto());

        Assert.Equal(step.EditorJson, json);
    }
}
