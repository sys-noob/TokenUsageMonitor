using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using TokenUsageMonitor.Services;
using TokenUsageMonitor.Views;

namespace TokenUsageMonitor;

public partial class App : System.Windows.Application
{
    private TrayIconService? _trayIconService;

    private static string LogPath => Path.Combine(AppContext.BaseDirectory, "error.log");

    private static void WriteLog(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, line);
        }
        catch
        {
            // 如果连文件都写不了，用最后的兜底方式
        }
    }

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            WriteLog($"[FATAL] UnhandledException: {e.ExceptionObject}");
            try
            {
                System.Windows.MessageBox.Show(
                    $"程序发生未处理异常：{e.ExceptionObject}",
                    "TokenUsageMonitor 错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch { }
        };

        DispatcherUnhandledException += (s, e) =>
        {
            WriteLog($"[FATAL] DispatcherUnhandledException: {e.Exception}");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            WriteLog($"[WARN] UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            WriteLog("========== OnStartup begin ==========");
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            WriteLog($"ShutdownMode set. BaseDirectory={AppContext.BaseDirectory}");

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            WriteLog("MainWindow created OK");

            ThemeManager.ApplySystemTheme();
            WriteLog($"Theme applied. DarkMode={ThemeManager.IsDarkMode}");

            _trayIconService = new TrayIconService(mainWindow);
            WriteLog("TrayIconService created OK");

            // 首次启动引导
            bool hasAnyKey = SecureStorageService.Instance.HasApiKey("GLM")
                          || SecureStorageService.Instance.HasApiKey("KIMI")
                          || SecureStorageService.Instance.HasApiKey("DeepSeek");

            if (!hasAnyKey)
            {
                WriteLog("No API key found, showing SettingsWindow for first-time setup");
                var settingsWindow = new Views.SettingsWindow();
                var result = settingsWindow.ShowDialog();
                if (result != true)
                {
                    WriteLog("User cancelled first-time setup, shutting down");
                    Shutdown();
                    return;
                }
                WriteLog("First-time setup completed");
            }
        }
        catch (Exception ex)
        {
            WriteLog($"[FATAL] OnStartup FAILED: {ex}");
            try
            {
                System.Windows.MessageBox.Show(
                    $"启动失败：{ex.Message}\n\n详细错误已写入：{LogPath}",
                    "TokenUsageMonitor 启动错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch { }
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        WriteLog("OnExit called");
        _trayIconService?.Dispose();
        base.OnExit(e);
    }
}
