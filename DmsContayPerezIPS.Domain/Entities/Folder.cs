
namespace DmsContayPerezIPS.Domain.Entities
{
    public class Folder
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public long? ParentId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public Folder? Parent { get; set; }
        public User? Creator { get; set; }
        public ICollection<Folder>? Subfolders { get; set; }
        public ICollection<Document>? Documents { get; set; }
    }
}
