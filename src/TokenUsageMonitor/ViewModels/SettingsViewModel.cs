using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokenUsageMonitor.Services;

namespace TokenUsageMonitor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _glmApiKey = string.Empty;

    [ObservableProperty]
    private string _kimiApiKey = string.Empty;

    [ObservableProperty]
    private string _deepSeekApiKey = string.Empty;

    [ObservableProperty]
    private int _refreshIntervalMinutes = 5;

    [ObservableProperty]
    private bool _autoHideOnLostFocus = true;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private string _selectedTheme = "System";

    public int[] RefreshIntervalOptions { get; } = { 1, 5, 15, 30, 0 };

    public string[] ThemeOptions { get; } = { "System", "Light", "Dark" };

    public SettingsViewModel()
    {
        LoadSettings();
        LoadApiKeys();
    }

    private void LoadSettings()
    {
        var settings = SettingsService.Instance.Load();
        RefreshIntervalMinutes = settings.RefreshIntervalMinutes;
        AutoHideOnLostFocus = settings.AutoHideOnLostFocus;
        StartWithWindows = settings.StartWithWindows;
        SelectedTheme = settings.Theme switch
        {
            "morandi_light" => "Light",
            "morandi_dark" => "Dark",
            "" or null => "System",
            _ => settings.Theme
        };
    }

    private void LoadApiKeys()
    {
        GlmApiKey = SecureStorageService.Instance.LoadApiKey("GLM") ?? string.Empty;
        KimiApiKey = SecureStorageService.Instance.LoadApiKey("KIMI") ?? string.Empty;
        DeepSeekApiKey = SecureStorageService.Instance.LoadApiKey("DeepSeek") ?? string.Empty;
    }

    [RelayCommand]
    private void SaveApiKey(string platform)
    {
        string? key = platform switch
        {
            "GLM" => GlmApiKey,
            "KIMI" => KimiApiKey,
            "DeepSeek" => DeepSeekApiKey,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(key))
        {
            SecureStorageService.Instance.SaveApiKey(platform, key);
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            RefreshIntervalMinutes = RefreshIntervalMinutes,
            AutoHideOnLostFocus = AutoHideOnLostFocus,
            StartWithWindows = StartWithWindows,
            Theme = SelectedTheme
        };

        SettingsService.Instance.Save(settings);

        if (!string.IsNullOrWhiteSpace(GlmApiKey))
            SecureStorageService.Instance.SaveApiKey("GLM", GlmApiKey);
        if (!string.IsNullOrWhiteSpace(KimiApiKey))
            SecureStorageService.Instance.SaveApiKey("KIMI", KimiApiKey);
        if (!string.IsNullOrWhiteSpace(DeepSeekApiKey))
            SecureStorageService.Instance.SaveApiKey("DeepSeek", DeepSeekApiKey);

        ThemeManager.ApplySystemTheme();

        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
        {
            if (window is Views.SettingsWindow settingsWindow)
            {
                settingsWindow.DialogResult = true;
                settingsWindow.Close();
                break;
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
        {
            if (window is Views.SettingsWindow settingsWindow)
            {
                settingsWindow.DialogResult = false;
                settingsWindow.Close();
                break;
            }
        }
    }
}