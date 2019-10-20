using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3246 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "environment",
                schema: "public",
                table: "workflows",
                maxLength: 10,
                nullable: true,
                defaultValue: "3");

            migrationBuilder.AddColumn<string>(
                name: "environment",
                schema: "public",
                table: "processes",
                maxLength: 10,
                nullable: true,
                defaultValue: "3");

            migrationBuilder.AddColumn<string>(
                name: "environment",
                schema: "public",
                table: "functions",
                maxLength: 10,
                nullable: true,
                defaultValue: "3");

            migrationBuilder.AddColumn<string>(
                name: "environment",
                schema: "public",
                table: "components",
                maxLength: 10,
                nullable: true,
                defaultValue: "3");

            migrationBuilder.AddColumn<string>(
                name: "environment",
                schema: "public",
                table: "action_buttons",
                maxLength: 10,
                nullable: true,
                defaultValue: "3");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_environment",
                schema: "public",
                table: "workflows",
                column: "environment");

            migrationBuilder.CreateIndex(
                name: "IX_processes_environment",
                schema: "public",
                table: "processes",
                column: "environment");

            migrationBuilder.CreateIndex(
                name: "IX_functions_environment",
                schema: "public",
                table: "functions",
                column: "environment");

            migrationBuilder.CreateIndex(
                name: "IX_components_environment",
                schema: "public",
                table: "components",
                column: "environment");

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_environment",
                schema: "public",
                table: "action_buttons",
                column: "environment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflows_environment",
                schema: "public",
                table: "workflows");

            migrationBuilder.DropIndex(
                name: "IX_processes_environment",
                schema: "public",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "IX_functions_environment",
                schema: "public",
                table: "functions");

            migrationBuilder.DropIndex(
                name: "IX_components_environment",
                schema: "public",
                table: "components");

            migrationBuilder.DropIndex(
                name: "IX_action_buttons_environment",
                schema: "public",
                table: "action_buttons");

            migrationBuilder.DropColumn(
                name: "environment",
                schema: "public",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "environment",
                schema: "public",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "environment",
                schema: "public",
                table: "functions");

            migrationBuilder.DropColumn(
                name: "environment",
                schema: "public",
                table: "components");

            migrationBuilder.DropColumn(
                name: "environment",
                schema: "public",
                table: "action_buttons");
        }
    }
}