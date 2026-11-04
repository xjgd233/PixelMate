using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PixelMate.Imaging;
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

    public MainWindow()
    {
        InitializeComponent();

        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        ImageCanvas.ViewChanged += ImageCanvas_ViewChanged;
        ImageCanvas.PointerImageChanged += ImageCanvas_PointerImageChanged;
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
            var bitmap = new Bitmap(filePath);
            ImageCanvas.SetImage(bitmap);
            _preferences.RememberDirectory(filePath);

            var fileName = Path.GetFileName(filePath);
            ImageNameTextBlock.Text = fileName;
            ImageSizeTextBlock.Text =
                $"{bitmap.PixelSize.Width} × {bitmap.PixelSize.Height}";
            EmptyStatePanel.IsVisible = false;
            PointerTextBlock.Text = "X —  Y —";
            HexTextBlock.Text = "HEX —";
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

    private async void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.O
            && (e.KeyModifiers.HasFlag(KeyModifiers.Meta)
                || e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            e.Handled = true;
            await OpenImagePickerAsync();
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

        SetStatusMessage(
            tool switch
            {
                ToolMode.Measure => "已选择测量工具",
                ToolMode.Eyedropper => "已选择取色工具",
                ToolMode.Magnifier => "已选择放大镜",
                _ => string.Empty
            },
            false);
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        ImageCanvas.DisposeImage();
    }
}
