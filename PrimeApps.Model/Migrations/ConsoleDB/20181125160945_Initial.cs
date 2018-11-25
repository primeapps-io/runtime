using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.ConsoleDB
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
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
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    icon = table.Column<string>(maxLength: 200, nullable: true),
                    owner_id = table.Column<int>(nullable: false)
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
                name: "templets",
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
                    name = table.Column<string>(maxLength: 50, nullable: true),
                    label = table.Column<string>(maxLength: 400, nullable: true),
                    description = table.Column<string>(maxLength: 4000, nullable: true),
                    logo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templets", x => x.id);
                    table.ForeignKey(
                        name: "FK_templets_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_templets_users_updated_by",
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
                    organization_id = table.Column<int>(nullable: false),
                    role = table.Column<int>(nullable: false)
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
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    organization_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    icon = table.Column<string>(maxLength: 200, nullable: true)
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
                name: "apps",
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
                    name = table.Column<string>(maxLength: 50, nullable: true),
                    label = table.Column<string>(maxLength: 400, nullable: true),
                    description = table.Column<string>(maxLength: 4000, nullable: true),
                    logo = table.Column<string>(nullable: true),
                    templet_id = table.Column<int>(nullable: false),
                    use_tenant_settings = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apps", x => x.id);
                    table.ForeignKey(
                        name: "FK_apps_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apps_templets_templet_id",
                        column: x => x.templet_id,
                        principalSchema: "public",
                        principalTable: "templets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apps_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team_users",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    team_id = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "app_collaborators",
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
                    user_id = table.Column<int>(nullable: true),
                    team_id = table.Column<int>(nullable: true),
                    role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_collaborators", x => x.id);
                    table.ForeignKey(
                        name: "FK_app_collaborators_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_app_collaborators_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_app_collaborators_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "public",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_app_collaborators_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_app_collaborators_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "app_settings",
                schema: "public",
                columns: table => new
                {
                    app_id = table.Column<int>(nullable: false),
                    app_domain = table.Column<string>(nullable: true),
                    auth_domain = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    culture = table.Column<string>(nullable: true),
                    time_zone = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    auth_theme = table.Column<string>(type: "jsonb", nullable: true),
                    app_theme = table.Column<string>(type: "jsonb", nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true),
                    google_analytics_code = table.Column<string>(nullable: true),
                    tenant_operation_webhook = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.app_id);
                    table.ForeignKey(
                        name: "FK_app_settings_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_collaborators_app_id",
                schema: "public",
                table: "app_collaborators",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_collaborators_created_by",
                schema: "public",
                table: "app_collaborators",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_app_collaborators_role",
                schema: "public",
                table: "app_collaborators",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_app_collaborators_team_id",
                schema: "public",
                table: "app_collaborators",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_collaborators_updated_by",
                schema: "public",
                table: "app_collaborators",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_app_collaborators_user_id",
                schema: "public",
                table: "app_collaborators",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_apps_created_at",
                schema: "public",
                table: "apps",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_apps_created_by",
                schema: "public",
                table: "apps",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_apps_deleted",
                schema: "public",
                table: "apps",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_apps_name",
                schema: "public",
                table: "apps",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_apps_templet_id",
                schema: "public",
                table: "apps",
                column: "templet_id");

            migrationBuilder.CreateIndex(
                name: "IX_apps_updated_at",
                schema: "public",
                table: "apps",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_apps_updated_by",
                schema: "public",
                table: "apps",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_organization_id",
                schema: "public",
                table: "organization_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_user_id",
                schema: "public",
                table: "organization_users",
                column: "user_id");

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
                name: "IX_team_users_team_id",
                schema: "public",
                table: "team_users",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_users_user_id",
                schema: "public",
                table: "team_users",
                column: "user_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_templets_created_at",
                schema: "public",
                table: "templets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_templets_created_by",
                schema: "public",
                table: "templets",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_templets_deleted",
                schema: "public",
                table: "templets",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_templets_name",
                schema: "public",
                table: "templets",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_templets_updated_at",
                schema: "public",
                table: "templets",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_templets_updated_by",
                schema: "public",
                table: "templets",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_collaborators",
                schema: "public");

            migrationBuilder.DropTable(
                name: "app_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organization_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "team_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "apps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "public");

            migrationBuilder.DropTable(
                name: "templets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
