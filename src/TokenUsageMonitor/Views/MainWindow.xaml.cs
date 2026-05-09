using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TokenUsageMonitor.Helpers;
using TokenUsageMonitor.Services;
using TokenUsageMonitor.ViewModels;

namespace TokenUsageMonitor.Views;

public partial class MainWindow : Window
{
    private bool _autoHideOnLostFocus = true;
    private DateTime _showTime = DateTime.MinValue;
    private double _savedLeft = double.NaN;
    private double _savedTop = double.NaN;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        LoadSettings();
        Deactivated += OnDeactivated;
        SourceInitialized += (_, _) =>
        {
            Left = -10000;
            Top = -10000;
        };
    }

    private void LoadSettings()
    {
        try
        {
            var settings = SettingsService.Instance.Load();
            _autoHideOnLostFocus = settings.AutoHideOnLostFocus;
            if (!double.IsNaN(settings.WindowLeft) && !double.IsNaN(settings.WindowTop))
            {
                _savedLeft = settings.WindowLeft;
                _savedTop = settings.WindowTop;
            }
        }
        catch { }
    }

    private void SavePosition()
    {
        try
        {
            if (double.IsNaN(Left) || double.IsNaN(Top)) return;
            var settings = SettingsService.Instance.Load();
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            SettingsService.Instance.Save(settings);
        }
        catch { }
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if ((DateTime.Now - _showTime).TotalMilliseconds < 300)
            return;

        if (_autoHideOnLostFocus)
        {
            HideWithAnimation();
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HideWithAnimation();
    }

    public void HideWithAnimation()
    {
        SavePosition();
        var border = FindName("RootBorder") as FrameworkElement;
        if (border == null) { Hide(); return; }

        var sb = System.Windows.Application.Current.Resources["PopupFadeOut"] as Storyboard;
        if (sb != null)
        {
            var clone = sb.Clone();
            clone.Completed += (_, _) => Hide();
            clone.Begin(border);
        }
        else
        {
            Hide();
        }
    }

    public void ShowNearTrayIcon()
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "error.log");
        try
        {
            _showTime = DateTime.Now;

            var border = FindName("RootBorder") as FrameworkElement;
            if (border != null)
            {
                border.Opacity = 0;
                border.RenderTransform = new TranslateTransform(0, 8);
            }

            Show();
            Activate();

            double x, y;

            // Prefer saved position, then calculate from tray, then center
            if (!double.IsNaN(_savedLeft) && !double.IsNaN(_savedTop))
            {
                x = _savedLeft;
                y = _savedTop;
            }
            else
            {
                var trayPos = System.Windows.Forms.Cursor.Position;
                var pos = WindowPositionHelper.CalculatePopupPosition(this, trayPos);

                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                if (screen == null) { x = pos.X; y = pos.Y; return; }
                var dpiX = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
                var dpiY = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice.M22 ?? 1.0;
                var screenW = screen.WorkingArea.Width / dpiX;
                var screenH = screen.WorkingArea.Height / dpiY;
                var screenL = screen.WorkingArea.Left / dpiX;
                var screenT = screen.WorkingArea.Top / dpiY;

                if (pos.X < screenL || pos.X + Width > screenL + screenW
                    || pos.Y < screenT || pos.Y + Height > screenT + screenH)
                {
                    pos.X = screenL + (screenW - Width) / 2;
                    pos.Y = screenT + (screenH - Height) / 2;
                }

                x = pos.X;
                y = pos.Y;
            }

            Left = x;
            Top = y;

            var fadeIn = System.Windows.Application.Current.Resources["PopupFadeIn"] as Storyboard;
            if (fadeIn != null && border != null)
            {
                var clone = fadeIn.Clone();
                clone.Begin(border);
            }
        }
        catch (Exception ex)
        {
            try { File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] [FATAL] ShowNearTrayIcon: {ex}\n"); }
            catch { }
        }
    }
}