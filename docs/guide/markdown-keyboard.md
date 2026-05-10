# Markdown 与键盘

Markdown 和互动键盘是 QQ 机器人提供丰富交互体验的核心功能。

## Markdown 消息

QQ 机器人支持两种 Markdown 模式：

1. **原生 Markdown** — 直接写 Markdown 语法（需要特殊权限，通常需要内邀）
2. **模板 Markdown** — 在 QQ 开放平台申请模板，填入参数（推荐）

### 原生 Markdown 内容

```csharp
using Luolan.QQBot.Helpers;

// 静态方法
var md = MarkdownBuilder.FromContent(
    "# 系统状态\n" +
    "| 指标 | 数值 |\n" +
    "|------|------|\n" +
    "| CPU  | 45%  |\n" +
    "| 内存 | 2.3GB|\n"
);

// Builder 模式
var md2 = new MarkdownBuilder()
    .UseContent("**粗体** *斜体* `代码` ~~删除线~~")
    .Build();
```

### 模板 Markdown

```csharp
// 不带参数的模板
var md = MarkdownBuilder.FromTemplate("template_id_here");

// 带参数的模板
var md2 = new MarkdownBuilder()
    .UseTemplate("template_id_here")
    .AddParam("title", "通知标题")
    .AddParam("content", "第一行", "第二行")
    .AddParam("time", DateTime.Now.ToString("HH:mm"))
    .Build();

// 使用字典传参
var md3 = MarkdownBuilder.FromTemplate("template_id", new Dictionary<string, string[]>
{
    ["title"] = new[] { "标题" },
    ["items"] = new[] { "项目1", "项目2", "项目3" }
});
```

### AddParam vs SetParam

```csharp
var builder = new MarkdownBuilder()
    .UseTemplate("tpl_id");

// AddParam — 追加值（如果 key 已存在，追加到列表）
builder.AddParam("key", "value1");
builder.AddParam("key", "value2"); // "key" 现在的值是 ["value1", "value2"]

// SetParam — 覆盖值
builder.SetParam("key", "newValue"); // "key" 的值被覆盖为 ["newValue"]
```

## 发送 Markdown

### 频道中发送

```csharp
using Luolan.QQBot.Extensions;

// 发送 Markdown 内容
await bot.SendMarkdownContentAsync(channelId,
    "# 通知\n内容...");

// 发送 Markdown + 键盘
await bot.SendMarkdownContentAsync(channelId, "# 确认\n确定继续吗？",
    keyboard: keyboard,
    msgId: message.Id);  // 被动回复时传入

// 发送模板
await bot.SendMarkdownTemplateAsync(channelId, "template_id");

// 使用 Builder 构建后发送
var md = new MarkdownBuilder()
    .UseContent("# 标题")
    .Build();
await bot.SendMarkdownAsync(channelId, md);

// 回复时发送 Markdown
await bot.ReplyMarkdownAsync(message, md);
await bot.ReplyMarkdownTemplateAsync(message, "template_id");
```

### 群和 C2C 中发送

```csharp
// 群 Markdown
await bot.Api.SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
{
    MsgType = 2,  // 2 = Markdown
    Markdown = MarkdownBuilder.FromContent("# 群通知"),
    MsgId = message.Id
});

// C2C Markdown
await bot.Api.SendC2CMessageAsync(userOpenId, new SendGroupMessageRequest
{
    MsgType = 2,
    Markdown = MarkdownBuilder.FromContent("# 私聊通知")
});
```

## 互动键盘

### 基础键盘

```csharp
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;

var keyboard = new KeyboardBuilder()
    .NewRow()
        .AddButton("btn_a", "按钮 A", style: 1)  // 蓝色
        .AddButton("btn_b", "按钮 B", style: 0)  // 灰色
    .NewRow()
        .AddButton("btn_c", "按钮 C")            // 默认蓝色
    .Build();
```

### 按钮样式

| Style | 效果 |
|-------|------|
| `1` | 蓝色（主要操作） |
| `0` | 灰色（次要操作） |

### 链接按钮

```csharp
var keyboard = new KeyboardBuilder()
    .NewRow()
        .AddLinkButton("link_github", "GitHub 仓库",
            "https://github.com/luolangaga/Luolan.QQBot", style: 1)
    .Build();
```

### 多行键盘

```csharp
var keyboard = new KeyboardBuilder()
    // 第一行：确认 / 取消
    .NewRow()
        .AddButton("confirm", "✅ 确认", style: 1)
        .AddButton("cancel", "❌ 取消", style: 0)

    // 第二行：功能按钮
    .NewRow()
        .AddButton("info", "📋 详情")
        .AddButton("share", "📤 分享")

    // 第三行：链接
    .NewRow()
        .AddLinkButton("web", "🔗 查看更多", "https://example.com", style: 1)
    .Build();
```

### 自定义按钮

```csharp
var customButton = new Button
{
    Id = "special_btn",
    RenderData = new ButtonRenderData
    {
        Label = "特殊按钮",
        VisitedLabel = "已点击",
        Style = 1
    },
    Action = new ButtonAction
    {
        Type = 2,              // 0=跳转, 1=回调, 2=指令
        Permission = new ButtonPermission
        {
            Type = 2           // 0=指定用户, 1=管理员, 2=所有人, 3=指定角色
        },
        Data = "custom_data",
        Enter = true,          // 点击后自动输入 "/custom_data"
        UnsupportTips = "请升级客户端"
    }
};

var keyboard = KeyboardBuilder.FromButtons(customButton);
```

### 完整的按钮权限控制

```csharp
// 仅管理员
var adminPermission = new ButtonPermission { Type = 1 };

// 指定用户
var userPermission = new ButtonPermission
{
    Type = 0,
    SpecifyUserIds = new List<string> { "userId1", "userId2" }
};

// 指定角色
var rolePermission = new ButtonPermission
{
    Type = 3,
    SpecifyRoleIds = new List<string> { "roleId1" }
};
```

## 处理按钮点击

```csharp
bot.OnInteractionCreate += async e =>
{
    var interaction = e.Interaction;
    var buttonId = interaction.Data?.Resolved?.ButtonId;

    switch (buttonId)
    {
        case "confirm":
            // 处理确认...
            Console.WriteLine($"用户 {interaction.UserOpenId} 确认了操作");
            break;

        case "cancel":
            // 处理取消...
            break;

        case "info":
            // 查询详情...
            break;
    }
};
```

## Markdown + Keyboard 组合示例

```csharp
// 投票消息
[Command("vote")]
public async Task<string> Vote()
{
    var markdown = MarkdownBuilder.FromContent(
        "# 🗳️ 今日投票\n" +
        "## 你觉得 C# 怎么样？\n" +
        "请点击下方按钮投票"
    );

    var keyboard = new KeyboardBuilder()
        .NewRow()
            .AddButton("vote_good", "👍 非常好", style: 1)
            .AddButton("vote_ok", "👌 还行", style: 0)
            .AddButton("vote_bad", "👎 不好", style: 0)
        .Build();

    if (!string.IsNullOrEmpty(Message.ChannelId))
    {
        await Client.SendMarkdownAsync(Message.ChannelId, markdown, keyboard);
    }

    return "投票已发起！";
}
```

## 下一步

- [API 参考 - Helpers](/api/helpers) — KeyboardBuilder 和 MarkdownBuilder 的完整参考
- [事件系统](/guide/events) — 处理按钮点击的 OnInteractionCreate 事件
