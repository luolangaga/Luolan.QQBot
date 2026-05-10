# 性能优化

Luolan.QQBot 内置了多项性能优化，开箱即用。

## 内置优化

### 1. 速率限制器（Rate Limiter）

SDK 使用 Token Bucket 算法自动限制 API 请求频率（默认 60次/分钟）：

```csharp
// 所有 API 调用自动限速，无需手动处理
await bot.Api.SendMessageAsync(channelId, request); // 自动限速
await bot.Api.GetGuildAsync(guildId);                // 自动限速
```

原理：
- 令牌桶容量：60（匹配 QQ API 限制）
- 补充速率：1 令牌/秒
- 当桶为空时，请求会等待直到有可用令牌

```csharp
// 创建自定义限速器
var limiter = new RateLimiter(capacity: 30, refillRate: 1);

// 非阻塞尝试
if (limiter.TryAcquire("key"))
{
    // 立即执行
}

// 阻塞等待
await limiter.AcquireAsync("key", cancellationToken);
```

### 2. 共享 JSON 配置

所有 HTTP 请求共享同一个 `JsonSerializerOptions` 实例，避免重复创建：

```csharp
// SDK 内部使用静态共享实例
private static readonly JsonSerializerOptions SharedJsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

### 3. IHttpClientFactory 支持

使用 `AddQQBotWithHttpClientFactory` 可以更好地管理 HttpClient 生命周期：

```csharp
// ✅ 推荐：自动管理连接池
builder.Services.AddQQBotWithHttpClientFactory(options => { ... });

// 手动方式：每个实例单独管理 HttpClient
var bot = new QQBotClientBuilder().Build();
```

`IHttpClientFactory` 的優勢：
- 避免 Socket 耗尽
- 自动管理连接池
- 处理 DNS 变更
- 可配置超时和重试策略

### 4. 增强的命令解析器

`CommandParser` 高效处理引号和转义：

```csharp
// 输入: /say "hello world" test
// 输出: ["/say", "hello world", "test"]
var args = CommandParser.Parse(input);
```

时间复杂度 O(n)，单次遍历完成解析。

### 5. Token 缓存

Token 获取后自动缓存，避免频繁请求认证接口：

```csharp
// SDK 内部缓存 Token，只在即将过期时刷新
.WithTokenRefreshBeforeExpire(120) // 提前 120 秒刷新
```

## 最佳实践

### 1. 合理设置 Intents

只订阅需要的事件：

```csharp
// ❌ 订阅所有事件
.WithIntents(Intents.PrivateAll)

// ✅ 只订阅需要的
.WithIntents(Intents.GroupAtMessages | Intents.PublicGuildMessages)
```

### 2. 避免频繁创建客户端

SDK 中的客户端设计为单例。创建多个实例会增加资源消耗。

```csharp
// ✅ 复用同一个实例
var bot = new QQBotClientBuilder().Build();
await bot.StartAsync();
// 在整个生命周期中使用同一个 bot 实例

// ❌ 多次创建
```

### 3. 批处理操作

当需要操作多个成员时，使用批量 API：

```csharp
// ✅ 批量禁言
await bot.Api.MuteMembersAsync(guildId, userIds, muteSeconds: 60);

// ❌ 逐个禁言
foreach (var userId in userIds)
{
    await bot.Api.MuteMemberAsync(guildId, userId, muteSeconds: 60);
}
```

### 4. 异步非阻塞

始终使用 async/await，避免同步等待：

```csharp
// ✅ 异步
var guilds = await bot.GetGuildsAsync();

// ❌ 同步阻塞
var guilds = bot.GetGuildsAsync().Result; // 可能导致死锁
```

### 5. 日志级别控制

生产环境降低日志级别：

```csharp
// 开发环境
.SetMinimumLevel(LogLevel.Debug)

// 生产环境
.SetMinimumLevel(LogLevel.Warning)
```

## 性能数据

| 指标 | 数值 |
|------|------|
| API 限速 | 60 次 / 分钟（自动） |
| Token 刷新间隔 | 约 2 小时（自动） |
| WebSocket 重连 | 指数退避，默认间隔 3 秒 |
| JSON 序列化 | 共享实例，零额外分配 |
| 命令解析 | O(n) 单次遍历 |

## 下一步

- [最佳实践](/guide/best-practices) — 更多推荐实践
- [API 参考](/api/overview) — 完整 API 文档
