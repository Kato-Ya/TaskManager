using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.Specifications.UserSessionSpecifications;
using Ardalis.Specification;
using System.Threading.Tasks;
using System.Net;

namespace UserService.Services;
public class UserSessionService : IUserSessionService
{
    private readonly IRepositoryBase<UserSession> _repository;
    public UserSessionService(IRepositoryBase<UserSession> repository)
    {
        _repository = repository;
    }

    public async Task SignInUserSessionAsync(int userId, string? ipAddress, string? userAgent)
    {
        var activeSession = await _repository.FirstOrDefaultAsync(
            new UserSessionGetActiveSessionSpecification(userId));

        if (activeSession != null)
        {
            activeSession.IsActive = false;
            activeSession.SignoutTime = DateTime.UtcNow;
            await _repository.UpdateAsync(activeSession);
        }

        var session = new UserSession
        {
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,  
            SigninTime = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.AddAsync(session);
    }

    public async Task SignOutUserSessionAsync(int userId)
    {
        var session = await _repository.FirstOrDefaultAsync(new UserSessionGetActiveSessionSpecification(userId));

        if (session != null)
        {
            session.IsActive = false;
            session.SignoutTime = DateTime.UtcNow;
            await _repository.UpdateAsync(session);
        }
    }

    public async Task<IEnumerable<UserSessionDto>> GetSessionsAsync(bool activeOnly = false)
    {
        var sessions = await _repository.ListAsync(new UserSessionGetAllSpecification(activeOnly));
        return sessions.Select(UserSessionToDto);
    }

    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(int userId, bool activeOnly = false)
    {
        var sessions = await _repository.ListAsync(new UserSessionGetByUserIdSpecification(userId, activeOnly));
        return sessions.Select(UserSessionToDto);
    }

    private static UserSessionDto UserSessionToDto(UserSession session)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            Username = session.User.Username,
            Email = session.User.Email,
            SignInTime = session.SigninTime,
            SignOutTime = session.SignoutTime,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            IsActive = session.IsActive
        };
    }
}
