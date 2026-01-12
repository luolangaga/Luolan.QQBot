using System.Net.WebSockets;
using Luolan.QQBot.Events;
using Luolan.QQBot.Models;
using Luolan.QQBot.Services;
using Microsoft.Extensions.Logging;

namespace Luolan.QQBot;

/// <summary>
/// QQ机器人客户端 - 整合HTTP和WebSocket功能
/// </summary>
public class QQBotClient : IDisposable
{
    private readonly QQBotClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;
    private readonly QQBotHttpClient _apiClient;
    private readonly QQBotWebSocketClient _webSocketClient;
    private readonly EventDispatcher _eventDispatcher;
    private readonly ILogger<QQBotClient>? _logger;
    private bool _disposed;

    /// <summary>
    /// 配置选项
    /// </summary>
    public QQBotClientOptions Options => _options;

    /// <summary>
    /// HTTP API客户端
    /// </summary>
    public QQBotHttpClient Api => _apiClient;

    /// <summary>
    /// WebSocket客户端
    /// </summary>
    public QQBotWebSocketClient WebSocket => _webSocketClient;

    /// <summary>
    /// 事件分发器
    /// </summary>
    public EventDispatcher Events => _eventDispatcher;

    /// <summary>
    /// Token管理器
    /// </summary>
    public TokenManager TokenManager => _tokenManager;

    /// <summary>
    /// 是否已连接(WebSocket)
    /// </summary>
    public bool IsConnected => _webSocketClient.IsConnected;

    /// <summary>
    /// 当前机器人用户信息
    /// </summary>
    public User? CurrentUser => _webSocketClient.Session.BotUser;

    #region 快捷事件访问

    /// <summary>
    /// 连接就绪事件
    /// </summary>
    public event Func<ReadyEvent, Task>? OnReady
    {
        add => _eventDispatcher.OnReady += value;
        remove => _eventDispatcher.OnReady -= value;
    }

    /// <summary>
    /// 频道消息事件(私域)
    /// </summary>
    public event Func<MessageCreateEvent, Task>? OnMessageCreate
    {
        add => _eventDispatcher.OnMessageCreate += value;
        remove => _eventDispatcher.OnMessageCreate -= value;
    }

    /// <summary>
    /// @机器人消息事件(公域)
    /// </summary>
    public event Func<AtMessageCreateEvent, Task>? OnAtMessageCreate
    {
        add => _eventDispatcher.OnAtMessageCreate += value;
        remove => _eventDispatcher.OnAtMessageCreate -= value;
    }

    /// <summary>
    /// 私信消息事件
    /// </summary>
    public event Func<DirectMessageCreateEvent, Task>? OnDirectMessageCreate
    {
        add => _eventDispatcher.OnDirectMessageCreate += value;
        remove => _eventDispatcher.OnDirectMessageCreate -= value;
    }

    /// <summary>
    /// 群@机器人消息事件
    /// </summary>
    public event Func<GroupAtMessageCreateEvent, Task>? OnGroupAtMessageCreate
    {
        add => _eventDispatcher.OnGroupAtMessageCreate += value;
        remove => _eventDispatcher.OnGroupAtMessageCreate -= value;
    }

    /// <summary>
    /// C2C消息事件
    /// </summary>
    public event Func<C2CMessageCreateEvent, Task>? OnC2CMessageCreate
    {
        add => _eventDispatcher.OnC2CMessageCreate += value;
        remove => _eventDispatcher.OnC2CMessageCreate -= value;
    }

    /// <summary>
    /// 互动事件(按钮回调)
    /// </summary>
    public event Func<InteractionCreateEvent, Task>? OnInteractionCreate
    {
        add => _eventDispatcher.OnInteractionCreate += value;
        remove => _eventDispatcher.OnInteractionCreate -= value;
    }

    /// <summary>
    /// 频道创建事件
    /// </summary>
    public event Func<GuildCreateEvent, Task>? OnGuildCreate
    {
        add => _eventDispatcher.OnGuildCreate += value;
        remove => _eventDispatcher.OnGuildCreate -= value;
    }

    /// <summary>
    /// 频道删除事件
    /// </summary>
    public event Func<GuildDeleteEvent, Task>? OnGuildDelete
    {
        add => _eventDispatcher.OnGuildDelete += value;
        remove => _eventDispatcher.OnGuildDelete -= value;
    }

    /// <summary>
    /// 成员加入事件
    /// </summary>
    public event Func<GuildMemberAddEvent, Task>? OnGuildMemberAdd
    {
        add => _eventDispatcher.OnGuildMemberAdd += value;
        remove => _eventDispatcher.OnGuildMemberAdd -= value;
    }

    /// <summary>
    /// 成员离开事件
    /// </summary>
    public event Func<GuildMemberRemoveEvent, Task>? OnGuildMemberRemove
    {
        add => _eventDispatcher.OnGuildMemberRemove += value;
        remove => _eventDispatcher.OnGuildMemberRemove -= value;
    }

    /// <summary>
    /// 群添加机器人事件
    /// </summary>
    public event Func<GroupAddRobotEvent, Task>? OnGroupAddRobot
    {
        add => _eventDispatcher.OnGroupAddRobot += value;
        remove => _eventDispatcher.OnGroupAddRobot -= value;
    }

    /// <summary>
    /// 群删除机器人事件
    /// </summary>
    public event Func<GroupDelRobotEvent, Task>? OnGroupDelRobot
    {
        add => _eventDispatcher.OnGroupDelRobot += value;
        remove => _eventDispatcher.OnGroupDelRobot -= value;
    }

    /// <summary>
    /// 好友添加事件
    /// </summary>
    public event Func<FriendAddEvent, Task>? OnFriendAdd
    {
        add => _eventDispatcher.OnFriendAdd += value;
        remove => _eventDispatcher.OnFriendAdd -= value;
    }

    /// <summary>
    /// 好友删除事件
    /// </summary>
    public event Func<FriendDelEvent, Task>? OnFriendDel
    {
        add => _eventDispatcher.OnFriendDel += value;
        remove => _eventDispatcher.OnFriendDel -= value;
    }

    #endregion

    /// <summary>
    /// 创建QQ机器人客户端
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="loggerFactory">日志工厂(可选)</param>
    public QQBotClient(QQBotClientOptions options, ILoggerFactory? loggerFactory = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = loggerFactory?.CreateLogger<QQBotClient>();

        // 创建HttpClient
        _httpClient = new HttpClient();

        // 创建各个组件
        _tokenManager = new TokenManager(
            _options,
            _httpClient,
            loggerFactory?.CreateLogger<TokenManager>()
        );

        _apiClient = new QQBotHttpClient(
            _options,
            _httpClient,
            _tokenManager,
            loggerFactory?.CreateLogger<QQBotHttpClient>()
        );

        _eventDispatcher = new EventDispatcher(
            loggerFactory?.CreateLogger<EventDispatcher>()
        );

        _webSocketClient = new QQBotWebSocketClient(
            _options,
            _apiClient,
            _tokenManager,
            _eventDispatcher,
            loggerFactory?.CreateLogger<QQBotWebSocketClient>()
        );
    }

    /// <summary>
    /// 内部构造函数 - 用于依赖注入
    /// </summary>
    internal QQBotClient(
        QQBotClientOptions options,
        HttpClient httpClient,
        TokenManager tokenManager,
        QQBotHttpClient apiClient,
        QQBotWebSocketClient webSocketClient,
        EventDispatcher eventDispatcher,
        ILogger<QQBotClient>? logger = null)
    {
        _options = options;
        _httpClient = httpClient;
        _tokenManager = tokenManager;
        _apiClient = apiClient;
        _webSocketClient = webSocketClient;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// 启动机器人(连接WebSocket)
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("正在启动QQ机器人...");
        return _webSocketClient.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 停止机器人
    /// </summary>
    public Task StopAsync()
    {
        _logger?.LogInformation("正在停止QQ机器人...");
        return _webSocketClient.StopAsync();
    }

    #region 消息发送快捷方法

    /// <summary>
    /// 回复频道消息
    /// </summary>
    public Task<Message> ReplyAsync(Message sourceMessage, string content)
    {
        if (string.IsNullOrEmpty(sourceMessage.ChannelId))
            throw new ArgumentException("消息没有ChannelId");

        return _apiClient.SendMessageAsync(sourceMessage.ChannelId, new SendMessageRequest
        {
            Content = content,
            MsgId = sourceMessage.Id
        });
    }

    /// <summary>
    /// 回复群消息
    /// </summary>
    public Task<SendGroupMessageResponse> ReplyGroupAsync(Message sourceMessage, string content, int msgSeq = 0)
    {
        if (string.IsNullOrEmpty(sourceMessage.GroupOpenId))
            throw new ArgumentException("消息没有GroupOpenId");

        return _apiClient.SendGroupTextMessageAsync(
            sourceMessage.GroupOpenId,
            content,
            sourceMessage.Id,
            msgSeq
        );
    }

    /// <summary>
    /// 回复C2C消息
    /// </summary>
    public Task<SendGroupMessageResponse> ReplyC2CAsync(Message sourceMessage, string content, int msgSeq = 0)
    {
        var openId = sourceMessage.Author?.UserOpenId;
        if (string.IsNullOrEmpty(openId))
            throw new ArgumentException("消息没有UserOpenId");

        return _apiClient.SendC2CTextMessageAsync(
            openId,
            content,
            sourceMessage.Id,
            msgSeq
        );
    }

    /// <summary>
    /// 发送频道消息
    /// </summary>
    public Task<Message> SendChannelMessageAsync(string channelId, string content, string? msgId = null)
        => _apiClient.SendTextMessageAsync(channelId, content, msgId);

    /// <summary>
    /// 发送频道消息(完整请求)
    /// </summary>
    public Task<Message> SendChannelMessageAsync(string channelId, SendMessageRequest request)
        => _apiClient.SendMessageAsync(channelId, request);

    /// <summary>
    /// 发送私信
    /// </summary>
    public async Task<Message> SendDirectMessageAsync(string userId, string sourceGuildId, string content, string? msgId = null)
    {
        // 先创建私信会话
        var session = await _apiClient.CreateDmsAsync(new CreateDmsRequest
        {
            RecipientId = userId,
            SourceGuildId = sourceGuildId
        });

        // 发送私信
        return await _apiClient.SendDmsAsync(session.GuildId, new SendMessageRequest
        {
            Content = content,
            MsgId = msgId
        });
    }

    /// <summary>
    /// 发送群消息
    /// </summary>
    public Task<SendGroupMessageResponse> SendGroupMessageAsync(string groupOpenId, string content, string? msgId = null, int msgSeq = 0)
        => _apiClient.SendGroupTextMessageAsync(groupOpenId, content, msgId, msgSeq);

    /// <summary>
    /// 发送群消息(完整请求)
    /// </summary>
    public Task<SendGroupMessageResponse> SendGroupMessageAsync(string groupOpenId, SendGroupMessageRequest request)
        => _apiClient.SendGroupMessageAsync(groupOpenId, request);

    /// <summary>
    /// 发送C2C消息
    /// </summary>
    public Task<SendGroupMessageResponse> SendC2CMessageAsync(string openId, string content, string? msgId = null, int msgSeq = 0)
        => _apiClient.SendC2CTextMessageAsync(openId, content, msgId, msgSeq);

    /// <summary>
    /// 发送C2C消息(完整请求)
    /// </summary>
    public Task<SendGroupMessageResponse> SendC2CMessageAsync(string openId, SendGroupMessageRequest request)
        => _apiClient.SendC2CMessageAsync(openId, request);

    #endregion

    #region 频道管理快捷方法

    /// <summary>
    /// 获取机器人加入的频道列表
    /// </summary>
    public Task<List<Guild>> GetGuildsAsync()
        => _apiClient.GetCurrentUserGuildsAsync();

    /// <summary>
    /// 获取频道详情
    /// </summary>
    public Task<Guild> GetGuildAsync(string guildId)
        => _apiClient.GetGuildAsync(guildId);

    /// <summary>
    /// 获取子频道列表
    /// </summary>
    public Task<List<Channel>> GetChannelsAsync(string guildId)
        => _apiClient.GetChannelsAsync(guildId);

    /// <summary>
    /// 获取频道成员列表
    /// </summary>
    public Task<List<Member>> GetMembersAsync(string guildId, string? after = null, int limit = 100)
        => _apiClient.GetGuildMembersAsync(guildId, after, limit);

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _webSocketClient.Dispose();
        _apiClient.Dispose();
        _tokenManager.Dispose();
        _httpClient.Dispose();

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// QQ机器人客户端构建器
/// </summary>
public class QQBotClientBuilder
{
    private string? _appId;
    private string? _clientSecret;
    private bool _isSandbox;
    private Intents _intents = Intents.Default;
    private int _tokenRefreshBeforeExpireSeconds = 60;
    private int _webSocketReconnectIntervalMs = 5000;
    private int _webSocketMaxReconnectAttempts = 10;
    private ILoggerFactory? _loggerFactory;

    /// <summary>
    /// 设置AppId
    /// </summary>
    public QQBotClientBuilder WithAppId(string appId)
    {
        _appId = appId;
        return this;
    }

    /// <summary>
    /// 设置ClientSecret
    /// </summary>
    public QQBotClientBuilder WithClientSecret(string clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }

    /// <summary>
    /// 设置是否为沙箱环境
    /// </summary>
    public QQBotClientBuilder UseSandbox(bool isSandbox = true)
    {
        _isSandbox = isSandbox;
        return this;
    }

    /// <summary>
    /// 设置订阅的事件类型
    /// </summary>
    public QQBotClientBuilder WithIntents(Intents intents)
    {
        _intents = intents;
        return this;
    }

    /// <summary>
    /// 添加订阅的事件类型
    /// </summary>
    public QQBotClientBuilder AddIntents(Intents intents)
    {
        _intents |= intents;
        return this;
    }

    /// <summary>
    /// 设置Token提前刷新时间(秒)
    /// </summary>
    public QQBotClientBuilder WithTokenRefreshBeforeExpire(int seconds)
    {
        _tokenRefreshBeforeExpireSeconds = seconds;
        return this;
    }

    /// <summary>
    /// 设置WebSocket重连间隔(毫秒)
    /// </summary>
    public QQBotClientBuilder WithWebSocketReconnectInterval(int milliseconds)
    {
        _webSocketReconnectIntervalMs = milliseconds;
        return this;
    }

    /// <summary>
    /// 设置WebSocket最大重连次数
    /// </summary>
    public QQBotClientBuilder WithWebSocketMaxReconnectAttempts(int attempts)
    {
        _webSocketMaxReconnectAttempts = attempts;
        return this;
    }

    /// <summary>
    /// 设置日志工厂
    /// </summary>
    public QQBotClientBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    /// <summary>
    /// 构建客户端
    /// </summary>
    public QQBotClient Build()
    {
        if (string.IsNullOrEmpty(_appId))
            throw new InvalidOperationException("AppId is required");
        if (string.IsNullOrEmpty(_clientSecret))
            throw new InvalidOperationException("ClientSecret is required");

        var options = new QQBotClientOptions
        {
            AppId = _appId,
            ClientSecret = _clientSecret,
            IsSandbox = _isSandbox,
            Intents = _intents,
            TokenRefreshBeforeExpireSeconds = _tokenRefreshBeforeExpireSeconds,
            WebSocketReconnectIntervalMs = _webSocketReconnectIntervalMs,
            WebSocketMaxReconnectAttempts = _webSocketMaxReconnectAttempts
        };

        return new QQBotClient(options, _loggerFactory);
    }
}
