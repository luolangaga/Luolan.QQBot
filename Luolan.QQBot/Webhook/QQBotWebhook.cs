using System.Text.Json;
using Luolan.QQBot.Events;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Webhook;

/// <summary>
/// Webhook 事件处理器(HTTP 回调接入方式)
/// </summary>
public sealed class QQBotWebhook
{
    /// <summary>
    /// 回调验证(OpCode 13)的操作码
    /// </summary>
    public const int OpValidation = 13;

    /// <summary>
    /// 回调应答(OpCode 12)的操作码
    /// </summary>
    public const int OpCallbackAck = 12;

    private readonly string _clientSecret;
    private readonly EventDispatcher _eventDispatcher;
    private readonly ILogger<QQBotWebhook>? _logger;

    /// <summary>
    /// 创建 Webhook 处理器
    /// </summary>
    /// <param name="client">QQ机器人客户端(用于事件分发)</param>
    /// <param name="clientSecret">机器人 ClientSecret(用于签名校验)</param>
    /// <param name="logger">日志</param>
    public QQBotWebhook(QQBotClient client, string clientSecret, ILogger<QQBotWebhook>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        _clientSecret = clientSecret;
        _eventDispatcher = client.Events;
        _logger = logger;
    }

    /// <summary>
    /// 处理一次 Webhook 回调, 返回应写回的 JSON 字符串
    /// </summary>
    /// <param name="body">HTTP 请求体原文</param>
    /// <param name="signature">X-Signature-Ed25519 请求头</param>
    /// <param name="timestamp">X-Signature-Timestamp 请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task<string> HandleAsync(
        string body,
        string? signature,
        string? timestamp,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var op = root.TryGetProperty("op", out var opElement) ? opElement.GetInt32() : -1;

        // 回调地址验证(op=13)由平台发起, 不带签名头; 能正确签回即证明身份
        if (op == OpValidation)
        {
            var data = root.GetProperty("d");
            var plainToken = data.GetProperty("plain_token").GetString() ?? string.Empty;
            var eventTs = data.GetProperty("event_ts").GetString() ?? string.Empty;
            return BuildValidationResponse(plainToken, eventTs);
        }

        if (!VerifySignature(body, signature, timestamp))
        {
            throw new UnauthorizedAccessException("Webhook 签名校验失败");
        }

        if (op == 0)
        {
            var eventType = root.TryGetProperty("t", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            var eventId = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;

            if (root.TryGetProperty("d", out var eventData))
            {
                await _eventDispatcher.DispatchAsync(eventType, eventData, eventId);
            }

            _logger?.LogDebug("Webhook 事件已处理: {EventType}", eventType);
        }

        return "{\"op\":12}";
    }

    /// <summary>
    /// 校验回调签名
    /// </summary>
    public bool VerifySignature(string body, string? signature, string? timestamp)
        => QQBotWebhookSignature.Verify(_clientSecret, body, signature, timestamp);

    /// <summary>
    /// 计算回调地址验证所需的签名
    /// </summary>
    public string SignValidation(string plainToken, string eventTs)
        => QQBotWebhookSignature.SignValidation(_clientSecret, plainToken, eventTs);

    private string BuildValidationResponse(string plainToken, string eventTs)
    {
        var signature = SignValidation(plainToken, eventTs);
        return JsonSerializer.Serialize(new
        {
            plain_token = plainToken,
            signature
        });
    }
}
