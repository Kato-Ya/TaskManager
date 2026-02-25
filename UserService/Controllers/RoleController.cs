using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Dto;
using UserService.Interfaces;
using UserService.Services;

namespace UserService.Controllers;

[Route("api/roles")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesList()
    {
        var roles = await _roleService.GetAllRoleAsync();
        return Ok(roles);
    }

    [HttpGet("{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(int roleId)
    {
        var role = await _roleService.GetByIdRoleAsync(roleId);
        return Ok(role);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
    {
        if (roleDto == null)
        {
            return BadRequest("Role is null");
        }

        var createdRole =  await _roleService.CreateRoleAsync(roleDto);
        return Ok(createdRole);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRole([FromBody] RoleDto roleDto, int id)
    {
        if (id != roleDto.Id)
        {
            return BadRequest("Id's do not match");
        }

        var updatedRole = await _roleService.UpdateRoleAsync(roleDto);
        return Ok(updatedRole);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRole(int roleId)
    {
        var deletedRole = await _roleService.DeleteRoleAsync(roleId);
        return Ok(deletedRole);
    }
}
