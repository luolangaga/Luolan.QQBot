using Luolan.QQBot.Models;

namespace Luolan.QQBot.Services;

public partial class QQBotHttpClient
{
    #region 自定义菜单

    /// <summary>
    /// 查询全局自定义菜单(C2C 场景)
    /// </summary>
    public Task<MenuResponse> GetMenuAsync(CancellationToken cancellationToken = default)
        => GetAsync<MenuResponse>("/v2/menu", cancellationToken);

    /// <summary>
    /// 修改全局自定义菜单(C2C 场景)
    /// </summary>
    public Task<MenuResponse> UpdateMenuAsync(QQBotMenu menu, CancellationToken cancellationToken = default)
        => PutAsync<MenuResponse>("/v2/menu", new UpdateMenuRequest { Menu = menu }, cancellationToken);

    #endregion

    #region 指令面板

    /// <summary>
    /// 查询指令面板列表
    /// </summary>
    public Task<PanelListResponse> GetPanelsAsync(
        string scope,
        string? cursor = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = $"?scope={Uri.EscapeDataString(scope)}&limit={limit}";
        if (!string.IsNullOrEmpty(cursor))
            query += $"&cursor={Uri.EscapeDataString(cursor)}";
        return GetAsync<PanelListResponse>($"/v2/panels{query}", cancellationToken);
    }

    /// <summary>
    /// 查询指令面板详情
    /// </summary>
    public Task<PanelRecord> GetPanelAsync(string panelId, CancellationToken cancellationToken = default)
        => GetAsync<PanelRecord>($"/v2/panels/{panelId}", cancellationToken);

    /// <summary>
    /// 创建指令面板
    /// </summary>
    public Task<CreatePanelResponse> CreatePanelAsync(CreatePanelRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreatePanelResponse>("/v2/panels", request, cancellationToken);

    /// <summary>
    /// 修改指令面板
    /// </summary>
    public Task<PanelVersionResponse> UpdatePanelAsync(
        string panelId,
        Panel panel,
        CancellationToken cancellationToken = default)
        => PutAsync<PanelVersionResponse>($"/v2/panels/{panelId}", new UpdatePanelRequest { Panel = panel }, cancellationToken);

    /// <summary>
    /// 修改指令面板关联对象
    /// </summary>
    public Task UpdatePanelTargetAsync(
        string panelId,
        UpdatePanelTargetRequest request,
        CancellationToken cancellationToken = default)
        => PutAsync($"/v2/panels/{panelId}/target", request, cancellationToken);

    /// <summary>
    /// 删除指令面板
    /// </summary>
    public Task DeletePanelAsync(string panelId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/v2/panels/{panelId}", cancellationToken);

    #endregion

    #region 互动回调

    /// <summary>
    /// 回应互动事件(避免客户端一直 loading)
    /// </summary>
    /// <param name="interactionId">互动事件ID(取事件 d.id, 不带 INTERACTION_CREATE: 前缀)</param>
    /// <param name="code">回调结果: 0=成功 1=操作失败 2=操作频繁 3=重复操作 4=没有权限 5=仅管理员操作</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task RespondInteractionAsync(
        string interactionId,
        int code = 0,
        CancellationToken cancellationToken = default)
        => PutAsync($"/interactions/{interactionId}", new InteractionResponseRequest { Code = code }, cancellationToken);

    #endregion

    #region 分享链接

    /// <summary>
    /// 生成机器人分享链接(用于邀请用户添加机器人为好友)
    /// </summary>
    public Task<GenerateUrlLinkResponse> GenerateUrlLinkAsync(
        string? callbackData = null,
        CancellationToken cancellationToken = default)
        => PostAsync<GenerateUrlLinkResponse>(
            "/v2/generate_url_link",
            new GenerateUrlLinkRequest { CallbackData = callbackData },
            cancellationToken);

    #endregion
}
