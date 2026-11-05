namespace PixelMate.Tests;

public sealed class AppTypographyTests
{
    [Fact]
    public void ApplicationFont_UsesBundledCompleteCjkFamily()
    {
        Assert.Equal(
            "avares://PixelMate/Assets/Fonts#Noto Sans CJK SC",
            AppTypography.BundledFontFamilyUri);
        Assert.Equal(
            AppTypography.BundledFontFamilyName,
            AppTypography.ApplicationFontFamily.Name);
        Assert.Single(AppTypography.ApplicationFontFamily.FamilyNames);
    }
}
