using Grpc.Core;
using Notification.Protos;
using Notification.Protos;
using NotificationService.Dto;

namespace NotificationService.GrpcServices;
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
                CreatedAt = DateTime.Parse(response.CreatedAt)
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    //public async Task<UserDto?> GetUserByUsernameAsync(string username)
    //{
    //    try
    //    {
    //        var response = await _client.GetUserByNameAsync(new UserNameRequest { Username = username });
    //        return new UserDto
    //        {
    //            Id = response.Id,
    //            Username = response.Username,
    //            Email = response.Email,
    //            CreatedAt = DateTime.Parse(response.CreatedAt)
    //        };
    //    }
    //    catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
    //    {
    //        return null;
    //    }
    //}
}