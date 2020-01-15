using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task3254 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_releases_apps_app_id",
                schema: "public",
                table: "releases");

            migrationBuilder.AlterColumn<int>(
                name: "app_id",
                schema: "public",
                table: "releases",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_releases_apps_app_id",
                schema: "public",
                table: "releases",
                column: "app_id",
                principalSchema: "public",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_releases_apps_app_id",
                schema: "public",
                table: "releases");

            migrationBuilder.AlterColumn<int>(
                name: "app_id",
                schema: "public",
                table: "releases",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_releases_apps_app_id",
                schema: "public",
                table: "releases",
                column: "app_id",
                principalSchema: "public",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
