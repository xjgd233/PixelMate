using PixelMate.Magnifier;

namespace PixelMate.Tests;

public sealed class MagnifierLayoutTests
{
    [Theory]
    [InlineData(8, 23)]
    [InlineData(12, 15)]
    [InlineData(16, 11)]
    public void SamplePixelCountIsOddAndFitsViewport(
        int magnification,
        int expectedCount)
    {
        var count = MagnifierLayoutCalculator.GetSamplePixelCount(magnification);

        Assert.Equal(expectedCount, count);
        Assert.Equal(1, count % 2);
        Assert.True(count * magnification <= MagnifierLayoutCalculator.ViewportSize);
    }

    [Fact]
    public void CentersCurrentPixelInsideViewport()
    {
        var layout = MagnifierLayoutCalculator.Create(
            1000,
            800,
            500,
            400,
            50,
            40,
            100,
            100,
            12);

        Assert.Equal(15, layout.SamplePixelCount);
        Assert.Equal(43, layout.SampleStartPixelX);
        Assert.Equal(33, layout.SampleStartPixelY);
        Assert.Equal(15, layout.SourceRegion.Width);
        Assert.Equal(15, layout.SourceRegion.Height);
        Assert.Equal(
            layout.ViewportBounds.X + (MagnifierLayoutCalculator.ViewportSize / 2),
            layout.CurrentPixelBounds.X + (layout.CurrentPixelBounds.Width / 2));
        Assert.Equal(
            layout.ViewportBounds.Y + (MagnifierLayoutCalculator.ViewportSize / 2),
            layout.CurrentPixelBounds.Y + (layout.CurrentPixelBounds.Height / 2));
    }

    [Fact]
    public void ClipsSourceRegionButKeepsPixelCenteredAtTopLeftEdge()
    {
        var layout = MagnifierLayoutCalculator.Create(
            1000,
            800,
            200,
            200,
            0,
            0,
            100,
            100,
            12);

        Assert.Equal(-7, layout.SampleStartPixelX);
        Assert.Equal(-7, layout.SampleStartPixelY);
        Assert.Equal(new MagnifierPixelRegion(0, 0, 8, 8), layout.SourceRegion);
        Assert.Equal(96, layout.SourceDestinationBounds.Width);
        Assert.Equal(96, layout.SourceDestinationBounds.Height);
        Assert.Equal(
            layout.ViewportBounds.X + (MagnifierLayoutCalculator.ViewportSize / 2),
            layout.CurrentPixelBounds.X + (layout.CurrentPixelBounds.Width / 2));
        Assert.Equal(
            layout.ViewportBounds.Y + (MagnifierLayoutCalculator.ViewportSize / 2),
            layout.CurrentPixelBounds.Y + (layout.CurrentPixelBounds.Height / 2));
    }

    [Fact]
    public void ClipsSourceRegionButKeepsPixelCenteredAtBottomRightEdge()
    {
        var layout = MagnifierLayoutCalculator.Create(
            1000,
            800,
            200,
            200,
            99,
            99,
            100,
            100,
            16);

        Assert.Equal(94, layout.SampleStartPixelX);
        Assert.Equal(94, layout.SampleStartPixelY);
        Assert.Equal(104, layout.SampleStartPixelX + layout.SamplePixelCount - 1);
        Assert.Equal(104, layout.SampleStartPixelY + layout.SamplePixelCount - 1);
        Assert.Equal(new MagnifierPixelRegion(94, 94, 6, 6), layout.SourceRegion);
        Assert.Equal(96, layout.SourceDestinationBounds.Width);
        Assert.Equal(96, layout.SourceDestinationBounds.Height);
        Assert.Equal(
            layout.ViewportBounds.X + (MagnifierLayoutCalculator.ViewportSize / 2),
            layout.CurrentPixelBounds.X + (layout.CurrentPixelBounds.Width / 2));
        Assert.Equal(
            layout.ViewportBounds.Y + (MagnifierLayoutCalculator.ViewportSize / 2),
            layout.CurrentPixelBounds.Y + (layout.CurrentPixelBounds.Height / 2));
    }

    [Fact]
    public void FlipsAndClampsCardInsideBottomRightEdge()
    {
        var layout = MagnifierLayoutCalculator.Create(
            1000,
            800,
            990,
            790,
            10,
            10,
            20,
            20,
            8);

        Assert.True(layout.CardBounds.X >= MagnifierLayoutCalculator.ViewportMargin);
        Assert.True(layout.CardBounds.Y >= MagnifierLayoutCalculator.ViewportMargin);
        Assert.True(layout.CardBounds.Right <= 994);
        Assert.True(layout.CardBounds.Bottom <= 794);
        Assert.True(layout.CardBounds.X < 990);
        Assert.True(layout.CardBounds.Y < 790);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsInvalidMagnification(int magnification)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MagnifierLayoutCalculator.GetSamplePixelCount(magnification));
    }
}
