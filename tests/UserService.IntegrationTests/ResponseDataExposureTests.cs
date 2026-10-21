using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.PasswordWorker;
using UserService.Repositories;
using UserService.Services;

namespace UserService.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public class ResponseDataExposureTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions ApiJson = new(JsonSerializerDefaults.Web);
    private readonly PostgreSqlFixture _postgreSqlFixture;

    public ResponseDataExposureTests(PostgreSqlFixture postgreSqlFixture)
    {
        _postgreSqlFixture = postgreSqlFixture;
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = _postgreSqlFixture.CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRoleById_DoesNotExposePasswordHash()
    {
        await using var dbContext = _postgreSqlFixture.CreateDbContext();
        var (_, role) = await AddUserWithRoleAsync(dbContext);
        var service = new RoleService(new EfRepositoryUser<Roles>(dbContext));

        var response = await service.GetByIdRoleAsync(role.Id);

        AssertNoPasswordHash(response);
    }

    [Fact]
    public async Task CreateUser_DoesNotExposePasswordHash()
    {
        await using var dbContext = _postgreSqlFixture.CreateDbContext();
        var role = new Roles { Name = Unique("role"), Description = "Test role" };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        var service = CreateUserService(dbContext);

        var response = await service.CreateUserAsync(new CreateUserDto
        {
            Username = Unique("user"),
            Email = UniqueEmail("user"),
            Password = "test-password",
            State = "Active",
            CreatedAt = DateTime.UtcNow,
            RoleIds = [role.Id]
        });

        AssertNoPasswordHash(response);
    }

    [Fact]
    public async Task UpdateUser_DoesNotExposePasswordHash()
    {
        await using var dbContext = _postgreSqlFixture.CreateDbContext();
        var (user, _) = await AddUserWithRoleAsync(dbContext);
        var service = CreateUserService(dbContext);

        var response = await service.UpdateUserAsync(new UserDto
        {
            Id = user.Id,
            Username = Unique("updated"),
            Email = UniqueEmail("updated"),
            State = "Active"
        });

        AssertNoPasswordHash(response);
    }

    private static void AssertNoPasswordHash(object? response)
    {
        Assert.NotNull(response);

        var json = JsonSerializer.Serialize(response, response!.GetType(), ApiJson);

        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(Users User, Roles Role)> AddUserWithRoleAsync(ApplicationDbContext dbContext)
    {
        var role = new Roles { Name = Unique("role"), Description = "Test role" };
        var user = new Users
        {
            Username = Unique("user"),
            Email = UniqueEmail("user"),
            PasswordHash = "existing-hash",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Roles.Add(role);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        dbContext.Set<UserRole>().Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        return (user, role);
    }

    private static global::UserService.Services.UserService CreateUserService(ApplicationDbContext dbContext) => new(
        new EfRepositoryUser<Users>(dbContext),
        new PasswordHasher(),
        new EfRepositoryUser<UserRole>(dbContext),
        new EfRepositoryUser<UserSession>(dbContext));

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..30];

    private static string UniqueEmail(string prefix) => $"{prefix}-{Guid.NewGuid():N}@example.test";
     
}