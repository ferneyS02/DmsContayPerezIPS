namespace DmsContayPerezIPS.Domain.Entities.Security;


public class Permission
{
    public int Id { get; set; }
    public string Slug { get; set; } = null!; // p.ej. "documents:create"
    public string Name { get; set; } = null!;
    public string? Description { get; set; }


    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}