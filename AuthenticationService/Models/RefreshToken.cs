namespace AuthenticationService.Models;
public class RefreshToken
{
    public RefreshToken(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException("id"); 

        }
        Id = id;
    }

    public string Id { get; set; }
    public int UserId { get; set; }
    public string GrantType { get; set; } = null!;
    public long ExpiresIn { get; set; }
    public bool IsExpired => DateTime.Now > new DateTime(ExpiresIn);
}
