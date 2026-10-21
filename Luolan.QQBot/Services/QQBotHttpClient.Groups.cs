using Luolan.QQBot.Models;

namespace Luolan.QQBot.Services;

public partial class QQBotHttpClient
{
    #region 群信息

    /// <summary>
    /// 获取群基本信息
    /// </summary>
    public Task<GroupInfo> GetGroupInfoAsync(string groupOpenId, CancellationToken cancellationToken = default)
        => GetAsync<GroupInfo>($"/v2/groups/{groupOpenId}/info", cancellationToken);

    /// <summary>
    /// 获取机器人群内状态
    /// </summary>
    public Task<GroupBotState> GetGroupBotStateAsync(string groupOpenId, CancellationToken cancellationToken = default)
        => GetAsync<GroupBotState>($"/v2/groups/{groupOpenId}/bot_state", cancellationToken);

    #endregion

    #region 群成员

    /// <summary>
    /// 获取群成员列表(每次最多 30 条)
    /// </summary>
    public Task<GroupMemberListResponse> GetGroupMembersAsync(
        string groupOpenId,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrEmpty(cursor) ? "" : $"?cursor={Uri.EscapeDataString(cursor)}";
        return GetAsync<GroupMemberListResponse>($"/v2/groups/{groupOpenId}/members{query}", cancellationToken);
    }

    /// <summary>
    /// 获取群成员信息
    /// </summary>
    public Task<GroupMember> GetGroupMemberAsync(
        string groupOpenId,
        string memberOpenId,
        CancellationToken cancellationToken = default)
        => GetAsync<GroupMember>($"/v2/groups/{groupOpenId}/members/{memberOpenId}", cancellationToken);

    /// <summary>
    /// 批量移除群成员(单次最多 20 个)
    /// </summary>
    public Task<BatchRemoveMembersResponse> BatchRemoveGroupMembersAsync(
        string groupOpenId,
        BatchRemoveMembersRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<BatchRemoveMembersResponse>($"/v2/groups/{groupOpenId}/batch_remove_members", request, cancellationToken);

    #endregion

    #region 入群申请

    /// <summary>
    /// 拉取入群申请列表
    /// </summary>
    public Task<JoinRequestListResponse> GetGroupJoinRequestsAsync(
        string groupOpenId,
        string? cursor = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = $"?limit={limit}";
        if (!string.IsNullOrEmpty(cursor))
            query += $"&cursor={Uri.EscapeDataString(cursor)}";
        return GetAsync<JoinRequestListResponse>($"/v2/groups/{groupOpenId}/join_request_list{query}", cancellationToken);
    }

    /// <summary>
    /// 审批入群申请
    /// </summary>
    public Task ApproveGroupJoinRequestAsync(
        string groupOpenId,
        string memberOpenId,
        ApproveJoinRequestRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync($"/v2/groups/{groupOpenId}/approval_join_request/{memberOpenId}", request, cancellationToken);

    #endregion

    #region 群黑名单

    /// <summary>
    /// 查询群黑名单
    /// </summary>
    public Task<GroupBlacklistResponse> GetGroupBlacklistAsync(
        string groupOpenId,
        string? cursor = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = $"?limit={limit}";
        if (!string.IsNullOrEmpty(cursor))
            query += $"&cursor={Uri.EscapeDataString(cursor)}";
        return GetAsync<GroupBlacklistResponse>($"/v2/groups/{groupOpenId}/member_blacklist{query}", cancellationToken);
    }

    /// <summary>
    /// 操作群黑名单(add/del)
    /// </summary>
    public Task<GroupBlacklistOperationResponse> UpdateGroupBlacklistAsync(
        string groupOpenId,
        GroupBlacklistRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<GroupBlacklistOperationResponse>($"/v2/groups/{groupOpenId}/member_blacklist", request, cancellationToken);

    #endregion

    #region 群禁言

    /// <summary>
    /// 查询群禁言状态
    /// </summary>
    public Task<GroupRestrictChatSetting> GetGroupRestrictChatSettingAsync(
        string groupOpenId,
        CancellationToken cancellationToken = default)
        => GetAsync<GroupRestrictChatSetting>($"/v2/groups/{groupOpenId}/restrict_chat_setting", cancellationToken);

    /// <summary>
    /// 设置群成员禁言
    /// </summary>
    public Task SetGroupMemberMuteAsync(
        string groupOpenId,
        SetGroupMemberMuteRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync($"/v2/groups/{groupOpenId}/restrict_chat_setting", request, cancellationToken);

    #endregion

    #region 入群自动审批策略

    /// <summary>
    /// 查询入群自动审批策略列表
    /// </summary>
    public Task<JoinApprovalStrategyListResponse> GetJoinApprovalStrategiesAsync(
        string? cursor = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = $"?limit={limit}";
        if (!string.IsNullOrEmpty(cursor))
            query += $"&cursor={Uri.EscapeDataString(cursor)}";
        return GetAsync<JoinApprovalStrategyListResponse>($"/v2/groups/join_approval_strategy{query}", cancellationToken);
    }

    /// <summary>
    /// 创建入群自动审批策略
    /// </summary>
    public Task<JoinApprovalStrategyResponse> CreateJoinApprovalStrategyAsync(
        CreateJoinApprovalStrategyRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<JoinApprovalStrategyResponse>("/v2/groups/join_approval_strategy", request, cancellationToken);

    /// <summary>
    /// 修改入群自动审批策略
    /// </summary>
    public Task<UpdateJoinApprovalStrategyResponse> UpdateJoinApprovalStrategyAsync(
        string strategyId,
        UpdateJoinApprovalStrategyRequest request,
        CancellationToken cancellationToken = default)
        => PatchAsync<UpdateJoinApprovalStrategyResponse>($"/v2/groups/join_approval_strategy/{strategyId}", request, cancellationToken);

    /// <summary>
    /// 删除入群自动审批策略
    /// </summary>
    public Task DeleteJoinApprovalStrategyAsync(string strategyId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/v2/groups/join_approval_strategy/{strategyId}", cancellationToken);

    /// <summary>
    /// 执行入群自动审批策略(异步扫描, 约 10 分钟)
    /// </summary>
    public Task ExecuteJoinApprovalStrategyAsync(string strategyId, CancellationToken cancellationToken = default)
        => PostAsync($"/v2/groups/join_approval_strategy/{strategyId}/execute", null, cancellationToken);

    /// <summary>
    /// 修改策略白名单号码
    /// </summary>
    public Task<UpdateWhitelistUsersResponse> UpdateJoinApprovalWhitelistAsync(
        string strategyId,
        UpdateWhitelistUsersRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<UpdateWhitelistUsersResponse>(
            $"/v2/groups/join_approval_strategy/{strategyId}/whitelist_users",
            request,
            cancellationToken);

    #endregion
}
