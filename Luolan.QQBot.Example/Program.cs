using Luolan.QQBot.Core.Abstractions;
using Luolan.QQBot.Core.Events;
using Luolan.QQBot.Core.Extensions;
using Luolan.QQBot.Core.Models;
using Luolan.QQBot.Official;
using Luolan.QQBot.OneBot;
using Microsoft.Extensions.Logging;

// ==================== 配置区 ====================
// 选择要使用的协议: "Official" 或 "OneBot"
string protocol = "OneBot";

// 官方机器人配置
string appId = Environment.GetEnvironmentVariable("QQBOT_APPID") ?? "";
string clientSecret = Environment.GetEnvironmentVariable("QQBOT_SECRET") ?? "";

// OneBot 配置
string wsUrl = "ws://127.0.0.1:6700"; // OneBot 11 默认端口
string accessToken = "123456";
// ===============================================

Console.WriteLine($"=== Luolan.QQBot 示例程序 ({protocol}) ===");

// 1. 创建日志工厂
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// 2. 创建机器人客户端 (IBotClient)
IBotClient bot;

if (protocol == "Official")
{
    if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientSecret))
    {
        Console.WriteLine("⚠️ 官方模式需要 AppId 和 ClientSecret");
        Console.WriteLine("   请设置环境变量 QQBOT_APPID 和 QQBOT_SECRET");
        return;
    }

    // 使用官方 API 实现
    bot = new OfficialBotClientBuilder()
        .WithAppId(appId)
        .WithClientSecret(clientSecret)
        .UseSandbox(true) // 默认使用沙箱
        .WithIntents(Intents.Default | Intents.GroupAtMessages | Intents.PublicGuildMessages)
        .WithLoggerFactory(loggerFactory)
        .Build();
}
else
{
    // 使用 OneBot 实现 (支持 V11 和 V12)
    bot = new OneBotClientBuilder()
        .UseWebSocket(wsUrl)
        .WithAccessToken(accessToken)
        .WithLoggerFactory(loggerFactory)
        // .WithProtocolVersion(OneBotVersion.V11) // 可以显式指定，默认 Auto
        .Build();
}

// 3. 注册统一事件
// 注意：这套事件处理逻辑适用于所有协议！

// 连接就绪
bot.Events.OnReady += async e =>
{
    Console.WriteLine();
    Console.WriteLine($"✅ 机器人已上线!");
    Console.WriteLine($"   平台: {e.Self?.Extra?["platform"] ?? "QQ"}");
    Console.WriteLine($"   用户: {e.Self?.Username} ({e.Self?.Id})");
    Console.WriteLine();
};

// 收到消息 (统一处理频道、群、私聊消息)
bot.Events.OnMessageReceived += async e =>
{
    var msg = e.Message;
    var sender = msg.Sender;
    var cleanContent = msg.Content?.Trim();

    Console.WriteLine($"[{e.SourceType}] {sender?.Username} ({sender?.Id}): {cleanContent}");

    // 频道/群组 里的 @回复
    if (e.SourceType != MessageSourceType.Private && cleanContent == "你好")
    {
        // 构建复合消息: @用户 + 文本
        var segments = new[]
        {
            MessageSegment.At(sender?.Id ?? ""),
            MessageSegment.Text(" 你好呀！")
        };
        await bot.Messages.SendMessageAsync(msg.Source, segments, msg.Id);
    }
};

// 成员变动
bot.Events.OnMemberChanged += async e =>
{
    Console.WriteLine($"[成员变动] {e.ChangeType}: {e.Member?.Username} (Group: {e.GroupId})");
};

bot.Events.OnRawEvent += async e =>
{
    // 调试：输出原始事件类型
    // Console.WriteLine($"[DEBUG] Raw Event: {e.EventType}"); 
};

// 4. 启用控制器模式 (推荐)
// 自动扫描当前程序集中的 Controller (见 ExampleController.cs)
bot.UseControllers("/");

// 5. 启动机器人
try
{
    Console.WriteLine("正在连接...");
    await bot.StartAsync();

    Console.WriteLine("机器人运行中... 按 Ctrl+C 退出");
    
    // 等待退出信号
    var tcs = new TaskCompletionSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        tcs.SetResult();
    };
    
    await tcs.Task;
}
catch (Exception ex)
{
    Console.WriteLine($"发生错误: {ex.Message}");
}
finally
{
    await bot.StopAsync();
}
