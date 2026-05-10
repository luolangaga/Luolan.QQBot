# 辅助工具

Luolan.QQBot 提供了一系列辅助工具类，位于 `Luolan.QQBot.Helpers` 命名空间。

## KeyboardBuilder

构建互动键盘按钮。

**命名空间**: `Luolan.QQBot.Helpers`

### 方法

| 方法 | 说明 |
|------|------|
| `NewRow()` | 开始新的一行按钮（每行最多 5 个） |
| `AddButton(string id, string label, string? visitedLabel, int style)` | 添加回调按钮 |
| `AddLinkButton(string id, string label, string url, int style)` | 添加链接按钮 |
| `Build()` | 构建 Keyboard 对象 |
| `FromButtons(params Button[] buttons)` | 静态方法，从按钮列表快速创建 |

### 使用示例

```csharp
var keyboard = new KeyboardBuilder()
    .NewRow()
        .AddButton("btn_yes", "确认", style: 1)      // 蓝色
        .AddButton("btn_no", "取消", style: 0)       // 灰色
    .NewRow()
        .AddLinkButton("link_web", "打开网页",
            "https://github.com", style: 1)
    .Build();
```

### 按钮样式

| Style | 效果 |
|-------|------|
| `1` | 蓝色（主要操作） |
| `0` | 灰色（次要操作） |

---

## MarkdownBuilder

构建 Markdown 消息内容。

**命名空间**: `Luolan.QQBot.Helpers`

### 方法

| 方法 | 说明 |
|------|------|
| `UseContent(string content)` | 使用原生 Markdown 内容 |
| `UseTemplate(string templateId)` | 使用模板 ID（字符串） |
| `AddParam(string key, params string[] values)` | 追加模板参数 |
| `SetParam(string key, params string[] values)` | 覆盖模板参数 |
| `Build()` | 构建 MessageMarkdown 对象 |

### 静态工厂方法

| 方法 | 说明 |
|------|------|
| `FromContent(string content)` | 从原生 Markdown 创建 |
| `FromTemplate(string templateId)` | 从模板创建 |

### 使用示例

```csharp
// 原生 Markdown
var md1 = MarkdownBuilder.FromContent("# 标题\n**加粗** *斜体*");

// 模板模式
var md2 = new MarkdownBuilder()
    .UseTemplate("custom_template_id")
    .AddParam("title", "标题内容")
    .AddParam("items", "项目1", "项目2")
    .Build();

// Builder 模式
var md3 = new MarkdownBuilder()
    .UseContent("**通知**")
    .Build();
```

---

## CommandParser

增强的命令解析器，支持引号参数。

**命名空间**: `Luolan.QQBot.Helpers`

```csharp
// 输入: /say "hello world" test
// 输出: ["/say", "hello world", "test"]
string[] args = CommandParser.Parse(input);
```

---

## RateLimiter

Token Bucket 速率限制器。

**命名空间**: `Luolan.QQBot.Helpers`

```csharp
var limiter = new RateLimiter(capacity: 60, refillRate: 1);

// 非阻塞尝试
if (limiter.TryAcquire("key"))
{
    // 立即执行
}

// 阻塞等待
await limiter.AcquireAsync("key", cancellationToken);
```
