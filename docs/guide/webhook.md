# Webhook 接入

Webhook 不是一套新的业务代码，而是 `QQBotClientOptions.Mode` 的一种传输模式。切换模式只需要改配置，**事件订阅、控制器和消息发送代码都不需要修改**。

## 两种模式对比

| 模式 | 适用场景 | 说明 |
|------|---------|------|
| `WebSocket`（默认） | 长期运行的进程 / 控制台 / 容器 | 需要保持长连接，SDK 自动重连 |
| `Webhook` | Serverless / 反向代理 / 已有 Web 服务 | 平台主动 POST 事件到你的 HTTPS 地址 |

## 安装

```bash
dotnet add package Luolan.QQBot
dotnet add package Luolan.QQBot.AspNetCore   # 只有 ASP.NET Core 项目需要
```

## 模式切换

```csharp
using Luolan.QQBot;
using Luolan.QQBot.AspNetCore;
using Luolan.QQBot.Extensions;

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = builder.Configuration["Bot:AppId"]!;
    options.ClientSecret = builder.Configuration["Bot:ClientSecret"]!;
    options.Intents = Intents.Default | Intents.GroupAndC2C;

    // 唯一需要改的地方：WebSocket（默认）或 Webhook
    options.Mode = QQBotConnectionMode.Webhook;
    options.WebhookPath = "/api/qqbot/webhook";
});

// 两种模式都保留，不需要按模式删除
builder.Services.AddQQBotHostedService();

var app = builder.Build();

// 两种模式都保留：WebSocket 模式下自动跳过，不注册路由
app.MapQQBotWebhook();

// 下面的业务代码完全不变
var bot = app.Services.GetRequiredService<QQBotClient>();
bot.UseControllers();

bot.OnGroupAtMessageCreate += async e =>
{
    await bot.ReplyGroupAsync(e.Message, "收到！");
};

app.Run();
```

也可以用 `appsettings.json` 控制：

```json
{
  "Bot": {
    "AppId": "你的AppId",
    "ClientSecret": "你的ClientSecret",
    "Mode": "Webhook",
    "WebhookPath": "/api/qqbot/webhook"
  }
}
```

```csharp
var botSection = builder.Configuration.GetSection("Bot");
options.Mode = Enum.TryParse<QQBotConnectionMode>(botSection["Mode"], true, out var mode)
    ? mode
    : QQBotConnectionMode.WebSocket;
```

## 平台配置

在 [QQ 开放平台管理端](https://q.qq.com/qqbot/#/developer/webhook-setting) 配置回调地址，端口仅支持：

- `80`、`443`、`8080`、`8443`

配置后平台会立即发起一次回调地址验证（`op=13`），`MapQQBotWebhook()` 会自动处理。

## 手动处理回调

如果不想使用 `MapQQBotWebhook()`，也可以自己写端点：

```csharp
using Luolan.QQBot.Webhook;

app.MapPost("/api/qqbot/webhook", async (
    HttpContext context,
    QQBotWebhook webhook,
    CancellationToken ct) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync(ct);

    try
    {
        var response = await webhook.HandleAsync(
            body,
            context.Request.Headers["X-Signature-Ed25519"].FirstOrDefault(),
            context.Request.Headers["X-Signature-Timestamp"].FirstOrDefault(),
            ct);

        return Results.Content(response, "application/json");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});
```

`QQBotWebhook` 已由 `AddQQBot` / `AddQQBotWithHttpClientFactory` 自动注册，无需额外调用 `AddQQBotWebhook()`。

## 签名校验

平台使用 Ed25519：

1. 用 `ClientSecret` 循环填充/截断为 32 字节 seed
2. 由 seed 生成 Ed25519 密钥对
3. 校验 `X-Signature-Ed25519(hex)` 对 `X-Signature-Timestamp + body` 的签名

SDK 已内置该流程，也可以单独使用：

```csharp
using Luolan.QQBot.Webhook;

bool ok = QQBotWebhookSignature.Verify(
    clientSecret,
    body,
    signatureHeader,
    timestampHeader);

string signature = QQBotWebhookSignature.SignValidation(
    clientSecret,
    plainToken,
    eventTs);
```

## 模式差异

| 行为 | WebSocket | Webhook |
|------|-----------|---------|
| `StartAsync` | 建立长连接 | 不连接，只做就绪初始化 |
| `IsConnected` | WebSocket 已连接 | 已启动即视为就绪 |
| `OnReady` | 平台下发 READY | 启动时拉取机器人信息后合成触发 |
| 事件分发 | 网关推送 | HTTP 回调 → `EventDispatcher` |
| 消息发送 | HTTP API | HTTP API（完全相同） |

::: tip 二选一
同一个机器人不要同时开启 WebSocket 和 Webhook，否则会收到重复事件。模式由配置决定，SDK 不会同时建立长连接。
:::

## 安全建议

- 始终校验签名，不要直接信任请求体
- 使用 HTTPS（平台只接受 HTTPS 回调地址）
- 不要将 `ClientSecret` 写进仓库，使用环境变量或 User Secrets
- 回调处理应尽快返回；耗时逻辑建议丢到后台队列

## 下一步

- [ASP.NET Core 集成](/guide/aspnetcore) — Web 项目完整配置
- [Webhook API](/api/webhook) — QQBotWebhook 完整参考
- [事件系统](/guide/events) — 所有可订阅事件
