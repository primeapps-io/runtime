using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3246 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_templates",
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
                    app_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: true),
                    subject = table.Column<string>(maxLength: 200, nullable: true),
                    content = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    type = table.Column<int>(nullable: false),
                    system_code = table.Column<string>(nullable: true),
                    active = table.Column<bool>(nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_app_templates_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_app_templates_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_app_templates_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_app_id",
                schema: "public",
                table: "app_templates",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_created_by",
                schema: "public",
                table: "app_templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_updated_by",
                schema: "public",
                table: "app_templates",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_templates",
                schema: "public");
        }
    }
}
