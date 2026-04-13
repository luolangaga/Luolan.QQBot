using Luolan.QQBot.Controllers;
using Luolan.QQBot.Events;
using Luolan.QQBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Extensions;

/// <summary>
/// IHttpClientFactory 扩展方法 - 优化 HttpClient 管理
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// 添加 QQBot 客户端到依赖注入容器 (使用 IHttpClientFactory)
    /// </summary>
    public static IServiceCollection AddQQBotWithHttpClientFactory(
        this IServiceCollection services, 
        Action<QQBotClientOptions> configureOptions)
    {
        // 配置选项
        services.Configure(configureOptions);
        
        // 注册 HttpClient (使用 IHttpClientFactory)
        services.AddHttpClient("QQBot");
        
        // 注册服务
        services.AddSingleton<TokenManager>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QQBotClientOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("QQBot");
            var logger = sp.GetService<ILogger<TokenManager>>();
            return new TokenManager(options, httpClient, logger);
        });
        
        services.AddSingleton<QQBotHttpClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QQBotClientOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("QQBot");
            var tokenManager = sp.GetRequiredService<TokenManager>();
            var logger = sp.GetService<ILogger<QQBotHttpClient>>();
            return new QQBotHttpClient(options, httpClient, tokenManager, logger);
        });
        
        services.AddSingleton<EventDispatcher>(sp =>
        {
            var logger = sp.GetService<ILogger<EventDispatcher>>();
            return new EventDispatcher(logger);
        });
        
        services.AddSingleton<QQBotWebSocketClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QQBotClientOptions>>().Value;
            var apiClient = sp.GetRequiredService<QQBotHttpClient>();
            var tokenManager = sp.GetRequiredService<TokenManager>();
            var eventDispatcher = sp.GetRequiredService<EventDispatcher>();
            var logger = sp.GetService<ILogger<QQBotWebSocketClient>>();
            return new QQBotWebSocketClient(options, apiClient, tokenManager, eventDispatcher, logger);
        });
        
        services.AddSingleton<QQBotClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QQBotClientOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("QQBot");
            var tokenManager = sp.GetRequiredService<TokenManager>();
            var apiClient = sp.GetRequiredService<QQBotHttpClient>();
            var webSocketClient = sp.GetRequiredService<QQBotWebSocketClient>();
            var eventDispatcher = sp.GetRequiredService<EventDispatcher>();
            var logger = sp.GetService<ILogger<QQBotClient>>();
            
            return new QQBotClient(options, httpClient, tokenManager, apiClient, webSocketClient, eventDispatcher, logger);
        });
        
        // 注册控制器管理器 (可选)
        services.AddSingleton<ControllerManager>(sp =>
        {
            var logger = sp.GetService<ILogger<ControllerManager>>();
            return new ControllerManager(logger);
        });
        
        return services;
    }
}
