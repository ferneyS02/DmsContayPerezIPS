namespace DmsContayPerezIPS.Domain.Entities
{
    public class DocumentVersion
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string ObjectKey { get; set; } = null!;
        public long? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }

        public Document? Document { get; set; }
        public User? Uploader { get; set; }
    }
}
