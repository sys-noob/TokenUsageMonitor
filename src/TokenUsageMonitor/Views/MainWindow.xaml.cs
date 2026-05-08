using System.Windows;
using TokenUsageMonitor.ViewModels;

namespace TokenUsageMonitor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
