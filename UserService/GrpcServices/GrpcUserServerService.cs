using Ardalis.Specification;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using UserService.Entities;
using UserService.PasswordWorker;
using UserService.Protos;
using UserService.Specifications.UserSpecifications;

namespace UserService.GrpcServices;

[AllowAnonymous]
public class GrpcUserServerService : UserGrpc.UserGrpcBase
{
    private const string DummyPasswordHash =
        "PBKDF2-SHA256$600000$NbAdTGmCnnNh2R1W/PklSA==$aIzvWjMAAJHONptGfqfH5BY6LLIvh435+Ufvwh3eiRI=";

    private readonly IRepositoryBase<Users> _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<GrpcUserServerService> _logger;

    public GrpcUserServerService(
        IRepositoryBase<Users> repository,
        IPasswordHasher passwordHasher,
        ILogger<GrpcUserServerService> logger)
    {
        _usersRepository = repository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public override async Task<UserResponse> GetUserById(UserIdRequest request, ServerCallContext context)
    {
        var user = await _usersRepository.FirstOrDefaultAsync(
            new UserGetByIdSpecification(request.Id), context.CancellationToken);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {request.Id} not found"));
        }

        return ToUserResponse(user);
    }

    public override async Task<UserResponse> GetUserByName(UserNameRequest request, ServerCallContext context)
    {
        var user = await _usersRepository.FirstOrDefaultAsync(
            new UserGetByNameSpecification(request.Username), context.CancellationToken);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with name {request.Username} not found"));
        }

        return ToUserResponse(user);
    }

    public override async Task<VerifyCredentialsResponse> VerifyCredentials(
        VerifyCredentialsRequest request, ServerCallContext context)
    {
        var user = await _usersRepository.FirstOrDefaultAsync(
            new UserGetByNameSpecification(request.Username), context.CancellationToken);

        if (user == null)
        {
            _passwordHasher.Verify(DummyPasswordHash, request.Password);
            return new VerifyCredentialsResponse { Success = false };
        }

        var result = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (result == PasswordCheckResult.Failed)
        {
            return new VerifyCredentialsResponse { Success = false };
        }

        if (result == PasswordCheckResult.SuccessRehashNeeded)
        {
            await UpgradePasswordHashAsync(user, request.Password, context.CancellationToken);
        }

        return new VerifyCredentialsResponse
        {
            Success = true,
            User = ToUserResponse(user)
        };
    }

    private async Task UpgradePasswordHashAsync(Users user, string password, CancellationToken cancellationToken)
    {
        try
        {
            user.PasswordHash = _passwordHasher.Hash(password);
            await _usersRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Password hash upgraded for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password hash upgrade failed for user {UserId}", user.Id);
        }
    }

    private static UserResponse ToUserResponse(Users user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt.ToString("O"),
        Roles = { user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role.Name) }
    };
}
