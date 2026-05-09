# TokenUsageMonitor 实现计划

## 项目概述

采用莫兰迪低饱和配色，构建 Windows 桌面 WPF 应用。需求规格见 [requirements.md](requirements.md)，视觉规格见 [design.md](design.md)。

执行报告：[implement/execution-report.md](implement/execution-report.md)

---

## 当前真实状态（2026-05-09）

### 已具备
- [x] .NET 10 WPF 项目骨架，MVVM 目录结构
- [x] CommunityToolkit.Mvvm 源生成器
- [x] 无边框圆角窗口（320×420，Radius=12），鼠标拖拽移动
- [x] 莫兰迪浅色主题 `MorandiTheme.xaml`（色板、按钮、卡片、进度条、动画、LoadingSpinner）
- [x] 莫兰迪深色主题 `MorandiDarkTheme.xaml`（完整深色版，含所有 Style + Storyboard）
- [x] 主界面 XAML 布局：标题栏、三个平台 Tab、三套 DataTemplate
- [x] **系统托盘**：WinForms NotifyIcon，左键弹出/收起，右键菜单（刷新/设置/关于/退出），启动气泡提示
- [x] **窗口定位**：WindowPositionHelper（任务栏四方向检测，DPI 感知，边界避让）
- [x] **失焦隐藏**：Deactivated 事件 + 可配置开关
- [x] **设置窗口**：SettingsWindow（API Key 三组密码框、刷新间隔、复选框、主题选择）
- [x] **DPAPI 加密存储**：SecureStorageService
- [x] **配置服务**：SettingsService（%AppData% 读写 AppSettings JSON）
- [x] **三平台 API 客户端**：GlmApiClient / KimiApiClient / DeepSeekApiClient（均实现 IApiClient）
- [x] **并发刷新服务**：QuotaRefreshService（Task.WhenAll + 10s 超时）
- [x] **QuotaInfo 数据模型**：与需求文档对齐
- [x] **MainViewModel**：Refresh → 真实 API + 模拟回退，ConcurrentTest ← 全链路并发
- [x] **AnimatedWidthConverter**：300ms CubicEase 宽度过渡
- [x] **全局异常处理**：AppDomain + Dispatcher + TaskScheduler 三层捕获，error.log 日志
- [x] **滚动条优化**：6px 宽度 + Track 背景 + Thumb hover 高亮（ObjectAnimationUsingKeyFrames）
- [x] **编译错误已修复**：`ConverterParameter={Binding}`、`ProgressBarValueConverter` 不存在

### 阻断性 Bug（4 个）

| # | 严重度 | 位置 | 问题 | 修复方案 |
|---|--------|------|------|----------|
| B1 | 🔴 | MainViewModel.cs:55 | `_apiKeys` 从未从 SecureStorageService 加载，Refresh() 直接回退到 LoadMockData | 构造时从 SecureStorageService 加载 |
| B2 | 🟡 | MainWindow.xaml.cs:29 | 配置读写路径不一致：MainWindow 读 Config/ 内置文件，SettingsService 写 %AppData% | MainWindow 改用 SettingsService |
| B3 | 🟡 | ThemeManager.cs / SettingsViewModel | 主题名 "Light"/"Dark" vs "morandi_light"/"morandi_dark" 不匹配 | 统一为 "Light"/"Dark" |
| B4 | 🟡 | ThemeManager.cs:66-93 | TryPersistTheme 反射调用不存在的方法，持久化静默失败 | 直接调用 SettingsService.Instance |

---

## 实现阶段

### Phase 1 — 托盘图标与窗口生命周期 → F-001 ~ F-005

进度: █████████░ 95%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 1.1 | 托盘图标创建 | ✅ | WinForms NotifyIcon，代码动态生成莫兰迪色位图 |
| 1.2 | App 初始化 | ✅ | OnExplicitShutdown，三层全局异常捕获 + error.log |
| 1.3 | 单击弹出/收起 | ✅ | ToggleMainWindow → ShowNearTrayIcon / Hide |
| 1.4 | 右键菜单 | ✅ | 刷新 / 设置 / 关于 / 退出 |
| 1.5 | 启动气泡提示 | ✅ | "程序已启动，单击托盘图标查看用量" 3 秒 |
| 1.6 | 窗口智能定位 | ✅ | WindowPositionHelper，任务栏四方向 + DPI 感知 |
| 1.7 | 失焦自动隐藏 | ✅ | Deactivated 事件 + 可配置开关（Bug B2 影响） |
| 1.8 | 正式 .ico 文件 | ❌ | 当前代码动态生成，无静态图标文件 |

> **技术决策**：原计划用 `Hardcodet.NotifyIcon.Wpf.TaskbarIcon`，实测无法可靠注册 Windows 消息钩子，鼠标点击无响应。最终改用 `System.Windows.Forms.NotifyIcon`（项目已启用 `UseWindowsForms`）。

---

### Phase 2 — 设置窗口与安全存储 → F-401 ~ F-405

进度: ████████░░ 80%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 2.1 | SettingsWindow | ✅ | 360×480，莫兰迪配色，三组密码框 + 下拉 + 复选框 |
| 2.2 | DPAPI 加密存储 | ✅ | SecureStorageService，%AppData% |
| 2.3 | 配置读写服务 | ✅ | SettingsService + AppSettings 类 |
| 2.4 | 首次启动引导 | ❌ | 检测无 Key → 弹出设置窗口 |

**依赖 Bug B2**（配置路径统一）后完成。

---

### Phase 3 — API 集成（三平台） → F-201 ~ F-206

进度: ████████░░ 80%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 3.1 | QuotaInfo 模型 | ✅ | Models/QuotaInfo.cs |
| 3.2 | IApiClient 接口 | ✅ | GetQuotaAsync(apiKey, ct)，10s 超时 |
| 3.3 | DeepSeekApiClient | ✅ | `api.deepseek.com/user/balance` |
| 3.4 | GlmApiClient | ✅ | `open.bigmodel.cn/api/paas/v4/user/info` |
| 3.5 | KimiApiClient | ✅ | `api.moonshot.cn/v1/users/me/balance` |
| 3.6 | QuotaRefreshService | ✅ | Task.WhenAll 并发 + 10s 独立超时 |
| 3.7 | MainViewModel 接入 | ⚠️ | 代码完成，`_apiKeys` 未加载（Bug B1） |
| 3.8 | 数据缓存 | ❌ | 启动缓存加载未实现 |
| 3.9 | 自动刷新定时器 | ❌ | |

**依赖 Bug B1**（apiKeys 加载）后激活真实通路。

---

### Phase 4 — 并发测试 → F-301 ~ F-304

进度: ██████░░░░ 60%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 4.1 | ConcurrentTestResult 模型 | ✅ | Models/ConcurrentTestResult.cs |
| 4.2 | ConcurrentTestCommand | ✅ | 全链路并发，独立计时，异常捕获 |
| 4.3 | 并发测试按钮 UI | ❌ | MainWindow.xaml 底部缺少按钮 |

---

### Phase 5 — 动画打磨

进度: █████░░░░░ 50%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 5.1 | PopupFadeIn/Out Storyboard | ✅ | 150ms/120ms CubicEase，两套主题 |
| 5.2 | AnimatedWidthConverter | ✅ | SmoothWidth 附加属性，300ms |
| 5.3 | LoadingSpinner | ✅ | 800ms 旋转 |
| 5.4 | 刷新脉冲高亮 | ✅ | CardBorderStyle DataTrigger "JustRefreshed" |
| 5.5 | 动画接入运行时 | ❌ | ShowNearTrayIcon 未触发 FadeIn，进度条未用 SmoothWidth |
| 5.6 | Tab 切换过渡 | ❌ | |

---

### Phase 6 — 夜间模式

进度: ████████░░ 85%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 6.1 | MorandiDarkTheme.xaml | ✅ | 完整深色主题（含滚动条优化+动画修复） |
| 6.2 | ThemeManager 整体切换 | ✅ | ResourceDictionary 替换方案 |
| 6.3 | 持久化 | ⚠️ | Bug B3+B4 导致静默失败 |

---

### Phase 7 — 图表精细化

进度: ░░░░░░░░░░ 0%

未开始。数据仍为 Random.Shared 占位。

---

## 最近一次变更（未提交）

| 文件 | 变更 |
|------|------|
| `App.xaml.cs` | 全局异常捕获 + error.log + OnStartup 日志 |
| `TrayIconService.cs` | 启动气泡提示 |
| `MorandiTheme.xaml` | 滚动条优化（6px + Track 背景 + Thumb hover）+ ColorAnimation→ObjectAnimation 修复 |
| `MorandiDarkTheme.xaml` | 同上 |
| `docs/implement/` | 新增执行报告 |

---

## 立即行动计划

| # | 任务 | 阶段 | 预估 |
|---|------|------|------|
| 1 | 修 B1: `_apiKeys` 从 SecureStorageService 加载 | Phase 3 | 10min |
| 2 | 修 B2: MainWindow 改用 SettingsService 读配置 | Phase 2 | 5min |
| 3 | 修 B3+B4: 统一主题名 + 修复持久化 | Phase 6 | 10min |
| 4 | 提交未提交的变更 | — | — |
| 5 | 添加"并发测试"按钮 UI + 绑定 Command | Phase 4 | 15min |
| 6 | 首次启动引导 | Phase 2 | 20min |
| 7 | 自动刷新定时器 | Phase 3 | 15min |
| 8 | 动画接入窗口 (FadeIn/Out/SmoothWidth) | Phase 5 | 20min |

---

## 技术决策记录

| 决策 | 方案 | 理由 |
|------|------|------|
| 图表实现 | 纯 XAML 自定义 | 零外部依赖，完全可控 |
| 目标框架 | .NET 10 | 本地 SDK 版本 |
| 托盘图标 | WinForms NotifyIcon | Hardcodet.NotifyIcon.Wpf 消息钩子不可靠，WinForms 行为稳定 |
| 安全存储 | Windows DPAPI | 非对称加密，不落盘明文 Key |
| MVVM 框架 | CommunityToolkit.Mvvm | 源生成器模式，零反射开销 |
| 主题切换 | ResourceDictionary 替换 | 完整切换全部色刷 + Style，无遗漏 |
| 滚动条动画 | ObjectAnimationUsingKeyFrames | ColorAnimation 内联 StaticResource 非法，DiscreteObjectKeyFrame 直接替换 Brush |

---

## 命名约定

| 上下文 | 使用 | 避免 |
|--------|------|------|
| 平台名称 | `GLM`, `KIMI`, `DeepSeek` | `Zhipu`, `智谱` |
| 项目名称 | `TokenUsageMonitor` | `TokenMonitor` |
| 资源 Key | `GlmTemplate`, `KimiTemplate`, `DeepSeekTemplate` | `ZhipuTemplate` |
