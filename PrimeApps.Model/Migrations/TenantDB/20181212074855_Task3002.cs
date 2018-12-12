using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "import_maps",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    module_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    skip = table.Column<bool>(nullable: false),
                    mapping = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_maps", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_maps_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_import_maps_modules_module_id",
                        column: x => x.module_id,
                        principalSchema: "public",
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_import_maps_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_created_at",
                schema: "public",
                table: "import_maps",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_created_by",
                schema: "public",
                table: "import_maps",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_deleted",
                schema: "public",
                table: "import_maps",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_module_id",
                schema: "public",
                table: "import_maps",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_name",
                schema: "public",
                table: "import_maps",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_updated_at",
                schema: "public",
                table: "import_maps",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_import_maps_updated_by",
                schema: "public",
                table: "import_maps",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_maps",
                schema: "public");
        }
    }
}
