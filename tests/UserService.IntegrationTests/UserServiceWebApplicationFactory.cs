using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserService.Dto;
using UserService.Entities;
using UserService.Interfaces;

namespace UserService.IntegrationTests;

public sealed class UserServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "test-key-that-is-at-least-thirty-two-characters-long";
    public const string JwtIssuer = "TMApi.Tests";
    public const string JwtAudience = "TMApp.Tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Port=5432;Database=tests;Username=tests;Password=tests"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUserService>();
            services.RemoveAll<IRoleService>();
            services.RemoveAll<IUserRoleService>();
            services.RemoveAll<IUserSessionService>();
            services.AddSingleton<IUserService, FakeUserService>();
            services.AddSingleton<IRoleService, FakeRoleService>();
            services.AddSingleton<IUserRoleService, FakeUserRoleService>();
            services.AddSingleton<IUserSessionService, FakeUserSessionService>();
        });
    }

    private sealed class FakeUserService : IUserService
    {
        public Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            return Task.FromResult<IEnumerable<UserResponseDto>>(
            [
                new UserResponseDto
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@example.test",
                    State = "Active",
                    CreatedAt = DateTime.UnixEpoch,
                    Roles = [new RoleDto { Id = 1, Name = "Admin" }]
                }
            ]);
        }

        public Task<IEnumerable<UserSearchDto>> GetUserSearchAsync()
        {
            return Task.FromResult<IEnumerable<UserSearchDto>>(
            [
                new UserSearchDto { Id = 1, Username = "admin" },
                new UserSearchDto { Id = 7, Username = "user" }
            ]);
        }

        public Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
        {
            if (userId == 404)
            {
                return Task.FromResult<CurrentUserDto?>(null);
            }

            return Task.FromResult<CurrentUserDto?>(new CurrentUserDto
            {
                Id = userId,
                Username = $"user-{userId}",
                Email = $"user-{userId}@example.test",
                State = "Active",
                CreatedAt = DateTime.UnixEpoch,
                Roles = ["User"]
            });
        }

        public Task<UserResponseDto?> GetByIdUserAsync(int userId)
        {
            if (userId == 404)
            {
                return Task.FromResult<UserResponseDto?>(null);
            }

            return Task.FromResult<UserResponseDto?>(new UserResponseDto
            {
                Id = userId,
                Username = $"user-{userId}",
                Email = $"user-{userId}@example.test",
                State = "Active",
                CreatedAt = DateTime.UnixEpoch
            });
        }

        public Task<Users> CreateUserAsync(CreateUserDto createUserDto) =>
            throw new NotSupportedException();

        public Task<Users> UpdateUserAsync(UserDto userDto) =>
            throw new NotSupportedException();

        public Task<bool> DeleteUserAsync(int userId) =>
            throw new NotSupportedException();
    }

    private sealed class FakeRoleService : IRoleService
    {
        public Task<IEnumerable<Roles>> GetAllRoleAsync() =>
            Task.FromResult<IEnumerable<Roles>>([CreateRole(1, "Admin")]);

        public Task<Roles?> GetByIdRoleAsync(int roleId) =>
            Task.FromResult<Roles?>(CreateRole(roleId, $"Role-{roleId}"));

        public Task<Roles> CreateRoleAsync(RoleDto roleDto) =>
            Task.FromResult(CreateRole(10, roleDto.Name, roleDto.Description));

        public Task<Roles> UpdateRoleAsync(RoleDto roleDto) =>
            Task.FromResult(CreateRole(roleDto.Id, roleDto.Name, roleDto.Description));

        public Task<bool> DeleteRoleAsync(int roleId) => Task.FromResult(true);

        private static Roles CreateRole(int id, string name, string? description = null) => new()
        {
            Id = id,
            Name = name,
            Description = description
        };
    }

    private sealed class FakeUserRoleService : IUserRoleService
    {
        public Task<IEnumerable<UserRole>> GetAllUserRoleAsync() =>
            Task.FromResult<IEnumerable<UserRole>>([]);

        public Task<UserRole?> GetUserRoleByIdAsync(int id) =>
            Task.FromResult<UserRole?>(null);

        public Task AssignRolesAsync(int userId, List<int> roleIds) => Task.CompletedTask;

        public Task DeleteUserRoleAsync(int id) => Task.CompletedTask;

        public Task<UserRole> UpdateUserRoleAsync(UserRoleDto userRoleDto) =>
            Task.FromResult(new UserRole
            {
                Id = userRoleDto.Id,
                UserId = userRoleDto.UserId,
                RoleId = userRoleDto.RoleId
            });
    }

    private sealed class FakeUserSessionService : IUserSessionService
    {
        public Task SignInUserSessionAsync(int userdId, string? ipAddress, string? userAgent) =>
            Task.CompletedTask;

        public Task SignOutUserSessionAsync(int userId) => Task.CompletedTask;

        public Task<IEnumerable<UserSessionDto>> GetSessionsAsync(bool activeOnly = false) =>
            Task.FromResult<IEnumerable<UserSessionDto>>(
                activeOnly ? [CreateSession(1, true)] : [CreateSession(1, true), CreateSession(2, false)]);

        public Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(
            int userId,
            bool activeOnly = false) =>
            Task.FromResult<IEnumerable<UserSessionDto>>(
                activeOnly
                    ? [CreateSession(userId, true)]
                    : [CreateSession(userId, true), CreateSession(userId, false)]);

        private static UserSessionDto CreateSession(int userId, bool isActive) => new()
        {
            Id = isActive ? 1 : 2,
            UserId = userId,
            Username = $"user-{userId}",
            Email = $"user-{userId}@example.test",
            SignInTime = DateTime.UnixEpoch,
            SignOutTime = isActive ? null : DateTime.UnixEpoch.AddHours(1),
            IsActive = isActive
        };
    }
}
