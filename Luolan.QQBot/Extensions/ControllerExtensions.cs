using System.Reflection;
using Luolan.QQBot.Controllers;

namespace Luolan.QQBot.Extensions;

public static class ControllerExtensions
{
    // keep reference to avoid GC
    private static ControllerManager? _manager;

    /// <summary>
    /// 启用控制器模式
    /// </summary>
    /// <param name="client">QQBotClient实例</param>
    /// <param name="assembly">包含控制器的程序集，默认为EntryAssembly</param>
    /// <returns></returns>
    public static QQBotClient UseControllers(this QQBotClient client, Assembly? assembly = null)
    {
        _manager = new ControllerManager();
        var targetAssembly = assembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        _manager.RegisterControllers(targetAssembly);
        _manager.Bind(client);
        return client;
    }
}
