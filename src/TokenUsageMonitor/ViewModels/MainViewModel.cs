using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokenUsageMonitor.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using TokenUsageMonitor.Services;

namespace TokenUsageMonitor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly QuotaRefreshService _quotaRefreshService = new();
    private readonly Dictionary<string, string> _apiKeys = new();
    private DispatcherTimer _refreshTimer = null!;
    [ObservableProperty]
    private string _title = "Coding Plan 用量监控";

    [ObservableProperty]
    private ObservableCollection<PlatformInfo> _platforms = new();

    [ObservableProperty]
    private PlatformInfo? _selectedPlatform;

    [ObservableProperty]
    private bool _isTokenMode = true;

    [ObservableProperty]
    private string _selectedTimeRange = "7天";

    [ObservableProperty]
    private string _lastUpdateTime = "刚刚更新";

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private ObservableCollection<UsageItem> _usageCards = new();

    [ObservableProperty]
    private ObservableCollection<UsageItem> _kimiItems = new();

    [ObservableProperty]
    private ObservableCollection<ServiceStatusItem> _deepSeekServices = new();

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _chartData = new();

    [ObservableProperty]
    private ObservableCollection<LineChartDataPoint> _healthData = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private ObservableCollection<QuotaInfo> _quotaInfos = new();

    [ObservableProperty]
    private ObservableCollection<ConcurrentTestResult> _concurrentTestResults = new();

    [ObservableProperty]
    private bool _isSwitchingTab;

    [ObservableProperty]
    private string _totalUsageText = "5.15M";

    [ObservableProperty]
    private string _totalCostText = "¥ 8.99";

    public List<string> TimeRanges { get; } = new() { "本日", "24h", "7天", "30天" };

    public MainViewModel()
    {
        InitializePlatforms();
        SelectedPlatform = Platforms.FirstOrDefault();
        LoadApiKeys();

        var cache = DataCacheService.Instance.Load();
        if (cache != null)
        {
            QuotaInfos.Clear();
            foreach (var item in cache.Values) QuotaInfos.Add(item);
            MapQuotaInfosToCollections();
        }
        else
        {
            LoadMockData();
        }

        _refreshTimer = new DispatcherTimer();
        _refreshTimer.Tick += async (s, e) => await RefreshInternalAsync();
        UpdateRefreshTimerInterval();
    }

    private void LoadApiKeys()
    {
        try
        {
            var storage = SecureStorageService.Instance;
            foreach (var platform in new[] { "GLM", "KIMI", "DeepSeek" })
            {
                var key = storage.LoadApiKey(platform);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _apiKeys[platform] = key;
                }
            }
        }
        catch
        {
            // Ignore storage errors on startup
        }
    }

    private void InitializePlatforms()
    {
        Platforms.Add(new PlatformInfo
        {
            Name = "GLM",
            DisplayName = "GLM",
            BrandBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C8B8A8")!),
            IsPro = true
        });
        Platforms.Add(new PlatformInfo
        {
            Name = "KIMI",
            DisplayName = "KIMI",
            BrandBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A8B8C8")!),
            IsPro = false
        });
        Platforms.Add(new PlatformInfo
        {
            Name = "DeepSeek",
            DisplayName = "DeepSeek",
            BrandBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B8A8B8")!),
            IsPro = false
        });
    }

    partial void OnSelectedPlatformChanged(PlatformInfo? value)
    {
        LoadMockData();
    }

    partial void OnSelectedTimeRangeChanged(string value)
    {
        LoadMockData();
    }

    partial void OnIsTokenModeChanged(bool value)
    {
        LoadMockData();
    }

    [RelayCommand]
    private void SwitchPlatform(PlatformInfo platform)
    {
        SelectedPlatform = platform;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await RefreshInternalAsync();
    }

    private async Task RefreshInternalAsync()
    {
        IsRefreshing = true;
        LastUpdateTime = $"{DateTime.Now:HH:mm:ss} 更新中...";
        try
        {
            if (_apiKeys.Count == 0 || _apiKeys.All(kv => string.IsNullOrWhiteSpace(kv.Value)))
            {
                LoadMockData();
                return;
            }

            var results = await _quotaRefreshService.RefreshAllAsync(_apiKeys);
            QuotaInfos.Clear();
            foreach (var result in results.Values)
            {
                QuotaInfos.Add(result);
            }

            MapQuotaInfosToCollections();
            DataCacheService.Instance.Save(results);
        }
        catch (Exception)
        {
            LoadMockData();
        }
        finally
        {
            IsRefreshing = false;
            LastUpdateTime = $"{DateTime.Now:HH:mm:ss} 更新";
        }
    }

    private void UpdateRefreshTimerInterval()
    {
        var settings = SettingsService.Instance.Load();
        if (settings.RefreshIntervalMinutes <= 0)
        {
            _refreshTimer.Stop();
        }
        else
        {
            _refreshTimer.Interval = TimeSpan.FromMinutes(settings.RefreshIntervalMinutes);
            _refreshTimer.Start();
        }
    }

    [RelayCommand]
    private void SwitchTimeRange(string range)
    {
        SelectedTimeRange = range;
    }

    [RelayCommand]
    private void ToggleTokenMode(string mode)
    {
        IsTokenMode = mode == "Token";
    }

    [RelayCommand]
    private void ToggleDarkMode()
    {
        Services.ThemeManager.ToggleTheme();
        IsDarkMode = Services.ThemeManager.IsDarkMode;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var window = new Views.SettingsWindow();
        window.Owner = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault();
        window.ShowDialog();
        // Reload API keys after settings window closes (user may have added new keys)
        LoadApiKeys();
    }

    [RelayCommand]
    private async Task ConcurrentTest()
    {
        ConcurrentTestResults.Clear();
        IsRefreshing = true;
        try
        {
            var tasks = new List<Task<ConcurrentTestResult>>();

            foreach (var kvp in _apiKeys.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
            {
                var platformId = kvp.Key;
                var apiKey = kvp.Value;
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        IApiClient? client = platformId switch
                        {
                            "GLM" => new GlmApiClient(SharedHttpClient.Instance),
                            "KIMI" => new KimiApiClient(SharedHttpClient.Instance),
                            "DeepSeek" => new DeepSeekApiClient(SharedHttpClient.Instance),
                            _ => null
                        };

                        if (client == null)
                        {
                            return new ConcurrentTestResult
                            {
                                PlatformName = platformId,
                                Success = false,
                                ErrorMessage = "Unknown platform"
                            };
                        }

                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        await client.GetQuotaAsync(apiKey, cts.Token);
                        sw.Stop();
                        return new ConcurrentTestResult
                        {
                            PlatformName = platformId,
                            Success = true,
                            LatencyMs = (int)sw.ElapsedMilliseconds
                        };
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        return new ConcurrentTestResult
                        {
                            PlatformName = platformId,
                            Success = false,
                            LatencyMs = (int)sw.ElapsedMilliseconds,
                            ErrorMessage = ex.Message
                        };
                    }
                }));
            }

            if (tasks.Count == 0)
            {
                ConcurrentTestResults.Add(new ConcurrentTestResult
                {
                    PlatformName = "All",
                    Success = false,
                    ErrorMessage = "No API keys configured"
                });
                return;
            }

            var results = await Task.WhenAll(tasks);
            foreach (var result in results)
            {
                ConcurrentTestResults.Add(result);
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void LoadMockData()
    {
        var platformName = SelectedPlatform?.Name ?? "GLM";

        UsageCards.Clear();
        KimiItems.Clear();
        DeepSeekServices.Clear();
        ChartData.Clear();
        HealthData.Clear();

        switch (platformName)
        {
            case "GLM":
                LoadZhipuData();
                break;
            case "KIMI":
                LoadKimiData();
                break;
            case "DeepSeek":
                LoadDeepSeekData();
                break;
        }
    }

    private void LoadZhipuData()
    {
        UsageCards.Add(new UsageItem
        {
            Title = "MCP 用量",
            Percentage = 24,
            UsedText = "1,234",
            TotalText = "10,000",
            TimeRangeText = "5月1日",
            DetailText = "24%"
        });

        UsageCards.Add(new UsageItem
        {
            Title = "5小时额度",
            Percentage = 25,
            UsedText = "25%",
            TimeRangeText = "19:34"
        });

        UsageCards.Add(new UsageItem
        {
            Title = "周额度",
            Percentage = 40,
            UsedText = "40%",
            TimeRangeText = "5月2日"
        });

        TotalUsageText = IsTokenMode ? "5.15M" : "3.24M";
        TotalCostText = "¥ 8.99";

        double maxValue = 0;
        var chartPoints = new List<ChartDataPoint>();
        for (int i = 0; i < 20; i++)
        {
            var point = new ChartDataPoint
            {
                Label = $"{i * 2}h",
                Values = new List<double> { Random.Shared.Next(10, 50), Random.Shared.Next(5, 30), Random.Shared.Next(0, 20) },
                ShowLabel = i % 3 == 0
            };
            chartPoints.Add(point);
            if (point.Values.Count > 0)
                maxValue = Math.Max(maxValue, point.Values.Max());
        }
        foreach (var point in chartPoints)
        {
            point.MaxValue = maxValue;
            ChartData.Add(point);
        }

        for (int i = 0; i < 7; i++)
        {
            HealthData.Add(new LineChartDataPoint
            {
                Label = $"04-{18 + i}",
                Value1 = Random.Shared.Next(50, 95),
                Value2 = Random.Shared.Next(40, 80)
            });
        }
    }

    private void LoadKimiData()
    {
        KimiItems.Add(new UsageItem
        {
            Title = "MiniMax-M*",
            SubTitle = "04/25 14:35 - 04/25 18:35",
            Percentage = 15,
            UsedText = "668",
            TotalText = "4500"
        });

        KimiItems.Add(new UsageItem
        {
            Title = "speech-hd",
            SubTitle = "04/25 02:35 - 04/26 02:35",
            Percentage = 2,
            UsedText = "352",
            TotalText = "19000"
        });

        KimiItems.Add(new UsageItem
        {
            Title = "MiniMax-Hailuo-2.3-Fast-6s-768p",
            SubTitle = "04/23 14:35 - 04/30 14:35",
            Percentage = 33,
            UsedText = "2100",
            TotalText = "133000"
        });

        KimiItems.Add(new UsageItem
        {
            Title = "music-2.5",
            SubTitle = "04/25 02:35 - 04/26 02:35",
            Percentage = 29,
            UsedText = "12",
            TotalText = "100"
        });
    }

    private void LoadDeepSeekData()
    {
        UsageCards.Add(new UsageItem
        {
            Title = "总余额",
            Percentage = 60,
            UsedText = "¥50.00",
            TotalText = "¥100.00",
            DetailText = "赠送余额 ¥30.00    充值余额 ¥20.00"
        });

        DeepSeekServices.Add(new ServiceStatusItem
        {
            Name = "API 服务",
            StatusText = "运行正常",
            Percentage = 99.86,
            IsHealthy = true
        });

        DeepSeekServices.Add(new ServiceStatusItem
        {
            Name = "网页对话服务",
            StatusText = "运行正常",
            Percentage = 99.0,
            IsHealthy = true
        });
    }

    private void MapQuotaInfosToCollections()
    {
        var platformName = SelectedPlatform?.Name ?? "GLM";

        UsageCards.Clear();
        KimiItems.Clear();
        DeepSeekServices.Clear();
        ChartData.Clear();
        HealthData.Clear();

        switch (platformName)
        {
            case "GLM":
                MapGlmData();
                break;
            case "KIMI":
                MapKimiData();
                break;
            case "DeepSeek":
                MapDeepSeekData();
                break;
        }
    }

    private void MapGlmData()
    {
        var glm = QuotaInfos.FirstOrDefault(q => q.PlatformId == "GLM");
        if (glm != null && glm.Status == QuotaStatus.Normal)
        {
            UsageCards.Add(new UsageItem
            {
                Title = "GLM 额度",
                Percentage = glm.Percentage,
                UsedText = glm.UsedAmount.ToString("F0"),
                TotalText = glm.TotalAmount.ToString("F0"),
                TimeRangeText = glm.LastUpdated.ToString("MM月dd日"),
                DetailText = glm.DisplayPercent
            });
        }
        else if (glm != null)
        {
            UsageCards.Add(new UsageItem
            {
                Title = "GLM 额度",
                Percentage = 0,
                UsedText = "Error",
                TotalText = glm.TotalAmount.ToString("F0"),
                DetailText = glm.ErrorMessage
            });
        }

        TotalUsageText = IsTokenMode ? "5.15M" : "3.24M";
        TotalCostText = "¥ 8.99";

        double maxValue = 0;
        var chartPoints = new List<ChartDataPoint>();
        for (int i = 0; i < 20; i++)
        {
            var point = new ChartDataPoint
            {
                Label = $"{i * 2}h",
                Values = new List<double> { Random.Shared.Next(10, 50), Random.Shared.Next(5, 30), Random.Shared.Next(0, 20) },
                ShowLabel = i % 3 == 0
            };
            chartPoints.Add(point);
            if (point.Values.Count > 0)
                maxValue = Math.Max(maxValue, point.Values.Max());
        }
        foreach (var point in chartPoints)
        {
            point.MaxValue = maxValue;
            ChartData.Add(point);
        }

        for (int i = 0; i < 7; i++)
        {
            HealthData.Add(new LineChartDataPoint
            {
                Label = $"04-{18 + i}",
                Value1 = Random.Shared.Next(50, 95),
                Value2 = Random.Shared.Next(40, 80)
            });
        }
    }

    private void MapKimiData()
    {
        var kimi = QuotaInfos.FirstOrDefault(q => q.PlatformId == "KIMI");
        if (kimi != null && kimi.Status == QuotaStatus.Normal)
        {
            KimiItems.Add(new UsageItem
            {
                Title = "KIMI 额度",
                SubTitle = kimi.LastUpdated.ToString("MM/dd HH:mm"),
                Percentage = kimi.Percentage,
                UsedText = kimi.UsedAmount.ToString("F0"),
                TotalText = kimi.TotalAmount.ToString("F0")
            });
        }
        else if (kimi != null)
        {
            KimiItems.Add(new UsageItem
            {
                Title = "KIMI 额度",
                SubTitle = "Error",
                Percentage = 0,
                UsedText = "Error",
                TotalText = kimi.TotalAmount.ToString("F0")
            });
        }
    }

    private void MapDeepSeekData()
    {
        var deepseek = QuotaInfos.FirstOrDefault(q => q.PlatformId == "DeepSeek");
        if (deepseek != null && deepseek.Status == QuotaStatus.Normal)
        {
            UsageCards.Add(new UsageItem
            {
                Title = "总余额",
                Percentage = deepseek.Percentage,
                UsedText = deepseek.UsedAmount.ToString("F2"),
                TotalText = deepseek.TotalAmount.ToString("F2"),
                DetailText = $"已用 {deepseek.DisplayPercent}"
            });
        }
        else if (deepseek != null)
        {
            UsageCards.Add(new UsageItem
            {
                Title = "总余额",
                Percentage = 0,
                UsedText = "Error",
                TotalText = deepseek.TotalAmount.ToString("F2"),
                DetailText = deepseek.ErrorMessage
            });
        }

        DeepSeekServices.Add(new ServiceStatusItem
        {
            Name = "API 服务",
            StatusText = "运行正常",
            Percentage = 99.86,
            IsHealthy = true
        });

        DeepSeekServices.Add(new ServiceStatusItem
        {
            Name = "网页对话服务",
            StatusText = "运行正常",
            Percentage = 99.0,
            IsHealthy = true
        });
    }
}
