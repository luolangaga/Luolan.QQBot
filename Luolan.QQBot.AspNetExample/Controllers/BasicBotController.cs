using Luolan.QQBot.Core.Controllers;
using Luolan.QQBot.Core.Models;

namespace Luolan.QQBot.AspNetExample.Controllers;

public class BasicBotController : BotController
{
    [Command("/ping")]
    public async Task Ping()
    {
        await ReplyAsync("🏓 Pong from Controller!");
    }

    [Command("/time")]
    public async Task Time()
    {
        await ReplyAsync($"Current Time: {DateTime.Now}");
    }
}
