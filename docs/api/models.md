# 模型类型

SDK 中使用的核心数据模型，位于 `Luolan.QQBot.Models` 命名空间。

## Message

消息对象，包含消息内容和元数据。

```csharp
public class Message
{
    public string Id { get; set; }             // 消息 ID
    public string Content { get; set; }         // 消息文本内容
    public string? ChannelId { get; set; }     // 子频道 ID（频道消息）
    public string? GuildId { get; set; }       // 频道 ID（频道消息）
    public string? GroupOpenId { get; set; }   // 群 OpenId（群消息）
    public User? Author { get; set; }          // 发送者
    public DateTime Timestamp { get; set; }    // 发送时间
    public int Seq { get; set; }               // 消息序号（群/C2C）
    // ...
}
```

---

## User

用户信息。

```csharp
public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string? Avatar { get; set; }
    // ...
}
```

---

## Guild

频道信息。

```csharp
public class Guild
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Icon { get; set; }
    public string? OwnerId { get; set; }
    // ...
}
```

---

## Channel

子频道信息。

```csharp
public class Channel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Type { get; set; }
    public string? GuildId { get; set; }
    // ...
}
```

---

## Member

频道成员信息。

```csharp
public class Member
{
    public User? User { get; set; }
    public string Nick { get; set; }
    public List<string> Roles { get; set; }
    public DateTime JoinedAt { get; set; }
    // ...
}
```

---

## MessageMarkdown

Markdown 消息内容。

```csharp
public class MessageMarkdown
{
    public string? CustomTemplateId { get; set; }
    public Dictionary<string, string[]>? Params { get; set; }
    public string? Content { get; set; }
}
```

---

## Keyboard / ButtonGroup

键盘和按钮组。

```csharp
public class Keyboard
{
    public List<ButtonGroup>? Rows { get; set; }
}

public class ButtonGroup
{
    public List<Button> Buttons { get; set; }
}

public class Button
{
    public string Id { get; set; }
    public ButtonRenderData RenderData { get; set; }
    public ButtonAction Action { get; set; }
}

public class ButtonRenderData
{
    public string Label { get; set; }
    public string? VisitedLabel { get; set; }
    public int Style { get; set; }
}

public class ButtonAction
{
    public int Type { get; set; }
    public ButtonPermission Permission { get; set; }
    public string Data { get; set; }
    public bool Enter { get; set; }
    public string? UnsupportTips { get; set; }
}

public class ButtonPermission
{
    public int Type { get; set; }
    public List<string>? SpecifyUserIds { get; set; }
    public List<string>? SpecifyRoleIds { get; set; }
}
```

---

## Interaction

按钮点击互动数据。

```csharp
public class Interaction
{
    public string Id { get; set; }
    public string UserOpenId { get; set; }
    public InteractionData? Data { get; set; }
    // ...
}

public class InteractionData
{
    public InteractionResolved? Resolved { get; set; }
}

public class InteractionResolved
{
    public string? ButtonId { get; set; }
}
```

---

## SendMessageRequest

发送频道消息的请求对象。

```csharp
public class SendMessageRequest
{
    public string? Content { get; set; }
    public MessageEmbed? Embed { get; set; }
    public string? Image { get; set; }
    public MessageMarkdown? Markdown { get; set; }
    public Keyboard? Keyboard { get; set; }
    public MessageReference? MessageReference { get; set; }
    public string? MsgId { get; set; }
}
```

---

## SendGroupMessageRequest

发送群/C2C 消息的请求对象。

```csharp
public class SendGroupMessageRequest
{
    public string? Content { get; set; }
    public int MsgType { get; set; }
    public MessageMarkdown? Markdown { get; set; }
    public MediaInfo? Media { get; set; }
    public Keyboard? Keyboard { get; set; }
    public string? MsgId { get; set; }
    public int MsgSeq { get; set; }
}
```

---

## QQBotApiException

API 异常类，位于 `Luolan.QQBot.Services`。

```csharp
public class QQBotApiException : Exception
{
    public int Code { get; }        // API 错误码
    public string? TraceId { get; } // 追踪 ID
}
```
