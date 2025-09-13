namespace DmsContayPerezIPS.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public long? EntityId { get; set; }
        public DateTime Ts { get; set; }
        public string? Detail { get; set; }

        public User? User { get; set; }
    }
}
