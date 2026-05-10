# 事件类型

完整的事件类型参考。

**命名空间**: `Luolan.QQBot.Events`

## 事件基类

```csharp
public abstract class QQBotEventBase
{
    public abstract string EventType { get; }
    public JsonElement? RawData { get; set; }
    public string? EventId { get; set; }
}
```

## 连接事件

```csharp
// 连接就绪
public class ReadyEvent : QQBotEventBase
{
    public string Version { get; set; }
    public string SessionId { get; set; }
    public User? User { get; set; }
    public int[]? Shard { get; set; }
}

// 会话恢复
public class ResumedEvent : QQBotEventBase { }
```

## 消息事件

### 基类
```csharp
public abstract class MessageEventBase : QQBotEventBase
{
    public Message Message { get; set; }
}
```

### 具体消息事件

| 事件类 | 触发场景 | 需要的 Intent |
|--------|---------|--------------|
| `MessageCreateEvent` | 频道消息（私域） | `GuildMessages` |
| `AtMessageCreateEvent` | 频道 @机器人（公域） | `PublicGuildMessages` |
| `DirectMessageCreateEvent` | 频道私信 | `DirectMessage` |
| `GroupAtMessageCreateEvent` | 群聊 @机器人 | `GroupAtMessages` |
| `C2CMessageCreateEvent` | C2C 私聊 | `C2CMessages` |
| `MessageDeleteEvent` | 消息被删除 | — |

## 频道事件

```csharp
public class GuildCreateEvent : QQBotEventBase
{
    public Guild Guild { get; set; }
    public string? OpUserId { get; set; }
}

public class GuildUpdateEvent : QQBotEventBase
{
    public Guild Guild { get; set; }
    public string? OpUserId { get; set; }
}

public class GuildDeleteEvent : QQBotEventBase
{
    public Guild Guild { get; set; }
    public string? OpUserId { get; set; }
}

public class ChannelCreateEvent : QQBotEventBase
{
    public Channel Channel { get; set; }
    public string? OpUserId { get; set; }
}

public class ChannelUpdateEvent : QQBotEventBase
{
    public Channel Channel { get; set; }
    public string? OpUserId { get; set; }
}

public class ChannelDeleteEvent : QQBotEventBase
{
    public Channel Channel { get; set; }
    public string? OpUserId { get; set; }
}
```

## 成员事件

```csharp
public class GuildMemberAddEvent : QQBotEventBase
{
    public Member Member { get; set; }
    public string GuildId { get; set; }
    public string? OpUserId { get; set; }
}

public class GuildMemberUpdateEvent : QQBotEventBase
{
    public Member Member { get; set; }
    public string GuildId { get; set; }
    public string? OpUserId { get; set; }
}

public class GuildMemberRemoveEvent : QQBotEventBase
{
    public Member Member { get; set; }
    public string GuildId { get; set; }
    public string? OpUserId { get; set; }
}
```

## 群和好友事件

```csharp
// 群添加/移除机器人
public class GroupAddRobotEvent : QQBotEventBase
{
    public long Timestamp { get; set; }
    public string GroupOpenId { get; set; }
    public string OpMemberOpenId { get; set; }
}

public class GroupDelRobotEvent : QQBotEventBase
{
    public long Timestamp { get; set; }
    public string GroupOpenId { get; set; }
    public string OpMemberOpenId { get; set; }
}

// 好友添加/删除
public class FriendAddEvent : QQBotEventBase
{
    public long Timestamp { get; set; }
    public string OpenId { get; set; }
}

public class FriendDelEvent : QQBotEventBase
{
    public long Timestamp { get; set; }
    public string OpenId { get; set; }
}
```

## 互动事件

```csharp
public class InteractionCreateEvent : QQBotEventBase
{
    public Interaction Interaction { get; set; }
}
```

## 表情表态事件

```csharp
public class MessageReactionAddEvent : QQBotEventBase
{
    public string UserId { get; set; }
    public string GuildId { get; set; }
    public string ChannelId { get; set; }
    public ReactionTarget? Target { get; set; }
    public Emoji? Emoji { get; set; }
}

public class MessageReactionRemoveEvent : QQBotEventBase
{
    // 同上结构
}

public class ReactionTarget
{
    public string Id { get; set; }
    public int Type { get; set; }
}

public class Emoji
{
    public string Id { get; set; }
    public int Type { get; set; }
}
```

## 审核事件

```csharp
public class MessageAuditPassEvent : QQBotEventBase
{
    public MessageAudited? MessageAudited { get; set; }
}

public class MessageAuditRejectEvent : QQBotEventBase
{
    public MessageAudited? MessageAudited { get; set; }
}
```

## 音频和论坛事件

```csharp
public class AudioStartEvent : QQBotEventBase { }
public class AudioFinishEvent : QQBotEventBase { }
public class AudioOnMicEvent : QQBotEventBase { }
public class AudioOffMicEvent : QQBotEventBase { }

public class ForumThreadCreateEvent : QQBotEventBase { }
public class ForumThreadUpdateEvent : QQBotEventBase { }
public class ForumThreadDeleteEvent : QQBotEventBase { }
public class ForumPostCreateEvent : QQBotEventBase { }
public class ForumPostDeleteEvent : QQBotEventBase { }
public class ForumReplyCreateEvent : QQBotEventBase { }
public class ForumReplyDeleteEvent : QQBotEventBase { }
```

## EventDispatcher

```csharp
// 通过 bot.Events 访问
public class EventDispatcher
{
    // 40+ 个事件委托
    public Func<ReadyEvent, Task>? OnReady { get; set; }
    public Func<MessageCreateEvent, Task>? OnMessageCreate { get; set; }
    public Func<AtMessageCreateEvent, Task>? OnAtMessageCreate { get; set; }
    // ... 所有事件类型

    // 原始事件入口
    public Func<string, JsonElement, string?, Task>? OnRawEvent { get; set; }
}
```

## Intents

```csharp
// 命名空间: Luolan.QQBot
public enum Intents
{
    None                    = 0,
    Guilds                  = 1 << 0,   // 频道事件
    GuildMembers            = 1 << 1,   // 成员事件
    GuildMessages           = 1 << 9,   // 频道全量消息（私域）
    GuildMessageReactions   = 1 << 10,  // 表情表态
    DirectMessage           = 1 << 12,  // 私信
    Interaction             = 1 << 26,  // 互动回调
    MessageAudit            = 1 << 27,  // 消息审核
    Forums                  = 1 << 28,  // 论坛
    AudioAction             = 1 << 29,  // 音频
    PublicGuildMessages     = 1 << 30,  // 公域消息
    GroupAtMessages         = 1 << 25,  // 群@消息
    C2CMessages             = 1 << 25,  // C2C消息

    Default   = Guilds | GuildMembers | PublicGuildMessages
              | GuildMessageReactions | DirectMessage
              | Interaction | MessageAudit,
    PrivateAll = Default | GuildMessages | Forums | AudioAction
}
```
