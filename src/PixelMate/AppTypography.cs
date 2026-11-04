using Avalonia.Media;

namespace PixelMate;

public static class AppTypography
{
    public const string SystemFontFamilyName =
        "Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI, Microsoft YaHei, "
        + "SF Pro Text, SF Pro Display, PingFang SC, Hiragino Sans GB, Noto Sans CJK SC";

    public static FontFamily SystemFontFamily { get; } =
        new(SystemFontFamilyName);
}
