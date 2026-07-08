using Ardalis.Specification.EntityFrameworkCore;
using UserService.Specifications.UserSpecifications;
using Grpc.Core;
using UserService.Data;
using UserService.Protos;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using UserService.Entities;
using Microsoft.AspNetCore.Authorization;

namespace UserService.GrpcServices;

[AllowAnonymous]
public class GrpcUserServerService : UserGrpc.UserGrpcBase
{
    //private readonly ApplicationDbContext _dbContext;
    private readonly IRepositoryBase<Users> _usersRepository;
    private readonly ILogger<GrpcUserServerService> _logger;

    public GrpcUserServerService(
        /*ApplicationDbContext dbContext,*/ IRepositoryBase<Users> repository,
        ILogger<GrpcUserServerService> logger)
    {
        //_dbContext = dbContext;
        _usersRepository = repository;
        _logger = logger;
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
            PasswordHash = user.PasswordHash,
            Roles = { user.UserRoles.Select(ur => ur.Role.Name) }
        };
    }

    public override async Task<UserResponse> GetUserByName(
        UserNameRequest request,
        ServerCallContext callContext)
    {
        try
        {
            var user = await _usersRepository.FirstOrDefaultAsync(
                new UserGetByNameSpecification(request.Username));

            if (user == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound,
                        $"User with name {request.Username} not found"));
            }

            _logger.LogDebug("User found by gRPC username: {Username}", user.Username);

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt.ToString("O"),
                PasswordHash = user.PasswordHash,
                Roles =
                {
                    (user.UserRoles ?? Enumerable.Empty<UserRole>())
                        .Where(ur => ur.Role != null)
                        .Select(ur => ur.Role.Name)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC username lookup failed for {Username}", request.Username);

            throw;
        }
    }
}
