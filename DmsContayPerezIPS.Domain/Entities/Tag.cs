
namespace DmsContayPerezIPS.Domain.Entities
{
    public class Tag
    {
        public long Id { get; set; }

        // 🏷️ Nombre de la etiqueta
        public string Name { get; set; } = null!;

        // 🔹 Relación con Documentos
        public ICollection<DocumentTag>? DocumentTags { get; set; }
    }
}
