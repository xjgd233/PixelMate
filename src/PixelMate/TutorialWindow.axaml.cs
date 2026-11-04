using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PixelMate;

public partial class TutorialWindow : Window
{
    public TutorialWindow()
    {
        InitializeComponent();
        KeyDown += TutorialWindow_KeyDown;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TutorialWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
