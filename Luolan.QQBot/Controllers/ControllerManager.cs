using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Luolan.QQBot.Events;
using Luolan.QQBot.Helpers;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Controllers;

/// <summary>
/// 控制器管理器
/// </summary>
public class ControllerManager
{
    private readonly Dictionary<string, (Type ControllerType, MethodInfo Method)> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ControllerManager>? _logger;
    private QQBotClient? _client;

    public ControllerManager(ILogger<ControllerManager>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 注册程序集中的所有控制器
    /// </summary>
    public void RegisterControllers(Assembly assembly)
    {
        var controllerTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(QQBotController)) && !t.IsAbstract);

        foreach (var type in controllerTypes)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes<CommandAttribute>();
                foreach (var attr in attrs)
                {
                    _commands[attr.Name] = (type, method);
                    foreach (var alias in attr.Aliases)
                    {
                        _commands[alias] = (type, method);
                    }
                    _logger?.LogInformation("Registered command: {Command} -> {Controller}.{Method}", attr.Name, type.Name, method.Name);
                }
            }
        }
    }

    /// <summary>
    /// 绑定到QQBotClient
    /// </summary>
    public void Bind(QQBotClient client)
    {
        _client = client;
        
        // 订阅各类消息事件
        client.OnMessageCreate += HandleMessageAsync;
        client.OnAtMessageCreate += HandleMessageAsync;
        client.OnGroupAtMessageCreate += HandleMessageAsync;
        client.OnC2CMessageCreate += HandleMessageAsync;
        client.OnDirectMessageCreate += HandleMessageAsync;
    }

    private async Task HandleMessageAsync(QQBotEventBase evt)
    {
        if (_client == null) return;

        Message? message = null;
        
        // 提取消息内容
        if (evt is MessageCreateEvent mce) message = mce.Message;
        else if (evt is AtMessageCreateEvent amce) message = amce.Message;
        else if (evt is GroupAtMessageCreateEvent gamce) message = gamce.Message;
        else if (evt is C2CMessageCreateEvent cmce) message = cmce.Message;
        else if (evt is DirectMessageCreateEvent dmce) message = dmce.Message;

        if (message == null || string.IsNullOrWhiteSpace(message.Content)) return;

        // 处理内容: 去除可能的@部分
        var content = message.Content.Trim();
        
        // 使用增强的命令解析器 (支持引号)
        var parts = CommandParser.Parse(content);
        if (parts.Length == 0) return;

        // 寻找命令
        // 有时候第一个部分是@，第二个部分才是命令
        string commandName = parts[0];
        string[] args = parts.Skip(1).ToArray();

        // 简单的尝试处理 @Tag
        if (commandName.StartsWith("<@") && parts.Length > 1)
        {
            commandName = parts[1];
            args = parts.Skip(2).ToArray();
        }

        if (_commands.TryGetValue(commandName, out var handler))
        {
            try
            {
                await ExecuteCommandAsync(handler.ControllerType, handler.Method, message, args);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing command {Command}", commandName);
                // 可选：回复错误信息
                // await SendReplyAsync(message, $"执行命令出错: {ex.Message}");
            }
        }
    }

    private async Task ExecuteCommandAsync(Type controllerType, MethodInfo method, Message message, string[] args)
    {
        // 创建控制器实例
        var controller = Activator.CreateInstance(controllerType) as QQBotController;
        if (controller == null) return;

        // 初始化上下文
        controller.Client = _client!;
        controller.Message = message;
        controller.RawArguments = args;
        controller.RawContent = string.Join(" ", args);

        // 参数绑定
        var parameters = method.GetParameters();
        var invokeArgs = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            
            // 如果参数比输入多，且没有默认值，则报错或中止? 
            // 这里简单处理：如果输入不够，且没有默认值，就传默认值
            if (i >= args.Length)
            {
                if (param.HasDefaultValue)
                {
                    invokeArgs[i] = param.DefaultValue;
                }
                else if (param.ParameterType == typeof(string[])) 
                {
                     // 支持 params string[] reset 
                     invokeArgs[i] = Array.Empty<string>();
                }
                else
                {
                     // 缺少参数
                     // 可以抛出异常或提示用法
                     // throw new ArgumentException($"缺少参数: {param.Name}");
                     invokeArgs[i] = GetDefault(param.ParameterType);
                }
                continue;
            }

            // 绑定参数
            // 特殊处理: 最后一个参数如果是字符串数组，且有 ParamArrayAttribute (params)，则把剩余参数都给它
            bool isParams = param.GetCustomAttribute<ParamArrayAttribute>() != null;
            if (isParams && param.ParameterType == typeof(string[]))
            {
                var remaining = args.Skip(i).ToArray();
                invokeArgs[i] = remaining;
                break; // 结束
            }

            // 增强的类型转换
            try 
            {
                var input = args[i];
                var targetType = param.ParameterType;
                
                invokeArgs[i] = ConvertValue(input, targetType);
            }
            catch
            {
                // 转换失败，使用默认值
                 invokeArgs[i] = GetDefault(param.ParameterType);
            }
        }

        // 执行方法
        var result = method.Invoke(controller, invokeArgs);

        if (result is Task task)
        {
            await task;
            
            var taskType = task.GetType();
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultProp = taskType.GetProperty("Result");
                var taskResult = resultProp?.GetValue(task);
                if (taskResult != null)
                {
                    await ProcessCommandResultAsync(controller, taskResult);
                }
            }
        }
        else if (result != null)
        {
            await ProcessCommandResultAsync(controller, result);
        }
    }

    private async Task ProcessCommandResultAsync(QQBotController controller, object result)
    {
        var msg = controller.Message;
        var client = controller.Client;

        try 
        {
            // 1. ImageResult
            if (result is ImageResult imageResult)
            {
                if (!string.IsNullOrEmpty(msg.ChannelId))
                {
                    // 频道发送图片
                    await client.Api.SendMessageAsync(msg.ChannelId, new SendMessageRequest 
                    { 
                        MsgId = msg.Id,
                        Image = imageResult.Url 
                    });
                }
                else if (!string.IsNullOrEmpty(msg.GroupOpenId))
                {
                    // 群发送图片 - 先上传
                    var uploadResp = await client.Api.UploadGroupMediaAsync(msg.GroupOpenId, new UploadMediaRequest 
                    { 
                        FileType = 1, // 图片
                        Url = imageResult.Url,
                        SrvSendMsg = true // 设为true直接发送? 不, 文档常说先上传再发. 但这里参数是SrvSendMsg
                    });
                    
                    if (uploadResp?.FileInfo != null)
                    {
                        await client.Api.SendGroupMessageAsync(msg.GroupOpenId, new SendGroupMessageRequest
                        {
                            MsgId = msg.Id,
                            MsgType = 7,
                            Media = new MediaInfo { FileInfo = uploadResp.FileInfo },
                            MsgSeq = msg.Seq + 1
                        });
                    }
                }
                else if (msg.Author?.UserOpenId != null)
                {
                    // C2C发送图片
                    var openId = msg.Author.UserOpenId;
                    var uploadResp = await client.Api.UploadC2CMediaAsync(openId, new UploadMediaRequest 
                    { 
                        FileType = 1, 
                        Url = imageResult.Url 
                    });

                    if (uploadResp?.FileInfo != null)
                    {
                        await client.Api.SendC2CMessageAsync(openId, new SendGroupMessageRequest
                        {
                            MsgId = msg.Id,
                            MsgType = 7,
                            Media = new MediaInfo { FileInfo = uploadResp.FileInfo },
                            MsgSeq = msg.Seq + 1
                        });
                    }
                }
                return;
            }

            // 2. MessageMarkdown
            if (result is MessageMarkdown markdown)
            {
                if (!string.IsNullOrEmpty(msg.ChannelId))
                {
                    await client.Api.SendMessageAsync(msg.ChannelId, new SendMessageRequest 
                    { 
                        MsgId = msg.Id,
                        Markdown = markdown
                    });
                }
                else if (!string.IsNullOrEmpty(msg.GroupOpenId))
                {
                    await client.Api.SendGroupMessageAsync(msg.GroupOpenId, new SendGroupMessageRequest
                    {
                        MsgId = msg.Id,
                        MsgType = 2,
                        Markdown = markdown,
                        MsgSeq = msg.Seq + 1
                    });
                }
                else if (msg.Author?.UserOpenId != null)
                {
                     await client.Api.SendC2CMessageAsync(msg.Author.UserOpenId, new SendGroupMessageRequest
                    {
                        MsgId = msg.Id,
                        MsgType = 2,
                        Markdown = markdown,
                        MsgSeq = msg.Seq + 1
                    });
                }
                return;
            }

            // 3. String (Default)
            var content = result.ToString();
            if (!string.IsNullOrEmpty(content))
            {
                await controller.ReplyAsync(content);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending command result");
        }
    }

    /// <summary>
    /// 增强的类型转换 - 支持 bool, enum, Guid 等
    /// </summary>
    private object? ConvertValue(string input, Type targetType)
    {
        // 处理可空类型
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            targetType = underlyingType;
        }

        // 字符串
        if (targetType == typeof(string))
            return input;

        // 布尔值
        if (targetType == typeof(bool))
        {
            if (bool.TryParse(input, out var boolValue))
                return boolValue;
            // 支持 1/0, yes/no, on/off
            return input.ToLowerInvariant() is "1" or "yes" or "on" or "true";
        }

        // Guid
        if (targetType == typeof(Guid))
        {
            if (Guid.TryParse(input, out var guidValue))
                return guidValue;
            throw new FormatException($"Cannot convert '{input}' to Guid");
        }

        // 枚举
        if (targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, input, true, out var enumValue))
                return enumValue;
            throw new FormatException($"Cannot convert '{input}' to {targetType.Name}");
        }

        // TypeConverter
        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFromString(input);
        }

        // 基础类型 (int, double, etc.)
        return Convert.ChangeType(input, targetType);
    }

    /// <summary>
    /// 获取类型默认值
    /// </summary>
    private object? GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}
