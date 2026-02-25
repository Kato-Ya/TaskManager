using Grpc.Core;
//using AuthenticationService.Protos;
using AuthenticationService.Dto;
using UserService.Protos;
using Microsoft.AspNetCore.Authorization;

namespace AuthenticationService.GrpcServices;

[AllowAnonymous]
public class GrpcUserClientService
{
    private readonly UserGrpc.UserGrpcClient _client;

    public GrpcUserClientService(UserGrpc.UserGrpcClient client)
    {
        _client = client;
    }

    public async Task<UserDto> GetUserByIdAsync(int userId)
    {
        try
        {
            var response = await _client.GetUserByIdAsync(new UserIdRequest { Id = userId });

            return new UserDto
            {
                Id = response.Id,
                Username = response.Username,
                Email = response.Email,
                CreatedAt = DateTime.Parse(response.CreatedAt),
                PasswordHash = response.PasswordHash,
                Roles = response.Roles.ToList()
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var response = await _client.GetUserByNameAsync(new UserNameRequest { Username = username });
            return new UserDto
            {
                Id = response.Id,
                Username = response.Username,
                Email = response.Email,
                CreatedAt = DateTime.Parse(response.CreatedAt),
                PasswordHash = response.PasswordHash,
                Roles = response.Roles.ToList()
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}