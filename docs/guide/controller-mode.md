# 控制器模式

控制器模式是 Luolan.QQBot 的核心特色功能。它让你像写 ASP.NET WebAPI 一样编写机器人指令。

## 基本概念

- **控制器** — 继承 `QQBotController` 的类
- **命令** — 控制器中标记 `[Command]` 特性的方法
- **自动路由** — 用户消息自动匹配到对应的命令方法
- **参数解析** — 自动将字符串参数转换为方法需要的类型

## 启用控制器

```csharp
using Luolan.QQBot;
using Luolan.QQBot.Extensions;

var bot = new QQBotClientBuilder()
    .WithAppId("...")
    .WithClientSecret("...")
    .Build();

// 自动扫描当前程序集中的所有控制器并注册
bot.UseControllers();

await bot.StartAsync();
```

`UseControllers()` 做了两件事：
1. 扫描程序集中所有 `QQBotController` 的子类
2. 注册所有 `[Command]` 方法并绑定消息事件

## 编写第一个控制器

```csharp
using Luolan.QQBot.Controllers;

public class HelloController : QQBotController
{
    // 用户发送: /hello
    [Command("hello")]
    public string Hello()
    {
        return "你好，世界！";
    }
}
```

就这么简单！用户发送 `/hello`，机器人回复 `你好，世界！`。

## 命令定义

### 命令别名

一个方法可以有多个命令名和别名：

```csharp
// 三种写法都可以触发同一个方法
// 用户发送: /help  或  /h  或  /?
[Command("help", "h", "?")]
public string Help()
{
    return "这是帮助信息...";
}
```

### 带参数的命令

```csharp
// 用户发送: /add 10 20
[Command("add")]
public string Add(int a, int b)
{
    return $"{a} + {b} = {a + b}";
}
```

SDK 自动将字符串 `"10"` `"20"` 转换为 `int` 类型。

### 可选参数

```csharp
// 用户发送: /greet           → 使用默认值 "世界"
// 用户发送: /greet 小明      → 使用 "小明"
[Command("greet")]
public string Greet(string name = "世界")
{
    return $"你好，{name}！";
}
```

## 支持的类型

SDK 内置了丰富的类型转换，以下类型都能自动解析：

| 类型 | 示例输入 | 说明 |
|------|---------|------|
| `string` | `hello` | 字符串（默认） |
| `int` | `42` | 整数 |
| `long` | `9999999999` | 长整数 |
| `double` | `3.14` | 双精度浮点 |
| `decimal` | `99.99` | 精确小数 |
| `bool` | `true` / `false` / `1` / `0` / `yes` / `no` / `on` / `off` | 布尔值 |
| `Guid` | `550e8400-e29b-41d4-a716-446655440000` | GUID |
| `enum` | `Information` / `information` | 枚举（大小写不敏感） |
| `int?` | `空` / `42` | 可空类型 |

### 示例

```csharp
[Command("toggle")]
public string Toggle(bool enabled)
{
    // 用户发送: /toggle true  或 /toggle yes  或 /toggle 1
    return enabled ? "✅ 已启用" : "❌ 已禁用";
}

[Command("loglevel")]
public string SetLevel(LogLevel level)
{
    // 用户发送: /loglevel Information  或 /loglevel error
    return $"日志级别: {level}";
}

[Command("timeout")]
public string SetTimeout(int? seconds = null)
{
    // 用户发送: /timeout     → null
    // 用户发送: /timeout 30  → 30
    return seconds == null ? "未设置" : $"超时：{seconds}秒";
}
```

## 返回值类型

控制器方法可以返回不同类型，SDK 自动处理回复：

### 返回字符串

最常见的用法，直接回复文本消息。

```csharp
[Command("ping")]
public string Ping() => "pong!";
```

### 返回图片

```csharp
[Command("cat")]
public ImageResult Cat()
{
    return new ImageResult("https://example.com/cat.jpg");
}

// 或者利用隐式转换
[Command("logo")]
public ImageResult Logo()
{
    ImageResult result = "https://example.com/logo.png";
    return result;
}
```

### 返回 Markdown

```csharp
using Luolan.QQBot.Models;
using Luolan.QQBot.Helpers;

// 方式一：原生内容
[Command("status")]
public MessageMarkdown Status()
{
    return MarkdownBuilder.FromContent(
        "# 系统状态\n" +
        "- CPU: 45%\n" +
        "- 内存: 2.3GB"
    );
}

// 方式二：模板（需要先在QQ开放平台申请模板）
[Command("notice")]
public MessageMarkdown Notice()
{
    return new MarkdownBuilder()
        .UseTemplate("your_custom_template_id")
        .AddParam("title", "通知标题")
        .Build();
}
```

### 返回异步结果

```csharp
[Command("fetch")]
public async Task<string> FetchData()
{
    await Task.Delay(1000); // 模拟网络请求
    return "数据获取完成！";
}
```

## 上下文访问

在任何控制器方法中，可以直接访问以下属性：

```csharp
public class InfoController : QQBotController
{
    [Command("info")]
    public string Info()
    {
        // Client — 当前机器人客户端实例
        var botId = Client.CurrentUser?.Id;

        // Message — 当前触发命令的消息对象
        var msgContent = Message.Content;
        var channelId = Message.ChannelId;
        var guildId = Message.GuildId;
        var groupOpenId = Message.GroupOpenId;

        // User — 发送消息的用户（等同于 Message.Author）
        var userName = User?.Username;
        var userId = User?.Id;

        // RawContent — 去除命令名后的原始文本
        // RawArguments — 解析后的参数数组
        var args = RawArguments;

        return $"用户: {userName}\n命令: {RawContent}\n参数: [{string.Join(", ", args)}]";
    }

    // 在方法内发送额外消息
    [Command("notify")]
    public async Task<string> Notify(string message)
    {
        await ReplyAsync($"正在广播: {message}");
        return "广播完成！";
    }
}
```

## 引号参数

SDK 的 `CommandParser` 支持引号包裹的参数，用于处理包含空格的参数：

```
用户发送: /say "hello world, how are you?" 小明
       → message = "hello world, how are you?"
       → target = "小明"
```

```csharp
[Command("say")]
public string Say(string message, string? target = null)
{
    return target != null
        ? $"对 {target} 说: {message}"
        : message;
}
```

## params 参数

如果需要接收不确定数量的参数，使用 `params string[]`：

```csharp
[Command("tags")]
public string Tags(params string[] tags)
{
    return $"标签: [{string.Join("], [", tags)}]";
}
// 用户发送: /tags C# .NET QQBot SDK
// 输出: 标签: [C#], [.NET], [QQBot], [SDK]
```

## 命令注册细节

- 命令是**不区分大小写**的（`/HELLO` 和 `/hello` 相同）
- 如果用户 @机器人后紧接命令（如 `@Bot /hello`），会自动跳过 @ 部分
- 命令名和参数之间用空格分隔
- 一个类可以有多个 `[Command]` 方法
- 一个程序集可以有多个控制器类

## 完整示例

```csharp
using Luolan.QQBot.Controllers;
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;

public class MyRobotController : QQBotController
{
    // 基础文本
    [Command("hello")]
    public string Hello(string? name = null)
        => $"你好，{name ?? User?.Username ?? "世界"}！";

    // 整数计算
    [Command("add")]
    public string Add(int a, int b)
        => $"{a} + {b} = {a + b}";

    // 布尔开关
    [Command("switch")]
    public string Switch(bool on)
        => on ? "开了" : "关了";

    // 枚举
    [Command("level")]
    public string Level(LogLevel level)
        => $"级别: {level}";

    // 可空参数
    [Command("delay")]
    public string Delay(int? ms = null)
        => $"延迟: {ms ?? 1000}ms";

    // 图片
    [Command("img")]
    public ImageResult Image(string url)
        => new ImageResult(url);

    // Markdown
    [Command("md")]
    public MessageMarkdown Markdown()
        => MarkdownBuilder.FromContent("**加粗** *斜体*");

    // 异步
    [Command("async")]
    public async Task<string> DoAsync()
    {
        await Task.Delay(500);
        return "异步完成！";
    }

    //上下文
    [Command("whoami")]
    public string WhoAmI()
        => $"你是 {User?.Username}, 来自 {Message.GuildId}";
}
```

## 下一步

- [事件系统](/guide/events) — 处理非命令类的 QQ 事件
- [消息发送](/guide/messages) — 深入了解消息发送功能
