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
