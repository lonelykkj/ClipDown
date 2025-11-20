using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClipDown.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void GoToDownload(object? sender, RoutedEventArgs args)
    {
        this.Content = new Views.DownloadView();
    }
}