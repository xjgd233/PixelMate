using PixelMate.Imaging;

namespace PixelMate.Tests;

public sealed class EditedImageExportTests
{
    [Theory]
    [InlineData(320, 240, 1d)]
    [InlineData(900, 540, 1d)]
    [InlineData(1800, 1200, 1.3333333333333333d)]
    [InlineData(4000, 3000, 3d)]
    public void CalculateOverlayScale_UsesReadableRange(
        int width,
        int height,
        double expected)
    {
        Assert.Equal(
            expected,
            EditedImageExport.CalculateOverlayScale(width, height),
            precision: 10);
    }

    [Fact]
    public void CreateSuggestedFileName_PreservesSourceName()
    {
        Assert.Equal(
            "homepage-已编辑.png",
            EditedImageExport.CreateSuggestedFileName("/tmp/homepage.webp"));
    }

    [Fact]
    public void CreateSuggestedFileName_UsesFallbackForMissingPath()
    {
        Assert.Equal(
            "PixelMate-已编辑.png",
            EditedImageExport.CreateSuggestedFileName(null));
    }
}
