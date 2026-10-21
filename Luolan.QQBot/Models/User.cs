using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 用户信息
/// </summary>
public class User
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像URL
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 是否为机器人
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; set; }

    /// <summary>
    /// 用户的openid(群/C2C消息)
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string? MemberOpenId { get; set; }

    /// <summary>
    /// 用户的openid(C2C消息)
    /// </summary>
    [JsonPropertyName("user_openid")]
    public string? UserOpenId { get; set; }

    /// <summary>
    /// 跨应用统一用户OpenID(需特殊申请)
    /// </summary>
    [JsonPropertyName("union_openid")]
    public string? UnionOpenId { get; set; }

    /// <summary>
    /// 跨应用统一用户账号(需特殊申请)
    /// </summary>
    [JsonPropertyName("union_user_account")]
    public string? UnionUserAccount { get; set; }

    /// <summary>
    /// 群内角色: member/admin/owner
    /// </summary>
    [JsonPropertyName("member_role")]
    public string? MemberRole { get; set; }
}

/// <summary>
/// 频道成员
/// </summary>
public class Member
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// 用户昵称
    /// </summary>
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    /// <summary>
    /// 用户角色ID列表
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    [JsonPropertyName("joined_at")]
    public DateTime? JoinedAt { get; set; }
}

/// <summary>
/// 机器人信息
/// </summary>
public class BotInfo : User
{
    /// <summary>
    /// 机器人分享链接
    /// </summary>
    [JsonPropertyName("share_url")]
    public string? ShareUrl { get; set; }

    /// <summary>
    /// 机器人欢迎语
    /// </summary>
    [JsonPropertyName("welcome_msg")]
    public string? WelcomeMsg { get; set; }
}
