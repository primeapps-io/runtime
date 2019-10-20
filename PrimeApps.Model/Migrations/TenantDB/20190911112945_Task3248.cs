using Microsoft.EntityFrameworkCore.Migrations;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3248 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "custom_url",
                schema: "public",
                table: "components",
                nullable: true);

            migrationBuilder.Sql("UPDATE components SET \"custom_url\"=(CASE WHEN \"content\" LIKE '{appConfigs%' THEN \"content\" WHEN \"content\" LIKE 'http%' THEN \"content\" ELSE NULL END), \"content\"=(CASE WHEN \"content\" LIKE '{appConfigs%' THEN NULL WHEN \"content\" LIKE 'http%' THEN NULL ELSE \"content\" END) WHERE \"type\"=1");
            migrationBuilder.Sql("UPDATE action_buttons SET \"url\"=(CASE WHEN \"template\" LIKE '{%' THEN \"template\" WHEN \"template\" LIKE 'http%' THEN \"template\" ELSE '' END), \"template\"=(CASE WHEN \"template\" LIKE '{%' THEN 'template' WHEN \"template\" LIKE 'http%' THEN 'template' ELSE \"template\" END) WHERE \"type\"=1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_url",
                schema: "public",
                table: "components");
        }
    }
}
