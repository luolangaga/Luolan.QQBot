using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 角色信息
/// </summary>
public class Role
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 角色名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色颜色(ARGB十六进制)
    /// </summary>
    [JsonPropertyName("color")]
    public uint Color { get; set; }

    /// <summary>
    /// 是否在成员列表中单独展示
    /// </summary>
    [JsonPropertyName("hoist")]
    public int Hoist { get; set; }

    /// <summary>
    /// 拥有此角色的成员数量
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }

    /// <summary>
    /// 角色成员上限
    /// </summary>
    [JsonPropertyName("member_limit")]
    public int MemberLimit { get; set; }
}

/// <summary>
/// 角色列表响应
/// </summary>
public class GetGuildRolesResponse
{
    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    /// <summary>
    /// 角色列表
    /// </summary>
    [JsonPropertyName("roles")]
    public List<Role>? Roles { get; set; }

    /// <summary>
    /// 默认角色上限
    /// </summary>
    [JsonPropertyName("role_num_limit")]
    public string? RoleNumLimit { get; set; }
}

/// <summary>
/// 创建角色请求
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 角色颜色(ARGB十六进制)
    /// </summary>
    [JsonPropertyName("color")]
    public uint? Color { get; set; }

    /// <summary>
    /// 是否在成员列表中单独展示
    /// </summary>
    [JsonPropertyName("hoist")]
    public int? Hoist { get; set; }
}

/// <summary>
/// 创建角色响应
/// </summary>
public class CreateRoleResponse
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [JsonPropertyName("role_id")]
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// 角色信息
    /// </summary>
    [JsonPropertyName("role")]
    public Role? Role { get; set; }
}

/// <summary>
/// 频道权限
/// </summary>
public class ChannelPermissions
{
    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID(查询用户权限时)
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// 角色ID(查询角色权限时)
    /// </summary>
    [JsonPropertyName("role_id")]
    public string? RoleId { get; set; }

    /// <summary>
    /// 权限值
    /// </summary>
    [JsonPropertyName("permissions")]
    public string Permissions { get; set; } = "0";
}

/// <summary>
/// 修改权限请求
/// </summary>
public class UpdateChannelPermissionsRequest
{
    /// <summary>
    /// 添加的权限
    /// </summary>
    [JsonPropertyName("add")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Add { get; set; }

    /// <summary>
    /// 移除的权限
    /// </summary>
    [JsonPropertyName("remove")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Remove { get; set; }
}

/// <summary>
/// 公告对象
/// </summary>
public class Announces
{
    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// 公告类型 0:成员公告 1:欢迎公告
    /// </summary>
    [JsonPropertyName("announces_type")]
    public int AnnouncesType { get; set; }

    /// <summary>
    /// 推荐子频道列表
    /// </summary>
    [JsonPropertyName("recommend_channels")]
    public List<RecommendChannel>? RecommendChannels { get; set; }
}

/// <summary>
/// 推荐子频道
/// </summary>
public class RecommendChannel
{
    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// 推荐语
    /// </summary>
    [JsonPropertyName("introduce")]
    public string? Introduce { get; set; }
}

/// <summary>
/// 日程对象
/// </summary>
public class Schedule
{
    /// <summary>
    /// 日程ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 日程名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 日程描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 日程开始时间戳(ms)
    /// </summary>
    [JsonPropertyName("start_timestamp")]
    public string StartTimestamp { get; set; } = string.Empty;

    /// <summary>
    /// 日程结束时间戳(ms)
    /// </summary>
    [JsonPropertyName("end_timestamp")]
    public string EndTimestamp { get; set; } = string.Empty;

    /// <summary>
    /// 创建者
    /// </summary>
    [JsonPropertyName("creator")]
    public Member? Creator { get; set; }

    /// <summary>
    /// 跳转子频道ID
    /// </summary>
    [JsonPropertyName("jump_channel_id")]
    public string? JumpChannelId { get; set; }

    /// <summary>
    /// 提醒类型
    /// </summary>
    [JsonPropertyName("remind_type")]
    public string RemindType { get; set; } = "0";
}

/// <summary>
/// 精华消息
/// </summary>
public class PinsMessage
{
    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// 精华消息ID列表
    /// </summary>
    [JsonPropertyName("message_ids")]
    public List<string>? MessageIds { get; set; }
}

/// <summary>
/// 表情表态对象
/// </summary>
public class MessageReaction
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// 目标消息/帖子
    /// </summary>
    [JsonPropertyName("target")]
    public ReactionTarget? Target { get; set; }

    /// <summary>
    /// 表态的表情
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji? Emoji { get; set; }
}

/// <summary>
/// 表态目标
/// </summary>
public class ReactionTarget
{
    /// <summary>
    /// 目标ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 目标类型 0:消息 1:帖子 2:评论 3:回复
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }
}

/// <summary>
/// 表情对象
/// </summary>
public class Emoji
{
    /// <summary>
    /// 表情ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 表情类型 1:系统表情 2:emoji表情
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }
}

/// <summary>
/// 互动事件
/// </summary>
public class Interaction
{
    /// <summary>
    /// 互动ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 应用ID
    /// </summary>
    [JsonPropertyName("application_id")]
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// 互动类型
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 互动数据
    /// </summary>
    [JsonPropertyName("data")]
    public InteractionData? Data { get; set; }

    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// 群openid
    /// </summary>
    [JsonPropertyName("group_openid")]
    public string? GroupOpenId { get; set; }

    /// <summary>
    /// 群成员openid
    /// </summary>
    [JsonPropertyName("group_member_openid")]
    public string? GroupMemberOpenId { get; set; }

    /// <summary>
    /// 聊天类型 0私聊 1群聊 2频道
    /// </summary>
    [JsonPropertyName("chat_type")]
    public int ChatType { get; set; }

    /// <summary>
    /// 场景(群/C2C) group/c2c
    /// </summary>
    [JsonPropertyName("scene")]
    public string? Scene { get; set; }

    /// <summary>
    /// 用户openid
    /// </summary>
    [JsonPropertyName("user_openid")]
    public string? UserOpenId { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}

/// <summary>
/// 互动数据
/// </summary>
public class InteractionData
{
    /// <summary>
    /// 按钮数据
    /// </summary>
    [JsonPropertyName("resolved")]
    public InteractionResolved? Resolved { get; set; }
}

/// <summary>
/// 互动解析数据
/// </summary>
public class InteractionResolved
{
    /// <summary>
    /// 按钮ID
    /// </summary>
    [JsonPropertyName("button_id")]
    public string? ButtonId { get; set; }

    /// <summary>
    /// 按钮数据
    /// </summary>
    [JsonPropertyName("button_data")]
    public string? ButtonData { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// 特性ID(群/C2C场景)
    /// </summary>
    [JsonPropertyName("feature_id")]
    public string? FeatureId { get; set; }
}

/// <summary>
/// 消息审核对象
/// </summary>
public class MessageAudited
{
    /// <summary>
    /// 审核ID
    /// </summary>
    [JsonPropertyName("audit_id")]
    public string AuditId { get; set; } = string.Empty;

    /// <summary>
    /// 消息ID(审核通过后有值)
    /// </summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// 审核时间
    /// </summary>
    [JsonPropertyName("audit_time")]
    public DateTime AuditTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("create_time")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 消息序号
    /// </summary>
    [JsonPropertyName("seq_in_channel")]
    public string? SeqInChannel { get; set; }
}

/// <summary>
/// API错误响应
/// </summary>
public class ApiError
{
    /// <summary>
    /// 错误码
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 追踪ID
    /// </summary>
    [JsonPropertyName("trace_id")]
    public string? TraceId { get; set; }

    /// <summary>
    /// 错误数据
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
