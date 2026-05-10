---
layout: home

hero:
  name: "Luolan.QQBot"
  text: "超级完善的 .NET 官方 QQ 机器人 SDK"
  tagline: 专为 .NET 开发者打造 · MVC 控制器模式 · 自动令牌管理 · 强类型 API
  image:
    src: /logo.svg
    alt: Luolan.QQBot
  actions:
    - theme: brand
      text: 快速开始
      link: /guide/getting-started
    - theme: alt
      text: 查看 API
      link: /api/overview
---

<style>
.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 24px;
  max-width: 1200px;
  margin: 48px auto 0;
  padding: 0 24px;
}
.feature-card {
  background: var(--vp-c-bg-soft);
  border-radius: 12px;
  padding: 24px;
  border: 1px solid transparent;
  transition: border-color 0.25s;
}
.feature-card:hover {
  border-color: var(--vp-c-brand-1);
}
.feature-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: var(--vp-c-brand-soft);
  color: var(--vp-c-brand-1);
  font-size: 24px;
  margin-bottom: 16px;
}
.feature-card h3 {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 8px;
}
.feature-card p {
  font-size: 14px;
  color: var(--vp-c-text-2);
  margin: 0;
  line-height: 1.6;
}
</style>

<div class="features-grid">
  <div class="feature-card">
    <div class="feature-icon"><Icon icon="ph:game-controller-duotone" width="24" /></div>
    <h3>MVC 控制器模式</h3>
    <p>类似 ASP.NET WebAPI 的开发体验。使用 [Command] 特性标记方法，自动路由、参数解析、类型转换。让写机器人指令像写 API Controller 一样自然。</p>
  </div>
  <div class="feature-card">
    <div class="feature-icon"><Icon icon="ph:lightning-duotone" width="24" /></div>
    <h3>极致简单</h3>
    <p>Builder 模式一行链式调用即可创建客户端。Token 自动刷新、WebSocket 自动重连，开发者只需关注业务逻辑。</p>
  </div>
  <div class="feature-card">
    <div class="feature-icon"><Icon icon="ph:flask-duotone" width="24" /></div>
    <h3>深度 DI 集成</h3>
    <p>完美支持 ASP.NET Core 依赖注入和 IHostedService。提供 AddQQBot() 一行注册，自动管理 Bot 生命周期。</p>
  </div>
  <div class="feature-card">
    <div class="feature-icon"><Icon icon="ph:shield-check-duotone" width="24" /></div>
    <h3>强类型支持</h3>
    <p>完整的 API 模型类型定义，享受 IDE 智能提示。所有事件参数和 API 返回值都是强类型，拒绝弱类型 JSON 操作。</p>
  </div>
  <div class="feature-card">
    <div class="feature-icon"><Icon icon="ph:broadcast-duotone" width="24" /></div>
    <h3>丰富事件系统</h3>
    <p>覆盖频道、群聊、私聊、互动等 40+ 种事件类型。支持事件委托和原始事件双重访问方式。</p>
  </div>
  <div class="feature-card">
    <div class="feature-icon"><Icon icon="ph:rocket-launch-duotone" width="24" /></div>
    <h3>性能优化内置</h3>
    <p>内置速率限制器、共享 JSON 配置、增强的命令解析器。支持 bool/enum/Guid/可空类型自动转换。</p>
  </div>
</div>

# Luolan.QQBot

一个简洁、高效、深度集成 .NET 依赖注入体系的 **QQ 官方机器人 SDK**。

## 安装

通过 NuGet 安装核心包：

::: code-group
```bash [.NET CLI]
dotnet add package Luolan.QQBot
```
```bash [Package Manager]
Install-Package Luolan.QQBot
```
```bash [Package Reference]
<PackageReference Include="Luolan.QQBot" Version="1.4.0" />
```
:::

## 快速预览

::: code-group
```csharp [控制器模式（推荐）]
using Luolan.QQBot.Controllers;
using Luolan.QQBot.Extensions;

// 构建客户端
var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")
    .WithClientSecret("你的ClientSecret")
    .UseSandbox(true)
    .Build();

// 启用控制器
bot.UseControllers();
await bot.StartAsync();

// 控制器定义
public class HelloController : QQBotController
{
    [Command("hello")]
    public string Hello(string name)
        => $"Hello, {name}!";
}
```

```csharp [事件模式]
using Luolan.QQBot;

var bot = new QQBotClientBuilder()
    .WithAppId("你的AppId")
    .WithClientSecret("你的ClientSecret")
    .Build();

bot.OnAtMessageCreate += async e =>
    await bot.ReplyAsync(e.Message, "收到消息！");

await bot.StartAsync();
await Task.Delay(-1);
```
:::

## 适用场景

- <Icon icon="ph:game-controller" /> **游戏社群** - 自动管理频道成员、发送公告、互动问答
- <Icon icon="ph:megaphone" /> **通知服务** - 系统监控告警、构建状态通知、定时提醒
- <Icon icon="ph:robot" /> **客服机器人** - 自动回复、常见问题解答、工单转接
- <Icon icon="ph:target" /> **运营工具** - 用户数据统计、活动管理、签到抽奖

## 系统要求

- .NET 8.0+
- 需要 [QQ 开放平台](https://q.qq.com) 注册机器人并获取 AppId 和 ClientSecret

## 许可证

本项目基于 MIT 协议开源。在 [GitHub](https://github.com/luolangaga/Luolan.QQBot) 上查看源码。
