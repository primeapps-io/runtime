using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task2814 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_integration_user",
                schema: "public",
                table: "users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_integration_user",
                schema: "public",
                table: "users");
        }
    }
}
