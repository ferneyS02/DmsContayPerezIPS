#if DEBUG
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DmsContayPerezIPS.API.Auth;
using DmsContayPerezIPS.Infrastructure.Persistence;
using DmsContayPerezIPS.Infrastructure.Persistence.Seeds;
using DmsContayPerezIPS.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsContayPerezIPS.API.Controllers;

[ApiController]
[Route("dev/auth")]
public sealed class DevAuthController : ControllerBase
{
    private readonly JwtFactory _jwt;
    private readonly AppDbContext _db;
    private readonly IUserPermissionService _perms;

    public DevAuthController(JwtFactory jwt, AppDbContext db, IUserPermissionService perms)
    {
        _jwt = jwt;
        _db = db;
        _perms = perms;
    }

    public sealed record TokenRequest(
        [Required] Guid UserId,
        [Required] string Username,
        bool MakeAdmin = false // si true, asigna rol admin.dms (Id=2) si no lo tuviera
    );

    [HttpPost("token")]
    public async Task<IActionResult> CreateToken([FromBody] TokenRequest req, CancellationToken ct)
    {
        // (Opcional) asegurar que el user tenga rol admin.dms para probar todo
        if (req.MakeAdmin)
        {
            await _db.EnsureAdminAsync(req.UserId, ct); // requiere AdminSeed helper; si no lo tienes, quita esta línea
        }

        // Claims extra (ejemplo): podrías añadir área/serie para ABAC en el futuro
        var extra = new List<Claim>
        {
            new Claim("app", "dms"),
        };

        var token = await _jwt.CreateAsync(req.UserId, req.Username, extra, ct);
        var perms = await _perms.GetPermissionsAsync(req.UserId, ct);

        return Ok(new
        {
            token,
            req.UserId,
            req.Username,
            permissions = perms
        });
    }
}
#endif
