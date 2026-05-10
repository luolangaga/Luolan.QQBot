# 事件系统

Luolan.QQBot 提供了一套完整的事件驱动架构。通过订阅不同的事件，你可以处理 QQ 频道的各种情况。

## 事件订阅方式

有两种方式订阅事件：

### 方式一：快捷属性（推荐）

在 `QQBotClient` 实例上直接订阅：

```csharp
bot.OnAtMessageCreate += async e =>
{
    await bot.ReplyAsync(e.Message, "收到！");
};
```

### 方式二：EventDispatcher

通过 `bot.Events` 访问完整的事件分发器：

```csharp
bot.Events.OnAtMessageCreate += async e =>
{
    // 处理逻辑
};
```

两种方式效果完全相同。快捷属性只是 `EventDispatcher` 的代理。

## 事件分类

### 消息事件

| 事件 | 触发时机 | 需要 Intents |
|------|---------|-------------|
| `OnAtMessageCreate` | 频道中 @机器人（公域） | `PublicGuildMessages` |
| `OnMessageCreate` | 频道中任意消息（私域） | `GuildMessages` |
| `OnDirectMessageCreate` | 频道私信 | `DirectMessage` |
| `OnGroupAtMessageCreate` | 群聊中 @机器人 | `GroupAtMessages` |
| `OnC2CMessageCreate` | C2C 私聊 | `C2CMessages` |

```csharp
// 公域 @消息
bot.OnAtMessageCreate += async e =>
{
    var msg = e.Message;
    var content = msg.Content;     // 消息内容
    var author = msg.Author;       // 发送者（User 对象）
    var channelId = msg.ChannelId; // 子频道 ID
    var guildId = msg.GuildId;     // 频道 ID

    if (content?.Trim() == "你好")
        await bot.ReplyAsync(msg, "你好呀！");
};

// 群 @消息
bot.OnGroupAtMessageCreate += async e =>
{
    var msg = e.Message;
    var groupId = msg.GroupOpenId; // 群 OpenId

    await bot.ReplyGroupAsync(msg, "收到群消息！");
};

// C2C 私聊消息
bot.OnC2CMessageCreate += async e =>
{
    await bot.ReplyC2CAsync(e.Message, "这是私聊回复");
};
```

### 频道事件

| 事件 | 触发时机 |
|------|---------|
| `OnGuildCreate` | 机器人被加入频道 |
| `OnGuildDelete` | 机器人被移出频道 |

```csharp
bot.OnGuildCreate += async e =>
{
    Console.WriteLine($"加入新频道: {e.Guild.Name} ({e.Guild.Id})");
};

bot.OnGuildDelete += async e =>
{
    Console.WriteLine($"离开频道: {e.Guild.Id}");
};
```

### 成员事件

| 事件 | 触发时机 |
|------|---------|
| `OnGuildMemberAdd` | 新成员加入频道 |
| `OnGuildMemberRemove` | 成员离开频道 |

```csharp
bot.OnGuildMemberAdd += async e =>
{
    var member = e.Member;
    Console.WriteLine($"{member.User?.Username} 加入 {e.GuildId}");
};

bot.OnGuildMemberRemove += async e =>
{
    Console.WriteLine($"{e.Member.User?.Username} 离开 {e.GuildId}");
};
```

### 群事件

| 事件 | 触发时机 |
|------|---------|
| `OnGroupAddRobot` | 机器人被拉入群 |
| `OnGroupDelRobot` | 机器人被移出群 |

```csharp
bot.OnGroupAddRobot += async e =>
{
    Console.WriteLine($"被拉入群: {e.GroupOpenId}, 操作人: {e.OpMemberOpenId}");
};

bot.OnGroupDelRobot += async e =>
{
    Console.WriteLine($"被移出群: {e.GroupOpenId}");
};
```

### 好友事件

| 事件 | 触发时机 |
|------|---------|
| `OnFriendAdd` | 用户添加机器人为好友 |
| `OnFriendDel` | 用户删除机器人好友 |

```csharp
bot.OnFriendAdd += async e =>
{
    Console.WriteLine($"新好友: {e.OpenId}");
};
```

### 互动事件

| 事件 | 触发时机 |
|------|---------|
| `OnInteractionCreate` | 用户点击按钮 |

```csharp
bot.OnInteractionCreate += async e =>
{
    var interaction = e.Interaction;
    var buttonId = interaction.Data?.Resolved?.ButtonId;

    if (buttonId == "confirm_btn")
    {
        Console.WriteLine($"用户 {interaction.UserOpenId} 点击了确认按钮");
        // 处理确认逻辑...
    }
};
```

### 连接事件

| 事件 | 触发时机 |
|------|---------|
| `OnReady` | WebSocket 连接成功，机器人上线 |

```csharp
bot.OnReady += async e =>
{
    Console.WriteLine($"机器人已上线！");
    Console.WriteLine($"  版本: {e.Version}");
    Console.WriteLine($"  用户: {e.User?.Username}");
    Console.WriteLine($"  会话ID: {e.SessionId}");
};
```

### EventDispatcher 中的更多事件

以下事件只能通过 `bot.Events` 访问（没有对应的快捷属性）：

| 事件 | 说明 |
|------|------|
| `OnMessageDelete` | 消息被删除 |
| `OnGuildUpdate` | 频道信息更新 |
| `OnChannelCreate` | 子频道创建 |
| `OnChannelUpdate` | 子频道更新 |
| `OnChannelDelete` | 子频道删除 |
| `OnGuildMemberUpdate` | 成员信息更新 |
| `OnMessageReactionAdd` | 表情表态添加 |
| `OnMessageReactionRemove` | 表情表态移除 |
| `OnMessageAuditPass` | 消息审核通过 |
| `OnMessageAuditReject` | 消息审核拒绝 |
| `OnAudioStart` / `OnAudioFinish` | 音频开始/结束 |
| `OnForumThreadCreate` / `OnForumThreadUpdate` / `OnForumThreadDelete` | 论坛帖子事件 |
| `OnForumPostCreate` / `OnForumPostDelete` | 论坛评论事件 |
| `OnResumed` | WebSocket 会话恢复 |
| `OnRawEvent` | 原始事件（所有事件的底层入口） |

```csharp
// 访问 EventDispatcher 中的事件
bot.Events.OnMessageReactionAdd += async e =>
{
    Console.WriteLine($"{e.UserId} 对消息 {e.Target?.Id} 点了 {e.Emoji?.Id}");
};

// 原始事件 —— 所有事件的入口
bot.Events.OnRawEvent += async (eventType, data, eventId) =>
{
    // 调试用途
    Console.WriteLine($"[RAW] {eventType}: {data}");
};
```

## Intents —— 事件开关

**重要：** 只有订阅了对应 `Intents` 才能收到事件。这类似 Discord Bot 的 Intents 机制。

```csharp
var bot = new QQBotClientBuilder()
    .WithIntents(
        Intents.Default              // 公域基本事件
        | Intents.GroupAtMessages    // 群 @消息
        | Intents.C2CMessages        // C2C 私聊
        | Intents.GuildMessages      // 频道全量消息（私域权限）
        | Intents.Interaction        // 按钮互动回调
        | Intents.MessageAudit       // 消息审核
    )
    .Build();
```

### 完整 Intents 列表

| Intent | 值 | 说明 |
|--------|-----|------|
| `None` | 0 | 不订阅任何事件 |
| `Guilds` | 1 (1<<0) | 频道创建/更新/删除 |
| `GuildMembers` | 2 (1<<1) | 成员加入/离开/更新 |
| `GuildMessages` | 512 (1<<9) | 频道全量消息（私域） |
| `GuildMessageReactions` | 1024 (1<<10) | 表情表态事件 |
| `DirectMessage` | 4096 (1<<12) | 频道私信事件 |
| `GroupAtMessages` | 1<<25 | 群 @消息 |
| `C2CMessages` | 1<<25 | C2C 私聊消息 |
| `Interaction` | 1<<26 | 按钮互动回调 |
| `MessageAudit` | 1<<27 | 消息审核事件 |
| `Forums` | 1<<28 | 论坛事件 |
| `AudioAction` | 1<<29 | 音频事件 |
| `PublicGuildMessages` | 1<<30 | 公域 @消息 |
| `Default` | - | Guilds \| GuildMembers \| PublicGuildMessages \| GuildMessageReactions \| DirectMessage \| Interaction \| MessageAudit |
| `PrivateAll` | - | Default + GuildMessages + Forums + AudioAction |

::: tip 只订阅需要的事件
订阅过多不需要的 Intents 会增加不必要的网络流量和处理开销。
::

## 完整事件处理示例

```csharp
using Luolan.QQBot.Events;

// 连接
bot.OnReady += OnReady;
// 消息
bot.OnAtMessageCreate += OnAtMessage;
bot.OnGroupAtMessageCreate += OnGroupMessage;
bot.OnC2CMessageCreate += OnC2CMessage;
// 成员
bot.OnGuildMemberAdd += OnMemberJoin;
bot.OnGuildMemberRemove += OnMemberLeave;
// 互动
bot.OnInteractionCreate += OnInteraction;

async Task OnReady(ReadyEvent e)
{
    Console.WriteLine($"上线: {e.User?.Username}");
}

async Task OnAtMessage(AtMessageCreateEvent e)
{
    var msg = e.Message;
    if (msg.Content?.Trim() == "/ping")
        await bot.ReplyAsync(msg, "pong!");
}

async Task OnGroupMessage(GroupAtMessageCreateEvent e)
{
    await bot.ReplyGroupAsync(e.Message, "群消息已收到");
}

async Task OnC2CMessage(C2CMessageCreateEvent e)
{
    await bot.ReplyC2CAsync(e.Message, "私聊消息已收到");
}

async Task OnMemberJoin(GuildMemberAddEvent e)
{
    Console.WriteLine($"欢迎新成员: {e.Member.User?.Username}");
}

async Task OnMemberLeave(GuildMemberRemoveEvent e)
{
    Console.WriteLine($"成员离开: {e.Member.User?.Username}");
}

async Task OnInteraction(InteractionCreateEvent e)
{
    var btnId = e.Interaction.Data?.Resolved?.ButtonId;
    Console.WriteLine($"按钮点击: {btnId}");
}
```

## 下一步

- [消息发送](/guide/messages) — 发送各种类型的消息
- [控制器模式](/guide/controller-mode) — 组织命令逻辑的更好方式
