using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task3245 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    settings = table.Column<string>(type: "jsonb", nullable: true)
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
        }
    }
}
