# 依赖注入

Luolan.QQBot 完全支持 .NET 的依赖注入（DI）体系。

## 什么是依赖注入？

依赖注入是一种设计模式，它将对象的创建和管理交给容器，而不是手动 `new`。

```csharp
// ❌ 不使用 DI：手动管理依赖
var httpClient = new HttpClient();
var tokenManager = new TokenManager(options, httpClient, logger);
var apiClient = new QQBotHttpClient(options, httpClient, tokenManager, logger);
var bot = new QQBotClient(options, httpClient, tokenManager, apiClient, webSocket, events, logger);

// ✅ 使用 DI：容器自动装配
builder.Services.AddQQBot(options => { ... });
// 所有依赖自动注册和注入
```

## DI 注册方法

### AddQQBot

标准注册方式：

```csharp
using Luolan.QQBot.Extensions;

// 委托方式
builder.Services.AddQQBot(options =>
{
    options.AppId = "...";
    options.ClientSecret = "...";
});

// 参数方式
builder.Services.AddQQBot("appId", "clientSecret", isSandbox: true, Intents.Default);
```

注册的服务（全部为 Singleton）：

| 服务 | 说明 |
|------|------|
| `QQBotClientOptions` | 配置选项 |
| `EventDispatcher` | 事件分发器 |
| `TokenManager` | Token 管理器 |
| `QQBotHttpClient` | HTTP API 客户端 |
| `QQBotWebSocketClient` | WebSocket 客户端 |
| `QQBotClient` | 机器人客户端主类 |

### AddQQBotWithHttpClientFactory

使用 `IHttpClientFactory` 的推荐方式：

```csharp
using Luolan.QQBot.Extensions;

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = "...";
    options.ClientSecret = "...";
});
```

优势：
- HttpClient 生命周期由框架管理
- 自动处理连接池和 Socket 耗尽问题
- 更好的性能和资源利用率

### AddQQBotHostedService

注册 `IHostedService`，自动管理 Bot 的启动和停止：

```csharp
builder.Services.AddQQBotHostedService();
```

## 注入和使用

### 在 Controller 中

```csharp
[ApiController]
public class BotApiController : ControllerBase
{
    private readonly QQBotClient _bot;
    private readonly EventDispatcher _events;

    public BotApiController(QQBotClient bot, EventDispatcher events)
    {
        _bot = bot;
        _events = events;
    }
}
```

### 在托管服务中

```csharp
public class CustomBotService : BackgroundService
{
    private readonly QQBotClient _bot;

    public CustomBotService(QQBotClient bot)
    {
        _bot = bot;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // 使用 _bot...
            await Task.Delay(60000, ct);
        }
    }
}

// 注册
builder.Services.AddHostedService<CustomBotService>();
```

### 在 Minimal API 中

```csharp
app.MapGet("/bot/status", (QQBotClient bot) => new
{
    Connected = bot.IsConnected,
    Username = bot.CurrentUser?.Username,
    GuildCount = bot.GetGuildsAsync().Result.Count
});

app.MapPost("/bot/send", async (QQBotClient bot, SendMessageDto dto) =>
{
    await bot.SendChannelMessageAsync(dto.ChannelId, dto.Content);
    return Results.Ok();
});
```

## 生命周期说明

QQ Bot 的服务都是 **Singleton**（单例），因为：

- Bot 客户端在整个应用生命周期中只需要一个实例
- WebSocket 连接需要持久化
- Token 缓存状态需要共享

```csharp
// 验证生命周期
var bot1 = app.Services.GetRequiredService<QQBotClient>();
var bot2 = app.Services.GetRequiredService<QQBotClient>();
Console.WriteLine(bot1 == bot2); // True — 是同一个实例
```

## 手动创建 vs DI

::: code-group
```csharp [DI 方式（Web 应用）]
// 适合 ASP.NET Core
builder.Services.AddQQBot(options => { ... });
builder.Services.AddQQBotHostedService();
```

```csharp [手动方式（控制台应用）]
// 适合简单的控制台程序
var bot = new QQBotClientBuilder()
    .WithAppId("...")
    .WithClientSecret("...")
    .Build();
await bot.StartAsync();
```
:::

两种方式可以混合使用，例如在控制台中使用 Builder，同时手动创建 `ServiceCollection` 注册其他服务。

## 高级：自定义 DI 注册

如果需要更精细的控制：

```csharp
builder.Services.AddSingleton(sp =>
{
    var options = new QQBotClientOptions
    {
        AppId = "...",
        ClientSecret = "..."
    };

    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("QQBot");

    return new QQBotClient(options, loggerFactory);
});
```

## 下一步

- [ASP.NET Core 集成](/guide/aspnetcore) — Web 应用的完整配置
- [性能优化](/guide/performance) — IHttpClientFactory 的性能优势
