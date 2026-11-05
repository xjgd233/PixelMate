using PixelMate.Imaging;
using System.Globalization;

namespace PixelMate.Annotations;

public readonly record struct AnnotationTextColor(byte Red, byte Green, byte Blue)
{
    public static AnnotationTextColor Default { get; } = new(0xEF, 0xFF, 0xFB);

    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";

    public static bool TryParseHex(string? value, out AnnotationTextColor color)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.StartsWith('#'))
        {
            normalized = normalized[1..];
        }

        if (normalized.Length != 6
            || !byte.TryParse(
                normalized.AsSpan(0, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var red)
            || !byte.TryParse(
                normalized.AsSpan(2, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var green)
            || !byte.TryParse(
                normalized.AsSpan(4, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var blue))
        {
            color = Default;
            return false;
        }

        color = new AnnotationTextColor(red, green, blue);
        return true;
    }
}

public sealed record PixelAnnotation(
    ImagePoint Anchor,
    string Text,
    AnnotationTextColor TextColor)
{
    public const int MaximumTextLength = 500;

    public static PixelAnnotation Create(ImagePoint anchor, string text)
    {
        return Create(anchor, text, AnnotationTextColor.Default);
    }

    public static PixelAnnotation Create(
        ImagePoint anchor,
        string text,
        AnnotationTextColor textColor)
    {
        var normalizedText = NormalizeText(text);
        if (normalizedText.Length == 0)
        {
            throw new ArgumentException("批注文字不能为空。", nameof(text));
        }

        return new PixelAnnotation(anchor, normalizedText, textColor);
    }

    public static string NormalizeText(string? text)
    {
        var normalized = (text ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();
        return normalized.Length <= MaximumTextLength
            ? normalized
            : normalized[..MaximumTextLength];
    }
}

public static class AnnotationFormatting
{
    public static string FormatClipboard(IEnumerable<PixelAnnotation> annotations)
    {
        return string.Join(
            Environment.NewLine,
            annotations.Select(annotation => annotation.Text));
    }
}
