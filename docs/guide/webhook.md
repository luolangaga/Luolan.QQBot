# Webhook 接入

除了 WebSocket，QQ 开放平台还支持通过 HTTP 回调（Webhook）推送事件。SDK 在 v1.5.0 提供了开箱即用的 `QQBotWebhook`。

## 两种接入方式对比

| 方式 | 适用场景 | 说明 |
|------|---------|------|
| WebSocket | 长期运行的进程 / 控制台 / 容器 | 需要保持长连接，SDK 自动重连 |
| Webhook | Serverless / 反向代理 / 已有 Web 服务 | 平台主动 POST 事件到你的 HTTPS 地址 |

::: tip 二选一
同一个机器人不要同时开启 WebSocket 和 Webhook，否则会收到重复事件。
:::

## 平台配置

在 [QQ 开放平台管理端](https://q.qq.com/qqbot/#/developer/webhook-setting) 配置回调地址。端口仅支持：

- `80`、`443`、`8080`、`8443`

配置后平台会立即发起一次回调地址验证（`op=13`）。

## 注册服务

```csharp
using Luolan.QQBot.Extensions;

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.GroupAndC2C | Intents.GroupMembers;
});

// 注册 Webhook 处理器
builder.Services.AddQQBotWebhook("你的ClientSecret");

// Webhook 模式不要再注册 AddQQBotHostedService()
```

## 处理回调

```csharp
using Luolan.QQBot.Webhook;

app.MapPost("/api/qqbot/webhook", async (
    HttpContext context,
    QQBotWebhook webhook,
    CancellationToken ct) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync(ct);

    var signature = context.Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
    var timestamp = context.Request.Headers["X-Signature-Timestamp"].FirstOrDefault();

    try
    {
        // 自动处理 op=13 验证、签名校验、事件分发和 op=12 ACK
        var response = await webhook.HandleAsync(body, signature, timestamp, ct);
        return Results.Content(response, "application/json");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (System.Text.Json.JsonException)
    {
        return Results.BadRequest();
    }
});
```

`HandleAsync` 的返回值：

| 场景 | 返回 |
|------|------|
| 回调地址验证（op=13） | `{"plain_token":"...","signature":"..."}` |
| 事件推送（op=0） | `{"op":12}` |
| 签名错误 | 抛出 `UnauthorizedAccessException` |

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

// 手动计算回调地址验证签名
string signature = QQBotWebhookSignature.SignValidation(
    clientSecret,
    plainToken,
    eventTs);
```

## 事件处理方式不变

Webhook 收到的事件仍然通过 `EventDispatcher` 分发，因此原来的事件订阅代码无需修改：

```csharp
var bot = app.Services.GetRequiredService<QQBotClient>();

bot.OnGroupAtMessageCreate += async e =>
{
    await bot.ReplyGroupAsync(e.Message, "收到！");
};

// 控制器模式同样可用
bot.UseControllers();
```

## 安全建议

- 始终校验签名，不要直接信任请求体
- 使用 HTTPS（平台只接受 HTTPS 回调地址）
- 不要将 `ClientSecret` 写进仓库，使用环境变量或 User Secrets
- 回调处理应尽快返回；耗时逻辑建议丢到后台队列

## 下一步

- [ASP.NET Core 集成](/guide/aspnetcore) — Web 项目完整配置
- [Webhook API](/api/webhook) — QQBotWebhook 完整参考
- [事件系统](/guide/events) — 所有可订阅事件
