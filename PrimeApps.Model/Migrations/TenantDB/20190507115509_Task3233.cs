using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3233 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bpm_workflows_modules_module_id",
                schema: "public",
                table: "bpm_workflows");

            migrationBuilder.AlterColumn<string>(
                name: "record_operations",
                schema: "public",
                table: "bpm_workflows",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "module_id",
                schema: "public",
                table: "bpm_workflows",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_bpm_workflows_modules_module_id",
                schema: "public",
                table: "bpm_workflows",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bpm_workflows_modules_module_id",
                schema: "public",
                table: "bpm_workflows");

            migrationBuilder.AlterColumn<string>(
                name: "record_operations",
                schema: "public",
                table: "bpm_workflows",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "module_id",
                schema: "public",
                table: "bpm_workflows",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_bpm_workflows_modules_module_id",
                schema: "public",
                table: "bpm_workflows",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
