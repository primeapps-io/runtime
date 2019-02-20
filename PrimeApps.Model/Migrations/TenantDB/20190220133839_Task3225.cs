using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3225 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "build_number",
                schema: "public",
                table: "deployments_function",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "revision",
                schema: "public",
                table: "deployments_function",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "build_number",
                schema: "public",
                table: "deployments_component",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "revision",
                schema: "public",
                table: "deployments_component",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "build_number",
                schema: "public",
                table: "deployments_function");

            migrationBuilder.DropColumn(
                name: "revision",
                schema: "public",
                table: "deployments_function");

            migrationBuilder.DropColumn(
                name: "build_number",
                schema: "public",
                table: "deployments_component");

            migrationBuilder.DropColumn(
                name: "revision",
                schema: "public",
                table: "deployments_component");
        }
    }
}
