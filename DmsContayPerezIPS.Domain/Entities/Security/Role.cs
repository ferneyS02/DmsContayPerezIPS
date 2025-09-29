namespace DmsContayPerezIPS.Domain.Entities.Security;


public class Role
{
    public int Id { get; set; }
    public string Slug { get; set; } = null!; // p.ej. "admin.dms"
    public string Name { get; set; } = null!;
    public string? Description { get; set; }


    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}