# API 总览

Luolan.QQBot SDK 的 API 分为以下几个层次：

## 架构分层

```
┌─────────────────────────────────────────┐
│          QQBotClient（顶层入口）          │
│  快捷方法：Reply, Send, GetGuilds 等     │
├─────────────────────────────────────────┤
│  ┌───────────────┐ ┌─────────────────┐  │
│  │  Controllers  │ │    Events       │  │
│  │  命令路由      │ │   事件分发       │  │
│  └───────────────┘ └─────────────────┘  │
├─────────────────────────────────────────┤
│       QQBotHttpClient（底层 API）        │
│  完整 HTTP API：频道/成员/角色/消息等     │
├─────────────────────────────────────────┤
│       Infrastructure（基础设施）          │
│  TokenManager, WebSocket, RateLimiter   │
└─────────────────────────────────────────┘
```

## 核心类一览

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `QQBotClient` | `Luolan.QQBot` | 机器人客户端主类 |
| `QQBotClientBuilder` | `Luolan.QQBot` | Builder 模式构建器 |
| `QQBotClientOptions` | `Luolan.QQBot` | 配置选项 |
| `QQBotController` | `Luolan.QQBot.Controllers` | 控制器基类 |
| `CommandAttribute` | `Luolan.QQBot.Controllers` | 命令特性标记 |
| `ImageResult` | `Luolan.QQBot.Controllers` | 图片返回结果 |
| `ControllerManager` | `Luolan.QQBot.Controllers` | 控制器管理器 |
| `QQBotHttpClient` | `Luolan.QQBot.Services` | HTTP API 客户端 |
| `TokenManager` | `Luolan.QQBot.Services` | Token 管理器 |
| `QQBotWebSocketClient` | `Luolan.QQBot.Services` | WebSocket 客户端 |
| `QQBotApiException` | `Luolan.QQBot.Services` | API 异常类 |
| `EventDispatcher` | `Luolan.QQBot.Events` | 事件分发器 |
| `KeyboardBuilder` | `Luolan.QQBot.Helpers` | 键盘构建器 |
| `MarkdownBuilder` | `Luolan.QQBot.Helpers` | Markdown 构建器 |
| `CommandParser` | `Luolan.QQBot.Helpers` | 命令解析器 |
| `RateLimiter` | `Luolan.QQBot.Helpers` | 速率限制器 |

## 扩展方法

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `ControllerExtensions` | `Luolan.QQBot.Extensions` | `UseControllers()` |
| `MessageExtensions` | `Luolan.QQBot.Extensions` | `SendMarkdownAsync` 等 |
| `QQBotServiceCollectionExtensions` | `Luolan.QQBot.Extensions` | `AddQQBot()` |
| `QQBotHostedServiceExtensions` | `Luolan.QQBot.Extensions` | `AddQQBotHostedService()` |
| `HttpClientFactoryExtensions` | `Luolan.QQBot.Extensions` | `AddQQBotWithHttpClientFactory()` |

## 使用建议

- **简单命令** → `QQBotController` + `[Command]`
- **事件处理** → `bot.OnXxx` 快捷事件 / `bot.Events.OnXxx`
- **底层 API** → `bot.Api.xxx()` / `QQBotHttpClient`
- **ASP.NET Core** → `AddQQBot()` + `AddQQBotHostedService()`
