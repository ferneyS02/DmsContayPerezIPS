using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DmsContayPerezIPS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subseries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SerieId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    RetencionGestion = table.Column<int>(type: "integer", nullable: false),
                    RetencionCentral = table.Column<int>(type: "integer", nullable: false),
                    DisposicionFinal = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subseries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subseries_Series_SerieId",
                        column: x => x.SerieId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Entity = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: true),
                    Ts = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Detail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Folders_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumentales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubserieId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    RetencionGestion = table.Column<short>(type: "smallint", nullable: false),
                    RetencionCentral = table.Column<short>(type: "smallint", nullable: false),
                    DisposicionFinal = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumentales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposDocumentales_Subseries_SubserieId",
                        column: x => x.SubserieId,
                        principalTable: "Subseries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FolderId = table.Column<long>(type: "bigint", nullable: true),
                    TipoDocId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentVersion = table.Column<int>(type: "integer", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GestionUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CentralUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipoDocumentalId = table.Column<long>(type: "bigint", nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_TiposDocumentales_TipoDocumentalId",
                        column: x => x.TipoDocumentalId,
                        principalTable: "TiposDocumentales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTags",
                columns: table => new
                {
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTags", x => new { x.DocumentId, x.TagId });
                    table.ForeignKey(
                        name: "FK_DocumentTags_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ObjectKey = table.Column<string>(type: "text", nullable: false),
                    UploadedBy = table.Column<long>(type: "bigint", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploaderId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "Admin" },
                    { 2L, "User" }
                });

            migrationBuilder.InsertData(
                table: "Series",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1L, "Gestión Clínica" },
                    { 2L, "Gestión Administrativa" },
                    { 3L, "Gestión Financiera y Contable" },
                    { 4L, "Gestión Jurídica" },
                    { 5L, "Gestión de Calidad" },
                    { 6L, "SG-SST" },
                    { 7L, "Administración de equipos biomédicos" }
                });

            migrationBuilder.InsertData(
                table: "Subseries",
                columns: new[] { "Id", "DisposicionFinal", "Nombre", "RetencionCentral", "RetencionGestion", "SerieId" },
                values: new object[,]
                {
                    { 1L, "Conservación total", "Historias clínicas", 0, 15, 1L },
                    { 2L, "Eliminación", "Incapacidades médicas", 0, 5, 1L },
                    { 3L, "Conservación total", "Contratos laborales", 10, 10, 2L },
                    { 4L, "Eliminación", "Correspondencia", 0, 2, 2L },
                    { 5L, "Conservación total", "Estados financieros", 0, 20, 3L },
                    { 6L, "Eliminación", "Facturas", 0, 5, 3L },
                    { 7L, "Conservación total", "Procesos judiciales", 20, 10, 4L },
                    { 8L, "Actualización / Sustitución", "Manuales de procesos", 0, 5, 5L },
                    { 9L, "Eliminación", "Registros de calidad", 0, 3, 5L },
                    { 10L, "Conservación total", "Accidentes laborales", 0, 20, 6L },
                    { 11L, "Eliminación", "Capacitaciones SST", 0, 5, 6L },
                    { 12L, "Conservación mientras dure la vida útil", "Hojas de vida de equipos", 0, 0, 7L },
                    { 13L, "Eliminación", "Mantenimientos", 0, 5, 7L }
                });

            migrationBuilder.InsertData(
                table: "TiposDocumentales",
                columns: new[] { "Id", "DisposicionFinal", "Nombre", "RetencionCentral", "RetencionGestion", "SubserieId" },
                values: new object[,]
                {
                    { 1L, "CT", "Historia de ingreso", (short)0, (short)0, 1L },
                    { 2L, "CT", "Notas de evolución", (short)0, (short)0, 1L },
                    { 3L, "CT", "Resultados de laboratorio", (short)0, (short)0, 1L },
                    { 4L, "CT", "Certificado de incapacidad", (short)0, (short)0, 2L },
                    { 5L, "CT", "Soporte médico", (short)0, (short)0, 2L },
                    { 6L, "CT", "Contrato firmado", (short)0, (short)0, 3L },
                    { 7L, "CT", "Acta de terminación", (short)0, (short)0, 3L },
                    { 8L, "CT", "Carta enviada", (short)0, (short)0, 4L },
                    { 9L, "CT", "Carta recibida", (short)0, (short)0, 4L },
                    { 10L, "CT", "Balance general", (short)0, (short)0, 5L },
                    { 11L, "CT", "Estado de resultados", (short)0, (short)0, 5L },
                    { 12L, "CT", "Factura de proveedor", (short)0, (short)0, 6L },
                    { 13L, "CT", "Factura de cliente", (short)0, (short)0, 6L },
                    { 14L, "CT", "Demanda", (short)0, (short)0, 7L },
                    { 15L, "CT", "Sentencia", (short)0, (short)0, 7L },
                    { 16L, "CT", "Manual de calidad", (short)0, (short)0, 8L },
                    { 17L, "CT", "Registro de auditoría interna", (short)0, (short)0, 9L },
                    { 18L, "CT", "Reporte de accidente", (short)0, (short)0, 10L },
                    { 19L, "CT", "Lista de asistencia", (short)0, (short)0, 11L },
                    { 20L, "CT", "Ficha técnica del equipo", (short)0, (short)0, 12L },
                    { 21L, "CT", "Reporte de mantenimiento preventivo", (short)0, (short)0, 13L },
                    { 22L, "CT", "Reporte de mantenimiento correctivo", (short)0, (short)0, 13L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatorId",
                table: "Documents",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FolderId",
                table: "Documents",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TipoDocumentalId",
                table: "Documents",
                column: "TipoDocumentalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTags_TagId",
                table: "DocumentTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId",
                table: "DocumentVersions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_UploaderId",
                table: "DocumentVersions",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CreatorId",
                table: "Folders",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                table: "Folders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subseries_SerieId",
                table: "Subseries",
                column: "SerieId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumentales_SubserieId",
                table: "TiposDocumentales",
                column: "SubserieId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DocumentTags");

            migrationBuilder.DropTable(
                name: "DocumentVersions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "TiposDocumentales");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Subseries");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Series");
        }
    }
}
