# TokenUsageMonitor 执行报告

> 记录多轮实现的实际执行结果。执行时间：2026-05-09

---

## 执行概览

| 轮次 | 内容 | 结果 |
|------|------|------|
| 第一轮 | 4 Agent 并行：托盘/设置/API/动画 | 代码完成，遗留 B1-B4 四个 bug |
| 第二轮 | Bug 修复 + 缓存/定时器/并发按钮/首次引导 | 代码完成，延迟验证通过 |
| 第三轮 | 用户实测反馈：5 个问题修复 | 设置按钮/窗口位置/密码回显/系统主题/托盘问号 |

---

## 第一轮 — Phase 0~7 代码骨架

采用 4 Agent 并行实现。成果：

- **编译错误修复**：`ProgressBarValueConverter` 不存在、`ConverterParameter={Binding}` 无效
- **Phase 1**：TrayIconService（WinForms NotifyIcon）、WindowPositionHelper、失焦隐藏
- **Phase 2**：SettingsWindow、SecureStorageService（DPAPI）、SettingsService
- **Phase 3/4**：三平台 ApiClient、QuotaRefreshService、ConcurrentTestCommand
- **Phase 5/6/7**：MorandiDarkTheme、AnimatedWidthConverter、Storyboard 动画

遗留 Bug：B1（apiKeys 未加载）、B2（配置路径不一致）、B3（主题名不匹配）、B4（反射持久化）

---

## 第二轮 — Bug 修复 + 功能补完

| 修复 | 文件 | 内容 |
|------|------|------|
| B1 | MainViewModel.cs | LoadApiKeys() 从 SecureStorageService 加载 |
| B2 | MainWindow.xaml.cs | 改用 SettingsService.Instance.Load() |
| B3 | ThemeManager + SettingsViewModel | 主题名统一 "Light"/"Dark" + 兼容旧名 |
| B4 | ThemeManager.cs | 反射改直接调用 SettingsService |

新增：
- DataCacheService（缓存 JSON）+ DispatcherTimer（自动刷新）
- 并发测试按钮 + LoadingSpinner
- 首次启动引导（无 Key → 设置窗口）
- 进度条 SmoothWidth 动画
- 全局异常处理 + error.log

---

## 第三轮 — 用户实测反馈修复（5 个问题）

### 问题 A：主页面设置齿轮无响应

**根因**：XAML 设置按钮只有图标没有 `Command` 绑定。  
**修复**：`Command="{Binding OpenSettingsCommand}"` → MainViewModel 新增 `OpenSettings()` 弹出 SettingsWindow。

### 问题 B：窗口每次固定在同一个位置

**根因**：无位置记忆机制，每次都从托盘位置计算，计算失败则 fallback 居中。  
**修复**：AppSettings 新增 `WindowLeft`/`WindowTop`，`HideWithAnimation` 时调用 `SavePosition()`，`ShowNearTrayIcon` 优先使用保存位置。

### 问题 C：设置保存后再次打开为空

**根因**：`PasswordBox.Password` 不是 DependencyProperty。`LoadApiKeys()` 将值赋给 ViewModel 属性，但无法绑定回 PasswordBox。  
**修复**：SettingsWindow 的 `Loaded` 事件直接写 `GlmPasswordBox.Password = _vm.GlmApiKey`。

### 问题 D：夜间模式不生效

**根因**：`AppSettings.Theme` 默认写死 `"Light"`，不读系统设置。  
**修复**：
- `AppSettings.Theme` 默认 `"System"`
- `ThemeManager.ApplySystemTheme()` 读注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`
- 设置窗口新增"跟随系统"选项
- 启动时 `App.OnStartup` 调用 `ApplySystemTheme()`

### 问题 E：托盘图标显示问号

**根因**：`Icon.FromHandle(hIcon)` 后立即 `DestroyIcon(hIcon)`，句柄失效。  
**修复**：
- `(Icon)Icon.FromHandle(hIcon).Clone()` 复制脱离句柄依赖
- 创建 `Assets/app.ico` 正式图标文件
- csproj 配置 `CopyToOutputDirectory`

---

### 本轮新增文件

```
Assets/app.ico                   托盘图标
Services/DataCacheService.cs    数据缓存（第二轮遗留文件）
```

---

## 编译状态

```
dotnet build → 已成功生成，0 错误，0 警告
目标框架：net10.0-windows
```

---

---

## 第四轮 — cc-switch 端点发现

**发现**：cc-switch (`github.com/farion1231/cc-switch`) 实现了相同三平台的用量查询，端点与我们的 requirements.md 完全不同：

| 平台 | 原假设端点 | 实际端点 (cc-switch) |
|------|-----------|---------------------|
| GLM | `open.bigmodel.cn/api/paas/v4/user/info` | `api.z.ai/api/monitor/usage/quota/limit` (不含 Bearer) |
| KIMI | `api.moonshot.cn/v1/users/me/balance` | `api.kimi.com/coding/v1/usages` |
| DeepSeek | `api.deepseek.com/user/balance` ✅ | 相同，但响应结构不同 (`balance_infos` 数组) |

**影响**：现有 `GlmApiClient`、`KimiApiClient`、`DeepSeekApiClient` 均需重写。

---

## 第五轮（2026-05-09 最终轮）— 代码实现

按第四轮发现的端点 + Phase 7/8/2/5 批量实现：

| 阶段 | 内容 | 关键文件 |
|------|------|----------|
| Phase 8 | UI 迁移 Windows 原生 | MorandiTheme→SystemColors DynamicResource，DarkTheme.xaml 替代 MorandiDarkTheme.xaml |
| Phase 3 | API 客户端重写 | 中文错误提示，raw body 日志，cc-switch 端点 |
| Phase 7 | 图表精细化 | ChartNormalizationConverter，HealthDataToEllipsePositionsConverter，Tooltip |
| Phase 5 | Tab 过渡 + 并发结果 UI | ContentControl 透明度动画，Footer WrapPanel |
| Phase 2 | 开机自启 | StartupService 写 HKCU Run |
| Phase 6 | ThemeManager | DWM 暗色模式 API，System→Light→Dark 循环，300ms 防抖 |

---

## 当前剩余工作

| # | 任务 | 说明 |
|---|------|------|
| 1 | 真实 API 连通性测试 | 用真实 Key 验证三平台 |
| 2 | 高 DPI 适配验证 | 125%/150%/200% 缩放 |
| 3 | 多显示器测试 | 副屏任务栏定位 |
