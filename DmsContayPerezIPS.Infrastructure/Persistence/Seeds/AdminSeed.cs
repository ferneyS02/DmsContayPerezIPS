using DmsContayPerezIPS.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace DmsContayPerezIPS.Infrastructure.Persistence.Seeds;

public static class AdminSeed
{
    public static async Task EnsureAdminAsync(this AppDbContext db, Guid adminUserId, CancellationToken ct = default)
    {
        var exists = await db.Set<UserRole>().AnyAsync(x => x.UserId == adminUserId && x.RoleId == 2, ct);
        if (!exists)
        {
            await db.Set<UserRole>().AddAsync(new UserRole { UserId = adminUserId, RoleId = 2 }, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
