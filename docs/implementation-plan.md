# TokenUsageMonitor 实现计划

## 项目概述

Windows 桌面 WPF 应用，UI 遵循 Windows 原生设计风格，使用系统主题色。需求规格见 [requirements.md](requirements.md)，设计规格见 [design.md](design.md)。

执行报告：[implement/execution-report.md](implement/execution-report.md)

---

## 当前真实状态（2026-05-09 第三轮）

### 已具备（全部代码已编写并通过编译）
- [x] .NET 10 WPF 项目骨架，MVVM 目录结构
- [x] MainWindow 无边框圆角窗口，鼠标拖拽，淡入淡出动画
- [x] 莫兰迪浅色/深色主题 `MorandiTheme.xaml` / `MorandiDarkTheme.xaml`
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

进度: ████████░░ 90%

| # | 任务 | 状态 |
|---|------|------|
| 2.1 | SettingsWindow | ✅ 密码框、下拉、复选框、主题"跟随系统" |
| 2.2 | DPAPI 加密 | ✅ SecureStorageService |
| 2.3 | 配置读写 | ✅ SettingsService |
| 2.4 | 首次启动引导 | ✅ 检测无 Key → 弹出设置 |
| 2.5 | 开机自启 | ❌ StartWithWindows 开关存在但未写注册表 |

### Phase 3 — API 集成

进度: ████████░░ 85%

| # | 任务 | 状态 |
|---|------|------|
| 3.1-3.6 | 客户端+刷新服务 | ✅ |
| 3.7 | MainViewModel 接入 | ✅ apiKeys 加载，真实通路 |
| 3.8 | 数据缓存 | ✅ DataCacheService |
| 3.9 | 自动刷新 | ✅ DispatcherTimer |
| 3.10 | API 端点验证 | ❌ GLM/KIMI 端点未验证 |

### Phase 4 — 并发测试

进度: ██████░░░░ 70%

| # | 任务 | 状态 |
|---|------|------|
| 4.1 | ConcurrentTestResult | ✅ |
| 4.2 | ConcurrentTestCommand | ✅ |
| 4.3 | 并发测试按钮 | ✅ Footer LoadSpinner |
| 4.4 | 测试结果展示 UI | ❌ 延迟毫秒未在界面展示 |

### Phase 5 — 动画打磨

进度: █████░░░░░ 55%

| # | 任务 | 状态 |
|---|------|------|
| 5.1-5.5 | Storyboard/Converter/Spinner/脉冲/SmoothWidth | ✅ |
| 5.6 | Tab 切换过渡 | ❌ |

### Phase 6 — 夜间模式

进度: ██████████ 100%

| # | 任务 | 状态 |
|---|------|------|
| 6.1 | MorandiDarkTheme.xaml | ✅ |
| 6.2 | 系统主题跟随 | ✅ 读注册表 AppsUseLightTheme |
| 6.3 | 持久化 | ✅ 直接调用 SettingsService |

### Phase 7 — 图表精细化

进度: ░░░░░░░░░░ 0%

未开始。数据为 Random.Shared 占位。

---

### Phase 8 — UI 风格迁移至 Windows 原生（新增）

当前使用自定义莫兰迪色板（`MorandiTheme.xaml` / `MorandiDarkTheme.xaml`），深色模式视觉效果差。改为使用 Windows 系统主题色。

| # | 任务 | 说明 |
|---|------|------|
| 8.1 | 浅色主题改用 `SystemColors` | 移除 `MorandiTheme.xaml` 中的自定义色刷，换用 `{DynamicResource {x:Static SystemColors.WindowBrushKey}}` 等系统资源 |
| 8.2 | 深色主题废弃 | 删除 `MorandiDarkTheme.xaml`，系统深色模式由 Windows 自动提供正确的系统颜色 |
| 8.3 | 进度条使用系统强调色 | `SystemColors.HighlightBrush` 替换 `ProgressBarFillBrush` |
| 8.4 | 平台品牌色保留 | GLM/KIMI/DeepSeek 三个平台色作为唯一自定义色，保持卡片区分度 |
| 8.5 | 清理 ThemeManager | 简化或移除自定义主题切换逻辑，只保留"跟随系统/浅色/深色"选择 |

---

## 待完成清单

| # | 任务 | 阶段 | 预估 |
|---|------|------|------|
| 1 | **UI 风格迁移至 Windows 原生** | Phase 8 | 60min |
| 2 | API 端点验证（GLM/KIMI） | Phase 3 | 30min |
| 3 | 并发测试结果展示 UI | Phase 4 | 15min |
| 4 | 图表精细化（比例/轴标签/Tooltip） | Phase 7 | 60min |
| 5 | Tab 切换过渡动画 | Phase 5 | 20min |
| 6 | 开机自启注册表写入 | Phase 2 | 15min |

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
