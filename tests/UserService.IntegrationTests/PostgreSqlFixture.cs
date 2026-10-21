using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using UserService.Data;

namespace UserService.IntegrationTests;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("tmapi_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }
}

[CollectionDefinition(Name)]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "UserService PostgreSQL";
}
