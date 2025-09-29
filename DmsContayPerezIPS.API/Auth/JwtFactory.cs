using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DmsContayPerezIPS.Application.Auth;
using DmsContayPerezIPS.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DmsContayPerezIPS.API.Auth;

public sealed class JwtFactory
{
    private readonly IConfiguration _config;
    private readonly IUserPermissionService _perms;

    public JwtFactory(IConfiguration config, IUserPermissionService perms)
    {
        _config = config;
        _perms = perms;
    }

    public async Task<string> CreateAsync(Guid userId, string username, IEnumerable<Claim>? extra = null, CancellationToken ct = default)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username)
        };
        if (extra != null) claims.AddRange(extra);

        // Cargar permisos desde BD y agregarlos como claims "perm"
        var perms = await _perms.GetPermissionsAsync(userId, ct);
        var identity = new ClaimsIdentity(claims);
        identity.AddPermissionClaims(perms);
        claims = identity.Claims.ToList();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
