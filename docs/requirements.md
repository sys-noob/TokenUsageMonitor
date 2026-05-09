# Token Usage Monitor - 需求文档

> 本文档作为项目开发的核心依据，涵盖功能需求、非功能需求、界面设计、数据规格及验收标准。

---

## 1. 项目背景与目标

### 1.1 背景
开发者日常同时使用多个AI Coding平台的付费计划（GLM Coding Plan、KIMI Coding Plan、DeepSeek）。各平台token/额度消耗情况分散，需要逐个登录网页查看，效率低下。需要一个轻量级的常驻工具，聚合展示各平台剩余配额。

### 1.2 目标
开发一款Windows桌面应用，常驻系统托盘，提供一键查看多平台token配额的优雅体验。界面遵循 Windows 原生设计风格，使用系统主题色，自动跟随浅色/深色模式。

---

## 2. 功能需求 (Functional Requirements)

### 2.1 系统托盘集成

| 编号 | 需求 | 优先级 | 描述 |
|------|------|--------|------|
| F-001 | 托盘图标常驻 | P0 | 应用启动后最小化至系统托盘，不占用任务栏 |
| F-002 | 单击弹出主窗 | P0 | 单击（左键）托盘图标，在图标附近弹出主详情浮窗；再次单击关闭 |
| F-003 | 右键上下文菜单 | P0 | 右键托盘图标弹出菜单：刷新 / 设置 / 关于 / 退出 |
| F-004 | 托盘图标状态反馈 | P1 | 所有平台正常时图标为彩色；任一平台异常时图标变色/带角标提示 |
| F-005 | 开机自启选项 | P2 | 设置中可选"开机自动启动" |

### 2.2 主详情浮窗

| 编号 | 需求 | 优先级 | 描述 |
|------|------|--------|------|
| F-101 | 无边框圆角弹窗 | P0 | 窗口无标题栏、圆角（Radius=12）、阴影效果 |
| F-102 | 智能定位 | P0 | 弹窗出现在托盘图标上方，自动避让屏幕边缘 |
| F-103 | 失焦自动隐藏 | P1 | 点击窗体外部区域，浮窗自动收起（可设置开关） |
| F-104 | 顶部标题栏 | P0 | 显示应用名称"Token Monitor" + 刷新按钮（圆形箭头图标）+ 关闭按钮（×） |
| F-105 | 平台卡片列表 | P0 | 垂直排列3个平台卡片（GLM / KIMI / DeepSeek） |
| F-106 | 卡片内容 | P0 | 每个卡片包含：平台Logo（左侧色块）+ 平台名称 + 已用/总额数字 + 百分比 + 横向进度条 |
| F-107 | 进度条样式 | P0 | 圆角进度条，配色跟随系统主题（使用系统强调色 AccentColor），宽度反映剩余比例 |
| F-108 | 最后更新时间 | P1 | 浮窗底部显示"最后更新: 12:34:56"格式的时间戳 |
| F-109 | 并发测试按钮 | P0 | 底部放置"并发测试"按钮，点击后触发多平台并发请求 |

### 2.3 平台配额数据展示

| 编号 | 需求 | 优先级 | 描述 |
|------|------|--------|------|
| F-201 | GLM Coding Plan | P0 | 显示当前余额/总额（如："已用 1,234 / 10,000"），支持数字千分位格式化 |
| F-202 | KIMI Coding Plan | P0 | 同上，适配Moonshot API返回数据结构 |
| F-203 | DeepSeek | P0 | 同上，适配DeepSeek API返回数据结构 |
| F-204 | 数据格式化 | P1 | token数 > 1000时以"1.2k"简写；金额类以"¥12.34"显示 |
| F-205 | 异常状态显示 | P0 | API请求失败时，卡片显示错误提示（如"请求超时"、"Key无效"），进度条置灰 |
| F-206 | 数据缓存 | P1 | 应用启动时先展示上次缓存数据，后台静默刷新 |

### 2.4 并发测试功能

| 编号 | 需求 | 优先级 | 描述 |
|------|------|--------|------|
| F-301 | 一键并发测试 | P0 | 点击按钮后同时向三个平台发起配额查询请求 |
| F-302 | 测试结果显示 | P0 | 在卡片上或弹窗中以图标+延迟毫秒展示各平台响应状态（如"✓ 234ms"、"✗ 超时"） |
| F-303 | 测试进度反馈 | P1 | 测试过程中按钮显示旋转loading态，防止重复点击 |
| F-304 | 测试日志 | P2 | 可选展示原始HTTP响应详情（展开/收起） |

### 2.5 设置功能

| 编号 | 需求 | 优先级 | 描述 |
|------|------|--------|------|
| F-401 | API Key配置 | P0 | 弹窗或独立设置页，输入三个平台的API Key；Key输入框默认隐藏（密码样式） |
| F-402 | 刷新间隔设置 | P1 | 可选：1分钟 / 5分钟 / 15分钟 / 30分钟 / 手动刷新，默认5分钟 |
| F-403 | 开机自启 | P2 | 开关控制是否写入注册表/启动文件夹实现开机自启 |
| F-404 | 主题 | P2 | 跟随系统 / 浅色 / 深色三种模式，默认跟随系统 |
| F-405 | 失焦自动隐藏开关 | P1 | 开关控制 |

---

## 3. 非功能需求 (Non-Functional Requirements)

### 3.1 性能

| 编号 | 需求 | 指标 |
|------|------|------|
| NF-001 | 启动速度 | 冷启动 < 3秒（从双击到托盘图标出现） |
| NF-002 | 内存占用 | 常驻内存 < 80MB |
| NF-003 | 单请求超时 | 单个API请求超时时间 configurable，默认 10 秒 |
| NF-004 | 并发请求 | 三平台并发总耗时 < 15 秒（受最慢平台限制） |
| NF-005 | UI响应 | 界面操作（点击/弹出）无明显卡顿，帧率 > 30fps |

### 3.2 可靠性

| 编号 | 需求 | 描述 |
|------|------|------|
| NF-101 | 网络异常容错 | 任一平台API失败不影响其他平台展示；失败平台显示上次缓存数据（若有）或错误状态 |
| NF-102 | 崩溃恢复 | 异常崩溃后重启能恢复上次配置（API Key除外，需重新输入） |
| NF-103 | API限流保护 | 不对平台API进行过度请求，严格遵守各平台rate limit |

### 3.3 安全性

| 编号 | 需求 | 描述 |
|------|------|------|
| NF-201 | API Key存储 | 配置文件中的API Key使用 Windows DPAPI 加密存储，避免明文落盘 |
| NF-202 | 无数据上传 | 应用仅向官方API发起请求，不向任何第三方服务器传输数据 |
| NF-203 | 配置隔离 | 用户配置存储在 `%AppData%/TokenMonitor/` 下，不污染安装目录 |

### 3.4 兼容性

| 编号 | 需求 | 描述 |
|------|------|------|
| NF-301 | Windows版本 | 支持 Windows 10 (1903+) 和 Windows 11 |
| NF-302 | DPI适配 | 支持125%、150%、200%等高DPI缩放，界面不模糊 |
| NF-303 | 多显示器 | 弹窗定位在正确显示器的任务栏区域 |

---

## 4. 数据需求 (Data Requirements)

### 4.1 数据模型

主数据模型——描述单个平台的配额状态，UI 层的卡片/列表/服务项均映射到此结构：

```csharp
// 平台配额信息（统一数据模型）
public class QuotaInfo
{
    public string PlatformName { get; set; }      // "GLM Coding Plan"
    public string PlatformId { get; set; }         // "glm"
    public double UsedAmount { get; set; }         // 已用量
    public double TotalAmount { get; set; }        // 总额度
    public string Unit { get; set; }               // "tokens" / "cny" / "usd"
    public DateTime? ExpiryDate { get; set; }      // 过期时间（如有）
    public QuotaStatus Status { get; set; }        // Normal / Error / Loading / Timeout
    public string ErrorMessage { get; set; }       // 错误描述
    public DateTime LastUpdated { get; set; }      // 数据更新时间

    // 展示用辅助属性
    public string DisplayPercent => $"{UsedAmount / TotalAmount * 100:F0}%";
    public double Percentage => UsedAmount / Math.Max(TotalAmount, 1) * 100;
}

public enum QuotaStatus { Normal, Error, Loading, Timeout }
```

界面层附加模型（UI 专用，不与 API 数据模型混淆）：

```csharp
// 平台元信息（Tab 切换、品牌色）
public class PlatformInfo
{
    public string Name { get; set; }              // "GLM" / "KIMI" / "DeepSeek"
    public string DisplayName { get; set; }        // 展示名称
    public bool IsPro { get; set; }                // PRO 订阅标识
}

// 服务状态条目（DeepSeek 等平台的服务健康度列表）
public class ServiceStatusItem
{
    public string Name { get; set; }
    public string StatusText { get; set; }         // "运行正常" / "异常"
    public double Percentage { get; set; }         // 99.86
    public bool IsHealthy { get; set; }
}

// 图表数据点
public class ChartDataPoint
{
    public string Label { get; set; }              // X 轴标签
    public List<double> Values { get; set; }       // 堆叠数据
}
```

### 4.2 API接口规格

> 端点来源：cc-switch (github.com/farion1231/cc-switch) 已验证的生产级实现。

**GLM (智谱AI)**
```
Endpoint: GET https://api.z.ai/api/monitor/usage/quota/limit
Headers: Authorization: {api_key}          ← 注意：不带 Bearer 前缀
        Content-Type: application/json
        Accept-Language: en-US,en
Response: {
  "success": true,
  "data": {
    "level": "pro",
    "limits": [
      { "type": "TOKENS_LIMIT", "percentage": 44.0, "nextResetTime": 1774967594803 },
      { "type": "TOKENS_LIMIT", "percentage": 53.0, "nextResetTime": 2000000000000 }
    ]
  }
}
解析规则: limits[] 中 type=TOKENS_LIMIT 的条目按 nextResetTime 升序，
         第 0 条=5小时桶(five_hour)，第 1 条=周限额(weekly_limit)
状态: ✅ 端点已确认（cc-switch 生产验证）
```

**KIMI Coding Plan**
```
Endpoint: GET https://api.kimi.com/coding/v1/usages
Headers: Authorization: Bearer {api_key}
        Accept: application/json
Response: {
  "limits": [
    { "detail": { "limit": 10000, "remaining": 8765, "resetTime": "..." } }
  ],
  "usage": { "limit": 100000, "remaining": 50000, "resetTime": "..." }
}
解析规则: limits[].detail 数组对应 5 小时桶，usage 对象对应周限额
状态: ✅ 端点已确认（cc-switch 生产验证）
```

**DeepSeek**
```
Endpoint: GET https://api.deepseek.com/user/balance
Headers: Authorization: Bearer {api_key}
        Accept: application/json
Response: {
  "is_available": true,
  "balance_infos": [
    { "currency": "CNY", "total_balance": "100.00", "granted_balance": "50.00", "topped_up_balance": "50.00" }
  ]
}
解析规则: balance_infos[0].total_balance 为总余额，
         total_balance - granted_balance - topped_up_balance = 已用量
状态: ✅ 端点已确认（cc-switch 生产验证），响应结构已更新
```

### 4.3 配置文件

非敏感配置存储在 `%AppData%/TokenUsageMonitor/appsettings.json`：

```json
{
  "refreshIntervalMinutes": 5,
  "autoHideOnLostFocus": true,
  "startWithWindows": false,
  "theme": "morandi_light"
}
```

API Key 使用 DPAPI 单独加密存储，不写入明文配置文件：

```
%AppData%/TokenUsageMonitor/
├── appsettings.json           公共配置（明文）
├── keys.glm.encrypted         GLM API Key（DPAPI 加密）
├── keys.kimi.encrypted        KIMI API Key（DPAPI 加密）
└── keys.deepseek.encrypted    DeepSeek API Key（DPAPI 加密）
```

---

## 5. 交互流程

### 5.1 首次启动流程
```
启动应用 → 检查配置 → 无API Key → 弹出设置窗口
                                    ↓
                              用户输入3个Key
                                    ↓
                              保存配置 → 自动刷新数据 → 显示托盘图标
```

### 5.2 日常使用流程
```
用户单击托盘图标 → 弹窗弹出（显示缓存数据）
         ↓
  后台静默刷新（若超过刷新间隔）
         ↓
  用户查看/点击并发测试/进入设置
         ↓
  点击外部 → 弹窗收起（若开启失焦隐藏）
```

### 5.3 并发测试流程
```
用户点击"并发测试" → UI进入loading态
         ↓
  Task.WhenAll(3个平台的GetQuotaAsync())
         ↓
  所有请求返回 → 更新各卡片延迟显示
         ↓
  按钮恢复可点击态
```

---

## 6. 验收标准 (Acceptance Criteria)

### 6.1 功能验收

- [ ] 应用启动后能在系统托盘看到图标
- [ ] 单击托盘图标能正确弹出/收起浮窗
- [ ] 右键托盘图标能看到完整菜单且功能可用
- [ ] 配置有效API Key后能正确显示三个平台的配额数据
- [ ] 进度条宽度与百分比正确对应
- [ ] 并发测试按钮能同时发起3个请求并展示结果
- [ ] 设置窗口能正确保存/读取配置
- [ ] 失焦自动隐藏功能正常工作

### 6.2 视觉验收

- [ ] 整体配色与 Windows 原生风格一致，使用系统主题色
- [ ] 弹窗圆角、阴影、动画效果与 Windows 11 原生风格协调
- [ ] 三个平台卡片有明确的品牌色区分
- [ ] 125%/150% DPI下界面不模糊、不重叠
- [ ] 浅色/深色/跟随系统三种模式均正常显示

### 6.3 性能验收

- [ ] 冷启动 < 3秒
- [ ] 常驻内存 < 80MB
- [ ] 弹窗动画流畅无明显卡顿
- [ ] 单API请求超时后不影响其他平台展示

---

## 7. 风险与假设

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 各平台API文档/端点变更 | 高 | 封装API客户端，预留适配层；关注官方文档更新 |
| API Key泄露 | 高 | 使用DPAPI加密存储；README中明确安全声明 |
| Windows Defender误报 | 中 | 使用合法签名（如有）或在README中说明 |
| 高DPI下WPF渲染问题 | 中 | 开发中在125%/150% DPI环境测试 |
| 托盘图标在不同任务栏位置的定位 | 低 | 使用NotifyIcon库自带的位置计算 |
