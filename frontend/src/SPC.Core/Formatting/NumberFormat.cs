using System.Globalization;

namespace SPC.Core.Formatting;

/// <summary>
/// Standard display format for quantities: whole numbers have no decimals;
/// anything that is not whole is shown with exactly two decimal places.
/// </summary>
public static class NumberFormat
{
    public static string Format(decimal value)
    {
        var rounded = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        if (rounded == decimal.Truncate(rounded))
        {
            return rounded.ToString("0", CultureInfo.InvariantCulture);
        }

        return rounded.ToString("0.00", CultureInfo.InvariantCulture);
    }

    public static string Format(decimal? value) =>
        value is null ? string.Empty : Format(value.Value);

    public static string WithUnit(decimal value, string unit) =>
        $"{Format(value)} {unit}";
}
