using System;
using System.Threading;
using System.Windows;
using TokenUsageMonitor.Views;
using TokenUsageMonitor.ViewModels;
using TokenUsageMonitor.Services;

namespace WindowTest;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Prevent multiple instances
        bool createdNew;
        using (var mutex = new System.Threading.Mutex(true, "TokenUsageMonitor_WindowTest", out createdNew))
        {
            if (!createdNew)
            {
                Console.WriteLine("Already running");
                return;
            }

            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize the window
            var window = new MainWindow();
            Console.WriteLine("MainWindow created");

            // Wait a moment then show
            var timer = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromSeconds(1),
                System.Windows.Threading.DispatcherPriority.Normal,
                (s, e) =>
                {
                    Console.WriteLine($"Before Show: IsVisible={window.IsVisible}");
                    window.ShowNearTrayIcon();
                    Console.WriteLine($"After Show: IsVisible={window.IsVisible}, Left={window.Left}, Top={window.Top}");
                },
                System.Windows.Threading.Dispatcher.CurrentDispatcher);
            timer.Start();

            // Keep app running for a while
            var shutdownTimer = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromSeconds(5),
                System.Windows.Threading.DispatcherPriority.Normal,
                (s, e) => app.Shutdown(),
                System.Windows.Threading.Dispatcher.CurrentDispatcher);
            shutdownTimer.Start();

            app.Run();
            Console.WriteLine("App shutdown");
        }
    }
}
