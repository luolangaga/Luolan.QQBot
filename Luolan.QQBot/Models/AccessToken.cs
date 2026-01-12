using System.Text.Json;
using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 访问令牌响应
/// </summary>
public class AccessTokenResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间(秒) - QQ Bot API 可能返回字符串或数字
    /// </summary>
    [JsonPropertyName("expires_in")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int ExpiresIn { get; set; }
}

/// <summary>
/// 灵活的整数转换器，支持字符串和数字类型
/// </summary>
public class FlexibleIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (int.TryParse(reader.GetString(), out var value))
            {
                return value;
            }
            throw new JsonException($"Unable to convert \"{reader.GetString()}\" to Int32.");
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }
        
        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// 缓存的访问令牌
/// </summary>
public class CachedAccessToken
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// 是否需要刷新
    /// </summary>
    public bool NeedsRefresh(int secondsBeforeExpire) =>
        DateTime.UtcNow >= ExpiresAt.AddSeconds(-secondsBeforeExpire);
}
