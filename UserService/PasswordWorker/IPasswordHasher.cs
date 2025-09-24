namespace UserService.PasswordWorker;
public interface IPasswordHasher
{
    public string Encrypt(string source);
    bool IsPassowrdTrue(string userPassword, string password);
}
