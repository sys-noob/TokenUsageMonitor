using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Services;

public class DataCacheService
{
    private static readonly Lazy<DataCacheService> _instance = new(() => new DataCacheService());
    public static DataCacheService Instance => _instance.Value;

    private readonly string _cacheFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    private DataCacheService()
    {
        var cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TokenUsageMonitor");

        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        _cacheFilePath = Path.Combine(cacheDirectory, "cache.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void Save(Dictionary<string, QuotaInfo> results)
    {
        try
        {
            var json = JsonSerializer.Serialize(results, _jsonOptions);
            File.WriteAllText(_cacheFilePath, json);
        }
        catch
        {
            // Ignore cache write errors
        }
    }

    public Dictionary<string, QuotaInfo>? Load()
    {
        if (!File.Exists(_cacheFilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_cacheFilePath);
            var cache = JsonSerializer.Deserialize<Dictionary<string, QuotaInfo>>(json, _jsonOptions);
            return cache;
        }
        catch
        {
            return null;
        }
    }

    public bool HasCache()
    {
        return File.Exists(_cacheFilePath);
    }
}
