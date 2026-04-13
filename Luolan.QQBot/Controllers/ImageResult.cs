namespace Luolan.QQBot.Controllers;

/// <summary>
/// 图片结果
/// </summary>
public class ImageResult
{
    /// <summary>
    /// 图片URL
    /// </summary>
    public string Url { get; }

    public ImageResult(string url)
    {
        Url = url;
    }

    public static implicit operator ImageResult(string url) => new(url);
}
