using DmsContayPerezIPS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsContayPerezIPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TRDController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TRDController(AppDbContext db)
        {
            _db = db;
        }

        // 🔹 Devuelve todas las series con sus subseries y tipos documentales
        [HttpGet("series")]
        // [Authorize] // 👉 Descomenta si quieres requerir token JWT
        public async Task<IActionResult> GetSeries()
        {
            var series = await _db.Series
                .Include(s => s.Subseries!)
                    .ThenInclude(ss => ss.Tipos)
                .Select(s => new
                {
                    s.Id,
                    s.Nombre,
                    Subseries = s.Subseries!.Select(ss => new
                    {
                        ss.Id,
                        ss.Nombre,
                        ss.RetencionGestion,
                        ss.RetencionCentral,
                        ss.DisposicionFinal,
                        Tipos = ss.Tipos!.Select(t => new
                        {
                            t.Id,
                            t.Nombre
                        })
                    })
                })
                .ToListAsync();

            return Ok(series);
        }
    }
}
