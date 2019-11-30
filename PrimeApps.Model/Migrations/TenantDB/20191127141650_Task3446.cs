using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3446 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "widgets",
                newName: "name_tr");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "reports",
                newName: "name_tr");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "report_categories",
                newName: "name_tr");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "dashlets",
                newName: "name_tr");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "dashboard",
                newName: "name_tr");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "dashboard",
                newName: "description_tr");

            migrationBuilder.RenameColumn(
                name: "y_axis_name",
                schema: "public",
                table: "charts",
                newName: "y_axis_name_tr");

            migrationBuilder.RenameColumn(
                name: "x_axis_name",
                schema: "public",
                table: "charts",
                newName: "x_axis_name_tr");

            migrationBuilder.RenameColumn(
                name: "sub_caption",
                schema: "public",
                table: "charts",
                newName: "sub_caption_tr");

            migrationBuilder.RenameColumn(
                name: "caption",
                schema: "public",
                table: "charts",
                newName: "caption_tr");

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "public",
                table: "widgets",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "public",
                table: "reports",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "public",
                table: "report_categories",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "public",
                table: "dashlets",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description_en",
                schema: "public",
                table: "dashboard",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "public",
                table: "dashboard",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "caption_en",
                schema: "public",
                table: "charts",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sub_caption_en",
                schema: "public",
                table: "charts",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "x_axis_name_en",
                schema: "public",
                table: "charts",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "y_axis_name_en",
                schema: "public",
                table: "charts",
                maxLength: 80,
                nullable: false,
                defaultValue: "");
            
            
            migrationBuilder.Sql("UPDATE reports  SET name_en = name_tr;");
            migrationBuilder.Sql("UPDATE report_categories  SET name_en = name_tr;");
            migrationBuilder.Sql("UPDATE widgets  SET name_en = name_tr;");
            migrationBuilder.Sql("UPDATE dashboard  SET name_en = name_tr;");
            migrationBuilder.Sql("UPDATE dashboard  SET description_en = description_tr;");
            migrationBuilder.Sql("UPDATE dashlets  SET name_en = name_tr;");
            migrationBuilder.Sql("UPDATE charts  SET caption_en = caption_tr;");
            migrationBuilder.Sql("UPDATE charts  SET sub_caption_en = sub_caption_tr;");
            migrationBuilder.Sql("UPDATE charts  SET y_axis_name_en = y_axis_name_tr;");
            migrationBuilder.Sql("UPDATE charts  SET x_axis_name_en = x_axis_name_tr;");
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "public",
                table: "widgets");

            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "public",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "public",
                table: "report_categories");

            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "public",
                table: "dashlets");

            migrationBuilder.DropColumn(
                name: "description_en",
                schema: "public",
                table: "dashboard");

            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "public",
                table: "dashboard");

            migrationBuilder.DropColumn(
                name: "caption_en",
                schema: "public",
                table: "charts");

            migrationBuilder.DropColumn(
                name: "sub_caption_en",
                schema: "public",
                table: "charts");

            migrationBuilder.DropColumn(
                name: "x_axis_name_en",
                schema: "public",
                table: "charts");

            migrationBuilder.DropColumn(
                name: "x_axis_name_tr",
                schema: "public",
                table: "charts");

            migrationBuilder.RenameColumn(
                name: "name_tr",
                schema: "public",
                table: "widgets",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "name_tr",
                schema: "public",
                table: "reports",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "name_tr",
                schema: "public",
                table: "report_categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "name_tr",
                schema: "public",
                table: "dashlets",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "name_tr",
                schema: "public",
                table: "dashboard",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "description_tr",
                schema: "public",
                table: "dashboard",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "y_axis_name",
                schema: "public",
                table: "charts",
                newName: "y_axis_name_tr");

            migrationBuilder.RenameColumn(
                name: "x_axis_name",
                schema: "public",
                table: "charts",
                newName: "x_axis_name_tr");

            migrationBuilder.RenameColumn(
                name: "sub_caption_tr",
                schema: "public",
                table: "charts",
                newName: "sub_caption");

            migrationBuilder.RenameColumn(
                name: "caption_tr",
                schema: "public",
                table: "charts",
                newName: "caption");
        }
    }
}
