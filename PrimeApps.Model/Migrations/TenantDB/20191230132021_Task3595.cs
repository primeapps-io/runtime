using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3595 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_public.menu_public.profiles_profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.DropIndex(
                name: "menu_IX_profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.DropColumn(
                name: "profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.AddColumn<string>(
                name: "profiles",
                schema: "public",
                table: "menu",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profiles",
                schema: "public",
                table: "menu");

            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                schema: "public",
                table: "menu",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_menu_profile_id",
                schema: "public",
                table: "menu",
                column: "profile_id");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_profiles_profile_id",
                schema: "public",
                table: "menu",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
