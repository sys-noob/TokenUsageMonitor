using System.Windows;
using System.Windows.Controls;
using TokenUsageMonitor.ViewModels;

namespace TokenUsageMonitor.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsWindow()
    {
        InitializeComponent();
        _vm = new SettingsViewModel();
        DataContext = _vm;
        Loaded += (_, _) => ApplyLoadedKeys();
    }

    private void ApplyLoadedKeys()
    {
        GlmPasswordBox.Password = _vm.GlmApiKey;
        KimiPasswordBox.Password = _vm.KimiApiKey;
        DeepSeekPasswordBox.Password = _vm.DeepSeekApiKey;
    }

    private void GlmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.GlmApiKey = GlmPasswordBox.Password;
    }

    private void KimiPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.KimiApiKey = KimiPasswordBox.Password;
    }

    private void DeepSeekPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.DeepSeekApiKey = DeepSeekPasswordBox.Password;
    }
}