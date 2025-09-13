namespace DmsContayPerezIPS.Domain.Entities
{
    public class SubserieDocumental
    {
        public long Id { get; set; }

        // 🔗 Relación con Serie Documental
        public long SerieId { get; set; }
        public SerieDocumental? Serie { get; set; }

        // 📌 Nombre de la subserie
        public string Nombre { get; set; } = null!;

        // 📌 Relación con Tipos Documentales
        public ICollection<TipoDocumental>? Tipos { get; set; }

        // =========================
        // 🔹 Campos de TRD
        // =========================

        // Retención en archivo de gestión (en años)
        public int RetencionGestion { get; set; }

        // Retención en archivo central (en años)
        public int RetencionCentral { get; set; }

        // Disposición final (Conservación total, Eliminación, Sustitución, etc.)
        public string DisposicionFinal { get; set; } = "Eliminación";
    }
}
