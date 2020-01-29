using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3967 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "close_smtp_settings",
                schema: "public",
                table: "profiles");

            migrationBuilder.AddColumn<bool>(
                name: "smtp_settings",
                schema: "public",
                table: "profiles",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "smtp_settings",
                schema: "public",
                table: "profiles");

            migrationBuilder.AddColumn<bool>(
                name: "close_smtp_settings",
                schema: "public",
                table: "profiles",
                nullable: false,
                defaultValue: false);
        }
    }
}
