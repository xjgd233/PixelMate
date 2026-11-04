namespace PixelMate.Magnifier;

public readonly record struct MagnifierLayoutRect(
    double X,
    double Y,
    double Width,
    double Height)
{
    public double Right => X + Width;

    public double Bottom => Y + Height;
}

public readonly record struct MagnifierPixelRegion(
    int X,
    int Y,
    int Width,
    int Height);

public readonly record struct MagnifierLayout(
    MagnifierLayoutRect CardBounds,
    MagnifierLayoutRect ViewportBounds,
    int SampleStartPixelX,
    int SampleStartPixelY,
    int SamplePixelCount,
    MagnifierLayoutRect SampleBounds,
    MagnifierPixelRegion SourceRegion,
    MagnifierLayoutRect SourceDestinationBounds,
    MagnifierLayoutRect CurrentPixelBounds,
    double CrosshairX,
    double CrosshairY);

public static class MagnifierLayoutCalculator
{
    public const double ViewportSize = 192;
    public const double CardPadding = 6;
    public const double FooterHeight = 27;
    public const double CardWidth = ViewportSize + (CardPadding * 2);
    public const double CardHeight = ViewportSize + FooterHeight + (CardPadding * 2);
    public const double PointerGap = 18;
    public const double ViewportMargin = 6;

    public static int GetSamplePixelCount(int magnification)
    {
        if (magnification <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(magnification),
                "放大倍数必须大于零。");
        }

        var viewportPixels = Math.Max(1, (int)Math.Floor(ViewportSize / magnification));
        return viewportPixels % 2 == 0
            ? Math.Max(1, viewportPixels - 1)
            : viewportPixels;
    }

    public static MagnifierLayout Create(
        double canvasWidth,
        double canvasHeight,
        double pointerX,
        double pointerY,
        int pixelX,
        int pixelY,
        int imageWidth,
        int imageHeight,
        int magnification)
    {
        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(canvasWidth),
                "画布尺寸必须大于零。");
        }

        if (imageWidth <= 0 || imageHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(imageWidth),
                "图片尺寸必须大于零。");
        }

        if (pixelX < 0 || pixelX >= imageWidth || pixelY < 0 || pixelY >= imageHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pixelX),
                "当前像素必须位于图片范围内。");
        }

        var samplePixelCount = GetSamplePixelCount(magnification);
        var sampleRadius = samplePixelCount / 2;
        var sampleStartPixelX = pixelX - sampleRadius;
        var sampleStartPixelY = pixelY - sampleRadius;
        var sourceLeft = Math.Max(0, sampleStartPixelX);
        var sourceTop = Math.Max(0, sampleStartPixelY);
        var sourceRight = Math.Min(imageWidth, sampleStartPixelX + samplePixelCount);
        var sourceBottom = Math.Min(imageHeight, sampleStartPixelY + samplePixelCount);

        var cardLeft = pointerX + PointerGap;
        var cardTop = pointerY + PointerGap;
        if (cardLeft + CardWidth > canvasWidth - ViewportMargin)
        {
            cardLeft = pointerX - CardWidth - PointerGap;
        }

        if (cardTop + CardHeight > canvasHeight - ViewportMargin)
        {
            cardTop = pointerY - CardHeight - PointerGap;
        }

        cardLeft = Math.Clamp(
            cardLeft,
            ViewportMargin,
            Math.Max(ViewportMargin, canvasWidth - CardWidth - ViewportMargin));
        cardTop = Math.Clamp(
            cardTop,
            ViewportMargin,
            Math.Max(ViewportMargin, canvasHeight - CardHeight - ViewportMargin));

        var viewportLeft = cardLeft + CardPadding;
        var viewportTop = cardTop + CardPadding;
        var renderedSampleSize = samplePixelCount * magnification;
        var sampleLeft = viewportLeft + ((ViewportSize - renderedSampleSize) / 2);
        var sampleTop = viewportTop + ((ViewportSize - renderedSampleSize) / 2);
        var sourceDestinationBounds = new MagnifierLayoutRect(
            sampleLeft + ((sourceLeft - sampleStartPixelX) * magnification),
            sampleTop + ((sourceTop - sampleStartPixelY) * magnification),
            (sourceRight - sourceLeft) * magnification,
            (sourceBottom - sourceTop) * magnification);
        var currentPixelBounds = new MagnifierLayoutRect(
            sampleLeft + ((pixelX - sampleStartPixelX) * magnification),
            sampleTop + ((pixelY - sampleStartPixelY) * magnification),
            magnification,
            magnification);
        var sampleBounds = new MagnifierLayoutRect(
            sampleLeft,
            sampleTop,
            renderedSampleSize,
            renderedSampleSize);

        return new MagnifierLayout(
            new MagnifierLayoutRect(cardLeft, cardTop, CardWidth, CardHeight),
            new MagnifierLayoutRect(viewportLeft, viewportTop, ViewportSize, ViewportSize),
            sampleStartPixelX,
            sampleStartPixelY,
            samplePixelCount,
            sampleBounds,
            new MagnifierPixelRegion(
                sourceLeft,
                sourceTop,
                sourceRight - sourceLeft,
                sourceBottom - sourceTop),
            sourceDestinationBounds,
            currentPixelBounds,
            viewportLeft + (ViewportSize / 2),
            viewportTop + (ViewportSize / 2));
    }
}
