using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChatService.ConnectionManager;
using ChatService.Dto;
using ChatService.Hubs;
using ChatService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using ConnectionManagerImplementation = ChatService.ConnectionManager.ConnectionManager;

namespace ChatService.IntegrationTests;

public class ChatHubSecurityTests
{
    [Fact]
    public async Task SendMessage_OverridesSenderIdWithAuthenticatedUser()
    {
        var chatService = new RecordingChatService();
        var hub = CreateHub(chatService, CreateUser(7), queryUserId: "999");
        var message = new CreateChatMessageDto { SenderId = 999, Text = "Hello" };

        await hub.SendMessage(message);

        Assert.Equal(7, chatService.LastMessage!.SenderId);
    }

    [Fact]
    public async Task SendMessage_DoesNotTrustUserIdFromQuery()
    {
        var chatService = new RecordingChatService();
        var hub = CreateHub(chatService, CreateUser(userId: null), queryUserId: "999");

        var exception = await Assert.ThrowsAsync<HubException>(() =>
            hub.SendMessage(new CreateChatMessageDto { SenderId = 999, Text = "Hello" }));

        Assert.Equal("User is not authenticated", exception.Message);
        Assert.Null(chatService.LastMessage);
    }

    [Fact]
    public async Task ConnectionLifecycle_TracksAuthenticatedUsersConnection()
    {
        var connectionManager = new ConnectionManagerImplementation();
        var hub = CreateHub(
            new RecordingChatService(),
            CreateUser(7),
            connectionManager: connectionManager);

        await hub.OnConnectedAsync();

        Assert.True(connectionManager.TryGetConnection(7, out var connections));
        Assert.Contains("connection-1", connections);

        await hub.OnDisconnectedAsync(null);

        Assert.False(connectionManager.TryGetConnection(7, out _));
    }

    private static ChatHub CreateHub(
        IChatService chatService,
        ClaimsPrincipal user,
        string? queryUserId = null,
        IConnectionManager? connectionManager = null)
    {
        var httpContext = new DefaultHttpContext();
        if (queryUserId != null)
        {
            httpContext.Request.QueryString = new QueryString($"?userId={queryUserId}");
        }

        var features = new FeatureCollection();
        features.Set<IHttpContextFeature>(new TestHttpContextFeature(httpContext));

        return new ChatHub(chatService, connectionManager ?? new ConnectionManagerImplementation())
        {
            Context = new TestHubCallerContext("connection-1", user, features)
        };
    }

    private static ClaimsPrincipal CreateUser(int? userId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));
        }

        claims.Add(new Claim(ClaimTypes.Role, "User"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private sealed class RecordingChatService : IChatService
    {
        public CreateChatMessageDto? LastMessage { get; private set; }

        public Task<ChatMessageDto> SendMessageAsync(CreateChatMessageDto dto)
        {
            LastMessage = dto;
            return Task.FromResult(new ChatMessageDto
            {
                SenderId = dto.SenderId,
                SenderName = $"user-{dto.SenderId}",
                Text = dto.Text
            });
        }
    }

    private sealed class TestHubCallerContext : HubCallerContext
    {
        public TestHubCallerContext(
            string connectionId,
            ClaimsPrincipal user,
            IFeatureCollection features)
        {
            ConnectionId = connectionId;
            User = user;
            Features = features;
        }

        public override string ConnectionId { get; }
        public override string? UserIdentifier => null;
        public override ClaimsPrincipal? User { get; }
        public override IDictionary<object, object?> Items { get; } =
            new Dictionary<object, object?>();
        public override IFeatureCollection Features { get; }
        public override CancellationToken ConnectionAborted => CancellationToken.None;
        public override void Abort() { }
    }

    private sealed class TestHttpContextFeature : IHttpContextFeature
    {
        public TestHttpContextFeature(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public HttpContext? HttpContext { get; set; }
    }
}
