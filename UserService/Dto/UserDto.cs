using UserService.Entities;

namespace UserService.Dto;
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    //public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    //public ICollection<Roles?> Roles { get; set; } = new List<Roles?>();
}
