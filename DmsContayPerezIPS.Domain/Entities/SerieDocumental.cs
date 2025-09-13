namespace DmsContayPerezIPS.Domain.Entities
{
    public class SerieDocumental
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = null!;

        public ICollection<SubserieDocumental>? Subseries { get; set; }
    }
}
