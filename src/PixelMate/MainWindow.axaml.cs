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

    private readonly AppPreferences _preferences = AppPreferences.CreateDefault();
    private ToolMode _activeTool;
    private PixelColor? _sampledColor;

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
    }

    private async void OpenImage_Click(object? sender, RoutedEventArgs e)
    {
        await OpenImagePickerAsync();
    }

    private async Task OpenImagePickerAsync()
    {
        IStorageFolder? suggestedStartLocation = null;
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

        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is not null)
        {
            OpenImage(path);
        }
    }

    private void OpenImage(string filePath)
    {
        if (!ImageFileSupport.IsSupportedPath(filePath))
        {
            SetStatusMessage("不支持这个文件格式，请选择 PNG、JPG、BMP 或 WebP。", true);
            return;
        }

        try
        {
            var decodedImage = OriginalImageDecoder.Decode(filePath);
            ImageCanvas.SetImage(decodedImage);
            _preferences.RememberDirectory(filePath);

            var fileName = Path.GetFileName(filePath);
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
        catch (Exception)
        {
            SetStatusMessage($"无法打开图片：{Path.GetFileName(filePath)}", true);
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
        UpdateSampledColor(e.Color);
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

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var file = e.DataTransfer.TryGetFiles()?.FirstOrDefault();
        var path = file?.TryGetLocalPath();
        if (path is not null)
        {
            OpenImage(path);
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
            ? new SolidColorBrush(Color.Parse("#FF7A7A"))
            : new SolidColorBrush(Color.Parse("#74747B"));
    }

    private void SetActiveTool(ToolMode tool)
    {
        _activeTool = tool;
        MeasureToolButton.IsChecked = tool == ToolMode.Measure;
        EyedropperToolButton.IsChecked = tool == ToolMode.Eyedropper;
        MagnifierToolButton.IsChecked = tool == ToolMode.Magnifier;
        ImageCanvas.SetMeasurementEnabled(tool == ToolMode.Measure);
        ImageCanvas.SetEyedropperEnabled(tool == ToolMode.Eyedropper);
        if (tool != ToolMode.Eyedropper)
        {
            UpdateSampledColor(null);
        }

        SetStatusMessage(
            tool switch
            {
                ToolMode.Measure => "左键拖动测量，可连续绘制多条测量线",
                ToolMode.Eyedropper => "移动鼠标读取颜色，左键复制 HEX；状态栏可复制 RGB",
                ToolMode.Magnifier => "已选择放大镜",
                _ => string.Empty
            },
            false);
    }

    private void UpdateSampledColor(PixelColor? color)
    {
        _sampledColor = color;
        var hasColor = color.HasValue && _activeTool == ToolMode.Eyedropper;
        EyedropperActionsPanel.IsVisible = hasColor;
        CopySampledHexButton.IsEnabled = hasColor;
        CopySampledRgbButton.IsEnabled = hasColor;

        if (!hasColor || color is not { } sampled)
        {
            ColorPreviewBorder.Background = new SolidColorBrush(Color.Parse("#4A4A4F"));
            ColorPreviewBorder.BorderBrush = new SolidColorBrush(Color.Parse("#66666D"));
            HexTextBlock.Text = "HEX —";
            return;
        }

        ColorPreviewBorder.Background = new SolidColorBrush(
            Color.FromArgb(sampled.Alpha, sampled.Red, sampled.Green, sampled.Blue));
        ColorPreviewBorder.BorderBrush = new SolidColorBrush(Color.Parse("#8A8A92"));
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
        ImageCanvas.DisposeImage();
    }
}
