using Luolan.QQBot.Core.Abstractions;
using Luolan.QQBot.Core.Extensions;
using Luolan.QQBot.Core.Models;
using Luolan.QQBot.Official;
using Luolan.QQBot.OneBot;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// ==================== 配置机器人 ====================
var botConfig = builder.Configuration.GetSection("Bot");
var protocol = botConfig["Protocol"] ?? "Official";

Console.WriteLine($"🚀 正在使用 {protocol} 协议启动机器人...");

if (protocol.Equals("Official", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOfficialBot(options =>
    {
        var officialConfig = botConfig.GetSection("Official");
        options.AppId = officialConfig["AppId"] ?? throw new InvalidOperationException("未配置 AppId");
        options.ClientSecret = officialConfig["ClientSecret"] ?? throw new InvalidOperationException("未配置 ClientSecret");
        options.IsSandbox = officialConfig.GetValue<bool>("IsSandbox");
        
        // 订阅所有事件
        options.Intents = Intents.Default | Intents.GroupAtMessages | Intents.PublicGuildMessages;
    });
}
else
{
    builder.Services.AddOneBot(options =>
    {
        var oneBotConfig = botConfig.GetSection("OneBot");
        options.WebSocketUrl = oneBotConfig["WebSocketUrl"] ?? "ws://127.0.0.1:3001";
        options.AccessToken = oneBotConfig["AccessToken"];
    });
}

// 注册启动服务 (负责启动机器人)
builder.Services.AddHostedService<BotBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==================== 配置机器人事件 ====================
// 获取 IBotClient (通用接口)
var bot = app.Services.GetRequiredService<IBotClient>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// ------ 通用事件处理 (适用于所有协议) ------

bot.Events.OnReady += async e =>
{
    logger.LogInformation("✅ 机器人已上线!");
    logger.LogInformation("   用户: {Username} ({UserId})", e.Self?.Username, e.Self?.Id);
    logger.LogInformation("   平台: {Platform}", e.Self?.Extra?["platform"] ?? "QQ");
};

bot.Events.OnMessageReceived += async e =>
{
    var msg = e.Message;
    var content = msg.Content?.Trim() ?? "";
    var sender = msg.Sender?.Username ?? "未知用户";
    
    logger.LogInformation("[{Source}] {Sender}: {Content}", e.SourceType, sender, content);
};

bot.Events.OnMemberChanged += async e =>
{
    logger.LogInformation("[成员变动] {Type}: {Member} in {Group}", e.ChangeType, e.Member?.Username, e.GroupId);
};

// ------ 启用控制器模式 ------
// 这将扫描当前程序集中的 BotController
bot.UseControllers("/");

// ==================== API 端点 ====================
app.MapControllers();

app.Run();



// 此时我们需要定义一个后台服务来启动机器人
public class BotBackgroundService : BackgroundService
{
    private readonly IBotClient _botClient;
    private readonly ILogger<BotBackgroundService> _logger;

    public BotBackgroundService(IBotClient botClient, ILogger<BotBackgroundService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("正在连接机器人...");
            await _botClient.StartAsync(stoppingToken);
            
            // 保持运行直到取消
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // 正常停止
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "机器人服务发生错误");
        }
        finally
        {
            _logger.LogInformation("正在停止机器人...");
            await _botClient.StopAsync();
        }
    }
}
