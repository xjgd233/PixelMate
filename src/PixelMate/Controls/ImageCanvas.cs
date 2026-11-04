using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PixelMate.Imaging;

namespace PixelMate.Controls;

public sealed class ImageCanvas : Control
{
    private static readonly IBrush BackgroundBrush = new SolidColorBrush(Color.Parse("#0B0B0D"));
    private static readonly IBrush CheckerLightBrush = new SolidColorBrush(Color.Parse("#202024"));
    private static readonly IBrush CheckerDarkBrush = new SolidColorBrush(Color.Parse("#18181B"));
    private static readonly IBrush ImageBorderBrush = new SolidColorBrush(Color.Parse("#4B4B52"));
    private static readonly IPen ImageBorderPen = new Pen(ImageBorderBrush, 1);
    private static readonly Cursor PanningCursor = new(StandardCursorType.SizeAll);

    private Bitmap? _bitmap;
    private bool _isPanning;
    private Point _lastPointerPosition;
    private int _lastPixelX = -1;
    private int _lastPixelY = -1;
    private bool _lastPixelInside;

    public ImageCanvas()
    {
        Focusable = true;
        ClipToBounds = true;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
    }

    public event EventHandler? ViewChanged;

    public event EventHandler<ImagePointerEventArgs>? PointerImageChanged;

    public ImageCanvasTransform Transform { get; } = new();

    public void SetImage(Bitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        var previous = _bitmap;
        _bitmap = bitmap;
        _lastPixelInside = false;
        _lastPixelX = -1;
        _lastPixelY = -1;
        Transform.SetImage(new ImageSize(bitmap.PixelSize.Width, bitmap.PixelSize.Height));

        if (Transform.IsFitToViewport)
        {
            Transform.SetViewport(Bounds.Width, Bounds.Height);
        }

        previous?.Dispose();
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ZoomBy(double factor)
    {
        Transform.ZoomBy(factor, new CanvasPoint(Bounds.Width / 2, Bounds.Height / 2));
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    public void FitToViewport()
    {
        Transform.FitToViewport();
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ShowActualSize()
    {
        Transform.ShowActualSize();
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(BackgroundBrush, Bounds);

        if (_bitmap is null || !Transform.HasImage)
        {
            return;
        }

        var imageRect = new Rect(
            Transform.OffsetX,
            Transform.OffsetY,
            Transform.ScaledImageWidth,
            Transform.ScaledImageHeight);
        DrawCheckerboard(context, imageRect.Intersect(Bounds));

        using (context.PushClip(Bounds))
        {
            context.DrawImage(
                _bitmap,
                new Rect(_bitmap.Size),
                imageRect);
            context.DrawRectangle(null, ImageBorderPen, imageRect);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Transform.SetViewport(e.NewSize.Width, e.NewSize.Height);
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsMiddleButtonPressed && _bitmap is not null)
        {
            _isPanning = true;
            _lastPointerPosition = point.Position;
            e.Pointer.Capture(this);
            Cursor = PanningCursor;
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var position = e.GetPosition(this);
        if (_isPanning)
        {
            var delta = position - _lastPointerPosition;
            _lastPointerPosition = position;
            Transform.PanBy(delta.X, delta.Y);
            InvalidateVisual();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        ReportPointerPosition(position);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
            Cursor = Cursor.Default;
            e.Handled = true;
            ReportPointerPosition(e.GetPosition(this));
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _lastPixelInside = false;
        PointerImageChanged?.Invoke(
            this,
            new ImagePointerEventArgs(_lastPixelX, _lastPixelY, false));
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (_bitmap is null || Math.Abs(e.Delta.Y) < 0.000001)
        {
            return;
        }

        var position = e.GetPosition(this);
        var zoomSteps = Math.Clamp(e.Delta.Y, -2, 2);
        var factor = Math.Pow(1.12, zoomSteps);
        Transform.ZoomBy(factor, new CanvasPoint(position.X, position.Y));
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
        ReportPointerPosition(position);
        e.Handled = true;
    }

    public void DisposeImage()
    {
        var bitmap = _bitmap;
        _bitmap = null;
        bitmap?.Dispose();
    }

    private void ReportPointerPosition(Point position)
    {
        var inside = Transform.TryCanvasToImagePixel(
            new CanvasPoint(position.X, position.Y),
            out var pixelX,
            out var pixelY);

        if (inside == _lastPixelInside && pixelX == _lastPixelX && pixelY == _lastPixelY)
        {
            return;
        }

        _lastPixelInside = inside;
        _lastPixelX = pixelX;
        _lastPixelY = pixelY;
        PointerImageChanged?.Invoke(
            this,
            new ImagePointerEventArgs(pixelX, pixelY, inside));
    }

    private static void DrawCheckerboard(DrawingContext context, Rect clip)
    {
        if (clip.Width <= 0 || clip.Height <= 0)
        {
            return;
        }

        const double cellSize = 12;
        context.FillRectangle(CheckerDarkBrush, clip);

        var startColumn = (int)Math.Floor(clip.X / cellSize);
        var endColumn = (int)Math.Ceiling(clip.Right / cellSize);
        var startRow = (int)Math.Floor(clip.Y / cellSize);
        var endRow = (int)Math.Ceiling(clip.Bottom / cellSize);

        for (var row = startRow; row < endRow; row++)
        {
            for (var column = startColumn; column < endColumn; column++)
            {
                if ((row + column) % 2 == 0)
                {
                    continue;
                }

                var cell = new Rect(
                    column * cellSize,
                    row * cellSize,
                    cellSize,
                    cellSize).Intersect(clip);
                context.FillRectangle(CheckerLightBrush, cell);
            }
        }
    }
}

public sealed class ImagePointerEventArgs(int pixelX, int pixelY, bool isInside) : EventArgs
{
    public int PixelX { get; } = pixelX;

    public int PixelY { get; } = pixelY;

    public bool IsInside { get; } = isInside;
}
