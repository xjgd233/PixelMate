using Avalonia.Media;

namespace PixelMate;

public static class AppTypography
{
    public const string BundledFontFamilyName = "Noto Sans CJK SC";

    public const string BundledFontFamilyUri =
        "avares://PixelMate/Assets/Fonts#Noto Sans CJK SC";

    public static FontFamily ApplicationFontFamily { get; } =
        new(BundledFontFamilyUri);
}
