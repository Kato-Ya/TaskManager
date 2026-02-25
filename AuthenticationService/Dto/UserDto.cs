namespace AuthenticationService.Dto;
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string State { get; set; } = null!;

    //public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public List<string> Roles { get; set; } = new();

    //public ICollection<Roles?> Roles { get; set; } = new List<Roles?>();
}
