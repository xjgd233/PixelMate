using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixelMate.Annotations;
using PixelMate.Colors;
using PixelMate.Imaging;
using PixelMate.Magnifier;
using PixelMate.Measurement;
using System.Globalization;
using System.Runtime.InteropServices;

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
    private static readonly IBrush PickerBackgroundBrush = new SolidColorBrush(Color.Parse("#F01A1A1E"));
    private static readonly IBrush PickerTextBrush = new SolidColorBrush(Color.Parse("#F5F5F7"));
    private static readonly IBrush PickerMutedTextBrush = new SolidColorBrush(Color.Parse("#AFAFB6"));
    private static readonly IPen PickerBorderPen =
        new Pen(new SolidColorBrush(Color.Parse("#5A5A62")), 1);
    private static readonly IBrush MagnifierBackgroundBrush =
        new SolidColorBrush(Color.Parse("#F21A1A1E"));
    private static readonly IBrush MagnifierViewportBackgroundBrush =
        new SolidColorBrush(Color.Parse("#121214"));
    private static readonly IBrush MagnifierTextBrush =
        new SolidColorBrush(Color.Parse("#F5F5F7"));
    private static readonly IBrush MagnifierMutedTextBrush =
        new SolidColorBrush(Color.Parse("#AFAFB6"));
    private static readonly IPen MagnifierBorderPen =
        new Pen(new SolidColorBrush(Color.Parse("#66666E")), 1);
    private static readonly IPen MagnifierDividerPen =
        new Pen(new SolidColorBrush(Color.Parse("#3D3D43")), 1);
    private static readonly IPen MagnifierGridPen =
        new Pen(new SolidColorBrush(Color.Parse("#55FFFFFF")), 0.55);
    private static readonly IPen MagnifierCurrentPixelShadowPen =
        new Pen(new SolidColorBrush(Color.Parse("#B0000000")), 4);
    private static readonly IPen MagnifierCurrentPixelPen =
        new Pen(new SolidColorBrush(Color.Parse("#52A7FF")), 2);
    private static readonly IPen MagnifierCrosshairShadowPen =
        new Pen(new SolidColorBrush(Color.Parse("#B0000000")), 3);
    private static readonly IPen MagnifierCrosshairPen =
        new Pen(new SolidColorBrush(Color.Parse("#F7FAFF")), 1);
    private static readonly IPen MeasurementShadowPen = new Pen(MeasurementShadowBrush, 2.8);
    private static readonly IPen MeasurementPen = new Pen(MeasurementBrush, 1.1);
    private static readonly IPen MeasurementLabelBorderPen =
        new Pen(new SolidColorBrush(Color.Parse("#995E5140")), 1);
    private static readonly IBrush AnnotationBrush =
        new SolidColorBrush(Color.Parse("#63E6BE"));
    private static readonly IBrush AnnotationInnerBrush =
        new SolidColorBrush(Color.Parse("#F2FFFC"));
    private static readonly IBrush AnnotationLabelBackgroundBrush =
        new SolidColorBrush(Color.Parse("#F0121A18"));
    private static readonly IBrush AnnotationLabelTextBrush =
        new SolidColorBrush(Color.Parse("#EFFFFB"));
    private static readonly IPen AnnotationLeaderPen =
        new Pen(new SolidColorBrush(Color.Parse("#CC63E6BE")), 1.2);
    private static readonly IPen AnnotationBorderPen =
        new Pen(new SolidColorBrush(Color.Parse("#6663E6BE")), 1);
    private static readonly Cursor PanReadyCursor = new(StandardCursorType.Hand);
    private static readonly Cursor PanningCursor = new(StandardCursorType.SizeAll);
    private static readonly Cursor MeasuringCursor = new(StandardCursorType.Cross);
    private static readonly Typeface MeasurementTypeface =
        new(AppTypography.SystemFontFamily);

    private readonly SolidColorBrush _eyedropperSwatchBrush =
        new(Avalonia.Media.Colors.Transparent);
    private Bitmap? _bitmap;
    private ImagePixelBuffer? _pixels;
    private bool _isPanning;
    private bool _isTemporaryPanEnabled;
    private bool _isMeasurementEnabled;
    private bool _isEyedropperEnabled;
    private bool _isMagnifierEnabled;
    private bool _isAnnotationEnabled;
    private bool _isMeasuring;
    private int _magnifierScale = 12;
    private IPointer? _measurementPointer;
    private ImagePoint _measurementStart;
    private PixelMeasurement? _measurementDraft;
    private ImagePoint? _annotationDraftAnchor;
    private Point _lastPointerPosition;
    private int _lastPixelX = -1;
    private int _lastPixelY = -1;
    private bool _lastPixelInside;
    private PixelColor? _sampledColor;
    private Point? _lastPointerCanvasPosition;
    private readonly List<PixelMeasurement> _measurements = [];
    private readonly List<PixelAnnotation> _annotations = [];

    public ImageCanvas()
    {
        Focusable = true;
        ClipToBounds = true;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
    }

    public event EventHandler? ViewChanged;

    public event EventHandler<ImagePointerEventArgs>? PointerImageChanged;

    public event EventHandler? MeasurementsChanged;

    public event EventHandler<ColorPickedEventArgs>? ColorPicked;

    public event EventHandler<AnnotationRequestedEventArgs>? AnnotationRequested;

    public event EventHandler? AnnotationsChanged;

    public ImageCanvasTransform Transform { get; } = new();

    public IReadOnlyList<PixelMeasurement> Measurements => _measurements;

    public bool HasMeasurements => _measurements.Count > 0;

    public IReadOnlyList<PixelAnnotation> Annotations => _annotations;

    public bool HasAnnotations => _annotations.Count > 0;

    public int MagnifierScale => _magnifierScale;

    internal ImageSize SetImage(DecodedOriginalImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        WriteableBitmap? bitmap = null;
        ImagePixelBuffer pixels;
        try
        {
            bitmap = new WriteableBitmap(
                new PixelSize(image.Width, image.Height),
                new Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Unpremul);
            using var framebuffer = bitmap.Lock();
            if (framebuffer.Format != PixelFormat.Bgra8888)
            {
                throw new NotSupportedException("当前渲染后端不支持 BGRA8888 像素显示。");
            }

            var packedRowBytes = checked(image.Width * 4);
            if (framebuffer.RowBytes < packedRowBytes)
            {
                throw new NotSupportedException("当前渲染后端的位图行宽无效。");
            }

            for (var row = 0; row < image.Height; row++)
            {
                Marshal.Copy(
                    image.BgraPixels,
                    row * image.RowBytes,
                    IntPtr.Add(framebuffer.Address, row * framebuffer.RowBytes),
                    packedRowBytes);
            }

            pixels = ImagePixelBuffer.FromOwnedBgra(
                image.Width,
                image.Height,
                image.BgraPixels,
                image.RowBytes);
        }
        catch
        {
            bitmap?.Dispose();
            throw;
        }

        var previous = _bitmap;
        _bitmap = bitmap;
        _pixels = pixels;
        _lastPixelInside = false;
        _lastPixelX = -1;
        _lastPixelY = -1;
        _sampledColor = null;
        _lastPointerCanvasPosition = null;
        var imageSize = new ImageSize(image.Width, image.Height);
        Transform.SetImage(imageSize);

        if (Transform.IsFitToViewport)
        {
            Transform.SetViewport(Bounds.Width, Bounds.Height);
        }

        ClearMeasurements();
        ClearAnnotations();
        previous?.Dispose();
        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
        return imageSize;
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

    public void SetEyedropperEnabled(bool isEnabled)
    {
        if (_isEyedropperEnabled == isEnabled)
        {
            return;
        }

        _isEyedropperEnabled = isEnabled;
        UpdateCursor();

        if (isEnabled && _lastPointerCanvasPosition is { } position)
        {
            ReportPointerPosition(position);
        }
        else if (!isEnabled && _sampledColor is not null)
        {
            _sampledColor = null;
            InvalidateVisual();
            PointerImageChanged?.Invoke(
                this,
                new ImagePointerEventArgs(
                    _lastPixelX,
                    _lastPixelY,
                    _lastPixelInside,
                    null));
        }
    }

    public void SetMagnifierEnabled(bool isEnabled)
    {
        if (_isMagnifierEnabled == isEnabled)
        {
            return;
        }

        _isMagnifierEnabled = isEnabled;
        UpdateCursor();

        if (isEnabled && _lastPointerCanvasPosition is { } position)
        {
            ReportPointerPosition(position);
        }
        else
        {
            InvalidateVisual();
        }
    }

    public void SetMagnifierScale(int scale)
    {
        if (scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "放大倍数必须大于零。");
        }

        if (_magnifierScale == scale)
        {
            return;
        }

        _magnifierScale = scale;
        InvalidateVisual();
    }

    public void SetAnnotationEnabled(bool isEnabled)
    {
        if (_isAnnotationEnabled == isEnabled)
        {
            return;
        }

        _isAnnotationEnabled = isEnabled;
        if (!isEnabled)
        {
            CancelAnnotationDraft();
        }

        UpdateCursor();
    }

    public void SetTemporaryPanEnabled(bool isEnabled)
    {
        if (_isTemporaryPanEnabled == isEnabled)
        {
            return;
        }

        _isTemporaryPanEnabled = isEnabled;
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

    public void CommitAnnotation(string text)
    {
        if (_annotationDraftAnchor is not { } anchor)
        {
            return;
        }

        var annotation = PixelAnnotation.Create(anchor, text);
        _annotations.Add(annotation);
        _annotationDraftAnchor = null;
        InvalidateVisual();
        AnnotationsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CancelAnnotationDraft()
    {
        if (_annotationDraftAnchor is null)
        {
            return;
        }

        _annotationDraftAnchor = null;
        InvalidateVisual();
    }

    public void ClearAnnotations()
    {
        var hadAnnotations = _annotations.Count > 0 || _annotationDraftAnchor is not null;
        _annotations.Clear();
        _annotationDraftAnchor = null;
        if (hadAnnotations)
        {
            InvalidateVisual();
            AnnotationsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SaveEditedImage(Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (_bitmap is null || _pixels is null)
        {
            throw new InvalidOperationException("当前没有可保存的图片。");
        }

        var width = _pixels.Width;
        var height = _pixels.Height;
        var imageBounds = new Rect(0, 0, width, height);
        var overlayScale = EditedImageExport.CalculateOverlayScale(width, height);
        using var renderTarget = new RenderTargetBitmap(
            new PixelSize(width, height),
            new Vector(96, 96));
        using (var context = renderTarget.CreateDrawingContext())
        {
            context.DrawImage(
                _bitmap,
                new Rect(_bitmap.Size),
                imageBounds);

            foreach (var measurement in _measurements)
            {
                DrawExportMeasurement(
                    context,
                    measurement,
                    imageBounds,
                    overlayScale);
            }

            foreach (var annotation in _annotations)
            {
                DrawExportAnnotation(
                    context,
                    annotation,
                    imageBounds,
                    overlayScale);
            }
        }

        renderTarget.Save(
            destination,
            new PngBitmapEncoderOptions
            {
                CompressionLevel = System.IO.Compression.CompressionLevel.Optimal
            });
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

            foreach (var annotation in _annotations)
            {
                DrawAnnotation(context, annotation);
            }

            if (_measurementDraft is { } draft)
            {
                DrawMeasurement(context, draft);
            }

            if (_annotationDraftAnchor is { } annotationAnchor)
            {
                DrawAnnotationMarker(context, Transform.ImageToCanvas(annotationAnchor));
            }

            DrawEyedropperPreview(context);
            DrawMagnifier(context);
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
        if ((point.Properties.IsMiddleButtonPressed
                || (point.Properties.IsLeftButtonPressed && _isTemporaryPanEnabled))
            && _bitmap is not null)
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
            return;
        }

        if (_isAnnotationEnabled
            && point.Properties.IsLeftButtonPressed
            && Transform.TryCanvasToImage(
                new CanvasPoint(point.Position.X, point.Position.Y),
                out var annotationAnchor))
        {
            _annotationDraftAnchor = annotationAnchor;
            InvalidateVisual();
            AnnotationRequested?.Invoke(
                this,
                new AnnotationRequestedEventArgs(annotationAnchor));
            e.Handled = true;
            return;
        }

        if (_isEyedropperEnabled
            && point.Properties.IsLeftButtonPressed
            && Transform.TryCanvasToImagePixel(
                new CanvasPoint(point.Position.X, point.Position.Y),
                out var pixelX,
                out var pixelY)
            && _pixels?.TryGetPixel(pixelX, pixelY, out var color) == true)
        {
            _sampledColor = color;
            InvalidateVisual();
            ColorPicked?.Invoke(this, new ColorPickedEventArgs(color));
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
            UpdateMeasurementDraft(position, e.KeyModifiers);
        }

        _lastPointerCanvasPosition = position;
        ReportPointerPosition(position);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_isMeasuring
            && (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            && _lastPointerCanvasPosition is { } position)
        {
            UpdateMeasurementDraft(position, e.KeyModifiers | KeyModifiers.Shift);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (_isMeasuring
            && (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            && _lastPointerCanvasPosition is { } position)
        {
            UpdateMeasurementDraft(position, e.KeyModifiers & ~KeyModifiers.Shift);
        }
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
            var measurement = CreateMeasurement(
                _measurementStart,
                end,
                e.KeyModifiers);

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
        _lastPointerCanvasPosition = null;
        InvalidateVisual();
        PointerImageChanged?.Invoke(
            this,
            new ImagePointerEventArgs(
                _lastPixelX,
                _lastPixelY,
                false,
                null));
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
        _pixels = null;
        _sampledColor = null;
        _lastPixelInside = false;
        _lastPointerCanvasPosition = null;
        ClearMeasurements();
        ClearAnnotations();
        bitmap?.Dispose();
    }

    private void ReportPointerPosition(Point position)
    {
        var inside = Transform.TryCanvasToImagePixel(
            new CanvasPoint(position.X, position.Y),
            out var pixelX,
            out var pixelY);
        PixelColor? sampledColor = null;
        if (inside
            && (_isEyedropperEnabled || _isMagnifierEnabled)
            && _pixels?.TryGetPixel(pixelX, pixelY, out var color) == true)
        {
            sampledColor = color;
        }

        var stateChanged = inside != _lastPixelInside
            || pixelX != _lastPixelX
            || pixelY != _lastPixelY
            || sampledColor != _sampledColor;
        if (!stateChanged && !_isMagnifierEnabled)
        {
            return;
        }

        _lastPixelInside = inside;
        _lastPixelX = pixelX;
        _lastPixelY = pixelY;
        _sampledColor = sampledColor;
        InvalidateVisual();
        if (!stateChanged)
        {
            return;
        }

        PointerImageChanged?.Invoke(
            this,
            new ImagePointerEventArgs(pixelX, pixelY, inside, sampledColor));
    }

    private void DrawMagnifier(DrawingContext context)
    {
        if (!_isMagnifierEnabled
            || _bitmap is null
            || _pixels is null
            || !_lastPixelInside
            || _sampledColor is not { } color
            || _lastPointerCanvasPosition is not { } pointer)
        {
            return;
        }

        var layout = MagnifierLayoutCalculator.Create(
            Bounds.Width,
            Bounds.Height,
            pointer.X,
            pointer.Y,
            _lastPixelX,
            _lastPixelY,
            _pixels.Width,
            _pixels.Height,
            _magnifierScale);
        var cardRect = ToRect(layout.CardBounds);
        var viewportRect = ToRect(layout.ViewportBounds);
        var sourceRect = new Rect(
            layout.SourceRegion.X,
            layout.SourceRegion.Y,
            layout.SourceRegion.Width,
            layout.SourceRegion.Height);
        var sourceDestinationRect = ToRect(layout.SourceDestinationBounds);

        context.DrawRectangle(
            MagnifierBackgroundBrush,
            MagnifierBorderPen,
            cardRect,
            8,
            8);
        context.DrawRectangle(
            MagnifierViewportBackgroundBrush,
            MagnifierDividerPen,
            viewportRect,
            4,
            4);
        using (context.PushClip(viewportRect))
        {
            DrawCheckerboard(context, viewportRect);
            context.DrawImage(_bitmap, sourceRect, sourceDestinationRect);
            DrawMagnifierGrid(context, layout);
        }

        context.DrawRectangle(
            null,
            MagnifierCurrentPixelShadowPen,
            ToRect(layout.CurrentPixelBounds));
        context.DrawRectangle(
            null,
            MagnifierCurrentPixelPen,
            ToRect(layout.CurrentPixelBounds));
        DrawMagnifierCrosshair(context, layout);

        var dividerY = viewportRect.Bottom + MagnifierLayoutCalculator.CardPadding;
        context.DrawLine(
            MagnifierDividerPen,
            new Point(cardRect.X + MagnifierLayoutCalculator.CardPadding, dividerY),
            new Point(cardRect.Right - MagnifierLayoutCalculator.CardPadding, dividerY));

        var scaleText = new FormattedText(
            $"{_magnifierScale}×",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            10,
            MagnifierMutedTextBrush);
        var coordinateText = new FormattedText(
            $"X {_lastPixelX}  Y {_lastPixelY}",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            10,
            MagnifierMutedTextBrush);
        var hexText = new FormattedText(
            color.Hex,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            11,
            MagnifierTextBrush);
        var footerTop = dividerY + 6;
        context.DrawText(
            scaleText,
            new Point(cardRect.X + MagnifierLayoutCalculator.CardPadding, footerTop));
        context.DrawText(
            hexText,
            new Point(
                cardRect.X + MagnifierLayoutCalculator.CardPadding + scaleText.Width + 10,
                footerTop - 0.5));
        context.DrawText(
            coordinateText,
            new Point(
                cardRect.Right - MagnifierLayoutCalculator.CardPadding - coordinateText.Width,
                footerTop));
    }

    private static void DrawMagnifierGrid(
        DrawingContext context,
        MagnifierLayout layout)
    {
        var sampleBounds = ToRect(layout.SampleBounds);
        var pixelSize = layout.CurrentPixelBounds.Width;
        for (var index = 0; index <= layout.SamplePixelCount; index++)
        {
            var x = sampleBounds.X + (index * pixelSize);
            context.DrawLine(
                MagnifierGridPen,
                new Point(x, sampleBounds.Y),
                new Point(x, sampleBounds.Bottom));

            var y = sampleBounds.Y + (index * pixelSize);
            context.DrawLine(
                MagnifierGridPen,
                new Point(sampleBounds.X, y),
                new Point(sampleBounds.Right, y));
        }
    }

    private void DrawMagnifierCrosshair(
        DrawingContext context,
        MagnifierLayout layout)
    {
        const double armLength = 16;
        const double centerGap = 3;
        var x = layout.CrosshairX;
        var y = layout.CrosshairY;

        DrawCrosshairLine(
            context,
            new Point(x - armLength, y),
            new Point(x - centerGap, y));
        DrawCrosshairLine(
            context,
            new Point(x + centerGap, y),
            new Point(x + armLength, y));
        DrawCrosshairLine(
            context,
            new Point(x, y - armLength),
            new Point(x, y - centerGap));
        DrawCrosshairLine(
            context,
            new Point(x, y + centerGap),
            new Point(x, y + armLength));
    }

    private static void DrawCrosshairLine(
        DrawingContext context,
        Point start,
        Point end)
    {
        context.DrawLine(MagnifierCrosshairShadowPen, start, end);
        context.DrawLine(MagnifierCrosshairPen, start, end);
    }

    private static Rect ToRect(MagnifierLayoutRect rect)
    {
        return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
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
        context.DrawEllipse(MeasurementShadowBrush, null, point, 4, 4);
        context.DrawEllipse(MeasurementEndpointInnerBrush, null, point, 2.2, 2.2);
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
            10.5,
            MeasurementLabelTextBrush);

        const double horizontalPadding = 7;
        const double verticalPadding = 3;
        var width = text.WidthIncludingTrailingWhitespace + (horizontalPadding * 2);
        var height = text.Height + (verticalPadding * 2);

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var lineLength = Math.Sqrt((dx * dx) + (dy * dy));
        var normalX = lineLength > 0.001 ? -dy / lineLength : 0;
        var normalY = lineLength > 0.001 ? dx / lineLength : -1;
        var middleX = (start.X + end.X) / 2;
        var middleY = (start.Y + end.Y) / 2;
        const double labelOffset = 14;

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

    private static void DrawExportMeasurement(
        DrawingContext context,
        PixelMeasurement measurement,
        Rect imageBounds,
        double scale)
    {
        var start = new Point(measurement.Start.X, measurement.Start.Y);
        var end = new Point(measurement.End.X, measurement.End.Y);
        var shadowPen = new Pen(MeasurementShadowBrush, 2.8 * scale);
        var linePen = new Pen(MeasurementBrush, 1.1 * scale);
        context.DrawLine(shadowPen, start, end);
        context.DrawLine(linePen, start, end);

        context.DrawEllipse(MeasurementShadowBrush, null, start, 4 * scale, 4 * scale);
        context.DrawEllipse(MeasurementEndpointInnerBrush, null, start, 2.2 * scale, 2.2 * scale);
        context.DrawEllipse(MeasurementShadowBrush, null, end, 4 * scale, 4 * scale);
        context.DrawEllipse(MeasurementEndpointInnerBrush, null, end, 2.2 * scale, 2.2 * scale);

        var text = new FormattedText(
            MeasurementFormatting.FormatLabel(measurement),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            10.5 * scale,
            MeasurementLabelTextBrush);
        var horizontalPadding = 7 * scale;
        var verticalPadding = 3 * scale;
        var width = text.WidthIncludingTrailingWhitespace + (horizontalPadding * 2);
        var height = text.Height + (verticalPadding * 2);

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var lineLength = Math.Sqrt((dx * dx) + (dy * dy));
        var normalX = lineLength > 0.001 ? -dy / lineLength : 0;
        var normalY = lineLength > 0.001 ? dx / lineLength : -1;
        var middleX = (start.X + end.X) / 2;
        var middleY = (start.Y + end.Y) / 2;
        var labelOffset = 14 * scale;
        var margin = 6 * scale;

        var left = middleX + (normalX * labelOffset) - (width / 2);
        var top = middleY + (normalY * labelOffset) - (height / 2);
        left = Math.Clamp(left, margin, Math.Max(margin, imageBounds.Width - width - margin));
        top = Math.Clamp(top, margin, Math.Max(margin, imageBounds.Height - height - margin));
        var labelRect = new Rect(left, top, width, height);
        context.DrawRectangle(
            MeasurementLabelBackgroundBrush,
            new Pen(new SolidColorBrush(Color.Parse("#995E5140")), scale),
            labelRect,
            5 * scale,
            5 * scale);
        context.DrawText(
            text,
            new Point(
                labelRect.X + horizontalPadding,
                labelRect.Y + verticalPadding));
    }

    private void DrawEyedropperPreview(DrawingContext context)
    {
        if (!_isEyedropperEnabled
            || _sampledColor is not { } color
            || _lastPointerCanvasPosition is not { } pointer)
        {
            return;
        }

        var hexText = new FormattedText(
            color.Hex,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            11,
            PickerTextBrush);
        var rgbText = new FormattedText(
            color.Rgb,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            10,
            PickerMutedTextBrush);

        const double padding = 8;
        const double swatchSize = 20;
        const double gap = 8;
        var textWidth = Math.Max(
            hexText.WidthIncludingTrailingWhitespace,
            rgbText.WidthIncludingTrailingWhitespace);
        var cardWidth = swatchSize + gap + textWidth + (padding * 2);
        const double cardHeight = 46;
        var left = pointer.X + 16;
        var top = pointer.Y + 16;
        if (left + cardWidth > Bounds.Width - 6)
        {
            left = pointer.X - cardWidth - 16;
        }

        if (top + cardHeight > Bounds.Height - 6)
        {
            top = pointer.Y - cardHeight - 16;
        }

        left = Math.Clamp(left, 6, Math.Max(6, Bounds.Width - cardWidth - 6));
        top = Math.Clamp(top, 6, Math.Max(6, Bounds.Height - cardHeight - 6));
        var cardRect = new Rect(left, top, cardWidth, cardHeight);
        context.DrawRectangle(PickerBackgroundBrush, PickerBorderPen, cardRect, 6, 6);

        var swatchRect = new Rect(
            cardRect.X + padding,
            cardRect.Y + ((cardRect.Height - swatchSize) / 2),
            swatchSize,
            swatchSize);
        DrawCheckerboard(context, swatchRect);
        _eyedropperSwatchBrush.Color =
            Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        context.DrawRectangle(
            _eyedropperSwatchBrush,
            PickerBorderPen,
            swatchRect,
            3,
            3);

        var textX = swatchRect.Right + gap;
        context.DrawText(hexText, new Point(textX, cardRect.Y + 7));
        context.DrawText(rgbText, new Point(textX, cardRect.Y + 24));
    }

    private void DrawAnnotation(
        DrawingContext context,
        PixelAnnotation annotation)
    {
        var anchor = Transform.ImageToCanvas(annotation.Anchor);
        var anchorPoint = new Point(anchor.X, anchor.Y);
        var displayText = new FormattedText(
            WrapAnnotationText(annotation.Text),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            11,
            AnnotationLabelTextBrush);

        const double horizontalPadding = 8;
        const double verticalPadding = 5;
        const double labelGap = 12;
        var width = Math.Min(
            260,
            displayText.WidthIncludingTrailingWhitespace + (horizontalPadding * 2));
        var height = displayText.Height + (verticalPadding * 2);
        var left = anchor.X + labelGap;
        var top = anchor.Y - height - labelGap;
        if (left + width > Bounds.Width - 6)
        {
            left = anchor.X - width - labelGap;
        }

        if (top < 6)
        {
            top = anchor.Y + labelGap;
        }

        left = Math.Clamp(left, 6, Math.Max(6, Bounds.Width - width - 6));
        top = Math.Clamp(top, 6, Math.Max(6, Bounds.Height - height - 6));
        var labelRect = new Rect(left, top, width, height);
        var leaderEnd = new Point(
            labelRect.X > anchor.X ? labelRect.X : labelRect.Right,
            labelRect.Y > anchor.Y ? labelRect.Y : labelRect.Bottom);
        context.DrawLine(AnnotationLeaderPen, anchorPoint, leaderEnd);
        context.DrawRectangle(
            AnnotationLabelBackgroundBrush,
            AnnotationBorderPen,
            labelRect,
            6,
            6);
        using (context.PushClip(
            labelRect.Deflate(new Thickness(horizontalPadding - 1, verticalPadding - 1))))
        {
            context.DrawText(
                displayText,
                new Point(
                    labelRect.X + horizontalPadding,
                    labelRect.Y + verticalPadding));
        }

        DrawAnnotationMarker(context, anchor);
    }

    private static void DrawExportAnnotation(
        DrawingContext context,
        PixelAnnotation annotation,
        Rect imageBounds,
        double scale)
    {
        var anchorPoint = new Point(annotation.Anchor.X, annotation.Anchor.Y);
        var displayText = new FormattedText(
            WrapAnnotationText(annotation.Text),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            MeasurementTypeface,
            11.5 * scale,
            AnnotationLabelTextBrush);

        var horizontalPadding = 8 * scale;
        var verticalPadding = 5 * scale;
        var labelGap = 12 * scale;
        var margin = 6 * scale;
        var maximumWidth = 260 * scale;
        var width = Math.Min(
            maximumWidth,
            displayText.WidthIncludingTrailingWhitespace + (horizontalPadding * 2));
        var height = displayText.Height + (verticalPadding * 2);
        var left = anchorPoint.X + labelGap;
        var top = anchorPoint.Y - height - labelGap;
        if (left + width > imageBounds.Width - margin)
        {
            left = anchorPoint.X - width - labelGap;
        }

        if (top < margin)
        {
            top = anchorPoint.Y + labelGap;
        }

        left = Math.Clamp(left, margin, Math.Max(margin, imageBounds.Width - width - margin));
        top = Math.Clamp(top, margin, Math.Max(margin, imageBounds.Height - height - margin));
        var labelRect = new Rect(left, top, width, height);
        var leaderEnd = new Point(
            labelRect.X > anchorPoint.X ? labelRect.X : labelRect.Right,
            labelRect.Y > anchorPoint.Y ? labelRect.Y : labelRect.Bottom);
        context.DrawLine(
            new Pen(new SolidColorBrush(Color.Parse("#CC63E6BE")), 1.2 * scale),
            anchorPoint,
            leaderEnd);
        context.DrawRectangle(
            AnnotationLabelBackgroundBrush,
            new Pen(new SolidColorBrush(Color.Parse("#6663E6BE")), scale),
            labelRect,
            6 * scale,
            6 * scale);
        using (context.PushClip(
            labelRect.Deflate(
                new Thickness(
                    horizontalPadding - scale,
                    verticalPadding - scale))))
        {
            context.DrawText(
                displayText,
                new Point(
                    labelRect.X + horizontalPadding,
                    labelRect.Y + verticalPadding));
        }

        context.DrawEllipse(
            AnnotationBrush,
            null,
            anchorPoint,
            6 * scale,
            6 * scale);
        context.DrawEllipse(
            AnnotationInnerBrush,
            null,
            anchorPoint,
            2.4 * scale,
            2.4 * scale);
    }

    private static void DrawAnnotationMarker(
        DrawingContext context,
        CanvasPoint point)
    {
        var center = new Point(point.X, point.Y);
        context.DrawEllipse(AnnotationBrush, null, center, 6, 6);
        context.DrawEllipse(AnnotationInnerBrush, null, center, 2.4, 2.4);
    }

    private static string WrapAnnotationText(string text)
    {
        const int charactersPerLine = 28;
        var lines = new List<string>();
        foreach (var sourceLine in text.Split('\n'))
        {
            if (sourceLine.Length == 0)
            {
                lines.Add(string.Empty);
                continue;
            }

            for (var index = 0; index < sourceLine.Length; index += charactersPerLine)
            {
                lines.Add(sourceLine.Substring(
                    index,
                    Math.Min(charactersPerLine, sourceLine.Length - index)));
            }
        }

        return string.Join('\n', lines);
    }

    private static PixelMeasurement CreateMeasurement(
        ImagePoint start,
        ImagePoint end,
        KeyModifiers modifiers)
    {
        var measurement = new PixelMeasurement(start, end);
        return modifiers.HasFlag(KeyModifiers.Shift)
            ? measurement.ConstrainToAxis()
            : measurement;
    }

    private void UpdateMeasurementDraft(Point position, KeyModifiers modifiers)
    {
        _measurementDraft = CreateMeasurement(
            _measurementStart,
            Transform.CanvasToImageClamped(new CanvasPoint(position.X, position.Y)),
            modifiers);
        InvalidateVisual();
    }

    private void UpdateCursor()
    {
        Cursor = _isPanning
            ? PanningCursor
            : _isTemporaryPanEnabled
                ? PanReadyCursor
                : (_isMeasurementEnabled
                        || _isEyedropperEnabled
                        || _isMagnifierEnabled
                        || _isAnnotationEnabled)
                    && _bitmap is not null
                    ? MeasuringCursor
                    : Cursor.Default;
    }
}

public sealed class ImagePointerEventArgs(
    int pixelX,
    int pixelY,
    bool isInside,
    PixelColor? color) : EventArgs
{
    public int PixelX { get; } = pixelX;

    public int PixelY { get; } = pixelY;

    public bool IsInside { get; } = isInside;

    public PixelColor? Color { get; } = color;
}

public sealed class ColorPickedEventArgs(PixelColor color) : EventArgs
{
    public PixelColor Color { get; } = color;
}

public sealed class AnnotationRequestedEventArgs(ImagePoint anchor) : EventArgs
{
    public ImagePoint Anchor { get; } = anchor;
}
