using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class FunctionComponent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "components",
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
                    name = table.Column<string>(maxLength: 15, nullable: false),
                    content = table.Column<string>(nullable: true),
                    status = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false),
                    place = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: false),
                    order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_components", x => x.id);
                    table.ForeignKey(
                        name: "FK_components_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_components_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "functions",
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
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    label = table.Column<string>(maxLength: 300, nullable: false),
                    dependencies = table.Column<string>(nullable: true),
                    content = table.Column<string>(nullable: true),
                    handler = table.Column<string>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    runtime = table.Column<int>(nullable: false),
                    content_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_functions", x => x.id);
                    table.ForeignKey(
                        name: "FK_functions_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_functions_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_components_created_by",
                schema: "public",
                table: "components",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_components_module_id",
                schema: "public",
                table: "components",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_components_name",
                schema: "public",
                table: "components",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_components_place",
                schema: "public",
                table: "components",
                column: "place");

            migrationBuilder.CreateIndex(
                name: "IX_components_status",
                schema: "public",
                table: "components",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_components_type",
                schema: "public",
                table: "components",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_components_updated_by",
                schema: "public",
                table: "components",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_functions_created_by",
                schema: "public",
                table: "functions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_functions_handler",
                schema: "public",
                table: "functions",
                column: "handler");

            migrationBuilder.CreateIndex(
                name: "IX_functions_name",
                schema: "public",
                table: "functions",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_functions_runtime",
                schema: "public",
                table: "functions",
                column: "runtime");

            migrationBuilder.CreateIndex(
                name: "IX_functions_status",
                schema: "public",
                table: "functions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_functions_updated_by",
                schema: "public",
                table: "functions",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "components",
                schema: "public");

            migrationBuilder.DropTable(
                name: "functions",
                schema: "public");
        }
    }
}
