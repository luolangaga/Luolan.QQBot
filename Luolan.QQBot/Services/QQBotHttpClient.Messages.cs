using Luolan.QQBot.Models;

namespace Luolan.QQBot.Services;

public partial class QQBotHttpClient
{
    #region 单聊流式消息

    /// <summary>
    /// 流式发送单聊消息(分片)
    /// </summary>
    /// <param name="userOpenId">用户 OpenID</param>
    /// <param name="request">流式消息请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task<StreamMessageResponse> SendC2CStreamMessageAsync(
        string userOpenId,
        SendC2CStreamMessageRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<StreamMessageResponse>($"/v2/users/{userOpenId}/stream_messages", request, cancellationToken);

    /// <summary>
    /// 流式发送单聊文本消息(首片, 返回 stream_msg_id)
    /// </summary>
    public Task<StreamMessageResponse> SendC2CStreamTextAsync(
        string userOpenId,
        string content,
        int index = 0,
        string? msgId = null,
        string? streamMsgId = null,
        int msgSeq = 0,
        bool isFinal = false,
        CancellationToken cancellationToken = default)
        => SendC2CStreamMessageAsync(userOpenId, new SendC2CStreamMessageRequest
        {
            ContentType = "text",
            ContentRaw = content,
            Index = index,
            InputState = isFinal ? 10 : 1,
            MsgId = msgId,
            StreamMsgId = streamMsgId,
            MsgSeq = msgSeq
        }, cancellationToken);

    #endregion

    #region 消息撤回

    /// <summary>
    /// 撤回群聊消息
    /// </summary>
    public Task DeleteGroupMessageAsync(string groupOpenId, string messageId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/v2/groups/{groupOpenId}/messages/{messageId}", cancellationToken);

    /// <summary>
    /// 撤回单聊消息
    /// </summary>
    public Task DeleteC2CMessageAsync(string userOpenId, string messageId, CancellationToken cancellationToken = default)
        => DeleteAsync($"/v2/users/{userOpenId}/messages/{messageId}", cancellationToken);

    #endregion

    #region 富媒体分片上传

    /// <summary>
    /// 群聊富媒体预上传(分片上传第一步)
    /// </summary>
    public Task<UploadPrepareResponse> PrepareGroupMediaUploadAsync(
        string groupOpenId,
        UploadPrepareRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<UploadPrepareResponse>($"/v2/groups/{groupOpenId}/upload_prepare", request, cancellationToken);

    /// <summary>
    /// 群聊分片上传完成确认(分片上传第二步)
    /// </summary>
    public Task FinishGroupMediaUploadPartAsync(
        string groupOpenId,
        UploadPartFinishRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync($"/v2/groups/{groupOpenId}/upload_part_finish", request, cancellationToken);

    /// <summary>
    /// 单聊富媒体预上传(分片上传第一步)
    /// </summary>
    public Task<UploadPrepareResponse> PrepareC2CMediaUploadAsync(
        string userOpenId,
        UploadPrepareRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<UploadPrepareResponse>($"/v2/users/{userOpenId}/upload_prepare", request, cancellationToken);

    /// <summary>
    /// 单聊分片上传完成确认(分片上传第二步)
    /// </summary>
    public Task FinishC2CMediaUploadPartAsync(
        string userOpenId,
        UploadPartFinishRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync($"/v2/users/{userOpenId}/upload_part_finish", request, cancellationToken);

    #endregion

    #region 输入状态

    /// <summary>
    /// 发送输入中状态(单聊, msg_type=6)
    /// </summary>
    public Task<SendGroupMessageResponse> SendC2CInputNotifyAsync(
        string userOpenId,
        string? msgId = null,
        int inputSecond = 60,
        int msgSeq = 1,
        CancellationToken cancellationToken = default)
        => SendC2CMessageAsync(userOpenId, new SendGroupMessageRequest
        {
            MsgType = 6,
            MsgId = msgId,
            MsgSeq = msgSeq,
            InputNotify = new InputNotify { InputSecond = inputSecond }
        }, cancellationToken);

    #endregion
}
