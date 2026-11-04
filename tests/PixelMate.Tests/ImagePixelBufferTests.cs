using PixelMate.Imaging;

namespace PixelMate.Tests;

public sealed class ImagePixelBufferTests
{
    [Fact]
    public void ReadsBgraPixelsAtRequestedCoordinates()
    {
        var pixels = new byte[]
        {
            3, 2, 1, 255,
            30, 20, 10, 128
        };
        var buffer = new ImagePixelBuffer(2, 1, pixels, 8);

        Assert.True(buffer.TryGetPixel(0, 0, out var first));
        Assert.Equal((byte)1, first.Red);
        Assert.Equal((byte)2, first.Green);
        Assert.Equal((byte)3, first.Blue);
        Assert.Equal((byte)255, first.Alpha);

        Assert.True(buffer.TryGetPixel(1, 0, out var second));
        Assert.Equal(new PixelMate.Colors.PixelColor(10, 20, 30, 128), second);
    }

    [Fact]
    public void HandlesPaddedRowsWithoutReadingNextRowData()
    {
        var pixels = new byte[]
        {
            1, 2, 3, 255,
            9, 9, 9, 9,
            4, 5, 6, 128,
            9, 9, 9, 9
        };
        var buffer = new ImagePixelBuffer(1, 2, pixels, 8);

        Assert.True(buffer.TryGetPixel(0, 1, out var color));
        Assert.Equal(new PixelMate.Colors.PixelColor(6, 5, 4, 128), color);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2, 0)]
    [InlineData(0, 1)]
    public void RejectsOutOfBoundsCoordinates(int x, int y)
    {
        var buffer = new ImagePixelBuffer(2, 1, new byte[8], 8);

        Assert.False(buffer.TryGetPixel(x, y, out _));
    }

    [Fact]
    public void CopiesInputInsteadOfRetainingMutableSource()
    {
        var pixels = new byte[] { 3, 2, 1, 255 };
        var buffer = new ImagePixelBuffer(1, 1, pixels, 4);
        pixels[2] = 99;

        Assert.True(buffer.TryGetPixel(0, 0, out var color));
        Assert.Equal((byte)1, color.Red);
    }

    [Fact]
    public void RejectsRowsTooNarrowForImageWidth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ImagePixelBuffer(2, 1, new byte[7], 7));
    }

    [Fact]
    public void RejectsBufferShorterThanAllRows()
    {
        Assert.Throws<ArgumentException>(
            () => new ImagePixelBuffer(1, 2, new byte[7], 4));
    }
}
