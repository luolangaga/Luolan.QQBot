using System.Text.Json.Serialization;

namespace Luolan.QQBot.Models;

/// <summary>
/// 群基本信息
/// </summary>
public class GroupInfo
{
    /// <summary>
    /// 群 OpenID
    /// </summary>
    [JsonPropertyName("group_openid")]
    public string GroupOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 群名称
    /// </summary>
    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    /// <summary>
    /// 群简介
    /// </summary>
    [JsonPropertyName("group_finger_memo")]
    public string? GroupFingerMemo { get; set; }

    /// <summary>
    /// 群分类
    /// </summary>
    [JsonPropertyName("group_class_text")]
    public string? GroupClassText { get; set; }

    /// <summary>
    /// 群标签列表
    /// </summary>
    [JsonPropertyName("group_tags")]
    public List<string>? GroupTags { get; set; }

    /// <summary>
    /// 群成员人数
    /// </summary>
    [JsonPropertyName("group_member_num")]
    public int GroupMemberNum { get; set; }
}

/// <summary>
/// 机器人群内状态
/// </summary>
public class GroupBotState
{
    /// <summary>
    /// 机器人的 openid
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string? MemberOpenId { get; set; }

    /// <summary>
    /// 入群时间
    /// </summary>
    [JsonPropertyName("joined_at")]
    public string? JoinedAt { get; set; }

    /// <summary>
    /// 是否接收主动推送
    /// </summary>
    [JsonPropertyName("allow_proactive_msg")]
    public bool AllowProactiveMsg { get; set; }

    /// <summary>
    /// 群内接收消息的设置: all / only_mention / mention_and_context
    /// </summary>
    [JsonPropertyName("recv_msg_setting")]
    public string? RecvMsgSetting { get; set; }

    /// <summary>
    /// 群成员角色: member / owner / admin
    /// </summary>
    [JsonPropertyName("member_role")]
    public string? MemberRole { get; set; }
}

/// <summary>
/// 群成员
/// </summary>
public class GroupMember
{
    /// <summary>
    /// 成员 OpenID
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string MemberOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// 群成员角色: member / owner / admin
    /// </summary>
    [JsonPropertyName("member_role")]
    public string? MemberRole { get; set; }

    /// <summary>
    /// 是否为机器人
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; set; }

    /// <summary>
    /// 入群时间
    /// </summary>
    [JsonPropertyName("joined_at")]
    public string? JoinedAt { get; set; }

    /// <summary>
    /// 跨应用统一用户标识
    /// </summary>
    [JsonPropertyName("union_openid")]
    public string? UnionOpenId { get; set; }
}

/// <summary>
/// 群成员列表响应
/// </summary>
public class GroupMemberListResponse
{
    /// <summary>
    /// 成员列表
    /// </summary>
    [JsonPropertyName("members")]
    public List<GroupMember>? Members { get; set; }

    /// <summary>
    /// 下一页游标(空串表示已到末页)
    /// </summary>
    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// 群成员批量移除请求
/// </summary>
public class BatchRemoveMembersRequest
{
    /// <summary>
    /// 需要移除的成员 member_openid 列表(单次最多 20 个)
    /// </summary>
    [JsonPropertyName("member_openids")]
    public List<string> MemberOpenIds { get; set; } = new();

    /// <summary>
    /// 是否同时加入群黑名单
    /// </summary>
    [JsonPropertyName("add_to_member_blacklist")]
    public bool AddToMemberBlacklist { get; set; }
}

/// <summary>
/// 群成员批量移除响应
/// </summary>
public class BatchRemoveMembersResponse
{
    /// <summary>
    /// 成功时返回 success
    /// </summary>
    [JsonPropertyName("remove_members_result")]
    public string? RemoveMembersResult { get; set; }

    /// <summary>
    /// 拉黑失败的 openid
    /// </summary>
    [JsonPropertyName("add_to_member_blacklist_fail_openids")]
    public List<string>? AddToMemberBlacklistFailOpenIds { get; set; }
}

/// <summary>
/// 入群申请列表响应
/// </summary>
public class JoinRequestListResponse
{
    /// <summary>
    /// 入群申请列表
    /// </summary>
    [JsonPropertyName("list")]
    public List<JoinRequestInfo>? List { get; set; }

    /// <summary>
    /// 下一页游标(空串表示已到末页)
    /// </summary>
    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// 入群申请
/// </summary>
public class JoinRequestInfo
{
    /// <summary>
    /// 申请ID(需要在审批接口回传)
    /// </summary>
    [JsonPropertyName("join_request_id")]
    public string JoinRequestId { get; set; } = string.Empty;

    /// <summary>
    /// 安全提示语
    /// </summary>
    [JsonPropertyName("risk_tips")]
    public string? RiskTips { get; set; }

    /// <summary>
    /// 用户统一标识
    /// </summary>
    [JsonPropertyName("union_openid")]
    public string? UnionOpenId { get; set; }

    /// <summary>
    /// 申请人 openid
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string MemberOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 申请人昵称
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    [JsonPropertyName("apply_at")]
    public string? ApplyAt { get; set; }

    /// <summary>
    /// 申请来源: self_apply / invited
    /// </summary>
    [JsonPropertyName("apply_source")]
    public string? ApplySource { get; set; }

    /// <summary>
    /// 邀请人 openid(apply_source=invited 时有效)
    /// </summary>
    [JsonPropertyName("invited_by")]
    public string? InvitedBy { get; set; }

    /// <summary>
    /// 是否为机器人账号
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; set; }

    /// <summary>
    /// 用户入群验证方式
    /// </summary>
    [JsonPropertyName("verify_info")]
    public JoinVerifyInfo? VerifyInfo { get; set; }

    /// <summary>
    /// 自动审批通过的扩展信息
    /// </summary>
    [JsonPropertyName("auto_approved")]
    public AutoApprovedInfo? AutoApproved { get; set; }
}

/// <summary>
/// 入群验证信息
/// </summary>
public class JoinVerifyInfo
{
    /// <summary>
    /// 验证方式: verify_message / admin_review_qa
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>
    /// 验证消息内容
    /// </summary>
    [JsonPropertyName("verify_message")]
    public string? VerifyMessage { get; set; }

    /// <summary>
    /// 问答列表
    /// </summary>
    [JsonPropertyName("review_qa_list")]
    public List<ReviewQA>? ReviewQaList { get; set; }
}

/// <summary>
/// 入群问答
/// </summary>
public class ReviewQA
{
    /// <summary>
    /// 管理员设置的问题
    /// </summary>
    [JsonPropertyName("question")]
    public string? Question { get; set; }

    /// <summary>
    /// 申请人填写的答案
    /// </summary>
    [JsonPropertyName("answer")]
    public string? Answer { get; set; }
}

/// <summary>
/// 自动审批通过信息
/// </summary>
public class AutoApprovedInfo
{
    /// <summary>
    /// 自动审批通过的策略ID
    /// </summary>
    [JsonPropertyName("strategy_id")]
    public string? StrategyId { get; set; }
}

/// <summary>
/// 入群申请审批请求
/// </summary>
public class ApproveJoinRequestRequest
{
    /// <summary>
    /// 审批动作: approve / decline
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = "approve";

    /// <summary>
    /// 申请ID
    /// </summary>
    [JsonPropertyName("join_request_id")]
    public string? JoinRequestId { get; set; }

    /// <summary>
    /// 拒绝理由(op=decline 时可填)
    /// </summary>
    [JsonPropertyName("reject_reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RejectReason { get; set; }

    /// <summary>
    /// 是否同时加入群黑名单(op=decline 时可填)
    /// </summary>
    [JsonPropertyName("add_to_member_blacklist")]
    public bool AddToMemberBlacklist { get; set; }
}

/// <summary>
/// 群黑名单用户
/// </summary>
public class GroupBlacklistUser
{
    /// <summary>
    /// 用户统一标识
    /// </summary>
    [JsonPropertyName("union_openid")]
    public string? UnionOpenId { get; set; }

    /// <summary>
    /// 用户 openid
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string MemberOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// 拉黑时间
    /// </summary>
    [JsonPropertyName("banned_at")]
    public string? BannedAt { get; set; }

    /// <summary>
    /// 是否为机器人账号
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; set; }
}

/// <summary>
/// 群黑名单查询响应
/// </summary>
public class GroupBlacklistResponse
{
    /// <summary>
    /// 黑名单用户列表
    /// </summary>
    [JsonPropertyName("users")]
    public List<GroupBlacklistUser>? Users { get; set; }

    /// <summary>
    /// 下一页游标(空串表示已到末页)
    /// </summary>
    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// 群黑名单操作请求
/// </summary>
public class GroupBlacklistRequest
{
    /// <summary>
    /// 操作类型: add / del
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = "add";

    /// <summary>
    /// 目标成员 openid 列表(单次最多 20 个)
    /// </summary>
    [JsonPropertyName("member_openids")]
    public List<string> MemberOpenIds { get; set; } = new();
}

/// <summary>
/// 群黑名单操作响应
/// </summary>
public class GroupBlacklistOperationResponse
{
    /// <summary>
    /// 操作失败的 openid 列表
    /// </summary>
    [JsonPropertyName("fail_openids")]
    public List<string>? FailOpenIds { get; set; }
}

/// <summary>
/// 群禁言状态
/// </summary>
public class GroupRestrictChatSetting
{
    /// <summary>
    /// 群级禁言规则
    /// </summary>
    [JsonPropertyName("global_rule")]
    public GlobalMuteRule? GlobalRule { get; set; }

    /// <summary>
    /// 当前处于禁言中的用户列表
    /// </summary>
    [JsonPropertyName("members")]
    public List<GroupMemberMuteState>? Members { get; set; }
}

/// <summary>
/// 群级禁言规则
/// </summary>
public class GlobalMuteRule
{
    /// <summary>
    /// 全员禁言模式: none / always / schedule
    /// </summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    /// <summary>
    /// 定时禁言规则
    /// </summary>
    [JsonPropertyName("schedule_rules")]
    public List<MuteScheduleRule>? ScheduleRules { get; set; }

    /// <summary>
    /// 周期禁言规则
    /// </summary>
    [JsonPropertyName("recurring_rules")]
    public List<MuteRecurringRule>? RecurringRules { get; set; }
}

/// <summary>
/// 定时禁言规则
/// </summary>
public class MuteScheduleRule
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("task_id")]
    public string? TaskId { get; set; }

    /// <summary>
    /// 禁言开始时间
    /// </summary>
    [JsonPropertyName("start_at")]
    public string? StartAt { get; set; }

    /// <summary>
    /// 禁言结束时间
    /// </summary>
    [JsonPropertyName("end_at")]
    public string? EndAt { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

/// <summary>
/// 周期禁言规则
/// </summary>
public class MuteRecurringRule
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("task_id")]
    public string? TaskId { get; set; }

    /// <summary>
    /// 生效星期几列表(1=周一, 7=周日)
    /// </summary>
    [JsonPropertyName("weekdays")]
    public List<int>? Weekdays { get; set; }

    /// <summary>
    /// 时段开始时间 HH:mm
    /// </summary>
    [JsonPropertyName("start_time")]
    public string? StartTime { get; set; }

    /// <summary>
    /// 时段结束时间 HH:mm
    /// </summary>
    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

/// <summary>
/// 被禁言成员
/// </summary>
public class GroupMemberMuteState
{
    /// <summary>
    /// 被禁言成员的 openid
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string MemberOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 禁言到期时间
    /// </summary>
    [JsonPropertyName("mute_expire_at")]
    public string? MuteExpireAt { get; set; }

    /// <summary>
    /// 被禁言成员昵称
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// 用户统一标识
    /// </summary>
    [JsonPropertyName("union_openid")]
    public string? UnionOpenId { get; set; }
}

/// <summary>
/// 设置群成员禁言请求
/// </summary>
public class SetGroupMemberMuteRequest
{
    /// <summary>
    /// 用户禁言列表(单次最多 20 个)
    /// </summary>
    [JsonPropertyName("members")]
    public List<SetMemberMuteState> Members { get; set; } = new();
}

/// <summary>
/// 单个成员的禁言操作
/// </summary>
public class SetMemberMuteState
{
    /// <summary>
    /// 操作类型: add / update / del
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = "add";

    /// <summary>
    /// 被禁言成员的 openid
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string MemberOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 禁言到期时间(op=del 时可传空串立即解除)
    /// </summary>
    [JsonPropertyName("mute_expire_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MuteExpireAt { get; set; }
}

/// <summary>
/// 入群自动审批策略
/// </summary>
public class JoinApprovalStrategy
{
    /// <summary>
    /// 策略 ID
    /// </summary>
    [JsonPropertyName("strategy_id")]
    public string StrategyId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的群 openid 列表
    /// </summary>
    [JsonPropertyName("group_openids")]
    public List<string>? GroupOpenIds { get; set; }

    /// <summary>
    /// 关联的 QQ 群号列表
    /// </summary>
    [JsonPropertyName("group_ids")]
    public List<ulong>? GroupIds { get; set; }

    /// <summary>
    /// 白名单中的号码数量(估算)
    /// </summary>
    [JsonPropertyName("whitelist_user_count")]
    public int WhitelistUserCount { get; set; }

    /// <summary>
    /// 是否启用: on / off
    /// </summary>
    [JsonPropertyName("is_enable")]
    public string? IsEnable { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [JsonPropertyName("expire_at")]
    public string? ExpireAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// 最近更新时间
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// 策略备注
    /// </summary>
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
}

/// <summary>
/// 入群自动审批策略列表响应
/// </summary>
public class JoinApprovalStrategyListResponse
{
    /// <summary>
    /// 生效中的策略列表
    /// </summary>
    [JsonPropertyName("strategies")]
    public List<JoinApprovalStrategy>? Strategies { get; set; }

    /// <summary>
    /// 下一页游标
    /// </summary>
    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// 创建入群自动审批策略请求
/// </summary>
public class CreateJoinApprovalStrategyRequest
{
    /// <summary>
    /// 关联的群 openid 列表(与 GroupIds 二选一, 最多 100 个)
    /// </summary>
    [JsonPropertyName("group_openids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? GroupOpenIds { get; set; }

    /// <summary>
    /// 关联的 QQ 群号列表(与 GroupOpenIds 二选一, 最多 100 个)
    /// </summary>
    [JsonPropertyName("group_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ulong>? GroupIds { get; set; }

    /// <summary>
    /// 是否启用: on / off(默认 on)
    /// </summary>
    [JsonPropertyName("is_enable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IsEnable { get; set; }

    /// <summary>
    /// 过期时间(不传默认一年)
    /// </summary>
    [JsonPropertyName("expire_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExpireAt { get; set; }

    /// <summary>
    /// 策略备注
    /// </summary>
    [JsonPropertyName("remark")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Remark { get; set; }
}

/// <summary>
/// 入群自动审批策略操作响应
/// </summary>
public class JoinApprovalStrategyResponse
{
    [JsonPropertyName("strategy_id")]
    public string? StrategyId { get; set; }

    [JsonPropertyName("is_enable")]
    public string? IsEnable { get; set; }

    [JsonPropertyName("expire_at")]
    public string? ExpireAt { get; set; }
}

/// <summary>
/// 修改入群自动审批策略请求
/// </summary>
public class UpdateJoinApprovalStrategyRequest
{
    [JsonPropertyName("is_enable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IsEnable { get; set; }

    [JsonPropertyName("expire_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExpireAt { get; set; }

    [JsonPropertyName("group_action")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GroupAction? GroupAction { get; set; }

    [JsonPropertyName("remark")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Remark { get; set; }
}

/// <summary>
/// 关联群增删操作
/// </summary>
public class GroupAction
{
    /// <summary>
    /// 操作类型: add / del
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = "add";

    [JsonPropertyName("group_openids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? GroupOpenIds { get; set; }

    [JsonPropertyName("group_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ulong>? GroupIds { get; set; }
}

/// <summary>
/// 修改入群自动审批策略响应
/// </summary>
public class UpdateJoinApprovalStrategyResponse
{
    [JsonPropertyName("is_enable")]
    public string? IsEnable { get; set; }

    [JsonPropertyName("expire_at")]
    public string? ExpireAt { get; set; }
}

/// <summary>
/// 修改策略白名单请求
/// </summary>
public class UpdateWhitelistUsersRequest
{
    /// <summary>
    /// 操作类型: add / del
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = "add";

    /// <summary>
    /// QQ 号码列表(单次最多 10000 个)
    /// </summary>
    [JsonPropertyName("whitelist_users")]
    public List<string> WhitelistUsers { get; set; } = new();
}

/// <summary>
/// 修改策略白名单响应
/// </summary>
public class UpdateWhitelistUsersResponse
{
    [JsonPropertyName("strategy_id")]
    public string? StrategyId { get; set; }

    [JsonPropertyName("whitelist_user_count")]
    public int WhitelistUserCount { get; set; }

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }
}
