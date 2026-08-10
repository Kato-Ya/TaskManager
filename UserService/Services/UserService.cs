using UserService.Interfaces;
using UserService.Dto;
using UserService.Entities;
using UserService.Specifications.UserSpecifications;
using UserService.Specifications.UserSessionSpecifications;
using Ardalis.Specification;
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

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        return await _repository.ListAsync(new UserResponseSpecification());
    }

    public async Task<IEnumerable<UserSearchDto>> GetUserSearchAsync()
    {
        return await _repository.ListAsync(new UserSearchSpecification());
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
    {
        return await _repository.FirstOrDefaultAsync(
            new CurrentUserSpecification(userId));
    }

    public async Task<UserResponseDto?> GetByIdUserAsync(int userId)
    {
        return await _repository.FirstOrDefaultAsync(
            new UserResponseSpecification(userId));
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
