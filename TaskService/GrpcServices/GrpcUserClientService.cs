using TaskService.Dto;
using Grpc.Core;
using TaskService.Protos;

namespace TaskService.GrpcServices;
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
}
