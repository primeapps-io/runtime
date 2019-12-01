using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3469 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "profiles",
                newName: "name_en");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "profiles",
                newName: "description_en");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "action_buttons",
                newName: "name_en");

            migrationBuilder.AddColumn<string>(
                name: "name_tr",
                schema: "public",
                table: "profiles",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description_tr",
                schema: "public",
                table: "profiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_tr",
                schema: "public",
                table: "action_buttons",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "language",
                schema: "public",
                table: "helps",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE profiles SET name_tr = name_en, description_tr = description_en;");
            migrationBuilder.Sql("UPDATE action_buttons SET name_tr = name_en;");
            migrationBuilder.Sql("UPDATE helps SET language = 2;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name_tr",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "description_tr",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "language",
                schema: "public",
                table: "helps");

            migrationBuilder.DropColumn(
                name: "name_tr",
                schema: "public",
                table: "action_buttons");

            migrationBuilder.RenameColumn(
                name: "name_en",
                schema: "public",
                table: "profiles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "description_en",
                schema: "public",
                table: "profiles",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "name_en",
                schema: "public",
                table: "action_buttons",
                newName: "name");
        }
    }
}
