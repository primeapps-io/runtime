using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3252 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deployments",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "public",
                table: "apps");

            migrationBuilder.AddColumn<string>(
                name: "options",
                schema: "public",
                table: "app_settings",
                type: "jsonb",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "packages",
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
                    status = table.Column<int>(nullable: false),
                    version = table.Column<string>(nullable: false),
                    revision = table.Column<int>(nullable: false),
                    start_time = table.Column<DateTime>(nullable: false),
                    end_time = table.Column<DateTime>(nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_packages", x => x.id);
                    table.ForeignKey(
                        name: "FK_packages_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_packages_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_packages_users_updated_by",
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

            migrationBuilder.CreateIndex(
                name: "IX_packages_app_id",
                schema: "public",
                table: "packages",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_packages_created_by",
                schema: "public",
                table: "packages",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_packages_end_time",
                schema: "public",
                table: "packages",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_packages_start_time",
                schema: "public",
                table: "packages",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_packages_status",
                schema: "public",
                table: "packages",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_packages_updated_by",
                schema: "public",
                table: "packages",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_templates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "packages",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "options",
                schema: "public",
                table: "app_settings");

            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "public",
                table: "apps",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "deployments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    app_id = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    created_by = table.Column<int>(nullable: false),
                    deleted = table.Column<bool>(nullable: false),
                    end_time = table.Column<DateTime>(nullable: false),
                    start_time = table.Column<DateTime>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    updated_by = table.Column<int>(nullable: true),
                    version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deployments", x => x.id);
                    table.ForeignKey(
                        name: "FK_deployments_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deployments_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deployments_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deployments_app_id",
                schema: "public",
                table: "deployments",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_created_by",
                schema: "public",
                table: "deployments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_end_time",
                schema: "public",
                table: "deployments",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_start_time",
                schema: "public",
                table: "deployments",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_status",
                schema: "public",
                table: "deployments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_updated_by",
                schema: "public",
                table: "deployments",
                column: "updated_by");
        }
    }
}
