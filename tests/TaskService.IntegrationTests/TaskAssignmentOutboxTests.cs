using System.Text.Json;
using Common.Messaging.Outbox;
using Grpc.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Entities;
using TaskService.GrpcServices;
using TaskService.Repositories;
using TaskService.Services;
using TMApi.Contracts;
using UserService.Protos;

namespace TaskService.IntegrationTests;

public class TaskAssignmentOutboxTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new SqliteConnection("Data Source=:memory:");
    private ApplicationDbContext _db = null!;
    private TaskUserService _service = null!;
    private readonly UserClient _userClient = new UserClient();

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        await _db.Database.EnsureCreatedAsync();
        _db.Tasks.Add(new Tasks { Id = 10, Title = "Prepare report", CreatorId = 1 });
        await _db.SaveChangesAsync();
        _service = new TaskUserService(new EfRepositoryTask<TaskUser>(_db), new EfRepositoryTask<Tasks>(_db),
            new GrpcUserClientService(_userClient), _db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Assignment_SavesEventWithoutBroker_AndDoesNotNotifyTwice()
    {
        Assert.True(await _service.AssignUserAsync(10, 7));
        Assert.False(await _service.AssignUserAsync(10, 7));
        Assert.Equal(1, await _db.TaskUser.CountAsync());
        var pending = await _db.Set<OutboxMessage>().SingleAsync();
        var assigned = JsonSerializer.Deserialize<TaskAssignedV1>(pending.Payload)!;
        Assert.Equal(10, assigned.TaskId);
        Assert.Equal(7, assigned.UserId);
        Assert.Equal("Prepare report", assigned.TaskTitle);
        Assert.Equal(pending.Id, assigned.EventId);
    }

    [Fact]
    public async Task OutboxFailure_DoesNotSaveAssignment()
    {
        await _db.Database.ExecuteSqlRawAsync("DROP TABLE task_outbox");
        await Assert.ThrowsAsync<DbUpdateException>(() => _service.AssignUserAsync(10, 7));
        _db.ChangeTracker.Clear();
        Assert.Equal(0, await _db.TaskUser.CountAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingUserOrGrpcFailure_DoesNotSaveAssignmentOrEvent(bool unavailable)
    {
        _userClient.Error = new RpcException(new Status(
            unavailable ? StatusCode.Unavailable : StatusCode.NotFound, "User lookup failed"));
        if (unavailable)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.AssignUserAsync(10, 7));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AssignUserAsync(10, 7));
        }

        Assert.Equal(0, await _db.TaskUser.CountAsync());
        Assert.Empty(await _db.Set<OutboxMessage>().ToListAsync());
    }

    private class UserClient : UserGrpc.UserGrpcClient
    {
        public Exception? Error { get; set; }
        public override AsyncUnaryCall<UserResponse> GetUserByIdAsync(UserIdRequest request, CallOptions options)
        {
            Task<UserResponse> response;

            if (Error == null)
            {
                response = Task.FromResult(new UserResponse
                {
                    Id = request.Id,
                    Username = "user",
                    CreatedAt = "2026-09-07T00:00:00Z"
                });
            }
            else
            {
                response = Task.FromException<UserResponse>(Error);
            }

            return new AsyncUnaryCall<UserResponse>(
                response,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }
    }
}
