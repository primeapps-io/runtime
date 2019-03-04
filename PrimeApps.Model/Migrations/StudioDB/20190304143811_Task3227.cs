using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3227 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organizations_name",
                schema: "public",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_apps_name",
                schema: "public",
                table: "apps");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_name",
                schema: "public",
                table: "organizations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_apps_name",
                schema: "public",
                table: "apps",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organizations_name",
                schema: "public",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_apps_name",
                schema: "public",
                table: "apps");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_name",
                schema: "public",
                table: "organizations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_apps_name",
                schema: "public",
                table: "apps",
                column: "name");
        }
    }
}
