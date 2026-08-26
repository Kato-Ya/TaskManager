using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.PasswordWorker;
using UserService.Repositories;
using UserService.Services;

namespace UserService.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class UserServicePersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public UserServicePersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateUser_HashesPasswordAndAssignsDistinctRoles()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var role = await AddRoleAsync(dbContext, Unique("creator"));
        var service = CreateUserService(dbContext);

        var user = await service.CreateUserAsync(new CreateUserDto
        {
            Username = Unique("created-user"),
            Email = UniqueEmail("created"),
            Password = "plain-password",
            State = "Active",
            CreatedAt = DateTime.UtcNow,
            RoleIds = [role.Id, role.Id]
        });

        dbContext.ChangeTracker.Clear();
        var persisted = await dbContext.Users
            .Include(item => item.UserRoles)
            .SingleAsync(item => item.Id == user.Id);

        Assert.Equal("hashed:plain-password", persisted.PasswordHash);
        Assert.Single(persisted.UserRoles);
        Assert.Equal(role.Id, persisted.UserRoles.Single().RoleId);
    }

    [Fact]
    public async Task UpdateUser_ChangesEditableFieldsAndPreservesPassword()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var user = await AddUserAsync(dbContext, Unique("update"), UniqueEmail("update"));
        var originalHash = user.PasswordHash;
        var service = CreateUserService(dbContext);

        await service.UpdateUserAsync(new UserDto
        {
            Id = user.Id,
            Username = Unique("updated"),
            Email = UniqueEmail("updated"),
            State = "Inactive"
        });

        dbContext.ChangeTracker.Clear();
        var persisted = await dbContext.Users.SingleAsync(item => item.Id == user.Id);

        Assert.Equal("Inactive", persisted.State);
        Assert.Equal(originalHash, persisted.PasswordHash);
    }

    [Fact]
    public async Task DeleteUser_RemovesRolesAndSessions()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var role = await AddRoleAsync(dbContext, Unique("delete-role"));
        var user = await AddUserAsync(dbContext, Unique("delete"), UniqueEmail("delete"));
        dbContext.Set<UserRole>().Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        dbContext.UserSessions.Add(new UserSession { UserId = user.Id, IsActive = true });
        await dbContext.SaveChangesAsync();
        var service = CreateUserService(dbContext);

        var deleted = await service.DeleteUserAsync(user.Id);

        dbContext.ChangeTracker.Clear();
        Assert.True(deleted);
        Assert.False(await dbContext.Users.AnyAsync(item => item.Id == user.Id));
        Assert.False(await dbContext.Set<UserRole>().AnyAsync(item => item.UserId == user.Id));
        Assert.False(await dbContext.UserSessions.AnyAsync(item => item.UserId == user.Id));
    }

    [Fact]
    public async Task UserUniqueIndexes_RejectDuplicateUsernameAndEmail()
    {
        var username = Unique("unique-user");
        var email = UniqueEmail("unique");

        await using (var seedContext = _fixture.CreateDbContext())
        {
            await AddUserAsync(seedContext, username, email);
        }

        await using (var usernameContext = _fixture.CreateDbContext())
        {
            usernameContext.Users.Add(CreateUser(username, UniqueEmail("other")));
            await Assert.ThrowsAsync<DbUpdateException>(() => usernameContext.SaveChangesAsync());
        }

        await using (var emailContext = _fixture.CreateDbContext())
        {
            emailContext.Users.Add(CreateUser(Unique("other-user"), email));
            await Assert.ThrowsAsync<DbUpdateException>(() => emailContext.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task RoleService_PerformsCrudOperations()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var service = new RoleService(new EfRepositoryUser<Roles>(dbContext));
        var roleName = Unique("role");

        var role = await service.CreateRoleAsync(new RoleDto
        {
            Name = roleName,
            Description = "Initial"
        });
        role.Description = "Updated";
        await service.UpdateRoleAsync(new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        });
        var deleted = await service.DeleteRoleAsync(role.Id);

        dbContext.ChangeTracker.Clear();
        Assert.True(deleted);
        Assert.False(await dbContext.Roles.AnyAsync(item => item.Id == role.Id));
    }

    [Fact]
    public async Task AssignRoles_ReplacesExistingRolesAndRemovesDuplicates()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var user = await AddUserAsync(dbContext, Unique("roles"), UniqueEmail("roles"));
        var oldRole = await AddRoleAsync(dbContext, Unique("old"));
        var firstRole = await AddRoleAsync(dbContext, Unique("first"));
        var secondRole = await AddRoleAsync(dbContext, Unique("second"));
        dbContext.Set<UserRole>().Add(new UserRole { UserId = user.Id, RoleId = oldRole.Id });
        await dbContext.SaveChangesAsync();
        var service = new UserRoleService(
            new EfRepositoryUser<Users>(dbContext),
            new EfRepositoryUser<UserRole>(dbContext));

        await service.AssignRolesAsync(user.Id, [firstRole.Id, firstRole.Id, secondRole.Id]);

        dbContext.ChangeTracker.Clear();
        var roleIds = await dbContext.Set<UserRole>()
            .Where(item => item.UserId == user.Id)
            .OrderBy(item => item.RoleId)
            .Select(item => item.RoleId)
            .ToArrayAsync();

        Assert.Equal(new[] { firstRole.Id, secondRole.Id }.OrderBy(id => id), roleIds);
    }

    [Fact]
    public async Task AssignRoles_RejectsUnknownUser()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var service = new UserRoleService(
            new EfRepositoryUser<Users>(dbContext),
            new EfRepositoryUser<UserRole>(dbContext));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.AssignRolesAsync(int.MaxValue, []));

        Assert.Contains("User", exception.Message);
    }

    [Fact]
    public async Task SignIn_ClosesPreviousActiveSession()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var user = await AddUserAsync(dbContext, Unique("signin"), UniqueEmail("signin"));
        dbContext.UserSessions.Add(new UserSession
        {
            UserId = user.Id,
            IsActive = true,
            SigninTime = DateTime.UtcNow.AddMinutes(-10)
        });
        await dbContext.SaveChangesAsync();
        var service = new UserSessionService(new EfRepositoryUser<UserSession>(dbContext));

        await service.SignInUserSessionAsync(user.Id, "127.0.0.1", "tests");

        dbContext.ChangeTracker.Clear();
        var sessions = await dbContext.UserSessions
            .Where(item => item.UserId == user.Id)
            .OrderBy(item => item.SigninTime)
            .ToArrayAsync();

        Assert.Equal(2, sessions.Length);
        Assert.False(sessions[0].IsActive);
        Assert.NotNull(sessions[0].SignoutTime);
        Assert.True(sessions[1].IsActive);
        Assert.Equal("127.0.0.1", sessions[1].IpAddress);
    }

    [Fact]
    public async Task SignOut_ClosesCurrentActiveSession()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var user = await AddUserAsync(dbContext, Unique("signout"), UniqueEmail("signout"));
        var session = new UserSession { UserId = user.Id, IsActive = true };
        dbContext.UserSessions.Add(session);
        await dbContext.SaveChangesAsync();
        var service = new UserSessionService(new EfRepositoryUser<UserSession>(dbContext));

        await service.SignOutUserSessionAsync(user.Id);

        dbContext.ChangeTracker.Clear();
        var persisted = await dbContext.UserSessions.SingleAsync(item => item.Id == session.Id);
        Assert.False(persisted.IsActive);
        Assert.NotNull(persisted.SignoutTime);
    }

    [Fact]
    public async Task SessionQueries_FilterByActivityAndUser()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var firstUser = await AddUserAsync(dbContext, Unique("session-a"), UniqueEmail("session-a"));
        var secondUser = await AddUserAsync(dbContext, Unique("session-b"), UniqueEmail("session-b"));
        dbContext.UserSessions.AddRange(
            new UserSession { UserId = firstUser.Id, IsActive = true },
            new UserSession
            {
                UserId = firstUser.Id,
                IsActive = false,
                SignoutTime = DateTime.UtcNow
            },
            new UserSession { UserId = secondUser.Id, IsActive = true });
        await dbContext.SaveChangesAsync();
        var service = new UserSessionService(new EfRepositoryUser<UserSession>(dbContext));

        var activeSessions = await service.GetSessionsAsync(activeOnly: true);
        var firstUserSessions = await service.GetUserSessionsAsync(firstUser.Id);

        Assert.DoesNotContain(activeSessions, item => !item.IsActive);
        Assert.Equal(2, firstUserSessions.Count());
        Assert.All(firstUserSessions, item => Assert.Equal(firstUser.Id, item.UserId));
    }

    private static global::UserService.Services.UserService CreateUserService(
        ApplicationDbContext dbContext) => new(
        new EfRepositoryUser<Users>(dbContext),
        new FixedPasswordHasher(),
        new EfRepositoryUser<UserRole>(dbContext),
        new EfRepositoryUser<UserSession>(dbContext));

    private static async Task<Roles> AddRoleAsync(
        ApplicationDbContext dbContext,
        string name)
    {
        var role = new Roles { Name = name, Description = "Test role" };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        return role;
    }

    private static async Task<Users> AddUserAsync(
        ApplicationDbContext dbContext,
        string username,
        string email)
    {
        var user = CreateUser(username, email);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static Users CreateUser(string username, string email) => new()
    {
        Username = username,
        Email = email,
        PasswordHash = "existing-hash",
        State = "Active",
        CreatedAt = DateTime.UtcNow
    };

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(50, prefix.Length + 33)];

    private static string UniqueEmail(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}@example.test";

    private sealed class FixedPasswordHasher : IPasswordHasher
    {
        public string Encrypt(string source) => $"hashed:{source}";

        public bool IsPassowrdTrue(string userPassword, string password) =>
            password == Encrypt(userPassword);
    }
}
