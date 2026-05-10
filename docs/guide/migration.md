# 从 v1.3 迁移

v1.4.0 引入了 MVC 控制器模式，同时保持了完全的向后兼容性。

## 新增功能

### 1. 控制器模式

v1.4.0 最大的新功能。使用 `[Command]` 特性标记方法：

```csharp
// v1.4.0 新写法
public class MyController : QQBotController
{
    [Command("hello")]
    public string Hello(string name) => $"Hello, {name}!";
}

// 启用
bot.UseControllers();
```

### 2. 增强的命令解析器

`CommandParser` 支持引号参数：

```csharp
// 用户输入: /say "hello world" test
var args = CommandParser.Parse(input);
// args = ["/say", "hello world", "test"]
```

### 3. 丰富的类型转换

v1.4.0 自动支持更多参数类型：

| 类型 | v1.3 | v1.4.0 |
|------|------|--------|
| `string`, `int` | ✅ | ✅ |
| `long`, `double`, `decimal` | ✅ | ✅ |
| `bool` (true/false/1/0/yes/no/on/off) | ❌ | ✅ |
| `enum` (大小写不敏感) | ❌ | ✅ |
| `Guid` | ❌ | ✅ |
| `int?`, `bool?` 可空类型 | ❌ | ✅ |

### 4. 速率限制器

内置 Token Bucket 限速：

```csharp
var limiter = new RateLimiter(capacity: 60, refillRate: 1);
```

### 5. Markdown 模板支持

新版 Markdown 使用 `custom_template_id`（字符串），旧版 `template_id`（int）已标记为 `[Obsolete]`：

```csharp
// ✅ v1.4.0
var md = new MarkdownBuilder()
    .UseTemplate("custom_template_id_string")
    .Build();

// ❌ 已弃用
var md = new MarkdownBuilder()
    .UseTemplate(12345)  // int，已弃用
    .Build();
```

## 升级步骤

### 1. 更新 NuGet 包

```bash
dotnet add package Luolan.QQBot --version 1.4.0
```

### 2. 迁移 Markdown 模板调用

```csharp
// v1.3
await bot.SendMarkdownTemplateAsync(channelId, 12345, params);

// v1.4.0
await bot.SendMarkdownTemplateAsync(channelId, "custom_template_id", params);
```

### 3. 考虑使用控制器模式

事件模式的代码可以逐步迁移到控制器模式，两者可以共存：

```csharp
// 事件模式（保持兼容）
bot.OnAtMessageCreate += async e =>
{
    if (e.Message.Content == "/ping")
    {
        await bot.ReplyAsync(e.Message, "pong!");
    }
};

// 同时使用控制器模式
bot.UseControllers();

// 控制器
public class PingController : QQBotController
{
    [Command("ping")]
    public string Ping() => "pong!";
}
```

## 破坏性变更

v1.4.0 **没有**破坏性变更。所有 v1.3 的代码都可以直接运行。

已有的 `[Obsolete]` 标记仅影响 Markdown 模板的 int 参数，旧代码仍可编译和运行，只是会有编译警告。

## API 变更汇总

| 变更 | 类型 | 说明 |
|------|------|------|
| `QQBotController` 类 | 新增 | 控制器基类 |
| `CommandAttribute` 类 | 新增 | 命令特性 |
| `ImageResult` 类 | 新增 | 图片返回结果 |
| `ControllerManager` 类 | 新增 | 控制器管理器 |
| `UseControllers()` | 新增 | 启用控制器模式 |
| `CommandParser.Parse()` | 增强 | 支持引号和转义 |
| `ConvertValue()` | 增强 | 支持bool/enum/Guid/可空类型 |
| `RateLimiter` 类 | 新增 | 速率限制器 |
| `MarkdownBuilder.UseTemplate(int)` | 弃用 | 改用 string 参数 |
| `MessageMarkdown.TemplateId` | 弃用 | 改用 CustomTemplateId |
