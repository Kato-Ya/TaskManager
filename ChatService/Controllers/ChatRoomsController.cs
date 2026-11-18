using ChatService.Dto;
using ChatService.Exceptions;
using ChatService.Interfaces;
using Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[Route("api/chat/rooms")]
[ApiController]
[Authorize]
public class ChatRoomsController : ControllerBase
{
    private readonly IChatRoomService _chatRoomService;

    public ChatRoomsController(IChatRoomService chatRoomService)
    {
        _chatRoomService = chatRoomService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRoomsList()
    {
        var userId = User.GetUserId();
        if (userId == null || userId.Value <= 0)
        {
            return Unauthorized();
        }

        var rooms = await _chatRoomService.GetUserRoomsAsync(userId.Value);
        return Ok(rooms);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateChatRoomDto createChatRoomDto)
    {
        var userId = User.GetUserId();
        if (userId == null || userId.Value <= 0)
        {
            return Unauthorized();
        }

        try
        {
            var createdRoom = await _chatRoomService.CreateRoomAsync(createChatRoomDto, userId.Value);
            return StatusCode(StatusCodes.Status201Created, createdRoom);
        }
        catch (RoomAlreadyExistsException)
        {
            return Conflict("Room with this name already exists.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
