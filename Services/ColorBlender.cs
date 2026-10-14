using System.Globalization;

namespace PersonalPlanner.Services;

public static class ColorBlender
{
    /// <summary>
    /// Blends multiple hex colors (#RRGGBB) into a single hex color using root-mean-square (gamma-aware) RGB averaging
    /// so blended calendar cells remain vibrant rather than muddy.
    /// </summary>
    public static string BlendHexColors(IEnumerable<string> hexColors)
    {
        var validRgb = hexColors
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Select(ParseHexToRgb)
            .ToList();

        if (validRgb.Count == 0)
            return "#FFFFFF";

        if (validRgb.Count == 1)
        {
            var single = validRgb[0];
            return $"#{single.R:X2}{single.G:X2}{single.B:X2}";
        }

        double sumRSq = 0, sumGSq = 0, sumBSq = 0;
        foreach (var (r, g, b) in validRgb)
        {
            sumRSq += r * r;
            sumGSq += g * g;
            sumBSq += b * b;
        }

        int count = validRgb.Count;
        int blendedR = (int)Math.Round(Math.Sqrt(sumRSq / count));
        int blendedG = (int)Math.Round(Math.Sqrt(sumGSq / count));
        int blendedB = (int)Math.Round(Math.Sqrt(sumBSq / count));

        return $"#{blendedR:X2}{blendedG:X2}{blendedB:X2}";
    }

    /// <summary>
    /// Converts a #RRGGBB hex color into an rgba(r, g, b, alpha) CSS string for calendar cell background fills.
    /// </summary>
    public static string ToRgbaCss(string hexColor, double alpha = 0.28)
    {
        var (r, g, b) = ParseHexToRgb(hexColor);
        return FormattableString.Invariant($"rgba({r}, {g}, {b}, {alpha:F2})");
    }

    public static (int R, int G, int B) ParseHexToRgb(string hex)
    {
        string clean = hex.Trim().TrimStart('#');
        if (clean.Length == 3)
        {
            clean = string.Concat(clean.Select(c => $"{c}{c}"));
        }

        if (clean.Length != 6 ||
            !int.TryParse(clean[0..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r) ||
            !int.TryParse(clean[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g) ||
            !int.TryParse(clean[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
        {
            return (255, 255, 255);
        }

        return (r, g, b);
    }
}
