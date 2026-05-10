using Luolan.QQBot.Controllers;
using Luolan.QQBot.Extensions;
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;

namespace Luolan.QQBot.Example;

/// <summary>
/// API 演示控制器 —— 展示所有 bot.Api 和 bot 快捷方法的用法
/// </summary>
public class ApiDemoController : QQBotController
{
    // ============================================================
    // 命令分发入口: /api <action>
    //    用户发送: /api user       → 获取当前用户信息
    //    用户发送: /api guilds     → 获取频道列表
    //    用户发送: /api channels   → 获取子频道列表
    //    用户发送: /api members    → 获取成员列表
    //    用户发送: /api roles      → 获取角色列表
    //    用户发送: /api mute       → 禁言演示
    //    用户发送: /api announce   → 公告演示
    //    用户发送: /api send       → 消息发送演示
    //    用户发送: /api markdown   → Markdown消息演示
    //    用户发送: /api keyboard   → 键盘消息演示
    //    用户发送: /api dm         → 私信演示
    //    用户发送: /api group      → 群消息演示
    //    用户发送: /api c2c        → C2C消息演示
    //    用户发送: /api reaction   → 表情表态演示
    //    用户发送: /api pins       → 精华消息演示
    //    用户发送: /api schedule   → 日程演示
    //    用户发送: /api gateway    → 网关演示
    // ============================================================
    [Command("api")]
    public async Task<string> ApiDispatch(string action = "help")
    {
        return action.ToLowerInvariant() switch
        {
            "user" => await GetCurrentUser(),
            "guilds" => await GetGuildList(),
            "channels" => await GetChannelList(),
            "members" => await GetMemberList(),
            "roles" => await GetRoleList(),
            "mute" => await DemoMute(),
            "announce" => await DemoAnnounce(),
            "send" => await DemoSendMessage(),
            "markdown" => await DemoMarkdown(),
            "keyboard" => await DemoKeyboard(),
            "dm" => await DemoDirectMessage(),
            "group" => await DemoGroupMessage(),
            "c2c" => await DemoC2CMessage(),
            "reaction" => await DemoReaction(),
            "pins" => await DemoPins(),
            "schedule" => await DemoSchedule(),
            "gateway" => await DemoGateway(),
            _ => ShowApiHelp()
        };
    }

    // ============================================================
    // 用户 API
    // ============================================================
    private async Task<string> GetCurrentUser()
    {
        // bot.Api.GetCurrentUserAsync() —— 获取机器人自身信息
        var user = await Client.Api.GetCurrentUserAsync();
        return $"🤖 机器人信息:\n"
             + $"  用户名: {user.Username}\n"
             + $"  ID: {user.Id}\n"
             + $"  是否Bot: {user.Bot}\n"
             + $"  UnionOpenId: {user.UnionOpenId ?? "无"}";
    }

    private async Task<string> GetGuildList()
    {
        // bot.GetGuildsAsync() —— 快捷方法，等同于 bot.Api.GetCurrentUserGuildsAsync()
        var guilds = await Client.GetGuildsAsync();
        if (guilds.Count == 0)
            return "未加入任何频道";

        var info = guilds.Take(10).Select(g => $"  - {g.Name} (ID: {g.Id})");
        return $"📺 频道列表 (共 {guilds.Count} 个):\n{string.Join("\n", info)}";
    }

    // ============================================================
    // 频道 API
    // ============================================================
    private async Task<string> GetChannelList()
    {
        var guildId = Message.GuildId;
        if (string.IsNullOrEmpty(guildId))
            return "⚠️ 当前消息不包含频道ID，无法演示";

        // bot.Api.GetChannelsAsync() / bot.GetChannelsAsync()
        var channels = await Client.GetChannelsAsync(guildId);
        if (channels.Count == 0)
            return "该频道下没有子频道";

        var info = channels.Take(10).Select(c => $"  - {c.Name} (ID: {c.Id}, 类型: {c.Type})");
        return $"📁 子频道列表 (共 {channels.Count} 个):\n{string.Join("\n", info)}";
    }

    // ============================================================
    // 成员 API
    // ============================================================
    private async Task<string> GetMemberList()
    {
        var guildId = Message.GuildId;
        if (string.IsNullOrEmpty(guildId))
            return "⚠️ 当前消息不包含频道ID";

        // bot.GetMembersAsync() / bot.Api.GetGuildMembersAsync()
        var members = await Client.GetMembersAsync(guildId, limit: 10);
        if (members.Count == 0)
            return "该频道无成员数据";

        var info = members.Take(10).Select(m =>
            $"  - {m.User?.Username ?? "未知"} (ID: {m.User?.Id}, 昵称: {m.Nick})");
        return $"👥 成员列表 (前 {Math.Min(members.Count, 10)} 位):\n{string.Join("\n", info)}";
    }

    // ============================================================
    // 角色 API
    // ============================================================
    private async Task<string> GetRoleList()
    {
        var guildId = Message.GuildId;
        if (string.IsNullOrEmpty(guildId))
            return "⚠️ 当前消息不包含频道ID";

        var response = await Client.Api.GetGuildRolesAsync(guildId);
        if (response.Roles == null || response.Roles.Count == 0)
            return "该频道无角色";

        var info = response.Roles.Take(10).Select(r =>
            $"  - {r.Name} (ID: {r.Id}, 颜色: #{r.Color:X6}, 人数: {r.Number})");
        return $"🎭 角色列表 (共 {response.Roles.Count} 个, 上限 {response.RoleNumLimit}):\n{string.Join("\n", info)}";
    }

    // ============================================================
    // 禁言 API
    // ============================================================
    private async Task<string> DemoMute()
    {
        var guildId = Message.GuildId;
        if (string.IsNullOrEmpty(guildId))
            return "⚠️ 当前消息不包含频道ID";

        // 全员禁言
        // await Client.Api.MuteGuildAsync(guildId, muteSeconds: 60);

        // 禁言单个成员
        // await Client.Api.MuteMemberAsync(guildId, userId, muteSeconds: 60);

        // 批量禁言
        // await Client.Api.MuteMembersAsync(guildId, new[] { "userId1", "userId2" }, muteSeconds: 60);

        return "🔇 禁言 API 说明:\n"
             + "  MuteGuildAsync(guildId, muteSeconds)    — 全员禁言\n"
             + "  MuteMemberAsync(guildId, userId, seconds) — 禁言单个成员\n"
             + "  MuteMembersAsync(guildId, userIds, seconds) — 批量禁言\n"
             + "  参数 muteSeconds=0 表示解除禁言";
    }

    // ============================================================
    // 公告 API
    // ============================================================
    private async Task<string> DemoAnnounce()
    {
        var guildId = Message.GuildId;
        if (string.IsNullOrEmpty(guildId))
            return "⚠️ 当前消息不包含频道ID";

        // 创建公告
        // var announce = await Client.Api.CreateAnnouncesAsync(guildId, channelId, messageId);

        // 删除公告(指定消息ID)
        // await Client.Api.DeleteAnnouncesAsync(guildId, messageId);

        // 删除全部公告
        // await Client.Api.DeleteAnnouncesAsync(guildId, "all");

        return "📢 公告 API 说明:\n"
             + "  CreateAnnouncesAsync(guildId, channelId, messageId) — 创建公告\n"
             + "  DeleteAnnouncesAsync(guildId, messageId)              — 删除公告\n"
             + "  传入 messageId=\"all\" 可删除全部公告";
    }

    // ============================================================
    // 消息发送 (完整 SendMessageRequest)
    // ============================================================
    private async Task<string> DemoSendMessage()
    {
        var channelId = Message.ChannelId;
        if (string.IsNullOrEmpty(channelId))
            return "⚠️ 当前消息不包含子频道ID";

        // 使用完整的 SendMessageRequest 发送
        await Client.Api.SendMessageAsync(channelId, new SendMessageRequest
        {
            Content = "这是一条使用完整请求发送的消息",
            MsgId = Message.Id, // 被动回复
            Embed = new MessageEmbed
            {
                Title = "嵌入标题",
                Description = "嵌入描述内容",
                Thumbnail = new MessageEmbedThumbnail { Url = "https://example.com/thumb.png" }
            }
        });

        return "✅ 消息已发送！";
    }

    // ============================================================
    // Markdown 消息发送 (使用扩展方法)
    // ============================================================
    private async Task<string> DemoMarkdown()
    {
        var channelId = Message.ChannelId;
        if (string.IsNullOrEmpty(channelId))
            return "⚠️ 当前消息不包含子频道ID";

        // 方式一：扩展方法 — 发送 Markdown 内容
        await Client.SendMarkdownContentAsync(channelId,
            "# 演示标题\n" +
            "## 二级标题\n" +
            "- 列表项 1\n" +
            "- 列表项 2\n" +
            "\n**粗体** *斜体* `代码`",
            msgId: Message.Id);

        // 方式二：扩展方法 — 发送 Markdown 模板
        // await Client.SendMarkdownTemplateAsync(channelId, "template_id",
        //     new Dictionary<string, string[]> { ["key"] = new[] { "value" } });

        // 方式三：回复时发送 Markdown
        // await Client.ReplyMarkdownAsync(Message, markdownObj);

        return "✅ Markdown 消息已发送！";
    }

    // ============================================================
    // 键盘消息发送
    // ============================================================
    private async Task<string> DemoKeyboard()
    {
        var channelId = Message.ChannelId;
        if (string.IsNullOrEmpty(channelId))
            return "⚠️ 当前消息不包含子频道ID";

        var keyboard = new KeyboardBuilder()
            .NewRow()
                .AddButton("btn_yes", "✅ 确认", style: 1)
                .AddButton("btn_no", "❌ 取消", style: 0)
            .NewRow()
                .AddLinkButton("btn_link", "🔗 查看详情", "https://github.com/luolangaga/Luolan.QQBot", style: 1)
            .Build();

        await Client.SendMarkdownContentAsync(channelId,
            "# 请确认操作\n确定要执行此操作吗？",
            keyboard: keyboard,
            msgId: Message.Id);

        return "✅ 键盘消息已发送！";
    }

    // ============================================================
    // 私信 API
    // ============================================================
    private async Task<string> DemoDirectMessage()
    {
        var guildId = Message.GuildId;
        var userId = User?.Id;
        if (string.IsNullOrEmpty(guildId) || string.IsNullOrEmpty(userId))
            return "⚠️ 当前上下文不支持私信演示";

        // 创建私信会话并发送
        // var session = await Client.Api.CreateDmsAsync(new CreateDmsRequest
        // {
        //     RecipientId = userId,
        //     SourceGuildId = guildId
        // });
        // await Client.Api.SendDmsAsync(session.GuildId, new SendMessageRequest { Content = "你好！" });

        // 或使用 bot 快捷方法（自动创建会话）
        // await Client.SendDirectMessageAsync(userId, guildId, "你好！");

        return "📩 私信 API 说明:\n"
             + "  CreateDmsAsync(CreateDmsRequest)              — 创建私信会话\n"
             + "  SendDmsAsync(guildId, SendMessageRequest)      — 发送私信\n"
             + "  DeleteDmsAsync(guildId, messageId)             — 撤回私信\n"
             + "  bot.SendDirectMessageAsync(userId, guildId, content) — 快捷方法";
    }

    // ============================================================
    // 群消息 API
    // ============================================================
    private async Task<string> DemoGroupMessage()
    {
        var groupOpenId = Message.GroupOpenId;
        if (string.IsNullOrEmpty(groupOpenId))
            return "⚠️ 当前消息不来自群聊，可用 `/api c2c` 演示私聊";

        // 发送群文本消息
        // await Client.Api.SendGroupTextMessageAsync(groupOpenId, "群消息内容", msgId: Message.Id);

        // 发送群 Markdown 消息
        // await Client.Api.SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
        // {
        //     MsgType = 2,
        //     Markdown = MarkdownBuilder.FromContent("# 群通知"),
        //     MsgId = Message.Id
        // });

        // 上传群文件
        // var uploadResp = await Client.Api.UploadGroupMediaAsync(groupOpenId, new UploadMediaRequest
        // {
        //     FileType = 1, // 1:图片 2:视频 3:语音 4:文件
        //     Url = "https://example.com/image.jpg"
        // });

        return $"👥 群消息 API (当前群: {groupOpenId}):\n"
             + "  SendGroupTextMessageAsync(openId, content) — 发送群文本\n"
             + "  SendGroupMessageAsync(openId, request)       — 发送群消息(完整)\n"
             + "  UploadGroupMediaAsync(openId, request)       — 上传群文件\n"
             + "  bot.ReplyGroupAsync(msg, content)            — 快捷回复";
    }

    // ============================================================
    // C2C 消息 API
    // ============================================================
    private async Task<string> DemoC2CMessage()
    {
        var openId = User?.UserOpenId;
        if (string.IsNullOrEmpty(openId))
            return "⚠️ 无法获取用户的 OpenId";

        // 发送 C2C 文本消息
        // await Client.Api.SendC2CTextMessageAsync(openId, "你好！这是私聊消息");

        // 发送 C2C Markdown
        // await Client.Api.SendC2CMessageAsync(openId, new SendGroupMessageRequest { MsgType = 2, Markdown = markdown });

        // 上传 C2C 文件
        // var uploadResp = await Client.Api.UploadC2CMediaAsync(openId, new UploadMediaRequest { FileType = 1, Url = "..." });

        return $"💬 C2C 消息 API (当前用户: {User?.Username}):\n"
             + "  SendC2CTextMessageAsync(openId, content) — 发送C2C文本\n"
             + "  SendC2CMessageAsync(openId, request)       — 发送C2C消息(完整)\n"
             + "  UploadC2CMediaAsync(openId, request)       — 上传C2C文件\n"
             + "  bot.ReplyC2CAsync(msg, content)            — 快捷回复";
    }

    // ============================================================
    // 表情表态 API
    // ============================================================
    private async Task<string> DemoReaction()
    {
        var channelId = Message.ChannelId;
        if (string.IsNullOrEmpty(channelId))
            return "⚠️ 当前消息不包含子频道ID";

        // 添加表情表态 (点赞)
        // await Client.Api.AddReactionAsync(channelId, Message.Id, "system", "1");

        // 取消表情表态
        // await Client.Api.DeleteReactionAsync(channelId, Message.Id, "system", "1");

        return "👍 表情表态 API:\n"
             + $"  对当前消息 (ID: {Message.Id})\n"
             + "  AddReactionAsync(channelId, msgId, emojiType, emojiId)    — 添加表态\n"
             + "  DeleteReactionAsync(channelId, msgId, emojiType, emojiId) — 取消表态";
    }

    // ============================================================
    // 精华消息 API
    // ============================================================
    private async Task<string> DemoPins()
    {
        var channelId = Message.ChannelId;
        if (string.IsNullOrEmpty(channelId))
            return "⚠️ 当前消息不包含子频道ID";

        // 添加精华
        // await Client.Api.AddPinsMessageAsync(channelId, Message.Id);

        // 获取精华列表
        // var pins = await Client.Api.GetPinsMessageAsync(channelId);

        // 删除精华
        // await Client.Api.DeletePinsMessageAsync(channelId, Message.Id);

        return "📌 精华消息 API:\n"
             + "  AddPinsMessageAsync(channelId, msgId)    — 设为精华\n"
             + "  GetPinsMessageAsync(channelId)           — 获取精华列表\n"
             + "  DeletePinsMessageAsync(channelId, msgId) — 取消精华";
    }

    // ============================================================
    // 日程 API
    // ============================================================
    private async Task<string> DemoSchedule()
    {
        var channelId = Message.ChannelId;
        if (string.IsNullOrEmpty(channelId))
            return "⚠️ 当前消息不包含子频道ID";

        // 获取日程列表
        // var schedules = await Client.Api.GetSchedulesAsync(channelId);

        // 获取日程详情
        // var detail = await Client.Api.GetScheduleAsync(channelId, "scheduleId");

        // 创建日程
        // var schedule = await Client.Api.CreateScheduleAsync(channelId, new Schedule
        // {
        //     Name = "团队会议",
        //     Description = "周例会，讨论本周进度",
        //     StartTimestamp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
        //     EndTimestamp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds()
        // });

        // 修改日程
        // await Client.Api.ModifyScheduleAsync(channelId, "scheduleId", new Schedule { Name = "更新后的会议" });

        // 删除日程
        // await Client.Api.DeleteScheduleAsync(channelId, "scheduleId");

        return "📅 日程 API:\n"
             + "  GetSchedulesAsync(channelId)              — 获取日程列表\n"
             + "  GetScheduleAsync(channelId, scheduleId)    — 获取日程详情\n"
             + "  CreateScheduleAsync(channelId, schedule)    — 创建日程\n"
             + "  ModifyScheduleAsync(channelId, id, schedule) — 修改日程\n"
             + "  DeleteScheduleAsync(channelId, scheduleId)   — 删除日程";
    }

    // ============================================================
    // 网关 API
    // ============================================================
    private async Task<string> DemoGateway()
    {
        var gateway = await Client.Api.GetGatewayAsync();
        var gatewayBot = await Client.Api.GetGatewayBotAsync();

        return $"🌐 网关 API:\n"
             + $"  Gateway URL: {gateway.Url}\n"
             + $"  Gateway Bot URL: {gatewayBot.Url}\n"
             + $"  分片数: {gatewayBot.Shards}\n"
             + $"  剩余连接: {gatewayBot.SessionStartLimit?.Remaining}/{gatewayBot.SessionStartLimit?.Total}";
    }

    // ============================================================
    // 帮助信息
    // ============================================================
    private static string ShowApiHelp()
    {
        return """
            📖 API 演示命令列表:
            /api user        用户 API — 获取机器人信息
            /api guilds      频道 API — 获取频道列表
            /api channels    子频道 API — 获取子频道列表
            /api members     成员 API — 获取成员列表
            /api roles       角色 API — 获取角色列表
            /api mute        禁言 API — 禁言操作
            /api announce    公告 API — 公告管理
            /api send        消息 API — 发送完整消息
            /api markdown    Markdown 消息发送
            /api keyboard    键盘消息发送
            /api dm          私信 API
            /api group       群消息 API
            /api c2c         C2C私聊 API
            /api reaction    表情表态 API
            /api pins        精华消息 API
            /api schedule    日程 API
            /api gateway     网关 API
            """;
    }
}
