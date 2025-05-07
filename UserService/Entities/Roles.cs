using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.Data;
using UserService.Dto;

namespace UserService.Entities;

public class Roles
{
    public Roles(RoleDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Description = dto.Description;
        Users = dto.Users;

    }
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Users> Users { get; set; } = new List<Users>();

}
