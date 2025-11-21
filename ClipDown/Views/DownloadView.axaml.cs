using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ClipDown.Views;

public partial class DownloadView : UserControl
{
    public DownloadView()
    {
        InitializeComponent();
    }
    
    public void GoToMenu(object? sender, RoutedEventArgs args)
    {
        this.Content = new Views.MainWindow();
    }
}