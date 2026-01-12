using Luolan.QQBot;
using Luolan.QQBot.Events;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== Luolan.QQBot 示例程序 ===");
Console.WriteLine();

// 你的凭证
var appId = "";
var clientSecret = "";

if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientSecret))
{
    Console.WriteLine("请设置环境变量:");
    
    
    if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientSecret))
    {
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
        return;
    }
}

// 创建日志工厂
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// 创建机器人客户端
var bot = new QQBotClientBuilder()
    .WithAppId(appId)
    .WithClientSecret(clientSecret)
    .WithIntents(
        Intents.Guilds |                    // 频道事件
        Intents.GuildMembers |              // 成员事件
        Intents.PublicGuildMessages |       // 公域消息(@机器人)
        Intents.DirectMessage |             // 私信
        Intents.GuildMessageReactions |     // 表情表态
        Intents.Interaction |               // 互动事件(按钮)
        Intents.MessageAudit |              // 消息审核
        Intents.GroupAtMessages             // 群@消息
    )
    .WithLoggerFactory(loggerFactory)
    .Build();

// ==================== 事件处理 ====================

// 连接就绪
bot.OnReady += async e =>
{
    Console.WriteLine();
    Console.WriteLine($"✅ 机器人已上线!");
    Console.WriteLine($"   用户名: {e.User?.Username}");
    Console.WriteLine($"   用户ID: {e.User?.Id}");
    Console.WriteLine($"   会话ID: {e.SessionId}");
    Console.WriteLine();
};

// 频道@消息 (公域)
bot.OnAtMessageCreate += async e =>
{
    var msg = e.Message;
    // 去除前导空格和尾部空格
    var content = msg.Content?.Trim() ?? "";
    var author = msg.Author?.Username ?? "未知用户";
    
    Console.WriteLine($"[频道消息] {author}: {content}");
    
    // 简单的命令处理
    if (content.Contains("/帮助") || content.Contains("/help"))
    {
        await bot.ReplyAsync(msg, @"🤖 可用命令:
/帮助 - 显示帮助
/ping - 测试延迟
/信息 - 显示频道信息
/echo [内容] - 复读内容");
    }
    else if (content.Contains("/ping"))
    {
        var start = DateTime.Now;
        var reply = await bot.ReplyAsync(msg, "🏓 Pong!");
        var delay = (DateTime.Now - start).TotalMilliseconds;
        Console.WriteLine($"   响应延迟: {delay:F0}ms");
    }
    else if (content.Contains("/信息") || content.Contains("/info"))
    {
        var info = $@"📊 信息:
频道ID: {msg.GuildId}
子频道ID: {msg.ChannelId}
消息ID: {msg.Id}
发送者: {author} ({msg.Author?.Id})";
        await bot.ReplyAsync(msg, info);
    }
    else if (content.Contains("/echo"))
    {
        var echo = content.Replace("/echo", "").Trim();
        if (!string.IsNullOrEmpty(echo))
        {
            await bot.ReplyAsync(msg, $"📢 {echo}");
        }
    }
    else
    {
        // 默认回复
        await bot.ReplyAsync(msg, $"👋 你好 {author}! 发送 /帮助 查看可用命令");
    }
};

// 私信消息
bot.OnDirectMessageCreate += async e =>
{
    var msg = e.Message;
    var author = msg.Author?.Username ?? "未知用户";
    Console.WriteLine($"[私信] {author}: {msg.Content}");
    
    // 私信需要使用DMS API回复
    if (!string.IsNullOrEmpty(msg.GuildId))
    {
        await bot.Api.SendDmsAsync(msg.GuildId, new SendMessageRequest
        {
            Content = $"收到你的私信: {msg.Content}",
            MsgId = msg.Id
        });
    }
};

// 群@消息
bot.OnGroupAtMessageCreate += async e =>
{
    var msg = e.Message;
    // 去除前导空格和尾部空格（QQ群消息前面可能有空格）
    var content = (msg.Content ?? "").Trim();
    Console.WriteLine($"[群消息] 群{msg.GroupOpenId}: {content}");
    
    // 命令处理
    if (content.Contains("/帮助") || content.Contains("/help"))
    {
        await bot.ReplyGroupAsync(msg, @"🤖 可用命令:
/帮助 - 显示帮助
/ping - 测试延迟
/echo [内容] - 复读内容");
    }
    else if (content.Contains("/ping"))
    {
        await bot.ReplyGroupAsync(msg, "🏓 Pong!");
    }
    else if (content.Contains("/echo"))
    {
        var echo = content.Replace("/echo", "").Trim();
        if (!string.IsNullOrEmpty(echo))
        {
            await bot.ReplyGroupAsync(msg, $"📢 {echo}");
        }
    }
    else
    {
        await bot.ReplyGroupAsync(msg, $"👋 收到群消息: {content}\n发送 /帮助 查看可用命令");
    }
};

// C2C消息 (私聊)
bot.OnC2CMessageCreate += async e =>
{
    var msg = e.Message;
    // 去除前导空格和尾部空格
    var content = (msg.Content ?? "").Trim();
    Console.WriteLine($"[C2C消息] {msg.Author?.UserOpenId}: {content}");
    
    // 命令处理
    if (content.Contains("/帮助") || content.Contains("/help"))
    {
        await bot.ReplyC2CAsync(msg, @"🤖 可用命令:
/帮助 - 显示帮助
/ping - 测试延迟
/echo [内容] - 复读内容");
    }
    else if (content.Contains("/ping"))
    {
        await bot.ReplyC2CAsync(msg, "🏓 Pong!");
    }
    else if (content.Contains("/echo"))
    {
        var echo = content.Replace("/echo", "").Trim();
        if (!string.IsNullOrEmpty(echo))
        {
            await bot.ReplyC2CAsync(msg, $"📢 {echo}");
        }
    }
    else
    {
        await bot.ReplyC2CAsync(msg, $"👋 收到消息: {content}\n发送 /帮助 查看可用命令");
    }
};

// 互动事件 (按钮点击)
bot.OnInteractionCreate += async e =>
{
    var interaction = e.Interaction;
    var buttonId = interaction.Data?.Resolved?.ButtonId;
    var buttonData = interaction.Data?.Resolved?.ButtonData;
    
    Console.WriteLine($"[互动] 按钮点击: {buttonId} - {buttonData}");
};

// 成员加入
bot.OnGuildMemberAdd += async e =>
{
    var nick = e.Member.Nick ?? e.Member.User?.Username ?? "新成员";
    Console.WriteLine($"[成员] {nick} 加入了频道 {e.GuildId}");
};

// 成员离开
bot.OnGuildMemberRemove += async e =>
{
    var nick = e.Member.Nick ?? e.Member.User?.Username ?? "成员";
    Console.WriteLine($"[成员] {nick} 离开了频道 {e.GuildId}");
};

// 加入新频道
bot.OnGuildCreate += async e =>
{
    Console.WriteLine($"[频道] 加入频道: {e.Guild.Name} ({e.Guild.Id})");
};

// 被移出频道
bot.OnGuildDelete += async e =>
{
    Console.WriteLine($"[频道] 离开频道: {e.Guild.Name} ({e.Guild.Id})");
};

// 群添加机器人
bot.OnGroupAddRobot += async e =>
{
    Console.WriteLine($"[群管理] 被添加到群: {e.GroupOpenId}");
};

// 群删除机器人
bot.OnGroupDelRobot += async e =>
{
    Console.WriteLine($"[群管理] 被移出群: {e.GroupOpenId}");
};

// 好友添加
bot.OnFriendAdd += async e =>
{
    Console.WriteLine($"[好友] 新好友: {e.OpenId}");
};

// 消息审核通过
bot.Events.OnMessageAuditPass += async e =>
{
    Console.WriteLine($"[审核] 消息审核通过: {e.MessageAudited.MessageId}");
};

// 消息审核不通过
bot.Events.OnMessageAuditReject += async e =>
{
    Console.WriteLine($"[审核] 消息审核不通过: {e.MessageAudited.AuditId}");
};

// ==================== 启动机器人 ====================

Console.WriteLine("正在启动机器人...");
Console.WriteLine();

try
{
    await bot.StartAsync();
    
    Console.WriteLine("机器人已启动! 按 Ctrl+C 退出");
    Console.WriteLine();
    
    // 等待退出信号
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };
    
    try
    {
        await Task.Delay(-1, cts.Token);
    }
    catch (OperationCanceledException)
    {
        // 正常退出
    }
    
    Console.WriteLine();
    Console.WriteLine("正在停止机器人...");
    await bot.StopAsync();
    Console.WriteLine("机器人已停止");
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
finally
{
    bot.Dispose();
}
