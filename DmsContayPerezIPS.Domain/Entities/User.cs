using System.Data;

namespace DmsContayPerezIPS.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public long RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
