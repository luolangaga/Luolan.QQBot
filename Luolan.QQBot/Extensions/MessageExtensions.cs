using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;

namespace Luolan.QQBot.Extensions;

/// <summary>
/// 消息发送扩展方法
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// 发送Markdown消息到频道
    /// </summary>
    /// <param name="client">QQ机器人客户端</param>
    /// <param name="channelId">子频道ID</param>
    /// <param name="markdown">Markdown消息</param>
    /// <param name="keyboard">按钮键盘(可选)</param>
    /// <param name="msgId">消息ID(用于被动回复)</param>
    /// <param name="eventId">事件ID(用于被动回复)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<Message> SendMarkdownAsync(
        this QQBotClient client,
        string channelId,
        MessageMarkdown markdown,
        MessageKeyboard? keyboard = null,
        string? msgId = null,
        string? eventId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new SendMessageRequest
        {
            Markdown = markdown,
            Keyboard = keyboard,
            MsgId = msgId,
            EventId = eventId
        };

        return client.Api.SendMessageAsync(channelId, request, cancellationToken);
    }

    /// <summary>
    /// 发送Markdown模板消息到频道
    /// </summary>
    /// <param name="client">QQ机器人客户端</param>
    /// <param name="channelId">子频道ID</param>
    /// <param name="customTemplateId">模板ID（custom_template_id）</param>
    /// <param name="parameters">模板参数</param>
    /// <param name="keyboard">按钮键盘(可选)</param>
    /// <param name="msgId">消息ID(用于被动回复)</param>
    /// <param name="eventId">事件ID(用于被动回复)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<Message> SendMarkdownTemplateAsync(
        this QQBotClient client,
        string channelId,
        string customTemplateId,
        Dictionary<string, string[]>? parameters = null,
        MessageKeyboard? keyboard = null,
        string? msgId = null,
        string? eventId = null,
        CancellationToken cancellationToken = default)
    {
        var markdown = parameters != null
            ? MarkdownBuilder.FromTemplate(customTemplateId, parameters)
            : MarkdownBuilder.FromTemplate(customTemplateId);

        return client.SendMarkdownAsync(channelId, markdown, keyboard, msgId, eventId, cancellationToken);
    }

    /// <summary>
    /// 发送原生Markdown内容到频道
    /// </summary>
    /// <param name="client">QQ机器人客户端</param>
    /// <param name="channelId">子频道ID</param>
    /// <param name="content">Markdown内容</param>
    /// <param name="keyboard">按钮键盘(可选)</param>
    /// <param name="msgId">消息ID(用于被动回复)</param>
    /// <param name="eventId">事件ID(用于被动回复)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<Message> SendMarkdownContentAsync(
        this QQBotClient client,
        string channelId,
        string content,
        MessageKeyboard? keyboard = null,
        string? msgId = null,
        string? eventId = null,
        CancellationToken cancellationToken = default)
    {
        var markdown = MarkdownBuilder.FromContent(content);
        return client.SendMarkdownAsync(channelId, markdown, keyboard, msgId, eventId, cancellationToken);
    }

    /// <summary>
    /// 回复消息时发送Markdown
    /// </summary>
    /// <param name="client">QQ机器人客户端</param>
    /// <param name="message">原始消息</param>
    /// <param name="markdown">Markdown消息</param>
    /// <param name="keyboard">按钮键盘(可选)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<Message> ReplyMarkdownAsync(
        this QQBotClient client,
        Message message,
        MessageMarkdown markdown,
        MessageKeyboard? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.ChannelId))
        {
            throw new ArgumentException("消息必须包含 ChannelId", nameof(message));
        }

        return client.SendMarkdownAsync(
            message.ChannelId,
            markdown,
            keyboard,
            message.Id,
            null,
            cancellationToken);
    }

    /// <summary>
    /// 回复消息时发送Markdown模板
    /// </summary>
    /// <param name="client">QQ机器人客户端</param>
    /// <param name="message">原始消息</param>
    /// <param name="customTemplateId">模板ID（custom_template_id）</param>
    /// <param name="parameters">模板参数</param>
    /// <param name="keyboard">按钮键盘(可选)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<Message> ReplyMarkdownTemplateAsync(
        this QQBotClient client,
        Message message,
        string customTemplateId,
        Dictionary<string, string[]>? parameters = null,
        MessageKeyboard? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        var markdown = parameters != null
            ? MarkdownBuilder.FromTemplate(customTemplateId, parameters)
            : MarkdownBuilder.FromTemplate(customTemplateId);

        return client.ReplyMarkdownAsync(message, markdown, keyboard, cancellationToken);
    }

    /// <summary>
    /// 兼容旧用法：将数字模板ID转为字符串。
    /// </summary>
    [Obsolete("QQ Bot 模板应使用 custom_template_id(string)。请改用 SendMarkdownTemplateAsync(string)。")]
    public static Task<Message> SendMarkdownTemplateAsync(
        this QQBotClient client,
        string channelId,
        int templateId,
        Dictionary<string, string[]>? parameters = null,
        MessageKeyboard? keyboard = null,
        string? msgId = null,
        string? eventId = null,
        CancellationToken cancellationToken = default)
        => client.SendMarkdownTemplateAsync(channelId, templateId.ToString(), parameters, keyboard, msgId, eventId, cancellationToken);

    /// <summary>
    /// 兼容旧用法：将数字模板ID转为字符串。
    /// </summary>
    [Obsolete("QQ Bot 模板应使用 custom_template_id(string)。请改用 ReplyMarkdownTemplateAsync(string)。")]
    public static Task<Message> ReplyMarkdownTemplateAsync(
        this QQBotClient client,
        Message message,
        int templateId,
        Dictionary<string, string[]>? parameters = null,
        MessageKeyboard? keyboard = null,
        CancellationToken cancellationToken = default)
        => client.ReplyMarkdownTemplateAsync(message, templateId.ToString(), parameters, keyboard, cancellationToken);
}
