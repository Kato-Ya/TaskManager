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
            services.AddSingleton<IUserService, FakeUserService>();
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
}
