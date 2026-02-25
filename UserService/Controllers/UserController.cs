using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Dto;
using UserService.Entities;
using UserService.Interfaces;

namespace UserService.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersList()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _userService.GetByIdUserAsync(userId);
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (createUserDto == null)
        {
            return BadRequest("User is null");
        }

        var createdUser= await _userService.CreateUserAsync(createUserDto);
        return Ok(createdUser);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto userDto, int id)
    {
        if (id != userDto.Id)
        {
            return BadRequest("Id's do not match");
        }

        var updatedUser = await _userService.UpdateUserAsync(userDto);
        return Ok(updatedUser);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var deletedUser = await _userService.DeleteUserAsync(userId);
        return Ok(deletedUser);
    }
}
