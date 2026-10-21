# Luolan.QQBot

<p align="center">
  <img src="https://img.shields.io/nuget/v/Luolan.QQBot.svg" alt="NuGet">
  <img src="https://img.shields.io/github/license/luolangaga/Luolan.QQBot" alt="License">
</p>

一个简洁、高效、深度集成 .NET 依赖注入体系的 QQ 官方机器人 SDK。旨在提供最符合 .NET 开发者直觉的开发体验。

> **v1.5.1** 已适配官方新域名 `api.bot.qq.com`，新增 Webhook 接入、群管理 API、单聊流式消息、分片上传、自定义菜单/指令面板等能力。

## 🌟 特性

- 🎮 **MVC 控制器模式** - 类似 WebAPI 的开发体验，支持命令路由、参数自动解析、直接返回对象，控制器支持依赖注入构造。
- ⚡ **极致简单** - 采用 Builder 模式，几行代码即可让机器人上线。
- 🔄 **全自动管理** - Token 自动刷新、WebSocket 自动重连，开发者只需关注业务逻辑。
- 🧪 **深度集成** - 完美支持 ASP.NET Core 依赖注入和 IHostedService。
- 🛡️ **强类型支持** - 完整的 API 模型定义，享受极致的 IDE 智能提示。
- 📡 **事件驱动** - 清晰的事件分发机制，支持频道、群聊、私聊等多种场景。
- 🌐 **双接入方式** - WebSocket / Webhook 作为 `QQBotClientOptions.Mode` 模式切换，业务事件和控制器代码无需修改；内置 Ed25519 签名校验。
- 👥 **群管理能力** - 群信息、群成员、入群审批、黑名单、禁言等管理接口。
- 🚀 **性能优化** - 内置速率限制器、共享 JSON 配置、增强的命令解析。
- 🔧 **类型丰富** - 支持 bool, enum, Guid, 可空类型等多种参数类型自动转换。

---

## 📦 安装

```bash
dotnet add package Luolan.QQBot
```

---

## 🎮 MVC控制器模式 (推荐)

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

在 ASP.NET Core 中，控制器会自动从 DI 容器创建，可注入任意注册服务：

```csharp
public class MyController : QQBotController
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger) => _logger = logger;

    [Command("hello")]
    public string Hello(string name) => $"Hello {name}!";
}

// Program.cs
app.Services.GetRequiredService<QQBotClient>().UseControllers();
```

### 2. 编写控制器

只需继承 `QQBotController` 并给方法加上 `[Command]` 特性。

```csharp
public class MyController : QQBotController
{
    [Command("hello")]
    public string Hello(string name) => $"Hello {name}!";

    // 参数自动转换
    [Command("add")]
    public string Add(int a, int b) => $"Result: {a + b}";

    // 返回图片
    [Command("cat")]
    public ImageResult Cat() => new("https://example.com/cat.jpg");

    // 返回 Markdown
    [Command("md")]
    public MessageMarkdown Markdown() => MarkdownBuilder.FromContent("# 标题\n**加粗内容**");

    // 异步任务
    [Command("async")]
    public async Task<string> AsyncWork()
    {
        await Task.Delay(1000);
        return "Work done!";
    }

    // 支持引号参数: /say "hello world" test
    [Command("say")]
    public string Say(string message, string? target = null)
        => target != null ? $"{target}: {message}" : message;

    // 枚举 / 布尔 / Guid / 可空类型自动转换
    [Command("set")]
    public string SetLevel(LogLevel level, bool enabled, int? timeout = null)
        => $"level={level}, enabled={enabled}, timeout={timeout}";
}
```

---

## 🚀 经典模式快速开始

### 1. 控制台模式
```csharp
using Luolan.QQBot;

var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")
    .WithClientSecret("你的ClientSecret")
    .WithIntents(Intents.Default | Intents.GroupAndC2C)
    .Build();

bot.OnGroupAtMessageCreate += async e => await bot.ReplyGroupAsync(e.Message, "收到群消息！");
bot.OnGroupMessageCreate += async e => Console.WriteLine($"[群全量] {e.Message.Content}");

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
builder.Services.AddQQBotWithHttpClientFactory(options => {
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.Default | Intents.GroupAndC2C;
});
builder.Services.AddQQBotHostedService();
```

### 3. WebSocket / Webhook 模式切换 (v1.5.1)

Webhook 是 `QQBotClientOptions.Mode` 的一种传输模式。切换模式只改配置，**事件订阅、控制器和业务代码都不需要修改**。

安装 ASP.NET Core 集成包：

```bash
dotnet add package Luolan.QQBot.AspNetCore
```

```csharp
using Luolan.QQBot;
using Luolan.QQBot.AspNetCore;
using Luolan.QQBot.Extensions;

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.Default | Intents.GroupAndC2C;

    // 只改这一处：WebSocket（默认）或 Webhook
    options.Mode = QQBotConnectionMode.Webhook;
    options.WebhookPath = "/api/qqbot/webhook";
});

// 两种模式都保留，不用删
builder.Services.AddQQBotHostedService();

var app = builder.Build();

// 两种模式都保留：WebSocket 模式下这行会自动跳过
app.MapQQBotWebhook();

// 事件订阅 / 控制器代码完全不变
app.Services.GetRequiredService<QQBotClient>().UseControllers();
app.Run();
```

管理端把回调地址配置为 `https://你的域名/api/qqbot/webhook`（端口仅支持 80/443/8080/8443）。

`MapQQBotWebhook()` 会自动处理回调地址验证 (OpCode 13)、Ed25519 签名校验、事件分发和 ACK (OpCode 12)。

---

## 💡 核心概念: Intents

订阅相应的 `Intents` 才能接收到对应的事件。

| Intent | 说明 |
| :--- | :--- |
| `Intents.Default` | **公域**推荐。包含 Guilds, Members, AtMessages (频道@) |
| `Intents.GroupAndC2C` | `1<<25`，群@消息、全量群消息、C2C 私聊、好友事件 |
| `Intents.GroupMembers` | `1<<24`，群成员加入/退出、入群申请 |
| `Intents.C2CMessages` | `1<<25`，等同 `GroupAndC2C` |
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

// Markdown + 键盘
await bot.SendGroupMarkdownAsync(groupOpenId, markdown, keyboard, msgId: msg.Id);
await bot.SendC2CMarkdownAsync(userOpenId, markdown, keyboard, msgId: msg.Id);

// 富媒体(自动上传并发送)
await bot.SendGroupMediaAsync(groupOpenId, "https://example.com/a.png", fileType: 1);
await bot.SendC2CMediaAsync(userOpenId, "https://example.com/a.png", fileType: 1);

// 撤回
await bot.RevokeGroupMessageAsync(groupOpenId, messageId);
await bot.RevokeC2CMessageAsync(userOpenId, messageId);
```

### 2. 单聊流式消息 (v1.5.1 新增)

```csharp
var first = await bot.SendC2CStreamMessageAsync(openId, new SendC2CStreamMessageRequest
{
    InputState = 1,           // 1=生成中 10=生成结束
    Index = 0,
    ContentType = "markdown",
    ContentRaw = "正在生成回答…",
    MsgId = sourceMsg.Id,
    MsgSeq = 1
});

await bot.SendC2CStreamMessageAsync(openId, new SendC2CStreamMessageRequest
{
    InputState = 10,
    Index = 1,
    ContentType = "markdown",
    ContentRaw = "正在生成回答…完成！",
    MsgId = sourceMsg.Id,
    MsgSeq = 1,
    StreamMsgId = first.Id
});
```

### 3. 群管理 (v1.5.1 新增)

```csharp
var info = await bot.GetGroupInfoAsync(groupOpenId);
var state = await bot.GetGroupBotStateAsync(groupOpenId);
var members = await bot.GetGroupMembersAsync(groupOpenId);          // 需内邀权限
var member = await bot.GetGroupMemberAsync(groupOpenId, memberOpenId);

// 入群申请
var requests = await bot.Api.GetGroupJoinRequestsAsync(groupOpenId);
await bot.ApproveGroupJoinRequestAsync(groupOpenId, memberOpenId, new ApproveJoinRequestRequest
{
    Op = "approve",                 // approve / decline
    JoinRequestId = requestId,
    RejectReason = null,
    AddToMemberBlacklist = false
});

// 黑名单
await bot.Api.UpdateGroupBlacklistAsync(groupOpenId, new GroupBlacklistRequest
{
    Op = "add",                     // add / del
    MemberOpenIds = new() { memberOpenId }
});

// 禁言
await bot.SetGroupMemberMuteAsync(groupOpenId, new SetGroupMemberMuteRequest
{
    Members = new()
    {
        new SetMemberMuteState
        {
            Op = "add",             // add / update / del
            MemberOpenId = memberOpenId,
            MuteExpireAt = DateTimeOffset.Now.AddHours(1).ToString("O")
        }
    }
});
```

### 4. 自定义菜单、指令面板与互动响应 (v1.5.1 新增)

```csharp
// 全局自定义菜单(仅 C2C)
await bot.Api.UpdateMenuAsync(new QQBotMenu
{
    Items = new()
    {
        new MenuItem { Type = "send_message", Name = "帮助", SendMessage = "/help" },
        new MenuItem { Type = "link", Name = "官网", Link = "https://example.com" }
    }
});

// 指令面板
var panel = await bot.Api.CreatePanelAsync(new CreatePanelRequest
{
    Scope = "group",
    TargetType = "all",
    Panel = new Panel
    {
        Items = new()
        {
            new PanelItem { Type = "command", Name = "签到", Desc = "每日签到" }
        }
    }
});

// 回应按钮互动，避免客户端一直 loading
await bot.Api.RespondInteractionAsync(interactionId, code: 0);

// 生成邀请分享链接
var share = await bot.Api.GenerateUrlLinkAsync("callback_data");
```

### 5. Markdown 与 互动键盘

```csharp
using Luolan.QQBot.Helpers;

var markdown = new MarkdownBuilder().UseContent("# 标题").Build();
var keyboard = new KeyboardBuilder()
    .NewRow().AddButton("btn1", "按钮").Build();

await bot.SendMarkdownAsync(channelId, markdown, keyboard);
await bot.SendMarkdownContentAsync(channelId, "# Hello");
```

### 6. 频道与成员管理
```csharp
var guilds = await bot.GetGuildsAsync();
var members = await bot.GetMembersAsync(guildId);

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
| | `OnGroupMessageCreate` | 群聊全量消息 (v1.5.1) |
| | `OnC2CMessageCreate` | C2C 私聊 |
| | `OnDirectMessageCreate` | 频道私信 |
| **群成员** | `OnGroupMemberAdd/Remove` | 群成员加入/退出 (v1.5.1) |
| | `OnGroupJoinRequest` | 用户申请加群 (v1.5.1) |
| **管理** | `OnGuildCreate/Delete` | 机器人加入/退出频道 |
| | `OnGuildMemberAdd/Remove` | 成员加入/退出 |
| | `OnGroupAdd/DelRobot` | 机器人进/出群 |
| **好友** | `OnFriendAdd/OnFriendDel` | 好友添加/删除 |
| **互动** | `OnInteractionCreate` | 按钮点击回调 |
| | `OnSubscribeMessageStatus` | 订阅消息授权状态 (v1.5.1) |
| **其他** | `OnRawEvent` | 所有原始事件 |

---

## 🔧 高级配置

```csharp
var bot = new QQBotClientBuilder()
    .WithAppId("...")
    .WithClientSecret("...")
    .UseSandbox(true)                              // 沙箱环境
    .UseWebhook("/api/qqbot/webhook")              // 切换为 Webhook 模式(默认 WebSocket)
    .WithIntents(Intents.Default | Intents.GroupAndC2C)
    .WithTokenRefreshBeforeExpire(120)             // Token 提前刷新
    .WithWebSocketReconnectInterval(3000)          // 重连间隔
    .WithWebSocketMaxReconnectAttempts(20)         // 最大尝试次数
    .WithLoggerFactory(loggerFactory)              // 自定义日志
    .Build();
```

自定义服务器地址（例如代理或私有网关）：

```csharp
var options = new QQBotClientOptions
{
    AppId = "...",
    ClientSecret = "...",
    ApiBaseUrlOverride = "https://api.bot.qq.com",
    AuthUrlOverride = "https://api.bot.qq.com/app/getAppAccessToken"
};
```

---

## ⚡ 性能优化

- **智能速率限制** - 默认令牌桶 100 容量 / 20 QPS 补充，可通过 `RateLimitCapacity`、`RateLimitRefillPerSecond` 调整。
- **共享 JSON 配置** - 所有 HTTP 请求共享 `JsonSerializerOptions` 实例。
- **IHttpClientFactory 支持** - `AddQQBotWithHttpClientFactory` 管理 HttpClient 生命周期。
- **增强的命令解析** - 支持引号参数、bool/enum/Guid/可空类型转换。
- **富媒体分片上传** - 支持 `upload_prepare` + `upload_part_finish` 大文件上传流程。

---

## 🛡️ License & Feedback

- **License**: MIT
- **Issues**: [Github Issues](https://github.com/luolangaga/Luolan.QQBot/issues)
- **Official Docs**: [QQ 机器人官方文档](https://bot.q.qq.com/wiki/develop/api-v2/)
