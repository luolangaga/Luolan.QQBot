using System.Text;

namespace Luolan.QQBot.Helpers;

/// <summary>
/// 命令参数解析器 - 支持引号
/// </summary>
public static class CommandParser
{
    /// <summary>
    /// 解析命令字符串，支持引号
    /// </summary>
    /// <param name="input">输入字符串，如: cmd "hello world" test</param>
    /// <returns>解析后的参数数组</returns>
    public static string[] Parse(string input)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var escape = false;

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (escape)
            {
                current.Append(c);
                escape = false;
                continue;
            }

            if (c == '\\')
            {
                escape = true;
                continue;
            }

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        return parts.ToArray();
    }
}
