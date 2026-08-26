using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Entities;
using UserService.Repositories;
using UserService.Specifications.UserSpecifications;

namespace UserService.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class UserSpecificationPostgreSqlTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public UserSpecificationPostgreSqlTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.MigrateAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var adminRole = new Roles
        {
            Name = "Admin",
            Description = "Administrator"
        };
        var userRole = new Roles
        {
            Name = "User",
            Description = "Regular user"
        };

        dbContext.Users.AddRange(
            CreateUser("alpha", "alpha@example.test", adminRole, userRole),
            CreateUser("zulu", "zulu@example.test", userRole));

        await dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migrations_CreateSchemaMatchingCurrentModel()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        var user = await dbContext.Users.SingleAsync(item => item.Username == "alpha");
        var session = new UserSession
        {
            UserId = user.Id,
            SigninTime = DateTime.UtcNow,
            IsActive = true
        };

        dbContext.UserSessions.Add(session);
        await dbContext.SaveChangesAsync();

        Assert.Empty(pendingMigrations);
        Assert.True(session.Id > 0);
    }

    [Fact]
    public async Task CurrentUserSpecification_ProjectsRoleNames()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var userId = await GetUserIdAsync(dbContext, "alpha");
        var repository = new EfRepositoryUser<Users>(dbContext);

        var result = await repository.FirstOrDefaultAsync(
            new CurrentUserSpecification(userId));

        Assert.NotNull(result);
        Assert.Equal("alpha", result.Username);
        Assert.Equal(["Admin", "User"], result.Roles.OrderBy(role => role).ToArray());
    }

    [Fact]
    public async Task UserResponseSpecification_ProjectsDetailedRoles()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var userId = await GetUserIdAsync(dbContext, "alpha");
        var repository = new EfRepositoryUser<Users>(dbContext);

        var result = await repository.FirstOrDefaultAsync(
            new UserResponseSpecification(userId));

        Assert.NotNull(result);
        Assert.Contains(result.Roles, role =>
            role.Name == "Admin" && role.Description == "Administrator");
    }

    [Fact]
    public async Task UserResponseSpecification_ReturnsNullForUnknownUser()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new EfRepositoryUser<Users>(dbContext);

        var result = await repository.FirstOrDefaultAsync(
            new UserResponseSpecification(int.MaxValue));

        Assert.Null(result);
    }

    [Fact]
    public async Task UserResponseSpecification_OrdersUsersByUsernameDescending()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new EfRepositoryUser<Users>(dbContext);

        var result = await repository.ListAsync(new UserResponseSpecification());

        Assert.Equal(["zulu", "alpha"], result.Select(user => user.Username).ToArray());
    }

    [Fact]
    public async Task UserSearchSpecification_SelectsOnlySearchFields()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var specification = new UserSearchSpecification();
        var repository = new EfRepositoryUser<Users>(dbContext);

        var result = await repository.ListAsync(specification);
        var query = SpecificationEvaluator.Default.GetQuery(dbContext.Users, specification);
        var sql = query.ToQueryString().ToLowerInvariant();

        Assert.Equal(["zulu", "alpha"], result.Select(user => user.Username).ToArray());
        Assert.Contains("username", sql);
        Assert.DoesNotContain("password_hash", sql);
        Assert.DoesNotContain("email", sql);
        Assert.DoesNotContain("state", sql);
    }

    [Fact]
    public async Task RoleNameUniqueIndex_RejectsDuplicateRole()
    {
        await using var dbContext = _fixture.CreateDbContext();
        dbContext.Roles.Add(new Roles { Name = "Admin" });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    private static Users CreateUser(
        string username,
        string email,
        params Roles[] roles)
    {
        var user = new Users
        {
            Username = username,
            Email = email,
            PasswordHash = "test-password-hash",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { User = user, Role = role });
        }

        return user;
    }

    private static Task<int> GetUserIdAsync(
        ApplicationDbContext dbContext,
        string username) =>
        dbContext.Users
            .Where(user => user.Username == username)
            .Select(user => user.Id)
            .SingleAsync();
}
