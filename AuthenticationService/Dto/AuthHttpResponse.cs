namespace AuthenticationService.Dto;
public class AuthHttpResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public long ExpiresIn { get; set; }
}