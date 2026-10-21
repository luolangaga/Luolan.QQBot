# 从 v1.4 迁移

v1.5.0 保持向后兼容，主要新增官方新域名适配、Webhook 接入、群管理 API 和新事件。

## 升级步骤

### 1. 更新 NuGet 包

```bash
dotnet add package Luolan.QQBot --version 1.5.0
```

### 2. 检查事件域名

v1.5.0 默认使用官方新域名：

| 配置 | v1.4 | v1.5.0 |
|------|------|--------|
| API 地址 | `https://api.sgroup.qq.com` | `https://api.bot.qq.com` |
| 鉴权地址 | `https://bots.qq.com/app/getAppAccessToken` | `https://api.bot.qq.com/app/getAppAccessToken` |

如果你的网络需要继续使用旧地址，可以覆盖：

```csharp
var options = new QQBotClientOptions
{
    AppId = "...",
    ClientSecret = "...",
    ApiBaseUrlOverride = "https://api.sgroup.qq.com",
    AuthUrlOverride = "https://bots.qq.com/app/getAppAccessToken"
};
```

### 3. 按需订阅新 Intents

```csharp
.WithIntents(
    Intents.Default
    | Intents.GroupAndC2C     // 群@/全量群消息/C2C/好友事件 (1<<25)
    | Intents.GroupMembers    // 群成员/入群申请 (1<<24)
)
```

## v1.5.0 新增功能

### Webhook 接入

```csharp
builder.Services.AddQQBotWebhook("你的ClientSecret");

app.MapPost("/api/qqbot/webhook", async (HttpContext ctx, QQBotWebhook webhook) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();
    var response = await webhook.HandleAsync(
        body,
        ctx.Request.Headers["X-Signature-Ed25519"].FirstOrDefault(),
        ctx.Request.Headers["X-Signature-Timestamp"].FirstOrDefault());
    return Results.Content(response, "application/json");
});
```

详见 [Webhook 接入](/guide/webhook)。

### 新事件

| 事件 | 说明 |
|------|------|
| `OnGroupMessageCreate` | 全量群消息（需开启"接收所有消息"） |
| `OnGroupMemberAdd` / `OnGroupMemberRemove` | 群成员加入/退出 |
| `OnGroupJoinRequest` | 入群申请（机器人需为管理员） |
| `OnSubscribeMessageStatus` | 订阅消息授权状态变更 |

### 新 API

| 分组 | 能力 |
|------|------|
| 消息 | 单聊流式消息、群/单聊撤回、分片上传、输入中状态、互动召回 |
| 群管理 | 群信息、机器人群内状态、群成员、批量移除、入群申请、自动审批策略、黑名单、禁言 |
| 互动 | 自定义菜单、指令面板、互动响应、分享链接 |

### 控制器构造函数注入

v1.5.0 修复了 `UseControllers()` 未传递 DI 容器的问题：

```csharp
public class HelloController : QQBotController
{
    private readonly ILogger<HelloController> _logger;

    public HelloController(ILogger<HelloController> logger)
        => _logger = logger;

    [Command("hello")]
    public string Hello(string name) => $"Hello, {name}!";
}
```

## 破坏性变更

v1.5.0 **没有**破坏性变更，但有以下行为调整：

| 变更 | 说明 |
|------|------|
| 默认 API 域名 | 改为 `https://api.bot.qq.com`，可用 `ApiBaseUrlOverride` 覆盖 |
| 速率限制默认值 | 令牌桶容量 100 / 每秒补充 20（原 60 / 1），可通过 options 调整 |
| `ControllerManager` 构造 | 新增可选 `IServiceProvider` 参数，旧调用方式仍可用 |
| 新增依赖 | `BouncyCastle.Cryptography`（用于 Webhook Ed25519 签名校验） |

## 从 v1.3 升级到 v1.4

如果你仍在使用 v1.3，v1.4 的主要变化：

### 1. 控制器模式

```csharp
public class MyController : QQBotController
{
    [Command("hello")]
    public string Hello(string name) => $"Hello, {name}!";
}

bot.UseControllers();
```

### 2. 增强的命令解析器

`CommandParser` 支持引号参数：

```csharp
// 用户输入: /say "hello world" test
var args = CommandParser.Parse(input);
// args = ["/say", "hello world", "test"]
```

### 3. 丰富的类型转换

支持 `bool`、`enum`、`Guid`、可空类型等。

### 4. 速率限制器

```csharp
var limiter = new RateLimiter(capacity: 60, refillRate: 1);
```

### 5. Markdown 模板

使用 `custom_template_id`（字符串），旧版 `template_id`（int）已标记为 `[Obsolete]`。

```csharp
// ✅ 新写法
var md = new MarkdownBuilder().UseTemplate("custom_template_id_string").Build();

// ❌ 已弃用
var md = new MarkdownBuilder().UseTemplate(12345).Build();
```
