namespace Luolan.QQBot.AspNetExample.Models;

/// <summary>
/// 发送消息请求
/// </summary>
/// <param name="TargetId">群 OpenID / 用户 OpenID / 子频道ID</param>
/// <param name="Content">消息内容</param>
/// <param name="TargetType">目标类型: group / c2c / channel</param>
public record SendMessageDto(string TargetId, string Content, string TargetType = "group");
