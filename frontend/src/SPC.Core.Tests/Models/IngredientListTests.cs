using SPC.Core.Models;
using Xunit;

namespace SPC.Core.Tests.Models;

public class IngredientListTests
{
    [Fact]
    public void Page_OrdersByName()
    {
        var zebra = new IngredientDto { Name = "zebra", CaloriesPer100g = 1 };
        var apple = new IngredientDto { Name = "apple", CaloriesPer100g = 52 };

        var page = IngredientList.Page([zebra, apple], page: 1, pageSize: 10);

        Assert.Equal(["apple", "zebra"], page.Items.Select(i => i.Name));
    }

    [Fact]
    public void Page_FiltersByNameContains()
    {
        var onion = new IngredientDto { Name = "onion", CaloriesPer100g = 40 };
        var carrot = new IngredientDto { Name = "carrot", CaloriesPer100g = 41 };

        var page = IngredientList.Page([onion, carrot], page: 1, pageSize: 10, nameQuery: "ONI");

        Assert.Equal(["onion"], page.Items.Select(i => i.Name));
    }
}
