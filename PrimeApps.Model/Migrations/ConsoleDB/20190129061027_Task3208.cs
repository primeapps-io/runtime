using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.ConsoleDB
{
    public partial class Task3208 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "default",
                schema: "public",
                table: "organizations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_organizations_label",
                schema: "public",
                table: "organizations",
                column: "label");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organizations_label",
                schema: "public",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "default",
                schema: "public",
                table: "organizations");
        }
    }
}
