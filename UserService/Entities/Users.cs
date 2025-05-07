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
        Roles = dto.Roles;

    }
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ICollection<Roles?> Roles { get; set; } = new List< Roles>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
