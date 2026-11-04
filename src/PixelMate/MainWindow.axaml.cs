using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PixelMate.Colors;
using PixelMate.Imaging;
using PixelMate.Measurement;
using PixelMate.Preferences;
using System.Globalization;

namespace PixelMate;

public partial class MainWindow : Window
{
    private enum ToolMode
    {
        None,
        Measure,
        Eyedropper,
        Magnifier
    }

    private static readonly IReadOnlyList<FilePickerFileType> ImageFileTypes =
    [
        new("图片")
        {
            Patterns = ImageFileSupport.Patterns,
            MimeTypes =
            [
                "image/png",
                "image/jpeg",
                "image/bmp",
                "image/webp"
            ]
        }
    ];
    private static readonly IBrush StatusNormalBrush =
        new SolidColorBrush(Color.Parse("#74747B"));
    private static readonly IBrush StatusErrorBrush =
        new SolidColorBrush(Color.Parse("#FF7A7A"));
    private static readonly IBrush EmptyColorPreviewBrush =
        new SolidColorBrush(Color.Parse("#4A4A4F"));
    private static readonly IBrush EmptyColorBorderBrush =
        new SolidColorBrush(Color.Parse("#66666D"));
    private static readonly IBrush LiveColorBorderBrush =
        new SolidColorBrush(Color.Parse("#8A8A92"));

    private readonly AppPreferences _preferences = AppPreferences.CreateDefault();
    private readonly SolidColorBrush _liveColorPreviewBrush =
        new(Avalonia.Media.Colors.Transparent);
    private ToolMode _activeTool;
    private PixelColor? _sampledColor;
    private CancellationTokenSource? _imageOpenCancellation;
    private int _imageOpenVersion;
    private bool _isClosed;

    public MainWindow()
    {
        InitializeComponent();

        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        ImageCanvas.ViewChanged += ImageCanvas_ViewChanged;
        ImageCanvas.PointerImageChanged += ImageCanvas_PointerImageChanged;
        ImageCanvas.MeasurementsChanged += ImageCanvas_MeasurementsChanged;
        ImageCanvas.ColorPicked += ImageCanvas_ColorPicked;
        KeyDown += MainWindow_KeyDown;
        Closed += MainWindow_Closed;
        UpdateMagnifierScaleControls();
    }

    private async void OpenImage_Click(object? sender, RoutedEventArgs e)
    {
        await OpenImagePickerAsync();
    }

    private async Task OpenImagePickerAsync()
    {
        IStorageFolder? suggestedStartLocation;
        string? path;
        try
        {
            suggestedStartLocation = null;
            if (!string.IsNullOrWhiteSpace(_preferences.LastOpenDirectory)
                && Directory.Exists(_preferences.LastOpenDirectory))
            {
                suggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(
                    _preferences.LastOpenDirectory);
            }

            var files = await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "打开图片",
                    AllowMultiple = false,
                    FileTypeFilter = ImageFileTypes,
                    SuggestedStartLocation = suggestedStartLocation
                });
            path = files.FirstOrDefault()?.TryGetLocalPath();
        }
        catch (Exception)
        {
            SetStatusMessage("无法打开文件选择器", true);
            return;
        }

        if (path is not null)
        {
            await OpenImageAsync(path);
        }
    }

    private async Task OpenImageAsync(string filePath)
    {
        if (!ImageFileSupport.IsSupportedPath(filePath))
        {
            SetStatusMessage("不支持这个文件格式，请选择 PNG、JPG、BMP 或 WebP。", true);
            return;
        }

        var openVersion = ++_imageOpenVersion;
        _imageOpenCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        _imageOpenCancellation = cancellation;
        var fileName = Path.GetFileName(filePath);
        SetStatusMessage($"正在打开 {fileName}", false);

        try
        {
            var decodedImage = await Task.Run(
                () => OriginalImageDecoder.Decode(filePath),
                cancellation.Token);
            if (cancellation.IsCancellationRequested
                || openVersion != _imageOpenVersion
                || _isClosed)
            {
                return;
            }

            ImageCanvas.SetImage(decodedImage);
            _preferences.RememberDirectory(filePath);

            ImageNameTextBlock.Text = fileName;
            ImageSizeTextBlock.Text = $"{decodedImage.Width} × {decodedImage.Height}";
            EmptyStatePanel.IsVisible = false;
            PointerTextBlock.Text = "X —  Y —";
            UpdateSampledColor(null);
            SetImageControlsEnabled(true);
            SetStatusMessage(string.Empty, false);
            Title = fileName;
            WindowTitleTextBlock.Text = fileName;
            UpdateViewStatus();
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (InvalidDataException ex)
        {
            if (!_isClosed && openVersion == _imageOpenVersion)
            {
                SetStatusMessage($"无法打开图片：{ex.Message}", true);
            }
        }
        catch (Exception)
        {
            if (!_isClosed && openVersion == _imageOpenVersion)
            {
                SetStatusMessage($"无法打开图片：{fileName}", true);
            }
        }
        finally
        {
            if (ReferenceEquals(_imageOpenCancellation, cancellation))
            {
                _imageOpenCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private void ZoomOut_Click(object? sender, RoutedEventArgs e)
    {
        ImageCanvas.ZoomBy(1 / 1.25);
    }

    private void ZoomIn_Click(object? sender, RoutedEventArgs e)
    {
        ImageCanvas.ZoomBy(1.25);
    }

    private void Fit_Click(object? sender, RoutedEventArgs e)
    {
        ImageCanvas.FitToViewport();
    }

    private void ActualSize_Click(object? sender, RoutedEventArgs e)
    {
        ImageCanvas.ShowActualSize();
    }

    private void ImageCanvas_ViewChanged(object? sender, EventArgs e)
    {
        UpdateViewStatus();
    }

    private void ImageCanvas_PointerImageChanged(object? sender, Controls.ImagePointerEventArgs e)
    {
        PointerTextBlock.Text = e.IsInside
            ? $"X {e.PixelX}  Y {e.PixelY}"
            : "X —  Y —";
        UpdateSampledColor(e.IsInside ? e.Color : null);
    }

    private async void ImageCanvas_ColorPicked(
        object? sender,
        Controls.ColorPickedEventArgs e)
    {
        UpdateSampledColor(e.Color);
        await CopyColorAsync(e.Color, copyHex: true);
    }

    private void MeasureTool_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveTool(ToolMode.Measure);
    }

    private void EyedropperTool_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveTool(ToolMode.Eyedropper);
    }

    private void MagnifierTool_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveTool(ToolMode.Magnifier);
    }

    private void Magnifier8_Click(object? sender, RoutedEventArgs e)
    {
        SetMagnifierScale(8);
    }

    private void Magnifier12_Click(object? sender, RoutedEventArgs e)
    {
        SetMagnifierScale(12);
    }

    private void Magnifier16_Click(object? sender, RoutedEventArgs e)
    {
        SetMagnifierScale(16);
    }

    private async void CopyMeasurements_Click(object? sender, RoutedEventArgs e)
    {
        await CopyMeasurementsAsync();
    }

    private void ClearMeasurements_Click(object? sender, RoutedEventArgs e)
    {
        ImageCanvas.ClearMeasurements();
        SetStatusMessage("已清除全部测量", false);
    }

    private async void CopySampledHex_Click(object? sender, RoutedEventArgs e)
    {
        if (_sampledColor is { } color)
        {
            await CopyColorAsync(color, copyHex: true);
        }
    }

    private async void CopySampledRgb_Click(object? sender, RoutedEventArgs e)
    {
        if (_sampledColor is { } color)
        {
            await CopyColorAsync(color, copyHex: false);
        }
    }

    private async void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.O
            && (e.KeyModifiers.HasFlag(KeyModifiers.Meta)
                || e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            e.Handled = true;
            await OpenImagePickerAsync();
            return;
        }

        if (e.Key == Key.C
            && ImageCanvas.HasMeasurements
            && (e.KeyModifiers.HasFlag(KeyModifiers.Meta)
                || e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            e.Handled = true;
            await CopyMeasurementsAsync();
            return;
        }

        if ((e.Key == Key.Back || e.Key == Key.Delete)
            && ImageCanvas.HasMeasurements)
        {
            e.Handled = true;
            ImageCanvas.ClearMeasurements();
            SetStatusMessage("已清除全部测量", false);
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.TryGetFiles() is { Length: > 0 }
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        var file = e.DataTransfer.TryGetFiles()?.FirstOrDefault();
        var path = file?.TryGetLocalPath();
        if (path is not null)
        {
            await OpenImageAsync(path);
        }
        else
        {
            SetStatusMessage("请拖入一个本地图片文件。", true);
        }

        e.Handled = true;
    }

    private void UpdateViewStatus()
    {
        if (!ImageCanvas.Transform.HasImage)
        {
            ZoomTextBlock.Text = "—";
            StatusZoomTextBlock.Text = "—";
            return;
        }

        var zoom = ImageCanvas.Transform.Scale * 100;
        var zoomText = zoom >= 100
            ? $"{zoom.ToString("0", CultureInfo.InvariantCulture)}%"
            : $"{zoom.ToString("0.#", CultureInfo.InvariantCulture)}%";
        ZoomTextBlock.Text = zoomText;
        StatusZoomTextBlock.Text = zoomText;
    }

    private void ImageCanvas_MeasurementsChanged(object? sender, EventArgs e)
    {
        UpdateMeasurementActions();

        if (ImageCanvas.HasMeasurements)
        {
            var count = ImageCanvas.Measurements.Count;
            SetStatusMessage($"已记录 {count} 条测量", false);
        }
    }

    private void UpdateMeasurementActions()
    {
        var count = ImageCanvas.Measurements.Count;
        MeasurementActionsPanel.IsVisible = count > 0;
        MeasurementCountTextBlock.Text = $"{count} 条";
    }

    private async Task CopyMeasurementsAsync()
    {
        if (!ImageCanvas.HasMeasurements)
        {
            SetStatusMessage("还没有可复制的测量结果", true);
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            SetStatusMessage("当前系统剪贴板不可用", true);
            return;
        }

        try
        {
            await clipboard.SetTextAsync(
                MeasurementFormatting.FormatClipboard(ImageCanvas.Measurements));
            SetStatusMessage($"已复制 {ImageCanvas.Measurements.Count} 条测量结果", false);
        }
        catch (Exception)
        {
            SetStatusMessage("复制测量结果失败", true);
        }
    }

    private void SetImageControlsEnabled(bool isEnabled)
    {
        FitButton.IsEnabled = isEnabled;
        ActualSizeButton.IsEnabled = isEnabled;
        ZoomOutButton.IsEnabled = isEnabled;
        ZoomInButton.IsEnabled = isEnabled;
        MeasureToolButton.IsEnabled = isEnabled;
        EyedropperToolButton.IsEnabled = isEnabled;
        MagnifierToolButton.IsEnabled = isEnabled;
        if (!isEnabled)
        {
            SetActiveTool(ToolMode.None);
        }
    }

    private void SetStatusMessage(string message, bool isError)
    {
        StatusMessageTextBlock.Text = message;
        StatusMessageTextBlock.Foreground = isError
            ? StatusErrorBrush
            : StatusNormalBrush;
    }

    private void SetActiveTool(ToolMode tool)
    {
        _activeTool = tool;
        MeasureToolButton.IsChecked = tool == ToolMode.Measure;
        EyedropperToolButton.IsChecked = tool == ToolMode.Eyedropper;
        MagnifierToolButton.IsChecked = tool == ToolMode.Magnifier;
        ImageCanvas.SetMeasurementEnabled(tool == ToolMode.Measure);
        ImageCanvas.SetEyedropperEnabled(tool == ToolMode.Eyedropper);
        ImageCanvas.SetMagnifierEnabled(tool == ToolMode.Magnifier);
        MagnifierScalePanel.IsVisible = tool == ToolMode.Magnifier;
        UpdateMagnifierScaleControls();
        if (tool != ToolMode.Eyedropper && tool != ToolMode.Magnifier)
        {
            UpdateSampledColor(null);
        }

        SetStatusMessage(
            tool switch
            {
                ToolMode.Measure => "左键拖动测量；按住 Shift 锁定水平或垂直",
                ToolMode.Eyedropper => "移动鼠标读取颜色，左键复制 HEX；状态栏可复制 RGB",
                ToolMode.Magnifier => "移动鼠标检查像素；像素网格、中心像素和 HEX 显示在指针附近",
                _ => string.Empty
            },
            false);
    }

    private void SetMagnifierScale(int scale)
    {
        ImageCanvas.SetMagnifierScale(scale);
        UpdateMagnifierScaleControls();
        ImageCanvas.Focus();
    }

    private void UpdateMagnifierScaleControls()
    {
        Magnifier8Button.IsChecked = ImageCanvas.MagnifierScale == 8;
        Magnifier12Button.IsChecked = ImageCanvas.MagnifierScale == 12;
        Magnifier16Button.IsChecked = ImageCanvas.MagnifierScale == 16;
    }

    private void UpdateSampledColor(PixelColor? color)
    {
        _sampledColor = color;
        var hasColor = color.HasValue
            && (_activeTool == ToolMode.Eyedropper || _activeTool == ToolMode.Magnifier);
        var canCopy = hasColor && _activeTool == ToolMode.Eyedropper;
        EyedropperActionsPanel.IsVisible = canCopy;
        CopySampledHexButton.IsEnabled = canCopy;
        CopySampledRgbButton.IsEnabled = canCopy;

        if (!hasColor || color is not { } sampled)
        {
            ColorPreviewBorder.Background = EmptyColorPreviewBrush;
            ColorPreviewBorder.BorderBrush = EmptyColorBorderBrush;
            HexTextBlock.Text = "HEX —";
            return;
        }

        _liveColorPreviewBrush.Color =
            Color.FromArgb(sampled.Alpha, sampled.Red, sampled.Green, sampled.Blue);
        ColorPreviewBorder.Background = _liveColorPreviewBrush;
        ColorPreviewBorder.BorderBrush = LiveColorBorderBrush;
        HexTextBlock.Text = sampled.Hex;
    }

    private async Task CopyColorAsync(PixelColor color, bool copyHex)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            SetStatusMessage("当前系统剪贴板不可用", true);
            return;
        }

        try
        {
            var value = copyHex ? color.Hex : color.Rgb;
            await clipboard.SetTextAsync(value);
            SetStatusMessage($"已复制 {value}", false);
        }
        catch (Exception)
        {
            SetStatusMessage(copyHex ? "复制 HEX 失败" : "复制 RGB 失败", true);
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _isClosed = true;
        _imageOpenCancellation?.Cancel();
        ImageCanvas.DisposeImage();
    }
}
