using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Luolan.QQBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luolan.QQBot.Services;

/// <summary>
/// Token管理服务 - 自动刷新Token
/// </summary>
public class TokenManager : IDisposable
{
    private readonly QQBotClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenManager>? _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    private CachedAccessToken? _cachedToken;
    private Timer? _refreshTimer;
    private bool _disposed;

    public TokenManager(QQBotClientOptions options, HttpClient httpClient, ILogger<TokenManager>? logger = null)
    {
        _options = options;
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// 获取有效的访问令牌
    /// </summary>
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedToken != null && !_cachedToken.NeedsRefresh(_options.TokenRefreshBeforeExpireSeconds))
            {
                return _cachedToken.AccessToken;
            }

            return await RefreshTokenInternalAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <summary>
    /// 获取鉴权头格式的Token
    /// </summary>
    public async Task<string> GetAuthorizationHeaderAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        return $"QQBot {token}";
    }

    /// <summary>
    /// 强制刷新Token
    /// </summary>
    public async Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            return await RefreshTokenInternalAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> RefreshTokenInternalAsync(CancellationToken cancellationToken)
    {
        _logger?.LogDebug("正在刷新访问令牌...");

        var requestBody = new
        {
            appId = _options.AppId,
            clientSecret = _options.ClientSecret
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(_options.AuthUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseJson, _jsonOptions);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("获取访问令牌失败: 响应为空");
        }

        _cachedToken = new CachedAccessToken
        {
            AccessToken = tokenResponse.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        };

        // 设置自动刷新定时器
        SetupRefreshTimer(tokenResponse.ExpiresIn);

        _logger?.LogInformation("访问令牌刷新成功, 过期时间: {ExpiresAt}", _cachedToken.ExpiresAt);

        return _cachedToken.AccessToken;
    }

    private void SetupRefreshTimer(int expiresInSeconds)
    {
        _refreshTimer?.Dispose();

        // 提前刷新
        var refreshInterval = TimeSpan.FromSeconds(
            Math.Max(1, expiresInSeconds - _options.TokenRefreshBeforeExpireSeconds)
        );

        _refreshTimer = new Timer(
            async _ =>
            {
                try
                {
                    await RefreshTokenAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "自动刷新Token失败");
                }
            },
            null,
            refreshInterval,
            Timeout.InfiniteTimeSpan
        );
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer?.Dispose();
        _tokenLock.Dispose();
    }
}

/// <summary>
/// QQ Bot HTTP API 客户端
/// </summary>
public class QQBotHttpClient : IDisposable
{
    private readonly QQBotClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;
    private readonly ILogger<QQBotHttpClient>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public QQBotHttpClient(
        QQBotClientOptions options,
        HttpClient httpClient,
        TokenManager tokenManager,
        ILogger<QQBotHttpClient>? logger = null)
    {
        _options = options;
        _httpClient = httpClient;
        _tokenManager = tokenManager;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region 基础请求方法

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method,
        string endpoint,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, $"{_options.ApiBaseUrl}{endpoint}");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "QQBot",
            await _tokenManager.GetAccessTokenAsync(cancellationToken)
        );

        if (body != null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, _jsonOptions),
                Encoding.UTF8,
                "application/json"
            );
        }

        return request;
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var error = JsonSerializer.Deserialize<ApiError>(errorContent, _jsonOptions);
            _logger?.LogError("API错误: {Code} - {Message}", error?.Code, error?.Message);
            throw new QQBotApiException(error?.Code ?? (int)response.StatusCode, error?.Message ?? errorContent, error?.TraceId);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return default!;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
    }

    private async Task SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var error = JsonSerializer.Deserialize<ApiError>(errorContent, _jsonOptions);
            _logger?.LogError("API错误: {Code} - {Message}", error?.Code, error?.Message);
            throw new QQBotApiException(error?.Code ?? (int)response.StatusCode, error?.Message ?? errorContent, error?.TraceId);
        }
    }

    /// <summary>
    /// GET请求
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, endpoint, null, cancellationToken);
        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// POST请求
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Post, endpoint, body, cancellationToken);
        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// POST请求(无返回)
    /// </summary>
    public async Task PostAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Post, endpoint, body, cancellationToken);
        await SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// PUT请求
    /// </summary>
    public async Task<T> PutAsync<T>(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Put, endpoint, body, cancellationToken);
        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// PUT请求(无返回)
    /// </summary>
    public async Task PutAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Put, endpoint, body, cancellationToken);
        await SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// PATCH请求
    /// </summary>
    public async Task<T> PatchAsync<T>(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Patch, endpoint, body, cancellationToken);
        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// DELETE请求
    /// </summary>
    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Delete, endpoint, null, cancellationToken);
        await SendAsync(request, cancellationToken);
    }

    #endregion

    #region 用户API

    /// <summary>
    /// 获取当前用户(机器人)信息
    /// </summary>
    public Task<BotInfo> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        => GetAsync<BotInfo>("/users/@me", cancellationToken);

    /// <summary>
    /// 获取当前用户(机器人)加入的频道列表
    /// </summary>
    public Task<List<Guild>> GetCurrentUserGuildsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<Guild>>("/users/@me/guilds", cancellationToken);

    #endregion

    #region 频道API

    /// <summary>
    /// 获取频道详情
    /// </summary>
    public Task<Guild> GetGuildAsync(string guildId, CancellationToken cancellationToken = default)
        => GetAsync<Guild>($"/guilds/{guildId}", cancellationToken);

    #endregion

    #region 子频道API

    /// <summary>
    /// 获取子频道列表
    /// </summary>
    public Task<List<Channel>> GetChannelsAsync(string guildId, CancellationToken cancellationToken = default)
        => GetAsync<List<Channel>>($"/guilds/{guildId}/channels", cancellationToken);

    /// <summary>
    /// 获取子频道详情
    /// </summary>
    public Task<Channel> GetChannelAsync(string channelId, CancellationToken cancellationToken = default)
        => GetAsync<Channel>($"/channels/{channelId}", cancellationToken);

    /// <summary>
    /// 创建子频道
    /// </summary>
    public Task<Channel> CreateChannelAsync(string guildId, object channelInfo, CancellationToken cancellationToken = default)
        => PostAsync<Channel>($"/guilds/{guildId}/channels", channelInfo, cancellationToken);

    /// <summary>
    /// 修改子频道
    /// </summary>
    public Task<Channel> ModifyChannelAsync(string channelId, object channelInfo, CancellationToken cancellationToken = default)
        => PatchAsync<Channel>($"/channels/{channelId}", channelInfo, cancellationToken);

    /// <summary>
    /// 删除子频道
    /// </summary>
    public Task DeleteChannelAsync(string channelId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/channels/{channelId}", cancellationToken);

    #endregion

    #region 成员API

    /// <summary>
    /// 获取频道成员列表
    /// </summary>
    public Task<List<Member>> GetGuildMembersAsync(string guildId, string? after = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        var query = $"?limit={limit}";
        if (!string.IsNullOrEmpty(after)) query += $"&after={after}";
        return GetAsync<List<Member>>($"/guilds/{guildId}/members{query}", cancellationToken);
    }

    /// <summary>
    /// 获取频道成员详情
    /// </summary>
    public Task<Member> GetGuildMemberAsync(string guildId, string userId, CancellationToken cancellationToken = default)
        => GetAsync<Member>($"/guilds/{guildId}/members/{userId}", cancellationToken);

    /// <summary>
    /// 删除频道成员
    /// </summary>
    public Task DeleteGuildMemberAsync(string guildId, string userId, bool addBlacklist = false, int deleteHistoryMsgDays = 0, CancellationToken cancellationToken = default)
        => DeleteAsync($"/guilds/{guildId}/members/{userId}?add_blacklist={addBlacklist}&delete_history_msg_days={deleteHistoryMsgDays}", cancellationToken);

    #endregion

    #region 角色API

    /// <summary>
    /// 获取频道身份组列表
    /// </summary>
    public Task<GetGuildRolesResponse> GetGuildRolesAsync(string guildId, CancellationToken cancellationToken = default)
        => GetAsync<GetGuildRolesResponse>($"/guilds/{guildId}/roles", cancellationToken);

    /// <summary>
    /// 创建频道身份组
    /// </summary>
    public Task<CreateRoleResponse> CreateGuildRoleAsync(string guildId, CreateRoleRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateRoleResponse>($"/guilds/{guildId}/roles", request, cancellationToken);

    /// <summary>
    /// 修改频道身份组
    /// </summary>
    public Task<CreateRoleResponse> ModifyGuildRoleAsync(string guildId, string roleId, CreateRoleRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<CreateRoleResponse>($"/guilds/{guildId}/roles/{roleId}", request, cancellationToken);

    /// <summary>
    /// 删除频道身份组
    /// </summary>
    public Task DeleteGuildRoleAsync(string guildId, string roleId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/guilds/{guildId}/roles/{roleId}", cancellationToken);

    /// <summary>
    /// 增加频道身份组成员
    /// </summary>
    public Task AddMemberToRoleAsync(string guildId, string userId, string roleId, string? channelId = null, CancellationToken cancellationToken = default)
    {
        var body = channelId != null ? new { channel = new { id = channelId } } : null;
        return PutAsync($"/guilds/{guildId}/members/{userId}/roles/{roleId}", body, cancellationToken);
    }

    /// <summary>
    /// 删除频道身份组成员
    /// </summary>
    public Task RemoveMemberFromRoleAsync(string guildId, string userId, string roleId, string? channelId = null, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/guilds/{guildId}/members/{userId}/roles/{roleId}";
        if (channelId != null) endpoint += $"?channel_id={channelId}";
        return DeleteAsync(endpoint, cancellationToken);
    }

    #endregion

    #region 子频道权限API

    /// <summary>
    /// 获取子频道用户权限
    /// </summary>
    public Task<ChannelPermissions> GetChannelPermissionsAsync(string channelId, string userId, CancellationToken cancellationToken = default)
        => GetAsync<ChannelPermissions>($"/channels/{channelId}/members/{userId}/permissions", cancellationToken);

    /// <summary>
    /// 修改子频道用户权限
    /// </summary>
    public Task ModifyChannelPermissionsAsync(string channelId, string userId, UpdateChannelPermissionsRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"/channels/{channelId}/members/{userId}/permissions", request, cancellationToken);

    /// <summary>
    /// 获取子频道角色权限
    /// </summary>
    public Task<ChannelPermissions> GetChannelRolePermissionsAsync(string channelId, string roleId, CancellationToken cancellationToken = default)
        => GetAsync<ChannelPermissions>($"/channels/{channelId}/roles/{roleId}/permissions", cancellationToken);

    /// <summary>
    /// 修改子频道角色权限
    /// </summary>
    public Task ModifyChannelRolePermissionsAsync(string channelId, string roleId, UpdateChannelPermissionsRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"/channels/{channelId}/roles/{roleId}/permissions", request, cancellationToken);

    #endregion

    #region 消息API

    /// <summary>
    /// 获取指定消息
    /// </summary>
    public Task<Message> GetMessageAsync(string channelId, string messageId, CancellationToken cancellationToken = default)
        => GetAsync<Message>($"/channels/{channelId}/messages/{messageId}", cancellationToken);

    /// <summary>
    /// 发送消息到子频道
    /// </summary>
    public Task<Message> SendMessageAsync(string channelId, SendMessageRequest request, CancellationToken cancellationToken = default)
        => PostAsync<Message>($"/channels/{channelId}/messages", request, cancellationToken);

    /// <summary>
    /// 发送文本消息到子频道
    /// </summary>
    public Task<Message> SendTextMessageAsync(string channelId, string content, string? msgId = null, CancellationToken cancellationToken = default)
        => SendMessageAsync(channelId, new SendMessageRequest { Content = content, MsgId = msgId }, cancellationToken);

    /// <summary>
    /// 撤回消息
    /// </summary>
    public Task DeleteMessageAsync(string channelId, string messageId, bool hideTip = false, CancellationToken cancellationToken = default)
        => DeleteAsync($"/channels/{channelId}/messages/{messageId}?hidetip={hideTip}", cancellationToken);

    #endregion

    #region 私信API

    /// <summary>
    /// 创建私信会话
    /// </summary>
    public Task<DirectMessageSession> CreateDmsAsync(CreateDmsRequest request, CancellationToken cancellationToken = default)
        => PostAsync<DirectMessageSession>("/users/@me/dms", request, cancellationToken);

    /// <summary>
    /// 发送私信
    /// </summary>
    public Task<Message> SendDmsAsync(string guildId, SendMessageRequest request, CancellationToken cancellationToken = default)
        => PostAsync<Message>($"/dms/{guildId}/messages", request, cancellationToken);

    /// <summary>
    /// 撤回私信
    /// </summary>
    public Task DeleteDmsAsync(string guildId, string messageId, bool hideTip = false, CancellationToken cancellationToken = default)
        => DeleteAsync($"/dms/{guildId}/messages/{messageId}?hidetip={hideTip}", cancellationToken);

    #endregion

    #region 群聊消息API

    /// <summary>
    /// 发送群消息
    /// </summary>
    public Task<SendGroupMessageResponse> SendGroupMessageAsync(string groupOpenId, SendGroupMessageRequest request, CancellationToken cancellationToken = default)
        => PostAsync<SendGroupMessageResponse>($"/v2/groups/{groupOpenId}/messages", request, cancellationToken);

    /// <summary>
    /// 发送群文本消息
    /// </summary>
    public Task<SendGroupMessageResponse> SendGroupTextMessageAsync(string groupOpenId, string content, string? msgId = null, int msgSeq = 0, CancellationToken cancellationToken = default)
        => SendGroupMessageAsync(groupOpenId, new SendGroupMessageRequest
        {
            Content = content,
            MsgType = 0,
            MsgId = msgId,
            MsgSeq = msgSeq
        }, cancellationToken);

    /// <summary>
    /// 上传群文件
    /// </summary>
    public Task<UploadMediaResponse> UploadGroupMediaAsync(string groupOpenId, UploadMediaRequest request, CancellationToken cancellationToken = default)
        => PostAsync<UploadMediaResponse>($"/v2/groups/{groupOpenId}/files", request, cancellationToken);

    #endregion

    #region C2C消息API

    /// <summary>
    /// 发送C2C消息
    /// </summary>
    public Task<SendGroupMessageResponse> SendC2CMessageAsync(string openId, SendGroupMessageRequest request, CancellationToken cancellationToken = default)
        => PostAsync<SendGroupMessageResponse>($"/v2/users/{openId}/messages", request, cancellationToken);

    /// <summary>
    /// 发送C2C文本消息
    /// </summary>
    public Task<SendGroupMessageResponse> SendC2CTextMessageAsync(string openId, string content, string? msgId = null, int msgSeq = 0, CancellationToken cancellationToken = default)
        => SendC2CMessageAsync(openId, new SendGroupMessageRequest
        {
            Content = content,
            MsgType = 0,
            MsgId = msgId,
            MsgSeq = msgSeq
        }, cancellationToken);

    /// <summary>
    /// 上传C2C文件
    /// </summary>
    public Task<UploadMediaResponse> UploadC2CMediaAsync(string openId, UploadMediaRequest request, CancellationToken cancellationToken = default)
        => PostAsync<UploadMediaResponse>($"/v2/users/{openId}/files", request, cancellationToken);

    #endregion

    #region 表情表态API

    /// <summary>
    /// 对消息进行表情表态
    /// </summary>
    public Task AddReactionAsync(string channelId, string messageId, string emojiType, string emojiId, CancellationToken cancellationToken = default)
        => PutAsync($"/channels/{channelId}/messages/{messageId}/reactions/{emojiType}/{emojiId}", null, cancellationToken);

    /// <summary>
    /// 删除表情表态
    /// </summary>
    public Task DeleteReactionAsync(string channelId, string messageId, string emojiType, string emojiId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/channels/{channelId}/messages/{messageId}/reactions/{emojiType}/{emojiId}", cancellationToken);

    #endregion

    #region 精华消息API

    /// <summary>
    /// 添加精华消息
    /// </summary>
    public Task<PinsMessage> AddPinsMessageAsync(string channelId, string messageId, CancellationToken cancellationToken = default)
        => PutAsync<PinsMessage>($"/channels/{channelId}/pins/{messageId}", null, cancellationToken);

    /// <summary>
    /// 删除精华消息
    /// </summary>
    public Task DeletePinsMessageAsync(string channelId, string messageId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/channels/{channelId}/pins/{messageId}", cancellationToken);

    /// <summary>
    /// 获取精华消息
    /// </summary>
    public Task<PinsMessage> GetPinsMessageAsync(string channelId, CancellationToken cancellationToken = default)
        => GetAsync<PinsMessage>($"/channels/{channelId}/pins", cancellationToken);

    #endregion

    #region 日程API

    /// <summary>
    /// 获取日程列表
    /// </summary>
    public Task<List<Schedule>> GetSchedulesAsync(string channelId, string? since = null, CancellationToken cancellationToken = default)
    {
        var query = since != null ? $"?since={since}" : "";
        return GetAsync<List<Schedule>>($"/channels/{channelId}/schedules{query}", cancellationToken);
    }

    /// <summary>
    /// 获取日程详情
    /// </summary>
    public Task<Schedule> GetScheduleAsync(string channelId, string scheduleId, CancellationToken cancellationToken = default)
        => GetAsync<Schedule>($"/channels/{channelId}/schedules/{scheduleId}", cancellationToken);

    /// <summary>
    /// 创建日程
    /// </summary>
    public Task<Schedule> CreateScheduleAsync(string channelId, Schedule schedule, CancellationToken cancellationToken = default)
        => PostAsync<Schedule>($"/channels/{channelId}/schedules", new { schedule }, cancellationToken);

    /// <summary>
    /// 修改日程
    /// </summary>
    public Task<Schedule> ModifyScheduleAsync(string channelId, string scheduleId, Schedule schedule, CancellationToken cancellationToken = default)
        => PatchAsync<Schedule>($"/channels/{channelId}/schedules/{scheduleId}", new { schedule }, cancellationToken);

    /// <summary>
    /// 删除日程
    /// </summary>
    public Task DeleteScheduleAsync(string channelId, string scheduleId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/channels/{channelId}/schedules/{scheduleId}", cancellationToken);

    #endregion

    #region 禁言API

    /// <summary>
    /// 频道全员禁言
    /// </summary>
    public Task MuteGuildAsync(string guildId, int muteSeconds = 0, string? muteEndTimestamp = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>();
        if (muteEndTimestamp != null)
            body["mute_end_timestamp"] = muteEndTimestamp;
        else
            body["mute_seconds"] = muteSeconds.ToString();

        return PatchAsync<object>($"/guilds/{guildId}/mute", body, cancellationToken);
    }

    /// <summary>
    /// 频道成员禁言
    /// </summary>
    public Task MuteMemberAsync(string guildId, string userId, int muteSeconds = 0, string? muteEndTimestamp = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>();
        if (muteEndTimestamp != null)
            body["mute_end_timestamp"] = muteEndTimestamp;
        else
            body["mute_seconds"] = muteSeconds.ToString();

        return PatchAsync<object>($"/guilds/{guildId}/members/{userId}/mute", body, cancellationToken);
    }

    /// <summary>
    /// 批量频道成员禁言
    /// </summary>
    public Task MuteMembersAsync(string guildId, IEnumerable<string> userIds, int muteSeconds = 0, string? muteEndTimestamp = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["user_ids"] = userIds.ToArray()
        };
        if (muteEndTimestamp != null)
            body["mute_end_timestamp"] = muteEndTimestamp;
        else
            body["mute_seconds"] = muteSeconds.ToString();

        return PatchAsync<object>($"/guilds/{guildId}/mute", body, cancellationToken);
    }

    #endregion

    #region 公告API

    /// <summary>
    /// 创建频道公告
    /// </summary>
    public Task<Announces> CreateAnnouncesAsync(string guildId, string channelId, string messageId, CancellationToken cancellationToken = default)
        => PostAsync<Announces>($"/guilds/{guildId}/announces", new { channel_id = channelId, message_id = messageId }, cancellationToken);

    /// <summary>
    /// 删除频道公告
    /// </summary>
    public Task DeleteAnnouncesAsync(string guildId, string messageId = "all", CancellationToken cancellationToken = default)
        => DeleteAsync($"/guilds/{guildId}/announces/{messageId}", cancellationToken);

    #endregion

    #region WebSocket网关API

    /// <summary>
    /// 获取WebSocket网关地址
    /// </summary>
    public Task<GatewayInfo> GetGatewayAsync(CancellationToken cancellationToken = default)
        => GetAsync<GatewayInfo>("/gateway", cancellationToken);

    /// <summary>
    /// 获取带分片信息的WebSocket网关地址
    /// </summary>
    public Task<GatewayBotInfo> GetGatewayBotAsync(CancellationToken cancellationToken = default)
        => GetAsync<GatewayBotInfo>("/gateway/bot", cancellationToken);

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // HttpClient由外部管理,不在这里释放
    }
}

/// <summary>
/// QQ Bot API异常
/// </summary>
public class QQBotApiException : Exception
{
    public int Code { get; }
    public string? TraceId { get; }

    public QQBotApiException(int code, string message, string? traceId = null)
        : base($"[{code}] {message}")
    {
        Code = code;
        TraceId = traceId;
    }
}
