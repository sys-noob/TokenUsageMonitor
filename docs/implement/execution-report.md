# TokenUsageMonitor 执行报告

> 记录本次多 Agent 并行实现的实际执行结果。
> 执行时间：2026-05-09

---

## 执行概览

采用 **4 个 Agent 并行** 方式，按实现计划分 Phase 推进。先修复编译错误，再并行启动各 Phase Agent，最后整合验证。

| Agent | 负责 Phase | 状态 |
|-------|-----------|------|
| Agent 1 | Phase 1 — 托盘图标与窗口生命周期 | ✅ 完成 |
| Agent 2 | Phase 2 — 设置窗口与安全存储 | ✅ 完成 |
| Agent 3 | Phase 3/4 — API 集成与并发测试 | ✅ 完成 |
| Agent 4 | Phase 5/6/7 — 动画、夜间模式、图表 | ⚠️ 部分完成 |

---

## Phase 0 — 编译错误修复（前置）

| 问题 | 修复方式 |
|------|----------|
| `MorandiTheme.xaml:194` `ProgressBarValueConverter` 不存在 | 删除未使用的 `CustomProgressBarStyle` / `SmallProgressBarStyle` |
| `MainWindow.xaml:133` `ConverterParameter={Binding}` 无效 | 将 `ItemsControl` 时间范围按钮替换为 4 个独立 `ToggleButton` |

---

## Phase 1 — 托盘图标与窗口生命周期

**状态**：✅ 功能完成，已验证可运行

### 已实现
- `App.xaml.cs`：移除 `StartupUri`，重写 `OnStartup`，`ShutdownMode = OnExplicitShutdown`
- `App.xaml.cs`：三层全局异常捕获 + `error.log` 日志（`DispatcherUnhandledException` / `UnhandledException` / `UnobservedTaskException`）
- `MainWindow.xaml.cs`：`Deactivated` 事件自动隐藏、`CloseButton_Click` 改为 `Hide()`、`ShowNearTrayIcon()` 窗口定位
- **新建** `Services/TrayIconService.cs`：系统托盘图标 + 左键单击弹出/收起 + 右键菜单（刷新/设置/关于/退出）
- **新建** `Helpers/WindowPositionHelper.cs`：任务栏四方向检测 + DPI 感知 + 边界避让

### 技术决策变更
原计划使用 `Hardcodet.NotifyIcon.Wpf.TaskbarIcon`，但实际测试发现**纯代码创建的 `TaskbarIcon` 无法可靠注册 Windows 消息钩子**，导致鼠标点击无响应。

**最终方案**：改为 `System.Windows.Forms.NotifyIcon`（项目已启用 `UseWindowsForms`），行为完全可靠。

### 仍待完善
- [ ] 正式 `.ico` 图标文件（当前代码动态生成 16×16 位图）
- [ ] `ShowNearTrayIcon()` 触发 `PopupFadeIn` Storyboard 动画

---

## Phase 2 — 设置窗口与安全存储

**状态**：✅ 功能完成

### 已实现
- **新建** `Views/SettingsWindow.xaml/.cs`：360×480 设置窗口，莫兰迪配色
- **新建** `ViewModels/SettingsViewModel.cs`：CommunityToolkit.Mvvm 绑定
- **新建** `Services/SecureStorageService.cs`：Windows DPAPI 加密，`%AppData%/TokenUsageMonitor/keys.{platform}.encrypted`
- **新建** `Services/SettingsService.cs`：`appsettings.json` 读写，`%AppData%/TokenUsageMonitor/`

### 仍待完善
- [ ] 首次启动引导（检测无 API Key → 自动弹出 SettingsWindow）
- [ ] Bug B2：MainWindow 配置路径统一（当前读 `Config/AppSettings.json`，SettingsService 写 `%AppData%`）

---

## Phase 3/4 — API 集成与并发测试

**状态**：✅ 代码完成，但 `_apiKeys` 未加载导致真实通路未激活

### 已实现
- **新建** `Models/QuotaInfo.cs`：统一数据模型 + `QuotaStatus` 枚举
- **新建** `Services/IApiClient.cs`：接口定义
- **新建** `Services/DeepSeekApiClient.cs`：`api.deepseek.com/user/balance`
- **新建** `Services/GlmApiClient.cs`：`open.bigmodel.cn/api/paas/v4/user/info`
- **新建** `Services/KimiApiClient.cs`：`api.moonshot.cn/v1/users/me/balance`
- **新建** `Services/QuotaRefreshService.cs`：`Task.WhenAll` 并发 + 10s 独立超时
- **修改** `ViewModels/MainViewModel.cs`：`Refresh()` 接入真实 API，`ConcurrentTest()` 全链路并发测速

### 仍待完善
- [ ] **Bug B1**：`_apiKeys` 从未从 `SecureStorageService` 加载，Refresh() 直接回退到 `LoadMockData()`
- [ ] 数据缓存（启动时加载上次缓存 JSON）
- [ ] 自动刷新定时器
- [ ] MainWindow.xaml 底部缺少"并发测试"按钮 UI

---

## Phase 5/6/7 — 动画、夜间模式、图表

**状态**：⚠️ 资源已创建，但未完全接入运行时代码

### 已实现
- **修改** `Assets/Themes/MorandiTheme.xaml`：新增 `PopupFadeIn` / `PopupFadeOut` Storyboard、`LoadingSpinnerStyle`、卡片刷新脉冲 `DataTrigger`
- **新建** `Assets/Themes/MorandiDarkTheme.xaml`：完整深色主题字典
- **修改** `Services/ThemeManager.cs`：重写为 ResourceDictionary 加载/卸载方案
- **新建** `Converters/AnimatedWidthConverter.cs`：`SmoothWidth` 附加属性（300ms CubicEase）

### 修复的崩溃 Bug
`MorandiTheme.xaml` 和 `MorandiDarkTheme.xaml` 中的滚动条 Thumb 悬停动画使用了非法语法 `{StaticResource SecondaryTextBrush.Color}`，导致窗口首次渲染时 `XamlParseException` 崩溃。已改为 `ObjectAnimationUsingKeyFrames`。

### 仍待完善
- [ ] `ShowNearTrayIcon()` 未触发 `PopupFadeIn`
- [ ] XAML 进度条未使用 `AnimatedWidthConverter.SmoothWidth`
- [ ] Tab 切换交叉淡入淡出
- [ ] 托盘图标状态反馈（正常/异常变色）
- [ ] **Bug B3+B4**：主题名不统一 + 持久化静默失败
- [ ] 图表仍为 `Random.Shared` 占位数据（Phase 7 全部未开始）

---

## 阻断性 Bug 清单（当前遗留）

| # | 严重度 | 位置 | 问题 | 影响 |
|---|--------|------|------|------|
| B1 | 🔴 | MainViewModel.cs | `_apiKeys` 从未从 SecureStorageService 加载 | 真实 API 通路无法激活 |
| B2 | 🟡 | MainWindow.xaml.cs | 配置读写路径不一致 | 失焦隐藏开关可能不同步 |
| B3 | 🟡 | ThemeManager / SettingsViewModel | 主题名 "Light"/"Dark" vs "morandi_light"/"morandi_dark" 不匹配 | 主题持久化可能失败 |
| B4 | 🟡 | ThemeManager.cs | `TryPersistTheme` 反射调用不存在的方法 | 主题切换后重启丢失 |

---

## 新增文件清单（本次执行）

```
Services/
  TrayIconService.cs
  SettingsService.cs
  SecureStorageService.cs
  ISettingsService.cs
  IApiClient.cs
  DeepSeekApiClient.cs
  GlmApiClient.cs
  KimiApiClient.cs
  QuotaRefreshService.cs

Views/
  SettingsWindow.xaml
  SettingsWindow.xaml.cs

ViewModels/
  SettingsViewModel.cs

Helpers/
  WindowPositionHelper.cs

Models/
  QuotaInfo.cs
  ConcurrentTestResult.cs

Converters/
  AnimatedWidthConverter.cs

Assets/Themes/
  MorandiDarkTheme.xaml
```

---

## 编译状态

```
dotnet build → 已成功生成，0 错误，0 警告
目标框架：net10.0-windows
```

---

## 下一步建议（按优先级）

1. **修 B1**：`_apiKeys` 从 `SecureStorageService` 加载（10min）
2. **修 B2**：MainWindow 改用 `SettingsService` 读配置（5min）
3. **修 B3+B4**：统一主题名 + 修复持久化（10min）
4. 添加"并发测试"按钮 + 绑定 Command（15min）
5. 首次启动引导（20min）
6. 自动刷新定时器（15min）
