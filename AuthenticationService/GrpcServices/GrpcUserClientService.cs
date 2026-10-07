using AuthenticationService.Dto;
using AuthenticationService.Interfaces;
using Grpc.Core;
using UserService.Protos;

namespace AuthenticationService.GrpcServices;

public class GrpcUserClientService : IUserClientService
{
    private readonly UserGrpc.UserGrpcClient _client;

    public GrpcUserClientService(UserGrpc.UserGrpcClient client)
    {
        _client = client;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        try
        {
            var response = await _client.GetUserByIdAsync(new UserIdRequest { Id = userId });
            return ToUserDto(response);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<UserDto?> VerifyCredentialsAsync(string username, string password)
    {
        var response = await _client.VerifyCredentialsAsync(new VerifyCredentialsRequest
        {
            Username = username,
            Password = password
        });

        return response.Success ? ToUserDto(response.User) : null;
    }

    private static UserDto ToUserDto(UserResponse response) => new()
    {
        Id = response.Id,
        Username = response.Username,
        Email = response.Email,
        CreatedAt = DateTime.Parse(response.CreatedAt),
        Roles = response.Roles.ToList()
    };
}
