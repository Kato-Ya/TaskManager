using Microsoft.AspNetCore.Mvc;
using ChatService.Services;
using ChatService.Hubs;
using ChatService.Entities;
using ChatService.Dto;
using ChatService.Interfaces;

namespace ChatService.Controllers;

[Route("api/chat")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IChatService _chatService;

    public MessagesController(IMessageService messageService, IChatService chartService)
    {
        _messageService = messageService;
        _chatService = chartService;
    }

    [HttpGet("room/{room}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomMessages(string room, [FromQuery] int take = 50)
    {
        var messages = await _messageService.GetMessagesByRoomAsync(room, take);
        return Ok(messages);
    }

    [HttpPost("sendMessage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage([FromBody] CreateChatMessageDto message)
    {
        var savedMessage = await _chatService.SendMessageAsync(message);
        return Ok(savedMessage);
    }
}
