using System.Net.WebSockets;
using Luolan.QQBot.Controllers;
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
    private bool _started;
    private User? _webhookBotUser;

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
    /// 是否已就绪(WebSocket 已连接或 Webhook 模式已启动)
    /// </summary>
    public bool IsConnected => _options.Mode == QQBotConnectionMode.Webhook
        ? _started
        : _webSocketClient.IsConnected;

    /// <summary>
    /// 当前机器人用户信息
    /// </summary>
    public User? CurrentUser => _webhookBotUser ?? _webSocketClient.Session.BotUser;

    /// <summary>
    /// 依赖注入容器(通过DI创建时可用)
    /// </summary>
    internal IServiceProvider? Services { get; private set; }

    /// <summary>
    /// 控制器管理器(启用控制器模式后可用)
    /// </summary>
    internal ControllerManager? ControllerManager { get; set; }

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
    /// 群消息事件(全量模式)
    /// </summary>
    public event Func<GroupMessageCreateEvent, Task>? OnGroupMessageCreate
    {
        add => _eventDispatcher.OnGroupMessageCreate += value;
        remove => _eventDispatcher.OnGroupMessageCreate -= value;
    }

    /// <summary>
    /// 群成员加入事件
    /// </summary>
    public event Func<GroupMemberAddEvent, Task>? OnGroupMemberAdd
    {
        add => _eventDispatcher.OnGroupMemberAdd += value;
        remove => _eventDispatcher.OnGroupMemberAdd -= value;
    }

    /// <summary>
    /// 群成员退出事件
    /// </summary>
    public event Func<GroupMemberRemoveEvent, Task>? OnGroupMemberRemove
    {
        add => _eventDispatcher.OnGroupMemberRemove += value;
        remove => _eventDispatcher.OnGroupMemberRemove -= value;
    }

    /// <summary>
    /// 用户申请加群事件
    /// </summary>
    public event Func<GroupJoinRequestEvent, Task>? OnGroupJoinRequest
    {
        add => _eventDispatcher.OnGroupJoinRequest += value;
        remove => _eventDispatcher.OnGroupJoinRequest -= value;
    }

    /// <summary>
    /// 订阅消息授权状态变更事件
    /// </summary>
    public event Func<SubscribeMessageStatusEvent, Task>? OnSubscribeMessageStatus
    {
        add => _eventDispatcher.OnSubscribeMessageStatus += value;
        remove => _eventDispatcher.OnSubscribeMessageStatus -= value;
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
        ILogger<QQBotClient>? logger = null,
        IServiceProvider? services = null)
    {
        _options = options;
        _httpClient = httpClient;
        _tokenManager = tokenManager;
        _apiClient = apiClient;
        _webSocketClient = webSocketClient;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        Services = services;
    }

    /// <summary>
    /// 启动机器人。WebSocket 模式建立长连接，Webhook 模式只做就绪初始化。
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_started) return;
        _started = true;

        _logger?.LogInformation("正在启动QQ机器人(模式: {Mode})...", _options.Mode);

        if (_options.Mode == QQBotConnectionMode.Webhook)
        {
            // Webhook 模式由平台主动推送事件，这里不需要建立长连接。
            // 为了保持与 WebSocket 模式一致的行为，尝试拉取机器人信息并触发 OnReady。
            await RaiseWebhookReadyAsync(cancellationToken);
            return;
        }

        await _webSocketClient.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 停止机器人
    /// </summary>
    public async Task StopAsync()
    {
        _logger?.LogInformation("正在停止QQ机器人...");
        _started = false;

        if (_options.Mode == QQBotConnectionMode.Webhook)
            return;

        await _webSocketClient.StopAsync();
    }

    private async Task RaiseWebhookReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));

            var user = await _apiClient.GetCurrentUserAsync(timeout.Token);
            _webhookBotUser = user;
            await _eventDispatcher.RaiseReadyAsync(user);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Webhook 模式拉取机器人信息失败，OnReady 不会触发。事件仍会正常分发。");
        }
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

    /// <summary>
    /// 发送群 Markdown 消息(可带键盘)
    /// </summary>
    public Task<SendGroupMessageResponse> SendGroupMarkdownAsync(
        string groupOpenId,
        MessageMarkdown markdown,
        MessageKeyboard? keyboard = null,
        string? msgId = null,
        int msgSeq = 0)
        => _apiClient.SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
        {
            MsgType = 2,
            Markdown = markdown,
            Keyboard = keyboard,
            MsgId = msgId,
            MsgSeq = msgSeq
        });

    /// <summary>
    /// 发送C2C Markdown 消息(可带键盘)
    /// </summary>
    public Task<SendGroupMessageResponse> SendC2CMarkdownAsync(
        string openId,
        MessageMarkdown markdown,
        MessageKeyboard? keyboard = null,
        string? msgId = null,
        int msgSeq = 0)
        => _apiClient.SendC2CMessageAsync(openId, new SendGroupMessageRequest
        {
            MsgType = 2,
            Markdown = markdown,
            Keyboard = keyboard,
            MsgId = msgId,
            MsgSeq = msgSeq
        });

    /// <summary>
    /// 上传并发送群富媒体消息
    /// </summary>
    /// <param name="groupOpenId">群 OpenID</param>
    /// <param name="url">文件URL</param>
    /// <param name="fileType">1=图片 2=视频 3=语音 4=文件</param>
    /// <param name="msgId">被动回复消息ID</param>
    /// <param name="msgSeq">回复序号</param>
    public async Task<SendGroupMessageResponse?> SendGroupMediaAsync(
        string groupOpenId,
        string url,
        int fileType = 1,
        string? msgId = null,
        int msgSeq = 1)
    {
        var upload = await _apiClient.UploadGroupMediaAsync(groupOpenId, new UploadMediaRequest
        {
            FileType = fileType,
            Url = url
        });

        if (upload?.FileInfo == null)
            return null;

        return await _apiClient.SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
        {
            MsgType = 7,
            Media = new MediaInfo { FileInfo = upload.FileInfo },
            MsgId = msgId,
            MsgSeq = msgSeq
        });
    }

    /// <summary>
    /// 上传并发送C2C富媒体消息
    /// </summary>
    public async Task<SendGroupMessageResponse?> SendC2CMediaAsync(
        string openId,
        string url,
        int fileType = 1,
        string? msgId = null,
        int msgSeq = 1)
    {
        var upload = await _apiClient.UploadC2CMediaAsync(openId, new UploadMediaRequest
        {
            FileType = fileType,
            Url = url
        });

        if (upload?.FileInfo == null)
            return null;

        return await _apiClient.SendC2CMessageAsync(openId, new SendGroupMessageRequest
        {
            MsgType = 7,
            Media = new MediaInfo { FileInfo = upload.FileInfo },
            MsgId = msgId,
            MsgSeq = msgSeq
        });
    }

    /// <summary>
    /// 流式发送C2C消息(首片)
    /// </summary>
    public Task<StreamMessageResponse> SendC2CStreamMessageAsync(
        string openId,
        SendC2CStreamMessageRequest request)
        => _apiClient.SendC2CStreamMessageAsync(openId, request);

    /// <summary>
    /// 撤回群聊消息
    /// </summary>
    public Task RevokeGroupMessageAsync(string groupOpenId, string messageId)
        => _apiClient.DeleteGroupMessageAsync(groupOpenId, messageId);

    /// <summary>
    /// 撤回单聊消息
    /// </summary>
    public Task RevokeC2CMessageAsync(string openId, string messageId)
        => _apiClient.DeleteC2CMessageAsync(openId, messageId);

    #endregion

    #region 群管理快捷方法

    /// <summary>
    /// 获取群基本信息
    /// </summary>
    public Task<GroupInfo> GetGroupInfoAsync(string groupOpenId)
        => _apiClient.GetGroupInfoAsync(groupOpenId);

    /// <summary>
    /// 获取机器人群内状态
    /// </summary>
    public Task<GroupBotState> GetGroupBotStateAsync(string groupOpenId)
        => _apiClient.GetGroupBotStateAsync(groupOpenId);

    /// <summary>
    /// 获取群成员列表
    /// </summary>
    public Task<GroupMemberListResponse> GetGroupMembersAsync(string groupOpenId, string? cursor = null)
        => _apiClient.GetGroupMembersAsync(groupOpenId, cursor);

    /// <summary>
    /// 获取群成员信息
    /// </summary>
    public Task<GroupMember> GetGroupMemberAsync(string groupOpenId, string memberOpenId)
        => _apiClient.GetGroupMemberAsync(groupOpenId, memberOpenId);

    /// <summary>
    /// 审批入群申请
    /// </summary>
    public Task ApproveGroupJoinRequestAsync(string groupOpenId, string memberOpenId, ApproveJoinRequestRequest request)
        => _apiClient.ApproveGroupJoinRequestAsync(groupOpenId, memberOpenId, request);

    /// <summary>
    /// 设置群成员禁言
    /// </summary>
    public Task SetGroupMemberMuteAsync(string groupOpenId, SetGroupMemberMuteRequest request)
        => _apiClient.SetGroupMemberMuteAsync(groupOpenId, request);

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
    private QQBotConnectionMode _mode = QQBotConnectionMode.WebSocket;
    private string _webhookPath = "/api/qqbot/webhook";
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
    /// 使用 Webhook 模式(HTTP 回调)
    /// </summary>
    public QQBotClientBuilder UseWebhook(string webhookPath = "/api/qqbot/webhook")
    {
        _mode = QQBotConnectionMode.Webhook;
        _webhookPath = webhookPath;
        return this;
    }

    /// <summary>
    /// 使用 WebSocket 模式(默认)
    /// </summary>
    public QQBotClientBuilder UseWebSocket()
    {
        _mode = QQBotConnectionMode.WebSocket;
        return this;
    }

    /// <summary>
    /// 设置事件传输模式
    /// </summary>
    public QQBotClientBuilder WithMode(QQBotConnectionMode mode)
    {
        _mode = mode;
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
            Mode = _mode,
            WebhookPath = _webhookPath,
            TokenRefreshBeforeExpireSeconds = _tokenRefreshBeforeExpireSeconds,
            WebSocketReconnectIntervalMs = _webSocketReconnectIntervalMs,
            WebSocketMaxReconnectAttempts = _webSocketMaxReconnectAttempts
        };

        return new QQBotClient(options, _loggerFactory);
    }
}
