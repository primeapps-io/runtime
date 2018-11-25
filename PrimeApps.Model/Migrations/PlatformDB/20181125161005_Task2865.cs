using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task2865 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "team_apps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "team_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_apps_template_id",
                schema: "public",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "template_id",
                schema: "public",
                table: "apps");

            migrationBuilder.AddColumn<int>(
                name: "app_draft_id",
                schema: "public",
                table: "apps",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_apps_app_draft_id",
                schema: "public",
                table: "apps",
                column: "app_draft_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_apps_app_draft_id",
                schema: "public",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "app_draft_id",
                schema: "public",
                table: "apps");

            migrationBuilder.AddColumn<int>(
                name: "template_id",
                schema: "public",
                table: "apps",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_at = table.Column<DateTime>(nullable: false),
                    created_by = table.Column<int>(nullable: false),
                    deleted = table.Column<bool>(nullable: false),
                    label = table.Column<string>(maxLength: 50, nullable: true),
                    name = table.Column<string>(maxLength: 700, nullable: true),
                    owner_id = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    updated_by = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_users",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    organization_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_users", x => new { x.user_id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_organization_users_users_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_users_organizations_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_at = table.Column<DateTime>(nullable: false),
                    created_by = table.Column<int>(nullable: false),
                    deleted = table.Column<bool>(nullable: false),
                    name = table.Column<string>(maxLength: 700, nullable: true),
                    organization_id = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    updated_by = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teams_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "public",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teams_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team_apps",
                schema: "public",
                columns: table => new
                {
                    app_id = table.Column<int>(nullable: false),
                    team_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_apps", x => new { x.app_id, x.team_id });
                    table.ForeignKey(
                        name: "FK_team_apps_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_apps_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "public",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_users",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    team_id = table.Column<int>(nullable: false),
                    role = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_users", x => new { x.user_id, x.team_id });
                    table.ForeignKey(
                        name: "FK_team_users_users_team_id",
                        column: x => x.team_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_users_teams_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apps_template_id",
                schema: "public",
                table: "apps",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_organization_id",
                schema: "public",
                table: "organization_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_at",
                schema: "public",
                table: "organizations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_by",
                schema: "public",
                table: "organizations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_deleted",
                schema: "public",
                table: "organizations",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_name",
                schema: "public",
                table: "organizations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_owner_id",
                schema: "public",
                table: "organizations",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_updated_at",
                schema: "public",
                table: "organizations",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_updated_by",
                schema: "public",
                table: "organizations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_team_apps_team_id",
                schema: "public",
                table: "team_apps",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_users_team_id",
                schema: "public",
                table: "team_users",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_at",
                schema: "public",
                table: "teams",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_by",
                schema: "public",
                table: "teams",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_teams_deleted",
                schema: "public",
                table: "teams",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_teams_organization_id",
                schema: "public",
                table: "teams",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_updated_at",
                schema: "public",
                table: "teams",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_teams_updated_by",
                schema: "public",
                table: "teams",
                column: "updated_by");
        }
    }
}
