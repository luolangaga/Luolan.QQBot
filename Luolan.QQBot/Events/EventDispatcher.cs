using System.Text.Json;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Events;

/// <summary>
/// 事件分发器
/// </summary>
public class EventDispatcher
{
    private readonly ILogger<EventDispatcher>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // 事件处理器委托
    public event Func<ReadyEvent, Task>? OnReady;
    public event Func<ResumedEvent, Task>? OnResumed;

    // 频道事件
    public event Func<GuildCreateEvent, Task>? OnGuildCreate;
    public event Func<GuildUpdateEvent, Task>? OnGuildUpdate;
    public event Func<GuildDeleteEvent, Task>? OnGuildDelete;
    public event Func<ChannelCreateEvent, Task>? OnChannelCreate;
    public event Func<ChannelUpdateEvent, Task>? OnChannelUpdate;
    public event Func<ChannelDeleteEvent, Task>? OnChannelDelete;

    // 成员事件
    public event Func<GuildMemberAddEvent, Task>? OnGuildMemberAdd;
    public event Func<GuildMemberUpdateEvent, Task>? OnGuildMemberUpdate;
    public event Func<GuildMemberRemoveEvent, Task>? OnGuildMemberRemove;

    // 频道消息事件
    public event Func<MessageCreateEvent, Task>? OnMessageCreate;
    public event Func<MessageDeleteEvent, Task>? OnMessageDelete;
    public event Func<AtMessageCreateEvent, Task>? OnAtMessageCreate;
    public event Func<PublicMessageDeleteEvent, Task>? OnPublicMessageDelete;
    public event Func<DirectMessageCreateEvent, Task>? OnDirectMessageCreate;
    public event Func<DirectMessageDeleteEvent, Task>? OnDirectMessageDelete;

    // 群/C2C消息事件
    public event Func<GroupAtMessageCreateEvent, Task>? OnGroupAtMessageCreate;
    public event Func<C2CMessageCreateEvent, Task>? OnC2CMessageCreate;

    // 群/C2C机器人事件
    public event Func<GroupAddRobotEvent, Task>? OnGroupAddRobot;
    public event Func<GroupDelRobotEvent, Task>? OnGroupDelRobot;
    public event Func<GroupMsgRejectEvent, Task>? OnGroupMsgReject;
    public event Func<GroupMsgReceiveEvent, Task>? OnGroupMsgReceive;
    public event Func<FriendAddEvent, Task>? OnFriendAdd;
    public event Func<FriendDelEvent, Task>? OnFriendDel;
    public event Func<C2CMsgRejectEvent, Task>? OnC2CMsgReject;
    public event Func<C2CMsgReceiveEvent, Task>? OnC2CMsgReceive;

    // 表情表态事件
    public event Func<MessageReactionAddEvent, Task>? OnMessageReactionAdd;
    public event Func<MessageReactionRemoveEvent, Task>? OnMessageReactionRemove;

    // 互动事件
    public event Func<InteractionCreateEvent, Task>? OnInteractionCreate;

    // 消息审核事件
    public event Func<MessageAuditPassEvent, Task>? OnMessageAuditPass;
    public event Func<MessageAuditRejectEvent, Task>? OnMessageAuditReject;

    // 音频事件
    public event Func<AudioStartEvent, Task>? OnAudioStart;
    public event Func<AudioFinishEvent, Task>? OnAudioFinish;
    public event Func<AudioOnMicEvent, Task>? OnAudioOnMic;
    public event Func<AudioOffMicEvent, Task>? OnAudioOffMic;

    // 论坛事件
    public event Func<ForumThreadCreateEvent, Task>? OnForumThreadCreate;
    public event Func<ForumThreadUpdateEvent, Task>? OnForumThreadUpdate;
    public event Func<ForumThreadDeleteEvent, Task>? OnForumThreadDelete;
    public event Func<ForumPostCreateEvent, Task>? OnForumPostCreate;
    public event Func<ForumPostDeleteEvent, Task>? OnForumPostDelete;
    public event Func<ForumReplyCreateEvent, Task>? OnForumReplyCreate;
    public event Func<ForumReplyDeleteEvent, Task>? OnForumReplyDelete;
    public event Func<ForumPublishAuditResultEvent, Task>? OnForumPublishAuditResult;

    // 通用事件处理器(处理所有原始事件)
    public event Func<string, JsonElement, Task>? OnRawEvent;

    public EventDispatcher(ILogger<EventDispatcher>? logger = null)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    public async Task DispatchAsync(string eventType, JsonElement data, string? eventId = null)
    {
        _logger?.LogDebug("收到事件: {EventType}", eventType);

        try
        {
            // 触发原始事件处理器
            if (OnRawEvent != null)
            {
                await OnRawEvent.Invoke(eventType, data);
            }

            switch (eventType)
            {
                // 连接事件
                case "READY":
                    await DispatchEventAsync(OnReady, () => ParseReadyEvent(data, eventId));
                    break;
                case "RESUMED":
                    await DispatchEventAsync(OnResumed, () => new ResumedEvent { EventId = eventId, RawData = data });
                    break;

                // 频道事件
                case "GUILD_CREATE":
                    await DispatchEventAsync(OnGuildCreate, () => ParseGuildEvent<GuildCreateEvent>(data, eventId));
                    break;
                case "GUILD_UPDATE":
                    await DispatchEventAsync(OnGuildUpdate, () => ParseGuildEvent<GuildUpdateEvent>(data, eventId));
                    break;
                case "GUILD_DELETE":
                    await DispatchEventAsync(OnGuildDelete, () => ParseGuildEvent<GuildDeleteEvent>(data, eventId));
                    break;
                case "CHANNEL_CREATE":
                    await DispatchEventAsync(OnChannelCreate, () => ParseChannelEvent<ChannelCreateEvent>(data, eventId));
                    break;
                case "CHANNEL_UPDATE":
                    await DispatchEventAsync(OnChannelUpdate, () => ParseChannelEvent<ChannelUpdateEvent>(data, eventId));
                    break;
                case "CHANNEL_DELETE":
                    await DispatchEventAsync(OnChannelDelete, () => ParseChannelEvent<ChannelDeleteEvent>(data, eventId));
                    break;

                // 成员事件
                case "GUILD_MEMBER_ADD":
                    await DispatchEventAsync(OnGuildMemberAdd, () => ParseMemberEvent<GuildMemberAddEvent>(data, eventId));
                    break;
                case "GUILD_MEMBER_UPDATE":
                    await DispatchEventAsync(OnGuildMemberUpdate, () => ParseMemberEvent<GuildMemberUpdateEvent>(data, eventId));
                    break;
                case "GUILD_MEMBER_REMOVE":
                    await DispatchEventAsync(OnGuildMemberRemove, () => ParseMemberEvent<GuildMemberRemoveEvent>(data, eventId));
                    break;

                // 频道消息事件
                case "MESSAGE_CREATE":
                    await DispatchEventAsync(OnMessageCreate, () => ParseMessageEvent<MessageCreateEvent>(data, eventId));
                    break;
                case "MESSAGE_DELETE":
                    await DispatchEventAsync(OnMessageDelete, () => ParseDeletedMessageEvent<MessageDeleteEvent>(data, eventId));
                    break;
                case "AT_MESSAGE_CREATE":
                    await DispatchEventAsync(OnAtMessageCreate, () => ParseMessageEvent<AtMessageCreateEvent>(data, eventId));
                    break;
                case "PUBLIC_MESSAGE_DELETE":
                    await DispatchEventAsync(OnPublicMessageDelete, () => ParseDeletedMessageEvent<PublicMessageDeleteEvent>(data, eventId));
                    break;
                case "DIRECT_MESSAGE_CREATE":
                    await DispatchEventAsync(OnDirectMessageCreate, () => ParseMessageEvent<DirectMessageCreateEvent>(data, eventId));
                    break;
                case "DIRECT_MESSAGE_DELETE":
                    await DispatchEventAsync(OnDirectMessageDelete, () => ParseDeletedMessageEvent<DirectMessageDeleteEvent>(data, eventId));
                    break;

                // 群/C2C消息事件
                case "GROUP_AT_MESSAGE_CREATE":
                    await DispatchEventAsync(OnGroupAtMessageCreate, () => ParseMessageEvent<GroupAtMessageCreateEvent>(data, eventId));
                    break;
                case "C2C_MESSAGE_CREATE":
                    await DispatchEventAsync(OnC2CMessageCreate, () => ParseMessageEvent<C2CMessageCreateEvent>(data, eventId));
                    break;

                // 群/C2C机器人事件
                case "GROUP_ADD_ROBOT":
                    await DispatchEventAsync(OnGroupAddRobot, () => data.Deserialize<GroupAddRobotEvent>(_jsonOptions) ?? new GroupAddRobotEvent { EventId = eventId, RawData = data });
                    break;
                case "GROUP_DEL_ROBOT":
                    await DispatchEventAsync(OnGroupDelRobot, () => data.Deserialize<GroupDelRobotEvent>(_jsonOptions) ?? new GroupDelRobotEvent { EventId = eventId, RawData = data });
                    break;
                case "GROUP_MSG_REJECT":
                    await DispatchEventAsync(OnGroupMsgReject, () => data.Deserialize<GroupMsgRejectEvent>(_jsonOptions) ?? new GroupMsgRejectEvent { EventId = eventId, RawData = data });
                    break;
                case "GROUP_MSG_RECEIVE":
                    await DispatchEventAsync(OnGroupMsgReceive, () => data.Deserialize<GroupMsgReceiveEvent>(_jsonOptions) ?? new GroupMsgReceiveEvent { EventId = eventId, RawData = data });
                    break;
                case "FRIEND_ADD":
                    await DispatchEventAsync(OnFriendAdd, () => data.Deserialize<FriendAddEvent>(_jsonOptions) ?? new FriendAddEvent { EventId = eventId, RawData = data });
                    break;
                case "FRIEND_DEL":
                    await DispatchEventAsync(OnFriendDel, () => data.Deserialize<FriendDelEvent>(_jsonOptions) ?? new FriendDelEvent { EventId = eventId, RawData = data });
                    break;
                case "C2C_MSG_REJECT":
                    await DispatchEventAsync(OnC2CMsgReject, () => data.Deserialize<C2CMsgRejectEvent>(_jsonOptions) ?? new C2CMsgRejectEvent { EventId = eventId, RawData = data });
                    break;
                case "C2C_MSG_RECEIVE":
                    await DispatchEventAsync(OnC2CMsgReceive, () => data.Deserialize<C2CMsgReceiveEvent>(_jsonOptions) ?? new C2CMsgReceiveEvent { EventId = eventId, RawData = data });
                    break;

                // 表情表态事件
                case "MESSAGE_REACTION_ADD":
                    await DispatchEventAsync(OnMessageReactionAdd, () => ParseReactionEvent<MessageReactionAddEvent>(data, eventId));
                    break;
                case "MESSAGE_REACTION_REMOVE":
                    await DispatchEventAsync(OnMessageReactionRemove, () => ParseReactionEvent<MessageReactionRemoveEvent>(data, eventId));
                    break;

                // 互动事件
                case "INTERACTION_CREATE":
                    await DispatchEventAsync(OnInteractionCreate, () => ParseInteractionEvent(data, eventId));
                    break;

                // 消息审核事件
                case "MESSAGE_AUDIT_PASS":
                    await DispatchEventAsync(OnMessageAuditPass, () => ParseAuditEvent<MessageAuditPassEvent>(data, eventId));
                    break;
                case "MESSAGE_AUDIT_REJECT":
                    await DispatchEventAsync(OnMessageAuditReject, () => ParseAuditEvent<MessageAuditRejectEvent>(data, eventId));
                    break;

                // 音频事件
                case "AUDIO_START":
                    await DispatchEventAsync(OnAudioStart, () => data.Deserialize<AudioStartEvent>(_jsonOptions) ?? new AudioStartEvent { EventId = eventId, RawData = data });
                    break;
                case "AUDIO_FINISH":
                    await DispatchEventAsync(OnAudioFinish, () => data.Deserialize<AudioFinishEvent>(_jsonOptions) ?? new AudioFinishEvent { EventId = eventId, RawData = data });
                    break;
                case "AUDIO_ON_MIC":
                    await DispatchEventAsync(OnAudioOnMic, () => data.Deserialize<AudioOnMicEvent>(_jsonOptions) ?? new AudioOnMicEvent { EventId = eventId, RawData = data });
                    break;
                case "AUDIO_OFF_MIC":
                    await DispatchEventAsync(OnAudioOffMic, () => data.Deserialize<AudioOffMicEvent>(_jsonOptions) ?? new AudioOffMicEvent { EventId = eventId, RawData = data });
                    break;

                // 论坛事件
                case "FORUM_THREAD_CREATE":
                    await DispatchEventAsync(OnForumThreadCreate, () => data.Deserialize<ForumThreadCreateEvent>(_jsonOptions) ?? new ForumThreadCreateEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_THREAD_UPDATE":
                    await DispatchEventAsync(OnForumThreadUpdate, () => data.Deserialize<ForumThreadUpdateEvent>(_jsonOptions) ?? new ForumThreadUpdateEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_THREAD_DELETE":
                    await DispatchEventAsync(OnForumThreadDelete, () => data.Deserialize<ForumThreadDeleteEvent>(_jsonOptions) ?? new ForumThreadDeleteEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_POST_CREATE":
                    await DispatchEventAsync(OnForumPostCreate, () => data.Deserialize<ForumPostCreateEvent>(_jsonOptions) ?? new ForumPostCreateEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_POST_DELETE":
                    await DispatchEventAsync(OnForumPostDelete, () => data.Deserialize<ForumPostDeleteEvent>(_jsonOptions) ?? new ForumPostDeleteEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_REPLY_CREATE":
                    await DispatchEventAsync(OnForumReplyCreate, () => data.Deserialize<ForumReplyCreateEvent>(_jsonOptions) ?? new ForumReplyCreateEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_REPLY_DELETE":
                    await DispatchEventAsync(OnForumReplyDelete, () => data.Deserialize<ForumReplyDeleteEvent>(_jsonOptions) ?? new ForumReplyDeleteEvent { EventId = eventId, RawData = data });
                    break;
                case "FORUM_PUBLISH_AUDIT_RESULT":
                    await DispatchEventAsync(OnForumPublishAuditResult, () => data.Deserialize<ForumPublishAuditResultEvent>(_jsonOptions) ?? new ForumPublishAuditResultEvent { EventId = eventId, RawData = data });
                    break;

                default:
                    _logger?.LogWarning("未处理的事件类型: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "处理事件 {EventType} 时发生异常", eventType);
        }
    }

    private async Task DispatchEventAsync<T>(Func<T, Task>? handler, Func<T> eventFactory) where T : QQBotEventBase
    {
        if (handler == null) return;

        var evt = eventFactory();
        await handler.Invoke(evt);
    }

    private ReadyEvent ParseReadyEvent(JsonElement data, string? eventId)
    {
        var readyData = data.Deserialize<ReadyData>(_jsonOptions);
        return new ReadyEvent
        {
            EventId = eventId,
            RawData = data,
            Version = readyData?.Version ?? 0,
            SessionId = readyData?.SessionId ?? string.Empty,
            User = readyData?.User,
            Shard = readyData?.Shard
        };
    }

    private T ParseGuildEvent<T>(JsonElement data, string? eventId) where T : QQBotEventBase, new()
    {
        var guild = data.Deserialize<Guild>(_jsonOptions);
        string? opUserId = null;
        if (data.TryGetProperty("op_user_id", out var opUserProp))
            opUserId = opUserProp.GetString();

        var evt = new T { EventId = eventId, RawData = data };
        if (evt is GuildCreateEvent gce) { gce.Guild = guild ?? new Guild(); gce.OpUserId = opUserId; }
        else if (evt is GuildUpdateEvent gue) { gue.Guild = guild ?? new Guild(); gue.OpUserId = opUserId; }
        else if (evt is GuildDeleteEvent gde) { gde.Guild = guild ?? new Guild(); gde.OpUserId = opUserId; }
        return evt;
    }

    private T ParseChannelEvent<T>(JsonElement data, string? eventId) where T : QQBotEventBase, new()
    {
        var channel = data.Deserialize<Channel>(_jsonOptions);
        string? opUserId = null;
        if (data.TryGetProperty("op_user_id", out var opUserProp))
            opUserId = opUserProp.GetString();

        var evt = new T { EventId = eventId, RawData = data };
        if (evt is ChannelCreateEvent cce) { cce.Channel = channel ?? new Channel(); cce.OpUserId = opUserId; }
        else if (evt is ChannelUpdateEvent cue) { cue.Channel = channel ?? new Channel(); cue.OpUserId = opUserId; }
        else if (evt is ChannelDeleteEvent cde) { cde.Channel = channel ?? new Channel(); cde.OpUserId = opUserId; }
        return evt;
    }

    private T ParseMemberEvent<T>(JsonElement data, string? eventId) where T : QQBotEventBase, new()
    {
        var member = data.Deserialize<Member>(_jsonOptions);
        string guildId = string.Empty;
        string? opUserId = null;

        if (data.TryGetProperty("guild_id", out var guildProp))
            guildId = guildProp.GetString() ?? string.Empty;
        if (data.TryGetProperty("op_user_id", out var opUserProp))
            opUserId = opUserProp.GetString();

        var evt = new T { EventId = eventId, RawData = data };
        if (evt is GuildMemberAddEvent gma) { gma.Member = member ?? new Member(); gma.GuildId = guildId; gma.OpUserId = opUserId; }
        else if (evt is GuildMemberUpdateEvent gmu) { gmu.Member = member ?? new Member(); gmu.GuildId = guildId; gmu.OpUserId = opUserId; }
        else if (evt is GuildMemberRemoveEvent gmr) { gmr.Member = member ?? new Member(); gmr.GuildId = guildId; gmr.OpUserId = opUserId; }
        return evt;
    }

    private T ParseMessageEvent<T>(JsonElement data, string? eventId) where T : MessageEventBase, new()
    {
        var message = data.Deserialize<Message>(_jsonOptions);
        return new T
        {
            EventId = eventId,
            RawData = data,
            Message = message ?? new Message()
        };
    }

    private T ParseDeletedMessageEvent<T>(JsonElement data, string? eventId) where T : QQBotEventBase, new()
    {
        var deletedMsg = data.Deserialize<DeletedMessage>(_jsonOptions);
        string? opUserId = null;
        if (data.TryGetProperty("op_user_id", out var opUserProp))
            opUserId = opUserProp.GetString();

        var evt = new T { EventId = eventId, RawData = data };
        if (evt is MessageDeleteEvent mde) { mde.Message = deletedMsg ?? new DeletedMessage(); mde.OpUserId = opUserId; }
        else if (evt is PublicMessageDeleteEvent pmde) { pmde.Message = deletedMsg ?? new DeletedMessage(); pmde.OpUserId = opUserId; }
        else if (evt is DirectMessageDeleteEvent dmde) { dmde.Message = deletedMsg ?? new DeletedMessage(); dmde.OpUserId = opUserId; }
        return evt;
    }

    private T ParseReactionEvent<T>(JsonElement data, string? eventId) where T : QQBotEventBase, new()
    {
        var reaction = data.Deserialize<MessageReaction>(_jsonOptions);
        var evt = new T { EventId = eventId, RawData = data };
        if (evt is MessageReactionAddEvent mra) { mra.Reaction = reaction ?? new MessageReaction(); }
        else if (evt is MessageReactionRemoveEvent mrr) { mrr.Reaction = reaction ?? new MessageReaction(); }
        return evt;
    }

    private InteractionCreateEvent ParseInteractionEvent(JsonElement data, string? eventId)
    {
        var interaction = data.Deserialize<Interaction>(_jsonOptions);
        return new InteractionCreateEvent
        {
            EventId = eventId,
            RawData = data,
            Interaction = interaction ?? new Interaction()
        };
    }

    private T ParseAuditEvent<T>(JsonElement data, string? eventId) where T : QQBotEventBase, new()
    {
        var audit = data.Deserialize<MessageAudited>(_jsonOptions);
        var evt = new T { EventId = eventId, RawData = data };
        if (evt is MessageAuditPassEvent map) { map.MessageAudited = audit ?? new MessageAudited(); }
        else if (evt is MessageAuditRejectEvent mar) { mar.MessageAudited = audit ?? new MessageAudited(); }
        return evt;
    }
}
