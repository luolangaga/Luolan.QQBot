using System;
using System.Threading.Tasks;
using Luolan.QQBot.Models;

namespace Luolan.QQBot.Controllers;

/// <summary>
/// 控制器基类
/// </summary>
public abstract class QQBotController
{
    /// <summary>
    /// 当前机器人客户端
    /// </summary>
    public QQBotClient Client { get; internal set; } = null!;

    /// <summary>
    /// 当前消息上下文
    /// </summary>
    public Message Message { get; internal set; } = null!;

    /// <summary>
    /// 发送消息的用户
    /// </summary>
    public User? User => Message.Author;

    /// <summary>
    /// 消息内容(去除了命令部分)
    /// </summary>
    public string RawContent { get; internal set; } = string.Empty;

    /// <summary>
    /// 原始参数数组
    /// </summary>
    public string[] RawArguments { get; internal set; } = Array.Empty<string>();

    /// <summary>
    /// 回复消息(并不结束执行，Return才是主要回复方式)
    /// </summary>
    /// <param name="content">回复内容</param>
    public async Task ReplyAsync(string content)
    {
        if (Client == null || Message == null) return;

        try 
        {
            if (!string.IsNullOrEmpty(Message.ChannelId))
            {
                await Client.SendChannelMessageAsync(Message.ChannelId, content, Message.Id);
            }
            else if (!string.IsNullOrEmpty(Message.GroupOpenId))
            {
                await Client.SendGroupMessageAsync(Message.GroupOpenId, content, Message.Id);
            }
            else if (Message.Author != null && !string.IsNullOrEmpty(Message.Author.UserOpenId))
            {
                // C2C usually requires openid
                 await Client.SendC2CMessageAsync(Message.Author.UserOpenId, content, Message.Id);
            }
        }
        catch(Exception ex)
        {
            // Log error?
            Console.WriteLine($"Reply Error: {ex.Message}");
        }
    }
}
