using System;
using System.IO;
using System.Text.Json;

namespace TokenUsageMonitor.Services;

public class AppSettings
{
    public int RefreshIntervalMinutes { get; set; } = 5;
    public bool AutoHideOnLostFocus { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;
    public string Theme { get; set; } = "morandi_light";
}

public class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    private SettingsService()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TokenUsageMonitor");

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        _configFilePath = Path.Combine(configDirectory, "appsettings.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public AppSettings Load()
    {
        if (!File.Exists(_configFilePath))
        {
            var defaultSettings = new AppSettings();
            Save(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_configFilePath, json);
    }
}
