using Luolan.QQBot.Controllers;
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Example;

/// <summary>
/// 示例控制器 —— 演示命令模式的所有功能
/// </summary>
public class ExampleController : QQBotController
{
    // ============================================================
    // 1. 基础命令：返回字符串
    //    用户发送: /hello
    //    用户发送: /hello 小明
    //    别名: /hi
    // ============================================================
    [Command("hello", "hi")]
    public string Hello(string? name = null)
    {
        name ??= User?.Username ?? "未知用户";
        return $"你好，{name}！";
    }

    // ============================================================
    // 2. 基础类型参数 (int)
    //    用户发送: /add 10 20
    //    SDK 自动将字符串 "10" "20" 转为 int
    // ============================================================
    [Command("add")]
    public string Add(int a, int b)
    {
        return $"{a} + {b} = {a + b}";
    }

    // ============================================================
    // 3. 多种数值类型 (long, double, decimal)
    //    用户发送: /calc 100 3.14
    // ============================================================
    [Command("calc")]
    public string Calc(long a, double b)
    {
        return $"long({a}) + double({b}) = {a + b:F2}";
    }

    // ============================================================
    // 4. 布尔类型 —— 支持 true/false, 1/0, yes/no, on/off
    //    用户发送: /toggle true
    //    用户发送: /toggle yes
    //    用户发送: /toggle 1
    // ============================================================
    [Command("toggle")]
    public string Toggle(bool enabled)
    {
        return enabled ? "✅ 已启用" : "❌ 已禁用";
    }

    // ============================================================
    // 5. 枚举类型 —— 大小写不敏感
    //    用户发送: /loglevel information
    //    用户发送: /loglevel error
    // ============================================================
    [Command("loglevel")]
    public string SetLogLevel(LogLevel level)
    {
        return $"当前日志级别: {level}";
    }

    // ============================================================
    // 6. Guid 类型
    //    用户发送: /guid 550e8400-e29b-41d4-a716-446655440000
    // ============================================================
    [Command("guid")]
    public string ShowGuid(Guid id)
    {
        return $"解析后的 GUID: {id:D}";
    }

    // ============================================================
    // 7. 可空类型 (int?, bool?, double?)
    //    用户发送: /timeout          → timeout = null, 使用默认
    //    用户发送: /timeout 30       → timeout = 30
    // ============================================================
    [Command("timeout")]
    public string Timeout(int? seconds = null)
    {
        if (seconds == null)
            return "未设置超时，使用默认值 60 秒";
        return $"超时设置为: {seconds} 秒";
    }

    // ============================================================
    // 8. params string[] —— 接收剩余参数
    //    用户发送: /tags C# .NET QQBot SDK
    // ============================================================
    [Command("tags")]
    public string Tags(params string[] tags)
    {
        if (tags.Length == 0)
            return "请至少输入一个标签！";
        return $"标签: [{string.Join("], [", tags)}] (共 {tags.Length} 个)";
    }

    // ============================================================
    // 9. 带默认值的参数
    //    用户发送: /greet              → 使用默认值
    //    用户发送: /greet 小明 中文     → 自定义
    // ============================================================
    [Command("greet")]
    public string Greet(string name = "世界", string lang = "中文")
    {
        return lang switch
        {
            "English" => $"Hello, {name}!",
            "日本語" => $"こんにちは, {name}！",
            _ => $"你好, {name}！"
        };
    }

    // ============================================================
    // 10. 异步方法
    //     用户发送: /ping
    // ============================================================
    [Command("ping")]
    public async Task<string> Ping()
    {
        var start = DateTimeOffset.UtcNow;
        await Task.Delay(10); // 模拟网络延迟
        var elapsed = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
        return $"🏓 Pong! 延迟: {elapsed:F1}ms";
    }

    // ============================================================
    // 11. 返回图片
    //     用户发送: /image https://example.com/cat.png
    // ============================================================
    [Command("image")]
    public ImageResult ShowImage(string url)
    {
        return new ImageResult(url);
    }

    // 12. 返回固定图片 (隐式转换)
    [Command("logo")]
    public ImageResult Logo()
    {
        // string 可以隐式转换为 ImageResult
        ImageResult result = "https://github.com/luolangaga.png";
        return result;
    }

    // ============================================================
    // 13. 返回 Markdown 消息
    //     用户发送: /md
    // ============================================================
    [Command("md")]
    public MessageMarkdown MarkdownContent()
    {
        return MarkdownBuilder.FromContent(
            "# 📊 机器人状态\n" +
            "## 基本信息\n" +
            $"- **用户**: {User?.Username}\n" +
            $"- **消息ID**: {Message.Id}\n" +
            $"- **时间**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
            "\n## 功能列表\n" +
            "1. ✅ 自动回复\n" +
            "2. ✅ 群管理\n" +
            "3. ✅ 数据统计\n"
        );
    }

    // ============================================================
    // 14. Markdown Builder 模式 (模板 + 参数)
    //     用户发送: /template
    // ============================================================
    [Command("template")]
    public MessageMarkdown TemplateMarkdown()
    {
        return new MarkdownBuilder()
            .UseTemplate("your_template_id")
            .AddParam("title", "系统通知")
            .AddParam("content", User?.Username ?? "用户", "你好")
            .Build();
    }

    // ============================================================
    // 15. 复杂消息 (Markdown + Keyboard)
    //     用户发送: /menu
    // ============================================================
    [Command("menu")]
    public MessageMarkdown MenuWithKeyboard()
    {
        // 注意：目前 Controller 返回 MessageMarkdown 不直接支持附带 Keyboard
        // 如需 Keyboard，请在方法内用 Client.Api 或扩展方法手动发送
        return MarkdownBuilder.FromContent(
            "# 📋 功能菜单\n" +
            "- `/help` - 查看帮助\n" +
            "- `/ping` - 测试延迟\n" +
            "- `/info` - 用户信息\n" +
            "- `/image <url>` - 显示图片"
        );
    }

    // ============================================================
    // 16. 访问上下文属性
    //     用户发送: /info
    //     展示所有可以从控制器访问的上下文
    // ============================================================
    [Command("info")]
    public string Info()
    {
        var lines = new[]
        {
            $"👤 用户: {User?.Username} (ID: {User?.Id})",
            $"💬 消息内容: {Message.Content}",
            $"📺 频道ID: {Message.GuildId ?? "无"}",
            $"📁 子频道ID: {Message.ChannelId ?? "无"}",
            $"👥 群OpenId: {Message.GroupOpenId ?? "无"}",
            $"🔢 消息序号: {Message.Seq}",
            $"📝 原始参数: [{string.Join(", ", RawArguments)}]",
            $"📋 参数个数: {RawArguments.Length}",
            $"🤖 机器人ID: {Client.CurrentUser?.Id ?? "未知"}",
        };
        return "📋 消息上下文信息:\n" + string.Join("\n", lines);
    }

    // ============================================================
    // 17. 在方法中使用 ReplyAsync 发送额外消息
    //     用户发送: /notify 大家好
    // ============================================================
    [Command("notify")]
    public async Task<string> Notify(string message)
    {
        // ReplyAsync 可以在返回前发送额外的消息
        // 注意：ReplyAsync 不阻止方法继续执行
        await ReplyAsync($"准备广播: {message}");
        await Task.Delay(500);

        return $"📢 通知已发送: {message}";
    }

    // ============================================================
    // 18. 引号参数示例
    //     用户发送: /say "hello world, this is a sentence" 小明
    //     CommandParser 自动处理引号，保持空格在参数内
    // ============================================================
    [Command("say")]
    public string Say(string message, string? target = null)
    {
        if (target != null)
            return $"对 {target} 说: {message}";
        return message;
    }

    // ============================================================
    // 19. 多命令别名的复杂用法
    //     用户发送: /help 或 /h 或 /?
    // ============================================================
    [Command("help", "h", "?")]
    public string Help(string? cmd = null)
    {
        if (cmd != null)
            return $"命令 '{cmd}' 的帮助信息...";

        return """
            📖 可用命令列表:
            /hello [name]    打招呼
            /add <a> <b>     计算两数之和
            /calc <a> <b>    浮点数计算
            /ping            测试延迟
            /info            查看消息上下文
            /image <url>     显示图片
            /md              显示 Markdown
            /menu            显示功能菜单
            /toggle <bool>   切换开关
            /loglevel <enum> 设置日志级别
            /guid <guid>     解析 GUID
            /timeout [secs]  设置超时(可空)
            /tags <...tags>  标签列表
            /say <msg> [to]  发送消息(支持引号)
            /help [cmd]      查看帮助
            /api <action>    调用 API 示例
            """;
    }
}
