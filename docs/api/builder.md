# QQBotClientBuilder

`QQBotClientBuilder` 使用 Builder（建造者）模式创建 `QQBotClient` 实例。

**命名空间**: `Luolan.QQBot`

## 方法

所有方法返回 `QQBotClientBuilder` 自身，支持链式调用。

| 方法 | 参数 | 说明 |
|------|------|------|
| `WithAppId` | `string` | 设置 AppId（必填） |
| `WithClientSecret` | `string` | 设置 ClientSecret（必填） |
| `UseSandbox` | `bool` (默认 true) | 是否使用沙箱环境 |
| `WithIntents` | `Intents` | 设置事件订阅（替换式） |
| `AddIntents` | `Intents` | 添加事件订阅（追加式） |
| `WithTokenRefreshBeforeExpire` | `int` 秒 | Token 提前刷新时间（默认 60） |
| `WithWebSocketReconnectInterval` | `int` 毫秒 | 重连间隔（默认 5000） |
| `WithWebSocketMaxReconnectAttempts` | `int` | 最大重连次数（默认 10） |
| `WithLoggerFactory` | `ILoggerFactory` | 日志工厂 |
| `Build` | — | 构建 QQBotClient 实例 |

## 使用示例

```csharp
// 最小配置
var bot = new QQBotClientBuilder()
    .WithAppId("your_app_id")
    .WithClientSecret("your_secret")
    .Build();

// 完整配置
var bot = new QQBotClientBuilder()
    .WithAppId(appId)
    .WithClientSecret(clientSecret)
    .UseSandbox(true)
    .WithIntents(Intents.Default | Intents.GroupAtMessages)
    .WithTokenRefreshBeforeExpire(120)
    .WithWebSocketReconnectInterval(3000)
    .WithWebSocketMaxReconnectAttempts(20)
    .WithLoggerFactory(loggerFactory)
    .Build();
```

### WithIntents vs AddIntents

```csharp
// WithIntents: 完整替换
.WithIntents(Intents.Default)      // 仅 Default

// AddIntents: 在已有基础上追加
.WithIntents(Intents.Default)      // 先设 Default
.AddIntents(Intents.GroupAtMessages) // 再加群消息
// 结果: Default | GroupAtMessages
```

### QQBotClientOptions

配置最终存储在 `QQBotClientOptions` 中：

```csharp
var options = bot.Options;

options.AppId                       // AppId
options.ClientSecret                // ClientSecret
options.IsSandbox                   // 是否沙箱
options.ApiBaseUrl                  // API 基础 URL（自动计算）
options.AuthUrl                     // 认证 URL
options.Intents                     // 事件订阅
options.TokenRefreshBeforeExpireSeconds // Token 刷新提前量
options.WebSocketReconnectIntervalMs    // 重连间隔
options.WebSocketMaxReconnectAttempts   // 最大重连次数
```
