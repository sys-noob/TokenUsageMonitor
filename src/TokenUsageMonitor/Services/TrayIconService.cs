using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using TokenUsageMonitor.ViewModels;
using TokenUsageMonitor.Views;

namespace TokenUsageMonitor.Services;

public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MainWindow _mainWindow;

    public ICommand RefreshCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand ExitCommand { get; }

    public TrayIconService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        RefreshCommand = new RelayCommand(OnRefresh);
        SettingsCommand = new RelayCommand(OnSettings);
        AboutCommand = new RelayCommand(OnAbout);
        ExitCommand = new RelayCommand(OnExit);

        _notifyIcon = new NotifyIcon
        {
            Icon = CreateTrayIcon(),
            Text = "Token 用量监控",
            Visible = true,
            ContextMenuStrip = CreateContextMenu()
        };

        _notifyIcon.MouseClick += OnTrayMouseClick;

        // 启动后显示气泡提示，让用户知道程序已运行
        _notifyIcon.ShowBalloonTip(3000, "Token 用量监控", "程序已启动，单击托盘图标查看用量", ToolTipIcon.Info);
    }

    private void OnTrayMouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ToggleMainWindow();
        }
    }

    private void ToggleMainWindow()
    {
        if (_mainWindow.IsVisible)
        {
            _mainWindow.Hide();
        }
        else
        {
            _mainWindow.ShowNearTrayIcon();
        }
    }

    private void OnRefresh()
    {
        if (_mainWindow.DataContext is MainViewModel vm)
        {
            vm.RefreshCommand.Execute(null);
        }
    }

    private static void OnSettings()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }

    private static void OnAbout()
    {
        System.Windows.MessageBox.Show("Token Usage Monitor v1.0", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void OnExit()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private static System.Drawing.Icon CreateTrayIcon()
    {
        using var bitmap = new System.Drawing.Bitmap(16, 16);
        using (var g = System.Drawing.Graphics.FromImage(bitmap))
        {
            g.Clear(System.Drawing.Color.Transparent);
            using var brush = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#B5A89A"));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillEllipse(brush, 0, 0, 15, 15);
        }
        var hIcon = bitmap.GetHicon();
        var icon = System.Drawing.Icon.FromHandle(hIcon);
        _ = DestroyIcon(hIcon);
        return icon;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(nint hIcon);

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(CreateMenuItem("刷新", OnRefresh));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CreateMenuItem("设置", () => SettingsCommand.Execute(null)));
        menu.Items.Add(CreateMenuItem("关于", () => AboutCommand.Execute(null)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CreateMenuItem("退出", () => ExitCommand.Execute(null)));
        return menu;
    }

    private static ToolStripMenuItem CreateMenuItem(string text, Action action)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += (s, e) => action();
        return item;
    }

    public void Dispose()
    {
        _notifyIcon.MouseClick -= OnTrayMouseClick;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
