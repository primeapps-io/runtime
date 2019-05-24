using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3236 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_picklists_label_en",
                schema: "public",
                table: "picklists");

            migrationBuilder.DropIndex(
                name: "IX_picklists_label_tr",
                schema: "public",
                table: "picklists");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_picklists_label_en",
                schema: "public",
                table: "picklists",
                column: "label_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_picklists_label_tr",
                schema: "public",
                table: "picklists",
                column: "label_tr",
                unique: true);
        }
    }
}
