# Luolan.QQBot

一个简洁、高效、符合 .NET 哲学的 QQ 官方机器人 SDK。

## ✨ 特性

- 🚀 **简单易用** - 符合 .NET 开发习惯，上手即用
- 🔄 **自动Token刷新** - 无需手动管理Token生命周期
- 🌐 **完整API支持** - 支持所有官方HTTP API
- 📡 **WebSocket事件** - 实时接收消息和事件
- 🔌 **依赖注入** - 完美集成 ASP.NET Core
- 🛡️ **自动重连** - WebSocket断线自动重连
- 📝 **强类型** - 完整的类型定义和智能提示

## 📦 安装

```bash
dotnet add package Luolan.QQBot
```

## 🚀 快速开始

### 基础使用

```csharp
using Luolan.QQBot;

// 使用Builder模式创建客户端
var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")
    .WithClientSecret("你的ClientSecret")
    .WithIntents(Intents.Default | Intents.GroupAtMessages)
    .Build();

// 监听@机器人消息(频道)
bot.OnAtMessageCreate += async e =>
{
    Console.WriteLine($"收到消息: {e.Message.Content}");
    await bot.ReplyAsync(e.Message, $"你好! 你说的是: {e.Message.Content}");
};

// 监听群@机器人消息
bot.OnGroupAtMessageCreate += async e =>
{
    Console.WriteLine($"[群消息] {e.Message.Content}");
    await bot.ReplyGroupAsync(e.Message, "收到群消息!");
};

// 监听C2C消息(私聊)
bot.OnC2CMessageCreate += async e =>
{
    Console.WriteLine($"[私聊] {e.Message.Content}");
    await bot.ReplyC2CAsync(e.Message, "收到私聊消息!");
};

// 启动机器人
await bot.StartAsync();

// 保持运行
Console.WriteLine("机器人已启动, 按Ctrl+C退出");
await Task.Delay(-1);
```

### ASP.NET Core 集成

```csharp
// Program.cs
using Luolan.QQBot;
using Luolan.QQBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加QQ机器人服务
builder.Services.AddQQBot(options =>
{
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.Default | Intents.GroupAtMessages;
});

// 添加托管服务(自动启动机器人)
builder.Services.AddQQBotHostedService();

var app = builder.Build();

// 配置事件处理
var bot = app.Services.GetRequiredService<QQBotClient>();
bot.OnAtMessageCreate += async e =>
{
    await bot.ReplyAsync(e.Message, "Hello from ASP.NET Core!");
};

app.Run();
```

## 📖 API 文档

### 消息发送

```csharp
// 发送频道消息
await bot.SendChannelMessageAsync(channelId, "Hello!");

// 发送带图片的消息
await bot.Api.SendMessageAsync(channelId, new SendMessageRequest
{
    Content = "看这张图片",
    Image = "https://example.com/image.png"
});

// 发送私信
await bot.SendDirectMessageAsync(userId, sourceGuildId, "私信内容");

// 发送群消息
await bot.SendGroupMessageAsync(groupOpenId, "群消息内容");

// 发送C2C消息
await bot.SendC2CMessageAsync(userOpenId, "私聊消息");

// 发送Markdown消息
await bot.Api.SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
{
    MsgType = 2,
    Markdown = new MessageMarkdown
    {
        Content = "# 标题\n这是**Markdown**内容"
    }
});

// 发送带按钮的消息
await bot.Api.SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
{
    MsgType = 2,
    Markdown = new MessageMarkdown { Content = "请选择:" },
    Keyboard = new MessageKeyboard
    {
        Content = new InlineKeyboard
        {
            Rows = new List<InlineKeyboardRow>
            {
                new InlineKeyboardRow
                {
                    Buttons = new List<Button>
                    {
                        new Button
                        {
                            RenderData = new ButtonRenderData { Label = "按钮1" },
                            Action = new ButtonAction { Type = 2, Data = "/cmd1" }
                        }
                    }
                }
            }
        }
    }
});
```

### 频道管理

```csharp
// 获取机器人加入的频道列表
var guilds = await bot.GetGuildsAsync();

// 获取子频道列表
var channels = await bot.GetChannelsAsync(guildId);

// 获取成员列表
var members = await bot.GetMembersAsync(guildId);

// 禁言成员
await bot.Api.MuteMemberAsync(guildId, userId, 60); // 禁言60秒

// 创建角色
await bot.Api.CreateGuildRoleAsync(guildId, new CreateRoleRequest
{
    Name = "新角色",
    Color = 0xFF0000
});
```

### 事件处理

```csharp
// 频道事件
bot.Events.OnGuildCreate += async e => { /* 加入新频道 */ };
bot.Events.OnGuildDelete += async e => { /* 被移出频道 */ };
bot.Events.OnChannelCreate += async e => { /* 子频道创建 */ };

// 成员事件
bot.Events.OnGuildMemberAdd += async e => { /* 新成员加入 */ };
bot.Events.OnGuildMemberRemove += async e => { /* 成员离开 */ };

// 消息事件
bot.Events.OnMessageCreate += async e => { /* 频道消息(私域) */ };
bot.Events.OnAtMessageCreate += async e => { /* @机器人消息(公域) */ };
bot.Events.OnDirectMessageCreate += async e => { /* 私信 */ };
bot.Events.OnGroupAtMessageCreate += async e => { /* 群@消息 */ };
bot.Events.OnC2CMessageCreate += async e => { /* C2C消息 */ };

// 互动事件
bot.Events.OnInteractionCreate += async e =>
{
    // 按钮点击回调
    var buttonId = e.Interaction.Data?.Resolved?.ButtonId;
    var buttonData = e.Interaction.Data?.Resolved?.ButtonData;
};

// 机器人管理事件
bot.Events.OnGroupAddRobot += async e => { /* 被添加到群 */ };
bot.Events.OnGroupDelRobot += async e => { /* 被移出群 */ };
bot.Events.OnFriendAdd += async e => { /* 被添加为好友 */ };
bot.Events.OnFriendDel += async e => { /* 被删除好友 */ };

// 消息审核事件
bot.Events.OnMessageAuditPass += async e => { /* 消息审核通过 */ };
bot.Events.OnMessageAuditReject += async e => { /* 消息审核不通过 */ };

// 原始事件(处理所有事件)
bot.Events.OnRawEvent += async (eventType, data) =>
{
    Console.WriteLine($"原始事件: {eventType}");
};
```

### Intents 配置

```csharp
// 公域机器人(推荐)
Intents.Default

// 私域机器人(需要申请)
Intents.PrivateAll

// 自定义组合
Intents.Guilds | Intents.GuildMembers | Intents.PublicGuildMessages | Intents.GroupAtMessages

// 可用的Intents:
// Guilds - 频道事件
// GuildMembers - 成员事件
// GuildMessages - 频道消息(私域)
// GuildMessageReactions - 表情表态
// DirectMessage - 私信
// MessageAudit - 消息审核
// Forums - 论坛(私域)
// AudioAction - 音频
// PublicGuildMessages - 公域消息
// Interaction - 互动事件
// GroupAtMessages - 群@消息
// C2CMessages - C2C消息
```

## 🔧 高级配置

```csharp
var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")
    .WithClientSecret("你的ClientSecret")
    .UseSandbox(true)                              // 使用沙箱环境
    .WithIntents(Intents.Default)                  // 订阅事件
    .WithTokenRefreshBeforeExpire(120)             // Token提前120秒刷新
    .WithWebSocketReconnectInterval(3000)          // 重连间隔3秒
    .WithWebSocketMaxReconnectAttempts(20)         // 最大重连20次
    .WithLoggerFactory(loggerFactory)              // 自定义日志
    .Build();
```

## 📋 完整示例

```csharp
using Luolan.QQBot;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;

// 创建日志工厂
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// 创建机器人
var bot = new QQBotClientBuilder()
    .WithAppId(Environment.GetEnvironmentVariable("QQ_BOT_APP_ID")!)
    .WithClientSecret(Environment.GetEnvironmentVariable("QQ_BOT_CLIENT_SECRET")!)
    .WithIntents(Intents.Default | Intents.GroupAtMessages)
    .WithLoggerFactory(loggerFactory)
    .Build();

// 就绪事件
bot.OnReady += async e =>
{
    Console.WriteLine($"机器人已上线: {e.User?.Username}");
};

// 频道@消息
bot.OnAtMessageCreate += async e =>
{
    var content = e.Message.Content?.Trim() ?? "";
    
    if (content.Contains("帮助"))
    {
        await bot.ReplyAsync(e.Message, "可用命令:\n/ping - 测试\n/info - 信息");
    }
    else if (content.Contains("ping"))
    {
        await bot.ReplyAsync(e.Message, "pong!");
    }
    else
    {
        await bot.ReplyAsync(e.Message, $"你说: {content}");
    }
};

// 群消息
bot.OnGroupAtMessageCreate += async e =>
{
    var content = e.Message.Content ?? "";
    await bot.ReplyGroupAsync(e.Message, $"收到: {content}");
};

// 按钮回调
bot.OnInteractionCreate += async e =>
{
    var data = e.Interaction.Data?.Resolved?.ButtonData;
    Console.WriteLine($"按钮点击: {data}");
};

// 启动
await bot.StartAsync();
Console.WriteLine("按任意键退出...");
Console.ReadKey();
await bot.StopAsync();
```

## 📄 License

MIT License

## 🔗 相关链接

- [QQ机器人官方文档](https://bot.q.qq.com/wiki/develop/api-v2/)
- [QQ开放平台](https://q.qq.com/)
