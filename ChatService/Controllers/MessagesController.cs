using Microsoft.AspNetCore.Mvc;
using ChatService.Services;
using Microsoft.AspNetCore.Authorization;
using ChatService.Hubs;
using ChatService.Entities;
using ChatService.Dto;
using ChatService.Exceptions;
using ChatService.Interfaces;
using System.Security.Claims;
using Common.Auth;

namespace ChatService.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IChatMessageService _chatMessageService;
    private readonly IRoomAccessService _roomAccessService;

    public MessagesController(IChatMessageService chatMessageService, IRoomAccessService roomAccessService)
    {
        _chatMessageService = chatMessageService;
        _roomAccessService = roomAccessService;
    }

    [HttpGet("room/{room}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomMessages(string room, [FromQuery] int take = 50)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        var canAccess = await _roomAccessService.CanAccessRoomAsync(room, userId.Value);
        if (!canAccess)
        {
            return Forbid();
        }

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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendMessage([FromBody] CreateChatMessageDto message)
    {
        try
        {
            var savedMessage = await _chatMessageService.SendMessageAsync(User, message);
            if (savedMessage is null)
            {
                return Unauthorized();
            }

            return Ok(savedMessage);
        }
        catch (RoomAccessDeniedException)
        {
            return Forbid();
        }
    }

    //[HttpGet("getMessage")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //public async Task<IActionResult> Get
}
