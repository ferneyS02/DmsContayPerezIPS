using DmsContayPerezIPS.Domain.Entities;
using DmsContayPerezIPS.Infrastructure.Persistence;
using DmsContayPerezIPS.API.Services; // 👈 para usar SpanishDateParser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using System.Text.Json;

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

        // ==========================================================
        // 🔐 Subida de documentos
        // ==========================================================
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload(IFormFile file, long tipoDocId, string? documentDate = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo inválido");

            // Validar que el TipoDocumental exista
            var tipoDoc = await _db.TiposDocumentales.FindAsync(tipoDocId);
            if (tipoDoc == null)
                return BadRequest("El tipo documental no existe");

            // Verificar/crear bucket en MinIO
            bool bucketExists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
            if (!bucketExists)
                await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));

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

            // ==========================================================
            // ✅ Parseo y normalización de fechas
            // ==========================================================
            DateTime? parsedDocDate = null;
            if (!string.IsNullOrWhiteSpace(documentDate))
            {
                if (SpanishDateParser.TryParse(documentDate, out var parsed))
                {
                    parsedDocDate = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
                }
            }

            var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Crear documento y asignar TipoDocId
            var doc = new Document
            {
                OriginalName = file.FileName,
                ObjectName = objectName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                CurrentVersion = 1,
                CreatedAt = createdAt,
                DocumentDate = parsedDocDate,
                TipoDocId = tipoDocId, // 🔹 Asignación obligatoria
                MetadataJson = JsonSerializer.Serialize(new
                {
                    DocumentDate = parsedDocDate,
                    UploadedBy = User.Identity?.Name ?? "anon"
                })
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Archivo subido", docId = doc.Id, objectName = doc.ObjectName });
        }

        // ==========================================================
        // 🔐 Listado simple
        // ==========================================================
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
                d.CreatedAt,
                d.DocumentDate
            });

            return Ok(docs);
        }

        // ==========================================================
        // 🔐 Descarga de documento
        // ==========================================================
        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> Download(long id)
        {
            var doc = _db.Documents.FirstOrDefault(d => d.Id == id);
            if (doc == null) return NotFound("Documento no encontrado");

            var ms = new MemoryStream();

            await _minio.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(doc.ObjectName)
                .WithCallbackStream(stream => stream.CopyTo(ms)));

            ms.Position = 0;
            return File(ms, doc.ContentType, doc.OriginalName);
        }

        // ==========================================================
        // 🔐 Búsqueda avanzada
        // ==========================================================
        [HttpGet("search")]
        [Authorize]
        public IActionResult Search(
            string? name,
            DateTime? fromUpload,
            DateTime? toUpload,
            string? fromDoc,
            string? toDoc,
            long? serieId,
            long? subserieId,
            long? tipoDocId,
            string? tag,
            string? metadata)
        {
            var query = _db.Documents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d => d.OriginalName.ToLower().Contains(name.ToLower()));

            if (fromUpload.HasValue) query = query.Where(d => d.CreatedAt >= fromUpload.Value);
            if (toUpload.HasValue) query = query.Where(d => d.CreatedAt <= toUpload.Value);

            if (!string.IsNullOrWhiteSpace(fromDoc) && SpanishDateParser.TryParse(fromDoc, out var fdoc))
                query = query.Where(d => d.DocumentDate >= DateTime.SpecifyKind(fdoc, DateTimeKind.Unspecified));

            if (!string.IsNullOrWhiteSpace(toDoc) && SpanishDateParser.TryParse(toDoc, out var tdoc))
                query = query.Where(d => d.DocumentDate <= DateTime.SpecifyKind(tdoc, DateTimeKind.Unspecified));

            if (tipoDocId.HasValue)
                query = query.Where(d => d.TipoDocId == tipoDocId.Value);
            else if (subserieId.HasValue)
                query = query.Where(d => d.TipoDocumental!.SubserieId == subserieId.Value);
            else if (serieId.HasValue)
                query = query.Where(d => d.TipoDocumental!.Subserie!.SerieId == serieId.Value);

            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(d => d.DocumentTags!.Any(dt => dt.Tag.Name.ToLower().Contains(tag.ToLower())));

            if (!string.IsNullOrWhiteSpace(metadata))
                query = query.Where(d => d.MetadataJson != null && d.MetadataJson.ToLower().Contains(metadata.ToLower()));

            var results = query.Select(d => new
            {
                d.Id,
                d.OriginalName,
                d.ContentType,
                d.SizeBytes,
                d.CreatedAt,
                d.DocumentDate,
                Tipo = d.TipoDocumental != null ? d.TipoDocumental.Nombre : null,
                Subserie = d.TipoDocumental!.Subserie != null ? d.TipoDocumental.Subserie.Nombre : null,
                Serie = d.TipoDocumental!.Subserie!.Serie.Nombre,
                Tags = d.DocumentTags!.Select(t => t.Tag.Name).ToList(),
                d.MetadataJson
            }).ToList();

            return Ok(results);
        }
    }
}
