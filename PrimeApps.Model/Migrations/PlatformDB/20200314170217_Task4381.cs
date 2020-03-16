using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
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
                name: "IX_app_templates_app_id_system_code_language",
                schema: "public",
                table: "app_templates",
                columns: new[] { "app_id", "system_code", "language" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
