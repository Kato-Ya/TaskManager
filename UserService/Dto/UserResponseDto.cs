namespace UserService.Dto;

public class UserResponseDto
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string State { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public List<RoleDto> Roles { get; set; } = new();
}