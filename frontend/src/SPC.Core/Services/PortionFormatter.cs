using SPC.Core.Formatting;

namespace SPC.Core.Services;

/// <summary>Formats portion counts with simple vulgar fractions when they match closely.</summary>
public static class PortionFormatter
{
    private static readonly (decimal Remainder, string Glyph)[] Fractions =
    [
        (0.25m, "¼"),
        (1m / 3m, "⅓"),
        (0.5m, "½"),
        (2m / 3m, "⅔"),
        (0.75m, "¾"),
    ];

    public static string FormatPortions(decimal portions)
    {
        if (portions < 0)
        {
            return NumberFormat.Format(portions);
        }

        var whole = decimal.Floor(portions);
        var remainder = portions - whole;

        if (remainder == 0)
        {
            return NumberFormat.Format(whole);
        }

        foreach (var (value, glyph) in Fractions)
        {
            if (decimal.Abs(remainder - value) <= 0.01m)
            {
                return whole == 0 ? glyph : $"{NumberFormat.Format(whole)}{glyph}";
            }
        }

        return NumberFormat.Format(portions);
    }
}
