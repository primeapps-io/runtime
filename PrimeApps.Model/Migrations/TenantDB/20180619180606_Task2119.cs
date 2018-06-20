using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task2119 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "public",
                table: "templates",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_templates_id_code",
                schema: "public",
                table: "templates",
                columns: new[] { "id", "code" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_templates_id_code",
                schema: "public",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "code",
                schema: "public",
                table: "templates");
        }
    }
}
