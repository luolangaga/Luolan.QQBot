using Luolan.QQBot.AspNetExample.Models;
using Luolan.QQBot.Core.Abstractions;
using Luolan.QQBot.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Luolan.QQBot.AspNetExample.Controllers;

[ApiController]
[Route("api/bot")]
public class BotApiController : ControllerBase
{
    private readonly IBotClient _botClient;
    private readonly ILogger<BotApiController> _logger;
    private readonly IConfiguration _configuration;

    public BotApiController(IBotClient botClient, ILogger<BotApiController> logger, IConfiguration configuration)
    {
        _botClient = botClient;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var protocol = _configuration["Bot:Protocol"] ?? "Official";
        return Ok(new
        {
            IsConnected = _botClient.IsConnected,
            CurrentUser = _botClient.CurrentUser,
            Protocol = protocol
        });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.TargetId) || string.IsNullOrEmpty(dto.Content))
            {
                return BadRequest("TargetId and Content cannot be empty");
            }

            // Default to sending as group message
            var target = MessageTarget.Group(dto.TargetId);
            
            var result = await _botClient.Messages.SendTextAsync(target, dto.Content);
            
            return result.Success 
                ? Ok(result) 
                : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            return Problem(ex.Message);
        }
    }
}
