using System.Windows;
using System.Windows.Input;
using TokenUsageMonitor.ViewModels;

namespace TokenUsageMonitor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
