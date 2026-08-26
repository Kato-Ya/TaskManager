using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ChatService.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace ChatService.IntegrationTests;

public class ChatHttpAuthorizationTests : IClassFixture<ChatServiceWebApplicationFactory>
{
    private readonly ChatServiceWebApplicationFactory _factory;

    public ChatHttpAuthorizationTests(ChatServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoomMessages_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/chat/room/global");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRoomMessages_ReturnsOkWithBearerToken()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/chat/room/global",
            accessToken: CreateAccessToken(7));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AccessTokenQuery_IsIgnoredOutsideChatHub()
    {
        var token = CreateAccessToken(7);

        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/chat/room/global?access_token={token}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/chat/sendMessage",
            JsonContent.Create(CreateMessage(senderId: 999)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_OverridesSenderIdWithAuthenticatedUser()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/chat/sendMessage",
            JsonContent.Create(CreateMessage(senderId: 999)),
            CreateAccessToken(7));
        var message = await response.Content.ReadFromJsonAsync<ChatMessageDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(message);
        Assert.Equal(7, message.SenderId);
    }

    [Fact]
    public async Task GetConversation_ReturnsUnauthorizedWhenTokenHasNoUserId()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/chat/conversation/9",
            accessToken: CreateAccessToken(userId: null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChatHubNegotiate_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/chatHub/negotiate?negotiateVersion=1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChatHubNegotiate_AcceptsAccessTokenQuery()
    {
        var token = CreateAccessToken(7);

        using var response = await SendAsync(
            HttpMethod.Post,
            $"/chatHub/negotiate?negotiateVersion=1&access_token={token}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        HttpContent? content = null,
        string? accessToken = null)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        using var request = new HttpRequestMessage(method, path) { Content = content };
        if (accessToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await client.SendAsync(request);
    }

    private static string CreateAccessToken(int? userId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));
        }

        claims.Add(new Claim(ClaimTypes.Role, "User"));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(ChatServiceWebApplicationFactory.JwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: ChatServiceWebApplicationFactory.JwtIssuer,
            audience: ChatServiceWebApplicationFactory.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static CreateChatMessageDto CreateMessage(int senderId) => new()
    {
        Room = "global",
        SenderId = senderId,
        ReceiverId = 9,
        Text = "Hello"
    };
}
