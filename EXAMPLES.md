# 示例项目快速指南

本文档介绍两个完整的示例项目，帮助你快速上手 Luolan.QQBot。

## 📦 项目列表

| 项目 | 类型 | 说明 |
|------|------|------|
| **Luolan.QQBot.Example** | 控制台应用 | 基础控制台机器人示例 |
| **Luolan.QQBot.AspNetExample** | ASP.NET Core Web API | Web 应用集成示例 |

---

## 🎯 示例 1: 控制台应用

**项目路径**: `Luolan.QQBot.Example/`

### 功能特性

- ✅ 完整的事件处理示例
- ✅ 频道消息、群消息、私聊消息支持
- ✅ 命令系统（/ping, /帮助, /信息等）
- ✅ 日志输出
- ✅ 优雅退出（Ctrl+C）

### 快速开始

#### 1. 配置凭据

**Windows PowerShell:**
```powershell
$env:QQ_BOT_APP_ID="你的AppId"
$env:QQ_BOT_CLIENT_SECRET="你的ClientSecret"
```

**Linux/Mac:**
```bash
export QQ_BOT_APP_ID="你的AppId"
export QQ_BOT_CLIENT_SECRET="你的ClientSecret"
```

或者直接在代码中修改（第24-25行）：
```csharp
// appId = "你的AppId";
// clientSecret = "你的ClientSecret";
```

#### 2. 运行

```bash
cd Luolan.QQBot.Example
dotnet run
```

#### 3. 测试

在频道中 @ 机器人并发送：
- `/帮助` - 显示可用命令
- `/ping` - 测试延迟
- `/信息` - 显示频道信息
- `/echo 测试` - 复读内容

### 示例输出

```
=== Luolan.QQBot 示例程序 ===

正在启动机器人...

✅ 机器人已上线!
   用户名: MyBot
   用户ID: 123456789
   会话ID: abc-def-ghi

机器人已启动! 按 Ctrl+C 退出

[频道消息] 张三: /ping
   响应延迟: 123ms
[成员] 李四 加入了频道 987654321
[群消息] 群666: 你好机器人
```

### 核心代码解析

```csharp
// 创建机器人
var bot = new QQBotClientBuilder()
    .WithAppId(appId)
    .WithClientSecret(clientSecret)
    .WithIntents(Intents.Default | Intents.GroupAtMessages)
    .WithLoggerFactory(loggerFactory)
    .Build();

// 处理@消息
bot.OnAtMessageCreate += async e =>
{
    if (e.Message.Content?.Contains("/ping") == true)
    {
        await bot.ReplyAsync(e.Message, "🏓 Pong!");
    }
};

// 启动
await bot.StartAsync();
```

---

## 🌐 示例 2: ASP.NET Core Web API

**项目路径**: `Luolan.QQBot.AspNetExample/`

### 功能特性

- ✅ 依赖注入集成
- ✅ 托管服务（自动启动/停止）
- ✅ RESTful API 端点
- ✅ Swagger API 文档
- ✅ 健康检查
- ✅ 配置系统集成

### 快速开始

#### 1. 配置凭据

**方式一：环境变量**
```powershell
$env:QQ_BOT_APP_ID="你的AppId"
$env:QQ_BOT_CLIENT_SECRET="你的ClientSecret"
```

**方式二：修改 appsettings.json**
```json
{
  "QQBot": {
    "AppId": "你的AppId",
    "ClientSecret": "你的ClientSecret"
  }
}
```

**方式三：User Secrets（推荐生产环境）**
```bash
cd Luolan.QQBot.AspNetExample
dotnet user-secrets set "QQBot:AppId" "你的AppId"
dotnet user-secrets set "QQBot:ClientSecret" "你的ClientSecret"
```

#### 2. 运行

```bash
cd Luolan.QQBot.AspNetExample
dotnet run
```

或使用热重载：
```bash
dotnet watch run
```

#### 3. 访问

- **Swagger UI**: https://localhost:5001/swagger
- **健康检查**: https://localhost:5001/health
- **机器人状态**: https://localhost:5001/api/bot/status

### API 端点

#### 1. 获取机器人状态
```bash
curl https://localhost:5001/api/bot/status
```

**响应示例:**
```json
{
  "isConnected": true,
  "currentUser": {
    "id": "123456789",
    "username": "MyBot"
  },
  "sessionId": "abc-def-ghi",
  "isAuthenticated": true
}
```

#### 2. 获取频道列表
```bash
curl https://localhost:5001/api/bot/guilds
```

#### 3. 发送消息
```bash
curl -X POST https://localhost:5001/api/bot/send \
  -H "Content-Type: application/json" \
  -d '{
    "channelId": "你的子频道ID",
    "content": "Hello from API!"
  }'
```

#### 4. 获取子频道列表
```bash
curl https://localhost:5001/api/bot/guilds/{guildId}/channels
```

### 核心代码解析

#### 依赖注入配置
```csharp
// 添加 QQ 机器人服务
builder.Services.AddQQBot(options =>
{
    options.AppId = builder.Configuration["QQBot:AppId"]!;
    options.ClientSecret = builder.Configuration["QQBot:ClientSecret"]!;
    options.Intents = Intents.Default | Intents.GroupAtMessages;
});

// 添加托管服务（自动启动）
builder.Services.AddQQBotHostedService();
```

#### 事件处理
```csharp
var bot = app.Services.GetRequiredService<QQBotClient>();

bot.OnAtMessageCreate += async e =>
{
    if (e.Message.Content?.Contains("/ping") == true)
    {
        await bot.ReplyAsync(e.Message, "Pong from ASP.NET Core!");
    }
};
```

#### API 端点定义
```csharp
app.MapGet("/api/bot/status", (QQBotClient bot) =>
{
    return Results.Ok(new
    {
        IsConnected = bot.IsConnected,
        CurrentUser = bot.CurrentUser
    });
});
```

---

## 🆚 两种方式对比

| 特性 | 控制台应用 | ASP.NET Core |
|------|-----------|-------------|
| **适用场景** | 独立运行的机器人 | Web服务集成 |
| **启动方式** | 手动启动 | IHostedService自动启动 |
| **配置方式** | 代码或环境变量 | appsettings.json + 环境变量 |
| **依赖注入** | ❌ 手动创建 | ✅ 完整支持 |
| **HTTP API** | ❌ 无 | ✅ RESTful API |
| **Swagger** | ❌ 无 | ✅ 自动生成文档 |
| **日志系统** | ✅ 控制台日志 | ✅ ASP.NET Core日志 |
| **部署方式** | 后台服务/Docker | Web应用/云平台 |

---

## 🎓 学习路径

### 初学者
1. 从**控制台示例**开始，理解基本概念
2. 学习事件处理和消息回复
3. 了解不同类型的消息（频道/群/私聊）

### 进阶开发者
1. 学习**ASP.NET Core示例**，了解依赖注入
2. 创建自己的 API 端点
3. 集成数据库和业务逻辑
4. 部署到生产环境

---

## 📚 常用代码片段

### 1. 创建客户端（控制台）
```csharp
var bot = new QQBotClientBuilder()
    .WithAppId("AppId")
    .WithClientSecret("Secret")
    .Build();
```

### 2. 注册服务（ASP.NET Core）
```csharp
builder.Services.AddQQBot(options =>
{
    options.AppId = "AppId";
    options.ClientSecret = "Secret";
});
builder.Services.AddQQBotHostedService();
```

### 3. 处理消息
```csharp
bot.OnAtMessageCreate += async e =>
{
    await bot.ReplyAsync(e.Message, "回复内容");
};
```

### 4. 主动发送消息
```csharp
await bot.SendChannelMessageAsync(channelId, "消息内容");
await bot.SendGroupMessageAsync(groupOpenId, "群消息");
await bot.SendC2CMessageAsync(userOpenId, "私聊消息");
```

### 5. 获取数据
```csharp
var guilds = await bot.GetGuildsAsync();
var channels = await bot.GetChannelsAsync(guildId);
var members = await bot.GetMembersAsync(guildId);
```

---

## 🚀 部署建议

### 控制台应用
```bash
# 发布为独立可执行文件
dotnet publish -c Release -r win-x64 --self-contained

# Linux服务
sudo systemctl enable qqbot.service
sudo systemctl start qqbot.service
```

### ASP.NET Core
```bash
# Docker部署
docker build -t qqbot-api .
docker run -d -p 5000:8080 \
  -e QQ_BOT_APP_ID="AppId" \
  -e QQ_BOT_CLIENT_SECRET="Secret" \
  qqbot-api

# 或使用Docker Compose
docker-compose up -d
```

---

## ❓ 常见问题

### Q: 环境变量不生效？
A: 重启终端或IDE，确保环境变量已加载。

### Q: 连接失败？
A: 检查 AppId 和 ClientSecret 是否正确，网络是否畅通。

### Q: 收不到消息？
A: 确认已订阅相应的 Intents，并且机器人有权限。

### Q: ASP.NET Core 示例不启动机器人？
A: 确保添加了 `AddQQBotHostedService()`。

---

## 📞 获取帮助

- 查看主项目 README.md
- 查看官方文档: https://bot.q.qq.com/wiki/
- 提交 Issue: GitHub Issues

---

**祝你开发愉快！** 🎉
