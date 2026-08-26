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

        Assert.Equal(["A stew", "B stew", "C stew"], page.Items.Select(r => r.Name));
    }

    [Fact]
    public void Page_SameTimestamp_SortsByName()
    {
        var time = DateTimeOffset.Parse("2026-08-01T00:00:00Z");
        var zebra = new RecipeDto { Name = "Zebra", UpdatedAt = time };
        var apple = new RecipeDto { Name = "Apple", UpdatedAt = time };

        var page = RecipeList.Page([zebra, apple], page: 1, pageSize: 10);

        Assert.Equal(["Apple", "Zebra"], page.Items.Select(r => r.Name));
    }

    [Fact]
    public void Page_FiltersByMealType()
    {
        var lunch = new RecipeDto { Name = "Soup", MealType = MealType.Lunch, UpdatedAt = DateTimeOffset.Parse("2026-08-01T00:00:00Z") };
        var dinner = new RecipeDto { Name = "Stew", MealType = MealType.Dinner, UpdatedAt = DateTimeOffset.Parse("2026-08-02T00:00:00Z") };

        var page = RecipeList.Page([lunch, dinner], page: 1, pageSize: 10, mealType: MealType.Dinner);

        Assert.Equal(["Stew"], page.Items.Select(r => r.Name));
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public void Page_FiltersByNameContains()
    {
        var soup = new RecipeDto { Name = "Lentil soup", UpdatedAt = DateTimeOffset.Parse("2026-08-02T00:00:00Z") };
        var stew = new RecipeDto { Name = "Beef stew", UpdatedAt = DateTimeOffset.Parse("2026-08-01T00:00:00Z") };

        var page = RecipeList.Page([soup, stew], page: 1, pageSize: 10, nameQuery: "stew");

        Assert.Equal(["Beef stew"], page.Items.Select(r => r.Name));
    }
}
