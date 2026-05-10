# ASP.NET Core 集成

Luolan.QQBot 深度集成 ASP.NET Core，支持依赖注入和托管服务生命周期。

## 添加服务

### 方式一：标准注册

```csharp
// Program.cs
using Luolan.QQBot;
using Luolan.QQBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 注册 QQ Bot 服务（委托方式）
builder.Services.AddQQBot(options =>
{
    options.AppId = builder.Configuration["QQBot:AppId"]!;
    options.ClientSecret = builder.Configuration["QQBot:ClientSecret"]!;
    options.IsSandbox = true;
    options.Intents = Intents.Default | Intents.GroupAtMessages;
});

// 注册 QQ Bot 服务（参数方式）
builder.Services.AddQQBot(
    appId: "你的AppId",
    clientSecret: "你的ClientSecret",
    isSandbox: true,
    intents: Intents.Default
);

// 注册托管服务 —— 自动管理启动和停止
builder.Services.AddQQBotHostedService();

var app = builder.Build();
app.Run();
```

### 方式二：IHttpClientFactory 集成（推荐）

使用 `IHttpClientFactory` 管理 HttpClient 生命周期，性能更优：

```csharp
using Luolan.QQBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = builder.Configuration["QQBot:AppId"]!;
    options.ClientSecret = builder.Configuration["QQBot:ClientSecret"]!;
    options.Intents = Intents.Default;
});

builder.Services.AddQQBotHostedService();
```

## 获取 Bot 实例

注册完成后，可以在任何通过 DI 获取的地方使用：

```csharp
// 在 Controller 中
[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    private readonly QQBotClient _bot;

    public BotController(QQBotClient bot)
    {
        _bot = bot;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            IsConnected = _bot.IsConnected,
            Username = _bot.CurrentUser?.Username,
            Id = _bot.CurrentUser?.Id
        });
    }
}
```

## 配置文件管理

### appsettings.json

```json
{
  "QQBot": {
    "AppId": "你的AppId",
    "ClientSecret": "你的ClientSecret",
    "IsSandbox": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Luolan.QQBot": "Debug"
    }
  }
}
```

### 使用配置

```csharp
builder.Services.AddQQBot(options =>
{
    builder.Configuration.GetSection("QQBot").Bind(options);
    // 或手动读取
    options.AppId = builder.Configuration["QQBot:AppId"]!;
    options.ClientSecret = builder.Configuration["QQBot:ClientSecret"]!;
});
```

::: tip 敏感信息处理
使用 `dotnet user-secrets` 存储 AppId 和 ClientSecret：

```bash
dotnet user-secrets set "QQBot:AppId" "你的AppId"
dotnet user-secrets set "QQBot:ClientSecret" "你的ClientSecret"
```

在开发环境中自动加载 User Secrets（Development 模式下自动启用）。
:::

## 在服务中使用 Bot

```csharp
// 自定义后台服务
public class BotNotificationService : BackgroundService
{
    private readonly QQBotClient _bot;
    private readonly ILogger<BotNotificationService> _logger;

    public BotNotificationService(QQBotClient bot, ILogger<BotNotificationService> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 等待 Bot 连接
        while (!_bot.IsConnected && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        // 定时任务
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("执行定时通知...");
            // await _bot.SendChannelMessageAsync("channelId", "定时通知");
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}

// 注册
builder.Services.AddHostedService<BotNotificationService>();
```

## 启用控制器模式

在 ASP.NET Core 中也可以使用控制器模式：

```csharp
// 在 Program.cs 中，获取 bot 实例后启用
var app = builder.Build();

// 在应用启动时配置控制器
var bot = app.Services.GetRequiredService<QQBotClient>();
bot.UseControllers(); // 扫描所有控制器
```

## 健康检查

```csharp
// 注册健康检查
builder.Services.AddHealthChecks()
    .AddCheck<QQBotHealthCheck>("qq_bot");

public class QQBotHealthCheck : IHealthCheck
{
    private readonly QQBotClient _bot;

    public QQBotHealthCheck(QQBotClient bot) => _bot = bot;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        if (_bot.IsConnected)
            return Task.FromResult(HealthCheckResult.Healthy("QQ Bot 已连接"));

        return Task.FromResult(HealthCheckResult.Unhealthy("QQ Bot 未连接"));
    }
}

// 映射端点
app.MapHealthChecks("/health");
```

## Swagger API 示例

结合控制器模式，可以同时提供 REST API 和 Bot 功能：

```csharp
// 一个完整的 ASP.NET Core 示例
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册 Bot
builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    builder.Configuration.GetSection("QQBot").Bind(options);
});
builder.Services.AddQQBotHostedService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用控制器模式
app.Services.GetRequiredService<QQBotClient>().UseControllers();

app.MapControllers();
app.Run();
```

## 完整项目结构

```
MyBot.Web/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json  (不提交到 Git)
├── MyBot.Web.csproj
│
├── Controllers/
│   ├── BotApiController.cs       ← REST API
│   ├── HelloBotController.cs     ← Bot 命令（: QQBotController）
│   └── AdminBotController.cs     ← Bot 管理命令
│
├── Services/
│   ├── BotNotificationService.cs ← 后台任务
│   └── BotHealthCheck.cs         ← 健康检查
│
└── Models/
    └── BotStatusDto.cs           ← 数据传输对象
```

## 下一步

- [依赖注入](/guide/dependency-injection) — 深入理解 DI 机制
- [最佳实践](/guide/best-practices) — 生产环境建议
