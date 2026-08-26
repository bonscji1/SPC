namespace SPC.Core.Models;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack,
}

public static class MealTypes
{
    public static readonly MealType[] All =
    [
        MealType.Breakfast,
        MealType.Lunch,
        MealType.Dinner,
        MealType.Snack,
    ];

    public static string DisplayName(MealType meal) => meal switch
    {
        MealType.Breakfast => "Breakfast",
        MealType.Lunch => "Lunch",
        MealType.Dinner => "Dinner",
        MealType.Snack => "Snack",
        _ => meal.ToString(),
    };
}
