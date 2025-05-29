using Microsoft.AspNetCore.Mvc;
using UserService.Dto;
using UserService.Entities;
using UserService.Interfaces;
using UserService.Services;

namespace UserService.Controllers;

[Route("api/usersRoles")]   
[ApiController]
public class UserRoleController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public UserRoleController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoleList()
    {
        var usersRoles = await _userRoleService.GetAllUserRoleAsync();
        return Ok(usersRoles);
    }

    [HttpGet("{userRoleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoleById(int userRoleId)
    {
        var userRole = await _userRoleService.GetUserRoleByIdAsync(userRoleId);
        return Ok(userRole);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUserRole(int id)
    {
        await _userRoleService.DeleteUserRoleAsync(id);
        return Ok();
    }

    [HttpPost("{userId}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRoles(int userId, [FromBody] List<int> roleIds)
    {
        await _userRoleService.AssignRolesAsync(userId, roleIds);
        return Ok();
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUsersRoles([FromBody] UserRoleDto userRoleDto, int userRoleId)
    {
        if (userRoleId != userRoleDto.Id)
        {
            return BadRequest("Id's do not match");
        }

        var updatedUserRole = await _userRoleService.UpdateUserRoleAsync(userRoleDto);
        return Ok(updatedUserRole);
    }
}
