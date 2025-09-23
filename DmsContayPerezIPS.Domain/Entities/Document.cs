using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace DmsContayPerezIPS.Domain.Entities
{
    public class Document
    {
        public long Id { get; set; }

        // 🔹 Datos básicos
        public string OriginalName { get; set; } = null!;   // Nombre original del archivo
        public string ObjectName { get; set; } = null!;   // Nombre real en MinIO
        public string ContentType { get; set; } = null!;
        public long SizeBytes { get; set; }

        // 🔹 Organización en carpetas / TRD
        public long? FolderId { get; set; }
        public long TipoDocId { get; set; }  // FK obligatoria a TipoDocumental

        // 🔹 Control de versiones
        public int CurrentVersion { get; set; } = 1;

        // 🔹 Metadatos dinámicos en JSON
        public string? MetadataJson { get; set; }

        // 🔹 Texto extraído (si quieres conservarlo adicionalmente)
        public string? ExtractedText { get; set; }

        // 🔹 Borrado lógico
        public bool IsDeleted { get; set; } = false;

        // 🔹 Auditoría
        public long? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // 🔹 Fechas de retención documental
        public DateTime? GestionUntil { get; set; }
        public DateTime? CentralUntil { get; set; }

        // 🔹 Fecha oficial del documento
        public DateTime? DocumentDate { get; set; }

        // 🔹 FTS (búsqueda de texto completo en PostgreSQL)
        /// <summary>Texto plano para FTS. Mejor guardar "" en vez de null.</summary>
        public string SearchText { get; set; } = string.Empty;

        /// <summary>Columna tsvector generada por PostgreSQL.</summary>
        public NpgsqlTsVector SearchVector { get; private set; } = default!;

        // ==========================================================
        // 🔹 Relaciones
        // ==========================================================
        public Folder? Folder { get; set; }
        public TipoDocumental? TipoDocumental { get; set; }
        public User? Creator { get; set; }

        public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
        public ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    }
}
