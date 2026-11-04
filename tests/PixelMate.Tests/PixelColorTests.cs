using PixelMate.Colors;

namespace PixelMate.Tests;

public sealed class PixelColorTests
{
    [Fact]
    public void OpaqueColorUsesSixDigitHexAndRgb()
    {
        var color = new PixelColor(255, 128, 0, 255);

        Assert.Equal("#FF8000", color.Hex);
        Assert.Equal("rgb(255, 128, 0)", color.Rgb);
        Assert.True(color.IsOpaque);
    }

    [Fact]
    public void TransparentColorUsesEightDigitHexAndRgba()
    {
        var color = new PixelColor(18, 52, 86, 128);

        Assert.Equal("#12345680", color.Hex);
        Assert.Equal("rgba(18, 52, 86, 0.502)", color.Rgb);
        Assert.False(color.IsOpaque);
    }

    [Fact]
    public void FullyTransparentColorPreservesRgbChannels()
    {
        var color = new PixelColor(12, 34, 56, 0);

        Assert.Equal("#0C223800", color.Hex);
        Assert.Equal("rgba(12, 34, 56, 0)", color.Rgb);
    }
}
