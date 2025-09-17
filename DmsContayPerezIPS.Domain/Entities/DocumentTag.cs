namespace DmsContayPerezIPS.Domain.Entities
{
    public class DocumentTag
    {
        // 🔹 Clave compuesta (DocumentId + TagId)
        public long DocumentId { get; set; }
        public Document Document { get; set; } = null!;

        public long TagId { get; set; }
        public Tag Tag { get; set; } = null!;

        // 🔹 Auditoría (lo configuramos en DbContext)
        public DateTime CreatedAt { get; set; }
    }
}
