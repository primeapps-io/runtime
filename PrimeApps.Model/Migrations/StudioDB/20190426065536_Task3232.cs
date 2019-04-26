using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3232 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                schema: "public",
                table: "apps",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "icon",
                schema: "public",
                table: "apps",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "icon",
                schema: "public",
                table: "apps");
        }
    }
}
