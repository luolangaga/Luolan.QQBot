# 最佳实践

本文档总结了使用 Luolan.QQBot 的推荐实践。

## 1. 使用控制器模式组织代码

将机器人指令分散在事件处理器中会导致代码难以维护。推荐使用控制器模式：

::: code-group
```csharp [推荐：控制器模式]
public class AdminController : QQBotController
{
    [Command("ban")]
    public async Task<string> Ban(string userId, int duration)
    {
        await Client.Api.MuteMemberAsync(Message.GuildId!, userId, duration);
        return $"已禁言用户 {duration} 秒";
    }

    [Command("unban")]
    public async Task<string> Unban(string userId)
    {
        await Client.Api.MuteMemberAsync(Message.GuildId!, userId, 0);
        return $"已解除禁言";
    }
}
```

```csharp [不推荐：事件内处理]
bot.OnAtMessageCreate += async e =>
{
    var content = e.Message.Content?.Trim();
    if (content?.StartsWith("/ban"))
    {
        // 手动解析参数...
        // 处理禁言...
    }
    else if (content?.StartsWith("/unban"))
    {
        // 手动解析参数...
        // 处理解禁...
    }
};
```
:::

## 2. 合理设置 Intents

只订阅你需要的事件，减少不必要的网络流量和资源消耗：

```csharp
// ✅ 只订阅需要的
.WithIntents(Intents.GroupAtMessages | Intents.C2CMessages)

// ❌ 订阅全部
.WithIntents(Intents.PrivateAll | Intents.GroupAtMessages | Intents.C2CMessages | ...)
```

## 3. 保护敏感信息

```csharp
// ✅ 使用环境变量或配置
string appId = Environment.GetEnvironmentVariable("QQBOT_APPID") ?? "";
string clientSecret = Environment.GetEnvironmentVariable("QQBOT_SECRET") ?? "";

// ✅ ASP.NET Core: User Secrets + appsettings.json
builder.Services.AddQQBot(options =>
    builder.Configuration.GetSection("QQBot").Bind(options));

// ❌ 硬编码在代码中（会泄露到 Git）
// string appId = "123456";
// string clientSecret = "abcdef";
```

加入 `.gitignore`：
```
appsettings.Development.json
secrets.json
.env
```

## 4. 善用类型转换

让 SDK 自动处理参数转换，不要手动解析：

```csharp
// ✅ SDK 自动转换
[Command("config")]
public string Config(bool enabled, int timeout, LogLevel level)
{
    // 所有参数自动转换完成
    return $"Enabled: {enabled}, Timeout: {timeout}, Level: {level}";
}

// ❌ 手动解析
[Command("config")]
public string Config(string enabledStr, string timeoutStr, string levelStr)
{
    bool enabled = bool.Parse(enabledStr);       // 可能抛异常
    int timeout = int.Parse(timeoutStr);         // 可能抛异常
    LogLevel level = Enum.Parse<LogLevel>(levelStr); // 可能抛异常
    // ...
}
```

## 5. 正确处理异步

```csharp
// ✅ Controller 支持异步
[Command("fetch")]
public async Task<string> Fetch()
{
    using var http = new HttpClient();
    var data = await http.GetStringAsync("https://api.example.com");
    return $"数据: {data}";
}

// ✅ 事件处理器始终是异步的
bot.OnAtMessageCreate += async e =>
{
    await bot.ReplyAsync(e.Message, "处理中...");
};
```

## 6. 错误处理

```csharp
// 使用合适的异常类型进行处理
try
{
    await bot.Api.MuteMemberAsync(guildId, userId, 60);
}
catch (QQBotApiException ex) when (ex.Code == 403)
{
    // 权限不足
    await ReplyAsync("我没有权限执行此操作");
}
catch (QQBotApiException ex)
{
    // 其他 API 错误
    Console.WriteLine($"API错误: [{ex.Code}] {ex.Message}");
    await ReplyAsync($"操作失败: {ex.Message}");
}
```

## 7. 性能优化

### 使用 IHttpClientFactory

在 ASP.NET Core 中，使用 `AddQQBotWithHttpClientFactory` 代替 `AddQQBot`：

```csharp
// ✅ 推荐：复用 HttpClient
builder.Services.AddQQBotWithHttpClientFactory(options => { ... });

// 次选：标准方式
builder.Services.AddQQBot(options => { ... });
```

### 使用 RateLimiter

SDK 内置了限速器，所有 API 调用自动限速（60次/分钟）。你也可以创建自定义限速器：

```csharp
var limiter = new RateLimiter(capacity: 30, refillRate: 1);

// 在调用外部 API 前检查
if (limiter.TryAcquire("my-service"))
{
    // 执行操作
}
```

## 8. 日志记录

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();      // 控制台输出
    builder.AddDebug();        // 调试窗口输出

    // 开发环境：详细日志
    builder.SetMinimumLevel(LogLevel.Debug);

    // 可以针对特定组件设置级别
    builder.AddFilter("Luolan.QQBot", LogLevel.Debug);   // SDK 详细日志
    builder.AddFilter("System.Net.Http", LogLevel.Warning); // 减少 HTTP 噪音
});
```

## 9. 优雅关闭

```csharp
// 控制台应用
var tcs = new TaskCompletionSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    tcs.SetResult();
};

try
{
    await bot.StartAsync();
    await tcs.Task;
}
finally
{
    await bot.StopAsync(); // 优雅关闭
    bot.Dispose();
}
```

ASP.NET Core 中，`QQBotHostedService` 自动处理优雅关闭。

## 10. 项目结构建议

```
MyQQBot/
├── Program.cs                      ← 入口 + 配置
├── MyQQBot.csproj
│
├── Controllers/                    ← 控制器（按功能模块分）
│   ├── GreetingController.cs       ← 打招呼等
│   ├── AdminController.cs          ← 管理类命令
│   ├── GameController.cs           ← 游戏类命令
│   └── ApiDemoController.cs        ← API 演示
│
├── Services/                       ← 业务逻辑（与 Controller 分离）
│   ├── UserService.cs
│   └── ConfigService.cs
│
└── Models/                         ← 自定义模型
    └── UserData.cs
```

## 常见问题

### Q: 控制器命令和事件处理器可以同时使用吗？

可以。`UseControllers()` 会订阅消息事件来处理命令，你仍然可以注册额外的事件处理器。

### Q: 一个命令可以有多个处理方式吗？

可以为一个方法注册多个 `[Command]` 特性，或者使用别名参数。

### Q: 如何在控制器中访问 DI 服务？

目前控制器是由 `Activator.CreateInstance` 创建的，不经过 DI。如需访问 DI 服务，可以通过 `Client` 属性或使用静态服务定位器模式。

### Q: 沙箱和正式环境的 Intents 不同怎么办？

利用 Builder 的条件配置：

```csharp
var builder = new QQBotClientBuilder();
if (isProduction)
    builder.WithIntents(productionIntents);
else
    builder.WithIntents(devIntents);
```
