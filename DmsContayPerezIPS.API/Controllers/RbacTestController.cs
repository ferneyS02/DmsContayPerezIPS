#if DEBUG
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DmsContayPerezIPS.Domain.Security.PermissionSlugs;

namespace DmsContayPerezIPS.API.Controllers;

[ApiController]
[Route("dev/rbac")]
public sealed class RbacTestController : ControllerBase
{
    // endpoint libre para saber si la API corre
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { ok = true, env = "DEBUG" });

    // endpoint PROTEGIDO que requiere perm:documents:read
    [HttpGet("secured")]
    [Authorize(Policy = "perm:" + DocumentsRead)]
    public IActionResult Secured() => Ok(new { ok = true, requires = DocumentsRead });
}
#endif
