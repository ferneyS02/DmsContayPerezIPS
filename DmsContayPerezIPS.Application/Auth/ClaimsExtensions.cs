using System.Security.Claims;

namespace DmsContayPerezIPS.Application.Auth;

public static class ClaimsExtensions
{
    public static void AddPermissionClaims(this ClaimsIdentity identity, IEnumerable<string> permissions)
    {
        foreach (var p in permissions.Distinct())
            identity.AddClaim(new Claim("perm", p));
    }
}
