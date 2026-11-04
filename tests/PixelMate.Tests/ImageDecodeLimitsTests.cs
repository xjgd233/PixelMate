using PixelMate.Imaging;

namespace PixelMate.Tests;

public sealed class ImageDecodeLimitsTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 100)]
    [InlineData(100, -1)]
    public void RejectsNonPositiveDimensions(int width, int height)
    {
        Assert.Throws<InvalidDataException>(
            () => ImageDecodeLimits.Validate(width, height));
    }

    [Theory]
    [InlineData(32_768, 1)]
    [InlineData(1, 32_768)]
    public void RejectsDimensionsAbovePerAxisLimit(int width, int height)
    {
        Assert.Throws<InvalidDataException>(
            () => ImageDecodeLimits.Validate(width, height));
    }

    [Fact]
    public void AcceptsMaximumPixelBudgetWithoutOverflow()
    {
        ImageDecodeLimits.Validate(8_000, 8_000);
    }

    [Fact]
    public void RejectsImageAbovePixelBudget()
    {
        Assert.Throws<InvalidDataException>(
            () => ImageDecodeLimits.Validate(8_001, 8_000));
    }
}
