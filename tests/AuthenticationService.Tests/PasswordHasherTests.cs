using PasswordHasherImplementation = AuthenticationService.PasswordHasher.PasswordHasher;

namespace AuthenticationService.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasherImplementation _hasher = new();

    [Fact]
    public void Encrypt_DoesNotReturnPlainText()
    {
        Assert.NotEqual("password", _hasher.Encrypt("password"));
    }

    [Fact]
    public void IsPassowrdTrue_AcceptsMatchingPassword()
    {
        var hash = _hasher.Encrypt("password");

        Assert.True(_hasher.IsPassowrdTrue(hash, "password"));
    }

    [Fact]
    public void IsPassowrdTrue_RejectsDifferentPassword()
    {
        var hash = _hasher.Encrypt("password");

        Assert.False(_hasher.IsPassowrdTrue(hash, "different-password"));
    }
}
