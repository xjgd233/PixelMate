namespace PixelMate.Tests;

public sealed class AppTypographyTests
{
    [Fact]
    public void SystemFontFamily_UsesPlatformNativeMetricsBeforeAppleFallbacks()
    {
        var familyNames = AppTypography.SystemFontFamilyName;

        Assert.True(familyNames.IndexOf("Segoe UI", StringComparison.Ordinal)
            < familyNames.IndexOf("SF Pro Text", StringComparison.Ordinal));
        Assert.True(familyNames.IndexOf("Microsoft YaHei UI", StringComparison.Ordinal)
            < familyNames.IndexOf("PingFang SC", StringComparison.Ordinal));
    }
}
