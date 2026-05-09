# TokenUsageMonitor 实现计划

## 项目概述

Windows 桌面 WPF 应用，UI 遵循 Windows 原生设计风格，使用系统主题色。需求规格见 [requirements.md](requirements.md)，设计规格见 [design.md](design.md)。

执行报告：[implement/execution-report.md](implement/execution-report.md)

---

## 当前真实状态（2026-05-09 第三轮）

### 已具备（全部代码已编写并通过编译）
- [x] .NET 10 WPF 项目骨架，MVVM 目录结构
- [x] MainWindow 无边框圆角窗口，鼠标拖拽，淡入淡出动画
- [x] Windows 原生主题 `MorandiTheme.xaml` / `DarkTheme.xaml`（系统色 DynamicResource）
- [x] **系统托盘**：WinForms NotifyIcon，左键弹出/收起，右键菜单，启动气泡提示，`app.ico` 图标文件
- [x] **窗口定位**：WindowPositionHelper 任务栏四方向 + 位置记忆（关闭时保存，打开时恢复）
- [x] **设置窗口**：SettingsWindow，API Key 三组密码框（保存后回显正常），刷新间隔、主题、失焦隐藏开关
- [x] **配置服务**：SettingsService（%AppData%/appsettings.json）+ SecureStorageService（DPAPI 加密 key）
- [x] **三平台 API 客户端**：GlmApiClient / KimiApiClient / DeepSeekApiClient + QuotaRefreshService 并发刷新
- [x] **MainViewModel**：Refresh 接入真实 API（有 Key 时）+ 自动回退模拟数据（无 Key 时）
- [x] **数据缓存**：DataCacheService，启动优先加载缓存
- [x] **自动刷新定时器**：DispatcherTimer，按 RefreshIntervalMinutes 间隔
- [x] **并发测试**：ConcurrentTestCommand + Footer 按钮（IsRefreshing 时显示 LoadingSpinner）
- [x] **夜间模式**：跟随系统 + 手动切换 + "跟随系统/浅色/深色"三选一 + 持久化
- [x] **进度条动画**：AnimatedWidthConverter.SmoothWidth，300ms CubicEase
- [x] **全局异常处理**：三层捕获 + error.log
- [x] **首次启动引导**：无 API Key → 弹出设置窗口

### 第三轮修复（用户实测反馈）

| # | 问题 | 根因 | 修复 |
|---|------|------|------|
| A | 主页面设置齿轮无响应 | XAML 按钮缺 `Command` | `OpenSettingsCommand` → 弹设置窗 |
| B | 窗口每次出现在固定位置 | 无位置记忆 | AppSettings 新增 WindowLeft/Top，关闭时保存，打开时恢复 |
| C | 设置保存后再次打开为空 | PasswordBox 不支持绑定 | SettingsWindow.Loaded 回写 PasswordBox.Password |
| D | 夜间模式不生效 | 默认写死 "Light"，不读系统 | `AppSettings.Theme` 默认 "System"，启动读注册表 `AppsUseLightTheme` |
| E | 托盘图标显示问号 | Icon.FromHandle 句柄未 Clone | `.Clone()` + 正式 `app.ico` 文件 + csproj CopyToOutputDirectory |

---

## 实现阶段

### Phase 1 — 托盘图标与窗口生命周期

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 1.1 | 托盘图标 | ✅ `app.ico` + 动态生成 fallback |
| 1.2 | App 初始化 | ✅ OnExplicitShutdown，三层异常捕获 |
| 1.3 | 单击弹出/收起 | ✅ Dispatcher.Invoke 线程安全 |
| 1.4 | 右键菜单 | ✅ 刷新/设置/关于/退出 |
| 1.5 | 启动气泡提示 | ✅ 3 秒 |
| 1.6 | 窗口定位 | ✅ 位置记忆 + 安全回退居中 |
| 1.7 | 失焦自动隐藏 | ✅ 300ms 防抖 |
| 1.8 | 窗口淡入淡出 | ✅ PopupFadeIn/Out Clone Storyboard |

### Phase 2 — 设置窗口与安全存储

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 2.1 | SettingsWindow | ✅ 密码框、下拉、复选框、主题"跟随系统" |
| 2.2 | DPAPI 加密 | ✅ SecureStorageService |
| 2.3 | 配置读写 | ✅ SettingsService |
| 2.4 | 首次启动引导 | ✅ 检测无 Key → 弹出设置 |
| 2.5 | 开机自启 | ✅ StartupService 写入 HKCU\...\Run |

### Phase 3 — API 集成

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 3.1-3.6 | 客户端+刷新服务 | ✅ 按 cc-switch 端点重写，中文错误提示，raw body 日志 |
| 3.7 | MainViewModel 接入 | ✅ apiKeys 加载，真实通路 |
| 3.8 | 数据缓存 | ✅ DataCacheService |
| 3.9 | 自动刷新 | ✅ DispatcherTimer |
| 3.10 | API 端点验证 | ✅ 端点已按 cc-switch 验证结果更新（详见 requirements.md §4.2） |

### Phase 4 — 并发测试

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 4.1 | ConcurrentTestResult | ✅ |
| 4.2 | ConcurrentTestCommand | ✅ |
| 4.3 | 并发测试按钮 | ✅ Footer LoadSpinner |
| 4.4 | 测试结果展示 UI | ✅ Footer WrapPanel 显示平台+✓/✗+延迟/错误 |

### Phase 5 — 动画打磨

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 5.1-5.5 | Storyboard/Converter/Spinner/脉冲/SmoothWidth | ✅ |
| 5.6 | Tab 切换过渡 | ✅ ContentControl 外 Border Opacity 1→0→1 动画 |

### Phase 6 — 夜间模式

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 6.1 | DarkTheme.xaml（替代 MorandiDarkTheme） | ✅ 系统色 DynamicResource |
| 6.2 | 系统主题跟随 | ✅ DWM 暗色 API + 注册表 AppsUseLightTheme |
| 6.3 | 持久化 | ✅ 直接调用 SettingsService |
| 6.4 | System→Light→Dark 循环 | ✅ ThemeManager.ToggleTheme() |

### Phase 7 — 图表精细化

进度: ██████████ 100%

- ✅ ChartNormalizationConverter 比例归一化 (MaxHeight=60)
- ✅ X轴标签每3个显示一个 (ShowLabel)
- ✅ Tooltip 显示完整数据
- ✅ 折线图数据点圆点 (HealthDataToEllipsePositionsConverter)

---

### Phase 8 — UI 风格迁移至 Windows 原生（新增）✅ 已完成

- ✅ MorandiTheme.xaml 改用 SystemColors 动态资源（Window/ControlLight/WindowText/GrayText/ControlDark/Control/Highlight）
- ✅ MorandiDarkTheme.xaml 已删除
- ✅ Success/Error 改为 Win11 固定色 `#10893E` / `#C42B1C`
- ✅ 平台品牌色保留（GLM `#C8B8A8` / KIMI `#A8B8C8` / DeepSeek `#B8A8B8`）
- ✅ ThemeManager 简化：System→Light→Dark 循环，DWM 暗色模式 API，持久化到 Settings

---

## 待完成清单

| # | 任务 | 说明 |
|---|------|------|
| 1 | 真实 API 连通性测试 | 用真实 Key 验证三平台客户端能正确解析响应 |
| 2 | 高 DPI 适配验证 | 125%/150%/200% 缩放测试 |
| 3 | 多显示器测试 | 任务栏在副屏时的定位验证 |

---

## 技术决策记录

| 决策 | 方案 | 理由 |
|------|------|------|
| 图表实现 | 纯 XAML 自定义 | 零外部依赖 |
| 目标框架 | .NET 10 | 本地 SDK 版本 |
| 托盘图标 | WinForms NotifyIcon | Hardcodet.NotifyIcon.Wpf 消息钩子不可靠 |
| 安全存储 | Windows DPAPI | 不落盘明文 Key |
| MVVM | CommunityToolkit.Mvvm | 源生成器，零反射 |
| 主题切换 | ResourceDictionary 替换 + 系统主题检测 | 完整切换 + 跟随 Windows |
| 滚动条动画 | ObjectAnimationUsingKeyFrames | StaticResource.Color 非法 |
| 窗口动画 | Storyboard.Clone() | Resource 中 Storyboard 只能 Begin 一次 |
| 托盘线程 | Dispatcher.Invoke | WinForms 线程不能操作 WPF 控件 |
| UI 风格 | Windows 原生 | 莫兰迪深色主题视觉效果差，改用系统色 + 跟随系统 |
