using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static bool IsOwnerOrAdmin(this ClaimsPrincipal user, string ownerId) =>
        user.GetUserId() == ownerId || user.IsInRole(AppRoles.Admin);
}