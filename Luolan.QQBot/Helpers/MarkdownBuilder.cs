using Luolan.QQBot.Models;

namespace Luolan.QQBot.Helpers;

/// <summary>
/// Markdown消息构建器
/// </summary>
public class MarkdownBuilder
{
    private string? _customTemplateId;
    private string? _content;
    private readonly Dictionary<string, List<string>> _params = new();

    /// <summary>
    /// 使用模板ID（custom_template_id）
    /// </summary>
    /// <param name="customTemplateId">模板ID（例如：101993071_1658748972）</param>
    /// <returns></returns>
    public MarkdownBuilder UseTemplate(string customTemplateId)
    {
        _customTemplateId = customTemplateId;
        _content = null;
        return this;
    }

    /// <summary>
    /// 兼容旧用法：将数字模板ID转为字符串。
    /// </summary>
    [Obsolete("QQ Bot 模板应使用 custom_template_id(string)。请改用 UseTemplate(string)。")]
    public MarkdownBuilder UseTemplate(int templateId) => UseTemplate(templateId.ToString());

    /// <summary>
    /// 使用原生Markdown内容
    /// </summary>
    /// <param name="content">Markdown内容</param>
    /// <returns></returns>
    public MarkdownBuilder UseContent(string content)
    {
        _content = content;
        _customTemplateId = null;
        _params.Clear();
        return this;
    }

    /// <summary>
    /// 添加模板参数(单个值)
    /// </summary>
    /// <param name="key">参数名</param>
    /// <param name="value">参数值</param>
    /// <returns></returns>
    public MarkdownBuilder AddParam(string key, string value)
    {
        if (!_params.ContainsKey(key))
        {
            _params[key] = new List<string>();
        }
        _params[key].Add(value);
        return this;
    }

    /// <summary>
    /// 添加模板参数(多个值)
    /// </summary>
    /// <param name="key">参数名</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public MarkdownBuilder AddParam(string key, params string[] values)
    {
        if (!_params.ContainsKey(key))
        {
            _params[key] = new List<string>();
        }
        _params[key].AddRange(values);
        return this;
    }

    /// <summary>
    /// 设置模板参数(覆盖已有值)
    /// </summary>
    /// <param name="key">参数名</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public MarkdownBuilder SetParam(string key, params string[] values)
    {
        _params[key] = new List<string>(values);
        return this;
    }

    /// <summary>
    /// 构建Markdown对象
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MessageMarkdown Build()
    {
        if (string.IsNullOrEmpty(_customTemplateId) && string.IsNullOrEmpty(_content))
        {
            throw new InvalidOperationException("必须指定 CustomTemplateId 或 Content");
        }

        var markdown = new MessageMarkdown();

        if (!string.IsNullOrEmpty(_customTemplateId))
        {
            markdown.CustomTemplateId = _customTemplateId;
            if (_params.Any())
            {
                markdown.Params = _params.Select(p => new MessageMarkdownParam
                {
                    Key = p.Key,
                    Values = p.Value
                }).ToList();
            }
        }
        else
        {
            markdown.Content = _content;
        }

        return markdown;
    }

    /// <summary>
    /// 快速创建原生Markdown消息
    /// </summary>
    /// <param name="content">Markdown内容</param>
    /// <returns></returns>
    public static MessageMarkdown FromContent(string content)
    {
        return new MessageMarkdown { Content = content };
    }

    /// <summary>
    /// 快速创建模板消息(无参数)
    /// </summary>
    /// <param name="customTemplateId">模板ID（custom_template_id）</param>
    /// <returns></returns>
    public static MessageMarkdown FromTemplate(string customTemplateId)
    {
        return new MessageMarkdown { CustomTemplateId = customTemplateId };
    }

    /// <summary>
    /// 兼容旧用法：将数字模板ID转为字符串。
    /// </summary>
    [Obsolete("QQ Bot 模板应使用 custom_template_id(string)。请改用 FromTemplate(string)。")]
    public static MessageMarkdown FromTemplate(int templateId) => FromTemplate(templateId.ToString());

    /// <summary>
    /// 快速创建模板消息(带参数)
    /// </summary>
    /// <param name="customTemplateId">模板ID（custom_template_id）</param>
    /// <param name="parameters">参数字典</param>
    /// <returns></returns>
    public static MessageMarkdown FromTemplate(string customTemplateId, Dictionary<string, string[]> parameters)
    {
        return new MessageMarkdown
        {
            CustomTemplateId = customTemplateId,
            Params = parameters.Select(p => new MessageMarkdownParam
            {
                Key = p.Key,
                Values = p.Value.ToList()
            }).ToList()
        };
    }

    /// <summary>
    /// 兼容旧用法：将数字模板ID转为字符串。
    /// </summary>
    [Obsolete("QQ Bot 模板应使用 custom_template_id(string)。请改用 FromTemplate(string, Dictionary<...>)。")]
    public static MessageMarkdown FromTemplate(int templateId, Dictionary<string, string[]> parameters)
        => FromTemplate(templateId.ToString(), parameters);
}
