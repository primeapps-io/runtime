using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3792 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "default",
                schema: "public",
                table: "views",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "system_type",
                schema: "public",
                table: "templates",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "system_type",
                schema: "public",
                table: "roles",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "system_type",
                schema: "public",
                table: "reports",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "system_type",
                schema: "public",
                table: "profiles",
                nullable: false,
                defaultValue: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "default",
                schema: "public",
                table: "views");

            migrationBuilder.DropColumn(
                name: "system_type",
                schema: "public",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "system_type",
                schema: "public",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "system_type",
                schema: "public",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "system_type",
                schema: "public",
                table: "profiles");
        }
    }
}
