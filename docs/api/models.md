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

    // v1.5.1：全量群消息字段
    public int MessageType { get; set; }
    public MessageScene? MessageScene { get; set; }
    public ArkData? ArkData { get; set; }
    public List<MsgElement>? MsgElements { get; set; }
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
    public bool Bot { get; set; }

    public string? MemberOpenId { get; set; }     // 群成员 openid
    public string? UserOpenId { get; set; }       // C2C 用户 openid
    public string? UnionOpenId { get; set; }      // 跨应用 openid
    public string? UnionUserAccount { get; set; }
    public string? MemberRole { get; set; }       // member/admin/owner
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
    public int MsgType { get; set; }        // 0=文本 2=Markdown 7=富媒体
    public MessageMarkdown? Markdown { get; set; }
    public MediaInfo? Media { get; set; }
    public Keyboard? Keyboard { get; set; }
    public string? MsgId { get; set; }
    public string? EventId { get; set; }
    public int MsgSeq { get; set; }
    public MessageReference? MessageReference { get; set; }
    public bool IsWakeup { get; set; }       // 互动召回消息
    public InputNotify? InputNotify { get; set; } // msg_type=6
}
```

---

## 群管理模型

```csharp
public class GroupInfo
{
    public string GroupOpenId { get; set; }
    public string? GroupName { get; set; }
    public string? GroupFingerMemo { get; set; }
    public string? GroupClassText { get; set; }
    public List<string>? GroupTags { get; set; }
    public int GroupMemberNum { get; set; }
}

public class GroupBotState
{
    public string? MemberOpenId { get; set; }
    public string? JoinedAt { get; set; }
    public bool AllowProactiveMsg { get; set; }
    public string? RecvMsgSetting { get; set; } // all/only_mention/mention_and_context
    public string? MemberRole { get; set; }     // member/admin/owner
}

public class GroupMember
{
    public string MemberOpenId { get; set; }
    public string? Username { get; set; }
    public string? MemberRole { get; set; }
    public bool Bot { get; set; }
    public string? JoinedAt { get; set; }
    public string? UnionOpenId { get; set; }
}

public class GroupMemberListResponse
{
    public List<GroupMember>? Members { get; set; }
    public string? NextCursor { get; set; }
}
```

入群申请、审批策略、黑名单和禁言模型位于 `Luolan.QQBot.Models`：

- `JoinRequestInfo` / `JoinRequestListResponse`
- `ApproveJoinRequestRequest`
- `JoinApprovalStrategy` / `CreateJoinApprovalStrategyRequest` / `UpdateJoinApprovalStrategyRequest` / `UpdateWhitelistUsersRequest`
- `GroupBlacklistUser` / `GroupBlacklistRequest` / `GroupBlacklistResponse`
- `GroupRestrictChatSetting` / `SetGroupMemberMuteRequest` / `SetMemberMuteState`

---

## 菜单与面板模型

```csharp
public class QQBotMenu
{
    public List<MenuItem>? Items { get; set; }
}

public class MenuItem
{
    public string? Name { get; set; }
    public string? Type { get; set; }          // switch/send_message/link/menu
    public List<SubMenuItem>? SubMenuItems { get; set; }
    public string? SendMessage { get; set; }
    public string? Link { get; set; }
    public MenuSwitch? Switch { get; set; }
}

public class Panel
{
    public List<PanelItem>? Items { get; set; }
    public string? Remark { get; set; }
    public int Version { get; set; }
}

public class PanelItem
{
    public string? Name { get; set; }
    public string? Desc { get; set; }
    public string? Type { get; set; }          // command/link
    public bool OnlyAdmin { get; set; }
    public string? Link { get; set; }
}

public class CreatePanelRequest
{
    public string Scope { get; set; }          // c2c/group/channel/dm
    public string TargetType { get; set; }     // all/specific
    public List<string>? UserOpenIds { get; set; }
    public List<string>? GroupOpenIds { get; set; }
    public Panel Panel { get; set; }
}
```

---

## 流式消息与上传模型

```csharp
public class SendC2CStreamMessageRequest
{
    public string? InputMode { get; set; }     // append/replace
    public int InputState { get; set; }        // 1=生成中 10=结束
    public int Index { get; set; }
    public string? ContentType { get; set; }   // text/markdown
    public string ContentRaw { get; set; }
    public string? EventId { get; set; }
    public string? MsgId { get; set; }
    public string? StreamMsgId { get; set; }
    public int MsgSeq { get; set; }
    public bool IsWakeup { get; set; }
}

public class UploadPrepareRequest
{
    public int FileType { get; set; }
    public string FileSize { get; set; }
    public string FileName { get; set; }
    public string Md5 { get; set; }
    public string Sha1 { get; set; }
    public string Md510m { get; set; }
}

public class UploadPrepareResponse
{
    public string UploadId { get; set; }
    public string BlockSize { get; set; }
    public List<UploadPart>? Parts { get; set; }
    public UploadConfig? UploadConfig { get; set; }
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
