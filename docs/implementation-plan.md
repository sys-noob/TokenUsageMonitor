# TokenUsageMonitor 实现计划

## 项目概述

基于 `reference/coding-quota-bar.gif` 的 UI 参考，采用莫兰迪低饱和配色，构建 Windows 桌面 WPF 应用。需求规格见 [requirements.md](requirements.md)，视觉规格见 [design.md](design.md)。

---

## 当前真实状态

### 已具备
- [x] .NET 10 WPF 项目骨架，MVVM 目录结构
- [x] CommunityToolkit.Mvvm 源生成器，部分 Converter
- [x] 无边框圆角窗口（320×420，Radius=12），鼠标拖拽移动
- [x] 莫兰迪浅色主题 `MorandiTheme.xaml`（色板、按钮、卡片、进度条、滚动条样式）
- [x] 主界面 XAML 布局：标题栏、三个平台 Tab、三套 DataTemplate、模拟数据填充
- [x] Hardcodet.NotifyIcon.Wpf NuGet 包已安装（尚未调用）

### 待修复的编译/运行时错误
1. `MorandiTheme.xaml:194` — `ProgressBarValueConverter` 引用了不存在的 Converter
2. `MainWindow.xaml:133` — `ConverterParameter={Binding}` 在 WPF 中无效，时间范围按钮选中态永远不会正确
3. `CustomProgressBarStyle` / `SmallProgressBarStyle` 从未被使用，但内含上述错误引用

### 未开始的核心功能
- 系统托盘集成、窗口智能定位、失焦自动隐藏
- 设置窗口、API Key 配置、DPAPI 加密存储
- 真实 API 通信（三个平台均为模拟数据）
- 数据缓存、异常状态展示
- 并发测试功能

---

## 实现阶段

### Phase 1 — 托盘图标与窗口生命周期 → 对应 F-001 ~ F-005

应用的外壳。所有交互的入口，必须最先完成。

| # | 任务 | 对应需求 | 说明 |
|---|------|----------|------|
| 1.1 | 创建托盘图标资源 | F-001, F-004 | 16×16 ICO，莫兰迪主色 + 异常色变体 |
| 1.2 | App.xaml.cs 中初始化 NotifyIcon | F-001 | 隐藏主窗口，ShowInTaskbar=false，托盘常驻 |
| 1.3 | 单击托盘弹出/收起主窗 | F-002 | 计算托盘图标屏幕位置，弹窗出现在图标上方 |
| 1.4 | 右键上下文菜单 | F-003 | 刷新 / 设置 / 关于 / 退出，绑定 Command |
| 1.5 | 窗口智能定位 | F-102 | 根据任务栏位置（上/下/左/右）自动调整弹出方向 |
| 1.6 | 失焦自动隐藏 | F-103 | Deactivated 事件监听，可配置开关 |

**依赖**：Hardcodet.NotifyIcon.Wpf（已安装）  
**产出**：应用可常驻托盘，单击弹出/收起，右键菜单可用

---

### Phase 2 — 设置窗口与安全存储 → 对应 F-401 ~ F-405

用户输入 API Key 的前置条件。没有 Key，API 集成无法验证。

| # | 任务 | 对应需求 | 说明 |
|---|------|----------|------|
| 2.1 | 创建 SettingsWindow | F-401 | 独立窗口，三个密码框输入 API Key，保存/取消 |
| 2.2 | DPAPI 加密存储 | NF-201 | `%AppData%/TokenUsageMonitor/` 下存储加密后的 Key |
| 2.3 | 配置读写服务 | F-402, NF-203 | AppSettings.json 作为非敏感配置（刷新间隔、主题等） |
| 2.4 | 设置界面完善 | F-402~F-405 | 刷新间隔下拉、开机自启开关、失焦隐藏开关、主题选择 |
| 2.5 | 首次启动引导 | 5.1 流程 | 检测无 API Key → 弹出设置窗口 → 引导输入 |

**依赖**：Phase 1（设置窗口从托盘菜单进入）  
**产出**：用户可安全配置 API Key 和偏好设置

---

### Phase 3 — API 集成（单平台先行） → 对应 F-201 ~ F-206

先跑通一个平台的全链路，验证架构正确后快速复制到其余两个。

| # | 任务 | 对应需求 | 说明 |
|---|------|----------|------|
| 3.1 | 定义 QuotaInfo 数据模型 | F-201 | 替换当前 UsageItem 的临时职责，统一三个平台 |
| 3.2 | IApiClient 接口 + HttpClient 工厂 | — | 超时 10s、取消令牌、统一异常模型 |
| 3.3 | DeepSeek API 客户端 | F-203 | `GET /user/balance`，解析余额与套餐信息 |
| 3.4 | 接入 MainViewModel | F-201 | 用真实数据替换 LoadMockData，保留缓存兜底 |
| 3.5 | 异常状态卡片 UI | F-205 | 请求失败时显示错误提示，进度条置灰 |
| 3.6 | 数据缓存 | F-206 | 启动时加载缓存 JSON，后台静默刷新后更新 |

**验证标准**：配置 DeepSeek API Key → 刷新 → 卡片显示真实余额  
**产出**：DeepSeek 平台数据从真实 API 获取

---

### Phase 4 — 剩余平台 API + 并发测试 → 对应 F-301 ~ F-304

| # | 任务 | 对应需求 | 说明 |
|---|------|----------|------|
| 4.1 | GLM API 客户端 | F-201 | `GET /api/paas/v4/user/info`，解析余额数据 |
| 4.2 | KIMI API 客户端 | F-202 | `GET /v1/users/me/balance`，解析余额数据 |
| 4.3 | 并发刷新服务 | F-301 | `Task.WhenAll(三个平台)`，各平台独立超时互不影响 |
| 4.4 | 并发测试按钮 UI | F-302, F-303 | 按钮 loading 态，结果展示响应延迟（✓ 234ms / ✗ 超时） |
| 4.5 | 自动刷新定时器 | F-402 | 按配置的间隔周期刷新 |

**依赖**：Phase 2（需要三个平台的 API Key 均已配置）  
**产出**：三个平台数据全部从真实 API 获取，支持手动和自动刷新

---

### Phase 5 — 交互打磨 → 对应动画与视觉

| # | 任务 | 对应需求 | 说明 |
|---|------|----------|------|
| 5.1 | 弹窗淡入淡出动画 | 设计文档 §9 | 150ms 淡入 + 120ms 淡出 |
| 5.2 | 进度条宽度过渡 | 设计文档 §9 | 300ms CubicEase |
| 5.3 | Tab 切换交叉淡入淡出 | 设计文档 §9 | 150ms 内容区切换 |
| 5.4 | 数据刷新脉冲高亮 | 设计文档 §9 | 卡片背景短暂高亮 |
| 5.5 | 托盘图标状态反馈 | F-004 | 正常/异常状态切换图标 |

---

### Phase 6 — 夜间模式完善

当前 ThemeManager 只覆盖 7 个基础色刷，需要补完。

| # | 任务 | 说明 |
|---|------|------|
| 6.1 | 品牌色、状态色、图表色的深色变体 | 12 个额外色刷 |
| 6.2 | ToggleButton 选中态深色适配 | 背景色/前景色在深色模式下的可读性 |
| 6.3 | 创建 MorandiDarkTheme.xaml | 独立深色资源字典，切换时整体替换 |
| 6.4 | 持久化主题选择 | 用户偏好写入 AppSettings |

---

### Phase 7 — 图表精细化

当前图表为占位实现，需根据真实数据重做。

| # | 任务 | 说明 |
|---|------|------|
| 7.1 | 柱状图按真实数据比例缩放 | 替换当前固定 10px 宽的小方块 |
| 7.2 | X/Y 轴标签 | 时间刻度 + 用量刻度 |
| 7.3 | Tooltip 交互 | 悬停显示详细数值 |
| 7.4 | 折线图数据绑定 | HealthData 接入真实系统数据或移除占位 |

---

## 技术决策记录

| 决策 | 方案 | 理由 |
|------|------|------|
| 图表实现 | 纯 XAML 自定义 | 零外部依赖，完全可控莫兰迪配色 |
| 目标框架 | .NET 10 | 本地环境仅安装 .NET 10 SDK |
| 托盘图标 | Hardcodet.NotifyIcon.Wpf | 已集成，社区成熟方案 |
| 安全存储 | Windows DPAPI | 非对称加密，不落盘明文 Key |
| MVVM 框架 | CommunityToolkit.Mvvm | 源生成器模式，零反射开销 |

---

## 命名约定

| 上下文 | 使用 | 避免 |
|--------|------|------|
| 平台名称（代码） | `GLM`, `KIMI`, `DeepSeek` | `Zhipu`, `智谱` |
| 项目名称 | `TokenUsageMonitor` | `TokenMonitor` |
| 数据模板 Key | `GlmTemplate`, `KimiTemplate`, `DeepSeekTemplate` | `ZhipuTemplate` |
