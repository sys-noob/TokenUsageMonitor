using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TokenUsageMonitor.Services;

public class SecureStorageService
{
    private static readonly Lazy<SecureStorageService> _instance = new(() => new SecureStorageService());
    public static SecureStorageService Instance => _instance.Value;

    private readonly string _storageDirectory;

    private SecureStorageService()
    {
        _storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TokenUsageMonitor");

        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
    }

    public void SaveApiKey(string platform, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(platform))
            throw new ArgumentException("Platform cannot be null or empty.", nameof(platform));

        var filePath = Path.Combine(_storageDirectory, $"keys.{platform}.encrypted");
        var bytes = Encoding.UTF8.GetBytes(apiKey);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(filePath, encrypted);
    }

    public string? LoadApiKey(string platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
            throw new ArgumentException("Platform cannot be null or empty.", nameof(platform));

        var filePath = Path.Combine(_storageDirectory, $"keys.{platform}.encrypted");
        if (!File.Exists(filePath))
            return null;

        var encrypted = File.ReadAllBytes(filePath);
        var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decrypted);
    }

    public bool HasApiKey(string platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
            return false;

        var filePath = Path.Combine(_storageDirectory, $"keys.{platform}.encrypted");
        return File.Exists(filePath);
    }
}
