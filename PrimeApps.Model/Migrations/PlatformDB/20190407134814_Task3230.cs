using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task3230 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "integration_user_client_id",
                schema: "public",
                table: "users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "secret",
                schema: "public",
                table: "apps",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "integration_user_client_id",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "secret",
                schema: "public",
                table: "apps");
        }
    }
}
