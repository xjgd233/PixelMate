using PixelMate.Imaging;
using PixelMate.Measurement;

namespace PixelMate.Tests;

public class PixelMeasurementTests
{
    [Fact]
    public void DiagonalMeasurement_CalculatesExpectedImagePixelDistances()
    {
        var measurement = new PixelMeasurement(
            new ImagePoint(10, 10),
            new ImagePoint(110, 76));

        Assert.Equal(100, measurement.DeltaX, 6);
        Assert.Equal(66, measurement.DeltaY, 6);
        Assert.Equal(119.816526, measurement.Length, 6);
    }

    [Fact]
    public void ReversedDrag_ProducesSameDistances()
    {
        var forward = new PixelMeasurement(
            new ImagePoint(10, 10),
            new ImagePoint(110, 76));
        var reversed = new PixelMeasurement(
            new ImagePoint(110, 76),
            new ImagePoint(10, 10));

        Assert.Equal(forward.DeltaX, reversed.DeltaX, 6);
        Assert.Equal(forward.DeltaY, reversed.DeltaY, 6);
        Assert.Equal(forward.Length, reversed.Length, 6);
    }

    [Theory]
    [InlineData(20, 50, 120, 50, 100, 0)]
    [InlineData(50, 20, 50, 100, 0, 80)]
    public void AxisAlignedMeasurements_ReportHorizontalAndVerticalDistances(
        double startX,
        double startY,
        double endX,
        double endY,
        double expectedDeltaX,
        double expectedDeltaY)
    {
        var measurement = new PixelMeasurement(
            new ImagePoint(startX, startY),
            new ImagePoint(endX, endY));

        Assert.Equal(expectedDeltaX, measurement.DeltaX, 6);
        Assert.Equal(expectedDeltaY, measurement.DeltaY, 6);
        Assert.Equal(Math.Sqrt((expectedDeltaX * expectedDeltaX) + (expectedDeltaY * expectedDeltaY)),
            measurement.Length,
            6);
    }

    [Fact]
    public void LabelAndClipboard_UsesInvariantReadableValues()
    {
        var measurement = new PixelMeasurement(
            new ImagePoint(10, 10),
            new ImagePoint(110, 76));

        var label = MeasurementFormatting.FormatLabel(measurement);
        var clipboard = MeasurementFormatting.FormatClipboard([measurement]);

        Assert.Equal("119.82px", label);
        Assert.Equal(
            "1. L 119.82 px | ΔX 100 | ΔY 66 | (10, 10) → (110, 76)",
            clipboard);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void MeasurementLength_RemainsAccurateAfterZoomAndPan(double scale)
    {
        var transform = new ImageCanvasTransform();
        transform.SetViewport(800, 600);
        transform.SetImage(new ImageSize(1000, 750));
        transform.ZoomTo(scale, new CanvasPoint(400, 300));
        transform.PanBy(-73, 41);
        var expected = new PixelMeasurement(
            new ImagePoint(10, 10),
            new ImagePoint(110, 76));

        var startOnCanvas = transform.ImageToCanvas(expected.Start);
        var endOnCanvas = transform.ImageToCanvas(expected.End);
        Assert.True(transform.TryCanvasToImage(startOnCanvas, out var start));
        Assert.True(transform.TryCanvasToImage(endOnCanvas, out var end));
        var roundTrip = new PixelMeasurement(start, end);

        Assert.Equal(expected.DeltaX, roundTrip.DeltaX, 6);
        Assert.Equal(expected.DeltaY, roundTrip.DeltaY, 6);
        Assert.Equal(expected.Length, roundTrip.Length, 6);
    }
}
