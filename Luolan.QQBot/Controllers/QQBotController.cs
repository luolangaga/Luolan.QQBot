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
        if (Client == null || Message == null)
        {
            Console.WriteLine($"[QQBotController.ReplyAsync] Client或Message为空");
            return;
        }

        try
        {
            if (!string.IsNullOrEmpty(Message.ChannelId))
            {
                Console.WriteLine($"[QQBotController.ReplyAsync] 发送频道回复到 ChannelId: {Message.ChannelId}");
                await Client.SendChannelMessageAsync(Message.ChannelId, content, Message.Id);
                Console.WriteLine($"[QQBotController.ReplyAsync] 频道回复成功");
            }
            else if (!string.IsNullOrEmpty(Message.GroupOpenId))
            {
                Console.WriteLine($"[QQBotController.ReplyAsync] 发送群回复到 GroupOpenId: {Message.GroupOpenId}");
                await Client.SendGroupMessageAsync(Message.GroupOpenId, content, Message.Id);
                Console.WriteLine($"[QQBotController.ReplyAsync] 群回复成功");
            }
            else if (Message.Author != null && !string.IsNullOrEmpty(Message.Author.UserOpenId))
            {
                Console.WriteLine($"[QQBotController.ReplyAsync] 发送C2C回复到 UserOpenId: {Message.Author.UserOpenId}");
                 await Client.SendC2CMessageAsync(Message.Author.UserOpenId, content, Message.Id);
                Console.WriteLine($"[QQBotController.ReplyAsync] C2C回复成功");
            }
            else
            {
                Console.WriteLine($"[QQBotController.ReplyAsync] 无法确定消息来源类型");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Reply Error: {ex.GetType().Name} - {ex.Message}");
        }
    }
}
