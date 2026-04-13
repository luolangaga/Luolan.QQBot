using Luolan.QQBot.Core.Abstractions;
using Luolan.QQBot.Core.Controllers;
using Luolan.QQBot.Core.Models;

namespace Luolan.QQBot.Example;

public class ExampleController : BotController
{
    // 基础命令: /hello [名字]
    // 别名: /hi
    [Command("hello", Aliases = new[] { "hi" }, Description = "打招呼")]
    public string Hello(string? name = null)
    {
        name ??= Sender?.Username ?? "Unknown";
        return $"Hello, {name}!";
    }

    // 参数自动转换: /add 1 2
    [Command("add", Description = "计算两个数的和")]
    public string Add(int a, int b)
    {
        return $"{a} + {b} = {a + b}";
    }

    // 异步方法: /ping
    [Command("ping", Description = "测试延迟")]
    public async Task<string> Ping()
    {
        var start = DateTimeOffset.Now;
        // 模拟一些工作
        await Task.Delay(10);
        var end = DateTimeOffset.Now;
        return $"Pong! ({(end - start).TotalMilliseconds:F1}ms)";
    }

    // 返回图片: /image
    [Command("image", Description = "发送图片")]
    public ImageResult Image(string url)
    {
        return new ImageResult(url);
    }

    // 混合消息段: /echo [text]
    [Command("echo", Description = "复读机")]
    public SegmentResult Echo(string text)
    {
        // 构造复合消息: 文本 + 表情 + @发送者
        return new SegmentResult(
            MessageSegment.Text("你发送了: "),
            MessageSegment.Text(text),
            MessageSegment.Text(" "),
            MessageSegment.Face("1"),
            MessageSegment.At(Sender?.Id ?? "")
        );
    }
}
