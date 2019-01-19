using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.ConsoleDB
{
    public partial class Task3196 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "label",
                schema: "public",
                table: "organizations",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "label",
                schema: "public",
                table: "organizations");
        }
    }
}
