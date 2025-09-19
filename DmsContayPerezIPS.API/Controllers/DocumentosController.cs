using System.Text.Json;
using DmsContayPerezIPS.Domain.Entities;
using DmsContayPerezIPS.Infrastructure.Persistence;
using DmsContayPerezIPS.API.Services;                 // ITextExtractor + (SpanishDateParser si lo tienes aquí)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;                  // Include/Where/EF.Functions
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
        private readonly ITextExtractor _textExtractor;   // ✅ extractor

        public DocumentosController(
            AppDbContext db,
            IMinioClient minio,
            IConfiguration config,
            ITextExtractor textExtractor)                 // ✅ inyección por ctor
        {
            _db = db;
            _minio = minio;
            _bucket = config["MinIO:Bucket"] ?? "dms";
            _textExtractor = textExtractor;               // ✅ guardar ref
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

            // Subir a MinIO
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
                    parsedDocDate = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
            }

            var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // ==========================================================
            // ✅ EXTRAER TEXTO PARA FTS (PDF/DOCX) — ANTES de crear la entidad
            // ==========================================================
            var extractedText = await _textExtractor.ExtractAsync(file, HttpContext.RequestAborted);

            // Crear documento y asignar TipoDocId + SearchText
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
                SearchText = extractedText, // 🔹 Clave para Full-Text Search
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
        // 🔐 Búsqueda avanzada (por metadatos/TRD/tags)
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

            if (fromUpload.HasValue)
                query = query.Where(d => d.CreatedAt >= fromUpload.Value);

            if (toUpload.HasValue)
                query = query.Where(d => d.CreatedAt <= toUpload.Value);

            if (!string.IsNullOrWhiteSpace(fromDoc) && SpanishDateParser.TryParse(fromDoc, out var fdoc))
                query = query.Where(d => d.DocumentDate >= DateTime.SpecifyKind(fdoc, DateTimeKind.Unspecified));

            if (!string.IsNullOrWhiteSpace(toDoc) && SpanishDateParser.TryParse(toDoc, out var tdoc))
                query = query.Where(d => d.DocumentDate <= DateTime.SpecifyKind(tdoc, DateTimeKind.Unspecified));

            if (tipoDocId.HasValue)
                query = query.Where(d => d.TipoDocId == tipoDocId.Value);
            else if (subserieId.HasValue)
                query = query.Where(d => d.TipoDocumental != null && d.TipoDocumental.SubserieId == subserieId.Value);
            else if (serieId.HasValue)
                query = query.Where(d => d.TipoDocumental != null && d.TipoDocumental.Subserie != null && d.TipoDocumental.Subserie.SerieId == serieId.Value);

            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(d => d.DocumentTags != null && d.DocumentTags.Any(dt => dt.Tag.Name.ToLower().Contains(tag.ToLower())));

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
                Tipo = d.TipoDocumental == null ? null : d.TipoDocumental.Nombre,
                Subserie = d.TipoDocumental != null && d.TipoDocumental.Subserie != null ? d.TipoDocumental.Subserie.Nombre : null,
                Serie = d.TipoDocumental != null && d.TipoDocumental.Subserie != null && d.TipoDocumental.Subserie.Serie != null
                    ? d.TipoDocumental.Subserie.Serie.Nombre
                    : null,
                Tags = d.DocumentTags != null ? d.DocumentTags.Select(t => t.Tag.Name).ToList() : new List<string>(),
                d.MetadataJson
            }).ToList();

            return Ok(results);
        }

        // ==========================================================
        // 🔐 Full-Text Search (tsvector español + índice GIN)
        // ==========================================================
        [HttpGet("fulltext")]
        [Authorize]
        public async Task<IActionResult> FullTextSearch([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Ingrese una consulta (q).");

            // Usa la columna generada SearchVector con diccionario 'spanish'
            var results = await _db.Documents
                .Where(d => d.SearchVector != null &&
                            d.SearchVector.Matches(EF.Functions.PlainToTsQuery("spanish", q)))
                .Select(d => new
                {
                    d.Id,
                    d.OriginalName,
                    d.ContentType,
                    d.SizeBytes,
                    d.CreatedAt,
                    d.DocumentDate,
                    // snippet simple
                    Snippet = d.SearchText != null && d.SearchText.Length > 240
                        ? d.SearchText.Substring(0, 240) + "..."
                        : d.SearchText
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
