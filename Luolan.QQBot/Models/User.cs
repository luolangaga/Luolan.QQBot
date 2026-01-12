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
    /// 开放平台 union_openid
    /// </summary>
    [JsonPropertyName("union_openid")]
    public string? UnionOpenId { get; set; }

    /// <summary>
    /// 开放平台 union_user_account
    /// </summary>
    [JsonPropertyName("union_user_account")]
    public string? UnionUserAccount { get; set; }
}
