using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// WebSocket网关信息
/// </summary>
public class GatewayInfo
{
    /// <summary>
    /// WebSocket URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// 带分片的WebSocket网关信息
/// </summary>
public class GatewayBotInfo : GatewayInfo
{
    /// <summary>
    /// 建议分片数
    /// </summary>
    [JsonPropertyName("shards")]
    public int Shards { get; set; }

    /// <summary>
    /// 会话限制
    /// </summary>
    [JsonPropertyName("session_start_limit")]
    public SessionStartLimit? SessionStartLimit { get; set; }
}

/// <summary>
/// 会话启动限制
/// </summary>
public class SessionStartLimit
{
    /// <summary>
    /// 每24小时可创建的Session数
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 剩余的Session数
    /// </summary>
    [JsonPropertyName("remaining")]
    public int Remaining { get; set; }

    /// <summary>
    /// 重置计数的剩余时间(ms)
    /// </summary>
    [JsonPropertyName("reset_after")]
    public int ResetAfter { get; set; }

    /// <summary>
    /// 每5秒可创建的Session数
    /// </summary>
    [JsonPropertyName("max_concurrency")]
    public int MaxConcurrency { get; set; }
}

/// <summary>
/// WebSocket负载
/// </summary>
public class WebSocketPayload
{
    /// <summary>
    /// 操作码
    /// </summary>
    [JsonPropertyName("op")]
    public OpCode Op { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("d")]
    public object? Data { get; set; }

    /// <summary>
    /// 序列号(用于心跳)
    /// </summary>
    [JsonPropertyName("s")]
    public int? Sequence { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [JsonPropertyName("t")]
    public string? Type { get; set; }

    /// <summary>
    /// 消息ID(用于回调)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// WebSocket操作码
/// </summary>
public enum OpCode
{
    /// <summary>
    /// 服务端推送事件
    /// </summary>
    Dispatch = 0,

    /// <summary>
    /// 客户端发送心跳
    /// </summary>
    Heartbeat = 1,

    /// <summary>
    /// 客户端发送鉴权
    /// </summary>
    Identify = 2,

    /// <summary>
    /// 客户端恢复会话
    /// </summary>
    Resume = 6,

    /// <summary>
    /// 服务端要求重连
    /// </summary>
    Reconnect = 7,

    /// <summary>
    /// 服务端拒绝连接(无效Session)
    /// </summary>
    InvalidSession = 9,

    /// <summary>
    /// 服务端发送Hello(包含心跳周期)
    /// </summary>
    Hello = 10,

    /// <summary>
    /// 服务端确认心跳
    /// </summary>
    HeartbeatAck = 11,

    /// <summary>
    /// 服务端要求客户端发送HTTP回调ACK
    /// </summary>
    HttpCallbackAck = 12
}

/// <summary>
/// 鉴权数据
/// </summary>
public class IdentifyData
{
    /// <summary>
    /// 鉴权令牌
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 订阅的事件
    /// </summary>
    [JsonPropertyName("intents")]
    public int Intents { get; set; }

    /// <summary>
    /// 分片信息 [当前分片, 总分片数]
    /// </summary>
    [JsonPropertyName("shard")]
    public int[]? Shard { get; set; }

    /// <summary>
    /// 客户端属性
    /// </summary>
    [JsonPropertyName("properties")]
    public IdentifyProperties? Properties { get; set; }
}

/// <summary>
/// 客户端属性
/// </summary>
public class IdentifyProperties
{
    /// <summary>
    /// 操作系统
    /// </summary>
    [JsonPropertyName("$os")]
    public string Os { get; set; } = Environment.OSVersion.Platform.ToString();

    /// <summary>
    /// 浏览器/库名
    /// </summary>
    [JsonPropertyName("$browser")]
    public string Browser { get; set; } = "Luolan.QQBot";

    /// <summary>
    /// 设备
    /// </summary>
    [JsonPropertyName("$device")]
    public string Device { get; set; } = "Luolan.QQBot";
}

/// <summary>
/// 恢复会话数据
/// </summary>
public class ResumeData
{
    /// <summary>
    /// 鉴权令牌
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 最后收到的序列号
    /// </summary>
    [JsonPropertyName("seq")]
    public int Seq { get; set; }
}

/// <summary>
/// Hello事件数据
/// </summary>
public class HelloData
{
    /// <summary>
    /// 心跳周期(毫秒)
    /// </summary>
    [JsonPropertyName("heartbeat_interval")]
    public int HeartbeatInterval { get; set; }
}

/// <summary>
/// Ready事件数据
/// </summary>
public class ReadyData
{
    /// <summary>
    /// 协议版本
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 当前机器人信息
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// 分片信息
    /// </summary>
    [JsonPropertyName("shard")]
    public int[]? Shard { get; set; }
}

/// <summary>
/// WebSocket会话状态
/// </summary>
public class WebSocketSession
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 最后序列号
    /// </summary>
    public int LastSequence { get; set; }

    /// <summary>
    /// 心跳周期(毫秒)
    /// </summary>
    public int HeartbeatInterval { get; set; }

    /// <summary>
    /// 是否已鉴权
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// 网关URL
    /// </summary>
    public string GatewayUrl { get; set; } = string.Empty;

    /// <summary>
    /// 机器人用户信息
    /// </summary>
    public User? BotUser { get; set; }
}
