using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task3247 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                schema: "public",
                table: "releases",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_releases_tenant_id",
                schema: "public",
                table: "releases",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "FK_releases_tenants_tenant_id",
                schema: "public",
                table: "releases",
                column: "tenant_id",
                principalSchema: "public",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_releases_tenants_tenant_id",
                schema: "public",
                table: "releases");

            migrationBuilder.DropIndex(
                name: "IX_releases_tenant_id",
                schema: "public",
                table: "releases");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "public",
                table: "releases");
        }
    }
}
