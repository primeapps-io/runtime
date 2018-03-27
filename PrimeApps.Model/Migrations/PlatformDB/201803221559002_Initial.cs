namespace PrimeApps.Model.Migrations.PlatformDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.active_directory_cache",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        unique_id = c.String(),
                        cache_bits = c.Binary(),
                        last_write = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.unique_id);
            
            CreateTable(
                "public.active_directory_tenants",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 500),
                        issuer = c.String(nullable: false, maxLength: 500),
                        admin_consented = c.Boolean(nullable: false),
                        created_at = c.DateTime(nullable: false),
                        tenant_id = c.Int(nullable: false),
                        confirmed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.issuer)
                .Index(t => t.created_at)
                .Index(t => t.tenant_id)
                .Index(t => t.confirmed);
            
            CreateTable(
                "public.apps",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(maxLength: 400),
                        description = c.String(maxLength: 4000),
                        user_id = c.Int(nullable: false),
                        logo = c.String(),
                        template_id = c.Int(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "public.clients",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        secret = c.String(nullable: false),
                        name = c.String(nullable: false, maxLength: 100),
                        application_type = c.Int(nullable: false),
                        active = c.Boolean(nullable: false),
                        refresh_token_life_time = c.Int(nullable: false),
                        allowed_origin = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.secret)
                .Index(t => t.name);
            
            CreateTable(
                "public.exchange_rates",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        usd = c.Decimal(nullable: false, precision: 18, scale: 2),
                        eur = c.Decimal(nullable: false, precision: 18, scale: 2),
                        date = c.DateTime(nullable: false),
                        year = c.Int(nullable: false),
                        month = c.Int(nullable: false),
                        day = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.date)
                .Index(t => t.year)
                .Index(t => t.month)
                .Index(t => t.day);
            
            CreateTable(
                "public.refresh_tokens",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        subject = c.String(nullable: false, maxLength: 50),
                        client_id = c.String(nullable: false, maxLength: 50),
                        issued_utc = c.DateTime(nullable: false),
                        expires_utc = c.DateTime(nullable: false),
                        protected_ticket = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.subject)
                .Index(t => t.client_id);
            
            CreateTable(
                "public.roles",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "public.user_roles",
                c => new
                    {
                        user_id = c.Int(nullable: false),
                        role_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.user_id, t.role_id })
                .ForeignKey("public.roles", t => t.role_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.user_id)
                .Index(t => t.role_id);
            
            CreateTable(
                "public.tenants",
                c => new
                    {
                        id = c.Int(nullable: false),
                        guid_id = c.Guid(nullable: false),
                        title = c.String(),
                        currency = c.String(),
                        language = c.String(),
                        logo = c.String(),
                        has_sample_data = c.Boolean(),
                        has_analytics = c.Boolean(),
                        has_phone = c.Boolean(),
                        custom_domain = c.String(),
                        mail_sender_name = c.String(),
                        mail_sender_email = c.String(),
                        custom_title = c.String(),
                        custom_description = c.String(),
                        custom_favicon = c.String(),
                        custom_color = c.String(),
                        custom_image = c.String(),
                        user_license_count = c.Int(nullable: false),
                        module_license_count = c.Int(nullable: false),
                        has_analytics_license = c.Boolean(nullable: false),
                        is_paid_customer = c.Boolean(nullable: false),
                        is_deactivated = c.Boolean(nullable: false),
                        is_suspended = c.Boolean(nullable: false),
                        deactivated_at = c.DateTime(),
                        suspended_at = c.DateTime(),
                        owner = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.owner, cascadeDelete: true)
                .Index(t => t.guid_id)
                .Index(t => t.language)
                .Index(t => t.has_sample_data)
                .Index(t => t.has_analytics)
                .Index(t => t.has_phone)
                .Index(t => t.custom_domain)
                .Index(t => t.mail_sender_email)
                .Index(t => t.custom_title)
                .Index(t => t.user_license_count)
                .Index(t => t.module_license_count)
                .Index(t => t.has_analytics_license)
                .Index(t => t.is_paid_customer)
                .Index(t => t.is_deactivated)
                .Index(t => t.is_suspended)
                .Index(t => t.deactivated_at)
                .Index(t => t.suspended_at)
                .Index(t => t.owner);
            
            CreateTable(
                "public.users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        app_id = c.Int(nullable: false),
                        tenant_id = c.Int(),
                        email = c.String(nullable: false, maxLength: 510),
                        first_name = c.String(nullable: false),
                        last_name = c.String(nullable: false),
                        email_confirmed = c.Boolean(nullable: false),
                        password_hash = c.String(),
                        security_stamp = c.String(),
                        phone_number = c.String(),
                        phone_number_confirmed = c.Boolean(nullable: false),
                        two_factor_enabled = c.Boolean(nullable: false),
                        lockout_end_date_utc = c.DateTime(),
                        lockout_enabled = c.Boolean(nullable: false),
                        access_failed_count = c.Int(nullable: false),
                        user_name = c.String(nullable: false, maxLength: 256),
                        culture = c.String(),
                        currency = c.String(),
                        created_at = c.DateTime(nullable: false),
                        active_directory_tenant_id = c.Int(nullable: false),
                        active_directory_email = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.apps", t => t.app_id, cascadeDelete: true)
                .ForeignKey("public.tenants", t => t.tenant_id)
                .Index(t => t.app_id)
                .Index(t => t.tenant_id)
                .Index(t => t.email)
                .Index(t => t.first_name)
                .Index(t => t.last_name)
                .Index(t => t.email_confirmed)
                .Index(t => t.password_hash)
                .Index(t => t.security_stamp)
                .Index(t => t.phone_number)
                .Index(t => t.phone_number_confirmed)
                .Index(t => t.two_factor_enabled)
                .Index(t => t.lockout_end_date_utc)
                .Index(t => t.lockout_enabled)
                .Index(t => t.access_failed_count)
                .Index(t => t.user_name)
                .Index(t => t.user_name, unique: true, name: "UserNameIndex")
                .Index(t => t.currency)
                .Index(t => t.created_at)
                .Index(t => t.active_directory_tenant_id)
                .Index(t => t.active_directory_email);
            
            CreateTable(
                "public.user_claims",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        claim_type = c.String(),
                        claim_value = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.user_id);
            
            CreateTable(
                "public.user_logins",
                c => new
                    {
                        login_provider = c.String(nullable: false, maxLength: 128),
                        provider_key = c.String(nullable: false, maxLength: 128),
                        user_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.login_provider, t.provider_key, t.user_id })
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.user_id);
            
            CreateTable(
                "public.user_apps",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        tenant_id = c.Int(nullable: false),
                        main_tenant_id = c.Int(nullable: false),
                        email = c.String(),
                        active = c.Boolean(nullable: false),
                        app_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.user_id)
                .Index(t => t.tenant_id)
                .Index(t => t.main_tenant_id)
                .Index(t => t.email)
                .Index(t => t.active)
                .Index(t => t.app_id);
            
            CreateTable(
                "public.warehouse",
                c => new
                    {
                        id = c.Guid(nullable: false),
                        tenant_id = c.Int(nullable: false),
                        database_name = c.String(),
                        database_user = c.String(),
                        powerbi_workspace_id = c.String(),
                        completed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.tenant_id)
                .Index(t => t.database_name)
                .Index(t => t.completed);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.users", "tenant_id", "public.tenants");
            DropForeignKey("public.tenants", "owner", "public.users");
            DropForeignKey("public.user_roles", "user_id", "public.users");
            DropForeignKey("public.user_logins", "user_id", "public.users");
            DropForeignKey("public.user_claims", "user_id", "public.users");
            DropForeignKey("public.users", "app_id", "public.apps");
            DropForeignKey("public.user_roles", "role_id", "public.roles");
            DropIndex("public.warehouse", new[] { "completed" });
            DropIndex("public.warehouse", new[] { "database_name" });
            DropIndex("public.warehouse", new[] { "tenant_id" });
            DropIndex("public.user_apps", new[] { "app_id" });
            DropIndex("public.user_apps", new[] { "active" });
            DropIndex("public.user_apps", new[] { "email" });
            DropIndex("public.user_apps", new[] { "main_tenant_id" });
            DropIndex("public.user_apps", new[] { "tenant_id" });
            DropIndex("public.user_apps", new[] { "user_id" });
            DropIndex("public.user_logins", new[] { "user_id" });
            DropIndex("public.user_claims", new[] { "user_id" });
            DropIndex("public.users", new[] { "active_directory_email" });
            DropIndex("public.users", new[] { "active_directory_tenant_id" });
            DropIndex("public.users", new[] { "created_at" });
            DropIndex("public.users", new[] { "currency" });
            DropIndex("public.users", "UserNameIndex");
            DropIndex("public.users", new[] { "user_name" });
            DropIndex("public.users", new[] { "access_failed_count" });
            DropIndex("public.users", new[] { "lockout_enabled" });
            DropIndex("public.users", new[] { "lockout_end_date_utc" });
            DropIndex("public.users", new[] { "two_factor_enabled" });
            DropIndex("public.users", new[] { "phone_number_confirmed" });
            DropIndex("public.users", new[] { "phone_number" });
            DropIndex("public.users", new[] { "security_stamp" });
            DropIndex("public.users", new[] { "password_hash" });
            DropIndex("public.users", new[] { "email_confirmed" });
            DropIndex("public.users", new[] { "last_name" });
            DropIndex("public.users", new[] { "first_name" });
            DropIndex("public.users", new[] { "email" });
            DropIndex("public.users", new[] { "tenant_id" });
            DropIndex("public.users", new[] { "app_id" });
            DropIndex("public.tenants", new[] { "owner" });
            DropIndex("public.tenants", new[] { "suspended_at" });
            DropIndex("public.tenants", new[] { "deactivated_at" });
            DropIndex("public.tenants", new[] { "is_suspended" });
            DropIndex("public.tenants", new[] { "is_deactivated" });
            DropIndex("public.tenants", new[] { "is_paid_customer" });
            DropIndex("public.tenants", new[] { "has_analytics_license" });
            DropIndex("public.tenants", new[] { "module_license_count" });
            DropIndex("public.tenants", new[] { "user_license_count" });
            DropIndex("public.tenants", new[] { "custom_title" });
            DropIndex("public.tenants", new[] { "mail_sender_email" });
            DropIndex("public.tenants", new[] { "custom_domain" });
            DropIndex("public.tenants", new[] { "has_phone" });
            DropIndex("public.tenants", new[] { "has_analytics" });
            DropIndex("public.tenants", new[] { "has_sample_data" });
            DropIndex("public.tenants", new[] { "language" });
            DropIndex("public.tenants", new[] { "guid_id" });
            DropIndex("public.user_roles", new[] { "role_id" });
            DropIndex("public.user_roles", new[] { "user_id" });
            DropIndex("public.roles", "RoleNameIndex");
            DropIndex("public.refresh_tokens", new[] { "client_id" });
            DropIndex("public.refresh_tokens", new[] { "subject" });
            DropIndex("public.exchange_rates", new[] { "day" });
            DropIndex("public.exchange_rates", new[] { "month" });
            DropIndex("public.exchange_rates", new[] { "year" });
            DropIndex("public.exchange_rates", new[] { "date" });
            DropIndex("public.clients", new[] { "name" });
            DropIndex("public.clients", new[] { "secret" });
            DropIndex("public.active_directory_tenants", new[] { "confirmed" });
            DropIndex("public.active_directory_tenants", new[] { "tenant_id" });
            DropIndex("public.active_directory_tenants", new[] { "created_at" });
            DropIndex("public.active_directory_tenants", new[] { "issuer" });
            DropIndex("public.active_directory_cache", new[] { "unique_id" });
            DropTable("public.warehouse");
            DropTable("public.user_apps");
            DropTable("public.user_logins");
            DropTable("public.user_claims");
            DropTable("public.users");
            DropTable("public.tenants");
            DropTable("public.user_roles");
            DropTable("public.roles");
            DropTable("public.refresh_tokens");
            DropTable("public.exchange_rates");
            DropTable("public.clients");
            DropTable("public.apps");
            DropTable("public.active_directory_tenants");
            DropTable("public.active_directory_cache");
        }
    }
}
