using System.Reflection;
using Luolan.QQBot.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Extensions;

public static class ControllerExtensions
{
    /// <summary>
    /// 启用控制器模式(使用客户端关联的依赖注入容器创建控制器)
    /// </summary>
    /// <param name="client">QQBotClient实例</param>
    /// <param name="assembly">包含控制器的程序集，默认为EntryAssembly</param>
    public static QQBotClient UseControllers(this QQBotClient client, Assembly? assembly = null)
    {
        var targetAssembly = assembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        return client.UseControllers(client.Services, targetAssembly);
    }

    /// <summary>
    /// 启用控制器模式并指定依赖注入容器
    /// </summary>
    /// <param name="client">QQBotClient实例</param>
    /// <param name="serviceProvider">依赖注入容器(用于构造控制器并注入服务)</param>
    /// <param name="assembly">包含控制器的程序集，默认为EntryAssembly</param>
    public static QQBotClient UseControllers(
        this QQBotClient client,
        IServiceProvider? serviceProvider,
        Assembly? assembly = null)
    {
        var targetAssembly = assembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

        ControllerManager manager;
        if (serviceProvider != null)
        {
            manager = serviceProvider.GetService<ControllerManager>()
                ?? new ControllerManager(
                    serviceProvider.GetService<ILogger<ControllerManager>>(),
                    serviceProvider);
        }
        else
        {
            manager = new ControllerManager();
        }

        manager.RegisterControllers(targetAssembly);
        manager.Bind(client);

        client.ControllerManager = manager;
        return client;
    }
}
