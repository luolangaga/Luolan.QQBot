# QQBotWebhook

Webhook（HTTP 回调）接入处理器。

**命名空间**: `Luolan.QQBot.Webhook`

## 构造

```csharp
public QQBotWebhook(
    QQBotClient client,
    string clientSecret,
    ILogger<QQBotWebhook>? logger = null)
```

| 参数 | 说明 |
|------|------|
| `client` | 用于获取 `EventDispatcher` 的机器人客户端 |
| `clientSecret` | 用于 Ed25519 签名校验与回调地址验证 |
| `logger` | 可选日志 |

## 方法

### HandleAsync

```csharp
Task<string> HandleAsync(
    string body,
    string? signature,
    string? timestamp,
    CancellationToken cancellationToken = default)
```

| 参数 | 来源 |
|------|------|
| `body` | HTTP 请求体原文（不要反序列化后再序列化） |
| `signature` | 请求头 `X-Signature-Ed25519` |
| `timestamp` | 请求头 `X-Signature-Timestamp` |

处理流程：

1. 解析 `op`
2. `op=13`：回调地址验证，返回 `plain_token` + `signature`
3. 其他请求：校验 Ed25519 签名
4. `op=0`：按 `t` 字段分发到 `EventDispatcher`
5. 返回 `{"op":12}` ACK

### VerifySignature

```csharp
bool VerifySignature(string body, string? signature, string? timestamp)
```

### SignValidation

```csharp
string SignValidation(string plainToken, string eventTs)
```

## QQBotWebhookSignature

不依赖 `QQBotClient` 的静态签名工具：

```csharp
using Luolan.QQBot.Webhook;

// 校验事件回调
bool ok = QQBotWebhookSignature.Verify(secret, body, signature, timestamp);

// 计算回调地址验证签名
string sig = QQBotWebhookSignature.SignValidation(secret, plainToken, eventTs);
```

## 常量

```csharp
QQBotWebhook.OpValidation   // 13，回调地址验证
QQBotWebhook.OpCallbackAck  // 12，回调 ACK
```

## 依赖注入

```csharp
using Luolan.QQBot.Extensions;

builder.Services.AddQQBotWebhook("你的ClientSecret");
```

等价于：

```csharp
builder.Services.AddSingleton(sp => new QQBotWebhook(
    sp.GetRequiredService<QQBotClient>(),
    "你的ClientSecret",
    sp.GetService<ILogger<QQBotWebhook>>()));
```

## 示例

```csharp
app.MapPost("/api/qqbot/webhook", async (
    HttpContext context,
    QQBotWebhook webhook,
    CancellationToken ct) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync(ct);

    var response = await webhook.HandleAsync(
        body,
        context.Request.Headers["X-Signature-Ed25519"].FirstOrDefault(),
        context.Request.Headers["X-Signature-Timestamp"].FirstOrDefault(),
        ct);

    return Results.Content(response, "application/json");
});
```
