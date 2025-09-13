namespace DmsContayPerezIPS.Domain.Entities
{
    public class Document
    {
        public long Id { get; set; }
        public string OriginalName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long SizeBytes { get; set; }
        public long? FolderId { get; set; }
        public long TipoDocId { get; set; }
        public int CurrentVersion { get; set; }
        public string? MetadataJson { get; set; }
        public bool IsDeleted { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? GestionUntil { get; set; }
        public DateTime? CentralUntil { get; set; }

        public Folder? Folder { get; set; }
        public TipoDocumental? TipoDocumental { get; set; }
        public User? Creator { get; set; }
        public ICollection<DocumentVersion>? Versions { get; set; }
        public ICollection<DocumentTag>? DocumentTags { get; set; }
    }
}
