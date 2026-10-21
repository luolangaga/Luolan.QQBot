using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 自定义菜单
/// </summary>
public class QQBotMenu
{
    /// <summary>
    /// 菜单项列表(最多 10 个)
    /// </summary>
    [JsonPropertyName("items")]
    public List<MenuItem>? Items { get; set; }
}

/// <summary>
/// 菜单项
/// </summary>
public class MenuItem
{
    /// <summary>
    /// 按钮名称(最多 10 个字符)
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 按钮类型: switch / send_message / link / menu
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 子菜单列表(仅 type=menu 时有效)
    /// </summary>
    [JsonPropertyName("sub_menu_items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SubMenuItem>? SubMenuItems { get; set; }

    /// <summary>
    /// 发送的内容(仅 type=send_message 时有效)
    /// </summary>
    [JsonPropertyName("send_message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SendMessage { get; set; }

    /// <summary>
    /// 跳转链接(仅 type=link 时有效)
    /// </summary>
    [JsonPropertyName("link")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Link { get; set; }

    /// <summary>
    /// 开关配置(仅 type=switch 时有效)
    /// </summary>
    [JsonPropertyName("switch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MenuSwitch? Switch { get; set; }
}

/// <summary>
/// 子菜单项
/// </summary>
public class SubMenuItem
{
    /// <summary>
    /// 按钮名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 按钮类型: send_message / link
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 发送的内容
    /// </summary>
    [JsonPropertyName("send_message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SendMessage { get; set; }

    /// <summary>
    /// 跳转链接
    /// </summary>
    [JsonPropertyName("link")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Link { get; set; }
}

/// <summary>
/// 菜单开关配置
/// </summary>
public class MenuSwitch
{
    /// <summary>
    /// 开关唯一标识
    /// </summary>
    [JsonPropertyName("switch_id")]
    public string? SwitchId { get; set; }

    /// <summary>
    /// 开关初始状态
    /// </summary>
    [JsonPropertyName("default")]
    public bool Default { get; set; }
}

/// <summary>
/// 菜单查询/修改响应
/// </summary>
public class MenuResponse
{
    /// <summary>
    /// 当前菜单版本号
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// 当前生效的菜单配置
    /// </summary>
    [JsonPropertyName("menu")]
    public QQBotMenu? Menu { get; set; }
}

/// <summary>
/// 修改菜单请求
/// </summary>
public class UpdateMenuRequest
{
    /// <summary>
    /// 菜单配置
    /// </summary>
    [JsonPropertyName("menu")]
    public QQBotMenu Menu { get; set; } = new();
}

/// <summary>
/// 指令面板
/// </summary>
public class Panel
{
    /// <summary>
    /// 面板元素列表(最多 20 个)
    /// </summary>
    [JsonPropertyName("items")]
    public List<PanelItem>? Items { get; set; }

    /// <summary>
    /// 面板备注(最多 255 字符, 不对用户展示)
    /// </summary>
    [JsonPropertyName("remark")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Remark { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }
}

/// <summary>
/// 面板元素
/// </summary>
public class PanelItem
{
    /// <summary>
    /// 元素名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 元素描述
    /// </summary>
    [JsonPropertyName("desc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Desc { get; set; }

    /// <summary>
    /// 元素类型: command / link
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 是否仅管理员可操作
    /// </summary>
    [JsonPropertyName("only_admin")]
    public bool OnlyAdmin { get; set; }

    /// <summary>
    /// 跳转链接(仅 type=link 时有效)
    /// </summary>
    [JsonPropertyName("link")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Link { get; set; }
}

/// <summary>
/// 创建指令面板请求
/// </summary>
public class CreatePanelRequest
{
    /// <summary>
    /// 生效场景: c2c / group / channel / dm
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "c2c";

    /// <summary>
    /// 作用范围: all / specific
    /// </summary>
    [JsonPropertyName("target_type")]
    public string TargetType { get; set; } = "all";

    /// <summary>
    /// 用户 openid 列表(c2c + specific)
    /// </summary>
    [JsonPropertyName("user_openids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserOpenIds { get; set; }

    /// <summary>
    /// 群 openid 列表(group + specific)
    /// </summary>
    [JsonPropertyName("group_openids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? GroupOpenIds { get; set; }

    /// <summary>
    /// 面板配置内容
    /// </summary>
    [JsonPropertyName("panel")]
    public Panel Panel { get; set; } = new();
}

/// <summary>
/// 修改指令面板请求
/// </summary>
public class UpdatePanelRequest
{
    /// <summary>
    /// 面板配置内容
    /// </summary>
    [JsonPropertyName("panel")]
    public Panel Panel { get; set; } = new();
}

/// <summary>
/// 修改指令面板关联对象请求
/// </summary>
public class UpdatePanelTargetRequest
{
    /// <summary>
    /// 操作类型: add / del
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = "add";

    /// <summary>
    /// 用户 openid 列表(仅 c2c 场景)
    /// </summary>
    [JsonPropertyName("user_openids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserOpenIds { get; set; }

    /// <summary>
    /// 群 openid 列表
    /// </summary>
    [JsonPropertyName("group_openids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? GroupOpenIds { get; set; }
}

/// <summary>
/// 创建指令面板响应
/// </summary>
public class CreatePanelResponse
{
    /// <summary>
    /// 新创建的面板 ID
    /// </summary>
    [JsonPropertyName("panel_id")]
    public string? PanelId { get; set; }
}

/// <summary>
/// 指令面板版本响应
/// </summary>
public class PanelVersionResponse
{
    /// <summary>
    /// 本次修改后的面板版本号
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }
}

/// <summary>
/// 指令面板记录
/// </summary>
public class PanelRecord
{
    /// <summary>
    /// 面板 ID
    /// </summary>
    [JsonPropertyName("panel_id")]
    public string? PanelId { get; set; }

    /// <summary>
    /// 生效场景
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// 作用范围
    /// </summary>
    [JsonPropertyName("target_type")]
    public string? TargetType { get; set; }

    /// <summary>
    /// 面板配置内容
    /// </summary>
    [JsonPropertyName("panel")]
    public Panel? Panel { get; set; }

    /// <summary>
    /// 面板创建时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// 面板更新时间
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// 面板版本号
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// 关联的用户 openid 列表(c2c + specific)
    /// </summary>
    [JsonPropertyName("user_openids")]
    public List<string>? UserOpenIds { get; set; }

    /// <summary>
    /// 关联的群 openid 列表(group + specific)
    /// </summary>
    [JsonPropertyName("group_openids")]
    public List<string>? GroupOpenIds { get; set; }
}

/// <summary>
/// 指令面板列表响应
/// </summary>
public class PanelListResponse
{
    /// <summary>
    /// 面板记录列表
    /// </summary>
    [JsonPropertyName("records")]
    public List<PanelRecord>? Records { get; set; }

    /// <summary>
    /// 下一页游标
    /// </summary>
    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否已拉取到最后一页
    /// </summary>
    [JsonPropertyName("is_end")]
    public bool IsEnd { get; set; }
}

/// <summary>
/// 互动事件响应请求
/// </summary>
public class InteractionResponseRequest
{
    /// <summary>
    /// 回调结果: 0=成功 1=操作失败 2=操作频繁 3=重复操作 4=没有权限 5=仅管理员操作
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }
}

/// <summary>
/// 生成分享链接请求
/// </summary>
public class GenerateUrlLinkRequest
{
    /// <summary>
    /// 回传给机器人后台的数据(最长 32 字符)
    /// </summary>
    [JsonPropertyName("callback_data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CallbackData { get; set; }
}

/// <summary>
/// 生成分享链接响应
/// </summary>
public class GenerateUrlLinkResponse
{
    /// <summary>
    /// 链接数据
    /// </summary>
    [JsonPropertyName("data")]
    public GenerateUrlLinkData? Data { get; set; }
}

/// <summary>
/// 分享链接数据
/// </summary>
public class GenerateUrlLinkData
{
    /// <summary>
    /// 生成的分享链接
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
