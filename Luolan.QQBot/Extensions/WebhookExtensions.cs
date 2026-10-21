using Luolan.QQBot.Webhook;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Extensions;

/// <summary>
/// Webhook 接入扩展
/// </summary>
public static class WebhookExtensions
{
    /// <summary>
    /// 注册 Webhook 处理器(用于 HTTP 回调接入方式)
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="clientSecret">机器人 ClientSecret，用于 Ed25519 签名校验</param>
    public static IServiceCollection AddQQBotWebhook(this IServiceCollection services, string clientSecret)
    {
        services.TryAddSingleton(sp => new QQBotWebhook(
            sp.GetRequiredService<QQBotClient>(),
            clientSecret,
            sp.GetService<ILogger<QQBotWebhook>>()));

        return services;
    }
}
