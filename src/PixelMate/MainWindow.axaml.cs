using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PixelMate.Imaging;
using PixelMate.Preferences;
using System.Globalization;

namespace PixelMate;

public partial class MainWindow : Window
{
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

    public MainWindow()
    {
        InitializeComponent();

        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        ImageCanvas.ViewChanged += ImageCanvas_ViewChanged;
        ImageCanvas.PointerImageChanged += ImageCanvas_PointerImageChanged;
        Closed += MainWindow_Closed;
    }

    private async void OpenImage_Click(object? sender, RoutedEventArgs e)
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
            Title = $"{fileName} — PixelMate";
            ToolbarHintTextBlock.Text = "滚轮缩放 · 中键拖动平移";
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
    }

    private void SetStatusMessage(string message, bool isError)
    {
        StatusMessageTextBlock.Text = message;
        StatusMessageTextBlock.Foreground = isError
            ? Avalonia.Media.Brushes.Salmon
            : Avalonia.Media.Brushes.Transparent;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        ImageCanvas.DisposeImage();
    }
}
