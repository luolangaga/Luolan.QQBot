using Luolan.QQBot;
using Luolan.QQBot.Controllers;
using Luolan.QQBot.Events;
using Luolan.QQBot.Extensions;
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;
using Luolan.QQBot.Services;
using Microsoft.Extensions.Logging;

// ============================================================
//  Luolan.QQBot 完整示例程序
//  本文件演示了 SDK 的全部功能，包括：
//  1. Builder 模式配置（所有选项）
//  2. 全部事件监听
//  3. 控制器模式（自动命令路由）
//  4. 手动 API 调用示例（频道管理、成员管理、角色管理等）
//  5. Markdown 消息构建
//  6. 互动键盘构建
//  7. ASP.NET Core 依赖注入集成模板
// ============================================================

#region 配置区 —— 请填写你的机器人信息

string appId = Environment.GetEnvironmentVariable("QQBOT_APPID") ?? "xxx";
string clientSecret = Environment.GetEnvironmentVariable("QQBOT_SECRET") ?? "xxxx";
bool useSandbox = true; // 是否使用沙箱环境

#endregion

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     Luolan.QQBot 完整示例程序 v1.4.0      ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

// ============================================================
// 1. 创建日志工厂
// ============================================================
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger("Example");

// ============================================================
// 2. Builder 模式 —— 全部配置选项演示
// ============================================================
logger.LogInformation("正在构建 QQBotClient...");

var bot = new QQBotClientBuilder()
    // --- 必填 ---
    .WithAppId(appId)
    .WithClientSecret(clientSecret)

    // --- 环境设置 ---
    .UseSandbox(useSandbox) // true=沙箱环境, false=正式环境

    // --- Intents 事件订阅 ---
    // ⚠️ 错误 4014 "disallowed intents" 表示你的机器人没有某些 Intents 权限
    //    请根据机器人实际拥有的权限选择 Intents，不要订阅未授权的 Intents
    //    沙箱/公域机器人通常只能使用 Default + GroupAtMessages
    .WithIntents(
        Intents.Default                 // 公域默认: Guilds | GuildMembers | PublicGuildMessages
                                        //           | GuildMessageReactions | DirectMessage
                                        //           | Interaction | MessageAudit
        | Intents.GroupAtMessages       // 群聊 @ 机器人消息 (需要单独申请)
        // 以下 Intents 需要额外权限，没有权限会导致 4014 错误：
        // | Intents.C2CMessages        // C2C 私聊消息
        // | Intents.GuildMessages      // 频道全量消息 (仅私域)
        // | Intents.Forums             // 论坛事件
        // | Intents.AudioAction        // 音频事件
    )

    // --- Token 管理 ---
    .WithTokenRefreshBeforeExpire(120)  // 在过期前 120 秒自动刷新 Token

    // --- WebSocket 重连 ---
    .WithWebSocketReconnectInterval(3000)   // 重连间隔 3 秒
    .WithWebSocketMaxReconnectAttempts(20)  // 最多重连 20 次

    // --- 日志 ---
    .WithLoggerFactory(loggerFactory)

    // --- 构建 ---
    .Build();

logger.LogInformation("QQBotClient 构建完成！");

// ============================================================
// 3. 注册全部事件
// ============================================================
logger.LogInformation("正在注册事件处理器...");

// 3.1 连接就绪 —— 机器人上线后触发
bot.OnReady += async e =>
{
    logger.LogInformation("✅ 机器人已上线！");
    logger.LogInformation("   版本: {Version}", e.Version);
    logger.LogInformation("   会话ID: {SessionId}", e.SessionId);
    logger.LogInformation("   用户名: {Username}", e.User?.Username);
    logger.LogInformation("   用户ID: {UserId}", e.User?.Id);
    await Task.CompletedTask;
};

// 3.2 频道消息 (私域) —— 接收频道内所有普通消息
bot.OnMessageCreate += async e =>
{
    var msg = e.Message;
    logger.LogInformation("[频道消息] {Author} ({Id}): {Content}",
        msg.Author?.Username, msg.Author?.Id, msg.Content);
    await Task.CompletedTask;
};

// 3.3 @机器人消息 (公域) —— 频道中 @机器人 时触发
bot.OnAtMessageCreate += async e =>
{
    var msg = e.Message;
    logger.LogInformation("[公域@消息] {Author}: {Content}",
        msg.Author?.Username, msg.Content);

    // 可以在这里做一些简单的文本处理
    if (msg.Content?.Trim() == "你好")
    {
        await bot.ReplyAsync(msg, "你好呀！有什么可以帮你的吗？");
    }
};

// 3.4 群 @ 机器人消息 —— 群聊中 @机器人 时触发
bot.OnGroupAtMessageCreate += async e =>
{
    var msg = e.Message;
    logger.LogInformation("[群@消息] 群:{GroupOpenId} {Author}: {Content}",
        msg.GroupOpenId, msg.Author?.Username, msg.Content);

    if (msg.Content?.Trim() == "你好")
    {
        await bot.ReplyGroupAsync(msg, "大家好！我是机器人 🤖");
    }
};

// 3.5 C2C 私聊消息 —— 用户私聊机器人时触发
bot.OnC2CMessageCreate += async e =>
{
    var msg = e.Message;
    logger.LogInformation("[C2C私聊] {Author}: {Content}",
        msg.Author?.Username, msg.Content);

    if (msg.Content?.Trim() == "你好")
    {
        await bot.ReplyC2CAsync(msg, "你好！这是私聊消息回复。");
    }
};

// 3.6 频道私信消息
bot.OnDirectMessageCreate += async e =>
{
    var msg = e.Message;
    logger.LogInformation("[频道私信] {Author}: {Content}",
        msg.Author?.Username, msg.Content);
    await Task.CompletedTask;
};

// 3.7 互动事件 —— 用户点击按钮时触发
bot.OnInteractionCreate += async e =>
{
    var interaction = e.Interaction;
    logger.LogInformation("[互动事件] 类型:{Type} 按钮ID:{ButtonId} 用户ID:{UserId}",
        interaction.Type, interaction.Data?.Resolved?.ButtonId, interaction.UserOpenId);

    // 根据按钮ID做不同处理
    var buttonId = interaction.Data?.Resolved?.ButtonId;
    if (buttonId == "confirm_btn")
    {
        // 处理确认按钮点击...
        logger.LogInformation("用户点击了确认按钮");
    }
    await Task.CompletedTask;
};

// 3.8 频道创建 —— 机器人被加入新频道时触发
bot.OnGuildCreate += async e =>
{
    logger.LogInformation("[频道创建] 频道ID:{GuildId} 名称:{Name}",
        e.Guild?.Id, e.Guild?.Name);
    await Task.CompletedTask;
};

// 3.9 频道删除 —— 机器人被移出频道时触发
bot.OnGuildDelete += async e =>
{
    logger.LogInformation("[频道删除] 频道ID:{GuildId}",
        e.Guild?.Id);
    await Task.CompletedTask;
};

// 3.10 成员加入
bot.OnGuildMemberAdd += async e =>
{
    logger.LogInformation("[成员加入] 用户:{Username} ({UserId}) 频道:{GuildId}",
        e.Member?.User?.Username, e.Member?.User?.Id, e.GuildId);
    await Task.CompletedTask;
};

// 3.11 成员离开
bot.OnGuildMemberRemove += async e =>
{
    logger.LogInformation("[成员离开] 用户:{Username} ({UserId}) 频道:{GuildId}",
        e.Member?.User?.Username, e.Member?.User?.Id, e.GuildId);
    await Task.CompletedTask;
};

// 3.12 机器人被加入群
bot.OnGroupAddRobot += async e =>
{
    logger.LogInformation("[群添加机器人] 群:{GroupOpenId}", e.GroupOpenId);
    await Task.CompletedTask;
};

// 3.13 机器人被移出群
bot.OnGroupDelRobot += async e =>
{
    logger.LogInformation("[群移除机器人] 群:{GroupOpenId}", e.GroupOpenId);
    await Task.CompletedTask;
};

// 3.14 好友添加
bot.OnFriendAdd += async e =>
{
    logger.LogInformation("[好友添加] 用户:{OpenId}", e.OpenId);
    await Task.CompletedTask;
};

// 3.15 好友删除
bot.OnFriendDel += async e =>
{
    logger.LogInformation("[好友删除] 用户:{OpenId}", e.OpenId);
    await Task.CompletedTask;
};

logger.LogInformation("事件处理器注册完成！共注册 15 个事件。");

// ============================================================
// 4. 启用控制器模式 (推荐)
//    自动扫描当前程序集中的所有 QQBotController 子类
//    命令前缀为 "/"，即用户发送 "/hello" 会触发 ExampleController.Hello()
// ============================================================
bot.UseControllers();
logger.LogInformation("控制器模式已启用，命令前缀: '/'");

// ============================================================
// 5. 演示：手动 API 调用（在机器人启动后可用）
//    以下代码展示了如何使用 bot.Api 进行各种 API 操作
//    实际的 ID 需要从事件中获取，此处为演示写法
// ============================================================
async Task DemonstrateAllApis()
{
    // 这些调用需要实际的 guildId / channelId 等参数
    // 在实际使用中，这些值从事件（如 OnReady, OnMessageCreate）中获取
    var exampleGuildId = "从事件获取";
    var exampleChannelId = "从事件获取";
    var exampleUserId = "从事件获取";
    var exampleRoleId = "从事件获取";
    _ = exampleRoleId; // demo

    try
    {
        Console.WriteLine("--- API 功能演示开始 ---");

        // --- 5.1 用户 API ---
        var currentUser = await bot.Api.GetCurrentUserAsync();
        Console.WriteLine($"[用户API] 当前机器人: {currentUser.Username}");

        var guilds = await bot.Api.GetCurrentUserGuildsAsync();
        Console.WriteLine($"[用户API] 加入的频道数: {guilds.Count}");

        // --- 5.2 频道 API ---
        if (guilds.Count > 0)
        {
            exampleGuildId = guilds[0].Id;
            var guild = await bot.Api.GetGuildAsync(exampleGuildId);
            Console.WriteLine($"[频道API] 频道名称: {guild.Name}");
        }

        // --- 5.3 子频道 API ---
        var channels = await bot.Api.GetChannelsAsync(exampleGuildId);
        Console.WriteLine($"[子频道API] 子频道数: {channels.Count}");

        if (channels.Count > 0)
        {
            exampleChannelId = channels[0].Id;
            var channel = await bot.Api.GetChannelAsync(exampleChannelId);
            Console.WriteLine($"[子频道API] 子频道名称: {channel.Name}");
        }

        // 创建子频道
        // var newChannel = await bot.Api.CreateChannelAsync(exampleGuildId, new { name = "新频道", type = 0 });

        // 修改子频道
        // var modifiedChannel = await bot.Api.ModifyChannelAsync(exampleChannelId, new { name = "新名字" });

        // 删除子频道
        // await bot.Api.DeleteChannelAsync(channelId);

        // --- 5.4 成员 API ---
        var members = await bot.Api.GetGuildMembersAsync(exampleGuildId, limit: 10);
        Console.WriteLine($"[成员API] 成员数: {members.Count}");

        if (members.Count > 0)
        {
            exampleUserId = members[0].User?.Id ?? "";
            var member = await bot.Api.GetGuildMemberAsync(exampleGuildId, exampleUserId);
            Console.WriteLine($"[成员API] 成员昵称: {member.Nick}");
        }

        // 移除成员
        // await bot.Api.DeleteGuildMemberAsync(exampleGuildId, exampleUserId, addBlacklist: true, deleteHistoryMsgDays: 7);

        // --- 5.5 角色 API ---
        var rolesResponse = await bot.Api.GetGuildRolesAsync(exampleGuildId);
        Console.WriteLine($"[角色API] 角色数: {rolesResponse.Roles?.Count}");

        // 创建角色
        // var newRole = await bot.Api.CreateGuildRoleAsync(exampleGuildId, new CreateRoleRequest
        // {
        //     Name = "测试角色",
        //     Color = 0xFF0000,
        //     Hoist = 1
        // });
        // exampleRoleId = newRole.RoleId;

        // 修改角色
        // await bot.Api.ModifyGuildRoleAsync(exampleGuildId, exampleRoleId, new CreateRoleRequest { Name = "新名称" });

        // 删除角色
        // await bot.Api.DeleteGuildRoleAsync(exampleGuildId, exampleRoleId);

        // 添加/移除身份组成员
        // await bot.Api.AddMemberToRoleAsync(exampleGuildId, exampleUserId, exampleRoleId);
        // await bot.Api.RemoveMemberFromRoleAsync(exampleGuildId, exampleUserId, exampleRoleId);

        // --- 5.6 子频道权限 API ---
        // var userPerms = await bot.Api.GetChannelPermissionsAsync(exampleChannelId, exampleUserId);
        // var rolePerms = await bot.Api.GetChannelRolePermissionsAsync(exampleChannelId, exampleRoleId);
        // await bot.Api.ModifyChannelPermissionsAsync(exampleChannelId, exampleUserId, new UpdateChannelPermissionsRequest { ... });
        // await bot.Api.ModifyChannelRolePermissionsAsync(exampleChannelId, exampleRoleId, new UpdateChannelPermissionsRequest { ... });

        // --- 5.7 消息 API ---
        // var message = await bot.Api.GetMessageAsync(exampleChannelId, "messageId");
        // var sentMsg = await bot.Api.SendTextMessageAsync(exampleChannelId, "主动发送的消息");
        // await bot.Api.DeleteMessageAsync(exampleChannelId, "messageId", hideTip: true);

        // --- 5.8 私信 API ---
        // var dmSession = await bot.Api.CreateDmsAsync(new CreateDmsRequest
        // {
        //     RecipientId = exampleUserId,
        //     SourceGuildId = exampleGuildId
        // });
        // await bot.Api.SendDmsAsync(dmSession.GuildId, new SendMessageRequest { Content = "你好！" });
        // await bot.Api.DeleteDmsAsync(dmSession.GuildId, "messageId");

        // --- 5.9 群消息 API ---
        // await bot.Api.SendGroupTextMessageAsync("groupOpenId", "群消息内容");
        // await bot.Api.SendGroupMessageAsync("groupOpenId", new SendGroupMessageRequest { Content = "群消息", MsgType = 0 });
        // var uploadResp = await bot.Api.UploadGroupMediaAsync("groupOpenId", new UploadMediaRequest
        // {
        //     FileType = 1, // 1=图片 2=视频 3=语音 4=文件
        //     Url = "https://example.com/image.jpg"
        // });

        // --- 5.10 C2C 消息 API ---
        // await bot.Api.SendC2CTextMessageAsync("userOpenId", "私聊内容");
        // await bot.Api.SendC2CMessageAsync("userOpenId", new SendGroupMessageRequest { Content = "私聊", MsgType = 0 });
        // var c2cUploadResp = await bot.Api.UploadC2CMediaAsync("userOpenId", new UploadMediaRequest
        // {
        //     FileType = 1,
        //     Url = "https://example.com/photo.jpg"
        // });

        // --- 5.11 表情表态 API ---
        // await bot.Api.AddReactionAsync(exampleChannelId, "messageId", "system", "1");  // 点赞
        // await bot.Api.DeleteReactionAsync(exampleChannelId, "messageId", "system", "1");

        // --- 5.12 精华消息 API ---
        // await bot.Api.AddPinsMessageAsync(exampleChannelId, "messageId");
        // var pins = await bot.Api.GetPinsMessageAsync(exampleChannelId);
        // await bot.Api.DeletePinsMessageAsync(exampleChannelId, "messageId");

        // --- 5.13 日程 API ---
        // var schedules = await bot.Api.GetSchedulesAsync(exampleChannelId);
        // var scheduleDetail = await bot.Api.GetScheduleAsync(exampleChannelId, "scheduleId");
        // var newSchedule = await bot.Api.CreateScheduleAsync(exampleChannelId, new Schedule
        // {
        //     Name = "新日程",
        //     Description = "日程描述",
        //     StartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        //     EndTimestamp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
        // });
        // await bot.Api.ModifyScheduleAsync(exampleChannelId, "scheduleId", new Schedule { Name = "更新后的日程" });
        // await bot.Api.DeleteScheduleAsync(exampleChannelId, "scheduleId");

        // --- 5.14 禁言 API ---
        // await bot.Api.MuteGuildAsync(exampleGuildId, muteSeconds: 60);                  // 全员禁言60秒
        // await bot.Api.MuteMemberAsync(exampleGuildId, exampleUserId, muteSeconds: 60);   // 禁言单个成员
        // await bot.Api.MuteMembersAsync(exampleGuildId, new[] { "userId1", "userId2" }, muteSeconds: 60); // 批量禁言

        // --- 5.15 公告 API ---
        // var announce = await bot.Api.CreateAnnouncesAsync(exampleGuildId, exampleChannelId, "messageId");
        // await bot.Api.DeleteAnnouncesAsync(exampleGuildId, "messageId"); // 或 "all" 删除全部

        // --- 5.16 WebSocket 网关 API ---
        // var gateway = await bot.Api.GetGatewayAsync();
        // var gatewayBot = await bot.Api.GetGatewayBotAsync();
        // Console.WriteLine($"[网关API] 网关URL: {gateway.Url}");

        // --- 5.17 QQBotClient 快捷方法 ---
        // 这些是 bot 实例上可以直接调用的方法，内部封装了 bot.Api 的调用
        // var guilds2 = await bot.GetGuildsAsync();                   // 获取频道列表
        // var guild2 = await bot.GetGuildAsync(exampleGuildId);       // 获取频道详情
        // var channels2 = await bot.GetChannelsAsync(exampleGuildId); // 获取子频道列表
        // var members2 = await bot.GetMembersAsync(exampleGuildId);   // 获取成员列表
        // await bot.ReplyAsync(msg, "回复频道消息");                  // 自动回复频道消息
        // await bot.ReplyGroupAsync(msg, "回复群消息");               // 自动回复群消息
        // await bot.ReplyC2CAsync(msg, "回复私聊消息");               // 自动回复私聊消息
        // await bot.SendChannelMessageAsync("channelId", "主动发频道消息");
        // await bot.SendGroupMessageAsync("groupOpenId", "主动发群消息");
        // await bot.SendC2CMessageAsync("userOpenId", "主动发私聊消息");

        Console.WriteLine("--- API 功能演示结束 ---");
    }
    catch (Exception ex)
    {
        // API 调用可能因为 QQBotApiException 或网络问题失败
        Console.WriteLine($"[API演示异常] {ex.GetType().Name}: {ex.Message}");
    }
}

// ============================================================
// 6. 演示：Markdown 消息构建
// ============================================================
void DemonstrateMarkdownAndKeyboard()
{
    Console.WriteLine("--- Markdown 与键盘演示 ---");

    // 方式一：Builder 模式 (推荐)
    var markdown = new MarkdownBuilder()
        .UseContent("# 机器人状态\n"
                  + "## 系统信息\n"
                  + "- **状态**: 正常运行\n"
                  + "- **延迟**: 50ms\n"
                  + "\n## 功能列表\n"
                  + "1. 自动回复\n"
                  + "2. 群管理\n"
                  + "3. 数据统计")
        .Build();

    // 方式二：静态工厂方法
    var simpleMarkdown = MarkdownBuilder.FromContent("**粗体** *斜体* `代码`");

    // 方式三：模板 + 参数
    var templateMarkdown = new MarkdownBuilder()
        .UseTemplate("your_template_id_here") // 需要先在QQ开放平台申请模板
        .AddParam("key1", "value1", "value2")
        .SetParam("key2", "替换值")
        .Build();

    // 键盘构建器
    var keyboard = new KeyboardBuilder()
        .NewRow()
            .AddButton("btn_help", "帮助", style: 1)
            .AddButton("btn_info", "信息", style: 0)
        .NewRow()
            .AddLinkButton("btn_github", "GitHub", "https://github.com/luolangaga/Luolan.QQBot", style: 1)
        .NewRow()
            .AddButton("btn_confirm", "✅ 确认", style: 1)
            .AddButton("btn_cancel", "❌ 取消", style: 0)
        .Build();

    // 快速创建单行键盘
    var quickKeyboard = KeyboardBuilder.FromButtons(
        new Button
        {
            Id = "btn1",
            RenderData = new ButtonRenderData { Label = "按钮1", VisitedLabel = "已点击", Style = 1 },
            Action = new ButtonAction { Type = 2, Permission = new ButtonPermission { Type = 2 }, Data = "btn1" }
        }
    );

    Console.WriteLine("   使用 bot.SendMarkdownAsync(channelId, markdown, keyboard) 发送");
    Console.WriteLine("   或使用扩展方法 bot.SendMarkdownContentAsync(channelId, content)");

    // 发送示例：
    // await bot.SendMarkdownAsync(channelId, markdown, keyboard);
    // await bot.SendMarkdownContentAsync(channelId, "# 标题\n内容");
    // await bot.SendMarkdownTemplateAsync(channelId, "template_id", new Dictionary<string, string[]> { ["key"] = new[] { "val" } });
    // await bot.ReplyMarkdownAsync(message, markdown, keyboard);
    // await bot.ReplyMarkdownTemplateAsync(message, "template_id");

    Console.WriteLine("--- Markdown 演示结束 ---");
}

// ============================================================
// 7. 演示：CommandParser 参数解析
// ============================================================
void DemonstrateCommandParser()
{
    Console.WriteLine("--- CommandParser 演示 ---");

    // 支持引号包裹的参数
    var args1 = CommandParser.Parse("/say \"hello world\" test 123");
    Console.WriteLine($"  输入: /say \"hello world\" test 123");
    Console.WriteLine($"  解析: [{string.Join("], [", args1)}]");

    // 支持转义
    var args2 = CommandParser.Parse("/cmd arg1 \"arg with \\\"quote\\\"\" arg3");
    Console.WriteLine($"  输入: /cmd arg1 \"arg with \\\"quote\\\"\" arg3");
    Console.WriteLine($"  解析: [{string.Join("], [", args2)}]");

    Console.WriteLine("--- CommandParser 演示结束 ---");
}

// ============================================================
// 8. 演示：RateLimiter 速率限制
// ============================================================
void DemonstrateRateLimiter()
{
    Console.WriteLine("--- RateLimiter 演示 ---");
    Console.WriteLine("  SDK 内置 Token Bucket 速率限制器");
    Console.WriteLine("  默认容量: 60 令牌/分钟 (匹配 QQ API 限制)");
    Console.WriteLine("  所有 API 调用自动限速，无需手动处理");

    // 自定义速率限制器
    var limiter = new RateLimiter(capacity: 30, refillRate: 1);
    if (limiter.TryAcquire("my-key"))
    {
        Console.WriteLine("  ✓ 成功获取令牌");
    }
    else
    {
        Console.WriteLine("  ✗ 令牌不足，需要等待");
    }

    Console.WriteLine("--- RateLimiter 演示结束 ---");
}

// ============================================================
// 9. 演示：Intents 组合
// ============================================================
void DemonstrateIntents()
{
    Console.WriteLine("--- Intents 事件订阅演示 ---");
    Console.WriteLine($"  None                 = {(int)Intents.None}");
    Console.WriteLine($"  Guilds               = {(int)Intents.Guilds}      (1<<0)  频道事件");
    Console.WriteLine($"  GuildMembers         = {(int)Intents.GuildMembers}    (1<<1)  成员事件");
    Console.WriteLine($"  GuildMessages        = {(int)Intents.GuildMessages}   (1<<9)  频道全量消息(私域)");
    Console.WriteLine($"  GuildMessageReactions= {(int)Intents.GuildMessageReactions} (1<<10) 表情表态");
    Console.WriteLine($"  DirectMessage        = {(int)Intents.DirectMessage}   (1<<12) 私信");
    Console.WriteLine($"  Interaction          = {(int)Intents.Interaction}     (1<<26) 互动回调");
    Console.WriteLine($"  MessageAudit         = {(int)Intents.MessageAudit}    (1<<27) 消息审核");
    Console.WriteLine($"  Forums               = {(int)Intents.Forums}          (1<<28) 论坛");
    Console.WriteLine($"  AudioAction          = {(int)Intents.AudioAction}     (1<<29) 音频");
    Console.WriteLine($"  PublicGuildMessages  = {(int)Intents.PublicGuildMessages} (1<<30) 公域@消息");
    Console.WriteLine($"  GroupAtMessages      = {(int)Intents.GroupAtMessages} (1<<25) 群@消息");
    Console.WriteLine($"  C2CMessages          = {(int)Intents.C2CMessages}     (1<<25) C2C消息");
    Console.WriteLine();
    Console.WriteLine($"  Default              = {(int)Intents.Default}");
    Console.WriteLine($"  PrivateAll           = {(int)Intents.PrivateAll}");
    Console.WriteLine("--- Intents 演示结束 ---");
}

// ============================================================
// 10. ASP.NET Core 集成模板（注释形式演示）
// ============================================================
void PrintAspNetIntegrationGuide()
{
    Console.WriteLine("--- ASP.NET Core 集成指南 ---");
    Console.WriteLine("方式一：标准集成");
    Console.WriteLine("```csharp");
    Console.WriteLine("// Program.cs");
    Console.WriteLine("builder.Services.AddQQBot(options => {");
    Console.WriteLine("    options.AppId = \"...\";");
    Console.WriteLine("    options.ClientSecret = \"...\";");
    Console.WriteLine("    options.Intents = Intents.Default;");
    Console.WriteLine("});");
    Console.WriteLine("builder.Services.AddQQBotHostedService();");
    Console.WriteLine("```");
    Console.WriteLine();
    Console.WriteLine("方式二：IHttpClientFactory 集成 (推荐，性能更优)");
    Console.WriteLine("```csharp");
    Console.WriteLine("builder.Services.AddQQBotWithHttpClientFactory(options => {");
    Console.WriteLine("    options.AppId = \"...\";");
    Console.WriteLine("    options.ClientSecret = \"...\";");
    Console.WriteLine("    options.Intents = Intents.Default;");
    Console.WriteLine("});");
    Console.WriteLine("builder.Services.AddQQBotHostedService();");
    Console.WriteLine();

    // 注意：AddQQBotWithHttpClientFactory 在 Luolan.QQBot.Extensions 命名空间中
    // 使用时需要: using Luolan.QQBot.Extensions;
    Console.WriteLine("```");
    Console.WriteLine("--- ASP.NET Core 集成指南结束 ---");
}

// ============================================================
// 11. 运行演示（在启动机器人之前）
// ============================================================
Console.WriteLine();
DemonstrateCommandParser();
Console.WriteLine();
DemonstrateRateLimiter();
Console.WriteLine();
DemonstrateIntents();
Console.WriteLine();
DemonstrateMarkdownAndKeyboard();
Console.WriteLine();
PrintAspNetIntegrationGuide();
Console.WriteLine();

// ============================================================
// 12. 启动机器人
// ============================================================
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("正在连接 QQ Bot API...");

try
{
    // 检查配置
    // 取消下面注释可在连接后演示所有 API：
    // await Task.Delay(3000);
    // await DemonstrateAllApis();
    Func<Task> _unused = DemonstrateAllApis; // 保留函数引用，避免编译警告

    if (appId == "你的AppId" || clientSecret == "你的ClientSecret")
    {
        Console.WriteLine("⚠️ 请先设置环境变量 QQBOT_APPID 和 QQBOT_SECRET");
        Console.WriteLine("   或在代码中直接填写你的 AppId 和 ClientSecret");
        Console.WriteLine();
        Console.WriteLine("   Windows (CMD):");
        Console.WriteLine("     set QQBOT_APPID=你的AppId");
        Console.WriteLine("     set QQBOT_SECRET=你的ClientSecret");
        Console.WriteLine();
        Console.WriteLine("   Windows (PowerShell):");
        Console.WriteLine("     $env:QQBOT_APPID=\"你的AppId\"");
        Console.WriteLine("     $env:QQBOT_SECRET=\"你的ClientSecret\"");
        Console.WriteLine();
        Console.WriteLine("   按任意键退出...");
        Console.ReadKey();
        return;
    }

    await bot.StartAsync();
    Console.WriteLine("✅ 机器人已启动！");
    Console.WriteLine($"   当前用户: {bot.CurrentUser?.Username ?? "(获取中...)"}");
    Console.WriteLine("   按 Ctrl+C 退出");
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine();

    // 如果在实际连接环境中，可以取消注释以下行来演示 API：
    // await Task.Delay(3000); // 等待连接稳定
    // await DemonstrateAllApis();

    // 等待退出信号
    var tcs = new TaskCompletionSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        Console.WriteLine();
        Console.WriteLine("正在优雅退出...");
        tcs.SetResult();
    };

    await tcs.Task;
}
catch (QQBotApiException apiEx)
{
    logger.LogError("QQ API 错误: [{Code}] {Message}, TraceId: {TraceId}",
        apiEx.Code, apiEx.Message, apiEx.TraceId);
    Console.WriteLine($"API 错误: [{apiEx.Code}] {apiEx.Message}");
    Console.WriteLine("请检查 AppId 和 ClientSecret 是否正确");
}
catch (Exception ex)
{
    logger.LogError(ex, "运行错误: {Message}", ex.Message);
    Console.WriteLine($"运行出错: {ex.GetType().Name} - {ex.Message}");
}
finally
{
    Console.WriteLine("正在停止机器人...");
    await bot.StopAsync();
    logger.LogInformation("机器人已停止。");
    Console.WriteLine("已退出。");
}

// ============================================================
// 附录：可用的事件类型完整列表
// ============================================================
// OnReady                 - 连接就绪
// OnMessageCreate         - 频道消息(私域)
// OnAtMessageCreate       - @机器人消息(公域)
// OnDirectMessageCreate   - 私信消息
// OnGroupAtMessageCreate  - 群@机器人消息
// OnC2CMessageCreate      - C2C私聊消息
// OnInteractionCreate     - 互动事件(按钮回调)
// OnGuildCreate           - 频道创建
// OnGuildDelete           - 频道删除
// OnGuildMemberAdd        - 成员加入
// OnGuildMemberRemove     - 成员离开
// OnGroupAddRobot         - 机器人加入群
// OnGroupDelRobot         - 机器人退出群
// OnFriendAdd             - 好友添加
// OnFriendDel             - 好友删除
// (更多事件可通过 bot.Events.OnXxx 访问)
