using SPC.Core.Models;
using Xunit;

namespace SPC.Core.Tests.Models;

public class RecipeListTests
{
    [Fact]
    public void Page_OrdersByUpdatedAtThenName()
    {
        var older = new RecipeDto { Name = "B stew", UpdatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z") };
        var newer = new RecipeDto { Name = "A stew", UpdatedAt = DateTimeOffset.Parse("2026-06-01T00:00:00Z") };
        var unnamedTime = new RecipeDto { Name = "C stew" };

        var page = RecipeList.Page([older, unnamedTime, newer], page: 1, pageSize: 10);

        Assert.Equal(["A stew", "B stew", "C stew"], page.Items.Select(f => f.Primary.Name));
    }

    [Fact]
    public void Page_SameTimestamp_SortsByName()
    {
        var time = DateTimeOffset.Parse("2026-08-01T00:00:00Z");
        var zebra = new RecipeDto { Name = "Zebra", UpdatedAt = time };
        var apple = new RecipeDto { Name = "Apple", UpdatedAt = time };

        var page = RecipeList.Page([zebra, apple], page: 1, pageSize: 10);

        Assert.Equal(["Apple", "Zebra"], page.Items.Select(f => f.Primary.Name));
    }

    [Fact]
    public void Page_FiltersByMealType()
    {
        var lunch = new RecipeDto { Name = "Soup", MealType = MealType.Lunch, UpdatedAt = DateTimeOffset.Parse("2026-08-01T00:00:00Z") };
        var dinner = new RecipeDto { Name = "Stew", MealType = MealType.Dinner, UpdatedAt = DateTimeOffset.Parse("2026-08-02T00:00:00Z") };

        var page = RecipeList.Page([lunch, dinner], page: 1, pageSize: 10, mealType: MealType.Dinner);

        Assert.Equal(["Stew"], page.Items.Select(f => f.Primary.Name));
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public void Page_FiltersByNameContains()
    {
        var soup = new RecipeDto { Name = "Lentil soup", UpdatedAt = DateTimeOffset.Parse("2026-08-02T00:00:00Z") };
        var stew = new RecipeDto { Name = "Beef stew", UpdatedAt = DateTimeOffset.Parse("2026-08-01T00:00:00Z") };

        var page = RecipeList.Page([soup, stew], page: 1, pageSize: 10, nameQuery: "stew");

        Assert.Equal(["Beef stew"], page.Items.Select(f => f.Primary.Name));
    }

    [Fact]
    public void Page_GroupsVariantsAsOneFamily()
    {
        var familyId = Guid.NewGuid();
        var original = new RecipeDto
        {
            Id = familyId,
            FamilyId = familyId,
            Name = "Bolognese",
            VariantLabel = string.Empty,
            UpdatedAt = DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
        };
        var extraOnion = new RecipeDto
        {
            FamilyId = familyId,
            Name = "Bolognese",
            VariantLabel = "extra onion",
            UpdatedAt = DateTimeOffset.Parse("2026-08-03T00:00:00Z"),
        };
        var other = new RecipeDto { Name = "Soup", UpdatedAt = DateTimeOffset.Parse("2026-08-02T00:00:00Z") };

        var page = RecipeList.Page([original, extraOnion, other], page: 1, pageSize: 10);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal("Bolognese", page.Items[0].Primary.Name);
        Assert.Equal(["extra onion"], page.Items[0].Variants.Select(v => v.VariantLabel));
        Assert.Equal(["Bolognese", "Bolognese"], page.Items[0].AllMembers.Select(m => m.Name));
        Assert.Equal(["Default", "extra onion"], page.Items[0].AllMembers.Select(m => m.DisplayVariantLabel()));
        Assert.Equal("Soup", page.Items[1].Primary.Name);
    }

    [Fact]
    public void Page_NameQuery_MatchesVariantLabel()
    {
        var familyId = Guid.NewGuid();
        var original = new RecipeDto { Id = familyId, FamilyId = familyId, Name = "Stew", VariantLabel = string.Empty };
        var turkey = new RecipeDto { FamilyId = familyId, Name = "Stew", VariantLabel = "turkey" };

        var page = RecipeList.Page([original, turkey], page: 1, pageSize: 10, nameQuery: "turkey");

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(familyId, page.Items[0].FamilyId);
        Assert.Contains(page.Items[0].Variants, v => v.VariantLabel == "turkey");
    }

    [Fact]
    public void Page_PagesByFamilyNotRow()
    {
        var familyId = Guid.NewGuid();
        var original = new RecipeDto { Id = familyId, FamilyId = familyId, Name = "A", UpdatedAt = DateTimeOffset.Parse("2026-08-02T00:00:00Z") };
        var variant = new RecipeDto { FamilyId = familyId, Name = "A", VariantLabel = "v", UpdatedAt = DateTimeOffset.Parse("2026-08-03T00:00:00Z") };
        var other = new RecipeDto { Name = "B", UpdatedAt = DateTimeOffset.Parse("2026-08-01T00:00:00Z") };

        var page = RecipeList.Page([original, variant, other], page: 1, pageSize: 10);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(1, page.TotalPages);
    }

    [Fact]
    public void VariantLabelIsTaken_IgnoresSelfAndTreatsDefaultAsEmpty()
    {
        var familyId = Guid.NewGuid();
        var original = new RecipeDto { Id = familyId, FamilyId = familyId, VariantLabel = string.Empty };
        var turkey = new RecipeDto { FamilyId = familyId, VariantLabel = "turkey" };

        Assert.False(RecipeList.VariantLabelIsTaken([original, turkey], turkey.Id, "turkey"));
        Assert.True(RecipeList.VariantLabelIsTaken([original, turkey], turkey.Id, string.Empty));
        Assert.True(RecipeList.VariantLabelIsTaken([original, turkey], original.Id, "TURKEY"));
        Assert.False(RecipeList.VariantLabelIsTaken([original, turkey], original.Id, string.Empty));
    }
}
