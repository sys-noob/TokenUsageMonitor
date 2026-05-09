# TokenUsageMonitor

一个 Windows 桌面工具，常驻系统托盘，聚合展示多 AI 平台（GLM / KIMI / DeepSeek）Token 配额使用情况。采用莫兰迪低饱和配色。

> **当前状态**：功能开发基本完成，API 端点待实测验证。详见 [实现计划](docs/implementation-plan.md)。

## 平台支持

| 平台 | 功能 | 状态 |
|------|------|------|
| GLM Coding Plan | 余额 / 用量查询 | API 代码完成，端点待验证 |
| KIMI Coding Plan | 余额 / 用量查询 | API 代码完成，端点待验证 |
| DeepSeek | 余额 / 服务状态查询 | API 代码完成，端点已确认 |

## 项目结构

```
TokenUsageMonitor/
├── src/TokenUsageMonitor/
│   ├── TokenUsageMonitor.csproj     (.NET 10, WPF + WinForms)
│   ├── App.xaml / App.xaml.cs      入口：托盘初始化、异常处理、首次引导
│   ├── Views/
│   │   ├── MainWindow.xaml / .cs         主弹窗 + 动画
│   │   ├── SettingsWindow.xaml / .cs     设置窗口
│   │   └── PlatformTemplateSelector.cs   平台内容切换
│   ├── ViewModels/
│   │   ├── MainViewModel.cs             主逻辑：刷新、并发测试、API 集成
│   │   └── SettingsViewModel.cs         设置逻辑：Key/间隔/主题
│   ├── Models/
│   │   ├── QuotaInfo.cs / PlatformInfo.cs / UsageItem.cs
│   │   ├── ChartDataPoint.cs / ServiceStatusItem.cs
│   │   └── ConcurrentTestResult.cs
│   ├── Services/
│   │   ├── TrayIconService.cs           托盘图标 + 菜单
│   │   ├── ThemeManager.cs              浅色/深色/跟随系统
│   │   ├── SettingsService.cs           配置读写（%AppData% JSON）
│   │   ├── SecureStorageService.cs      DPAPI 加密 Key
│   │   ├── DataCacheService.cs          数据缓存
│   │   ├── IApiClient.cs / Glm|Kimi|DeepSeekApiClient.cs   API 客户端
│   │   └── QuotaRefreshService.cs       并发刷新
│   ├── Converters/
│   │   ├── PercentageConverter.cs / InverseBooleanConverter.cs
│   │   ├── StringEqualityConverter.cs / HealthDataToPointsConverter.cs
│   │   └── AnimatedWidthConverter.cs   进度条平滑动画
│   ├── Helpers/
│   │   └── WindowPositionHelper.cs      窗口定位
│   ├── Assets/Themes/
│   │   ├── MorandiTheme.xaml            浅色主题
│   │   └── MorandiDarkTheme.xaml        深色主题
│   ├── Assets/app.ico                   托盘图标
│   └── Config/AppSettings.json          默认配置
├── docs/
│   ├── requirements.md                  需求文档
│   ├── design.md                        设计规格
│   ├── implementation-plan.md           实现计划
│   ├── review-2026-05-09.md             首次审查报告
│   └── implement/execution-report.md    执行报告
└── README.md
```

## 开发环境

- Windows 10 (1903+) / Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## 本地运行

```bash
cd src/TokenUsageMonitor
dotnet restore
dotnet run
```

## License

MIT
