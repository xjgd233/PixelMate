using Avalonia.Media;

namespace PixelMate;

public static class AppTypography
{
    public const string SystemFontFamilyName =
        "SF Pro Text, SF Pro Display, -apple-system, Helvetica Neue, PingFang SC, "
        + "Hiragino Sans GB, Microsoft YaHei UI, Segoe UI";

    public static FontFamily SystemFontFamily { get; } =
        new(SystemFontFamilyName);
}
