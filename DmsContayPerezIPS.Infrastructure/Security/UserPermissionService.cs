using DmsContayPerezIPS.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace DmsContayPerezIPS.Infrastructure.Security;

public interface IUserPermissionService
{
    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default);
}

public sealed class UserPermissionService : IUserPermissionService
{
    private readonly Persistence.AppDbContext _db;
    public UserPermissionService(Persistence.AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var q = from ur in _db.Set<UserRole>().AsNoTracking()
                join rp in _db.Set<RolePermission>().AsNoTracking() on ur.RoleId equals rp.RoleId
                join p in _db.Set<Permission>().AsNoTracking() on rp.PermissionId equals p.Id
                where ur.UserId == userId
                select p.Slug;
        return await q.Distinct().ToListAsync(ct);
    }
}
