using System.ComponentModel.DataAnnotations.Schema;
using UserService.Dto;

namespace UserService.Entities;

[Table("roles")]
public class Roles
{
    public Roles() { }
    public Roles(RoleDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Description = dto.Description;
        //UserRoles = dto.UserRoles;

    }
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    //public ICollection<Users> Users { get; set; } = new List<Users>();


}
