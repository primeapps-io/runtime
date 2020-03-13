using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task4202 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_profiles_profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.AlterColumn<int>(
                name: "profile_id",
                schema: "public",
                table: "menu",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_menu_profiles_profile_id",
                schema: "public",
                table: "menu",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_profiles_profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.AlterColumn<int>(
                name: "profile_id",
                schema: "public",
                table: "menu",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

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
