using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3234 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "settings",
                schema: "public",
                table: "deployments",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "enable_registration",
                schema: "public",
                table: "app_settings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings",
                schema: "public",
                table: "deployments");

            migrationBuilder.DropColumn(
                name: "enable_registration",
                schema: "public",
                table: "app_settings");
        }
    }
}
