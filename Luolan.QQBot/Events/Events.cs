using System.Text.Json;
using System.Text.Json.Serialization;
using Luolan.QQBot.Models;

namespace Luolan.QQBot.Events;

/// <summary>
/// 事件基类
/// </summary>
public abstract class QQBotEventBase
{
    /// <summary>
    /// 事件类型名称
    /// </summary>
    public abstract string EventType { get; }

    /// <summary>
    /// 原始JSON数据
    /// </summary>
    [JsonIgnore]
    public JsonElement? RawData { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    public string? EventId { get; set; }
}

#region 频道事件

/// <summary>
/// 频道创建事件
/// </summary>
public class GuildCreateEvent : QQBotEventBase
{
    public override string EventType => "GUILD_CREATE";

    /// <summary>
    /// 频道信息
    /// </summary>
    public Guild Guild { get; set; } = new();

    /// <summary>
    /// 操作人ID
    /// </summary>
    public string? OpUserId { get; set; }
}

/// <summary>
/// 频道更新事件
/// </summary>
public class GuildUpdateEvent : QQBotEventBase
{
    public override string EventType => "GUILD_UPDATE";
    public Guild Guild { get; set; } = new();
    public string? OpUserId { get; set; }
}

/// <summary>
/// 频道删除事件
/// </summary>
public class GuildDeleteEvent : QQBotEventBase
{
    public override string EventType => "GUILD_DELETE";
    public Guild Guild { get; set; } = new();
    public string? OpUserId { get; set; }
}

/// <summary>
/// 子频道创建事件
/// </summary>
public class ChannelCreateEvent : QQBotEventBase
{
    public override string EventType => "CHANNEL_CREATE";
    public Channel Channel { get; set; } = new();
    public string? OpUserId { get; set; }
}

/// <summary>
/// 子频道更新事件
/// </summary>
public class ChannelUpdateEvent : QQBotEventBase
{
    public override string EventType => "CHANNEL_UPDATE";
    public Channel Channel { get; set; } = new();
    public string? OpUserId { get; set; }
}

/// <summary>
/// 子频道删除事件
/// </summary>
public class ChannelDeleteEvent : QQBotEventBase
{
    public override string EventType => "CHANNEL_DELETE";
    public Channel Channel { get; set; } = new();
    public string? OpUserId { get; set; }
}

#endregion

#region 成员事件

/// <summary>
/// 成员加入事件
/// </summary>
public class GuildMemberAddEvent : QQBotEventBase
{
    public override string EventType => "GUILD_MEMBER_ADD";
    public Member Member { get; set; } = new();
    public string GuildId { get; set; } = string.Empty;
    public string? OpUserId { get; set; }
}

/// <summary>
/// 成员更新事件
/// </summary>
public class GuildMemberUpdateEvent : QQBotEventBase
{
    public override string EventType => "GUILD_MEMBER_UPDATE";
    public Member Member { get; set; } = new();
    public string GuildId { get; set; } = string.Empty;
    public string? OpUserId { get; set; }
}

/// <summary>
/// 成员移除事件
/// </summary>
public class GuildMemberRemoveEvent : QQBotEventBase
{
    public override string EventType => "GUILD_MEMBER_REMOVE";
    public Member Member { get; set; } = new();
    public string GuildId { get; set; } = string.Empty;
    public string? OpUserId { get; set; }
}

#endregion

#region 消息事件

/// <summary>
/// 消息事件基类
/// </summary>
public abstract class MessageEventBase : QQBotEventBase
{
    /// <summary>
    /// 消息对象
    /// </summary>
    public Message Message { get; set; } = new();
}

/// <summary>
/// 频道消息创建事件(私域)
/// </summary>
public class MessageCreateEvent : MessageEventBase
{
    public override string EventType => "MESSAGE_CREATE";
}

/// <summary>
/// 频道消息删除事件
/// </summary>
public class MessageDeleteEvent : QQBotEventBase
{
    public override string EventType => "MESSAGE_DELETE";

    /// <summary>
    /// 被删除的消息信息
    /// </summary>
    public DeletedMessage Message { get; set; } = new();

    /// <summary>
    /// 操作人ID
    /// </summary>
    public string? OpUserId { get; set; }
}

/// <summary>
/// 被删除的消息
/// </summary>
public class DeletedMessage
{
    [JsonPropertyName("message")]
    public MessageInfo? MessageInfo { get; set; }

    [JsonPropertyName("op_user")]
    public User? OpUser { get; set; }
}

/// <summary>
/// 删除的消息信息
/// </summary>
public class MessageInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;
}

/// <summary>
/// 公域消息创建事件(@机器人)
/// </summary>
public class AtMessageCreateEvent : MessageEventBase
{
    public override string EventType => "AT_MESSAGE_CREATE";
}

/// <summary>
/// 公域消息删除事件
/// </summary>
public class PublicMessageDeleteEvent : QQBotEventBase
{
    public override string EventType => "PUBLIC_MESSAGE_DELETE";
    public DeletedMessage Message { get; set; } = new();
    public string? OpUserId { get; set; }
}

/// <summary>
/// 私信消息创建事件
/// </summary>
public class DirectMessageCreateEvent : MessageEventBase
{
    public override string EventType => "DIRECT_MESSAGE_CREATE";
}

/// <summary>
/// 私信消息删除事件
/// </summary>
public class DirectMessageDeleteEvent : QQBotEventBase
{
    public override string EventType => "DIRECT_MESSAGE_DELETE";
    public DeletedMessage Message { get; set; } = new();
    public string? OpUserId { get; set; }
}

#endregion

#region 群/C2C消息事件

/// <summary>
/// 群@机器人消息事件
/// </summary>
public class GroupAtMessageCreateEvent : MessageEventBase
{
    public override string EventType => "GROUP_AT_MESSAGE_CREATE";
}

/// <summary>
/// C2C消息事件
/// </summary>
public class C2CMessageCreateEvent : MessageEventBase
{
    public override string EventType => "C2C_MESSAGE_CREATE";
}

/// <summary>
/// 群添加机器人事件
/// </summary>
public class GroupAddRobotEvent : QQBotEventBase
{
    public override string EventType => "GROUP_ADD_ROBOT";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("group_openid")]
    public string GroupOpenId { get; set; } = string.Empty;

    [JsonPropertyName("op_member_openid")]
    public string OpMemberOpenId { get; set; } = string.Empty;
}

/// <summary>
/// 群删除机器人事件
/// </summary>
public class GroupDelRobotEvent : QQBotEventBase
{
    public override string EventType => "GROUP_DEL_ROBOT";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("group_openid")]
    public string GroupOpenId { get; set; } = string.Empty;

    [JsonPropertyName("op_member_openid")]
    public string OpMemberOpenId { get; set; } = string.Empty;
}

/// <summary>
/// 群消息拒绝机器人主动消息事件
/// </summary>
public class GroupMsgRejectEvent : QQBotEventBase
{
    public override string EventType => "GROUP_MSG_REJECT";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("group_openid")]
    public string GroupOpenId { get; set; } = string.Empty;

    [JsonPropertyName("op_member_openid")]
    public string OpMemberOpenId { get; set; } = string.Empty;
}

/// <summary>
/// 群消息接受机器人主动消息事件
/// </summary>
public class GroupMsgReceiveEvent : QQBotEventBase
{
    public override string EventType => "GROUP_MSG_RECEIVE";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("group_openid")]
    public string GroupOpenId { get; set; } = string.Empty;

    [JsonPropertyName("op_member_openid")]
    public string OpMemberOpenId { get; set; } = string.Empty;
}

/// <summary>
/// 用户添加机器人事件
/// </summary>
public class FriendAddEvent : QQBotEventBase
{
    public override string EventType => "FRIEND_ADD";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("openid")]
    public string OpenId { get; set; } = string.Empty;
}

/// <summary>
/// 用户删除机器人事件
/// </summary>
public class FriendDelEvent : QQBotEventBase
{
    public override string EventType => "FRIEND_DEL";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("openid")]
    public string OpenId { get; set; } = string.Empty;
}

/// <summary>
/// C2C消息拒绝机器人主动消息事件
/// </summary>
public class C2CMsgRejectEvent : QQBotEventBase
{
    public override string EventType => "C2C_MSG_REJECT";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("openid")]
    public string OpenId { get; set; } = string.Empty;
}

/// <summary>
/// C2C消息接受机器人主动消息事件
/// </summary>
public class C2CMsgReceiveEvent : QQBotEventBase
{
    public override string EventType => "C2C_MSG_RECEIVE";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("openid")]
    public string OpenId { get; set; } = string.Empty;
}

#endregion

#region 表情表态事件

/// <summary>
/// 表情表态添加事件
/// </summary>
public class MessageReactionAddEvent : QQBotEventBase
{
    public override string EventType => "MESSAGE_REACTION_ADD";
    public MessageReaction Reaction { get; set; } = new();
}

/// <summary>
/// 表情表态移除事件
/// </summary>
public class MessageReactionRemoveEvent : QQBotEventBase
{
    public override string EventType => "MESSAGE_REACTION_REMOVE";
    public MessageReaction Reaction { get; set; } = new();
}

#endregion

#region 互动事件

/// <summary>
/// 互动事件
/// </summary>
public class InteractionCreateEvent : QQBotEventBase
{
    public override string EventType => "INTERACTION_CREATE";
    public Interaction Interaction { get; set; } = new();
}

#endregion

#region 消息审核事件

/// <summary>
/// 消息审核通过事件
/// </summary>
public class MessageAuditPassEvent : QQBotEventBase
{
    public override string EventType => "MESSAGE_AUDIT_PASS";
    public MessageAudited MessageAudited { get; set; } = new();
}

/// <summary>
/// 消息审核不通过事件
/// </summary>
public class MessageAuditRejectEvent : QQBotEventBase
{
    public override string EventType => "MESSAGE_AUDIT_REJECT";
    public MessageAudited MessageAudited { get; set; } = new();
}

#endregion

#region 音频事件

/// <summary>
/// 音频开始播放事件
/// </summary>
public class AudioStartEvent : QQBotEventBase
{
    public override string EventType => "AUDIO_START";

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    [JsonPropertyName("audio_url")]
    public string? AudioUrl { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// 音频结束播放事件
/// </summary>
public class AudioFinishEvent : QQBotEventBase
{
    public override string EventType => "AUDIO_FINISH";

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;
}

/// <summary>
/// 机器人上麦事件
/// </summary>
public class AudioOnMicEvent : QQBotEventBase
{
    public override string EventType => "AUDIO_ON_MIC";

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;
}

/// <summary>
/// 机器人下麦事件
/// </summary>
public class AudioOffMicEvent : QQBotEventBase
{
    public override string EventType => "AUDIO_OFF_MIC";

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;
}

#endregion

#region 论坛事件

/// <summary>
/// 论坛帖子基类
/// </summary>
public abstract class ForumEventBase : QQBotEventBase
{
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("author_id")]
    public string AuthorId { get; set; } = string.Empty;
}

/// <summary>
/// 论坛主题创建事件
/// </summary>
public class ForumThreadCreateEvent : ForumEventBase
{
    public override string EventType => "FORUM_THREAD_CREATE";

    [JsonPropertyName("thread_info")]
    public ForumThreadInfo? ThreadInfo { get; set; }
}

/// <summary>
/// 论坛主题更新事件
/// </summary>
public class ForumThreadUpdateEvent : ForumEventBase
{
    public override string EventType => "FORUM_THREAD_UPDATE";

    [JsonPropertyName("thread_info")]
    public ForumThreadInfo? ThreadInfo { get; set; }
}

/// <summary>
/// 论坛主题删除事件
/// </summary>
public class ForumThreadDeleteEvent : ForumEventBase
{
    public override string EventType => "FORUM_THREAD_DELETE";

    [JsonPropertyName("thread_info")]
    public ForumThreadInfo? ThreadInfo { get; set; }
}

/// <summary>
/// 论坛帖子创建事件
/// </summary>
public class ForumPostCreateEvent : ForumEventBase
{
    public override string EventType => "FORUM_POST_CREATE";

    [JsonPropertyName("post_info")]
    public ForumPostInfo? PostInfo { get; set; }
}

/// <summary>
/// 论坛帖子删除事件
/// </summary>
public class ForumPostDeleteEvent : ForumEventBase
{
    public override string EventType => "FORUM_POST_DELETE";

    [JsonPropertyName("post_info")]
    public ForumPostInfo? PostInfo { get; set; }
}

/// <summary>
/// 论坛回复创建事件
/// </summary>
public class ForumReplyCreateEvent : ForumEventBase
{
    public override string EventType => "FORUM_REPLY_CREATE";

    [JsonPropertyName("reply_info")]
    public ForumReplyInfo? ReplyInfo { get; set; }
}

/// <summary>
/// 论坛回复删除事件
/// </summary>
public class ForumReplyDeleteEvent : ForumEventBase
{
    public override string EventType => "FORUM_REPLY_DELETE";

    [JsonPropertyName("reply_info")]
    public ForumReplyInfo? ReplyInfo { get; set; }
}

/// <summary>
/// 论坛主题审核通过事件
/// </summary>
public class ForumPublishAuditResultEvent : ForumEventBase
{
    public override string EventType => "FORUM_PUBLISH_AUDIT_RESULT";

    [JsonPropertyName("thread_info")]
    public ForumThreadInfo? ThreadInfo { get; set; }

    [JsonPropertyName("audit_result")]
    public int AuditResult { get; set; }
}

/// <summary>
/// 论坛主题信息
/// </summary>
public class ForumThreadInfo
{
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("date_time")]
    public DateTime DateTime { get; set; }
}

/// <summary>
/// 论坛帖子信息
/// </summary>
public class ForumPostInfo
{
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; set; } = string.Empty;

    [JsonPropertyName("post_id")]
    public string PostId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("date_time")]
    public DateTime DateTime { get; set; }
}

/// <summary>
/// 论坛回复信息
/// </summary>
public class ForumReplyInfo
{
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; set; } = string.Empty;

    [JsonPropertyName("post_id")]
    public string PostId { get; set; } = string.Empty;

    [JsonPropertyName("reply_id")]
    public string ReplyId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("date_time")]
    public DateTime DateTime { get; set; }
}

#endregion

#region 连接事件

/// <summary>
/// 连接就绪事件
/// </summary>
public class ReadyEvent : QQBotEventBase
{
    public override string EventType => "READY";

    public int Version { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public User? User { get; set; }
    public int[]? Shard { get; set; }
}

/// <summary>
/// 恢复成功事件
/// </summary>
public class ResumedEvent : QQBotEventBase
{
    public override string EventType => "RESUMED";
}

#endregion
