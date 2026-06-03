namespace UserService.Dto;

public class UserSessionDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public DateTime SignInTime { get; set; }

    public DateTime? SignOutTime { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsActive { get; set; }
}
