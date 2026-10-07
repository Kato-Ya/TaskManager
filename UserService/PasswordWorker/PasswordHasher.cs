using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace UserService.PasswordWorker;
public class PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int SaltSize = 16;          // 128 bit
    private const int KeySize = 32;           // 256 bit
    private const int Iterations = 600_000;   // OWASP recommendation for PBKDF2-HMAC-SHA256
    private static readonly HashAlgorithmName HashName = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashName, KeySize);

        return string.Join('$',
            Algorithm,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public PasswordCheckResult Verify(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || providedPassword == null)
        {
            return PasswordCheckResult.Failed;
        }

        var parts = hashedPassword.Split('$');
        if (parts.Length == 4 && parts[0] == Algorithm)
        {
            return VerifyPbkdf2(parts, providedPassword);
        }

        if (IsLegacySha256(hashedPassword))
        {
            return VerifyLegacySha256(hashedPassword, providedPassword)
                ? PasswordCheckResult.SuccessRehashNeeded
                : PasswordCheckResult.Failed;
        }

        return PasswordCheckResult.Failed;
    }

    private static PasswordCheckResult VerifyPbkdf2(string[] parts, string password)
    {
        if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations <= 0)
        {
            return PasswordCheckResult.Failed;
        }

        byte[] salt;
        byte[] expectedKey;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedKey = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return PasswordCheckResult.Failed;
        }

        if (salt.Length == 0 || expectedKey.Length == 0)
        {
            return PasswordCheckResult.Failed;
        }

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashName, expectedKey.Length);

        if (!CryptographicOperations.FixedTimeEquals(actualKey, expectedKey))
        {
            return PasswordCheckResult.Failed;
        }

        return iterations < Iterations
            ? PasswordCheckResult.SuccessRehashNeeded
            : PasswordCheckResult.Success;
    }

    private static bool IsLegacySha256(string hash) => hash.Length == 64 && hash.All(Uri.IsHexDigit);

    private static bool VerifyLegacySha256(string storedHex, string password)
    {
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var expected = Convert.FromHexString(storedHex);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
