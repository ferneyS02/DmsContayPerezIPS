using Microsoft.AspNetCore.Authorization;

namespace DmsContayPerezIPS.Application.Auth;

public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var has = context.User?.Claims?.Any(c => c.Type == "perm" && c.Value == requirement.Permission) == true;
        if (has) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
