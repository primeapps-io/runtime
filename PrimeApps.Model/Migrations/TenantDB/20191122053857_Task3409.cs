using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3409 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_public.components_public.modules_module_id",
                schema: "public",
                table: "components");

            migrationBuilder.AlterColumn<int>(
                name: "module_id",
                schema: "public",
                table: "components",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_components_modules_module_id",
                schema: "public",
                table: "components",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_components_modules_module_id",
                schema: "public",
                table: "components");

            migrationBuilder.AlterColumn<int>(
                name: "module_id",
                schema: "public",
                table: "components",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_public.components_public.modules_module_id",
                schema: "public",
                table: "components",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
