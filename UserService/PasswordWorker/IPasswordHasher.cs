namespace UserService.PasswordWorker;

public enum PasswordCheckResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}

public interface IPasswordHasher
{
    string Hash(string password);
    PasswordCheckResult Verify(string hashedPassword, string providedPassword);
}
