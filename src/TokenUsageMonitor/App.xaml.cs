using System.Windows;
using TokenUsageMonitor.Services;
using TokenUsageMonitor.Views;

namespace TokenUsageMonitor;

public partial class App : System.Windows.Application
{
    private TrayIconService? _trayIconService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        _trayIconService = new TrayIconService(mainWindow);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        base.OnExit(e);
    }
}
