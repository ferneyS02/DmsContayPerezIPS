namespace DmsContayPerezIPS.Domain.Entities
{
    public class TipoDocumental
    {
        public long Id { get; set; }
        public long SubserieId { get; set; }
        public string Nombre { get; set; } = null!;

        // Retención en años (puede ser 0 si aplica solo en una etapa)
        public short RetencionGestion { get; set; } = 0;
        public short RetencionCentral { get; set; } = 0;

        // Disposición final reglamentaria (CT, E, S, M)
        public string DisposicionFinal { get; set; } = "CT";

        public SubserieDocumental? Subserie { get; set; }
    }
}
