using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.ViewModels;

public partial class MainViewModel : ObservableObject
{
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
    private string _totalUsageText = "5.15M";

    [ObservableProperty]
    private string _totalCostText = "¥ 8.99";

    public List<string> TimeRanges { get; } = new() { "本日", "24h", "7天", "30天" };

    public MainViewModel()
    {
        InitializePlatforms();
        SelectedPlatform = Platforms.FirstOrDefault();
        LoadMockData();
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
    private void Refresh()
    {
        LastUpdateTime = $"{DateTime.Now:HH:mm:ss} 更新";
        LoadMockData();
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

        for (int i = 0; i < 20; i++)
        {
            ChartData.Add(new ChartDataPoint
            {
                Label = $"{i * 2}h",
                Values = new List<double> { Random.Shared.Next(10, 50), Random.Shared.Next(5, 30), Random.Shared.Next(0, 20) }
            });
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
}
