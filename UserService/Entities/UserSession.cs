using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Dto;

namespace UserService.Entities;

[Table("user_session")]
public class UserSession
{
    public int Id { get; set; }
    public DateTime SigninTime { get; set; } = DateTime.UtcNow;
    public DateTime? SignoutTime { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; } = true;
    public int UserId { get; set; }
    public Users User { get; set; } = null!;

    /*public ICollection<Users> Users { get; set; } = new List<Users>();*/
}
