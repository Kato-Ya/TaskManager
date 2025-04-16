using UserService.Entities;

namespace UserService.Dto;
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int RoleId { get; set; }

    public Roles Role { get; set; } = null!;
}
