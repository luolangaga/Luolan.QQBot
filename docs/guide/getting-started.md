# 快速开始

本文档将带你从零开始创建一个 QQ 机器人。

## 前置条件

1. 安装 [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
2. 在 [QQ 开放平台](https://q.qq.com) 注册机器人，获取 **AppId** 和 **ClientSecret**
3. 沙箱环境建议将你的 QQ 号加入测试白名单

## 第一步：创建项目

```bash
dotnet new console -n MyQQBot
cd MyQQBot
dotnet add package Luolan.QQBot
```

## 第二步：编写代码

打开 `Program.cs`，写入以下内容：

```csharp
using Luolan.QQBot;
using Luolan.QQBot.Events;

// 1. 构建机器人客户端
var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")          // 从开放平台获取
    .WithClientSecret("你的ClientSecret") // 从开放平台获取
    .UseSandbox(true)                // 使用沙箱环境调试
    .Build();

// 2. 注册事件 —— @消息
bot.OnAtMessageCreate += async e =>
{
    var msg = e.Message;
    Console.WriteLine($"[@{msg.Author?.Username}] {msg.Content}");

    // 回复消息
    await bot.ReplyAsync(msg, $"收到你的消息：{msg.Content}");
};

// 3. 注册事件 —— 连接就绪
bot.OnReady += async e =>
{
    Console.WriteLine($"✅ 机器人已上线：{e.User?.Username}");
};

// 4. 启动机器人
await bot.StartAsync();

Console.WriteLine("机器人运行中... 按 Ctrl+C 退出");
await Task.Delay(-1); // 永远等待
```

## 第三步：运行

```bash
dotnet run
```

如果一切正常，你会看到：
```
正在启动QQ机器人...
✅ 机器人已上线：你的机器人名称
机器人运行中... 按 Ctrl+C 退出
```

## 第四步：测试

在 QQ 频道中 @你的机器人并发送消息，机器人会回复相同的内容。

## 环境变量配置（推荐）

不要在代码中直接写 AppId 和 Secret，使用环境变量：

```bash
# Windows PowerShell
$env:QQBOT_APPID = "你的AppId"
$env:QQBOT_SECRET = "你的ClientSecret"

# Windows CMD
set QQBOT_APPID=你的AppId
set QQBOT_SECRET=你的ClientSecret

# Linux / macOS
export QQBOT_APPID=你的AppId
export QQBOT_SECRET=你的ClientSecret
```

然后在代码中读取：

```csharp
string appId = Environment.GetEnvironmentVariable("QQBOT_APPID")
    ?? throw new Exception("请设置 QQBOT_APPID 环境变量");
string clientSecret = Environment.GetEnvironmentVariable("QQBOT_SECRET")
    ?? throw new Exception("请设置 QQBOT_SECRET 环境变量");
```

## 完整代码结构参考

```
MyQQBot/
├── Program.cs              ← 入口文件
├── MyQQBot.csproj          ← 项目配置
│
├── Controllers/            ← 控制器目录
│   ├── HelloController.cs
│   └── AdminController.cs
│
└── appsettings.json        ← ASP.NET Core 配置文件（可选）
```

## 下一步

- [控制器模式](/guide/controller-mode) — 用 MVC 方式组织命令
- [事件系统](/guide/events) — 了解所有可用事件
- [消息发送](/guide/messages) — 发送文本、图片、Markdown 等消息
