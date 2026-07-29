using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Common.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue("sub");

        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    public static bool IsAdminOrManager(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin") || user.IsInRole("Manager");
    }

    public static bool AccessUser(this ClaimsPrincipal user, int userId)
    {
        return user.IsAccessUser(userId);
    }

    public static bool AccessManagedUser(this ClaimsPrincipal user, int userId)
    {
        return user.IsAccessManagedUser(userId);
    }

    public static bool CanAccessUser(this ClaimsPrincipal user, int userId)
    {
        return user.IsAccessUser(userId);
    }

    public static bool CanAccessManagedUser(this ClaimsPrincipal user, int userId)
    {
        return user.IsAccessManagedUser(userId);
    }

    public static bool IsAccessUser(this ClaimsPrincipal user, int userId)
    {
        var currentUserId = user.GetUserId();
        return currentUserId == userId || user.IsInRole("Admin");
    }

    public static bool IsAccessManagedUser(this ClaimsPrincipal user, int userId)
    {
        var currentUserId = user.GetUserId();
        return currentUserId == userId || user.IsAdminOrManager();
    }
}
