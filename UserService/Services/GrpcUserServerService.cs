using Ardalis.Specification.EntityFrameworkCore;
using Grpc.Core;
using UserService.Data;
using UserService.Protos;
using Microsoft.EntityFrameworkCore;

namespace UserService.Services;
public class GrpcUserServerService : UserGrpc.UserGrpcBase
{
    private readonly ApplicationDbContext _dbContext;

    public GrpcUserServerService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<UserResponse> GetUserById(UserIdRequest request, ServerCallContext callContext)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.Id);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {request.Id} not found"));
        }

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt.ToString("O"),
        };
    }
}
