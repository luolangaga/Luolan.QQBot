# Luolan.QQBot

<p align="center">
  <img src="https://img.shields.io/nuget/v/Luolan.QQBot.svg" alt="NuGet">
  <img src="https://img.shields.io/github/license/luolangaga/Luolan.QQBot" alt="License">
</p>

一个简洁、高效、深度集成 .NET 依赖注入体系的 QQ 官方机器人 SDK。旨在提供最符合 .NET 开发者直觉的开发体验。

## 🌟 特性

- 🎮 **MVC 控制器模式** - **[NEW]** 类似 WebAPI 的开发体验，支持命令路由、参数自动解析、直接返回对象。
- ⚡ **极致简单** - 采用 Builder 模式，几行代码即可让机器人上线。
- 🔄 **全自动管理** - Token 自动刷新、WebSocket 自动重连，开发者只需关注业务逻辑。
- 🧪 **深度集成** - 完美支持 ASP.NET Core 依赖注入和 IHostedService。
- 🛡️ **强类型支持** - 完整的 API 模型定义，享受极致的 IDE 智能提示。
- 📡 **事件驱动** - 清晰的事件分发机制，支持频道、群聊、私聊等多种场景。
- 🚀 **性能优化** - **[NEW]** 内置速率限制器、共享 JSON 配置、增强的命令解析。
- 🔧 **类型丰富** - **[NEW]** 支持 bool, enum, Guid, 可空类型等多种参数类型自动转换。

---

## 📦 安装

通过 NuGet 安装核心包：

```bash
dotnet add package Luolan.QQBot
```

---

## 🎮 MVC控制器模式 (推荐)

这是 v1.4.0 引入的全新架构优化模式，允许你像写 WebAPI 一样编写机器人指令。

### 1. 启用控制器

```csharp
using Luolan.QQBot.Extensions;

var bot = new QQBotClientBuilder()
    // ... 基础配置
    .Build();

// 自动扫描并注册当前程序集中的所有控制器
bot.UseControllers();

await bot.StartAsync();
```

### 2. 编写控制器

只需继承 `QQBotController` 并给方法加上 `[Command]` 特性。

```csharp
using Luolan.QQBot.Controllers;
using Luolan.QQBot.Helpers;

public class MyController : QQBotController
{
    // 基础文本命令
    // 用户发送: /hello world
    [Command("hello")]
    public string Hello(string name)
    {
        return $"Hello {name}!";
    }

    // 参数自动转换
    // 用户发送: /add 10 20
    [Command("add")]
    public string Add(int a, int b)
    {
        return $"Result: {a + b}";
    }

    // 返回图片
    // 用户发送: /cat
    [Command("cat")]
    public ImageResult Cat()
    {
        return new ImageResult("https://example.com/cat.jpg");
    }

    // 返回 Markdown
    // 用户发送: /md
    [Command("md")]
    public MessageMarkdown Markdown()
    {
        return MarkdownBuilder.FromContent("# 标题\n**加粗内容**");
    }

    // 异步任务
    [Command("async")]
    public async Task<string> AsyncWork()
    {
        await Task.Delay(1000);
        return "Work done!";
    }
    
    // 访问上下文
    [Command("info")]
    public string Info()
    {
        // 任何方法内都可以访问 User, Message, Client 等属性
        return $"User: {User?.Username}, Channel: {Message.ChannelId}";
    }
    
    // 支持引号参数 (新增)
    // 用户发送: /say "hello world" test
    [Command("say")]
    public string Say(string message, string? target = null)
    {
        return target != null ? $"{target}: {message}" : message;
    }
    
    // 支持枚举类型 (新增)
    [Command("set")]
    public string SetLevel(LogLevel level)
    {
        return $"Level set to: {level}";
    }
    
    // 支持布尔值 (新增)
    // 用户发送: /toggle true  或 /toggle 1  或 /toggle yes
    [Command("toggle")]
    public string Toggle(bool enabled)
    {
        return enabled ? "Enabled" : "Disabled";
    }
}
```

---

## 🚀 经典模式快速开始

如果你更喜欢直接监听事件：

### 1. 控制台模式
```csharp
using Luolan.QQBot;

var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")
    .WithClientSecret("你的ClientSecret")
    .WithIntents(Intents.Default | Intents.GroupAtMessages)
    .Build();

bot.OnAtMessageCreate += async e => await bot.ReplyAsync(e.Message, "收到频道消息！");
bot.OnGroupAtMessageCreate += async e => await bot.ReplyGroupAsync(e.Message, "收到群消息！");

await bot.StartAsync();
await Task.Delay(-1);
```

### 2. ASP.NET Core 集成

**方式一：标准集成**
```csharp
builder.Services.AddQQBot(options => {
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.Default;
});
builder.Services.AddQQBotHostedService();
```

**方式二：IHttpClientFactory 集成 (推荐，性能更优)**
```csharp
using Luolan.QQBot.Extensions;

builder.Services.AddQQBotWithHttpClientFactory(options => {
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.Default;
});
builder.Services.AddQQBotHostedService();
```

---

## 💡 核心概念: Intents

订阅相应的 `Intents` 才能接收到对应类型的事件。

| Intent | 说明 |
| :--- | :--- |
| `Intents.Default` | **公域**推荐。包含 Guilds, Members, AtMessages (频道@) |
| `Intents.GroupAtMessages` | **群聊** @机器人消息事件 |
| `Intents.C2CMessages` | **C2C** (私聊) 消息事件 |
| `Intents.GuildMessages` | **私域**特权。接收频道内所有普通消息 |
| `Intents.PublicGuildMessages` | 公域接收频道 @ 消息 |

---

## 📖 API 详细说明

### 1. 消息发送与回复

```csharp
// 自动解析来源并回复 (支持频道/群/C2C)
await bot.ReplyAsync(sourceMsg, "回复内容");
await bot.ReplyGroupAsync(sourceMsg, "回复群内容");
await bot.ReplyC2CAsync(sourceMsg, "回复私聊内容");

// 主动发送
await bot.SendChannelMessageAsync("channelId", "Hello!");
await bot.SendGroupMessageAsync("groupOpenId", "Hello Group!");
await bot.SendC2CMessageAsync("userOpenId", "Hello Private!");
```

### 2. Markdown 与 互动键盘

```csharp
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Extensions;

// 方式一：Builder (推荐)
var markdown = new MarkdownBuilder().UseContent("# 标题").Build();
var keyboard = new KeyboardBuilder()
    .NewRow().AddButton("btn1", "按钮").Build();

await bot.SendMarkdownAsync(channelId, markdown, keyboard);

// 方式二：扩展方法
await bot.SendMarkdownContentAsync(channelId, "# Hello");

// 方式三：Controller (见上文)
```

### 3. 频道与成员管理
```csharp
// 获取信息
var guilds = await bot.GetGuildsAsync();
var members = await bot.GetMembersAsync(guildId);

// 禁言与管理
await bot.Api.MuteMemberAsync(guildId, userId, 60); 
await bot.Api.CreateGuildRoleAsync(guildId, new() { Name = "新角色" });
```

---

## 📡 事件列表

| 类型 | 事件名 | 说明 |
| :--- | :--- | :--- |
| **消息** | `OnAtMessageCreate` | 频道 @ 机器人 |
| | `OnMessageCreate` | 频道全量消息 (私域) |
| | `OnGroupAtMessageCreate` | 群聊 @ 机器人 |
| | `OnC2CMessageCreate` | C2C 私聊 |
| **管理** | `OnGuildCreate/Delete` | 机器人加入/退出频道 |
| | `OnGuildMemberAdd/Remove` | 成员加入/退出 |
| | `OnGroupAdd/DelRobot` | 机器人进/出群 |
| **互动** | `OnInteractionCreate` | 按钮点击回调 |

---

## 🔧 高级配置

```csharp
var bot = new QQBotClientBuilder()
    .WithAppId("...")
    .WithClientSecret("...")
    .UseSandbox(true)                              // 沙箱环境
    .WithIntents(Intents.Default)                  // 事件订阅
    .WithTokenRefreshBeforeExpire(120)             // Token 提前刷新
    .WithWebSocketReconnectInterval(3000)          // 重连间隔
    .WithWebSocketMaxReconnectAttempts(20)         // 最大尝试次数
    .WithLoggerFactory(loggerFactory)              // 自定义日志
    .Build();
```

---

---

## ⚡ 性能优化

### 内置优化特性

本 SDK 已经内置了多项性能优化，开箱即用：

#### 1. 智能速率限制
自动限制 API 请求频率，防止触发 QQ API 的速率限制（60次/分钟）：
```csharp
// 无需手动配置，SDK 自动处理
await client.SendChannelMessageAsync(channelId, "消息内容");
```

#### 2. 共享 JSON 配置
所有 HTTP 请求共享同一个 `JsonSerializerOptions` 实例，减少内存分配。

#### 3. IHttpClientFactory 支持
使用 `AddQQBotWithHttpClientFactory` 可以更好地管理 HttpClient 生命周期：
```csharp
builder.Services.AddQQBotWithHttpClientFactory(options => { ... });
```

#### 4. 增强的命令解析
支持引号参数，自动处理复杂参数：
```csharp
// 用户输入: /command "hello world" 123 true
[Command("command")]
public string MyCommand(string text, int number, bool flag)
{
    // text = "hello world"
    // number = 123
    // flag = true
    return "OK";
}
```

#### 5. 丰富的类型转换
自动支持以下类型参数：
- 基础类型: `int`, `long`, `double`, `decimal`, `string`
- 布尔值: `bool` (支持 `true/false`, `1/0`, `yes/no`, `on/off`)
- 枚举: `enum` (大小写不敏感)
- Guid: `Guid`
- 可空类型: `int?`, `bool?` 等

---

## 💡 最佳实践

### 1. 使用 IHttpClientFactory
在 ASP.NET Core 项目中，推荐使用 `AddQQBotWithHttpClientFactory`：
```csharp
builder.Services.AddQQBotWithHttpClientFactory(options => { ... });
```

### 2. 合理设置 Intents
只订阅需要的事件类型，避免不必要的网络流量：
```csharp
var bot = new QQBotClientBuilder()
    .WithIntents(Intents.GroupAtMessages | Intents.C2CMessages)  // 只订阅群聊和私聊
    .Build();
```

### 3. 使用控制器模式
控制器模式提供了更好的代码组织和参数解析：
```csharp
public class AdminController : QQBotController
{
    [Command("ban")]
    public async Task<string> Ban(string userId, int duration)
    {
        await Client.Api.MuteMemberAsync(Message.GuildId!, userId, duration);
        return $"已禁言用户 {duration} 秒";
    }
}
```

### 4. 充分利用类型转换
让 SDK 自动处理参数类型转换：
```csharp
[Command("config")]
public string Config(bool enabled, LogLevel level, int? timeout = null)
{
    // 所有参数自动从字符串转换
    return $"Config: {enabled}, {level}, {timeout}";
}
```

---

## 🛡️ License & Feedback

- **License**: MIT
- **Issues**: [Github Issues](https://github.com/luolangaga/Luolan.QQBot/issues)
- **Official Docs**: [QQ 机器人官方文档](https://bot.q.qq.com/wiki/develop/api-v2/)
