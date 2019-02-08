using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task2886 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    name = table.Column<string>(maxLength: 200, nullable: false),
                    dependencies = table.Column<string>(nullable: true),
                    content = table.Column<string>(nullable: true),
                    handler = table.Column<string>(nullable: false),
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
                name: "IX_functions_updated_by",
                schema: "public",
                table: "functions",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "functions",
                schema: "public");
        }
    }
}
