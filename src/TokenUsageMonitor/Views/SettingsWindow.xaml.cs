using System.Windows;
using System.Windows.Controls;
using TokenUsageMonitor.ViewModels;

namespace TokenUsageMonitor.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }

    private void GlmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is PasswordBox pb)
        {
            vm.GlmApiKey = pb.Password;
        }
    }

    private void KimiPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is PasswordBox pb)
        {
            vm.KimiApiKey = pb.Password;
        }
    }

    private void DeepSeekPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is PasswordBox pb)
        {
            vm.DeepSeekApiKey = pb.Password;
        }
    }
}
