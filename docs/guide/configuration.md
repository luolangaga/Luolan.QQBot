# 项目配置

Luolan.QQBot 使用 Builder 模式进行配置，所有选项都有合理的默认值。

## QQBotClientBuilder 完整配置

```csharp
var bot = new QQBotClientBuilder()
    // --- 必填配置 ---
    .WithAppId("你的AppId")              // 从QQ开放平台获取
    .WithClientSecret("你的ClientSecret") // 从QQ开放平台获取

    // --- 环境设置 ---
    .UseSandbox(true)                    // true=沙箱环境, false=正式环境

    // --- Intents 事件订阅 ---
    .WithIntents(                        // 设置完整的 Intents
        Intents.Default                  // 公域默认事件
        | Intents.GroupAtMessages        // 群@消息
        | Intents.C2CMessages            // C2C私聊
    )
    // 或者增量添加
    .AddIntents(Intents.Interaction)     // 额外添加互动事件

    // --- Token 管理 ---
    .WithTokenRefreshBeforeExpire(120)   // 过期前120秒自动刷新

    // --- WebSocket 重连 ---
    .WithWebSocketReconnectInterval(3000)   // 重连间隔(ms)
    .WithWebSocketMaxReconnectAttempts(20)  // 最大重连次数

    // --- 日志 ---
    .WithLoggerFactory(loggerFactory)    // 自定义日志工厂

    .Build();
```

## 配置项详解

### AppId 和 ClientSecret

**必填。** 在 [QQ 开放平台](https://q.qq.com) 创建机器人后获取。

::: danger 安全提示
不要将 AppId 和 ClientSecret 提交到 Git 仓库！使用环境变量或 User Secrets。
:::

```csharp
// 方式一：环境变量（推荐）
string appId = Environment.GetEnvironmentVariable("QQBOT_APPID") ?? "";
string clientSecret = Environment.GetEnvironmentVariable("QQBOT_SECRET") ?? "";

// 方式二：ASP.NET Core User Secrets
// builder.Configuration["QQBot:AppId"]

// 方式三：配置文件 + 不提交
// 将 appsettings.Development.json 加入 .gitignore
```

### 环境选择（UseSandbox）

- `true`（沙箱环境）— 开发和测试使用，不影响线上数据
- `false`（正式环境）— 生产环境

沙箱环境和正式环境的 API 地址不同，SDK 自动切换。

### Intents 事件订阅

`Intents` 决定你的机器人能接收到哪些事件。只订阅需要的可以节省资源：

```csharp
// 最小配置：只收群消息和私聊
.WithIntents(Intents.GroupAtMessages | Intents.C2CMessages)

// 公域机器人推荐配置
.WithIntents(Intents.Default | Intents.GroupAtMessages)

// 私域机器人完整配置
.WithIntents(Intents.PrivateAll | Intents.GroupAtMessages | Intents.C2CMessages)
```

### Token 刷新

```csharp
// Token 会在到期前自动刷新
// 默认提前 60 秒刷新
.WithTokenRefreshBeforeExpire(120)  // 提前 120 秒刷新
```

### WebSocket 重连

```csharp
// 断线自动重连，使用指数退避策略
.WithWebSocketReconnectInterval(3000)   // 首次重连等待 3 秒
.WithWebSocketMaxReconnectAttempts(20)  // 最多尝试 20 次
```

### 日志

```csharp
// 创建日志工厂
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();              // 输出到控制台
    builder.SetMinimumLevel(LogLevel.Information); // 设置日志级别
});

// 传给 Bot
.WithLoggerFactory(loggerFactory)
```

## QQBotClientOptions 属性

如果需要编程方式访问配置：

```csharp
var options = bot.Options;

Console.WriteLine($"AppId: {options.AppId}");
Console.WriteLine($"沙箱: {options.IsSandbox}");
Console.WriteLine($"API地址: {options.ApiBaseUrl}");
Console.WriteLine($"Intents: {options.Intents}");
Console.WriteLine($"重连间隔: {options.WebSocketReconnectIntervalMs}ms");
Console.WriteLine($"最大重连: {options.WebSocketMaxReconnectAttempts}次");
```

## 运行时信息

```csharp
// 连接状态
bool isConnected = bot.IsConnected;

// 当前用户
User? currentUser = bot.CurrentUser;
Console.WriteLine($"用户名: {currentUser?.Username}");

// Token 管理器
var token = await bot.TokenManager.GetAccessTokenAsync();

// WebSocket 会话
var session = bot.WebSocket.Session;
Console.WriteLine($"SessionId: {session.SessionId}");
```

## 下一步

- [ASP.NET Core 集成](/guide/aspnetcore) — 在 Web 项目中配置机器人
- [最佳实践](/guide/best-practices) — 推荐的配置方式
