using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class ProfilePicture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_picture",
                schema: "public",
                table: "users",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_picture",
                schema: "public",
                table: "users");
        }
    }
}
