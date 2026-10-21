# 群管理与群消息

v1.5.0 补齐了 QQ 群管理能力，包括群信息、群成员、入群审批、黑名单与禁言。

## 事件订阅

群相关能力分布在两个 Intent 中：

| Intent | 值 | 涵盖事件 |
|--------|-----|---------|
| `GroupAndC2C` | `1 << 25` | 群 @消息、全量群消息、C2C 私聊、好友事件、订阅消息授权 |
| `GroupMembers` | `1 << 24` | 群成员加入/退出、入群申请 |

```csharp
var bot = new QQBotClientBuilder()
    .WithAppId("...")
    .WithClientSecret("...")
    .WithIntents(Intents.GroupAndC2C | Intents.GroupMembers)
    .Build();
```

::: tip 权限说明
`GROUP_MEMBER_EVENT`、群成员列表、黑名单等能力目前需要平台白名单权限；没有权限时接口会返回 `11253 应用无接口访问权限`。
:::

## 群消息

### 接收群消息

```csharp
// 仅 @机器人 的消息
bot.OnGroupAtMessageCreate += async e =>
{
    Console.WriteLine($"[@] {e.Message.Author?.Username}: {e.Message.Content}");
    await bot.ReplyGroupAsync(e.Message, "收到！");
};

// 全量群消息（需在开放平台开启"接收所有消息"）
bot.OnGroupMessageCreate += async e =>
{
    var msg = e.Message;
    Console.WriteLine($"[全量] {msg.Author?.Username}: {msg.Content}");

    // 引用消息、附件、卡片等
    foreach (var attachment in msg.Attachments ?? [])
    {
        Console.WriteLine($"附件: {attachment.ContentType} {attachment.Url}");
    }
};
```

`Message` 在 v1.5.0 新增了全量消息相关字段：

| 字段 | 说明 |
|------|------|
| `MessageType` | 0=文本, 3=卡片, 101=并行消息, 102=聊天记录, 103=引用消息 |
| `MessageScene` | 消息场景上下文，`Ext` 中包含 `msg_idx`、`ref_msg_idx` 等 |
| `ArkData` | 结构化卡片消息 |
| `MsgElements` | 消息元素列表（支持嵌套） |
| `Attachments` | 附件，新增 `VoiceWavUrl`、`AsrReferText` |

### 发送群消息

```csharp
// 文本
await bot.SendGroupMessageAsync(groupOpenId, "hello", msgId: msg.Id);

// Markdown + 键盘
await bot.SendGroupMarkdownAsync(groupOpenId, markdown, keyboard, msgId: msg.Id);

// 富媒体（自动上传并发送）
await bot.SendGroupMediaAsync(groupOpenId, "https://example.com/a.png", fileType: 1);

// 撤回
await bot.RevokeGroupMessageAsync(groupOpenId, messageId);
```

`msg_type` 取值：`0` 文本、`2` Markdown、`7` 富媒体。

## 群信息

```csharp
var info = await bot.GetGroupInfoAsync(groupOpenId);
Console.WriteLine($"{info.GroupName} / {info.GroupMemberNum} 人");

var state = await bot.GetGroupBotStateAsync(groupOpenId);
Console.WriteLine($"机器人角色: {state.MemberRole}");
Console.WriteLine($"接收主动消息: {state.AllowProactiveMsg}");
Console.WriteLine($"收消息设置: {state.RecvMsgSetting}"); // all / only_mention / mention_and_context
```

## 群成员

```csharp
// 分页拉取，每次最多 30 条
var page = await bot.GetGroupMembersAsync(groupOpenId);
foreach (var member in page.Members ?? [])
{
    Console.WriteLine($"{member.Username} ({member.MemberRole})");
}

// 下一页
if (!string.IsNullOrEmpty(page.NextCursor))
{
    var next = await bot.GetGroupMembersAsync(groupOpenId, page.NextCursor);
}

// 单个成员
var one = await bot.GetGroupMemberAsync(groupOpenId, memberOpenId);

// 批量移除（单次最多 20 个）
await bot.Api.BatchRemoveGroupMembersAsync(groupOpenId, new BatchRemoveMembersRequest
{
    MemberOpenIds = new() { memberOpenId },
    AddToMemberBlacklist = false
});
```

## 入群申请与审批

### 接收申请事件

```csharp
bot.OnGroupJoinRequest += async e =>
{
    Console.WriteLine($"申请人: {e.Username}");
    Console.WriteLine($"验证方式: {e.VerifyInfo?.Method}");
    Console.WriteLine($"验证消息: {e.VerifyInfo?.VerifyMessage}");

    // 自动通过
    await bot.ApproveGroupJoinRequestAsync(e.GroupOpenId, e.MemberOpenId,
        new ApproveJoinRequestRequest
        {
            Op = "approve",
            JoinRequestId = e.JoinRequestId
        });
};
```

### 拉取申请列表

```csharp
var requests = await bot.Api.GetGroupJoinRequestsAsync(groupOpenId, limit: 20);
foreach (var request in requests.List ?? [])
{
    Console.WriteLine($"{request.Username} 申请加入，来源: {request.ApplySource}");
}
```

### 自动审批策略

```csharp
// 创建策略（group_openids 与 group_ids 二选一）
var strategy = await bot.Api.CreateJoinApprovalStrategyAsync(new CreateJoinApprovalStrategyRequest
{
    GroupOpenIds = new() { groupOpenId },
    IsEnable = "on",
    Remark = "白名单自动通过"
});

// 修改白名单
await bot.Api.UpdateJoinApprovalWhitelistAsync(strategy.StrategyId!, new UpdateWhitelistUsersRequest
{
    Op = "add",
    WhitelistUsers = new() { "123456789" }
});

// 立即执行一次扫描（异步，约 10 分钟）
await bot.Api.ExecuteJoinApprovalStrategyAsync(strategy.StrategyId!);

// 查询 / 修改 / 删除
var strategies = await bot.Api.GetJoinApprovalStrategiesAsync();
await bot.Api.UpdateJoinApprovalStrategyAsync(strategy.StrategyId!, new UpdateJoinApprovalStrategyRequest
{
    IsEnable = "off"
});
await bot.Api.DeleteJoinApprovalStrategyAsync(strategy.StrategyId!);
```

## 群黑名单

```csharp
// 查询
var blacklist = await bot.Api.GetGroupBlacklistAsync(groupOpenId);
Console.WriteLine($"黑名单人数: {blacklist.Users?.Count ?? 0}");

// 添加 / 移除
await bot.Api.UpdateGroupBlacklistAsync(groupOpenId, new GroupBlacklistRequest
{
    Op = "add", // add / del
    MemberOpenIds = new() { memberOpenId }
});
```

## 群禁言

```csharp
// 查询全员禁言规则和成员禁言列表
var setting = await bot.Api.GetGroupRestrictChatSettingAsync(groupOpenId);
Console.WriteLine($"全员禁言模式: {setting.GlobalRule?.Mode}"); // none / always / schedule

// 设置成员禁言
await bot.SetGroupMemberMuteAsync(groupOpenId, new SetGroupMemberMuteRequest
{
    Members = new()
    {
        new SetMemberMuteState
        {
            Op = "add", // add / update / del
            MemberOpenId = memberOpenId,
            MuteExpireAt = DateTimeOffset.Now.AddHours(1).ToString("O")
        }
    }
});

// 解除禁言
await bot.SetGroupMemberMuteAsync(groupOpenId, new SetGroupMemberMuteRequest
{
    Members = new()
    {
        new SetMemberMuteState { Op = "del", MemberOpenId = memberOpenId }
    }
});
```

## 下一步

- [Webhook 接入](/guide/webhook) — 不用 WebSocket 也能接收事件
- [事件类型](/api/event-types) — 查看全部群事件字段
- [QQBotHttpClient](/api/http-client) — 查看完整群管理 API
