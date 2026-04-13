using Luolan.QQBot.Models;

namespace Luolan.QQBot.Helpers;

/// <summary>
/// 按钮键盘构建器
/// </summary>
public class KeyboardBuilder
{
    private readonly List<List<Button>> _rows = new();
    private List<Button>? _currentRow;

    /// <summary>
    /// 开始新的一行
    /// </summary>
    /// <returns></returns>
    public KeyboardBuilder NewRow()
    {
        _currentRow = new List<Button>();
        _rows.Add(_currentRow);
        return this;
    }

    /// <summary>
    /// 添加按钮到当前行
    /// </summary>
    /// <param name="button">按钮</param>
    /// <returns></returns>
    public KeyboardBuilder AddButton(Button button)
    {
        if (_currentRow == null)
        {
            NewRow();
        }
        _currentRow!.Add(button);
        return this;
    }

    /// <summary>
    /// 添加文本按钮
    /// </summary>
    /// <param name="id">按钮ID</param>
    /// <param name="label">按钮文本</param>
    /// <param name="visitedLabel">点击后文本</param>
    /// <param name="style">样式(0灰色 1蓝色)</param>
    /// <returns></returns>
    public KeyboardBuilder AddButton(string id, string label, string? visitedLabel = null, int style = 1)
    {
        return AddButton(new Button
        {
            Id = id,
            RenderData = new ButtonRenderData
            {
                Label = label,
                VisitedLabel = visitedLabel ?? label,
                Style = style
            },
            Action = new ButtonAction
            {
                Type = 2,
                Permission = new ButtonPermission { Type = 2 },
                Data = id
            }
        });
    }

    /// <summary>
    /// 添加链接按钮
    /// </summary>
    /// <param name="id">按钮ID</param>
    /// <param name="label">按钮文本</param>
    /// <param name="url">跳转URL</param>
    /// <param name="style">样式(0灰色 1蓝色)</param>
    /// <returns></returns>
    public KeyboardBuilder AddLinkButton(string id, string label, string url, int style = 1)
    {
        return AddButton(new Button
        {
            Id = id,
            RenderData = new ButtonRenderData
            {
                Label = label,
                VisitedLabel = label,
                Style = style
            },
            Action = new ButtonAction
            {
                Type = 0,
                Permission = new ButtonPermission { Type = 2 },
                Data = url
            }
        });
    }

    /// <summary>
    /// 构建键盘对象
    /// </summary>
    /// <returns></returns>
    public MessageKeyboard Build()
    {
        return new MessageKeyboard
        {
            Content = new InlineKeyboard
            {
                Rows = _rows.Select(r => new InlineKeyboardRow
                {
                    Buttons = r
                }).ToList()
            }
        };
    }

    /// <summary>
    /// 快速创建单行按钮键盘
    /// </summary>
    /// <param name="buttons">按钮列表</param>
    /// <returns></returns>
    public static MessageKeyboard FromButtons(params Button[] buttons)
    {
        var builder = new KeyboardBuilder();
        builder.NewRow();
        foreach (var button in buttons)
        {
            builder.AddButton(button);
        }
        return builder.Build();
    }
}
