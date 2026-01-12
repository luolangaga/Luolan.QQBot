using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Luolan.QQBot.Events;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot.Services;

/// <summary>
/// QQ Bot WebSocket 客户端
/// </summary>
public class QQBotWebSocketClient : IDisposable
{
    private readonly QQBotClientOptions _options;
    private readonly QQBotHttpClient _httpClient;
    private readonly TokenManager _tokenManager;
    private readonly EventDispatcher _eventDispatcher;
    private readonly ILogger<QQBotWebSocketClient>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private Timer? _heartbeatTimer;
    private WebSocketSession _session = new();
    private int _reconnectAttempts;
    private bool _isRunning;
    private bool _disposed;

    /// <summary>
    /// 连接状态改变事件
    /// </summary>
    public event Action<WebSocketState>? OnStateChanged;

    /// <summary>
    /// 连接已建立事件
    /// </summary>
    public event Action<User>? OnConnected;

    /// <summary>
    /// 连接断开事件
    /// </summary>
    public event Action<string?>? OnDisconnected;

    /// <summary>
    /// 当前连接状态
    /// </summary>
    public WebSocketState State => _webSocket?.State ?? WebSocketState.None;

    /// <summary>
    /// 是否已连接并鉴权成功
    /// </summary>
    public bool IsConnected => _webSocket?.State == WebSocketState.Open && _session.IsAuthenticated;

    /// <summary>
    /// 当前会话信息
    /// </summary>
    public WebSocketSession Session => _session;

    /// <summary>
    /// 事件分发器
    /// </summary>
    public EventDispatcher Events => _eventDispatcher;

    public QQBotWebSocketClient(
        QQBotClientOptions options,
        QQBotHttpClient httpClient,
        TokenManager tokenManager,
        EventDispatcher eventDispatcher,
        ILogger<QQBotWebSocketClient>? logger = null)
    {
        _options = options;
        _httpClient = httpClient;
        _tokenManager = tokenManager;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 启动WebSocket连接
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger?.LogWarning("WebSocket客户端已在运行中");
            return;
        }

        _isRunning = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _reconnectAttempts = 0;

        await ConnectAsync(_cts.Token);
    }

    /// <summary>
    /// 停止WebSocket连接
    /// </summary>
    public async Task StopAsync()
    {
        _isRunning = false;
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;

        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "关闭WebSocket时发生异常");
            }
        }

        _cts?.Cancel();
        _webSocket?.Dispose();
        _webSocket = null;
        _session = new WebSocketSession();

        OnDisconnected?.Invoke("客户端主动停止");
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 获取网关地址
            if (string.IsNullOrEmpty(_session.GatewayUrl))
            {
                var gatewayInfo = await _httpClient.GetGatewayBotAsync(cancellationToken);
                _session.GatewayUrl = gatewayInfo.Url;
                _logger?.LogInformation("获取到网关地址: {Url}", _session.GatewayUrl);
            }

            // 创建WebSocket连接
            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();

            _logger?.LogInformation("正在连接到WebSocket网关...");
            await _webSocket.ConnectAsync(new Uri(_session.GatewayUrl), cancellationToken);
            _logger?.LogInformation("WebSocket连接已建立");

            OnStateChanged?.Invoke(WebSocketState.Open);

            // 开始接收消息
            _ = ReceiveLoopAsync(_cts!.Token);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "连接WebSocket失败");
            await HandleReconnectAsync();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        var messageBuffer = new MemoryStream();

        try
        {
            while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger?.LogWarning("服务端关闭连接: {Status} - {Description}",
                            result.CloseStatus, result.CloseStatusDescription);
                        break;
                    }

                    messageBuffer.Write(buffer, 0, result.Count);

                    if (result.EndOfMessage)
                    {
                        var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                        messageBuffer.SetLength(0);

                        await ProcessMessageAsync(message, cancellationToken);
                    }
                }
                catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    _logger?.LogWarning("WebSocket连接异常断开");
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger?.LogDebug("接收循环被取消");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "接收消息时发生异常");
        }
        finally
        {
            messageBuffer.Dispose();
            if (_isRunning)
            {
                await HandleReconnectAsync();
            }
        }
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        _logger?.LogDebug("收到消息: {Message}", message.Length > 500 ? message[..500] + "..." : message);

        try
        {
            var payload = JsonSerializer.Deserialize<WebSocketPayload>(message, _jsonOptions);
            if (payload == null) return;

            // 更新序列号
            if (payload.Sequence.HasValue)
            {
                _session.LastSequence = payload.Sequence.Value;
            }

            switch (payload.Op)
            {
                case OpCode.Hello:
                    await HandleHelloAsync(payload, cancellationToken);
                    break;

                case OpCode.Dispatch:
                    await HandleDispatchAsync(payload, cancellationToken);
                    break;

                case OpCode.HeartbeatAck:
                    _logger?.LogDebug("收到心跳确认");
                    break;

                case OpCode.Reconnect:
                    _logger?.LogWarning("服务端要求重连");
                    await HandleReconnectAsync();
                    break;

                case OpCode.InvalidSession:
                    _logger?.LogWarning("会话无效, 需要重新鉴权");
                    _session = new WebSocketSession { GatewayUrl = _session.GatewayUrl };
                    await HandleReconnectAsync();
                    break;

                default:
                    _logger?.LogDebug("未处理的操作码: {Op}", payload.Op);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "解析消息JSON失败: {Message}", message);
        }
    }

    private async Task HandleHelloAsync(WebSocketPayload payload, CancellationToken cancellationToken)
    {
        if (payload.Data is JsonElement data)
        {
            var helloData = data.Deserialize<HelloData>(_jsonOptions);
            if (helloData != null)
            {
                _session.HeartbeatInterval = helloData.HeartbeatInterval;
                _logger?.LogInformation("收到Hello, 心跳周期: {Interval}ms", _session.HeartbeatInterval);

                // 启动心跳
                StartHeartbeat();

                // 鉴权或恢复会话
                if (!string.IsNullOrEmpty(_session.SessionId))
                {
                    await SendResumeAsync(cancellationToken);
                }
                else
                {
                    await SendIdentifyAsync(cancellationToken);
                }
            }
        }
    }

    private async Task HandleDispatchAsync(WebSocketPayload payload, CancellationToken cancellationToken)
    {
        var eventType = payload.Type ?? string.Empty;

        if (payload.Data is JsonElement data)
        {
            // 特殊处理Ready事件
            if (eventType == "READY")
            {
                var readyData = data.Deserialize<ReadyData>(_jsonOptions);
                if (readyData != null)
                {
                    _session.SessionId = readyData.SessionId;
                    _session.IsAuthenticated = true;
                    _session.BotUser = readyData.User;
                    _reconnectAttempts = 0;

                    _logger?.LogInformation("鉴权成功, SessionId: {SessionId}, 用户: {Username}",
                        _session.SessionId, readyData.User?.Username);

                    OnConnected?.Invoke(readyData.User!);
                }
            }
            else if (eventType == "RESUMED")
            {
                _session.IsAuthenticated = true;
                _reconnectAttempts = 0;
                _logger?.LogInformation("会话恢复成功");
            }

            // 分发事件
            var eventId = payload.Id;
            await _eventDispatcher.DispatchAsync(eventType, data, eventId);
        }
    }

    private async Task SendIdentifyAsync(CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetAccessTokenAsync(cancellationToken);

        var identify = new WebSocketPayload
        {
            Op = OpCode.Identify,
            Data = new IdentifyData
            {
                Token = $"QQBot {token}",
                Intents = (int)_options.Intents,
                Shard = new[] { 0, 1 },
                Properties = new IdentifyProperties()
            }
        };

        await SendPayloadAsync(identify, cancellationToken);
        _logger?.LogDebug("已发送鉴权请求");
    }

    private async Task SendResumeAsync(CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetAccessTokenAsync(cancellationToken);

        var resume = new WebSocketPayload
        {
            Op = OpCode.Resume,
            Data = new ResumeData
            {
                Token = $"QQBot {token}",
                SessionId = _session.SessionId,
                Seq = _session.LastSequence
            }
        };

        await SendPayloadAsync(resume, cancellationToken);
        _logger?.LogDebug("已发送恢复会话请求");
    }

    private void StartHeartbeat()
    {
        _heartbeatTimer?.Dispose();

        _heartbeatTimer = new Timer(async _ =>
        {
            if (_webSocket?.State != WebSocketState.Open) return;

            try
            {
                var heartbeat = new WebSocketPayload
                {
                    Op = OpCode.Heartbeat,
                    Data = _session.LastSequence > 0 ? _session.LastSequence : null
                };

                await SendPayloadAsync(heartbeat, CancellationToken.None);
                _logger?.LogDebug("已发送心跳, seq: {Seq}", _session.LastSequence);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发送心跳失败");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_session.HeartbeatInterval));
    }

    private async Task SendPayloadAsync(WebSocketPayload payload, CancellationToken cancellationToken)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            _logger?.LogWarning("WebSocket未连接, 无法发送消息");
            return;
        }

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var buffer = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            cancellationToken
        );
    }

    private async Task HandleReconnectAsync()
    {
        if (!_isRunning) return;

        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
        _session.IsAuthenticated = false;

        OnStateChanged?.Invoke(WebSocketState.Closed);
        OnDisconnected?.Invoke("连接断开, 尝试重连");

        _reconnectAttempts++;

        if (_reconnectAttempts > _options.WebSocketMaxReconnectAttempts)
        {
            _logger?.LogError("重连次数超过最大限制 ({Max}), 停止重连",
                _options.WebSocketMaxReconnectAttempts);
            _isRunning = false;
            return;
        }

        var delay = Math.Min(_options.WebSocketReconnectIntervalMs * _reconnectAttempts, 30000);
        _logger?.LogInformation("将在 {Delay}ms 后进行第 {Attempt} 次重连",
            delay, _reconnectAttempts);

        await Task.Delay(delay, _cts?.Token ?? CancellationToken.None);

        if (_isRunning)
        {
            await ConnectAsync(_cts?.Token ?? CancellationToken.None);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _isRunning = false;
        _heartbeatTimer?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
        _webSocket?.Dispose();
    }
}
