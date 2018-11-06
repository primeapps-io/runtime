using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task2485 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bpm_categories",
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
                    order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bpm_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_bpm_categories_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bpm_categories_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bpm_workflows",
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
                    code = table.Column<string>(maxLength: 200, nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    description = table.Column<string>(nullable: true),
                    category_id = table.Column<int>(nullable: true),
                    version = table.Column<int>(nullable: false),
                    active = table.Column<bool>(nullable: false),
                    trigger_type = table.Column<int>(nullable: false),
                    start_time = table.Column<DateTime>(nullable: false),
                    end_time = table.Column<DateTime>(nullable: false),
                    record_operations = table.Column<string>(maxLength: 50, nullable: false),
                    frequency = table.Column<int>(nullable: false),
                    changed_fields = table.Column<string>(maxLength: 4000, nullable: true),
                    can_start_manuel = table.Column<bool>(nullable: false),
                    definition_json = table.Column<string>(nullable: true),
                    diagram_json = table.Column<string>(nullable: true),
                    module_id = table.Column<int>(nullable: false),
                    process_filter = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bpm_workflows", x => x.id);
                    table.ForeignKey(
                        name: "FK_bpm_workflows_bpm_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "bpm_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bpm_workflows_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bpm_workflows_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bpm_record_filters",
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
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    Operator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    No = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bpm_record_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_bpm_record_filters_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bpm_record_filters_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bpm_record_filters_bpm_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "bpm_workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bpm_workflow_logs",
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
                    module_id = table.Column<int>(nullable: false),
                    record_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bpm_workflow_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_bpm_workflow_logs_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bpm_workflow_logs_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bpm_workflow_logs_bpm_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "bpm_workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bpm_categories_created_at",
                schema: "public",
                table: "bpm_categories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_categories_created_by",
                schema: "public",
                table: "bpm_categories",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_categories_deleted",
                schema: "public",
                table: "bpm_categories",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_categories_name",
                schema: "public",
                table: "bpm_categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_categories_updated_at",
                schema: "public",
                table: "bpm_categories",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_categories_updated_by",
                schema: "public",
                table: "bpm_categories",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_record_filters_created_by",
                schema: "public",
                table: "bpm_record_filters",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_record_filters_updated_by",
                schema: "public",
                table: "bpm_record_filters",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_record_filters_workflow_id",
                schema: "public",
                table: "bpm_record_filters",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_created_at",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_created_by",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_deleted",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_module_id",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_record_id",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_updated_at",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_updated_by",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflow_logs_workflow_id",
                schema: "public",
                table: "bpm_workflow_logs",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_active",
                schema: "public",
                table: "bpm_workflows",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_category_id",
                schema: "public",
                table: "bpm_workflows",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_code",
                schema: "public",
                table: "bpm_workflows",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_created_at",
                schema: "public",
                table: "bpm_workflows",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_created_by",
                schema: "public",
                table: "bpm_workflows",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_deleted",
                schema: "public",
                table: "bpm_workflows",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_name",
                schema: "public",
                table: "bpm_workflows",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_updated_at",
                schema: "public",
                table: "bpm_workflows",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_updated_by",
                schema: "public",
                table: "bpm_workflows",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_bpm_workflows_version",
                schema: "public",
                table: "bpm_workflows",
                column: "version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bpm_record_filters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bpm_workflow_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bpm_workflows",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bpm_categories",
                schema: "public");
        }
    }
}
