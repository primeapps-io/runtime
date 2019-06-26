using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.StudioDB
{
    public partial class Task3239 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deployments",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "enable_registration",
                schema: "public",
                table: "app_settings");

            migrationBuilder.AddColumn<string>(
                name: "options",
                schema: "public",
                table: "app_settings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "releases",
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
                    start_time = table.Column<DateTime>(nullable: false),
                    end_time = table.Column<DateTime>(nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: true),
                    published = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_releases", x => x.id);
                    table.ForeignKey(
                        name: "FK_releases_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_releases_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_releases_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_releases_app_id",
                schema: "public",
                table: "releases",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_releases_created_by",
                schema: "public",
                table: "releases",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_releases_end_time",
                schema: "public",
                table: "releases",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_releases_published",
                schema: "public",
                table: "releases",
                column: "published");

            migrationBuilder.CreateIndex(
                name: "IX_releases_start_time",
                schema: "public",
                table: "releases",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_releases_status",
                schema: "public",
                table: "releases",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_releases_updated_by",
                schema: "public",
                table: "releases",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "releases",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "options",
                schema: "public",
                table: "app_settings");

            migrationBuilder.AddColumn<bool>(
                name: "enable_registration",
                schema: "public",
                table: "app_settings",
                nullable: false,
                defaultValue: false);

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
                    settings = table.Column<string>(type: "jsonb", nullable: true),
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
