using DmsContayPerezIPS.Domain.Entities;
using DmsContayPerezIPS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace DmsContayPerezIPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMinioClient _minio;
        private readonly string _bucket;

        public DocumentosController(AppDbContext db, IMinioClient minio, IConfiguration config)
        {
            _db = db;
            _minio = minio;
            _bucket = config["MinIO:Bucket"] ?? "dms";
        }

        // 🔐 Requiere token JWT
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo inválido");

            var objectName = $"{Guid.NewGuid()}_{file.FileName}";

            using (var stream = file.OpenReadStream())
            {
                await _minio.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType));
            }

            var doc = new Document
            {
                OriginalName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                CurrentVersion = 1,
                CreatedAt = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Archivo subido", docId = doc.Id, objectName });
        }

        // 🔐 Requiere token JWT
        [HttpGet("list")]
        [Authorize]
        public IActionResult ListDocuments()
        {
            var docs = _db.Documents.Select(d => new
            {
                d.Id,
                d.OriginalName,
                d.ContentType,
                d.SizeBytes,
                d.CreatedAt
            });

            return Ok(docs);
        }
    }
}
