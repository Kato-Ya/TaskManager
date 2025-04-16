namespace UserService.Entities;

public class Roles
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description{ get; set; }

    public ICollection<Users> Users { get; set; } = new List<Users>();

}
