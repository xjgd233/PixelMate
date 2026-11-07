using PixelMate.Imaging;

namespace PixelMate.Annotations;

public static class AnnotationEditorPlacement
{
    private const double AnchorGap = 12;
    private const double ViewportMargin = 8;

    public static CanvasPoint Calculate(
        CanvasPoint anchor,
        double viewportWidth,
        double viewportHeight,
        double editorWidth,
        double editorHeight)
    {
        var x = anchor.X + AnchorGap;
        if (x + editorWidth > viewportWidth - ViewportMargin)
        {
            x = anchor.X - AnchorGap - editorWidth;
        }

        var y = anchor.Y + AnchorGap;
        if (y + editorHeight > viewportHeight - ViewportMargin)
        {
            y = anchor.Y - AnchorGap - editorHeight;
        }

        var maxX = Math.Max(ViewportMargin, viewportWidth - editorWidth - ViewportMargin);
        var maxY = Math.Max(ViewportMargin, viewportHeight - editorHeight - ViewportMargin);
        return new CanvasPoint(
            Math.Clamp(x, ViewportMargin, maxX),
            Math.Clamp(y, ViewportMargin, maxY));
    }
}
