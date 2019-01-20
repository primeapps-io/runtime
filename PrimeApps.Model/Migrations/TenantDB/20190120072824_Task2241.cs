using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task2241 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "encrypted",
                schema: "public",
                table: "fields",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "encryption_authorized_users",
                schema: "public",
                table: "fields",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "encrypted",
                schema: "public",
                table: "fields");

            migrationBuilder.DropColumn(
                name: "encryption_authorized_users",
                schema: "public",
                table: "fields");
        }
    }
}
