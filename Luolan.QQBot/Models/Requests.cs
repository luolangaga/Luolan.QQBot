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

    /// <summary>
    /// 引用回复
    /// </summary>
    [JsonPropertyName("message_reference")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageReference? MessageReference { get; set; }

    /// <summary>
    /// 是否为互动召回消息(单聊)
    /// </summary>
    [JsonPropertyName("is_wakeup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsWakeup { get; set; }

    /// <summary>
    /// 输入中状态(单聊, msg_type=6)
    /// </summary>
    [JsonPropertyName("input_notify")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InputNotify? InputNotify { get; set; }
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

    /// <summary>
    /// 扩展信息
    /// </summary>
    [JsonPropertyName("ext_info")]
    public MessageExtInfo? ExtInfo { get; set; }
}

/// <summary>
/// 消息扩展信息
/// </summary>
public class MessageExtInfo
{
    /// <summary>
    /// 引用消息索引
    /// </summary>
    [JsonPropertyName("ref_idx")]
    public string? RefIdx { get; set; }
}

/// <summary>
/// 输入中状态
/// </summary>
public class InputNotify
{
    /// <summary>
    /// 输入类型, 填 1
    /// </summary>
    [JsonPropertyName("input_type")]
    public int InputType { get; set; } = 1;

    /// <summary>
    /// 状态持续时间(秒), 最长 60
    /// </summary>
    [JsonPropertyName("input_second")]
    public int InputSecond { get; set; } = 60;
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
    /// 文件URL(分片上传合并时可为空)
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }

    /// <summary>
    /// 是否需要服务端后续使用
    /// </summary>
    [JsonPropertyName("srv_send_msg")]
    public bool SrvSendMsg { get; set; } = false;

    /// <summary>
    /// 分片上传任务ID
    /// </summary>
    [JsonPropertyName("upload_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UploadId { get; set; }

    /// <summary>
    /// 文件名(分片上传或二进制上传时必填)
    /// </summary>
    [JsonPropertyName("file_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FileName { get; set; }

    /// <summary>
    /// 直接上传的二进制数据(Base64, 5M 以内)
    /// </summary>
    [JsonPropertyName("file_data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FileData { get; set; }
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

    /// <summary>
    /// 发送消息的唯一ID(仅 srv_send_msg=true 时返回)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 文件下载链接(仅分片上传合并的图片/视频/语音返回)
    /// </summary>
    [JsonPropertyName("raw_url")]
    public string? RawUrl { get; set; }
}

/// <summary>
/// 流式消息请求(单聊)
/// </summary>
public class SendC2CStreamMessageRequest
{
    /// <summary>
    /// 输入模式: append(默认) / replace
    /// </summary>
    [JsonPropertyName("input_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InputMode { get; set; }

    /// <summary>
    /// 输入状态: 1=生成中, 10=生成结束
    /// </summary>
    [JsonPropertyName("input_state")]
    public int InputState { get; set; } = 1;

    /// <summary>
    /// 分片序号, 从 0 递增
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// 内容格式: text / markdown
    /// </summary>
    [JsonPropertyName("content_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; set; } = "markdown";

    /// <summary>
    /// 文本/Markdown 内容
    /// </summary>
    [JsonPropertyName("content_raw")]
    public string ContentRaw { get; set; } = string.Empty;

    /// <summary>
    /// 被动回复事件ID
    /// </summary>
    [JsonPropertyName("event_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EventId { get; set; }

    /// <summary>
    /// 被动回复消息ID
    /// </summary>
    [JsonPropertyName("msg_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MsgId { get; set; }

    /// <summary>
    /// 流式消息ID(首片由服务端返回)
    /// </summary>
    [JsonPropertyName("stream_msg_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StreamMsgId { get; set; }

    /// <summary>
    /// 消息序号
    /// </summary>
    [JsonPropertyName("msg_seq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MsgSeq { get; set; }

    /// <summary>
    /// 是否为召回消息
    /// </summary>
    [JsonPropertyName("is_wakeup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsWakeup { get; set; }
}

/// <summary>
/// 流式消息响应
/// </summary>
public class StreamMessageResponse
{
    /// <summary>
    /// 消息ID(首片为 stream_msg_id)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// 扩展信息
    /// </summary>
    [JsonPropertyName("ext_info")]
    public MessageExtInfo? ExtInfo { get; set; }

    /// <summary>
    /// 流式消息剩余长度(字符数)
    /// </summary>
    [JsonPropertyName("remain_msg_len")]
    public int? RemainMsgLen { get; set; }
}

/// <summary>
/// 富媒体分片预上传请求
/// </summary>
public class UploadPrepareRequest
{
    /// <summary>
    /// 业务类型 1=图片 2=视频 3=语音 4=文件
    /// </summary>
    [JsonPropertyName("file_type")]
    public int FileType { get; set; }

    /// <summary>
    /// 文件大小(字节)
    /// </summary>
    [JsonPropertyName("file_size")]
    public string FileSize { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 整个文件的 MD5
    /// </summary>
    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    /// <summary>
    /// 整个文件的 SHA1
    /// </summary>
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; } = string.Empty;

    /// <summary>
    /// 文件前约 10MB 的 MD5
    /// </summary>
    [JsonPropertyName("md5_10m")]
    public string Md510m { get; set; } = string.Empty;
}

/// <summary>
/// 富媒体分片预上传响应
/// </summary>
public class UploadPrepareResponse
{
    /// <summary>
    /// 上传任务ID
    /// </summary>
    [JsonPropertyName("upload_id")]
    public string UploadId { get; set; } = string.Empty;

    /// <summary>
    /// 分块大小(字节)
    /// </summary>
    [JsonPropertyName("block_size")]
    public string BlockSize { get; set; } = string.Empty;

    /// <summary>
    /// 分片列表
    /// </summary>
    [JsonPropertyName("parts")]
    public List<UploadPart>? Parts { get; set; }

    /// <summary>
    /// 上传配置
    /// </summary>
    [JsonPropertyName("upload_config")]
    public UploadConfig? UploadConfig { get; set; }
}

/// <summary>
/// 上传分片
/// </summary>
public class UploadPart
{
    /// <summary>
    /// 分片序号, 从 0 开始
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// 预签名上传URL
    /// </summary>
    [JsonPropertyName("presigned_url")]
    public string PresignedUrl { get; set; } = string.Empty;

    /// <summary>
    /// 该分块大小(字节)
    /// </summary>
    [JsonPropertyName("block_size")]
    public string BlockSize { get; set; } = string.Empty;
}

/// <summary>
/// 上传配置
/// </summary>
public class UploadConfig
{
    /// <summary>
    /// 上传并发数
    /// </summary>
    [JsonPropertyName("concurrency")]
    public int Concurrency { get; set; } = 1;

    /// <summary>
    /// 重试超时时间(秒)
    /// </summary>
    [JsonPropertyName("retry_timeout")]
    public int RetryTimeout { get; set; } = 300;

    /// <summary>
    /// 重试延迟(秒)
    /// </summary>
    [JsonPropertyName("retry_delay")]
    public int RetryDelay { get; set; } = 1;
}

/// <summary>
/// 分片上传完成请求
/// </summary>
public class UploadPartFinishRequest
{
    /// <summary>
    /// 上传任务ID
    /// </summary>
    [JsonPropertyName("upload_id")]
    public string UploadId { get; set; } = string.Empty;

    /// <summary>
    /// 分片序号
    /// </summary>
    [JsonPropertyName("part_index")]
    public int PartIndex { get; set; }

    /// <summary>
    /// 该分块的实际大小(字节)
    /// </summary>
    [JsonPropertyName("block_size")]
    public string BlockSize { get; set; } = string.Empty;

    /// <summary>
    /// 该分片的 MD5
    /// </summary>
    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;
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
