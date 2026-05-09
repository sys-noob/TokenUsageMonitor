using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using TokenUsageMonitor.Helpers;
using TokenUsageMonitor.ViewModels;

namespace TokenUsageMonitor.Views;

public partial class MainWindow : Window
{
    private bool _autoHideOnLostFocus = true;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        LoadSettings();
        Deactivated += OnDeactivated;
    }

    private void LoadSettings()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "Config", "AppSettings.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("autoHideOnLostFocus", out var prop))
                {
                    _autoHideOnLostFocus = prop.GetBoolean();
                }
            }
        }
        catch
        {
            // Use default value
        }
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (_autoHideOnLostFocus)
        {
            Hide();
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    public void ShowNearTrayIcon()
    {
        Show();
        Activate();

        var trayPos = System.Windows.Forms.Cursor.Position;
        var position = WindowPositionHelper.CalculatePopupPosition(this, trayPos);
        Left = position.X;
        Top = position.Y;
    }
}
