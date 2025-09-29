namespace DmsContayPerezIPS.Domain.Entities.Security;


public class UserRole
{
    public Guid UserId { get; set; } // Ajusta el tipo si tu UserId no es Guid
    public int RoleId { get; set; }


    public Role Role { get; set; } = null!;
    // public User User { get; set; } = null!; // Si tienes entidad User, agrega la navegación
}