using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PixelMate.Annotations;
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
        Magnifier,
        Annotation
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
    private static readonly IReadOnlyList<FilePickerFileType> EditedImageFileTypes =
    [
        new("PNG 图片")
        {
            Patterns = ["*.png"],
            MimeTypes = ["image/png"]
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
    private static readonly int[] MagnifierScales = [8, 12, 16];

    private readonly AppPreferences _preferences = AppPreferences.CreateDefault();
    private readonly SolidColorBrush _liveColorPreviewBrush =
        new(Avalonia.Media.Colors.Transparent);
    private ToolMode _activeTool;
    private PixelColor? _sampledColor;
    private AnnotationTextColor _lastAnnotationTextColor = AnnotationTextColor.Default;
    private CancellationTokenSource? _imageOpenCancellation;
    private int _imageOpenVersion;
    private bool _isClosed;
    private bool _hasShownStartupTutorial;
    private string? _currentImagePath;
    private ImagePoint? _annotationEditorAnchor;

    public MainWindow()
    {
        InitializeComponent();

        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        ImageCanvas.ViewChanged += ImageCanvas_ViewChanged;
        ImageCanvas.PointerImageChanged += ImageCanvas_PointerImageChanged;
        ImageCanvas.MeasurementsChanged += ImageCanvas_MeasurementsChanged;
        ImageCanvas.AnnotationsChanged += ImageCanvas_AnnotationsChanged;
        ImageCanvas.EditHistoryChanged += ImageCanvas_EditHistoryChanged;
        ImageCanvas.ColorPicked += ImageCanvas_ColorPicked;
        ImageCanvas.AnnotationRequested += ImageCanvas_AnnotationRequested;
        ImageCanvas.SizeChanged += (_, _) => PositionAnnotationEditor();
        AnnotationTextBox.AddHandler(
            InputElement.KeyDownEvent,
            AnnotationTextBox_KeyDown,
            RoutingStrategies.Tunnel);
        KeyDown += MainWindow_KeyDown;
        KeyUp += MainWindow_KeyUp;
        Deactivated += MainWindow_Deactivated;
        Opened += MainWindow_Opened;
        Closed += MainWindow_Closed;
        ImageCanvas.SetMagnifierScale(_preferences.MagnifierScale);
        ConfigureShortcutHints();
        UpdateMagnifierScaleControls();
        UpdateUndoRedoControls();
    }

    private async void OpenImage_Click(object? sender, RoutedEventArgs e)
    {
        await OpenImagePickerAsync();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else
        {
            BeginMoveDrag(e);
        }

        e.Handled = true;
    }

    private async Task OpenImagePickerAsync()
    {
        IStorageFolder? suggestedStartLocation;
        string? path;
        try
        {
            suggestedStartLocation = null;
            if (_preferences.RememberLastDirectory
                && !string.IsNullOrWhiteSpace(_preferences.LastOpenDirectory)
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
            _currentImagePath = filePath;
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

    private async void SaveEditedImage_Click(object? sender, RoutedEventArgs e)
    {
        await SaveEditedImageAsync();
    }

    private void Undo_Click(object? sender, RoutedEventArgs e)
    {
        UndoEdit();
    }

    private void Redo_Click(object? sender, RoutedEventArgs e)
    {
        RedoEdit();
    }

    private async Task SaveEditedImageAsync()
    {
        if (!ImageCanvas.Transform.HasImage || _currentImagePath is null)
        {
            SetStatusMessage("请先打开一张图片", true);
            return;
        }

        IStorageFile? file;
        try
        {
            IStorageFolder? suggestedStartLocation = null;
            if (!string.IsNullOrWhiteSpace(_preferences.LastOpenDirectory)
                && Directory.Exists(_preferences.LastOpenDirectory))
            {
                suggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(
                    _preferences.LastOpenDirectory);
            }

            file = await StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "保存更改后的图片",
                    SuggestedFileName = EditedImageExport.CreateSuggestedFileName(
                        _currentImagePath),
                    SuggestedStartLocation = suggestedStartLocation,
                    DefaultExtension = "png",
                    FileTypeChoices = EditedImageFileTypes,
                    ShowOverwritePrompt = true
                });
        }
        catch (Exception)
        {
            SetStatusMessage("无法打开保存位置选择器", true);
            return;
        }

        if (file is null)
        {
            return;
        }

        SaveEditedImageButton.IsEnabled = false;
        SetStatusMessage("正在保存图片…", false);
        try
        {
            await using var stream = await file.OpenWriteAsync();
            ImageCanvas.SaveEditedImage(stream);
            await stream.FlushAsync();
            var savedPath = file.TryGetLocalPath();
            if (!string.IsNullOrWhiteSpace(savedPath))
            {
                _preferences.RememberDirectory(savedPath);
            }

            SetStatusMessage($"已保存 {file.Name}", false);
        }
        catch (Exception)
        {
            SetStatusMessage("保存图片失败，请检查保存位置是否可写", true);
        }
        finally
        {
            SaveEditedImageButton.IsEnabled = ImageCanvas.Transform.HasImage;
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

    private void AnnotationTool_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveTool(ToolMode.Annotation);
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

    private async void CopyAnnotations_Click(object? sender, RoutedEventArgs e)
    {
        await CopyAnnotationsAsync();
    }

    private void ClearAnnotations_Click(object? sender, RoutedEventArgs e)
    {
        ImageCanvas.ClearAnnotations();
        SetStatusMessage("已清除全部批注", false);
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

    private void ImageCanvas_AnnotationRequested(
        object? sender,
        Controls.AnnotationRequestedEventArgs e)
    {
        _annotationEditorAnchor = e.Anchor;
        AnnotationCoordinateTextBlock.Text =
            $"X {Math.Floor(e.Anchor.X)}  Y {Math.Floor(e.Anchor.Y)}";
        AnnotationTextBox.Text = string.Empty;
        AnnotationColorTextBox.Text = _lastAnnotationTextColor.Hex;
        UpdateAnnotationColorPreview();
        AnnotationEditorLayer.IsVisible = true;
        PositionAnnotationEditor();
        Dispatcher.UIThread.Post(() =>
        {
            PositionAnnotationEditor();
            AnnotationTextBox.Focus();
        });
    }

    private void PositionAnnotationEditor()
    {
        if (!AnnotationEditorLayer.IsVisible
            || _annotationEditorAnchor is not { } imageAnchor)
        {
            return;
        }

        var anchor = ImageCanvas.Transform.ImageToCanvas(imageAnchor);
        var editorWidth = AnnotationEditorPanel.Bounds.Width > 0
            ? AnnotationEditorPanel.Bounds.Width
            : AnnotationEditorPanel.Width;
        var editorHeight = AnnotationEditorPanel.Bounds.Height > 0
            ? AnnotationEditorPanel.Bounds.Height
            : 206;
        var position = AnnotationEditorPlacement.Calculate(
            anchor,
            ImageCanvas.Bounds.Width,
            ImageCanvas.Bounds.Height,
            editorWidth,
            editorHeight);
        Canvas.SetLeft(AnnotationEditorPanel, position.X);
        Canvas.SetTop(AnnotationEditorPanel, position.Y);
    }

    private void AnnotationColorPreset_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string color })
        {
            AnnotationColorTextBox.Text = color;
            AnnotationTextBox.Focus();
        }
    }

    private void AnnotationColorTextBox_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        UpdateAnnotationColorPreview();
    }

    private void UpdateAnnotationColorPreview()
    {
        if (!AnnotationTextColor.TryParseHex(AnnotationColorTextBox.Text, out var color))
        {
            AnnotationColorSwatch.Background =
                new SolidColorBrush(Color.Parse("#2B2B2F"));
            AnnotationColorPreviewTextBlock.Foreground =
                new SolidColorBrush(Color.Parse("#74747B"));
            AnnotationColorPreviewTextBlock.Text = "颜色格式无效";
            return;
        }

        var brush = new SolidColorBrush(
            Color.FromRgb(color.Red, color.Green, color.Blue));
        AnnotationColorSwatch.Background = brush;
        AnnotationColorPreviewTextBlock.Foreground = brush;
        AnnotationColorPreviewTextBlock.Text = "示例文字";
    }

    private void SaveAnnotation_Click(object? sender, RoutedEventArgs e)
    {
        SaveAnnotation();
    }

    private void CancelAnnotation_Click(object? sender, RoutedEventArgs e)
    {
        CloseAnnotationEditor(cancelDraft: true);
    }

    private void AnnotationTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        var hasPrimaryModifier = HasPrimaryModifier(e.KeyModifiers);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CloseAnnotationEditor(cancelDraft: true);
            return;
        }

        if ((e.Key == Key.Enter || e.Key == Key.Return) && hasPrimaryModifier)
        {
            e.Handled = true;
            SaveAnnotation();
        }
    }

    private void SaveAnnotation()
    {
        var text = PixelAnnotation.NormalizeText(AnnotationTextBox.Text);
        if (text.Length == 0)
        {
            SetStatusMessage("请输入批注文字", true);
            AnnotationTextBox.Focus();
            return;
        }

        if (!AnnotationTextColor.TryParseHex(AnnotationColorTextBox.Text, out var color))
        {
            SetStatusMessage("请输入有效的字体颜色，例如 #FFD166", true);
            AnnotationColorTextBox.Focus();
            AnnotationColorTextBox.SelectAll();
            return;
        }

        ImageCanvas.CommitAnnotation(text, color);
        _lastAnnotationTextColor = color;
        CloseAnnotationEditor(cancelDraft: false);
        SetStatusMessage("已添加批注", false);
        ImageCanvas.Focus();
    }

    private void CloseAnnotationEditor(bool cancelDraft)
    {
        AnnotationEditorLayer.IsVisible = false;
        _annotationEditorAnchor = null;
        if (cancelDraft)
        {
            ImageCanvas.CancelAnnotationDraft();
        }

        ImageCanvas.Focus();
    }

    private async void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (AnnotationEditorLayer.IsVisible)
        {
            return;
        }

        var hasPrimaryModifier = HasPrimaryModifier(e.KeyModifiers);

        if (e.Key == Key.Space
            && e.KeyModifiers == KeyModifiers.None
            && ImageCanvas.Transform.HasImage)
        {
            ImageCanvas.SetTemporaryPanEnabled(true);
            e.Handled = true;
            return;
        }

        if (hasPrimaryModifier && e.Key == Key.O)
        {
            e.Handled = true;
            await OpenImagePickerAsync();
            return;
        }

        if (hasPrimaryModifier && e.Key == Key.Z)
        {
            e.Handled = true;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                RedoEdit();
            }
            else
            {
                UndoEdit();
            }

            return;
        }

        if (hasPrimaryModifier && e.Key == Key.Y)
        {
            e.Handled = true;
            RedoEdit();
            return;
        }

        if (hasPrimaryModifier && e.Key == Key.S)
        {
            e.Handled = true;
            await SaveEditedImageAsync();
            return;
        }

        if (hasPrimaryModifier
            && (e.Key == Key.OemPlus || e.Key == Key.Add))
        {
            e.Handled = true;
            ZoomIn_Click(sender, e);
            return;
        }

        if (hasPrimaryModifier
            && (e.Key == Key.OemMinus || e.Key == Key.Subtract))
        {
            e.Handled = true;
            ZoomOut_Click(sender, e);
            return;
        }

        if (hasPrimaryModifier && (e.Key == Key.D0 || e.Key == Key.NumPad0))
        {
            e.Handled = true;
            Fit_Click(sender, e);
            return;
        }

        if (hasPrimaryModifier && (e.Key == Key.D1 || e.Key == Key.NumPad1))
        {
            e.Handled = true;
            ActualSize_Click(sender, e);
            return;
        }

        if (hasPrimaryModifier && e.Key == Key.C)
        {
            if (_activeTool == ToolMode.Annotation && ImageCanvas.HasAnnotations)
            {
                e.Handled = true;
                await CopyAnnotationsAsync();
            }
            else if (_sampledColor is { } color
                && (_activeTool == ToolMode.Eyedropper
                    || _activeTool == ToolMode.Magnifier))
            {
                e.Handled = true;
                await CopyColorAsync(color, copyHex: true);
            }
            else if (ImageCanvas.HasMeasurements)
            {
                e.Handled = true;
                await CopyMeasurementsAsync();
            }

            return;
        }

        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            ImageCanvas.CancelMeasurementDraft();
            ImageCanvas.SetTemporaryPanEnabled(false);
            return;
        }

        if (HasPlainToolModifier(e.KeyModifiers) && ImageCanvas.Transform.HasImage)
        {
            switch (e.Key)
            {
                case Key.M:
                    e.Handled = true;
                    SetActiveTool(ToolMode.Measure);
                    return;
                case Key.I:
                    e.Handled = true;
                    SetActiveTool(ToolMode.Eyedropper);
                    return;
                case Key.Z:
                    e.Handled = true;
                    SetActiveTool(ToolMode.Magnifier);
                    return;
                case Key.T:
                    e.Handled = true;
                    SetActiveTool(ToolMode.Annotation);
                    return;
                case Key.OemOpenBrackets:
                    e.Handled = true;
                    ChangeMagnifierScale(-1);
                    return;
                case Key.OemCloseBrackets:
                    e.Handled = true;
                    ChangeMagnifierScale(1);
                    return;
            }
        }

        if (e.Key == Key.Back || e.Key == Key.Delete)
        {
            if (_activeTool == ToolMode.Annotation)
            {
                if (ImageCanvas.HasAnnotations)
                {
                    e.Handled = true;
                    ImageCanvas.ClearAnnotations();
                    SetStatusMessage("已清除全部批注", false);
                }
            }
            else if (ImageCanvas.HasMeasurements)
            {
                e.Handled = true;
                ImageCanvas.ClearMeasurements();
                SetStatusMessage("已清除全部测量", false);
            }
        }
    }

    private void MainWindow_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && ImageCanvas.Transform.HasImage)
        {
            ImageCanvas.SetTemporaryPanEnabled(false);
            e.Handled = true;
        }
    }

    private void MainWindow_Deactivated(object? sender, EventArgs e)
    {
        ImageCanvas.SetTemporaryPanEnabled(false);
    }

    private static bool HasPrimaryModifier(KeyModifiers modifiers)
    {
        return modifiers.HasFlag(KeyModifiers.Meta)
            || modifiers.HasFlag(KeyModifiers.Control);
    }

    private static bool HasPlainToolModifier(KeyModifiers modifiers)
    {
        return modifiers == KeyModifiers.None;
    }

    private void ChangeMagnifierScale(int direction)
    {
        if (_activeTool != ToolMode.Magnifier)
        {
            return;
        }

        var currentIndex = Array.IndexOf(MagnifierScales, ImageCanvas.MagnifierScale);
        if (currentIndex < 0)
        {
            currentIndex = 1;
        }

        var nextIndex = Math.Clamp(
            currentIndex + Math.Sign(direction),
            0,
            MagnifierScales.Length - 1);
        SetMagnifierScale(MagnifierScales[nextIndex]);
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

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        if (_hasShownStartupTutorial || !_preferences.ShouldShowTutorial)
        {
            return;
        }

        _hasShownStartupTutorial = true;
        await ShowTutorialAsync();
    }

    private async Task ShowTutorialAsync()
    {
        await new TutorialWindow().ShowDialog(this);
        _preferences.MarkTutorialSeen();
    }

    private async void Settings_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow(_preferences.CreateSettingsSnapshot());
        var result = await dialog.ShowDialog<AppSettingsSnapshot?>(this);
        if (result is not { } settings)
        {
            return;
        }

        _preferences.ApplySettings(settings);
        ImageCanvas.SetMagnifierScale(settings.MagnifierScale);
        UpdateMagnifierScaleControls();
        SetStatusMessage("设置已保存", false);
        ImageCanvas.Focus();
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
        UpdateOverlayActions();

        if (ImageCanvas.HasMeasurements)
        {
            var count = ImageCanvas.Measurements.Count;
            SetStatusMessage($"已记录 {count} 条测量", false);
        }
    }

    private void UpdateMeasurementActions()
    {
        var count = ImageCanvas.Measurements.Count;
        MeasurementCountTextBlock.Text = $"{count} 条";
    }

    private void ImageCanvas_AnnotationsChanged(object? sender, EventArgs e)
    {
        UpdateOverlayActions();

        if (ImageCanvas.HasAnnotations)
        {
            var count = ImageCanvas.Annotations.Count;
            SetStatusMessage($"已记录 {count} 条批注", false);
        }
    }

    private void ImageCanvas_EditHistoryChanged(object? sender, EventArgs e)
    {
        UpdateUndoRedoControls();
    }

    private void UndoEdit()
    {
        if (ImageCanvas.Undo() is not { } description)
        {
            return;
        }

        SetStatusMessage($"已撤销：{description}", false);
        UpdateUndoRedoControls();
    }

    private void RedoEdit()
    {
        if (ImageCanvas.Redo() is not { } description)
        {
            return;
        }

        SetStatusMessage($"已重做：{description}", false);
        UpdateUndoRedoControls();
    }

    private void UpdateUndoRedoControls()
    {
        UndoButton.IsEnabled = ImageCanvas.CanUndo;
        RedoButton.IsEnabled = ImageCanvas.CanRedo;
    }

    private void UpdateOverlayActions()
    {
        UpdateMeasurementActions();
        AnnotationCountTextBlock.Text = $"{ImageCanvas.Annotations.Count} 条";
        MeasurementActionsPanel.IsVisible =
            _activeTool == ToolMode.Measure && ImageCanvas.HasMeasurements;
        AnnotationActionsPanel.IsVisible =
            _activeTool == ToolMode.Annotation && ImageCanvas.HasAnnotations;
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

    private async Task CopyAnnotationsAsync()
    {
        if (!ImageCanvas.HasAnnotations)
        {
            SetStatusMessage("还没有可复制的批注", true);
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
                AnnotationFormatting.FormatClipboard(ImageCanvas.Annotations));
            SetStatusMessage($"已复制 {ImageCanvas.Annotations.Count} 条批注", false);
        }
        catch (Exception)
        {
            SetStatusMessage("复制批注失败", true);
        }
    }

    private void SetImageControlsEnabled(bool isEnabled)
    {
        FitButton.IsEnabled = isEnabled;
        ActualSizeButton.IsEnabled = isEnabled;
        ZoomOutButton.IsEnabled = isEnabled;
        ZoomInButton.IsEnabled = isEnabled;
        SaveEditedImageButton.IsEnabled = isEnabled;
        UpdateUndoRedoControls();
        MeasureToolButton.IsEnabled = isEnabled;
        EyedropperToolButton.IsEnabled = isEnabled;
        MagnifierToolButton.IsEnabled = isEnabled;
        AnnotationToolButton.IsEnabled = isEnabled;
        if (!isEnabled)
        {
            ImageCanvas.SetTemporaryPanEnabled(false);
            CloseAnnotationEditor(cancelDraft: true);
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
        AnnotationToolButton.IsChecked = tool == ToolMode.Annotation;
        ImageCanvas.SetMeasurementEnabled(tool == ToolMode.Measure);
        ImageCanvas.SetEyedropperEnabled(tool == ToolMode.Eyedropper);
        ImageCanvas.SetMagnifierEnabled(tool == ToolMode.Magnifier);
        ImageCanvas.SetAnnotationEnabled(tool == ToolMode.Annotation);
        MagnifierScalePanel.IsVisible = tool == ToolMode.Magnifier;
        UpdateMagnifierScaleControls();
        if (tool != ToolMode.Annotation)
        {
            CloseAnnotationEditor(cancelDraft: true);
        }

        if (tool != ToolMode.Eyedropper && tool != ToolMode.Magnifier)
        {
            UpdateSampledColor(null);
        }

        UpdateOverlayActions();
        SetStatusMessage(
            tool switch
            {
                ToolMode.Measure => "测量：左键拖动；Shift 锁定方向；空格拖动平移",
                ToolMode.Eyedropper => "取色：移动读取颜色；左键复制 HEX；空格拖动平移",
                ToolMode.Magnifier => "放大镜：移动检查像素；[ ] 调整倍率；空格拖动平移",
                ToolMode.Annotation => "批注：点击图片输入文字；Delete 清除全部批注",
                _ => string.Empty
            },
            false);
        ImageCanvas.Focus();
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
        ImageCanvas.SetTemporaryPanEnabled(false);
        _imageOpenCancellation?.Cancel();
        ImageCanvas.DisposeImage();
    }

    private void ConfigureShortcutHints()
    {
        var primary = OperatingSystem.IsMacOS() ? "⌘" : "Ctrl+";
        var clearShortcut = OperatingSystem.IsMacOS() ? "⌫" : "Delete";

        ToolTip.SetTip(
            OpenImageButton,
            $"打开图片 ({primary}O)");
        ToolTip.SetTip(
            SaveEditedImageButton,
            $"保存带测量和批注的图片 ({primary}S)");
        ToolTip.SetTip(
            UndoButton,
            $"撤销上一次编辑 ({primary}Z)");
        ToolTip.SetTip(
            RedoButton,
            OperatingSystem.IsMacOS()
                ? "重做/前进 (⌘⇧Z)"
                : "重做/前进 (Ctrl+Y 或 Ctrl+Shift+Z)");
        ToolTip.SetTip(
            MeasureToolButton,
            "测量像素距离 (M)；按住 Shift 锁定水平或垂直");
        ToolTip.SetTip(
            EyedropperToolButton,
            "吸取图片像素颜色 (I)；左键复制 HEX");
        ToolTip.SetTip(
            MagnifierToolButton,
            "放大并检查单个像素 (Z)；[ ] 调整倍率");
        ToolTip.SetTip(
            AnnotationToolButton,
            "在图片上添加可自定义颜色的文字批注 (T)");
        ToolTip.SetTip(
            FitButton,
            $"图片适应窗口 ({primary}0)");
        ToolTip.SetTip(
            ActualSizeButton,
            $"恢复 100% 原始尺寸 ({primary}1)");
        ToolTip.SetTip(
            ZoomOutButton,
            $"缩小 ({primary}−)");
        ToolTip.SetTip(
            ZoomInButton,
            $"放大 ({primary}+)");
        ToolTip.SetTip(
            Magnifier8Button,
            "每像素放大 8 倍 ([ / ] 切换)");
        ToolTip.SetTip(
            Magnifier12Button,
            "每像素放大 12 倍 ([ / ] 切换)");
        ToolTip.SetTip(
            Magnifier16Button,
            "每像素放大 16 倍 ([ / ] 切换)");
        ToolTip.SetTip(
            CopyMeasurementsButton,
            $"复制全部测量结果 ({primary}C)");
        ToolTip.SetTip(
            ClearMeasurementsButton,
            $"清除全部测量 ({clearShortcut})");
        ToolTip.SetTip(
            CopySampledHexButton,
            $"复制当前 HEX 颜色 ({primary}C)");
        ToolTip.SetTip(
            CopySampledRgbButton,
            "复制当前 RGB 颜色");
        ToolTip.SetTip(
            CopyAnnotationsButton,
            $"复制全部批注 ({primary}C)");
        ToolTip.SetTip(
            ClearAnnotationsButton,
            $"清除全部批注 ({clearShortcut})");
        ToolTip.SetTip(
            SettingsButton,
            "设置");

        EmptyStateShortcutTextBlock.Text =
            $"PNG · JPG · BMP · WebP     {primary}O";
        AnnotationEditorShortcutTextBlock.Text =
            $"{(OperatingSystem.IsMacOS() ? "⌘" : "Ctrl+")}↩ 保存   Esc 取消";
    }
}
