using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicManagement.Migrations
{
    /// <inheritdoc />
    public partial class CatalogLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);

            // Limbile implicite de instruire (RO, RU, EN). Idempotent: nu dublează limbile deja existente.
            migrationBuilder.Sql(@"
                INSERT INTO ""Languages"" (""Id"", ""Code"", ""Name"")
                SELECT gen_random_uuid(), v.code, v.name
                FROM (VALUES ('RO', 'Română'), ('RU', 'Rusă'), ('EN', 'Engleză')) AS v(code, name)
                WHERE NOT EXISTS (SELECT 1 FROM ""Languages"" l WHERE l.""Code"" = v.code);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Languages_Code",
                table: "Languages");
        }
    }
}
