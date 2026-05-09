using System;
using System.Linq;
using System.Windows;

namespace TokenUsageMonitor.Services;

public static class ThemeManager
{
    private static bool _isDarkMode = false;

    public static bool IsDarkMode => _isDarkMode;

    public static event Action<string>? ThemeChanged;

    public static void ToggleTheme()
    {
        ApplyTheme(_isDarkMode ? "Light" : "Dark");
    }

    public static void ApplyTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            themeName = "Light";
        }

        bool isDark = themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase);

        string sourcePath = isDark
            ? "Assets/Themes/MorandiDarkTheme.xaml"
            : "Assets/Themes/MorandiTheme.xaml";

        var app = System.Windows.Application.Current;
        if (app == null) return;

        var mergedDicts = app.Resources.MergedDictionaries;

        // Remove existing theme dictionaries
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

        // Add new theme dictionary
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
            var assembly = typeof(ThemeManager).Assembly;
            var settingsType = assembly.GetType("TokenUsageMonitor.Services.SettingsService");
            if (settingsType != null)
            {
                var method = settingsType.GetMethod(
                    "SaveThemePreferenceAsync",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    var instance = method.IsStatic ? null : Activator.CreateInstance(settingsType);
                    method.Invoke(instance, new object?[] { themeName });
                }
            }
        }
        catch
        {
            // SettingsService not yet available — preference will not be persisted.
            // Implement ISettingsService and create SettingsService to enable persistence.
        }
    }
}
