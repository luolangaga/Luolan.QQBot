using Luolan.QQBot;
using Luolan.QQBot.Events;
using Luolan.QQBot.Extensions;
using Luolan.QQBot.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==================== 配置 QQ 机器人 ====================
builder.Services.AddQQBot(options =>
{
    // 从配置文件或环境变量读取
    options.AppId = builder.Configuration["QQBot:AppId"] 
        ?? Environment.GetEnvironmentVariable("QQ_BOT_APP_ID") 
        ?? throw new InvalidOperationException("未配置 QQ_BOT_APP_ID");
    
    options.ClientSecret = builder.Configuration["QQBot:ClientSecret"] 
        ?? Environment.GetEnvironmentVariable("QQ_BOT_CLIENT_SECRET") 
        ?? throw new InvalidOperationException("未配置 QQ_BOT_CLIENT_SECRET");
    
    // 配置订阅的事件类型
    options.Intents = Intents.Guilds | 
                      Intents.GuildMembers | 
                      Intents.PublicGuildMessages | 
                      Intents.DirectMessage | 
                      Intents.Interaction | 
                      Intents.MessageAudit |
                      Intents.GroupAtMessages;
    
    // 可选: 使用沙箱环境
    // options.IsSandbox = true;
});

// 添加托管服务 - 自动启动和停止机器人
builder.Services.AddQQBotHostedService();

// 添加内存缓存 (用于示例)
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==================== 配置机器人事件处理 ====================
var bot = app.Services.GetRequiredService<QQBotClient>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// 连接就绪事件
bot.OnReady += async e =>
{
    logger.LogInformation("🤖 机器人已上线: {Username} ({UserId})", e.User?.Username, e.User?.Id);
};

// 频道@消息处理
bot.OnAtMessageCreate += async e =>
{
    var msg = e.Message;
    var content = msg.Content?.Trim() ?? "";
    var author = msg.Author?.Username ?? "未知";
    
    logger.LogInformation("[频道@消息] {Author}: {Content}", author, content);
    
    try
    {
        if (content.Contains("/ping"))
        {
            await bot.ReplyAsync(msg, "🏓 Pong! 机器人运行正常");
        }
        else if (content.Contains("/time") || content.Contains("/时间"))
        {
            await bot.ReplyAsync(msg, $"⏰ 当前服务器时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        else if (content.Contains("/status") || content.Contains("/状态"))
        {
            var status = $@"✅ 机器人状态:
• 在线: {bot.IsConnected}
• 环境: ASP.NET Core {Environment.Version}
• 运行时间: {Environment.TickCount64 / 1000}秒";
            await bot.ReplyAsync(msg, status);
        }
        else if (content.Contains("/help") || content.Contains("/帮助"))
        {
            await bot.ReplyAsync(msg, @"📖 可用命令:
/ping - 测试连接
/time - 查看时间
/status - 查看状态
/help - 显示帮助");
        }
        else
        {
            await bot.ReplyAsync(msg, $"👋 你好 {author}! 发送 /help 查看可用命令");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "处理消息时出错");
    }
};

// 群@消息处理
bot.OnGroupAtMessageCreate += async e =>
{
    var msg = e.Message;
    var content = msg.Content ?? "";
    
    logger.LogInformation("[群@消息] 群{GroupId}: {Content}", msg.GroupOpenId, content);
    
    try
    {
        if (content.Contains("/ping"))
        {
            await bot.ReplyGroupAsync(msg, "🏓 Pong from ASP.NET Core!");
        }
        else
        {
            await bot.ReplyGroupAsync(msg, $"收到群消息: {content}");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "处理群消息时出错");
    }
};

// C2C消息处理
bot.OnC2CMessageCreate += async e =>
{
    var msg = e.Message;
    logger.LogInformation("[C2C消息] {OpenId}: {Content}", msg.Author?.UserOpenId, msg.Content);
    
    try
    {
        await bot.ReplyC2CAsync(msg, $"收到你的消息: {msg.Content}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "处理C2C消息时出错");
    }
};

// 互动事件 (按钮点击)
bot.OnInteractionCreate += async e =>
{
    var buttonData = e.Interaction.Data?.Resolved?.ButtonData;
    logger.LogInformation("[互动事件] 按钮点击: {ButtonData}", buttonData);
};

// 成员加入事件
bot.OnGuildMemberAdd += async e =>
{
    var nick = e.Member.Nick ?? e.Member.User?.Username ?? "新成员";
    logger.LogInformation("[成员加入] {Nick} 加入频道 {GuildId}", nick, e.GuildId);
};

// 群添加机器人事件
bot.OnGroupAddRobot += async e =>
{
    logger.LogInformation("[群管理] 被添加到群: {GroupId}", e.GroupOpenId);
};

// ==================== API 端点 ====================

// 获取机器人状态
app.MapGet("/api/bot/status", () =>
{
    return Results.Ok(new
    {
        IsConnected = bot.IsConnected,
        CurrentUser = bot.CurrentUser,
        SessionId = bot.WebSocket.Session.SessionId,
        IsAuthenticated = bot.WebSocket.Session.IsAuthenticated
    });
})
.WithName("GetBotStatus")
.WithOpenApi();

// 获取机器人加入的频道列表
app.MapGet("/api/bot/guilds", async () =>
{
    try
    {
        var guilds = await bot.Api.GetCurrentUserGuildsAsync();
        return Results.Ok(guilds);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "获取频道列表失败");
        return Results.Problem(ex.Message);
    }
})
.WithName("GetGuilds")
.WithOpenApi();

// 发送消息到指定频道
app.MapPost("/api/bot/send", async (SendMessageDto dto) =>
{
    try
    {
        if (string.IsNullOrEmpty(dto.ChannelId) || string.IsNullOrEmpty(dto.Content))
        {
            return Results.BadRequest("ChannelId 和 Content 不能为空");
        }
        
        var message = await bot.SendChannelMessageAsync(dto.ChannelId, dto.Content);
        return Results.Ok(message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "发送消息失败");
        return Results.Problem(ex.Message);
    }
})
.WithName("SendMessage")
.WithOpenApi();

// 获取指定频道的子频道列表
app.MapGet("/api/bot/guilds/{guildId}/channels", async (string guildId) =>
{
    try
    {
        var channels = await bot.Api.GetChannelsAsync(guildId);
        return Results.Ok(channels);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "获取子频道列表失败");
        return Results.Problem(ex.Message);
    }
})
.WithName("GetChannels")
.WithOpenApi();

// 健康检查端点
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        BotConnected = bot.IsConnected,
        Timestamp = DateTime.UtcNow
    });
})
.WithName("HealthCheck")
.WithOpenApi();

logger.LogInformation("🚀 ASP.NET Core QQ Bot 示例程序启动");
logger.LogInformation("📖 API 文档: https://localhost:{Port}/swagger", builder.Configuration["ASPNETCORE_URLS"]?.Split(':').LastOrDefault()?.TrimEnd('/') ?? "5001");

app.Run();

// DTO 类
record SendMessageDto(string ChannelId, string Content);
