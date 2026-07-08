using Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Interfaces;

namespace UserService.Controllers;

[Route("api/user-sessions")]
[ApiController]
public class UserSessionController : ControllerBase
{
    private readonly IUserSessionService _userSessionService;

    public UserSessionController(IUserSessionService userSessionService)
    {
        _userSessionService = userSessionService;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions([FromQuery] bool activeOnly = false)
    {
        var sessions = await _userSessionService.GetSessionsAsync(activeOnly);
        return Ok(sessions);
    }

    [HttpGet("user/{userId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserSessions(int userId, [FromQuery] bool activeOnly = false)
    {
        if (!User.AccessUser(userId))
        {
            return Forbid();
        }

        var sessions = await _userSessionService.GetUserSessionsAsync(userId, activeOnly);
        return Ok(sessions);
    }
}
