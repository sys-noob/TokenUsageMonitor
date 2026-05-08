# Token Usage Monitor

一个优雅常驻Windows系统托盘的Token配额监控工具，采用莫兰迪配色设计，帮助你实时掌握各AI平台Coding Plan的使用情况。

![Preview](reference/coding-quota-bar.gif)

## Features

- **常驻托盘**: 最小化到系统托盘，单击图标弹出详情浮窗，右键呼出菜单
- **多平台支持**: 同时监控 GLM Coding Plan、KIMI Coding Plan、DeepSeek
- **并发测试**: 支持多平台并发API测试，一键验证各平台可用性
- **莫兰迪美学**: 柔和低饱和配色，精致进度条与数据可视化
- **实时刷新**: 自动轮询各平台API，即时展示最新配额数据
- **安全存储**: API Key使用Windows DPAPI加密存储

## Supported Platforms

| 平台 | 配额API | 说明 |
|------|---------|------|
| GLM Coding Plan | `/v1/user/info` 或用户余额接口 | 获取balance / total_quota |
| KIMI Coding Plan | Moonshot Open API | 用户额度查询 |
| DeepSeek | DeepSeek API | 账户余额/使用量查询 |

## Project Structure

```
tokenUsageMonitor/
├── src/                        # C# 项目代码
│   └── TokenMonitor/
│       ├── TokenMonitor.csproj
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── MainWindow.xaml          # 主弹窗 (托盘点击触发)
│       ├── MainWindow.xaml.cs
│       ├── Views/
│       │   ├── PlatformCard.xaml    # 平台卡片组件
│       │   └── PlatformCard.xaml.cs
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   └── PlatformViewModel.cs
│       ├── Models/
│       │   └── QuotaInfo.cs
│       ├── Services/
│       │   ├── IApiClient.cs
│       │   ├── GlmApiClient.cs
│       │   ├── KimiApiClient.cs
│       │   ├── DeepSeekApiClient.cs
│       │   └── ConcurrentTester.cs
│       ├── Converters/
│       │   └── PercentageConverter.cs
│       ├── Helpers/
│       │   └── TrayIconHelper.cs
│       ├── Assets/
│       │   ├── Themes/
│       │   │   └── MorandiTheme.xaml
│       │   ├── Fonts/
│       │   └── icon.ico
│       └── Config/
│           └── AppSettings.json
├── tests/                      # 测试相关
├── docs/                       # 文档
│   ├── requirements.md         # 详细需求文档
│   └── design.md               # 设计规格文档
├── scripts/                    # 构建脚本
│   └── build.bat
├── reference/                  # 设计参考
│   └── coding-quota-bar.gif
├── release/                    # 发布目录 (手动放置测试通过的EXE)
├── README.md
├── .gitignore
└── LICENSE
```

## Development

### 环境要求
- Windows 10 (1903+) / Windows 11
- Visual Studio 2022 或 VS Code + C# Dev Kit
- .NET 8.0 SDK

### 依赖 (NuGet)
- `Hardcodet.NotifyIcon.Wpf` — 系统托盘支持
- `Microsoft.Extensions.Http` — HttpClient工厂
- `System.Text.Json` — JSON序列化
- `CommunityToolkit.Mvvm` — MVVM工具包

### 本地运行
```bash
cd src
dotnet restore
dotnet run --project TokenMonitor
```

### 打包发布
```bash
scripts/build.bat
```

## License
MIT
