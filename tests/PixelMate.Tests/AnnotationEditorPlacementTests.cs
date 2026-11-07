using PixelMate.Annotations;
using PixelMate.Imaging;

namespace PixelMate.Tests;

public sealed class AnnotationEditorPlacementTests
{
    [Fact]
    public void Calculate_PrefersRightAndBelowAnchorWhenSpaceIsAvailable()
    {
        var position = AnnotationEditorPlacement.Calculate(
            new CanvasPoint(100, 80),
            viewportWidth: 800,
            viewportHeight: 600,
            editorWidth: 320,
            editorHeight: 150);

        Assert.Equal(new CanvasPoint(112, 92), position);
    }

    [Fact]
    public void Calculate_FlipsLeftWhenRightSideWouldOverflow()
    {
        var position = AnnotationEditorPlacement.Calculate(
            new CanvasPoint(720, 80),
            viewportWidth: 800,
            viewportHeight: 600,
            editorWidth: 320,
            editorHeight: 150);

        Assert.Equal(new CanvasPoint(388, 92), position);
    }

    [Fact]
    public void Calculate_FlipsAboveWhenBottomWouldOverflow()
    {
        var position = AnnotationEditorPlacement.Calculate(
            new CanvasPoint(100, 520),
            viewportWidth: 800,
            viewportHeight: 600,
            editorWidth: 320,
            editorHeight: 150);

        Assert.Equal(new CanvasPoint(112, 358), position);
    }

    [Fact]
    public void Calculate_KeepsTopLeftMarginWhenViewportIsSmallerThanEditor()
    {
        var position = AnnotationEditorPlacement.Calculate(
            new CanvasPoint(150, 100),
            viewportWidth: 300,
            viewportHeight: 200,
            editorWidth: 320,
            editorHeight: 150);

        Assert.Equal(new CanvasPoint(8, 8), position);
    }
}
