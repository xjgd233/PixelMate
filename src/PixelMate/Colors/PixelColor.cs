using System.Globalization;

namespace PixelMate.Colors;

public readonly record struct PixelColor(byte Red, byte Green, byte Blue, byte Alpha)
{
    public bool IsOpaque => Alpha == byte.MaxValue;

    public string Hex => IsOpaque
        ? $"#{Red:X2}{Green:X2}{Blue:X2}"
        : $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";

    public string Rgb => IsOpaque
        ? string.Create(
            CultureInfo.InvariantCulture,
            $"rgb({Red}, {Green}, {Blue})")
        : string.Create(
            CultureInfo.InvariantCulture,
            $"rgba({Red}, {Green}, {Blue}, {Alpha / 255d:0.###})");
}
