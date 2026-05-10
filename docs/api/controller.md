# QQBotController

控制器模式的基类，所有 Bot 命令控制器必须继承此类。

**命名空间**: `Luolan.QQBot.Controllers`

## 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Client` | `QQBotClient` | 当前机器人客户端 |
| `Message` | `Message` | 触发当前命令的消息对象 |
| `User` | `User?` | 发送消息的用户（`Message.Author`） |
| `RawContent` | `string` | 去除命令名后的原始文本 |
| `RawArguments` | `string[]` | 解析后的参数数组 |

## 方法

| 方法 | 说明 |
|------|------|
| `ReplyAsync(string content)` | 发送一条额外回复（不阻止 return） |

## 继承与定义

```csharp
public class MyController : QQBotController
{
    [Command("ping")]
    public string Ping() => "pong!";
}
```

## CommandAttribute

标记命令方法。

**命名空间**: `Luolan.QQBot.Controllers`

**构造函数**: `CommandAttribute(string name, params string[] aliases)`

```csharp
// 单命令
[Command("hello")]
public string Hello() => "Hello!";

// 多别名
[Command("help", "h", "?")]
public string Help() => "帮助信息...";
```

## ImageResult

图片返回结果。字符串可隐式转换为 ImageResult。

**命名空间**: `Luolan.QQBot.Controllers`

```csharp
[Command("image")]
public ImageResult ShowImage(string url)
    => new ImageResult(url);

// 隐式转换
[Command("logo")]
public ImageResult Logo()
{
    ImageResult result = "https://example.com/logo.png";
    return result;
}
```

## 支持的返回类型

| 返回类型 | 说明 |
|----------|------|
| `string` | 文本消息（默认） |
| `ImageResult` | 图片消息 |
| `MessageMarkdown` | Markdown 消息 |
| `Task<string>` | 异步文本 |
| `Task<ImageResult>` | 异步图片 |
| `Task<MessageMarkdown>` | 异步 Markdown |

## 支持的参数类型

| 类型 | 示例输入 |
|------|---------|
| `string` | `hello` |
| `int` | `42` |
| `long` | `9999999999` |
| `double` | `3.14` |
| `decimal` | `99.99` |
| `bool` | `true` / `1` / `yes` / `on` |
| `Guid` | `550e8400-...` |
| `enum` | `Information` |
| `T?` (可空) | 空 或 值 |
| `string[]` (params) | 剩余全部参数 |

## ControllerManager

内部管理命令注册和分发的类。

```csharp
// 通过扩展方法使用
bot.UseControllers();    // 扫描入口程序集
bot.UseControllers(asm); // 扫描指定程序集
```

`UseControllers` 做了：
1. 扫描程序集中所有 `QQBotController` 子类
2. 发现所有 `[Command]` 方法并注册
3. 订阅消息事件（5 种消息事件）
4. 自动解析命令、转换参数、发送回复
