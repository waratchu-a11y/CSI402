using System.Security.Claims;
using warat_0583_l005.Models;

namespace warat_0583_l005.Services;

public static class RoleAccess
{
    public static bool HasRole(Csi402dbContext database, ClaimsPrincipal principal, params string[] roles)
    {
        string? username = principal.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        string? userRole = database.UserModels
            .Where(user => user.Username == username && user.IsActive)
            .Select(user => user.Role)
            .FirstOrDefault();

        return userRole is not null && roles.Contains(userRole, StringComparer.Ordinal);
    }
}
