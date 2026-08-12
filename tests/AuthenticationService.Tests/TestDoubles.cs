using AuthenticationService.Dto;
using AuthenticationService.Interfaces;
using AuthenticationService.Models;
using AuthenticationService.Repositories.TokenRepository;
using Grpc.Core;

namespace AuthenticationService.Tests;

internal sealed class InMemoryRefreshTokenRepository : IJwtRefreshTokenRepository
{
    private readonly Dictionary<string, RefreshToken> _tokens = new();

    public Task<RefreshToken?> GetByIdAsync(string id)
    {
        _tokens.TryGetValue(id, out var token);
        return Task.FromResult(token);
    }

    public Task SaveAsync(RefreshToken refreshToken)
    {
        _tokens[refreshToken.Id] = refreshToken;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RefreshToken refreshToken)
    {
        _tokens.Remove(refreshToken.Id);
        return Task.CompletedTask;
    }

    public bool Contains(string id) => _tokens.ContainsKey(id);
}

internal sealed class FakeUserClientService : IUserClientService
{
    private readonly Dictionary<int, UserDto> _usersById = new();
    private readonly Dictionary<string, UserDto> _usersByName =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(UserDto user)
    {
        _usersById[user.Id] = user;
        _usersByName[user.Username] = user;
    }

    public Task<UserDto?> GetUserByIdAsync(int userId)
    {
        _usersById.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        _usersByName.TryGetValue(username, out var user);
        return Task.FromResult(user);
    }
}

internal sealed class RecordingUserSessionTracker : IUserSessionTracker
{
    public List<int> SignedInUsers { get; } = new();
    public List<int> SignedOutUsers { get; } = new();

    public Task TrackUserSignInAsync(int userId, ServerCallContext context)
    {
        SignedInUsers.Add(userId);
        return Task.CompletedTask;
    }

    public Task TrackUserSignOutAsync(int userId)
    {
        SignedOutUsers.Add(userId);
        return Task.CompletedTask;
    }
}
