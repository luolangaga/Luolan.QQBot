using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 频道信息
/// </summary>
public class Guild
{
    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 频道名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 频道头像URL
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// 频道创建者ID
    /// </summary>
    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    /// <summary>
    /// 频道创建者标识(是否为创建者)
    /// </summary>
    [JsonPropertyName("owner")]
    public bool Owner { get; set; }

    /// <summary>
    /// 成员数
    /// </summary>
    [JsonPropertyName("member_count")]
    public int MemberCount { get; set; }

    /// <summary>
    /// 最大成员数
    /// </summary>
    [JsonPropertyName("max_members")]
    public int MaxMembers { get; set; }

    /// <summary>
    /// 频道描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    [JsonPropertyName("joined_at")]
    public DateTime? JoinedAt { get; set; }
}

/// <summary>
/// 子频道信息
/// </summary>
public class Channel
{
    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = string.Empty;

    /// <summary>
    /// 子频道名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 子频道类型
    /// </summary>
    [JsonPropertyName("type")]
    public ChannelType Type { get; set; }

    /// <summary>
    /// 子频道子类型
    /// </summary>
    [JsonPropertyName("sub_type")]
    public ChannelSubType SubType { get; set; }

    /// <summary>
    /// 排序位置
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>
    /// 父分组ID
    /// </summary>
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    /// <summary>
    /// 子频道私密类型
    /// </summary>
    [JsonPropertyName("private_type")]
    public int PrivateType { get; set; }

    /// <summary>
    /// 子频道发言权限
    /// </summary>
    [JsonPropertyName("speak_permission")]
    public int SpeakPermission { get; set; }

    /// <summary>
    /// 应用子频道的应用ID(仅应用子频道)
    /// </summary>
    [JsonPropertyName("application_id")]
    public string? ApplicationId { get; set; }

    /// <summary>
    /// 子频道操作权限
    /// </summary>
    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }
}

/// <summary>
/// 子频道类型
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// 文字子频道
    /// </summary>
    Text = 0,

    /// <summary>
    /// 保留类型
    /// </summary>
    Reserved1 = 1,

    /// <summary>
    /// 语音子频道
    /// </summary>
    Voice = 2,

    /// <summary>
    /// 保留类型
    /// </summary>
    Reserved2 = 3,

    /// <summary>
    /// 子频道分组
    /// </summary>
    Category = 4,

    /// <summary>
    /// 直播子频道
    /// </summary>
    Live = 10005,

    /// <summary>
    /// 应用子频道
    /// </summary>
    Application = 10006,

    /// <summary>
    /// 论坛子频道
    /// </summary>
    Forum = 10007
}

/// <summary>
/// 子频道子类型
/// </summary>
public enum ChannelSubType
{
    /// <summary>
    /// 闲聊
    /// </summary>
    Chat = 0,

    /// <summary>
    /// 公告
    /// </summary>
    Announcement = 1,

    /// <summary>
    /// 攻略
    /// </summary>
    Guide = 2,

    /// <summary>
    /// 开黑
    /// </summary>
    Gaming = 3
}
