namespace UserService.Dto;
public class CreateUserDto
{
    //public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string State { get; set; } = null!;
}
