using Luolan.QQBot.AspNetExample.Models;
using Microsoft.AspNetCore.Mvc;

namespace Luolan.QQBot.AspNetExample.Controllers;

[ApiController]
[Route("api/bot")]
public class BotApiController : ControllerBase
{
    private readonly QQBotClient _bot;
    private readonly ILogger<BotApiController> _logger;

    public BotApiController(QQBotClient bot, ILogger<BotApiController> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    /// <summary>
    /// 机器人连接状态
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            _bot.IsConnected,
            _bot.CurrentUser,
            ApiBaseUrl = _bot.Options.ApiBaseUrl
        });
    }

    /// <summary>
    /// 发送消息(targetType: group / c2c / channel)
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (string.IsNullOrEmpty(dto.TargetId) || string.IsNullOrEmpty(dto.Content))
        {
            return BadRequest("TargetId and Content cannot be empty");
        }

        try
        {
            switch (dto.TargetType.ToLowerInvariant())
            {
                case "c2c":
                    var c2c = await _bot.SendC2CMessageAsync(dto.TargetId, dto.Content);
                    return Ok(c2c);

                case "channel":
                    var channel = await _bot.SendChannelMessageAsync(dto.TargetId, dto.Content);
                    return Ok(channel);

                default:
                    var group = await _bot.SendGroupMessageAsync(dto.TargetId, dto.Content);
                    return Ok(group);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息失败");
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// 获取群基本信息
    /// </summary>
    [HttpGet("groups/{groupOpenId}/info")]
    public async Task<IActionResult> GetGroupInfo(string groupOpenId)
    {
        return Ok(await _bot.GetGroupInfoAsync(groupOpenId));
    }

    /// <summary>
    /// 获取机器人群内状态
    /// </summary>
    [HttpGet("groups/{groupOpenId}/bot-state")]
    public async Task<IActionResult> GetGroupBotState(string groupOpenId)
    {
        return Ok(await _bot.GetGroupBotStateAsync(groupOpenId));
    }

    /// <summary>
    /// 获取群成员列表
    /// </summary>
    [HttpGet("groups/{groupOpenId}/members")]
    public async Task<IActionResult> GetGroupMembers(string groupOpenId, [FromQuery] string? cursor = null)
    {
        return Ok(await _bot.GetGroupMembersAsync(groupOpenId, cursor));
    }
}
