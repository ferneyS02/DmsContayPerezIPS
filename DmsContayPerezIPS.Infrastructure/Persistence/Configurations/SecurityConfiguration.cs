using System.Linq;
using DmsContayPerezIPS.Domain.Entities.Security;
using DmsContayPerezIPS.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DmsContayPerezIPS.Infrastructure.Persistence.Configurations;

public class SecurityConfiguration :
    IEntityTypeConfiguration<Role>,
    IEntityTypeConfiguration<Permission>,
    IEntityTypeConfiguration<RolePermission>,
    IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Slug).IsRequired().HasMaxLength(64);
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);

        b.HasData(SeedRoles());
    }

    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permissions");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Slug).IsRequired().HasMaxLength(64);
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);

        b.HasData(SeedPermissions());
    }

    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("RolePermissions");
        b.HasKey(x => new { x.RoleId, x.PermissionId });
        b.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
        b.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);

        b.HasData(SeedRolePermissionsAdmin());
    }

    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("UserRoles");
        b.HasKey(x => new { x.UserId, x.RoleId });
        b.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
        // Si tienes entidad User en el modelo, agrega la relación acá.
    }

    private static Role[] SeedRoles() => new[]
    {
        new Role { Id = 1, Slug = RoleSlugs.SuperAdmin, Name = "SuperAdmin" },
        new Role { Id = 2, Slug = RoleSlugs.AdminDms, Name = "Admin DMS" },
        new Role { Id = 3, Slug = RoleSlugs.TrdManager, Name = "Responsable TRD" },
        new Role { Id = 4, Slug = RoleSlugs.AreaLead, Name = "Líder de Área" },
        new Role { Id = 5, Slug = RoleSlugs.DocProducer, Name = "Productor" },
        new Role { Id = 6, Slug = RoleSlugs.DocReviewer, Name = "Revisor/Aprobador" },
        new Role { Id = 7, Slug = RoleSlugs.Calidad, Name = "Calidad" },
        new Role { Id = 8, Slug = RoleSlugs.Juridica, Name = "Jurídica" },
        new Role { Id = 9, Slug = RoleSlugs.SgSst, Name = "SG-SST" },
        new Role { Id = 10, Slug = RoleSlugs.Biomed, Name = "Biomédico" },
        new Role { Id = 11, Slug = RoleSlugs.OpsSys, Name = "Sistemas (Ops)" },
        new Role { Id = 12, Slug = RoleSlugs.AuditorInt, Name = "Auditor Interno" },
        new Role { Id = 13, Slug = RoleSlugs.AuditorExt, Name = "Auditor Externo" },
        new Role { Id = 14, Slug = RoleSlugs.AdmisionFact, Name = "Admisiones/Facturación" },
        new Role { Id = 15, Slug = RoleSlugs.ClinicoMed, Name = "Clínico Médico" },
        new Role { Id = 16, Slug = RoleSlugs.ClinicoEnf, Name = "Clínico Enfermería" },
        new Role { Id = 17, Slug = RoleSlugs.SvcIntegrations, Name = "Integraciones (svc)" },
        new Role { Id = 18, Slug = RoleSlugs.SvcRetention, Name = "Retention Worker (svc)" },
        new Role { Id = 19, Slug = RoleSlugs.Guest, Name = "Invitado" }
    };

    private static Permission[] SeedPermissions() => new[]
    {
        new Permission { Id = 1, Slug = PermissionSlugs.DocumentsCreate, Name = "Crear documentos" },
        new Permission { Id = 2, Slug = PermissionSlugs.DocumentsRead, Name = "Leer documentos" },
        new Permission { Id = 3, Slug = PermissionSlugs.DocumentsReadPhi, Name = "Leer PHI" },
        new Permission { Id = 4, Slug = PermissionSlugs.DocumentsReadRedacted, Name = "Leer redactado" },
        new Permission { Id = 5, Slug = PermissionSlugs.DocumentsUpdateMetadata, Name = "Actualizar metadatos" },
        new Permission { Id = 6, Slug = PermissionSlugs.DocumentsVersionCreate, Name = "Nueva versión" },
        new Permission { Id = 7, Slug = PermissionSlugs.DocumentsApprove, Name = "Aprobar" },
        new Permission { Id = 8, Slug = PermissionSlugs.DocumentsPublish, Name = "Publicar" },
        new Permission { Id = 9, Slug = PermissionSlugs.DocumentsObsolete, Name = "Obsoletar" },
        new Permission { Id = 10, Slug = PermissionSlugs.DocumentsArchive, Name = "Archivar" },
        new Permission { Id = 11, Slug = PermissionSlugs.DocumentsDispose, Name = "Disponer (Eliminar)" },
        new Permission { Id = 12, Slug = PermissionSlugs.DocumentsMove, Name = "Reclasificar" },
        new Permission { Id = 13, Slug = PermissionSlugs.DocumentsAclManage, Name = "Gestionar ACL" },
        new Permission { Id = 14, Slug = PermissionSlugs.DocumentsSharePresign, Name = "Compartir (presign)" },
        new Permission { Id = 15, Slug = PermissionSlugs.DocumentsExport, Name = "Exportar" },
        new Permission { Id = 16, Slug = PermissionSlugs.TagsManage, Name = "Tags: gestionar" },
        new Permission { Id = 17, Slug = PermissionSlugs.SeriesManage, Name = "Series/Subseries: gestionar" },
        new Permission { Id = 18, Slug = PermissionSlugs.RetentionHold, Name = "Retention Hold" },
        new Permission { Id = 19, Slug = PermissionSlugs.RetentionLegalHold, Name = "Legal Hold" },
        new Permission { Id = 20, Slug = PermissionSlugs.AuditRead, Name = "Auditoría: leer" },
        new Permission { Id = 21, Slug = PermissionSlugs.UsersManage, Name = "Usuarios: gestionar" },
        new Permission { Id = 22, Slug = PermissionSlugs.RolesManage, Name = "Roles: gestionar" },
        new Permission { Id = 23, Slug = PermissionSlugs.SystemConfigure, Name = "Sistema: configurar" },
        new Permission { Id = 24, Slug = PermissionSlugs.MinioBucketsManage, Name = "MinIO: buckets" },
        new Permission { Id = 25, Slug = PermissionSlugs.SearchAdvanced, Name = "Búsqueda avanzada" }
    };

    private static RolePermission[] SeedRolePermissionsAdmin()
    {
        // Asigna TODOS los permisos al rol admin.dms (Id=2) para iniciar
        var all = Enumerable.Range(1, 25).Select(pid => new RolePermission { RoleId = 2, PermissionId = pid });
        return all.ToArray();
    }
}
