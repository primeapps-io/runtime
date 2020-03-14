using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task4381 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_app_templates_app_id",
                schema: "public",
                table: "app_templates");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_active",
                schema: "public",
                table: "app_templates",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_language",
                schema: "public",
                table: "app_templates",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_name",
                schema: "public",
                table: "app_templates",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_system_code",
                schema: "public",
                table: "app_templates",
                column: "system_code");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_type",
                schema: "public",
                table: "app_templates",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_app_id_system_code_language",
                schema: "public",
                table: "app_templates",
                columns: new[] { "app_id", "system_code", "language" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_app_templates_active",
                schema: "public",
                table: "app_templates");

            migrationBuilder.DropIndex(
                name: "IX_app_templates_language",
                schema: "public",
                table: "app_templates");

            migrationBuilder.DropIndex(
                name: "IX_app_templates_name",
                schema: "public",
                table: "app_templates");

            migrationBuilder.DropIndex(
                name: "IX_app_templates_system_code",
                schema: "public",
                table: "app_templates");

            migrationBuilder.DropIndex(
                name: "IX_app_templates_type",
                schema: "public",
                table: "app_templates");

            migrationBuilder.DropIndex(
                name: "IX_app_templates_app_id_system_code_language",
                schema: "public",
                table: "app_templates");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_app_id",
                schema: "public",
                table: "app_templates",
                column: "app_id");
        }
    }
}
