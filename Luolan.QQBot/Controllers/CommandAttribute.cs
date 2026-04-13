using System;

namespace Luolan.QQBot.Controllers;

/// <summary>
/// 标记一个方法为命令处理方法
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 命令别名
    /// </summary>
    public string[] Aliases { get; }

    public CommandAttribute(string name, params string[] aliases)
    {
        Name = name;
        Aliases = aliases;
    }
}
