using Luolan.QQBot.Events;
using Luolan.QQBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luolan.QQBot.Extensions;

/// <summary>
/// QQ机器人服务扩展
/// </summary>
public static class QQBotServiceCollectionExtensions
{
    /// <summary>
    /// 添加QQ机器人服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQQBot(
        this IServiceCollection services,
        Action<QQBotClientOptions> configure)
    {
        // 配置选项
        services.Configure(configure);

        // 注册HttpClient
        services.AddHttpClient<QQBotHttpClient>();

        // 注册服务
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QQBotClientOptions>>().Value;
            return options;
        });

        services.TryAddSingleton<EventDispatcher>();

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<QQBotClientOptions>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(QQBotHttpClient));
            var logger = sp.GetService<ILogger<TokenManager>>();
            return new TokenManager(options, httpClient, logger);
        });

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<QQBotClientOptions>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(QQBotHttpClient));
            var tokenManager = sp.GetRequiredService<TokenManager>();
            var logger = sp.GetService<ILogger<QQBotHttpClient>>();
            return new QQBotHttpClient(options, httpClient, tokenManager, logger);
        });

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<QQBotClientOptions>();
            var httpClient = sp.GetRequiredService<QQBotHttpClient>();
            var tokenManager = sp.GetRequiredService<TokenManager>();
            var eventDispatcher = sp.GetRequiredService<EventDispatcher>();
            var logger = sp.GetService<ILogger<QQBotWebSocketClient>>();
            return new QQBotWebSocketClient(options, httpClient, tokenManager, eventDispatcher, logger);
        });

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<QQBotClientOptions>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(QQBotClient));
            var tokenManager = sp.GetRequiredService<TokenManager>();
            var apiClient = sp.GetRequiredService<QQBotHttpClient>();
            var webSocketClient = sp.GetRequiredService<QQBotWebSocketClient>();
            var eventDispatcher = sp.GetRequiredService<EventDispatcher>();
            var logger = sp.GetService<ILogger<QQBotClient>>();

            return new QQBotClient(
                options,
                httpClient,
                tokenManager,
                apiClient,
                webSocketClient,
                eventDispatcher,
                logger
            );
        });

        return services;
    }

    /// <summary>
    /// 添加QQ机器人服务(使用配置节)
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="appId">AppId</param>
    /// <param name="clientSecret">ClientSecret</param>
    /// <param name="isSandbox">是否为沙箱环境</param>
    /// <param name="intents">订阅的事件类型</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQQBot(
        this IServiceCollection services,
        string appId,
        string clientSecret,
        bool isSandbox = false,
        Intents intents = Intents.Default)
    {
        return services.AddQQBot(options =>
        {
            options.AppId = appId;
            options.ClientSecret = clientSecret;
            options.IsSandbox = isSandbox;
            options.Intents = intents;
        });
    }
}

/// <summary>
/// QQ机器人托管服务(用于ASP.NET Core后台运行)
/// </summary>
public class QQBotHostedService : Microsoft.Extensions.Hosting.IHostedService
{
    private readonly QQBotClient _client;
    private readonly ILogger<QQBotHostedService>? _logger;

    public QQBotHostedService(QQBotClient client, ILogger<QQBotHostedService>? logger = null)
    {
        _client = client;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("QQ机器人托管服务正在启动...");
        await _client.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("QQ机器人托管服务正在停止...");
        await _client.StopAsync();
    }
}

/// <summary>
/// 托管服务扩展
/// </summary>
public static class QQBotHostedServiceExtensions
{
    /// <summary>
    /// 添加QQ机器人托管服务
    /// </summary>
    public static IServiceCollection AddQQBotHostedService(this IServiceCollection services)
    {
        services.AddHostedService<QQBotHostedService>();
        return services;
    }
}
