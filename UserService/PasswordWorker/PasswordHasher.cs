using System.Security.Cryptography;
using System.Text;

namespace UserService.PasswordWorker;
public class PasswordHasher : IPasswordHasher
{
    public string Encrypt(string source)
    {
        using var hashAlg = SHA256.Create();
        return GetHash(hashAlg, source);
    }

    private static string GetHash(HashAlgorithm hashAlg, string source)
    {
        var stringBuilder = new StringBuilder();
        var sourceArray = Encoding.UTF8.GetBytes(source);
        var hashBytes = hashAlg.ComputeHash(sourceArray);

        foreach (var t in hashBytes)
        {
            stringBuilder.Append(t.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    public bool IsPassowrdTrue(string userPassword, string password)
    {
        return Encrypt(password) == userPassword;
    }


}
