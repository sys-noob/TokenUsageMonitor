using System;
using System.Windows;
using Screen = System.Windows.Forms.Screen;
using System.Windows.Media;

namespace TokenUsageMonitor.Helpers;

public static class WindowPositionHelper
{
    public static System.Windows.Point CalculatePopupPosition(Window window, System.Drawing.Point trayPosition)
    {
        var screen = Screen.FromPoint(trayPosition);
        var workingArea = screen.WorkingArea;
        var bounds = screen.Bounds;

        // Convert physical pixels to WPF device-independent pixels (DIPs)
        double scaleX = 1.0;
        double scaleY = 1.0;

        var source = PresentationSource.FromVisual(window);
        if (source?.CompositionTarget != null)
        {
            scaleX = source.CompositionTarget.TransformFromDevice.M11;
            scaleY = source.CompositionTarget.TransformFromDevice.M22;
        }

        double workLeft = workingArea.Left * scaleX;
        double workTop = workingArea.Top * scaleY;
        double workRight = workingArea.Right * scaleX;
        double workBottom = workingArea.Bottom * scaleY;
        double trayX = trayPosition.X * scaleX;
        double trayY = trayPosition.Y * scaleY;
        double windowWidth = window.Width;
        double windowHeight = window.Height;

        // Determine taskbar position
        bool taskbarAtTop = workingArea.Top > bounds.Top;
        bool taskbarAtLeft = workingArea.Left > bounds.Left;
        bool taskbarAtRight = workingArea.Right < bounds.Right;

        double x, y;

        if (taskbarAtTop)
        {
            x = trayX - windowWidth / 2;
            y = workTop;
        }
        else if (taskbarAtLeft)
        {
            x = workLeft;
            y = trayY - windowHeight / 2;
        }
        else if (taskbarAtRight)
        {
            x = workRight - windowWidth;
            y = trayY - windowHeight / 2;
        }
        else // Taskbar at bottom (default)
        {
            x = trayX - windowWidth / 2;
            y = trayY - windowHeight;
        }

        // Ensure window stays within working area
        x = Math.Max(workLeft, Math.Min(x, workRight - windowWidth));
        y = Math.Max(workTop, Math.Min(y, workBottom - windowHeight));

        return new System.Windows.Point(x, y);
    }
}
