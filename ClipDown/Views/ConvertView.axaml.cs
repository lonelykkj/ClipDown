using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ClipDown.Views;

public partial class ConvertView : UserControl
{
    public ConvertView()
    {
        InitializeComponent();
    }
    
    public void GoToMenu(object? sender, RoutedEventArgs args)
    {
        this.Content = new Views.MainWindow();
    }
}