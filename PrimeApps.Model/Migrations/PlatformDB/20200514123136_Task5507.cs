using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task5507 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "picklist_language",
                schema: "public",
                table: "app_settings",
                nullable: true,
                defaultValue: "en");

            migrationBuilder.Sql($"UPDATE app_settings SET picklist_language = sq.\"language\" FROM (SELECT DISTINCT te.app_id, ts.\"language\" FROM tenants te JOIN tenant_settings ts ON ts.tenant_id=te.\"id\" WHERE ts.\"language\" IS NOT NULL) AS sq WHERE app_settings.app_id = sq.app_id;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "picklist_language",
                schema: "public",
                table: "app_settings");
        }
    }
}
