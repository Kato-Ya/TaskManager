using Microsoft.AspNetCore.Mvc;
using ChatService.Services;
using ChatService.Entities;
using ChatService.Interfaces;

namespace ChatService.Controllers;

[Route("api/chat")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("room/{room}")]
    public async Task<IActionResult> GetRoomMessages(string room, [FromQuery] int take = 50)
    {
        var messages = await _messageService.GetMessagesByRoomAsync(room, take);
        return Ok(messages);
    }
}
