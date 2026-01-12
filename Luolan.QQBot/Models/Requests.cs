using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 发送消息请求
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// 消息内容(选填)
    /// </summary>
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    /// <summary>
    /// Embed消息(选填)
    /// </summary>
    [JsonPropertyName("embed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageEmbed? Embed { get; set; }

    /// <summary>
    /// Ark消息(选填)
    /// </summary>
    [JsonPropertyName("ark")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageArk? Ark { get; set; }

    /// <summary>
    /// 引用消息(选填)
    /// </summary>
    [JsonPropertyName("message_reference")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageReference? MessageReference { get; set; }

    /// <summary>
    /// 图片URL(选填)
    /// </summary>
    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; set; }

    /// <summary>
    /// 被动消息ID(选填)
    /// </summary>
    [JsonPropertyName("msg_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MsgId { get; set; }

    /// <summary>
    /// 事件ID(选填,用于被动消息)
    /// </summary>
    [JsonPropertyName("event_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EventId { get; set; }

    /// <summary>
    /// Markdown消息(选填)
    /// </summary>
    [JsonPropertyName("markdown")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageMarkdown? Markdown { get; set; }

    /// <summary>
    /// 按钮键盘(选填)
    /// </summary>
    [JsonPropertyName("keyboard")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageKeyboard? Keyboard { get; set; }
}

/// <summary>
/// 群/C2C消息请求
/// </summary>
public class SendGroupMessageRequest
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    /// <summary>
    /// 消息类型 0:文本 2:markdown 3:ark 4:embed 7:富媒体
    /// </summary>
    [JsonPropertyName("msg_type")]
    public int MsgType { get; set; }

    /// <summary>
    /// 被动消息ID
    /// </summary>
    [JsonPropertyName("msg_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MsgId { get; set; }

    /// <summary>
    /// 前置收到的事件ID
    /// </summary>
    [JsonPropertyName("event_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EventId { get; set; }

    /// <summary>
    /// Markdown消息
    /// </summary>
    [JsonPropertyName("markdown")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageMarkdown? Markdown { get; set; }

    /// <summary>
    /// 按钮键盘
    /// </summary>
    [JsonPropertyName("keyboard")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageKeyboard? Keyboard { get; set; }

    /// <summary>
    /// Ark消息
    /// </summary>
    [JsonPropertyName("ark")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageArk? Ark { get; set; }

    /// <summary>
    /// 富媒体消息
    /// </summary>
    [JsonPropertyName("media")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MediaInfo? Media { get; set; }

    /// <summary>
    /// 消息序号
    /// </summary>
    [JsonPropertyName("msg_seq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MsgSeq { get; set; }
}

/// <summary>
/// 群/C2C消息响应
/// </summary>
public class SendGroupMessageResponse
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 发送时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}

/// <summary>
/// 富媒体信息
/// </summary>
public class MediaInfo
{
    /// <summary>
    /// 文件信息(文件上传后返回的file_info)
    /// </summary>
    [JsonPropertyName("file_info")]
    public string? FileInfo { get; set; }
}

/// <summary>
/// 富媒体上传请求
/// </summary>
public class UploadMediaRequest
{
    /// <summary>
    /// 文件类型 1:图片 2:视频 3:语音 4:文件
    /// </summary>
    [JsonPropertyName("file_type")]
    public int FileType { get; set; }

    /// <summary>
    /// 文件URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 是否需要服务端后续使用
    /// </summary>
    [JsonPropertyName("srv_send_msg")]
    public bool SrvSendMsg { get; set; } = false;
}

/// <summary>
/// 富媒体上传响应
/// </summary>
public class UploadMediaResponse
{
    /// <summary>
    /// 文件ID
    /// </summary>
    [JsonPropertyName("file_uuid")]
    public string? FileUuid { get; set; }

    /// <summary>
    /// 文件信息(用于发送消息)
    /// </summary>
    [JsonPropertyName("file_info")]
    public string? FileInfo { get; set; }

    /// <summary>
    /// 过期时间(秒)
    /// </summary>
    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }
}

/// <summary>
/// 创建私信会话请求
/// </summary>
public class CreateDmsRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;

    /// <summary>
    /// 源频道ID
    /// </summary>
    [JsonPropertyName("source_guild_id")]
    public string SourceGuildId { get; set; } = string.Empty;
}
