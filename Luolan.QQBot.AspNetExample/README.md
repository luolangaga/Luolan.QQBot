# ASP.NET Core QQ Bot 示例

这是一个完整的 ASP.NET Core Web API 项目，展示如何将 QQ 机器人集成到 ASP.NET Core 应用中。

## 🚀 快速开始

### 1. 配置机器人凭据

**方式一：使用环境变量（推荐）**

```bash
# Windows PowerShell
$env:QQ_BOT_APP_ID="你的AppId"
$env:QQ_BOT_CLIENT_SECRET="你的ClientSecret"

# Linux/Mac
export QQ_BOT_APP_ID="你的AppId"
export QQ_BOT_CLIENT_SECRET="你的ClientSecret"
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

### 2. 运行项目

```bash
cd Luolan.QQBot.AspNetExample
dotnet run
```

或者使用热重载：

```bash
dotnet watch run
```

### 3. 访问 API 文档

打开浏览器访问：`https://localhost:5001/swagger`

## 📡 API 端点

| 端点 | 方法 | 说明 |
|------|------|------|
| `/health` | GET | 健康检查 |
| `/api/bot/status` | GET | 获取机器人状态 |
| `/api/bot/guilds` | GET | 获取机器人加入的频道列表 |
| `/api/bot/guilds/{guildId}/channels` | GET | 获取指定频道的子频道列表 |
| `/api/bot/send` | POST | 发送消息到指定频道 |

## 📝 示例请求

### 检查机器人状态

```bash
curl https://localhost:5001/api/bot/status
```

### 发送消息

```bash
curl -X POST https://localhost:5001/api/bot/send \
  -H "Content-Type: application/json" \
  -d '{
    "channelId": "你的子频道ID",
    "content": "Hello from API!"
  }'
```

### 获取频道列表

```bash
curl https://localhost:5001/api/bot/guilds
```

## 🎯 核心功能

### 1. 依赖注入集成

```csharp
// 添加 QQ 机器人服务
builder.Services.AddQQBot(options =>
{
    options.AppId = "你的AppId";
    options.ClientSecret = "你的ClientSecret";
    options.Intents = Intents.Default | Intents.GroupAtMessages;
});

// 添加托管服务（自动启动）
builder.Services.AddQQBotHostedService();
```

### 2. 事件处理

```csharp
var bot = app.Services.GetRequiredService<QQBotClient>();

// 处理@消息
bot.OnAtMessageCreate += async e =>
{
    await bot.ReplyAsync(e.Message, "Hello from ASP.NET Core!");
};

// 处理群消息
bot.OnGroupAtMessageCreate += async e =>
{
    await bot.ReplyGroupAsync(e.Message, "收到群消息!");
};
```

### 3. HTTP API 调用

```csharp
// 获取频道列表
app.MapGet("/api/bot/guilds", async (QQBotClient bot) =>
{
    var guilds = await bot.Api.GetCurrentUserGuildsAsync();
    return Results.Ok(guilds);
});
```

## 🔧 配置说明

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Luolan.QQBot": "Debug"  // QQ机器人日志级别
    }
  },
  "QQBot": {
    "AppId": "YOUR_APP_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
  }
}
```

### 环境变量

支持的环境变量：
- `QQ_BOT_APP_ID` - 机器人 AppId
- `QQ_BOT_CLIENT_SECRET` - 机器人 ClientSecret

## 🌟 功能特性

- ✅ **自动启动** - 使用 IHostedService 在应用启动时自动启动机器人
- ✅ **自动停止** - 应用关闭时自动停止机器人
- ✅ **依赖注入** - 所有组件都可以通过 DI 获取
- ✅ **日志集成** - 使用 ASP.NET Core 日志系统
- ✅ **配置系统** - 支持 appsettings.json 和环境变量
- ✅ **Swagger 文档** - 自动生成 API 文档
- ✅ **健康检查** - 内置健康检查端点

## 🎮 机器人命令

在频道中 @ 机器人并发送以下命令：

- `/ping` - 测试连接
- `/time` - 查看服务器时间
- `/status` - 查看机器人状态
- `/help` - 显示帮助

## 🔒 生产环境建议

1. **使用 User Secrets** 存储敏感信息：
   ```bash
   dotnet user-secrets set "QQBot:AppId" "你的AppId"
   dotnet user-secrets set "QQBot:ClientSecret" "你的ClientSecret"
   ```

2. **配置 HTTPS**：
   ```json
   {
     "Kestrel": {
       "Endpoints": {
         "Https": {
           "Url": "https://localhost:5001"
         }
       }
     }
   }
   ```

3. **配置 CORS**（如果需要前端调用）：
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(policy =>
       {
           policy.WithOrigins("https://your-frontend.com")
                 .AllowAnyHeader()
                 .AllowAnyMethod();
       });
   });
   ```

## 📚 更多示例

查看主项目的 README.md 获取更多使用示例和文档。
