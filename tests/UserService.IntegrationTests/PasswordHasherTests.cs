using System.Security.Cryptography;
using System.Text;
using UserService.PasswordWorker;

namespace UserService.IntegrationTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_DoesNotContainPlainTextAndUsesPbkdf2Format()
    {
        var hash = _hasher.Hash("password");

        Assert.DoesNotContain("password", hash);
        Assert.StartsWith("PBKDF2-SHA256$600000$", hash);
        Assert.Equal(4, hash.Split('$').Length);
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword()
    {
        Assert.NotEqual(_hasher.Hash("password"), _hasher.Hash("password"));
    }

    [Fact]
    public void Verify_AcceptsCorrectPassword()
    {
        var hash = _hasher.Hash("password");

        Assert.Equal(PasswordCheckResult.Success, _hasher.Verify(hash, "password"));
    }

    [Fact]
    public void Verify_RejectsWrongPassword()
    {
        var hash = _hasher.Hash("password");

        Assert.Equal(PasswordCheckResult.Failed, _hasher.Verify(hash, "different-password"));
    }

    [Fact]
    public void Verify_AcceptsLegacySha256AndAsksForRehash()
    {
        var legacy = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("password"))).ToLowerInvariant();

        Assert.Equal(PasswordCheckResult.SuccessRehashNeeded, _hasher.Verify(legacy, "password"));
        Assert.Equal(PasswordCheckResult.Failed, _hasher.Verify(legacy, "different-password"));
    }

    [Fact]
    public void Verify_AsksForRehashWhenIterationsAreOutdated()
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = Rfc2898DeriveBytes.Pbkdf2("password", salt, 10_000, HashAlgorithmName.SHA256, 32);
        var weakHash = $"PBKDF2-SHA256$10000${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";

        Assert.Equal(PasswordCheckResult.SuccessRehashNeeded, _hasher.Verify(weakHash, "password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("PBKDF2-SHA256$abc$xx$yy")]
    [InlineData("PBKDF2-SHA256$600000$not-base64$not-base64")]
    public void Verify_RejectsMalformedHashes(string storedHash)
    {
        Assert.Equal(PasswordCheckResult.Failed, _hasher.Verify(storedHash, "password"));
    }
}
