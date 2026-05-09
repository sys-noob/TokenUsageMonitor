namespace TokenUsageMonitor.Services;

/// <summary>
/// Interface for persisting user preferences such as theme selection.
/// Implement this in SettingsService to enable theme preference persistence.
/// </summary>
public interface ISettingsService
{
    System.Threading.Tasks.Task SaveThemePreferenceAsync(string themeName);
    System.Threading.Tasks.Task<string> LoadThemePreferenceAsync();
}
