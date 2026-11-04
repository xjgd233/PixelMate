using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PixelMate.Imaging;
using PixelMate.Measurement;
using System.Globalization;

namespace PixelMate.Controls;

public sealed class ImageCanvas : Control
{
    private static readonly IBrush BackgroundBrush = new SolidColorBrush(Color.Parse("#0B0B0D"));
    private static readonly IBrush CheckerLightBrush = new SolidColorBrush(Color.Parse("#202024"));
    private static readonly IBrush CheckerDarkBrush = new SolidColorBrush(Color.Parse("#18181B"));
    private static readonly IBrush ImageBorderBrush = new SolidColorBrush(Color.Parse("#4B4B52"));
    private static readonly IPen ImageBorderPen = new Pen(ImageBorderBrush, 1);
    private static readonly IBrush MeasurementBrush = new SolidColorBrush(Color.Parse("#FFB454"));
    private static readonly IBrush MeasurementShadowBrush = new SolidColorBrush(Color.Parse("#D908080A"));
    private static readonly IBrush MeasurementEndpointInnerBrush = new SolidColorBrush(Color.Parse("#FFE0A0"));
    private static readonly IBrush MeasurementLabelBackgroundBrush = new SolidColorBrush(Color.Parse("#F01A1A1E"));
    private static readonly IBrush MeasurementLabelTextBrush = new SolidColorBrush(Color.Parse("#FFF7EA"));
    private static readonly IPen MeasurementShadowPen = new Pen(MeasurementShadowBrush, 4);
    private static readonly IPen MeasurementPen = new Pen(MeasurementBrush, 1.6);
    private static readonly IPen MeasurementLabelBorderPen =
        new Pen(new SolidColorBrush(Color.Parse("#995E5140")), 1);
    private static readonly Cursor PanningCursor = new(StandardCursorType.SizeAll);
    private static readonly Cursor MeasuringCursor = new(StandardCursorType.Cross);
    private static readonly Typeface MeasurementTypeface =
        new(new FontFamily("SF Mono, Menlo, Consolas, monospace"));

    private Bitmap? _bitmap;
    private bool _isPanning;
    private bool _isMeasurementEnabled;
    private bool _isMeasuring;
    private IPointer? _measurementPointer;
    private ImagePoint _measurementStart;
    private PixelMeasurement? _measurementDraft;
    private Point _lastPointerPosition;
    private int _lastPixelX = -1;
    private int _lastPixelY = -1;
    private bool _lastPixelInside;
    private readonly List<PixelMeasurement> _measurements = [];

    public ImageCanvas()
    {
        Focusable = true;
        ClipToBounds = true;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
    }

    public event EventHandler? ViewChanged;

    public event EventHandler<ImagePointerEventArgs>? PointerImageChanged;

    public event EventHandler? MeasurementsChanged;

    public ImageCanvasTransform Transform { get; } = new();

    public IReadOnlyList<PixelMeasurement> Measurements => _measurements;

    public bool HasMeasurements => _measurements.Count > 0;

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

        ClearMeasurements();
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

    public void SetMeasurementEnabled(bool isEnabled)
    {
        if (_isMeasurementEnabled == isEnabled)
        {
            return;
        }

        _isMeasurementEnabled = isEnabled;
        if (!isEnabled)
        {
            CancelMeasurementDraft();
        }

        UpdateCursor();
    }

    public void ClearMeasurements()
    {
        var hadMeasurements = _measurements.Count > 0 || _measurementDraft is not null;
        _measurements.Clear();
        _isMeasuring = false;
        _measurementDraft = null;
        _measurementPointer?.Capture(null);
        _measurementPointer = null;

        if (hadMeasurements)
        {
            InvalidateVisual();
            MeasurementsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void CancelMeasurementDraft()
    {
        if (!_isMeasuring && _measurementDraft is null)
        {
            return;
        }

        _isMeasuring = false;
        _measurementDraft = null;
        _measurementPointer?.Capture(null);
        _measurementPointer = null;
        UpdateCursor();
        InvalidateVisual();
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

            foreach (var measurement in _measurements)
            {
                DrawMeasurement(context, measurement);
            }

            if (_measurementDraft is { } draft)
            {
                DrawMeasurement(context, draft);
            }
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
            return;
        }

        if (_isMeasurementEnabled
            && point.Properties.IsLeftButtonPressed
            && Transform.TryCanvasToImage(
                new CanvasPoint(point.Position.X, point.Position.Y),
                out var start))
        {
            _isMeasuring = true;
            _measurementPointer = e.Pointer;
            _measurementStart = start;
            _measurementDraft = new PixelMeasurement(start, start);
            e.Pointer.Capture(this);
            InvalidateVisual();
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
        else if (_isMeasuring)
        {
            _measurementDraft = new PixelMeasurement(
                _measurementStart,
                Transform.CanvasToImageClamped(new CanvasPoint(position.X, position.Y)));
            InvalidateVisual();
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
            UpdateCursor();
            e.Handled = true;
            ReportPointerPosition(e.GetPosition(this));
            return;
        }

        if (_isMeasuring && e.InitialPressMouseButton == MouseButton.Left)
        {
            var end = Transform.CanvasToImageClamped(
                new CanvasPoint(e.GetPosition(this).X, e.GetPosition(this).Y));
            var measurement = new PixelMeasurement(_measurementStart, end);

            _isMeasuring = false;
            _measurementDraft = null;
            _measurementPointer = null;
            e.Pointer.Capture(null);
            UpdateCursor();

            if (measurement.Length > 0.001)
            {
                _measurements.Add(measurement);
                MeasurementsChanged?.Invoke(this, EventArgs.Empty);
            }

            InvalidateVisual();
            ReportPointerPosition(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_isPanning)
        {
            _isPanning = false;
            UpdateCursor();
        }

        if (_isMeasuring)
        {
            CancelMeasurementDraft();
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
        ClearMeasurements();
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

    private void DrawMeasurement(DrawingContext context, PixelMeasurement measurement)
    {
        var start = Transform.ImageToCanvas(measurement.Start);
        var end = Transform.ImageToCanvas(measurement.End);
        var startPoint = new Point(start.X, start.Y);
        var endPoint = new Point(end.X, end.Y);

        context.DrawLine(MeasurementShadowPen, startPoint, endPoint);
        context.DrawLine(MeasurementPen, startPoint, endPoint);
        DrawEndpoint(context, startPoint);
        DrawEndpoint(context, endPoint);
        DrawMeasurementLabel(context, measurement, startPoint, endPoint);
    }

    private static void DrawEndpoint(DrawingContext context, Point point)
    {
        context.DrawEllipse(MeasurementShadowBrush, null, point, 5.5, 5.5);
        context.DrawEllipse(MeasurementEndpointInnerBrush, null, point, 3.2, 3.2);
    }

    private void DrawMeasurementLabel(
        DrawingContext context,
        PixelMeasurement measurement,
        Point start,
        Point end)
    {
        var text = new FormattedText(
            MeasurementFormatting.FormatLabel(measurement),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            11,
            MeasurementLabelTextBrush);

        const double horizontalPadding = 8;
        const double verticalPadding = 4;
        var width = text.WidthIncludingTrailingWhitespace + (horizontalPadding * 2);
        var height = text.Height + (verticalPadding * 2);

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var lineLength = Math.Sqrt((dx * dx) + (dy * dy));
        var normalX = lineLength > 0.001 ? -dy / lineLength : 0;
        var normalY = lineLength > 0.001 ? dx / lineLength : -1;
        var middleX = (start.X + end.X) / 2;
        var middleY = (start.Y + end.Y) / 2;
        const double labelOffset = 20;

        var left = middleX + (normalX * labelOffset) - (width / 2);
        var top = middleY + (normalY * labelOffset) - (height / 2);
        left = Math.Clamp(left, 6, Math.Max(6, Bounds.Width - width - 6));
        top = Math.Clamp(top, 6, Math.Max(6, Bounds.Height - height - 6));

        var labelRect = new Rect(left, top, width, height);
        context.DrawRectangle(
            MeasurementLabelBackgroundBrush,
            MeasurementLabelBorderPen,
            labelRect,
            5,
            5);
        context.DrawText(
            text,
            new Point(labelRect.X + horizontalPadding, labelRect.Y + verticalPadding));
    }

    private void UpdateCursor()
    {
        Cursor = _isPanning
            ? PanningCursor
            : _isMeasurementEnabled && _bitmap is not null
                ? MeasuringCursor
                : Cursor.Default;
    }
}

public sealed class ImagePointerEventArgs(int pixelX, int pixelY, bool isInside) : EventArgs
{
    public int PixelX { get; } = pixelX;

    public int PixelY { get; } = pixelY;

    public bool IsInside { get; } = isInside;
}
