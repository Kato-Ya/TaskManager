using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.Specifications.UserSpecifications;
using UserService.Specifications.UserRoleSpecifications;
using Ardalis.Specification;
using System.Threading.Tasks;

namespace UserService.Services;
public class UserService : IUserService
{
    private readonly IRepositoryBase<Users> _repository;
    public UserService(IRepositoryBase<Users> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Users>> GetAllUsersAsync()
    {
        return await _repository.ListAsync(new UserGetAllSpecification());
    }

    public async Task<Users?> GetByIdUserAsync(int userId)
    {
        return await _repository.FirstOrDefaultAsync(new UserGetByIdSpecification(userId));
    }

    public async Task<Users> CreateUserAsync(UserDto userDto)
    {
        return await _repository.AddAsync(new Users(userDto));
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
        currentUser.PasswordHash = userDto.PasswordHash;
        currentUser.CreatedAt = userDto.CreatedAt;
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

        await _repository.DeleteAsync(user);
        return true;
    }


}
