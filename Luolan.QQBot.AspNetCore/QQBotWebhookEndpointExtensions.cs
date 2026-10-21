using Luolan.QQBot.Webhook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Luolan.QQBot.AspNetCore;

/// <summary>
/// ASP.NET Core Webhook 端点扩展
/// </summary>
public static class QQBotWebhookEndpointExtensions
{
    /// <summary>
    /// 映射 QQ 机器人 Webhook 回调端点。
    /// 该方法在 WebSocket 模式下会直接跳过，因此可以无条件保留在 Program.cs 中，
    /// 只需切换 <see cref="QQBotClientOptions.Mode"/> 即可在两种传输模式之间切换。
    /// </summary>
    public static IEndpointRouteBuilder MapQQBotWebhook(this IEndpointRouteBuilder endpoints)
        => endpoints.MapQQBotWebhook(null);

    /// <summary>
    /// 映射 QQ 机器人 Webhook 回调端点(指定路径)
    /// </summary>
    /// <param name="endpoints">端点路由</param>
    /// <param name="path">回调路径；为空时使用 <see cref="QQBotClientOptions.WebhookPath"/></param>
    public static IEndpointRouteBuilder MapQQBotWebhook(this IEndpointRouteBuilder endpoints, string? path)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<QQBotClientOptions>>().Value;

        // WebSocket 模式下不注册端点，但调用代码可以保持不变
        if (options.Mode != QQBotConnectionMode.Webhook)
            return endpoints;

        var route = string.IsNullOrWhiteSpace(path) ? options.WebhookPath : path;
        if (string.IsNullOrWhiteSpace(route))
            route = "/api/qqbot/webhook";
        if (!route.StartsWith('/'))
            route = "/" + route;

        endpoints.MapPost(route, async (HttpContext context) =>
        {
            var webhook = context.RequestServices.GetRequiredService<QQBotWebhook>();
            var cancellationToken = context.RequestAborted;

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

        return endpoints;
    }
}
