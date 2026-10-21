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
    /// API基础URL。官方已于 2026 年切换至 api.bot.qq.com。
    /// </summary>
    public string ApiBaseUrl => !string.IsNullOrWhiteSpace(ApiBaseUrlOverride)
        ? ApiBaseUrlOverride.TrimEnd('/')
        : IsSandbox
            ? "https://sandbox.api.sgroup.qq.com"
            : "https://api.bot.qq.com";

    /// <summary>
    /// 自定义API基础URL(为空时使用官方地址)
    /// </summary>
    public string? ApiBaseUrlOverride { get; set; }

    /// <summary>
    /// 鉴权URL
    /// </summary>
    public string AuthUrl => !string.IsNullOrWhiteSpace(AuthUrlOverride)
        ? AuthUrlOverride
        : "https://api.bot.qq.com/app/getAppAccessToken";

    /// <summary>
    /// 自定义鉴权URL(为空时使用官方地址)
    /// </summary>
    public string? AuthUrlOverride { get; set; }

    /// <summary>
    /// 速率限制器容量(令牌桶容量)
    /// </summary>
    public int RateLimitCapacity { get; set; } = 100;

    /// <summary>
    /// 速率限制器每秒补充的令牌数
    /// </summary>
    public int RateLimitRefillPerSecond { get; set; } = 20;

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
    /// 群成员事件(群成员加入/退出、入群申请)
    /// </summary>
    GroupMembers = 1 << 24,

    /// <summary>
    /// 群与C2C消息事件(官方 GROUP_AND_C2C_EVENT)
    /// </summary>
    GroupAndC2C = 1 << 25,

    /// <summary>
    /// 群聊@消息事件
    /// </summary>
    GroupAtMessages = GroupAndC2C,

    /// <summary>
    /// C2C消息事件
    /// </summary>
    C2CMessages = GroupAndC2C,

    /// <summary>
    /// 默认订阅(公域)
    /// </summary>
    Default = Guilds | GuildMembers | PublicGuildMessages | GuildMessageReactions | DirectMessage | Interaction | MessageAudit,

    /// <summary>
    /// 私域全部订阅
    /// </summary>
    PrivateAll = Guilds | GuildMembers | GuildMessages | GuildMessageReactions | DirectMessage | MessageAudit | Forums | AudioAction | Interaction
}
