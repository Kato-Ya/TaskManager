using Authentication.Protos;
using AuthenticationService.Dto;
using AuthenticationService.Interfaces;
using AuthenticationService.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controller;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthHttpResponse>> SignIn([FromBody] SignInRequest request)
    {
        try
        {
            var response = await _authService.SignIn(request, null!);
            //return Ok(response);
            return Ok(new AuthHttpResponse
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                ExpiresIn = response.ExpiresIn
            });
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized(new { message = ex.Status.Detail });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var response = await _authService.Refresh(request, null!);
            return Ok(response);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized(new { message = ex.Status.Detail });
        }
    }

    [HttpPost("signout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignOut([FromBody] SignOutRequest request)
    {
        var response = await _authService.SignOut(request, null!);
        return Ok(response);
    }
}