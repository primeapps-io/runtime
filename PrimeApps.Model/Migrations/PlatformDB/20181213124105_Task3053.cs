using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task3053 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mail_sender_email",
                schema: "public",
                table: "app_templates");

            migrationBuilder.DropColumn(
                name: "mail_sender_name",
                schema: "public",
                table: "app_templates");

            migrationBuilder.AddColumn<string>(
                name: "settings",
                schema: "public",
                table: "app_templates",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings",
                schema: "public",
                table: "app_templates");

            migrationBuilder.AddColumn<string>(
                name: "mail_sender_email",
                schema: "public",
                table: "app_templates",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mail_sender_name",
                schema: "public",
                table: "app_templates",
                nullable: true);
        }
    }
}
