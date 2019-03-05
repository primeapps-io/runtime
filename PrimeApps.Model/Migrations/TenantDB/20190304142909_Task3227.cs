using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3227 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "system_code",
                schema: "public",
                table: "picklists",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "label",
                schema: "public",
                table: "components",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_picklists_system_code",
                schema: "public",
                table: "picklists",
                column: "system_code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_picklists_system_code",
                schema: "public",
                table: "picklists");

            migrationBuilder.DropColumn(
                name: "system_code",
                schema: "public",
                table: "picklists");

            migrationBuilder.DropColumn(
                name: "label",
                schema: "public",
                table: "components");
        }
    }
}
