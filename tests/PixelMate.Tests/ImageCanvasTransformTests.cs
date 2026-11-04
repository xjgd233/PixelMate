using PixelMate.Imaging;

namespace PixelMate.Tests;

public class ImageCanvasTransformTests
{
    [Fact]
    public void FitToViewport_CentersImageAndPreservesAspectRatio()
    {
        var transform = CreateTransform(1000, 500, 500, 500);

        Assert.True(transform.IsFitToViewport);
        Assert.Equal(0.452, transform.Scale, 3);
        Assert.Equal(24, transform.OffsetX, 3);
        Assert.Equal((500 - transform.ScaledImageHeight) / 2, transform.OffsetY, 3);
    }

    [Fact]
    public void ActualSize_CentersImageAtOneToOneScale()
    {
        var transform = CreateTransform(1000, 500, 500, 500);

        transform.ShowActualSize();

        Assert.False(transform.IsFitToViewport);
        Assert.Equal(1, transform.Scale);
        Assert.Equal(-250, transform.OffsetX);
        Assert.Equal(0, transform.OffsetY);
    }

    [Fact]
    public void ZoomTo_KeepsAnchorPixelUnderPointer()
    {
        var transform = CreateTransform(1000, 500, 500, 500);
        transform.ShowActualSize();
        var anchor = new CanvasPoint(250, 250);
        Assert.True(transform.TryCanvasToImage(anchor, out var imageBefore));

        transform.ZoomTo(2, anchor);

        Assert.True(transform.TryCanvasToImage(anchor, out var imageAfter));
        Assert.Equal(imageBefore.X, imageAfter.X, 6);
        Assert.Equal(imageBefore.Y, imageAfter.Y, 6);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(2.0)]
    public void CoordinateRoundTrip_RemainsExactAfterZoomAndPan(double scale)
    {
        var transform = CreateTransform(1000, 750, 800, 600);
        transform.ZoomTo(scale, new CanvasPoint(400, 300));
        transform.PanBy(-120, 47);
        var original = new ImagePoint(321.25, 487.75);

        var canvasPoint = transform.ImageToCanvas(original);
        var converted = transform.TryCanvasToImage(canvasPoint, out var roundTrip);

        Assert.True(converted);
        Assert.Equal(original.X, roundTrip.X, 6);
        Assert.Equal(original.Y, roundTrip.Y, 6);
    }

    [Fact]
    public void PanBy_ClampsImageToViewportEdges()
    {
        var transform = CreateTransform(1000, 500, 500, 500);
        transform.ShowActualSize();

        transform.PanBy(2000, -2000);

        Assert.Equal(0, transform.OffsetX);
        Assert.Equal(0, transform.OffsetY);
    }

    [Fact]
    public void ViewportResize_ReappliesFitMode()
    {
        var transform = CreateTransform(1000, 500, 500, 500);

        transform.SetViewport(800, 600);

        Assert.True(transform.IsFitToViewport);
        Assert.Equal(752.0 / 1000, transform.Scale, 6);
        Assert.Equal(24, transform.OffsetX, 6);
        Assert.Equal((600 - transform.ScaledImageHeight) / 2, transform.OffsetY, 6);
    }

    [Fact]
    public void TryCanvasToImagePixel_RejectsPointsOutsideImage()
    {
        var transform = CreateTransform(100, 80, 500, 500);
        transform.ShowActualSize();

        var inside = transform.TryCanvasToImagePixel(
            new CanvasPoint(transform.OffsetX + 99.9, transform.OffsetY + 79.9),
            out var x,
            out var y);
        var outside = transform.TryCanvasToImagePixel(
            new CanvasPoint(transform.OffsetX + 100, transform.OffsetY + 80),
            out _,
            out _);

        Assert.True(inside);
        Assert.Equal(99, x);
        Assert.Equal(79, y);
        Assert.False(outside);
    }

    [Fact]
    public void ZoomTo_IsLimitedToSupportedScaleRange()
    {
        var transform = CreateTransform(100, 100, 500, 500);
        var anchor = new CanvasPoint(250, 250);

        transform.ZoomTo(9999, anchor);
        Assert.Equal(ImageCanvasTransform.MaximumScale, transform.Scale);

        transform.ZoomTo(0.000001, anchor);
        Assert.Equal(ImageCanvasTransform.MinimumScale, transform.Scale);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(2.0)]
    public void ImageCoordinates_StayStableThroughZoomAndPan(double scale)
    {
        var transform = CreateTransform(1000, 750, 800, 600);
        transform.ZoomTo(scale, new CanvasPoint(400, 300));
        transform.PanBy(-120, 47);
        var imagePoint = new ImagePoint(321.25, 487.75);

        var canvasPoint = transform.ImageToCanvas(imagePoint);
        Assert.True(transform.TryCanvasToImage(canvasPoint, out var roundTrip));

        Assert.Equal(imagePoint.X, roundTrip.X, 6);
        Assert.Equal(imagePoint.Y, roundTrip.Y, 6);
    }

    [Fact]
    public void CanvasToImageClamped_ConstrainsDragToOriginalImageBounds()
    {
        var transform = CreateTransform(100, 80, 500, 500);
        transform.ShowActualSize();

        var upperLeft = transform.CanvasToImageClamped(
            new CanvasPoint(transform.OffsetX - 50, transform.OffsetY - 50));
        var lowerRight = transform.CanvasToImageClamped(
            new CanvasPoint(transform.OffsetX + 150, transform.OffsetY + 130));

        Assert.Equal(new ImagePoint(0, 0), upperLeft);
        Assert.Equal(new ImagePoint(100, 80), lowerRight);
    }

    private static ImageCanvasTransform CreateTransform(
        double imageWidth,
        double imageHeight,
        double viewportWidth,
        double viewportHeight)
    {
        var transform = new ImageCanvasTransform();
        transform.SetViewport(viewportWidth, viewportHeight);
        transform.SetImage(new ImageSize(imageWidth, imageHeight));
        return transform;
    }
}
