using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task4202 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * Bu migration Task3595 ile silinen profil_id'yi geri getirmek icin eklendi
             * Test ortamlarina uygulanarak hata giderildi. Task3595'de silme islemi geri 
             * alindigi icin canli ortamlarda uygulanmasina gerek yok. Bundan dolayi comment'lendi.
             * 
            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                schema: "public",
                table: "menu",
                nullable: true);

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
                onDelete: ReferentialAction.Restrict);
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_menu_profiles_profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.DropIndex(
                name: "IX_menu_profile_id",
                schema: "public",
                table: "menu");

            migrationBuilder.DropColumn(
                name: "profile_id",
                schema: "public",
                table: "menu");
            */
        }
    }
}