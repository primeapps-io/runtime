using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.ConsoleDB
{
    public partial class ProfileAndColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_id",
                schema: "public",
                table: "app_collaborators");

            migrationBuilder.AddColumn<string>(
                name: "color",
                schema: "public",
                table: "organizations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "profile",
                schema: "public",
                table: "app_collaborators",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "profile",
                schema: "public",
                table: "app_collaborators");

            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                schema: "public",
                table: "app_collaborators",
                nullable: false,
                defaultValue: 0);
        }
    }
}
