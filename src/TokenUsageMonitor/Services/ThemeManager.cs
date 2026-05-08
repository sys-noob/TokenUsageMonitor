using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace TokenUsageMonitor.Services;

public static class ThemeManager
{
    private static bool _isDarkMode = false;

    private static readonly Dictionary<string, string> LightColors = new()
    {
        ["MainBackgroundBrush"] = "#E8E3DF",
        ["CardBackgroundBrush"] = "#F5F0EB",
        ["PrimaryTextBrush"] = "#4A4A4A",
        ["SecondaryTextBrush"] = "#9B9B9B",
        ["BorderBrush"] = "#D0C8C0",
        ["ProgressBarBackgroundBrush"] = "#D5CEC7",
        ["ProgressBarFillBrush"] = "#B5A89A",
    };

    private static readonly Dictionary<string, string> DarkColors = new()
    {
        ["MainBackgroundBrush"] = "#2D2A28",
        ["CardBackgroundBrush"] = "#3A3532",
        ["PrimaryTextBrush"] = "#D8D4D0",
        ["SecondaryTextBrush"] = "#8A8580",
        ["BorderBrush"] = "#4A4540",
        ["ProgressBarBackgroundBrush"] = "#4A4540",
        ["ProgressBarFillBrush"] = "#B5A89A",
    };

    public static bool IsDarkMode => _isDarkMode;

    public static void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        ApplyTheme(_isDarkMode ? DarkColors : LightColors);
    }

    private static void ApplyTheme(Dictionary<string, string> colors)
    {
        var resources = System.Windows.Application.Current.Resources;
        foreach (var pair in colors)
        {
            if (resources[pair.Key] is SolidColorBrush brush)
            {
                brush.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(pair.Value)!;
            }
        }
    }
}
