using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace TokenUsageMonitor.Services;

public static class ThemeManager
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private static bool _isDarkMode;

    public static bool IsDarkMode => _isDarkMode;

    public static event Action<string>? ThemeChanged;

    public static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int value)
                return value == 0;
        }
        catch { }
        return false;
    }

    public static void ApplySystemTheme()
    {
        var settings = SettingsService.Instance.Load();
        ApplyTheme(settings.Theme);
    }

    public static void ToggleTheme()
    {
        var settings = SettingsService.Instance.Load();
        var current = settings.Theme;
        var next = current switch
        {
            "System" => "Light",
            "Light" => "Dark",
            _ => "System"
        };
        ApplyTheme(next);
    }

    private static System.Windows.ResourceDictionary? _darkThemeDict;
    private static DateTime _lastThemeChange = DateTime.MinValue;

    public static void ApplyTheme(string themeName)
    {
        // Debounce: ignore rapid clicks within 300ms
        if ((DateTime.Now - _lastThemeChange).TotalMilliseconds < 300)
            return;
        _lastThemeChange = DateTime.Now;

        if (string.IsNullOrWhiteSpace(themeName))
            themeName = "System";

        bool isDark = themeName switch
        {
            "System" => IsSystemDarkMode(),
            "Dark" => true,
            _ => false
        };

        _isDarkMode = isDark;
        UpdateWindowDarkMode(themeName);
        UpdateApplicationResources(isDark);

        ThemeChanged?.Invoke(themeName);

        try
        {
            var settings = SettingsService.Instance.Load();
            settings.Theme = themeName;
            SettingsService.Instance.Save(settings);
        }
        catch { }
    }

    private static void UpdateApplicationResources(bool isDark)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        var merged = app.Resources.MergedDictionaries;

        if (_darkThemeDict != null)
        {
            merged.Remove(_darkThemeDict);
            _darkThemeDict = null;
        }

        if (isDark)
        {
            _darkThemeDict = new System.Windows.ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/TokenUsageMonitor;component/Assets/Themes/DarkTheme.xaml")
            };
            merged.Add(_darkThemeDict);
        }
    }

    private static void UpdateWindowDarkMode(string themeName)
    {
        int darkModeValue = themeName switch
        {
            "System" => IsSystemDarkMode() ? 1 : 0,
            "Dark" => 1,
            _ => 0
        };

        var app = System.Windows.Application.Current;
        if (app == null) return;

        foreach (System.Windows.Window window in app.Windows)
        {
            if (window.IsLoaded)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    try
                    {
                        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkModeValue, sizeof(int));
                    }
                    catch { }
                }
            }
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int cbAttr);
}
