using PixelMate.Annotations;
using PixelMate.Imaging;

namespace PixelMate.Tests;

public class PixelAnnotationTests
{
    [Fact]
    public void Create_NormalizesTextAndPreservesImageCoordinate()
    {
        var annotation = PixelAnnotation.Create(
            new ImagePoint(12.5, 8.25),
            "  first line\r\nsecond line  ");

        Assert.Equal(new ImagePoint(12.5, 8.25), annotation.Anchor);
        Assert.Equal("first line\nsecond line", annotation.Text);
        Assert.Equal(AnnotationTextColor.Default, annotation.TextColor);
    }

    [Fact]
    public void Create_PreservesCustomTextColor()
    {
        var color = new AnnotationTextColor(0x12, 0xAB, 0xEF);

        var annotation = PixelAnnotation.Create(
            new ImagePoint(1, 1),
            "colored",
            color);

        Assert.Equal(color, annotation.TextColor);
    }

    [Theory]
    [InlineData("#12abEF", 0x12, 0xAB, 0xEF)]
    [InlineData("F0C040", 0xF0, 0xC0, 0x40)]
    public void TryParseHex_AcceptsSixDigitRgbColor(
        string value,
        byte red,
        byte green,
        byte blue)
    {
        var parsed = AnnotationTextColor.TryParseHex(value, out var color);

        Assert.True(parsed);
        Assert.Equal(new AnnotationTextColor(red, green, blue), color);
        Assert.Equal($"#{red:X2}{green:X2}{blue:X2}", color.Hex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("#FFF")]
    [InlineData("#GG0000")]
    [InlineData("1234567")]
    public void TryParseHex_RejectsInvalidColor(string value)
    {
        Assert.False(AnnotationTextColor.TryParseHex(value, out _));
    }

    [Fact]
    public void Create_RejectsWhitespaceOnlyText()
    {
        Assert.Throws<ArgumentException>(
            () => PixelAnnotation.Create(new ImagePoint(1, 1), " \r\n "));
    }

    [Fact]
    public void Create_TruncatesTextAtSafetyLimit()
    {
        var annotation = PixelAnnotation.Create(
            new ImagePoint(1, 1),
            new string('x', PixelAnnotation.MaximumTextLength + 20));

        Assert.Equal(PixelAnnotation.MaximumTextLength, annotation.Text.Length);
    }

    [Fact]
    public void FormatClipboard_JoinsNonEmptyAnnotationsByLine()
    {
        PixelAnnotation[] annotations =
        [
            PixelAnnotation.Create(new ImagePoint(1, 1), "first"),
            PixelAnnotation.Create(new ImagePoint(2, 2), "second")
        ];

        Assert.Equal(
            $"first{Environment.NewLine}second",
            AnnotationFormatting.FormatClipboard(annotations));
    }
}
