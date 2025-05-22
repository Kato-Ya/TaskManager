using System.ComponentModel.DataAnnotations.Schema;
using UserService.Entities;

namespace UserService.Entities;

[Table("user_roles")]
public class UserRole
{
    public UserRole() { }

    public int Id { get; set; }

    public int UserId { get; set; }
    public Users User { get; set; } = null!;

    public int RoleId { get; set; }
    public Roles Role { get; set; } = null!;
}