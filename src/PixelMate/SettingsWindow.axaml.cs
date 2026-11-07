using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PixelMate.Preferences;
using System.Reflection;

namespace PixelMate;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
        : this(new AppSettingsSnapshot(true, false, 12))
    {
    }

    public SettingsWindow(AppSettingsSnapshot settings)
    {
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        SettingsVersionTextBlock.Text = version is null
            ? "PixelMate 1.0.1"
            : $"PixelMate {version.Major}.{version.Minor}.{version.Build}";
        RememberDirectoryCheckBox.IsChecked = settings.RememberLastDirectory;
        ShowTutorialEveryLaunchCheckBox.IsChecked = settings.ShowTutorialEveryLaunch;
        Magnifier8RadioButton.IsChecked = settings.MagnifierScale == 8;
        Magnifier12RadioButton.IsChecked = settings.MagnifierScale == 12;
        Magnifier16RadioButton.IsChecked = settings.MagnifierScale == 16;
        KeyDown += SettingsWindow_KeyDown;
    }

    private async void OpenTutorial_Click(object? sender, RoutedEventArgs e)
    {
        await new TutorialWindow().ShowDialog(this);
    }

    private async void OpenAbout_Click(object? sender, RoutedEventArgs e)
    {
        await new AboutWindow().ShowDialog(this);
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        var scale = Magnifier8RadioButton.IsChecked == true
            ? 8
            : Magnifier16RadioButton.IsChecked == true
                ? 16
                : 12;
        Close(new AppSettingsSnapshot(
            RememberDirectoryCheckBox.IsChecked == true,
            ShowTutorialEveryLaunchCheckBox.IsChecked == true,
            scale));
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void SettingsWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close(null);
        }
    }
}
