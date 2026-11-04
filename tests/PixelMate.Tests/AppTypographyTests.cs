namespace PixelMate.Tests;

public sealed class AppTypographyTests
{
    [Fact]
    public void SystemFontFamily_UsesOneCompleteNativeCjkFamilyPerPlatform()
    {
        var familyNames = AppTypography.SystemFontFamilyName;

        var expectedPrimaryFont = OperatingSystem.IsMacOS()
            ? "PingFang SC"
            : OperatingSystem.IsWindows()
                ? "Microsoft YaHei UI"
                : "Noto Sans CJK SC";
        Assert.StartsWith(expectedPrimaryFont, familyNames, StringComparison.Ordinal);
    }
}
