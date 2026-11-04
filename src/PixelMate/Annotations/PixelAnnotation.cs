using PixelMate.Imaging;

namespace PixelMate.Annotations;

public sealed record PixelAnnotation(ImagePoint Anchor, string Text)
{
    public const int MaximumTextLength = 500;

    public static PixelAnnotation Create(ImagePoint anchor, string text)
    {
        var normalizedText = NormalizeText(text);
        if (normalizedText.Length == 0)
        {
            throw new ArgumentException("批注文字不能为空。", nameof(text));
        }

        return new PixelAnnotation(anchor, normalizedText);
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
