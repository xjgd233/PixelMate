namespace PixelMate.Imaging;

public readonly record struct ImageSize(double Width, double Height);

public readonly record struct ImagePoint(double X, double Y);

public readonly record struct CanvasPoint(double X, double Y);

public sealed class ImageCanvasTransform
{
    public const double MinimumScale = 0.01;
    public const double MaximumScale = 64;

    private const double FitMargin = 24;

    private ImageSize _imageSize;
    private double _viewportWidth;
    private double _viewportHeight;

    public double Scale { get; private set; } = 1;

    public double OffsetX { get; private set; }

    public double OffsetY { get; private set; }

    public bool IsFitToViewport { get; private set; }

    public bool HasImage => _imageSize.Width > 0 && _imageSize.Height > 0;

    public ImageSize ImageSize => _imageSize;

    public double ScaledImageWidth => _imageSize.Width * Scale;

    public double ScaledImageHeight => _imageSize.Height * Scale;

    public void SetImage(ImageSize imageSize)
    {
        if (imageSize.Width <= 0 || imageSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), "图片尺寸必须大于零。");
        }

        _imageSize = imageSize;
        FitToViewport();
    }

    public void SetViewport(double width, double height)
    {
        _viewportWidth = Math.Max(0, width);
        _viewportHeight = Math.Max(0, height);

        if (!HasImage || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        if (IsFitToViewport)
        {
            FitToViewport();
            return;
        }

        ClampOffset();
    }

    public void FitToViewport()
    {
        if (!HasImage || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        var usableWidth = Math.Max(1, _viewportWidth - (FitMargin * 2));
        var usableHeight = Math.Max(1, _viewportHeight - (FitMargin * 2));
        Scale = Math.Clamp(
            Math.Min(usableWidth / _imageSize.Width, usableHeight / _imageSize.Height),
            MinimumScale,
            MaximumScale);
        IsFitToViewport = true;
        CenterImage();
    }

    public void ShowActualSize()
    {
        if (!HasImage || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        Scale = 1;
        IsFitToViewport = false;
        CenterImage();
    }

    public void ZoomBy(double factor, CanvasPoint anchor)
    {
        if (!HasImage || factor <= 0)
        {
            return;
        }

        ZoomTo(Scale * factor, anchor);
    }

    public void ZoomTo(double requestedScale, CanvasPoint anchor)
    {
        if (!HasImage || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        if (!TryCanvasToImage(anchor, out var imageAnchor))
        {
            imageAnchor = new ImagePoint(_imageSize.Width / 2, _imageSize.Height / 2);
        }

        var nextScale = Math.Clamp(requestedScale, MinimumScale, MaximumScale);
        if (Math.Abs(nextScale - Scale) < 0.000001)
        {
            return;
        }

        Scale = nextScale;
        OffsetX = anchor.X - (imageAnchor.X * Scale);
        OffsetY = anchor.Y - (imageAnchor.Y * Scale);
        IsFitToViewport = false;
        ClampOffset();
    }

    public void PanBy(double deltaX, double deltaY)
    {
        if (!HasImage || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        OffsetX += deltaX;
        OffsetY += deltaY;
        IsFitToViewport = false;
        ClampOffset();
    }

    public bool TryCanvasToImage(CanvasPoint canvasPoint, out ImagePoint imagePoint)
    {
        imagePoint = default;

        if (!HasImage || Scale <= 0)
        {
            return false;
        }

        var x = (canvasPoint.X - OffsetX) / Scale;
        var y = (canvasPoint.Y - OffsetY) / Scale;
        if (x < 0 || y < 0 || x >= _imageSize.Width || y >= _imageSize.Height)
        {
            return false;
        }

        imagePoint = new ImagePoint(x, y);
        return true;
    }

    public bool TryCanvasToImagePixel(CanvasPoint canvasPoint, out int x, out int y)
    {
        x = 0;
        y = 0;

        if (!TryCanvasToImage(canvasPoint, out var imagePoint))
        {
            return false;
        }

        x = Math.Min((int)Math.Floor(imagePoint.X), (int)_imageSize.Width - 1);
        y = Math.Min((int)Math.Floor(imagePoint.Y), (int)_imageSize.Height - 1);
        return true;
    }

    public CanvasPoint ImageToCanvas(ImagePoint imagePoint)
    {
        return new CanvasPoint(
            OffsetX + (imagePoint.X * Scale),
            OffsetY + (imagePoint.Y * Scale));
    }

    private void CenterImage()
    {
        OffsetX = (_viewportWidth - ScaledImageWidth) / 2;
        OffsetY = (_viewportHeight - ScaledImageHeight) / 2;
        ClampOffset();
    }

    private void ClampOffset()
    {
        if (!HasImage || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        OffsetX = ClampAxis(OffsetX, ScaledImageWidth, _viewportWidth);
        OffsetY = ClampAxis(OffsetY, ScaledImageHeight, _viewportHeight);
    }

    private static double ClampAxis(double offset, double contentLength, double viewportLength)
    {
        if (contentLength <= viewportLength)
        {
            return (viewportLength - contentLength) / 2;
        }

        return Math.Clamp(offset, viewportLength - contentLength, 0);
    }
}
