using Luolan.QQBot;
using Luolan.QQBot.AspNetCore;
using Luolan.QQBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var botSection = builder.Configuration.GetSection("Bot");
var appId = botSection["AppId"];
var clientSecret = botSection["ClientSecret"];

if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(clientSecret))
{
    throw new InvalidOperationException("请在 appsettings.json 或环境变量中配置 Bot:AppId 与 Bot:ClientSecret");
}

// 切换 WebSocket / Webhook 只需要改这里(appsettings 的 Bot:Mode)
var mode = Enum.TryParse<QQBotConnectionMode>(botSection["Mode"], ignoreCase: true, out var parsedMode)
    ? parsedMode
    : QQBotConnectionMode.WebSocket;

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = appId;
    options.ClientSecret = clientSecret;
    options.IsSandbox = botSection.GetValue("IsSandbox", false);
    options.Mode = mode;
    options.WebhookPath = botSection["WebhookPath"] ?? "/api/qqbot/webhook";
    options.Intents = Intents.Default
        | Intents.GroupAndC2C
        | Intents.GroupMembers;
});

// 两种模式都注册托管服务：
// WebSocket 模式用于建立长连接；Webhook 模式只做就绪初始化，不会连接 WebSocket。
builder.Services.AddQQBotHostedService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用控制器模式：控制器通过依赖注入构造，可直接注入 ILogger、HttpClient 等服务
var bot = app.Services.GetRequiredService<QQBotClient>();
bot.UseControllers();

// 两种模式都可以无条件保留这行：
// Webhook 模式注册回调端点，WebSocket 模式直接跳过。
// 平台管理端回调地址填写 https://你的域名/api/qqbot/webhook
app.MapQQBotWebhook();

app.MapGet("/health", () => Results.Ok(new { status = "ok", mode = mode.ToString(), time = DateTimeOffset.Now }));

app.MapControllers();

app.Run();
