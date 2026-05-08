# TokenUsageMonitor 实现计划

## 项目概述

基于 `reference/coding-quota-bar.gif` 的 UI 参考，采用莫兰迪低饱和配色方案，构建 Windows 桌面 WPF 应用。

---

## 已完成功能

### 基础架构
- [x] .NET 10 WPF 项目搭建（原 .NET 8，已升级）
- [x] MVVM 目录结构（Models / ViewModels / Views / Converters / Services）
- [x] CommunityToolkit.Mvvm 源生成器集成
- [x] Git 版本管理初始化

### 视觉设计
- [x] 莫兰迪浅色主题 `MorandiTheme.xaml`
- [x] 设计文档 `design.md`（配色、布局、字体、动画、组件规格）
- [x] 无边框圆角窗口（Radius=12）

### 主界面 UI
- [x] 标题栏：应用名称 + 图标按钮组（刷新/设置/夜间模式/通知）
- [x] 平台 Tab 切换（GLM / KIMI / DeepSeek），居中对齐
- [x] **GLM 页面**：MCP用量大卡片 + 额度小卡片 + Token/MCP切换 + 时间维度切换 + 柱状图 + 折线图
- [x] **KIMI 页面**：模型列表（多项用量卡片 + 进度条）
- [x] **DeepSeek 页面**：余额卡片 + API服务状态列表
- [x] 底部更新时间戳
- [x] 窗口鼠标拖拽移动

### 数据与绑定
- [x] MainViewModel 模拟数据填充
- [x] PercentageConverter / StringEqualityConverter / InverseBooleanConverter / HealthDataToPointsConverter
- [x] PlatformTemplateSelector 多平台内容切换

---

## 待完成功能

### 高优先级
- [ ] **夜间模式切换** — 深色主题 `MorandiDarkTheme.xaml`，月亮图标点击切换
- [ ] **托盘图标集成** — Hardcodet.NotifyIcon.Wpf，单击弹出/收起，右键菜单
- [ ] **窗口智能定位** — 弹窗出现在托盘图标上方，自动避让屏幕边缘
- [ ] **失焦自动隐藏** — 点击窗体外部区域自动收起

### 中优先级
- [ ] **图表精度优化** — 柱状图/折线图数据绑定与 Tooltip 交互
- [ ] **设置窗口** — API Key 配置（密码框）、刷新间隔、开机自启、失焦隐藏开关
- [ ] **深色主题完善** — 所有组件在深色模式下的视觉适配

### 低优先级
- [ ] **API 数据接入** — 智谱/ Moonshot / DeepSeek 真实 API 请求与数据解析
- [ ] **数据缓存** — 本地缓存上次数据，启动时先展示缓存
- [ ] **异常状态处理** — API 失败时卡片显示错误提示，进度条置灰
- [ ] **并发测试** — 一键向三平台并发请求，展示延迟
- [ ] **动画完善** — 弹窗淡入淡出、进度条平滑过渡、Tab 切换交叉淡入淡出
- [ ] **PRO 标签动态显示** — 根据平台订阅状态显示/隐藏

---

## 技术决策记录

| 决策 | 方案 | 理由 |
|------|------|------|
| 图表实现 | 纯 XAML 自定义（Rectangle/Polyline） | 零依赖，完全可控莫兰迪配色 |
| 目标框架 | .NET 10 | 用户本地环境仅安装 .NET 10 SDK |
| 托盘图标 | Hardcodet.NotifyIcon.Wpf | 已引入 NuGet 包，社区成熟方案 |
| 配置存储 | `%AppData%/TokenUsageMonitor/` + DPAPI 加密 | 需求文档要求安全存储 API Key |

---

## 当前已知问题

1. 图表为占位实现，柱状图高度未按真实数据比例缩放
2. 设置/夜间模式/通知按钮仅有图标，功能未绑定
3. 窗口首次弹出位置为默认位置，未关联托盘图标位置
