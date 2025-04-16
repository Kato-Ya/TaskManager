using Microsoft.VisualBasic;
using UserService.Dto;

namespace UserService.Entities;
public class Users
{
    public Users(UserDto dto)
    {
        Id = dto.Id;
        Username = dto.Username;
        Email = dto.Email;
        PasswordHash = dto.PasswordHash;
        CreatedAt = dto.CreatedAt;
        RoleId = dto.RoleId;
        Role = dto.Role;

    }
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int RoleId { get; set; }

    public Roles Role { get; set; } = null!;
}
