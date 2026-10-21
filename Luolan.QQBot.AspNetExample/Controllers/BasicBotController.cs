using Luolan.QQBot.Controllers;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.AspNetExample.Controllers;

/// <summary>
/// 基础命令控制器 —— 演示控制器模式的依赖注入
/// </summary>
public class BasicBotController : QQBotController
{
    private readonly ILogger<BasicBotController> _logger;

    // 控制器由 ControllerManager 通过依赖注入创建，可直接注入任意注册服务
    public BasicBotController(ILogger<BasicBotController> logger)
    {
        _logger = logger;
    }

    [Command("ping")]
    public Task Ping()
    {
        _logger.LogInformation("收到 ping 命令, 来源: {User}", User?.Username);
        return ReplyAsync("🏓 Pong from Controller!");
    }

    [Command("time")]
    public Task Time()
    {
        return ReplyAsync($"Current Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    [Command("info")]
    public string Info()
    {
        return $"""
            👤 用户: {User?.Username}
            📺 频道: {Message.GuildId ?? "无"}
            📁 子频道: {Message.ChannelId ?? "无"}
            👥 群: {Message.GroupOpenId ?? "无"}
            """;
    }
}
