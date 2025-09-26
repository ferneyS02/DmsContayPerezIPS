using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace DmsContayPerezIPS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_20250926 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Documents",
                type: "tsvector",
                nullable: false,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .Annotation("Npgsql:TsVectorConfig", "spanish")
                .Annotation("Npgsql:TsVectorProperties", new[] { "SearchText" })
                .OldAnnotation("Npgsql:TsVectorConfig", "spanish")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "SearchText" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Documents",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector")
                .Annotation("Npgsql:TsVectorConfig", "spanish")
                .Annotation("Npgsql:TsVectorProperties", new[] { "SearchText" })
                .OldAnnotation("Npgsql:TsVectorConfig", "spanish")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "SearchText" });
        }
    }
}
