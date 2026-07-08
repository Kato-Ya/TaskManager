using Microsoft.AspNetCore.Mvc;
using ChatService.Services;
using Microsoft.AspNetCore.Authorization;
using ChatService.Hubs;
using ChatService.Entities;
using ChatService.Dto;
using ChatService.Interfaces;
using System.Security.Claims;

namespace ChatService.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IChatMessageService _chatMessageService;

    public MessagesController(IChatMessageService chatMessageService)
    {
        _chatMessageService = chatMessageService;
    }

    [HttpGet("room/{room}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomMessages(string room, [FromQuery] int take = 50)
    {
        var messages = await _chatMessageService.GetMessagesByRoomAsync(room, take);
        return Ok(messages);
    }

    [HttpGet("conversation/{otherUserId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversationMessages(int otherUserId, [FromQuery] int take = 50)
    {
        var result = await _chatMessageService.GetConversationMessagesAsync(User, otherUserId, take);
        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }

    [HttpPost("sendMessage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage([FromBody] CreateChatMessageDto message)
    {

        var savedMessage = await _chatMessageService.SendMessageAsync(User, message);
        if (savedMessage is null)
        {
            return Unauthorized();
        }

        return Ok(savedMessage);
    }
}
