 using Authentication.Protos;
using AuthenticationService.Interfaces;
using Grpc.Core;
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
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        try
        {
            var response = await _authService.SignIn(request, null!);
            return Ok(response);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized(new { message = ex.Status.Detail });
        }
    }

    [HttpPost("refresh")]
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
    public async Task<IActionResult> SignOut([FromBody] SignOutRequest request)
    {
        var response = await _authService.SignOut(request, null!);
        return Ok(response);
    }
}