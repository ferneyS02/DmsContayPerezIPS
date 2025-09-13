namespace DmsContayPerezIPS.Domain.Entities
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<DocumentTag>? DocumentTags { get; set; }
    }
}
