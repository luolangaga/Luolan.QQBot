# ASP.NET Core QQ Bot 示例

展示如何将 Luolan.QQBot 集成到 ASP.NET Core 应用，包含控制器模式、依赖注入、WebSocket 与 Webhook 两种接入方式。

## 🚀 快速开始

### 1. 配置机器人凭据

推荐使用 User Secrets 或环境变量，不要提交真实密钥：

```bash
dotnet user-secrets set "Bot:AppId" "你的AppId"
dotnet user-secrets set "Bot:ClientSecret" "你的ClientSecret"
```

或修改 `appsettings.json`：

```json
{
  "Bot": {
    "AppId": "你的AppId",
    "ClientSecret": "你的ClientSecret",
    "IsSandbox": false,
    "UseWebSocket": true
  }
}
```

### 2. 运行

```bash
dotnet run
```

Swagger: `https://localhost:7192/swagger`

## 🔌 接入方式

Webhook 是一种传输模式，切换只需改配置，事件订阅和控制器代码不需要修改。

### WebSocket（默认）

```json
{ "Bot": { "Mode": "WebSocket" } }
```

### Webhook

```json
{ "Bot": { "Mode": "Webhook", "WebhookPath": "/api/qqbot/webhook" } }
```

在开放平台管理端把回调地址配置为：

```
https://你的域名/api/qqbot/webhook
```

`Program.cs` 中的 `AddQQBotHostedService()` 和 `app.MapQQBotWebhook()` 在两种模式下都保留：Webhook 模式不会连接 WebSocket，WebSocket 模式下端点映射会自动跳过。

## 📡 API 端点

| 端点 | 方法 | 说明 |
|------|------|------|
| `/health` | GET | 健康检查 |
| `/api/bot/status` | GET | 机器人连接状态 |
| `/api/bot/send` | POST | 发送群/单聊/频道消息 |
| `/api/bot/groups/{groupOpenId}/info` | GET | 群基本信息 |
| `/api/bot/groups/{groupOpenId}/bot-state` | GET | 机器人群内状态 |
| `/api/bot/groups/{groupOpenId}/members` | GET | 群成员列表 |
| `/api/qqbot/webhook` | POST | Webhook 回调入口 |

### 发送消息示例

```bash
curl -X POST https://localhost:7192/api/bot/send \
  -H "Content-Type: application/json" \
  -d '{
    "targetId": "群OpenID或用户OpenID",
    "content": "Hello from API!",
    "targetType": "group"
  }'
```

`targetType` 支持 `group` / `c2c` / `channel`。

## 🎮 机器人命令

在频道或群中 @ 机器人：

- `/ping` - 测试连接
- `/time` - 查看服务器时间
- `/info` - 查看当前消息上下文

控制器示例同时演示了构造函数注入 `ILogger<BasicBotController>`。
