# QQBotClient

`QQBotClient` 是机器人的主入口类，整合了 HTTP API、WebSocket 连接和事件系统。

**命名空间**: `Luolan.QQBot`

## 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Options` | `QQBotClientOptions` | 配置选项 |
| `Api` | `QQBotHttpClient` | HTTP API 客户端 |
| `WebSocket` | `QQBotWebSocketClient` | WebSocket 客户端 |
| `Events` | `EventDispatcher` | 事件分发器 |
| `TokenManager` | `TokenManager` | Token 管理器 |
| `IsConnected` | `bool` | 是否已连接 |
| `CurrentUser` | `User?` | 当前机器人用户信息 |

## 生命周期方法

```csharp
// 启动机器人（连接 WebSocket）
Task StartAsync(CancellationToken ct = default)

// 停止机器人
Task StopAsync()

// 释放资源
void Dispose()
```

## 快捷事件（15个）

所有快捷事件是 `EventDispatcher` 中对应事件的代理。

### 消息事件

```csharp
event Func<ReadyEvent, Task>? OnReady
event Func<MessageCreateEvent, Task>? OnMessageCreate
event Func<AtMessageCreateEvent, Task>? OnAtMessageCreate
event Func<DirectMessageCreateEvent, Task>? OnDirectMessageCreate
event Func<GroupAtMessageCreateEvent, Task>? OnGroupAtMessageCreate
event Func<C2CMessageCreateEvent, Task>? OnC2CMessageCreate
event Func<InteractionCreateEvent, Task>? OnInteractionCreate
```

### 频道和成员事件

```csharp
event Func<GuildCreateEvent, Task>? OnGuildCreate
event Func<GuildDeleteEvent, Task>? OnGuildDelete
event Func<GuildMemberAddEvent, Task>? OnGuildMemberAdd
event Func<GuildMemberRemoveEvent, Task>? OnGuildMemberRemove
```

### 群和好友事件

```csharp
event Func<GroupAddRobotEvent, Task>? OnGroupAddRobot
event Func<GroupDelRobotEvent, Task>? OnGroupDelRobot
event Func<FriendAddEvent, Task>? OnFriendAdd
event Func<FriendDelEvent, Task>? OnFriendDel
```

## 快捷消息方法

```csharp
// 回复消息（自动检测来源：频道/群/C2C）
Task<Message> ReplyAsync(Message source, string content)
Task<SendGroupMessageResponse> ReplyGroupAsync(Message source, string content, int msgSeq = 0)
Task<SendGroupMessageResponse> ReplyC2CAsync(Message source, string content, int msgSeq = 0)

// 主动发送
Task<Message> SendChannelMessageAsync(string channelId, string content, string? msgId = null)
Task<Message> SendChannelMessageAsync(string channelId, SendMessageRequest request)
Task<Message> SendDirectMessageAsync(string userId, string sourceGuildId, string content, string? msgId = null)
Task<SendGroupMessageResponse> SendGroupMessageAsync(string groupOpenId, string content, string? msgId = null, int msgSeq = 0)
Task<SendGroupMessageResponse> SendGroupMessageAsync(string groupOpenId, SendGroupMessageRequest request)
Task<SendGroupMessageResponse> SendC2CMessageAsync(string openId, string content, string? msgId = null, int msgSeq = 0)
Task<SendGroupMessageResponse> SendC2CMessageAsync(string openId, SendGroupMessageRequest request)
```

## 快捷频道方法

```csharp
Task<List<Guild>> GetGuildsAsync()
Task<Guild> GetGuildAsync(string guildId)
Task<List<Channel>> GetChannelsAsync(string guildId)
Task<List<Member>> GetMembersAsync(string guildId, string? after = null, int limit = 100)
```

## 使用示例

```csharp
var bot = new QQBotClientBuilder()
    .WithAppId("...")
    .WithClientSecret("...")
    .Build();

// 注册事件
bot.OnReady += async e =>
{
    Console.WriteLine($"上线: {e.User?.Username}");
};

bot.OnAtMessageCreate += async e =>
{
    // 回复消息
    await bot.ReplyAsync(e.Message, "收到！");

    // 主动发送
    await bot.SendChannelMessageAsync(e.Message.ChannelId!, "主动消息");
};

// 获取频道信息
var guilds = await bot.GetGuildsAsync();
var members = await bot.GetMembersAsync(guilds[0].Id, limit: 50);

// 使用底层 API
var roles = await bot.Api.GetGuildRolesAsync(guilds[0].Id);

// 启动
await bot.StartAsync();
```
