using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task2886 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "public",
                table: "components",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "deployments_component",
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
                    component_id = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    publish_status = table.Column<int>(nullable: false),
                    version = table.Column<string>(nullable: false),
                    start_time = table.Column<DateTime>(nullable: false),
                    end_time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deployments_component", x => x.id);
                    table.ForeignKey(
                        name: "FK_deployments_component_components_component_id",
                        column: x => x.component_id,
                        principalSchema: "public",
                        principalTable: "components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deployments_component_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deployments_component_users_updated_by",
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
                    label = table.Column<string>(maxLength: 300, nullable: true),
                    dependencies = table.Column<string>(nullable: true),
                    content = table.Column<string>(nullable: true),
                    handler = table.Column<string>(nullable: false),
                    runtime = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "deployments_function",
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
                    function_id = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    publish_status = table.Column<int>(nullable: false),
                    version = table.Column<string>(nullable: false),
                    start_time = table.Column<DateTime>(nullable: false),
                    end_time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deployments_function", x => x.id);
                    table.ForeignKey(
                        name: "FK_deployments_function_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deployments_function_functions_function_id",
                        column: x => x.function_id,
                        principalSchema: "public",
                        principalTable: "functions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deployments_function_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_component_id",
                schema: "public",
                table: "deployments_component",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_created_by",
                schema: "public",
                table: "deployments_component",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_end_time",
                schema: "public",
                table: "deployments_component",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_publish_status",
                schema: "public",
                table: "deployments_component",
                column: "publish_status");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_start_time",
                schema: "public",
                table: "deployments_component",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_status",
                schema: "public",
                table: "deployments_component",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_component_updated_by",
                schema: "public",
                table: "deployments_component",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_created_by",
                schema: "public",
                table: "deployments_function",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_end_time",
                schema: "public",
                table: "deployments_function",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_function_id",
                schema: "public",
                table: "deployments_function",
                column: "function_id");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_publish_status",
                schema: "public",
                table: "deployments_function",
                column: "publish_status");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_start_time",
                schema: "public",
                table: "deployments_function",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_status",
                schema: "public",
                table: "deployments_function",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_function_updated_by",
                schema: "public",
                table: "deployments_function",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_functions_created_by",
                schema: "public",
                table: "functions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_functions_deleted",
                schema: "public",
                table: "functions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_functions_label",
                schema: "public",
                table: "functions",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "IX_functions_name",
                schema: "public",
                table: "functions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_functions_runtime",
                schema: "public",
                table: "functions",
                column: "runtime");

            migrationBuilder.CreateIndex(
                name: "IX_functions_updated_by",
                schema: "public",
                table: "functions",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deployments_component",
                schema: "public");

            migrationBuilder.DropTable(
                name: "deployments_function",
                schema: "public");

            migrationBuilder.DropTable(
                name: "functions",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "public",
                table: "components");
        }
    }
}
