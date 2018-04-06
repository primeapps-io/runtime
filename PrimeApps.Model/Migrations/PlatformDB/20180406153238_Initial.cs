using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    unique_id = table.Column<string>(nullable: true),
                    cache_bits = table.Column<byte[]>(nullable: true),
                    last_write = table.Column<DateTime>(nullable: false)
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
                    name = table.Column<string>(maxLength: 500, nullable: false),
                    issuer = table.Column<string>(maxLength: 500, nullable: false),
                    admin_consented = table.Column<bool>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    tenant_id = table.Column<int>(nullable: false),
                    confirmed = table.Column<bool>(nullable: false)
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
                    name = table.Column<string>(maxLength: 400, nullable: true),
                    description = table.Column<string>(maxLength: 4000, nullable: true),
                    user_id = table.Column<int>(nullable: false),
                    logo = table.Column<string>(nullable: true),
                    template_id = table.Column<int>(nullable: true),
                    deleted = table.Column<bool>(nullable: false)
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
                    secret = table.Column<string>(nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    application_type = table.Column<int>(nullable: false),
                    active = table.Column<bool>(nullable: false),
                    refresh_token_life_time = table.Column<int>(nullable: false),
                    allowed_origin = table.Column<string>(maxLength: 200, nullable: true)
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
                    usd = table.Column<decimal>(nullable: false),
                    eur = table.Column<decimal>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    year = table.Column<int>(nullable: false),
                    month = table.Column<int>(nullable: false),
                    day = table.Column<int>(nullable: false)
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
                    subject = table.Column<string>(maxLength: 50, nullable: false),
                    client_id = table.Column<string>(maxLength: 50, nullable: false),
                    issued_utc = table.Column<DateTime>(nullable: false),
                    expires_utc = table.Column<DateTime>(nullable: false),
                    protected_ticket = table.Column<string>(nullable: false)
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
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
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
                    user_id = table.Column<int>(nullable: false),
                    tenant_id = table.Column<int>(nullable: false),
                    main_tenant_id = table.Column<int>(nullable: false),
                    email = table.Column<string>(nullable: true),
                    active = table.Column<bool>(nullable: false),
                    app_id = table.Column<int>(nullable: false)
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
                    tenant_id = table.Column<int>(nullable: false),
                    database_name = table.Column<string>(nullable: true),
                    database_user = table.Column<string>(nullable: true),
                    powerbi_workspace_id = table.Column<string>(nullable: true),
                    completed = table.Column<bool>(nullable: false)
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
                    RoleId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    app_id = table.Column<int>(nullable: false),
                    tenant_id = table.Column<int>(nullable: true),
                    email = table.Column<string>(maxLength: 510, nullable: false),
                    first_name = table.Column<string>(nullable: false),
                    last_name = table.Column<string>(nullable: false),
                    email_confirmed = table.Column<bool>(nullable: false),
                    password_hash = table.Column<string>(nullable: true),
                    security_stamp = table.Column<string>(nullable: true),
                    phone_number = table.Column<string>(nullable: true),
                    phone_number_confirmed = table.Column<bool>(nullable: false),
                    two_factor_enabled = table.Column<bool>(nullable: false),
                    lockout_end_date_utc = table.Column<string>(nullable: true),
                    lockout_enabled = table.Column<bool>(nullable: false),
                    access_failed_count = table.Column<int>(nullable: false),
                    user_name = table.Column<string>(maxLength: 256, nullable: true),
                    culture = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    active_directory_tenant_id = table.Column<int>(nullable: false),
                    active_directory_email = table.Column<string>(nullable: true)
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
                    user_id = table.Column<int>(nullable: false),
                    claim_type = table.Column<string>(nullable: true),
                    claim_value = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false)
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
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    user_id = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false)
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
                    guid_id = table.Column<Guid>(nullable: false),
                    title = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    logo = table.Column<string>(nullable: true),
                    has_sample_data = table.Column<bool>(nullable: true),
                    has_analytics = table.Column<bool>(nullable: true),
                    has_phone = table.Column<bool>(nullable: true),
                    custom_domain = table.Column<string>(nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true),
                    custom_title = table.Column<string>(nullable: true),
                    custom_description = table.Column<string>(nullable: true),
                    custom_favicon = table.Column<string>(nullable: true),
                    custom_color = table.Column<string>(nullable: true),
                    custom_image = table.Column<string>(nullable: true),
                    user_license_count = table.Column<int>(nullable: false),
                    module_license_count = table.Column<int>(nullable: false),
                    has_analytics_license = table.Column<bool>(nullable: false),
                    is_paid_customer = table.Column<bool>(nullable: false),
                    is_deactivated = table.Column<bool>(nullable: false),
                    is_suspended = table.Column<bool>(nullable: false),
                    deactivated_at = table.Column<DateTime>(nullable: true),
                    suspended_at = table.Column<DateTime>(nullable: true),
                    owner = table.Column<int>(nullable: false)
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
                name: "IX_exchange_rates_date",
                schema: "public",
                table: "exchange_rates",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_day",
                schema: "public",
                table: "exchange_rates",
                column: "day");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_month",
                schema: "public",
                table: "exchange_rates",
                column: "month");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_year",
                schema: "public",
                table: "exchange_rates",
                column: "year");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "public",
                table: "roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_custom_domain",
                schema: "public",
                table: "tenants",
                column: "custom_domain");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_custom_title",
                schema: "public",
                table: "tenants",
                column: "custom_title");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_deactivated_at",
                schema: "public",
                table: "tenants",
                column: "deactivated_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_guid_id",
                schema: "public",
                table: "tenants",
                column: "guid_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_has_analytics",
                schema: "public",
                table: "tenants",
                column: "has_analytics");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_has_analytics_license",
                schema: "public",
                table: "tenants",
                column: "has_analytics_license");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_has_phone",
                schema: "public",
                table: "tenants",
                column: "has_phone");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_has_sample_data",
                schema: "public",
                table: "tenants",
                column: "has_sample_data");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_is_deactivated",
                schema: "public",
                table: "tenants",
                column: "is_deactivated");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_is_paid_customer",
                schema: "public",
                table: "tenants",
                column: "is_paid_customer");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_is_suspended",
                schema: "public",
                table: "tenants",
                column: "is_suspended");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_language",
                schema: "public",
                table: "tenants",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_mail_sender_email",
                schema: "public",
                table: "tenants",
                column: "mail_sender_email");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_module_license_count",
                schema: "public",
                table: "tenants",
                column: "module_license_count");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_owner",
                schema: "public",
                table: "tenants",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_suspended_at",
                schema: "public",
                table: "tenants",
                column: "suspended_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_user_license_count",
                schema: "public",
                table: "tenants",
                column: "user_license_count");

            migrationBuilder.CreateIndex(
                name: "IX_user_apps_active",
                schema: "public",
                table: "user_apps",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_user_apps_app_id",
                schema: "public",
                table: "user_apps",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_apps_email",
                schema: "public",
                table: "user_apps",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_user_apps_main_tenant_id",
                schema: "public",
                table: "user_apps",
                column: "main_tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_apps_tenant_id",
                schema: "public",
                table: "user_apps",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_apps_user_id",
                schema: "public",
                table: "user_apps",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_access_failed_count",
                schema: "public",
                table: "users",
                column: "access_failed_count");

            migrationBuilder.CreateIndex(
                name: "IX_users_active_directory_email",
                schema: "public",
                table: "users",
                column: "active_directory_email");

            migrationBuilder.CreateIndex(
                name: "IX_users_app_id",
                schema: "public",
                table: "users",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_currency",
                schema: "public",
                table: "users",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "public",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_users_email_confirmed",
                schema: "public",
                table: "users",
                column: "email_confirmed");

            migrationBuilder.CreateIndex(
                name: "IX_users_first_name",
                schema: "public",
                table: "users",
                column: "first_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_id",
                schema: "public",
                table: "users",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_users_last_name",
                schema: "public",
                table: "users",
                column: "last_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_lockout_enabled",
                schema: "public",
                table: "users",
                column: "lockout_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_users_lockout_end_date_utc",
                schema: "public",
                table: "users",
                column: "lockout_end_date_utc");

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
                name: "IX_users_password_hash",
                schema: "public",
                table: "users",
                column: "password_hash");

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number_confirmed",
                schema: "public",
                table: "users",
                column: "phone_number_confirmed");

            migrationBuilder.CreateIndex(
                name: "IX_users_tenant_id",
                schema: "public",
                table: "users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_two_factor_enabled",
                schema: "public",
                table: "users",
                column: "two_factor_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_completed",
                schema: "public",
                table: "warehouse",
                column: "completed");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_database_name",
                schema: "public",
                table: "warehouse",
                column: "database_name");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_tenant_id",
                schema: "public",
                table: "warehouse",
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
