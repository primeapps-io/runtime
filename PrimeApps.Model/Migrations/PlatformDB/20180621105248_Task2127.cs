using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task2127 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflows",
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
                    name = table.Column<string>(maxLength: 200, nullable: false),
                    active = table.Column<bool>(nullable: false),
                    frequency = table.Column<int>(nullable: false),
                    operations = table.Column<string>(maxLength: 50, nullable: false),
                    app_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflows_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflows_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflows_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_logs",
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
                    workflow_id = table.Column<int>(nullable: false),
                    app_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_logs_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_logs_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_logs_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_webhooks",
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
                    workflow_id = table.Column<int>(nullable: false),
                    callback_url = table.Column<string>(maxLength: 500, nullable: false),
                    method_type = table.Column<int>(nullable: false),
                    parameters = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_webhooks", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_app_id",
                schema: "public",
                table: "workflow_logs",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_workflow_id",
                schema: "public",
                table: "workflow_logs",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_created_by",
                schema: "public",
                table: "workflow_logs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_updated_by",
                schema: "public",
                table: "workflow_logs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_created_by",
                schema: "public",
                table: "workflow_webhooks",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_updated_by",
                schema: "public",
                table: "workflow_webhooks",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_workflow_id",
                schema: "public",
                table: "workflow_webhooks",
                column: "workflow_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_active",
                schema: "public",
                table: "workflows",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_app_id",
                schema: "public",
                table: "workflows",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_created_by",
                schema: "public",
                table: "workflows",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_updated_by",
                schema: "public",
                table: "workflows",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_webhooks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflows",
                schema: "public");
        }
    }
}
