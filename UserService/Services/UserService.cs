using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.Specifications.UserSpecifications;
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
        return await _repository.GetByIdAsync(new UserGetByIdSpecification(userId));
    }

    public async Task<Users> CreateUserAsync(UserDto userDto)
    {
        return await _repository.AddAsync(new Users(userDto));
    }

}
