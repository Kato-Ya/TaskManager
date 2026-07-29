using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.Specifications.UserSpecifications;
using UserService.Specifications.UserRoleSpecifications;
using UserService.Specifications.UserSessionSpecifications;
using Ardalis.Specification;
using System.Threading.Tasks;
using UserService.PasswordWorker;

namespace UserService.Services;
public class UserService : IUserService
{
    private readonly IRepositoryBase<Users> _repository;
    private readonly IRepositoryBase<UserRole> _userRoleRepository;
    private readonly IRepositoryBase<UserSession> _userSessionRepository;
    private readonly IPasswordHasher _passwordHasher;
    public UserService(
        IRepositoryBase<Users> repository,
        IPasswordHasher passwordHasher,
        IRepositoryBase<UserRole> userRoleRepository,
        IRepositoryBase<UserSession> userSessionRepository)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _userRoleRepository = userRoleRepository;
        _userSessionRepository = userSessionRepository;
    }

    //public async Task<IEnumerable<Users>> GetAllUsersAsync()
    //{
    //    return await _repository.ListAsync(new UserGetAllSpecification());
    //}

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var userList = await _repository.ListAsync(new UserGetAllSpecification());

        return userList.Select(user => new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            State = user.State,
            CreatedAt = user.CreatedAt,

            Roles = user.UserRoles
                .Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description
                })
                .ToList()
        });
    }

    public async Task<IEnumerable<UserSearchDto>> GetUserSearchAsync()
    {
        var userList = await _repository.ListAsync(new UserGetAllSpecification());

        return userList.Select(user => new UserSearchDto
        {
            Id = user.Id,
            Username = user.Username
        });
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _repository.FirstOrDefaultAsync(
            new UserGetByIdSpecification(userId));

        if (user == null)
            return null;

        return new CurrentUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            State = user.State,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.Name)
                .ToList()
        };
    }

    //public async Task<Users?> GetByIdUserAsync(int userId)
    //{
    //    return await _repository.FirstOrDefaultAsync(new UserGetByIdSpecification(userId));
    //}

    public async Task<UserResponseDto?> GetByIdUserAsync(int userId)
    {
        var user = await _repository.FirstOrDefaultAsync(
            new UserGetByIdSpecification(userId));

        if (user == null)
            return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            State = user.State,
            CreatedAt = user.CreatedAt,

            Roles = user.UserRoles
                .Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description
                })
                .ToList()
        };
    }

    public async Task<Users> CreateUserAsync(CreateUserDto createUserDto)
    {
        string passwordHash = _passwordHasher.Encrypt(createUserDto.Password);
        var user = new Users
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,    
            PasswordHash = passwordHash,
            CreatedAt = createUserDto.CreatedAt,
            State = createUserDto.State
        };

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        foreach (var roleId in createUserDto.RoleIds.Distinct())
        {
            await _userRoleRepository.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        await _userRoleRepository.SaveChangesAsync();

        return user;
    }

    public async Task<Users> UpdateUserAsync(UserDto userDto)
    {
        var currentUser = await _repository.FirstOrDefaultAsync(new UserGetByIdSpecification(userDto.Id));

        if (currentUser == null)
        {
            throw new ArgumentException("User did not found");
        }

        currentUser.Id = userDto.Id;
        currentUser.Username = userDto.Username;
        currentUser.Email = userDto.Email;
        currentUser.State = userDto.State;
        //currentUser.PasswordHash = userDto.PasswordHash;
        //currentUser.CreatedAt = userDto.CreatedAt;
        //currentUser.UserRoles = userDto.UserRoles;
        //currentUser.Roles = userDto.Roles;


        await _repository.UpdateAsync(currentUser);
        return currentUser;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user= await _repository.FirstOrDefaultAsync(new UserGetByIdSpecification(userId));

        if (user == null)
        {
            throw new ArgumentException("User did not found");
        }

        var userSessions = await _userSessionRepository.ListAsync(
            new UserSessionGetByUserIdSpecification(userId));

        if (userSessions.Any())
        {
            await _userSessionRepository.DeleteRangeAsync(userSessions);
        }

        await _repository.DeleteAsync(user);
        return true;
    }


}
