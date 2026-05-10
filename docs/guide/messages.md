# 消息发送

Luolan.QQBot 支持发送多种类型的消息：文本、图片、Markdown、键盘互动等。

## 基础回复

### 快捷回复方法

```csharp
// 频道消息回复（自动检测消息来源）
await bot.ReplyAsync(message, "回复内容");

// 群消息回复
await bot.ReplyGroupAsync(message, "群回复内容");

// C2C 私聊回复
await bot.ReplyC2CAsync(message, "私聊回复内容");
```

### 主动发送

```csharp
// 发送频道消息
await bot.SendChannelMessageAsync("channelId", "主动发送的消息");

// 发送群消息
await bot.SendGroupMessageAsync("groupOpenId", "群消息");

// 发送 C2C 私聊消息
await bot.SendC2CMessageAsync("userOpenId", "私聊消息");

// 发送频道私信
await bot.SendDirectMessageAsync("userId", "sourceGuildId", "私信内容");
```

## 使用完整请求

使用 `SendMessageRequest` 可以发送更复杂的消息：

```csharp
using Luolan.QQBot.Models;

var request = new SendMessageRequest
{
    Content = "消息内容",

    // Embed 嵌入消息
    Embed = new MessageEmbed
    {
        Title = "标题",
        Description = "描述内容",
        Thumbnail = new MessageEmbedThumbnail { Url = "https://example.com/image.jpg" }
    },

    // 引用回复
    MessageReference = new MessageReference
    {
        MessageId = "要引用的消息ID"
    },

    // 图片
    Image = "https://example.com/pic.jpg",

    // 被动回复（标记这是对某条消息的回复）
    MsgId = originalMessage.Id
};

await bot.Api.SendMessageAsync(channelId, request);
```

## Markdown 消息

### 发送 Markdown

```csharp
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Extensions;

// 方式一：扩展方法发送原生内容
await bot.SendMarkdownContentAsync(channelId,
    "# 标题\n" +
    "**粗体文字**\n" +
    "*斜体文字*\n" +
    "- 列表项 1\n" +
    "- 列表项 2");

// 方式二：Builder 构建
var markdown = new MarkdownBuilder()
    .UseContent("# 通知\n内容...")
    .Build();
await bot.SendMarkdownAsync(channelId, markdown);

// 方式三：回复时发送 Markdown
await bot.ReplyMarkdownAsync(message, markdown);

// 方式四：使用模板
await bot.SendMarkdownTemplateAsync(channelId, "template_id",
    new Dictionary<string, string[]> { ["key"] = new[] { "value" } });
```

### MarkdownBuilder 详解

```csharp
// 原生内容模式
var md1 = new MarkdownBuilder()
    .UseContent("# 标题\n内容")
    .Build();

// 模板 + 参数模式
var md2 = new MarkdownBuilder()
    .UseTemplate("your_custom_template_id") // 从 QQ 开放平台申请
    .AddParam("title", "标题内容")          // 添加单个值
    .AddParam("items", "项目1", "项目2")    // 添加多个值
    .SetParam("count", "5")                 // 覆盖已有值
    .Build();

// 静态工厂方法
var md3 = MarkdownBuilder.FromContent("**加粗**");
var md4 = MarkdownBuilder.FromTemplate("template_id");
var md5 = MarkdownBuilder.FromTemplate("template_id", new Dictionary<string, string[]>
{
    ["title"] = new[] { "标题" }
});
```

## 键盘按钮

使用 `KeyboardBuilder` 构建互动按钮：

```csharp
using Luolan.QQBot.Helpers;

var keyboard = new KeyboardBuilder()
    // 第一行
    .NewRow()
        .AddButton("btn_yes", "✅ 确认", style: 1)     // 蓝色按钮
        .AddButton("btn_no", "❌ 取消", style: 0)      // 灰色按钮

    // 第二行
    .NewRow()
        .AddLinkButton("btn_web", "🔗 打开网页",
            "https://github.com/luolangaga/Luolan.QQBot", style: 1)

    // 第三行
    .NewRow()
        .AddButton("btn_info", "查询信息")
    .Build();

// 配合 Markdown 发送
await bot.SendMarkdownAsync(channelId, markdown, keyboard);
```

### 按钮属性说明

```csharp
// AddButton 参数
// id: 按钮ID，用于在 OnInteractionCreate 事件中识别哪个按钮被点击
// label: 按钮显示文字
// visitedLabel: 点击后显示的文字（可选，默认同 label）
// style: 样式（0=灰色, 1=蓝色）

// AddLinkButton 参数
// id: 按钮ID
// label: 按钮显示文字
// url: 点击后跳转的链接
// style: 样式

// 处理按钮点击
bot.OnInteractionCreate += async e =>
{
    var buttonId = e.Interaction.Data?.Resolved?.ButtonId;
    switch (buttonId)
    {
        case "btn_yes":
            // 处理确认...
            break;
        case "btn_no":
            // 处理取消...
            break;
    }
};
```

### 自定义按钮

```csharp
var customButton = new Button
{
    Id = "custom_btn",
    RenderData = new ButtonRenderData
    {
        Label = "自定义",
        VisitedLabel = "已点击",
        Style = 1
    },
    Action = new ButtonAction
    {
        Type = 2,           // 0=跳转链接, 1=回调, 2=指令
        Permission = new ButtonPermission
        {
            Type = 2        // 0=指定用户, 1=仅管理员, 2=所有人, 3=指定角色
        },
        Data = "自定义数据",
        Enter = false       // 是否在输入框回填指令
    }
};

// 快速创建单行键盘
var quick = KeyboardBuilder.FromButtons(customButton, button2, button3);
```

## 群消息和 C2C 消息

群和 C2C 消息支持类似的功能，但 API 略有不同：

```csharp
// 群文本消息
await bot.Api.SendGroupTextMessageAsync("groupOpenId", "内容", msgId: "回复的消息ID");

// 群 Markdown 消息
await bot.Api.SendGroupMessageAsync("groupOpenId", new SendGroupMessageRequest
{
    MsgType = 2,  // 0=文本, 2=Markdown, 3=Ark, 4=Embed, 7=富媒体
    Markdown = MarkdownBuilder.FromContent("# 群通知"),
    MsgId = "回复的消息ID"
});

// 群文件上传
var uploadResp = await bot.Api.UploadGroupMediaAsync("groupOpenId", new UploadMediaRequest
{
    FileType = 1, // 1=图片, 2=视频, 3=语音, 4=文件
    Url = "https://example.com/file.jpg",
    SrvSendMsg = true
});

// C2C 消息（私有聊）
await bot.Api.SendC2CTextMessageAsync("userOpenId", "内容");
await bot.Api.SendC2CMessageAsync("userOpenId", new SendGroupMessageRequest { MsgType = 2, ... });
await bot.Api.UploadC2CMediaAsync("userOpenId", new UploadMediaRequest { ... });
```

## 消息删除

```csharp
// 删除频道消息
await bot.Api.DeleteMessageAsync(channelId, messageId, hideTip: true);

// 删除私信消息
await bot.Api.DeleteDmsAsync(guildId, messageId, hideTip: true);
```

## 表情表态

```csharp
// 对消息添加表情表态
await bot.Api.AddReactionAsync(channelId, messageId, "system", "1"); // system:1 = 点赞

// 删除表情表态
await bot.Api.DeleteReactionAsync(channelId, messageId, "system", "1");
```

## 精华消息

```csharp
// 设为精华
await bot.Api.AddPinsMessageAsync(channelId, messageId);

// 获取精华列表
var pins = await bot.Api.GetPinsMessageAsync(channelId);

// 取消精华
await bot.Api.DeletePinsMessageAsync(channelId, messageId);
```

## 下一步

- [Markdown 与键盘](/guide/markdown-keyboard) — 深入了解 Markdown 和键盘
- [API 参考](/api/overview) — 完整的 API 文档
