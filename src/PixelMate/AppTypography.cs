using Avalonia.Media;

namespace PixelMate;

public static class AppTypography
{
    public static string SystemFontFamilyName =>
        OperatingSystem.IsMacOS()
            ? "PingFang SC, Hiragino Sans GB"
            : OperatingSystem.IsWindows()
                ? "Microsoft YaHei UI, Segoe UI Variable Text, Segoe UI"
                : "Noto Sans CJK SC, DejaVu Sans";

    public static FontFamily SystemFontFamily { get; } =
        new(SystemFontFamilyName);
}
