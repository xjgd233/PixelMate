using Avalonia.Media;

namespace PixelMate;

public static class AppTypography
{
    public static string SystemFontFamilyName =>
        OperatingSystem.IsMacOS()
            ? "PingFang SC"
            : OperatingSystem.IsWindows()
                ? "Microsoft YaHei UI"
                : "Noto Sans CJK SC";

    public static FontFamily SystemFontFamily { get; } =
        new(SystemFontFamilyName);
}
