using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3237 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tag",
                schema: "public",
                table: "history_storage",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tag",
                schema: "public",
                table: "history_database",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_history_storage_tag",
                schema: "public",
                table: "history_storage",
                column: "tag");

            migrationBuilder.CreateIndex(
                name: "IX_history_storage_unique_name",
                schema: "public",
                table: "history_storage",
                column: "unique_name");

            migrationBuilder.CreateIndex(
                name: "IX_history_database_table_name",
                schema: "public",
                table: "history_database",
                column: "table_name");

            migrationBuilder.CreateIndex(
                name: "IX_history_database_tag",
                schema: "public",
                table: "history_database",
                column: "tag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_history_storage_tag",
                schema: "public",
                table: "history_storage");

            migrationBuilder.DropIndex(
                name: "IX_history_storage_unique_name",
                schema: "public",
                table: "history_storage");

            migrationBuilder.DropIndex(
                name: "IX_history_database_table_name",
                schema: "public",
                table: "history_database");

            migrationBuilder.DropIndex(
                name: "IX_history_database_tag",
                schema: "public",
                table: "history_database");

            migrationBuilder.DropColumn(
                name: "tag",
                schema: "public",
                table: "history_storage");

            migrationBuilder.DropColumn(
                name: "tag",
                schema: "public",
                table: "history_database");
        }
    }
}
