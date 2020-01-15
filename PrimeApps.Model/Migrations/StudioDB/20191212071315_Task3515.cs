using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3515 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "secret",
                schema: "public",
                table: "apps");
        }
    }
}
