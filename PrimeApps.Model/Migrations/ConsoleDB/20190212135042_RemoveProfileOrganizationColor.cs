using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.ConsoleDB
{
    public partial class RemoveProfileOrganizationColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                schema: "public",
                table: "organizations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "organizations");
        }
    }
}
