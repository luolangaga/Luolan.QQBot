using Luolan.QQBot;
using Luolan.QQBot.Extensions;
using Luolan.QQBot.Webhook;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var botSection = builder.Configuration.GetSection("Bot");
var appId = botSection["AppId"] ?? botSection["Official:AppId"];
var clientSecret = botSection["ClientSecret"] ?? botSection["Official:ClientSecret"];
var useWebSocket = botSection.GetValue("UseWebSocket", true);

if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(clientSecret))
{
    throw new InvalidOperationException("请在 appsettings.json 或环境变量中配置 Bot:AppId 与 Bot:ClientSecret");
}

builder.Services.AddQQBotWithHttpClientFactory(options =>
{
    options.AppId = appId;
    options.ClientSecret = clientSecret;
    options.IsSandbox = botSection.GetValue("IsSandbox", false);
    options.Intents = Intents.Default
        | Intents.GroupAtMessages
        | Intents.GroupMembers
        | Intents.GroupAndC2C;
});

builder.Services.AddQQBotWebhook(clientSecret);

if (useWebSocket)
{
    builder.Services.AddQQBotHostedService();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用控制器模式：控制器通过依赖注入构造，可直接注入 ILogger、HttpClient 等服务
var bot = app.Services.GetRequiredService<QQBotClient>();
bot.UseControllers();

// Webhook 回调入口(管理端配置的回调地址指向 /api/qqbot/webhook)
app.MapPost("/api/qqbot/webhook", async (
    HttpContext context,
    [FromServices] QQBotWebhook webhook,
    CancellationToken cancellationToken) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync(cancellationToken);

    var signature = context.Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
    var timestamp = context.Request.Headers["X-Signature-Timestamp"].FirstOrDefault();

    try
    {
        var response = await webhook.HandleAsync(body, signature, timestamp, cancellationToken);
        return Results.Content(response, "application/json");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (System.Text.Json.JsonException)
    {
        return Results.BadRequest();
    }
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.Now }));

app.MapControllers();

app.Run();
