using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Auth;

namespace Common.Auth.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ReturnsNameIdentifier()
    {
        var user = CreatePrincipal("42");

        Assert.Equal(42, user.GetUserId());
    }

    [Fact]
    public void GetUserId_ReturnsJwtSubject()
    {
        var identity = new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "17")],
            "Test");

        Assert.Equal(17, new ClaimsPrincipal(identity).GetUserId());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void GetUserId_ReturnsNullForMissingOrInvalidClaim(string? userId)
    {
        var user = CreatePrincipal(userId);

        Assert.Null(user.GetUserId());
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Manager", true)]
    [InlineData("User", false)]
    public void IsAdminOrManager_UsesRoleClaims(string role, bool expected)
    {
        var user = CreatePrincipal("1", role);

        Assert.Equal(expected, user.IsAdminOrManager());
    }

    [Fact]
    public void CanAccessUser_AllowsOwnData()
    {
        var user = CreatePrincipal("5", "User");

        Assert.True(user.CanAccessUser(5));
    }

    [Fact]
    public void CanAccessUser_AllowsAdminToAccessAnotherUser()
    {
        var user = CreatePrincipal("5", "Admin");

        Assert.True(user.CanAccessUser(9));
    }

    [Fact]
    public void CanAccessUser_DeniesManagerAccessToAnotherUser()
    {
        var user = CreatePrincipal("5", "Manager");

        Assert.False(user.CanAccessUser(9));
    }

    [Fact]
    public void CanAccessManagedUser_AllowsManagerToAccessAnotherUser()
    {
        var user = CreatePrincipal("5", "Manager");

        Assert.True(user.CanAccessManagedUser(9));
    }

    private static ClaimsPrincipal CreatePrincipal(string? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (userId != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
    }
}
