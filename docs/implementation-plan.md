# TokenUsageMonitor 实现计划

## 项目概述

采用莫兰迪低饱和配色，构建 Windows 桌面 WPF 应用。需求规格见 [requirements.md](requirements.md)，视觉规格见 [design.md](design.md)。

最新审查报告：[review-2026-05-09.md](review-2026-05-09.md)

---

## 当前真实状态（2026-05-09）

### 已具备
- [x] .NET 10 WPF 项目骨架，MVVM 目录结构
- [x] CommunityToolkit.Mvvm 源生成器
- [x] 无边框圆角窗口（320×420，Radius=12），鼠标拖拽移动
- [x] 莫兰迪浅色主题 `MorandiTheme.xaml`（色板、按钮、卡片、进度条、滚动条、动画、LoadingSpinner）
- [x] 莫兰迪深色主题 `MorandiDarkTheme.xaml`（完整的深色版，含所有 Style + Storyboard）
- [x] 主界面 XAML 布局：标题栏、三个平台 Tab、三套 DataTemplate
- [x] **系统托盘**：TrayIconService（左键弹出/收起，右键菜单刷新/设置/关于/退出）
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
- [x] **已修复的遗留问题**：`ConverterParameter={Binding}` 错误、`ProgressBarValueConverter` 不存在、`CustomProgressBarStyle` 未使用

### 阻断性 Bug（必须先修）

| # | 严重度 | 位置 | 问题 | 修复方案 |
|---|--------|------|------|----------|
| B1 | 🔴 | MainViewModel.cs:55 | `_apiKeys` 从未从存储加载，Refresh() 直接回退到 LoadMockData | 构造时从 SecureStorageService 加载 |
| B2 | 🔴 | MainWindow.xaml.cs:29 | 配置读写路径不一致：MainWindow 读 Config/ 内置文件，SettingsService 写 %AppData% | MainWindow 改用 SettingsService |
| B3 | 🟡 | ThemeManager.cs / SettingsViewModel | 主题名 "Light"/"Dark" vs "morandi_light"/"morandi_dark" 不匹配 | 统一为 "Light"/"Dark" |
| B4 | 🟡 | ThemeManager.cs:66-93 | TryPersistTheme 反射调用不存在的方法，持久化静默失败 | 直接调用 SettingsService.Instance |

---

## 实现阶段

### Phase 1 — 托盘图标与窗口生命周期 → 对应 F-001 ~ F-005

进度: ████████░░ 85%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 1.1 | 创建托盘图标资源 | ❌ | 缺 .ico 文件，当前代码动态生成 |
| 1.2 | TrayIconService 初始化 | ✅ | App.xaml.cs 中 OnStartup，ShutdownMode.OnExplicitShutdown |
| 1.3 | 单击弹出/收起 | ✅ | ToggleMainWindow → ShowNearTrayIcon / Hide |
| 1.4 | 右键上下文菜单 | ✅ | 刷新 / 设置 / 关于 / 退出 |
| 1.5 | 窗口智能定位 | ✅ | WindowPositionHelper，DPI 感知，四方向避让 |
| 1.6 | 失焦自动隐藏 | ✅ | Deactivated 事件，由配置开关控制 |

**剩余**: 正式 .ico 文件 + ShowNearTrayIcon 触发 PopupFadeIn 动画

---

### Phase 2 — 设置窗口与安全存储 → 对应 F-401 ~ F-405

进度: ████████░░ 80%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 2.1 | SettingsWindow | ✅ | 三平台密码框、刷新间隔、复选框、主题选择 |
| 2.2 | DPAPI 加密存储 | ✅ | SecureStorageService，%AppData% |
| 2.3 | 配置读写服务 | ✅ | SettingsService + AppSettings 类 |
| 2.4 | 设置界面完善 | ✅ | 所有 UI 控件 + 绑定完成 |
| 2.5 | 首次启动引导 | ❌ | 检测无 Key → 弹出设置窗口（未实现） |

**剩余**: 修 Bug B2（配置路径统一）+ 首次启动引导

---

### Phase 3 — API 集成（三平台） → 对应 F-201 ~ F-206

进度: ████████░░ 80%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 3.1 | QuotaInfo 数据模型 | ✅ | Models/QuotaInfo.cs |
| 3.2 | IApiClient 接口 | ✅ | Services/IApiClient.cs，10s 超时，取消令牌 |
| 3.3 | DeepSeek API 客户端 | ✅ | Services/DeepSeekApiClient.cs |
| 3.4 | GLM API 客户端 | ✅ | Services/GlmApiClient.cs |
| 3.5 | KIMI API 客户端 | ✅ | Services/KimiApiClient.cs |
| 3.6 | 并发刷新服务 | ✅ | QuotaRefreshService，Task.WhenAll |
| 3.7 | 接入 MainViewModel | ⚠️ | 代码逻辑完成，但 _apiKeys 未加载（B1） |
| 3.8 | 异常状态卡片 UI | ⚠️ | 代码逻辑完成，因 B1 无法触发真实通路 |
| 3.9 | 数据缓存 | ❌ | 启动缓存加载未实现 |

**剩余**: 修 Bug B1 + 数据缓存 + 图表真实数据替换

---

### Phase 4 — 并发测试 → 对应 F-301 ~ F-304

进度: ██████░░░░ 60%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 4.1 | ConcurrentTestResult 模型 | ✅ | Models/ConcurrentTestResult.cs |
| 4.2 | ConcurrentTestCommand | ✅ | 全链路并发，独立计时，异常捕获 |
| 4.3 | 并发测试按钮 UI | ❌ | MainWindow.xaml 底部缺少"并发测试"按钮 |
| 4.4 | 自动刷新定时器 | ❌ | 按配置间隔周期刷新 |

**剩余**: 添加按钮 + 绑定 Command + 结果显示 UI + 定时刷新

---

### Phase 5 — 交互打磨 → 对应动画与视觉

进度: ████░░░░░░ 40%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 5.1 | 弹窗淡入淡出 Storyboard | ✅ | PopupFadeIn / PopupFadeOut 在主题文件中 |
| 5.2 | AnimatedWidthConverter | ✅ | 300ms CubicEase 附加属性 |
| 5.3 | 刷新脉冲高亮 | ✅ | CardBorderStyle + DataTrigger "JustRefreshed" |
| 5.4 | LoadingSpinner | ✅ | 800ms 旋转，主题文件中 |
| 5.5 | 动画接入窗口 | ❌ | ShowNearTrayIcon 未触发 PopupFadeIn |
| 5.6 | 进度条宽度绑定 Converter | ❌ | XAML 仍用静态 Width 而非 SmoothWidth |
| 5.7 | Tab 切换交叉淡入淡出 | ❌ | 未实现 |
| 5.8 | 托盘图标状态反馈 | ❌ | 依赖正式 .ico 文件 |

---

### Phase 6 — 夜间模式完善

进度: ████████░░ 85%

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 6.1 | MorandiDarkTheme.xaml | ✅ | 完整深色主题：色板、动画、全部 Style |
| 6.2 | ToggleButton 深色适配 | ✅ | 选中态改用 #D8D4D0 / #2D2A28 确保可读性 |
| 6.3 | ThemeManager 整体切换 | ✅ | 替换 ResourceDictionary 方案 |
| 6.4 | 持久化主题选择 | ⚠️ | 因 B3 + B4 静默失败 |

**剩余**: 修 Bug B3 + B4（主题名统一 + 持久化修复）

---

### Phase 7 — 图表精细化

进度: ░░░░░░░░░░ 0%

尚未开始。图表仍使用 `Random.Shared` 占位数据。

| # | 任务 | 状态 | 说明 |
|---|------|------|------|
| 7.1 | 柱状图真实数据比例 | ❌ | |
| 7.2 | X/Y 轴标签 | ❌ | |
| 7.3 | Tooltip 交互 | ❌ | |
| 7.4 | 折线图数据绑定 | ❌ | |

---

## 立即行动计划

按优先级排序：

| 顺序 | 任务 | 阶段 | 预估 |
|------|------|------|------|
| 1 | 修 B1: `_apiKeys` 从 SecureStorageService 加载 | Phase 3 | 10min |
| 2 | 修 B2: MainWindow 改用 SettingsService 读配置 | Phase 2 | 5min |
| 3 | 修 B3+B4: 统一主题名 + 修复持久化 | Phase 6 | 10min |
| 4 | 添加"并发测试"按钮 + 绑定 Command | Phase 4 | 15min |
| 5 | 提交全部 28 个文件的变更 | — | — |
| 6 | 首次启动引导（无 Key → 弹出设置） | Phase 2 | 20min |
| 7 | 数据缓存（启动加载上次数据） | Phase 3 | 30min |
| 8 | 动画接入窗口 (FadeIn/FadeOut/SmoothWidth) | Phase 5 | 20min |
| 9 | 自动刷新定时器 | Phase 4 | 15min |

---

## 技术决策记录

| 决策 | 方案 | 理由 |
|------|------|------|
| 图表实现 | 纯 XAML 自定义 | 零外部依赖，完全可控莫兰迪配色 |
| 目标框架 | .NET 10 | 本地 SDK 版本 |
| 托盘图标 | System.Windows.Forms.NotifyIcon | 已通过 UseWindowsForms 集成，无需额外 NuGet |
| 安全存储 | Windows DPAPI | 非对称加密，不落盘明文 Key |
| MVVM 框架 | CommunityToolkit.Mvvm | 源生成器模式，零反射开销 |
| 主题切换 | 替换 ResourceDictionary | 完整切换全部色刷+Style，无遗漏 |

---

## 命名约定

| 上下文 | 使用 | 避免 |
|--------|------|------|
| 平台名称（代码） | `GLM`, `KIMI`, `DeepSeek` | `Zhipu`, `智谱` |
| 项目名称 | `TokenUsageMonitor` | `TokenMonitor` |
| 数据模板 Key | `GlmTemplate`, `KimiTemplate`, `DeepSeekTemplate` | `ZhipuTemplate` |
