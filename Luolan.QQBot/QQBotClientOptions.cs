using System.Text.Json.Serialization;

namespace Luolan.QQBot;

/// <summary>
/// QQ机器人客户端配置选项
/// </summary>
public class QQBotClientOptions
{
    /// <summary>
    /// 机器人AppId
    /// </summary>
    public required string AppId { get; set; }

    /// <summary>
    /// 机器人ClientSecret
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// 是否为沙箱环境
    /// </summary>
    public bool IsSandbox { get; set; } = false;

    /// <summary>
    /// API基础URL
    /// </summary>
    public string ApiBaseUrl => IsSandbox
        ? "https://sandbox.api.sgroup.qq.com"
        : "https://api.sgroup.qq.com";

    /// <summary>
    /// 鉴权URL
    /// </summary>
    public string AuthUrl => "https://bots.qq.com/app/getAppAccessToken";

    /// <summary>
    /// Token过期前提前刷新的秒数
    /// </summary>
    public int TokenRefreshBeforeExpireSeconds { get; set; } = 60;

    /// <summary>
    /// WebSocket重连间隔(毫秒)
    /// </summary>
    public int WebSocketReconnectIntervalMs { get; set; } = 5000;

    /// <summary>
    /// WebSocket最大重连次数
    /// </summary>
    public int WebSocketMaxReconnectAttempts { get; set; } = 10;

    /// <summary>
    /// 订阅的事件类型(Intents)
    /// </summary>
    public Intents Intents { get; set; } = Intents.Default;
}

/// <summary>
/// 事件订阅类型
/// </summary>
[Flags]
public enum Intents
{
    None = 0,

    /// <summary>
    /// 频道相关事件
    /// </summary>
    Guilds = 1 << 0,

    /// <summary>
    /// 频道成员相关事件
    /// </summary>
    GuildMembers = 1 << 1,

    /// <summary>
    /// 频道消息事件(私域)
    /// </summary>
    GuildMessages = 1 << 9,

    /// <summary>
    /// 频道消息表情表态事件
    /// </summary>
    GuildMessageReactions = 1 << 10,

    /// <summary>
    /// 私信事件
    /// </summary>
    DirectMessage = 1 << 12,

    /// <summary>
    /// 消息审核事件
    /// </summary>
    MessageAudit = 1 << 27,

    /// <summary>
    /// 论坛事件(私域)
    /// </summary>
    Forums = 1 << 28,

    /// <summary>
    /// 音频事件
    /// </summary>
    AudioAction = 1 << 29,

    /// <summary>
    /// 公域消息事件
    /// </summary>
    PublicGuildMessages = 1 << 30,

    /// <summary>
    /// 互动事件
    /// </summary>
    Interaction = 1 << 26,

    /// <summary>
    /// 群聊@消息事件
    /// </summary>
    GroupAtMessages = 1 << 25,

    /// <summary>
    /// C2C消息事件
    /// </summary>
    C2CMessages = 1 << 25,

    /// <summary>
    /// 默认订阅(公域)
    /// </summary>
    Default = Guilds | GuildMembers | PublicGuildMessages | GuildMessageReactions | DirectMessage | Interaction | MessageAudit,

    /// <summary>
    /// 私域全部订阅
    /// </summary>
    PrivateAll = Guilds | GuildMembers | GuildMessages | GuildMessageReactions | DirectMessage | MessageAudit | Forums | AudioAction | Interaction
}
