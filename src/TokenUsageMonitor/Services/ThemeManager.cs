using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace TokenUsageMonitor.Services;

public static class ThemeManager
{
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
        var theme = settings.Theme;
        if (theme == "Light" || theme == "Dark")
        {
            ApplyTheme(theme);
        }
        else
        {
            // First run or reset — follow system
            ApplyTheme(IsSystemDarkMode() ? "Dark" : "Light");
        }
    }

    public static void ToggleTheme()
    {
        ApplyTheme(_isDarkMode ? "Light" : "Dark");
    }

    public static void ApplyTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
            themeName = "Light";

        bool isDark = themeName.Contains("dark", StringComparison.OrdinalIgnoreCase)
                   || themeName.Contains("Dark", StringComparison.OrdinalIgnoreCase);

        string sourcePath = isDark
            ? "Assets/Themes/MorandiDarkTheme.xaml"
            : "Assets/Themes/MorandiTheme.xaml";

        var app = System.Windows.Application.Current;
        if (app == null) return;

        var mergedDicts = app.Resources.MergedDictionaries;

        for (int i = mergedDicts.Count - 1; i >= 0; i--)
        {
            var dict = mergedDicts[i];
            if (dict.Source != null)
            {
                var source = dict.Source.OriginalString;
                if (source.Contains("MorandiTheme") || source.Contains("MorandiDarkTheme"))
                {
                    mergedDicts.RemoveAt(i);
                }
            }
        }

        var newDict = new ResourceDictionary
        {
            Source = new Uri(sourcePath, UriKind.Relative)
        };
        mergedDicts.Add(newDict);

        _isDarkMode = isDark;
        ThemeChanged?.Invoke(themeName);
        TryPersistTheme(themeName);
    }

    private static void TryPersistTheme(string themeName)
    {
        try
        {
            var settings = SettingsService.Instance.Load();
            settings.Theme = themeName;
            SettingsService.Instance.Save(settings);
        }
        catch { }
    }
}