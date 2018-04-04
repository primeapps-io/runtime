using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "active_directory_cache",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    cache_bits = table.Column<byte[]>(nullable: true),
                    last_write = table.Column<DateTime>(nullable: false),
                    unique_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_active_directory_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "active_directory_tenants",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    admin_consented = table.Column<bool>(nullable: false),
                    confirmed = table.Column<bool>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    issuer = table.Column<string>(maxLength: 500, nullable: false),
                    name = table.Column<string>(maxLength: 500, nullable: false),
                    tenant_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_active_directory_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "apps",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    deleted = table.Column<bool>(nullable: false),
                    description = table.Column<string>(maxLength: 4000, nullable: true),
                    logo = table.Column<string>(nullable: true),
                    name = table.Column<string>(maxLength: 400, nullable: true),
                    template_id = table.Column<int>(nullable: true),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    active = table.Column<bool>(nullable: false),
                    allowed_origin = table.Column<string>(maxLength: 200, nullable: true),
                    application_type = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    refresh_token_life_time = table.Column<int>(nullable: false),
                    secret = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    date = table.Column<DateTime>(nullable: false),
                    day = table.Column<int>(nullable: false),
                    eur = table.Column<decimal>(nullable: false),
                    month = table.Column<int>(nullable: false),
                    usd = table.Column<decimal>(nullable: false),
                    year = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    client_id = table.Column<string>(maxLength: 50, nullable: false),
                    expires_utc = table.Column<DateTime>(nullable: false),
                    issued_utc = table.Column<DateTime>(nullable: false),
                    protected_ticket = table.Column<string>(nullable: false),
                    subject = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_apps",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    active = table.Column<bool>(nullable: false),
                    app_id = table.Column<int>(nullable: false),
                    email = table.Column<string>(nullable: true),
                    main_tenant_id = table.Column<int>(nullable: false),
                    tenant_id = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_apps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    completed = table.Column<bool>(nullable: false),
                    database_name = table.Column<string>(nullable: true),
                    database_user = table.Column<string>(nullable: true),
                    powerbi_workspace_id = table.Column<string>(nullable: true),
                    tenant_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    access_failed_count = table.Column<int>(nullable: false),
                    active_directory_email = table.Column<string>(nullable: true),
                    active_directory_tenant_id = table.Column<int>(nullable: false),
                    app_id = table.Column<int>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    culture = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    email = table.Column<string>(maxLength: 510, nullable: false),
                    email_confirmed = table.Column<bool>(nullable: false),
                    first_name = table.Column<string>(nullable: false),
                    last_name = table.Column<string>(nullable: false),
                    lockout_enabled = table.Column<bool>(nullable: false),
                    lockout_end_date_utc = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    password_hash = table.Column<string>(nullable: true),
                    phone_number = table.Column<string>(nullable: true),
                    phone_number_confirmed = table.Column<bool>(nullable: false),
                    security_stamp = table.Column<string>(nullable: true),
                    tenant_id = table.Column<int>(nullable: true),
                    two_factor_enabled = table.Column<bool>(nullable: false),
                    user_name = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    claim_type = table.Column<string>(nullable: true),
                    claim_value = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "public",
                columns: table => new
                {
                    login_provider = table.Column<string>(nullable: false),
                    provider_key = table.Column<string>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    role_id = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "public",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    currency = table.Column<string>(nullable: true),
                    custom_color = table.Column<string>(nullable: true),
                    custom_description = table.Column<string>(nullable: true),
                    custom_domain = table.Column<string>(nullable: true),
                    custom_favicon = table.Column<string>(nullable: true),
                    custom_image = table.Column<string>(nullable: true),
                    custom_title = table.Column<string>(nullable: true),
                    deactivated_at = table.Column<DateTime>(nullable: true),
                    guid_id = table.Column<Guid>(nullable: false),
                    has_analytics = table.Column<bool>(nullable: true),
                    has_analytics_license = table.Column<bool>(nullable: false),
                    has_phone = table.Column<bool>(nullable: true),
                    has_sample_data = table.Column<bool>(nullable: true),
                    is_deactivated = table.Column<bool>(nullable: false),
                    is_paid_customer = table.Column<bool>(nullable: false),
                    is_suspended = table.Column<bool>(nullable: false),
                    language = table.Column<string>(nullable: true),
                    logo = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    module_license_count = table.Column<int>(nullable: false),
                    owner = table.Column<int>(nullable: false),
                    suspended_at = table.Column<DateTime>(nullable: true),
                    title = table.Column<string>(nullable: true),
                    user_license_count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenants_users_owner",
                        column: x => x.owner,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "public",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_user_id",
                schema: "public",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_user_id",
                schema: "public",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_role_id",
                schema: "public",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "public",
                table: "roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_owner",
                schema: "public",
                table: "tenants",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "IX_users_app_id",
                schema: "public",
                table: "users",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "public",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "public",
                table: "users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_tenant_id",
                schema: "public",
                table: "users",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_tenants_tenant_id",
                schema: "public",
                table: "users",
                column: "tenant_id",
                principalSchema: "public",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tenants_users_owner",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropTable(
                name: "active_directory_cache",
                schema: "public");

            migrationBuilder.DropTable(
                name: "active_directory_tenants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "public");

            migrationBuilder.DropTable(
                name: "exchange_rates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_apps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "warehouse",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "apps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "public");
        }
    }
}
