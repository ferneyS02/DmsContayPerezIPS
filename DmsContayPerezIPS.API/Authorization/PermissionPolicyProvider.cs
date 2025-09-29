using DmsContayPerezIPS.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DmsContayPerezIPS.API.Authorization;

/// <summary>
/// Permite usar [Authorize(Policy = "perm:documents:read")] creando la policy al vuelo.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        => FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("perm:"))
        {
            var requirement = new PermissionRequirement(policyName.Substring("perm:".Length));
            var policy = new AuthorizationPolicyBuilder().AddRequirements(requirement).Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
