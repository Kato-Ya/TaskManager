using Ardalis.Specification.EntityFrameworkCore;
using UserService.Specifications.UserSpecifications;
using Grpc.Core;
using UserService.Data;
using UserService.Protos;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using UserService.Entities;

namespace UserService.GrpcServices;
public class GrpcUserServerService : UserGrpc.UserGrpcBase
{
    //private readonly ApplicationDbContext _dbContext;
    private readonly IRepositoryBase<Users> _usersRepository;

    public GrpcUserServerService(/*ApplicationDbContext dbContext,*/ IRepositoryBase<Users> repository)
    {
        //_dbContext = dbContext;
        _usersRepository = repository;
    }

    public override async Task<UserResponse> GetUserById(UserIdRequest request, ServerCallContext callContext)
    {
        //var user = await _dbContext.Users
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(user => user.Id == request.Id);
        var user = await _usersRepository.FirstOrDefaultAsync(new UserGetByIdSpecification(request.Id));

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

    public override async Task<UserResponse> GetUserByName(UserNameRequest request, ServerCallContext callContext)
    {
        var user = await _usersRepository.FirstOrDefaultAsync(new UserGetByNameSpecification(request.Username));

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with name {request.Username} not found"));
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