# QQBotHttpClient

底层 HTTP API 客户端，封装了所有 QQ Bot REST API 调用。

**命名空间**: `Luolan.QQBot.Services`

通过 `bot.Api` 访问实例。

## 基础请求方法

```csharp
Task<T> GetAsync<T>(string endpoint, CancellationToken ct = default)
Task<T> PostAsync<T>(string endpoint, object? body = null, CancellationToken ct = default)
Task PostAsync(string endpoint, object? body = null, CancellationToken ct = default)
Task<T> PutAsync<T>(string endpoint, object? body = null, CancellationToken ct = default)
Task PutAsync(string endpoint, object? body = null, CancellationToken ct = default)
Task<T> PatchAsync<T>(string endpoint, object? body = null, CancellationToken ct = default)
Task DeleteAsync(string endpoint, CancellationToken ct = default)
```

## 用户 API

```csharp
Task<BotInfo> GetCurrentUserAsync()
Task<List<Guild>> GetCurrentUserGuildsAsync()
```

## 频道 API

```csharp
Task<Guild> GetGuildAsync(string guildId)
```

## 子频道 API

```csharp
Task<List<Channel>> GetChannelsAsync(string guildId)
Task<Channel> GetChannelAsync(string channelId)
Task<Channel> CreateChannelAsync(string guildId, object channelInfo)
Task<Channel> ModifyChannelAsync(string channelId, object channelInfo)
Task DeleteChannelAsync(string channelId)
```

## 成员 API

```csharp
Task<List<Member>> GetGuildMembersAsync(string guildId, string? after = null, int limit = 100)
Task<Member> GetGuildMemberAsync(string guildId, string userId)
Task DeleteGuildMemberAsync(string guildId, string userId, bool addBlacklist = false, int deleteHistoryMsgDays = 0)
```

## 角色 API

```csharp
Task<GetGuildRolesResponse> GetGuildRolesAsync(string guildId)
Task<CreateRoleResponse> CreateGuildRoleAsync(string guildId, CreateRoleRequest request)
Task<CreateRoleResponse> ModifyGuildRoleAsync(string guildId, string roleId, CreateRoleRequest request)
Task DeleteGuildRoleAsync(string guildId, string roleId)
Task AddMemberToRoleAsync(string guildId, string userId, string roleId, string? channelId = null)
Task RemoveMemberFromRoleAsync(string guildId, string userId, string roleId, string? channelId = null)
```

## 子频道权限 API

```csharp
Task<ChannelPermissions> GetChannelPermissionsAsync(string channelId, string userId)
Task ModifyChannelPermissionsAsync(string channelId, string userId, UpdateChannelPermissionsRequest request)
Task<ChannelPermissions> GetChannelRolePermissionsAsync(string channelId, string roleId)
Task ModifyChannelRolePermissionsAsync(string channelId, string roleId, UpdateChannelPermissionsRequest request)
```

## 消息 API

```csharp
Task<Message> GetMessageAsync(string channelId, string messageId)
Task<Message> SendMessageAsync(string channelId, SendMessageRequest request)
Task<Message> SendTextMessageAsync(string channelId, string content, string? msgId = null)
Task DeleteMessageAsync(string channelId, string messageId, bool hideTip = false)
```

## 私信 API

```csharp
Task<DirectMessageSession> CreateDmsAsync(CreateDmsRequest request)
Task<Message> SendDmsAsync(string guildId, SendMessageRequest request)
Task DeleteDmsAsync(string guildId, string messageId, bool hideTip = false)
```

## 群消息 API

```csharp
Task<SendGroupMessageResponse> SendGroupMessageAsync(string groupOpenId, SendGroupMessageRequest request)
Task<SendGroupMessageResponse> SendGroupTextMessageAsync(string groupOpenId, string content, string? msgId = null, int msgSeq = 0)
Task<UploadMediaResponse> UploadGroupMediaAsync(string groupOpenId, UploadMediaRequest request)
```

## C2C 消息 API

```csharp
Task<SendGroupMessageResponse> SendC2CMessageAsync(string openId, SendGroupMessageRequest request)
Task<SendGroupMessageResponse> SendC2CTextMessageAsync(string openId, string content, string? msgId = null, int msgSeq = 0)
Task<UploadMediaResponse> UploadC2CMediaAsync(string openId, UploadMediaRequest request)
```

## 表情表态 API

```csharp
Task AddReactionAsync(string channelId, string messageId, string emojiType, string emojiId)
Task DeleteReactionAsync(string channelId, string messageId, string emojiType, string emojiId)
```

## 精华消息 API

```csharp
Task<PinsMessage> AddPinsMessageAsync(string channelId, string messageId)
Task DeletePinsMessageAsync(string channelId, string messageId)
Task<PinsMessage> GetPinsMessageAsync(string channelId)
```

## 日程 API

```csharp
Task<List<Schedule>> GetSchedulesAsync(string channelId, string? since = null)
Task<Schedule> GetScheduleAsync(string channelId, string scheduleId)
Task<Schedule> CreateScheduleAsync(string channelId, Schedule schedule)
Task<Schedule> ModifyScheduleAsync(string channelId, string scheduleId, Schedule schedule)
Task DeleteScheduleAsync(string channelId, string scheduleId)
```

## 禁言 API

```csharp
Task MuteGuildAsync(string guildId, int muteSeconds = 0, string? muteEndTimestamp = null)
Task MuteMemberAsync(string guildId, string userId, int muteSeconds = 0, string? muteEndTimestamp = null)
Task MuteMembersAsync(string guildId, IEnumerable<string> userIds, int muteSeconds = 0, string? muteEndTimestamp = null)
```

## 公告 API

```csharp
Task<Announces> CreateAnnouncesAsync(string guildId, string channelId, string messageId)
Task DeleteAnnouncesAsync(string guildId, string messageId = "all")
```

## 网关 API

```csharp
Task<GatewayInfo> GetGatewayAsync()
Task<GatewayBotInfo> GetGatewayBotAsync()
```

## 异常

```csharp
public class QQBotApiException : Exception
{
    public int Code { get; }        // API 错误码
    public string? TraceId { get; } // 追踪 ID
}
```

## TokenManager

```csharp
// 通过 bot.TokenManager 访问
Task<string> GetAccessTokenAsync(CancellationToken ct = default)
Task<string> GetAuthorizationHeaderAsync(CancellationToken ct = default)
Task<string> RefreshTokenAsync(CancellationToken ct = default)
```
