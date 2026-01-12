using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 消息对象
/// </summary>
public class Message
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 消息创建时间
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 消息编辑时间
    /// </summary>
    [JsonPropertyName("edited_timestamp")]
    public DateTime? EditedTimestamp { get; set; }

    /// <summary>
    /// 是否@全体
    /// </summary>
    [JsonPropertyName("mention_everyone")]
    public bool MentionEveryone { get; set; }

    /// <summary>
    /// 消息发送者
    /// </summary>
    [JsonPropertyName("author")]
    public User? Author { get; set; }

    /// <summary>
    /// 消息发送者的成员信息
    /// </summary>
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<MessageAttachment>? Attachments { get; set; }

    /// <summary>
    /// Embed内容
    /// </summary>
    [JsonPropertyName("embeds")]
    public List<MessageEmbed>? Embeds { get; set; }

    /// <summary>
    /// @的用户列表
    /// </summary>
    [JsonPropertyName("mentions")]
    public List<User>? Mentions { get; set; }

    /// <summary>
    /// Ark消息
    /// </summary>
    [JsonPropertyName("ark")]
    public MessageArk? Ark { get; set; }

    /// <summary>
    /// 消息序列号
    /// </summary>
    [JsonPropertyName("seq")]
    public int Seq { get; set; }

    /// <summary>
    /// 消息序号(群/C2C)
    /// </summary>
    [JsonPropertyName("seq_in_channel")]
    public string? SeqInChannel { get; set; }

    /// <summary>
    /// 引用消息
    /// </summary>
    [JsonPropertyName("message_reference")]
    public MessageReference? MessageReference { get; set; }

    /// <summary>
    /// 源频道ID(私信)
    /// </summary>
    [JsonPropertyName("src_guild_id")]
    public string? SrcGuildId { get; set; }

    /// <summary>
    /// 群openid(群消息)
    /// </summary>
    [JsonPropertyName("group_openid")]
    public string? GroupOpenId { get; set; }
}

/// <summary>
/// 消息附件
/// </summary>
public class MessageAttachment
{
    /// <summary>
    /// 附件ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    /// <summary>
    /// 附件URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>
    /// 图片高度
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// 图片宽度
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }
}

/// <summary>
/// 消息Embed
/// </summary>
public class MessageEmbed
{
    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 消息弹窗内容
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// 缩略图
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public MessageEmbedThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// 字段列表
    /// </summary>
    [JsonPropertyName("fields")]
    public List<MessageEmbedField>? Fields { get; set; }
}

/// <summary>
/// 消息Embed缩略图
/// </summary>
public class MessageEmbedThumbnail
{
    /// <summary>
    /// 图片URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// 消息Embed字段
/// </summary>
public class MessageEmbedField
{
    /// <summary>
    /// 字段名
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// 消息引用
/// </summary>
public class MessageReference
{
    /// <summary>
    /// 引用的消息ID
    /// </summary>
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 是否忽略获取引用消息详情错误
    /// </summary>
    [JsonPropertyName("ignore_get_message_error")]
    public bool IgnoreGetMessageError { get; set; }
}

/// <summary>
/// Ark消息
/// </summary>
public class MessageArk
{
    /// <summary>
    /// 模板ID
    /// </summary>
    [JsonPropertyName("template_id")]
    public int TemplateId { get; set; }

    /// <summary>
    /// 参数列表
    /// </summary>
    [JsonPropertyName("kv")]
    public List<MessageArkKv>? Kv { get; set; }
}

/// <summary>
/// Ark消息键值对
/// </summary>
public class MessageArkKv
{
    /// <summary>
    /// 键名
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// 对象数组
    /// </summary>
    [JsonPropertyName("obj")]
    public List<MessageArkObj>? Obj { get; set; }
}

/// <summary>
/// Ark消息对象
/// </summary>
public class MessageArkObj
{
    /// <summary>
    /// 对象键值对列表
    /// </summary>
    [JsonPropertyName("obj_kv")]
    public List<MessageArkObjKv>? ObjKv { get; set; }
}

/// <summary>
/// Ark消息对象键值对
/// </summary>
public class MessageArkObjKv
{
    /// <summary>
    /// 键名
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
/// Markdown消息
/// </summary>
public class MessageMarkdown
{
    /// <summary>
    /// 模板ID(和content二选一)
    /// </summary>
    [JsonPropertyName("template_id")]
    public int? TemplateId { get; set; }

    /// <summary>
    /// 模板参数
    /// </summary>
    [JsonPropertyName("params")]
    public List<MessageMarkdownParam>? Params { get; set; }

    /// <summary>
    /// 原生markdown内容(和template_id二选一)
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// Markdown模板参数
/// </summary>
public class MessageMarkdownParam
{
    /// <summary>
    /// 参数名
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 参数值列表
    /// </summary>
    [JsonPropertyName("values")]
    public List<string>? Values { get; set; }
}

/// <summary>
/// 按钮键盘
/// </summary>
public class MessageKeyboard
{
    /// <summary>
    /// 模板ID(和content二选一)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 自定义内容
    /// </summary>
    [JsonPropertyName("content")]
    public InlineKeyboard? Content { get; set; }
}

/// <summary>
/// 内联键盘
/// </summary>
public class InlineKeyboard
{
    /// <summary>
    /// 按钮行列表
    /// </summary>
    [JsonPropertyName("rows")]
    public List<InlineKeyboardRow>? Rows { get; set; }
}

/// <summary>
/// 内联键盘行
/// </summary>
public class InlineKeyboardRow
{
    /// <summary>
    /// 按钮列表
    /// </summary>
    [JsonPropertyName("buttons")]
    public List<Button>? Buttons { get; set; }
}

/// <summary>
/// 按钮
/// </summary>
public class Button
{
    /// <summary>
    /// 按钮ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 按钮渲染数据
    /// </summary>
    [JsonPropertyName("render_data")]
    public ButtonRenderData? RenderData { get; set; }

    /// <summary>
    /// 按钮交互数据
    /// </summary>
    [JsonPropertyName("action")]
    public ButtonAction? Action { get; set; }
}

/// <summary>
/// 按钮渲染数据
/// </summary>
public class ButtonRenderData
{
    /// <summary>
    /// 按钮文本
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 点击后按钮文本
    /// </summary>
    [JsonPropertyName("visited_label")]
    public string? VisitedLabel { get; set; }

    /// <summary>
    /// 按钮样式
    /// </summary>
    [JsonPropertyName("style")]
    public int Style { get; set; }
}

/// <summary>
/// 按钮交互数据
/// </summary>
public class ButtonAction
{
    /// <summary>
    /// 按钮类型 0跳转 1回调 2指令
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 权限类型 0指定用户 1仅管理者 2所有人 3指定角色
    /// </summary>
    [JsonPropertyName("permission")]
    public ButtonPermission? Permission { get; set; }

    /// <summary>
    /// 按钮点击后的数据
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>
    /// 是否在输入框回填
    /// </summary>
    [JsonPropertyName("at_bot_show_channel_list")]
    public bool AtBotShowChannelList { get; set; }

    /// <summary>
    /// 指令是否弹框
    /// </summary>
    [JsonPropertyName("enter")]
    public bool Enter { get; set; }

    /// <summary>
    /// 客户端不支持本action的时候是否将data发给服务器
    /// </summary>
    [JsonPropertyName("unsupport_tips")]
    public string? UnsupportTips { get; set; }
}

/// <summary>
/// 按钮权限
/// </summary>
public class ButtonPermission
{
    /// <summary>
    /// 权限类型
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 指定用户ID列表
    /// </summary>
    [JsonPropertyName("specify_user_ids")]
    public List<string>? SpecifyUserIds { get; set; }

    /// <summary>
    /// 指定角色ID列表
    /// </summary>
    [JsonPropertyName("specify_role_ids")]
    public List<string>? SpecifyRoleIds { get; set; }
}

/// <summary>
/// 私信会话
/// </summary>
public class DirectMessageSession
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
    /// 创建时间
    /// </summary>
    [JsonPropertyName("create_time")]
    public DateTime CreateTime { get; set; }
}
