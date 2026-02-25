using System.ComponentModel.DataAnnotations.Schema;
using UserService.Dto;

namespace UserService.Entities;

[Table("users")]
public class Users
{
    public Users() { }
    public Users(UserDto dto)
    {
        Id = dto.Id;
        Username = dto.Username;
        Email = dto.Email;
        State = dto.State;
        //PasswordHash = dto.PasswordHash;
        //CreatedAt = dto.CreatedAt;
        //Roles = dto.Roles;
        //UserRoles = dto.UserRoles;

    }
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string State { get; set; } = null!;

    //public ICollection<Roles?> Roles { get; set; } = new List< Roles>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserSession> UserSession { get; set; } = new List<UserSession>();

}
