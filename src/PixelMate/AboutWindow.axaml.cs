using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixelMate.About;
using System.Diagnostics;
using System.Reflection;

namespace PixelMate;

public partial class AboutWindow : Window
{
    private Bitmap? _avatarBitmap;
    private Bitmap? _sponsorBitmap;

    public AboutWindow()
    {
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionTextBlock.Text = version is null
            ? "Version 1.0.0"
            : $"Version {version.Major}.{version.Minor}.{version.Build}";
        GitHubButton.Content = AppIdentity.GitHubUrl
            .Replace("https://github.com/", string.Empty, StringComparison.OrdinalIgnoreCase);
        LoadAboutImages();
        Closed += AboutWindow_Closed;
        KeyDown += AboutWindow_KeyDown;
    }

    private async void Qq_Click(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            ContactStatusTextBlock.Text = "当前剪贴板不可用";
            return;
        }

        try
        {
            await clipboard.SetTextAsync(AppIdentity.QqNumber);
            ContactStatusTextBlock.Text = $"QQ {AppIdentity.QqNumber} 已复制";
        }
        catch (Exception)
        {
            ContactStatusTextBlock.Text = "复制 QQ 失败";
        }
    }

    private void GitHub_Click(object? sender, RoutedEventArgs e)
    {
        OpenExternal(AppIdentity.GitHubUrl, "无法打开 GitHub 主页");
    }

    private void Email_Click(object? sender, RoutedEventArgs e)
    {
        OpenExternal($"mailto:{AppIdentity.EmailAddress}", "无法打开邮件应用");
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OpenExternal(string target, string failureMessage)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target)
            {
                UseShellExecute = true
            });
            ContactStatusTextBlock.Text = string.Empty;
        }
        catch (Exception)
        {
            ContactStatusTextBlock.Text = failureMessage;
        }
    }

    private void LoadAboutImages()
    {
        var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        _avatarBitmap = LoadBitmap(
            AboutImageLocator.FindPicture(pictures, ["avatar", "头像"]),
            "avares://PixelMate/Assets/About/avatar.png");
        _sponsorBitmap = LoadBitmap(
            AboutImageLocator.FindPicture(pictures, ["sponsor", "赞助", "收款码", "赞赏"]),
            "avares://PixelMate/Assets/About/sponsor.png");

        if (_avatarBitmap is not null)
        {
            AvatarImage.Source = _avatarBitmap;
            AvatarImage.IsVisible = true;
            AvatarPlaceholder.IsVisible = false;
        }

        if (_sponsorBitmap is not null)
        {
            SponsorImage.Source = _sponsorBitmap;
            SponsorImage.IsVisible = true;
            SponsorPlaceholder.IsVisible = false;
        }
    }

    private static Bitmap? LoadBitmap(string? filePath, string assetPath)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                return new Bitmap(filePath);
            }

            var uri = new Uri(assetPath);
            return AssetLoader.Exists(uri)
                ? new Bitmap(AssetLoader.Open(uri))
                : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void AboutWindow_Closed(object? sender, EventArgs e)
    {
        _avatarBitmap?.Dispose();
        _sponsorBitmap?.Dispose();
    }

    private void AboutWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
