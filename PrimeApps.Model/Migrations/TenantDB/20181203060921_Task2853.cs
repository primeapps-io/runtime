using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task2853 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "public",
                table: "action_buttons",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 15);

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_module_id",
                schema: "public",
                table: "bpm_workflows",
                column: "module_id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bpm_workflows_modules_module_id",
                schema: "public",
                table: "bpm_workflows");

            migrationBuilder.DropIndex(
                name: "IX_bpm_workflows_module_id",
                schema: "public",
                table: "bpm_workflows");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "public",
                table: "action_buttons",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 50);
        }
    }
}
