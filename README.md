# TokenUsageMonitor

一个 Windows 桌面工具，常驻系统托盘，聚合展示多 AI 平台（GLM / KIMI / DeepSeek）Token 配额使用情况。采用莫兰迪低饱和配色。

> **当前状态：早期开发阶段** — UI 骨架已完成，API 集成和托盘功能正在开发中。详见 [实现计划](docs/implementation-plan.md)。

## 平台支持

| 平台 | 功能 | 状态 |
|------|------|------|
| GLM Coding Plan | 余额 / 用量查询 | 模拟数据 |
| KIMI Coding Plan | 余额 / 用量查询 | 模拟数据 |
| DeepSeek | 余额 / 服务状态查询 | 模拟数据 |

## 项目结构

```
TokenUsageMonitor/
├── src/TokenUsageMonitor/
│   ├── TokenUsageMonitor.csproj     (.NET 10, WPF)
│   ├── App.xaml / App.xaml.cs
│   ├── Views/
│   │   ├── MainWindow.xaml / .cs       主弹窗
│   │   └── PlatformTemplateSelector.cs  平台内容切换
│   ├── ViewModels/
│   │   └── MainViewModel.cs            主视图模型 + 模拟数据
│   ├── Models/
│   │   ├── PlatformInfo.cs
│   │   ├── UsageItem.cs
│   │   ├── ChartDataPoint.cs
│   │   └── ServiceStatusItem.cs
│   ├── Services/
│   │   └── ThemeManager.cs             浅色/深色切换
│   ├── Converters/
│   │   ├── PercentageConverter.cs
│   │   ├── InverseBooleanConverter.cs
│   │   ├── StringEqualityConverter.cs
│   │   └── HealthDataToPointsConverter.cs
│   ├── Assets/Themes/
│   │   └── MorandiTheme.xaml           莫兰迪浅色主题
│   └── Config/
│       └── AppSettings.json
├── docs/
│   ├── requirements.md                 需求文档
│   ├── design.md                       设计规格文档
│   └── implementation-plan.md          实现计划
├── reference/                           UI 设计参考素材
└── README.md
```

## 开发环境

- Windows 10 (1903+) / Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 或 VS Code + C# Dev Kit

## 依赖

| NuGet 包 | 用途 |
|----------|------|
| `CommunityToolkit.Mvvm` (8.2.2) | MVVM 源生成器 |
| `Hardcodet.NotifyIcon.Wpf` (2.0.1) | 系统托盘 |
| `Microsoft.Extensions.Http` (8.0.0) | HttpClient 工厂 |

## 本地运行

```bash
cd src/TokenUsageMonitor
dotnet restore
dotnet run
```

## License

MIT
