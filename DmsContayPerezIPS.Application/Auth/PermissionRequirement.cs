using Microsoft.AspNetCore.Authorization;

namespace DmsContayPerezIPS.Application.Auth;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
