/*
 Navicat Premium Data Transfer

 Source Server         : Localhost
 Source Server Type    : PostgreSQL
 Source Server Version : 90611
 Source Host           : localhost:5432
 Source Catalog        : studio
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 90611
 File Encoding         : 65001

 Date: 02/04/2019 11:01:56
*/


-- ----------------------------
-- Sequence structure for app_collaborators_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."app_collaborators_id_seq";
CREATE SEQUENCE "public"."app_collaborators_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."app_collaborators_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for apps_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."apps_id_seq";
CREATE SEQUENCE "public"."apps_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."apps_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for deployments_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."deployments_id_seq";
CREATE SEQUENCE "public"."deployments_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."deployments_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for organization_users_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."organization_users_id_seq";
CREATE SEQUENCE "public"."organization_users_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."organization_users_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for organizations_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."organizations_id_seq";
CREATE SEQUENCE "public"."organizations_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."organizations_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for teams_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."teams_id_seq";
CREATE SEQUENCE "public"."teams_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."teams_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for templet_categories_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."templet_categories_id_seq";
CREATE SEQUENCE "public"."templet_categories_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."templet_categories_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for templets_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."templets_id_seq";
CREATE SEQUENCE "public"."templets_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."templets_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Table structure for _migration_history
-- ----------------------------
DROP TABLE IF EXISTS "public"."_migration_history";
CREATE TABLE "public"."_migration_history" (
  "migration_id" varchar(150) COLLATE "pg_catalog"."default" NOT NULL,
  "product_version" varchar(32) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."_migration_history" OWNER TO "postgres";

-- ----------------------------
-- Records of _migration_history
-- ----------------------------
BEGIN;
INSERT INTO "public"."_migration_history" VALUES ('20190217184904_Initial', '2.2.0-rtm-35687');
COMMIT;

-- ----------------------------
-- Table structure for app_collaborators
-- ----------------------------
DROP TABLE IF EXISTS "public"."app_collaborators";
CREATE TABLE "public"."app_collaborators" (
  "id" int4 NOT NULL DEFAULT nextval('app_collaborators_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "app_id" int4 NOT NULL,
  "user_id" int4,
  "team_id" int4,
  "profile" int4 NOT NULL
)
;
ALTER TABLE "public"."app_collaborators" OWNER TO "postgres";

-- ----------------------------
-- Table structure for app_settings
-- ----------------------------
DROP TABLE IF EXISTS "public"."app_settings";
CREATE TABLE "public"."app_settings" (
  "app_id" int4 NOT NULL,
  "app_domain" text COLLATE "pg_catalog"."default",
  "auth_domain" text COLLATE "pg_catalog"."default",
  "currency" text COLLATE "pg_catalog"."default",
  "culture" text COLLATE "pg_catalog"."default",
  "time_zone" text COLLATE "pg_catalog"."default",
  "language" text COLLATE "pg_catalog"."default",
  "auth_theme" jsonb,
  "app_theme" jsonb,
  "mail_sender_name" text COLLATE "pg_catalog"."default",
  "mail_sender_email" text COLLATE "pg_catalog"."default",
  "google_analytics_code" text COLLATE "pg_catalog"."default",
  "tenant_operation_webhook" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."app_settings" OWNER TO "postgres";

-- ----------------------------
-- Table structure for apps
-- ----------------------------
DROP TABLE IF EXISTS "public"."apps";
CREATE TABLE "public"."apps" (
  "id" int4 NOT NULL DEFAULT nextval('apps_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default",
  "label" varchar(400) COLLATE "pg_catalog"."default",
  "description" varchar(4000) COLLATE "pg_catalog"."default",
  "logo" text COLLATE "pg_catalog"."default",
  "organization_id" int4 NOT NULL,
  "templet_id" int4 NOT NULL,
  "use_tenant_settings" bool NOT NULL,
  "status" int4 NOT NULL
)
;
ALTER TABLE "public"."apps" OWNER TO "postgres";

-- ----------------------------
-- Table structure for deployments
-- ----------------------------
DROP TABLE IF EXISTS "public"."deployments";
CREATE TABLE "public"."deployments" (
  "id" int4 NOT NULL DEFAULT nextval('deployments_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "app_id" int4 NOT NULL,
  "status" int4 NOT NULL,
  "version" text COLLATE "pg_catalog"."default" NOT NULL,
  "start_time" timestamp(6) NOT NULL,
  "end_time" timestamp(6) NOT NULL
)
;
ALTER TABLE "public"."deployments" OWNER TO "postgres";

-- ----------------------------
-- Table structure for organization_users
-- ----------------------------
DROP TABLE IF EXISTS "public"."organization_users";
CREATE TABLE "public"."organization_users" (
  "user_id" int4 NOT NULL,
  "organization_id" int4 NOT NULL,
  "id" int4 NOT NULL DEFAULT nextval('organization_users_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "role" int4 NOT NULL
)
;
ALTER TABLE "public"."organization_users" OWNER TO "postgres";

-- ----------------------------
-- Records of organization_users
-- ----------------------------
BEGIN;
INSERT INTO "public"."organization_users" VALUES (1, 1, 1, 1, NULL, '2019-02-24 17:34:22', NULL, 'f', 1);
COMMIT;

-- ----------------------------
-- Table structure for organizations
-- ----------------------------
DROP TABLE IF EXISTS "public"."organizations";
CREATE TABLE "public"."organizations" (
  "id" int4 NOT NULL DEFAULT nextval('organizations_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "label" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "icon" varchar(200) COLLATE "pg_catalog"."default",
  "owner_id" int4 NOT NULL,
  "default" bool NOT NULL,
  "color" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."organizations" OWNER TO "postgres";

-- ----------------------------
-- Records of organizations
-- ----------------------------
BEGIN;
INSERT INTO "public"."organizations" VALUES (1, 1, NULL, '2019-02-24 17:32:32', NULL, 'f', 'master-organization', 'Master Organization', 'fas fa-building', 1, 't', '#9F5590');
COMMIT;

-- ----------------------------
-- Table structure for team_users
-- ----------------------------
DROP TABLE IF EXISTS "public"."team_users";
CREATE TABLE "public"."team_users" (
  "user_id" int4 NOT NULL,
  "team_id" int4 NOT NULL
)
;
ALTER TABLE "public"."team_users" OWNER TO "postgres";

-- ----------------------------
-- Table structure for teams
-- ----------------------------
DROP TABLE IF EXISTS "public"."teams";
CREATE TABLE "public"."teams" (
  "id" int4 NOT NULL DEFAULT nextval('teams_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "organization_id" int4 NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "icon" varchar(200) COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."teams" OWNER TO "postgres";

-- ----------------------------
-- Table structure for templet_categories
-- ----------------------------
DROP TABLE IF EXISTS "public"."templet_categories";
CREATE TABLE "public"."templet_categories" (
  "id" int4 NOT NULL DEFAULT nextval('templet_categories_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "label" varchar(400) COLLATE "pg_catalog"."default",
  "description" varchar(4000) COLLATE "pg_catalog"."default",
  "image" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."templet_categories" OWNER TO "postgres";

-- ----------------------------
-- Records of templet_categories
-- ----------------------------
BEGIN;
INSERT INTO "public"."templet_categories" VALUES (0, 1, 1, '2018-12-21 09:34:54', '2018-12-21 09:34:55', 'f', 'Master Category', 'Master Category', 'banner.jpg');
COMMIT;

-- ----------------------------
-- Table structure for templets
-- ----------------------------
DROP TABLE IF EXISTS "public"."templets";
CREATE TABLE "public"."templets" (
  "id" int4 NOT NULL DEFAULT nextval('templets_id_seq'::regclass),
  "created_by" int4 NOT NULL,
  "updated_by" int4,
  "created_at" timestamp(6) NOT NULL,
  "updated_at" timestamp(6),
  "deleted" bool NOT NULL,
  "category_id" int4 NOT NULL,
  "label" varchar(400) COLLATE "pg_catalog"."default",
  "description" varchar(4000) COLLATE "pg_catalog"."default",
  "logo" text COLLATE "pg_catalog"."default",
  "image" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."templets" OWNER TO "postgres";

-- ----------------------------
-- Records of templets
-- ----------------------------
BEGIN;
INSERT INTO "public"."templets" VALUES (0, 1, 1, '2018-12-21 09:35:29', '2018-12-21 09:35:29', 'f', 0, 'Master Template', 'Master Template', 'logo.png', 'banner.jpg');
COMMIT;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS "public"."users";
CREATE TABLE "public"."users" (
  "id" int4 NOT NULL
)
;
ALTER TABLE "public"."users" OWNER TO "postgres";

-- ----------------------------
-- Records of users
-- ----------------------------
BEGIN;
INSERT INTO "public"."users" VALUES (1);
COMMIT;

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."app_collaborators_id_seq"
OWNED BY "public"."app_collaborators"."id";
SELECT setval('"public"."app_collaborators_id_seq"', 3, false);
ALTER SEQUENCE "public"."apps_id_seq"
OWNED BY "public"."apps"."id";
SELECT setval('"public"."apps_id_seq"', 3, false);
ALTER SEQUENCE "public"."deployments_id_seq"
OWNED BY "public"."deployments"."id";
SELECT setval('"public"."deployments_id_seq"', 3, false);
ALTER SEQUENCE "public"."organization_users_id_seq"
OWNED BY "public"."organization_users"."id";
SELECT setval('"public"."organization_users_id_seq"', 3, true);
ALTER SEQUENCE "public"."organizations_id_seq"
OWNED BY "public"."organizations"."id";
SELECT setval('"public"."organizations_id_seq"', 3, true);
ALTER SEQUENCE "public"."teams_id_seq"
OWNED BY "public"."teams"."id";
SELECT setval('"public"."teams_id_seq"', 3, false);
ALTER SEQUENCE "public"."templet_categories_id_seq"
OWNED BY "public"."templet_categories"."id";
SELECT setval('"public"."templet_categories_id_seq"', 3, false);
ALTER SEQUENCE "public"."templets_id_seq"
OWNED BY "public"."templets"."id";
SELECT setval('"public"."templets_id_seq"', 3, false);

-- ----------------------------
-- Primary Key structure for table _migration_history
-- ----------------------------
ALTER TABLE "public"."_migration_history" ADD CONSTRAINT "PK__migration_history" PRIMARY KEY ("migration_id");

-- ----------------------------
-- Indexes structure for table app_collaborators
-- ----------------------------
CREATE INDEX "IX_app_collaborators_app_id" ON "public"."app_collaborators" USING btree (
  "app_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_collaborators_created_by" ON "public"."app_collaborators" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_collaborators_team_id" ON "public"."app_collaborators" USING btree (
  "team_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_collaborators_updated_by" ON "public"."app_collaborators" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_app_collaborators_user_id" ON "public"."app_collaborators" USING btree (
  "user_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table app_collaborators
-- ----------------------------
ALTER TABLE "public"."app_collaborators" ADD CONSTRAINT "PK_app_collaborators" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table app_settings
-- ----------------------------
ALTER TABLE "public"."app_settings" ADD CONSTRAINT "PK_app_settings" PRIMARY KEY ("app_id");

-- ----------------------------
-- Indexes structure for table apps
-- ----------------------------
CREATE INDEX "IX_apps_created_at" ON "public"."apps" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_created_by" ON "public"."apps" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_deleted" ON "public"."apps" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_name" ON "public"."apps" USING btree (
  "name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_organization_id" ON "public"."apps" USING btree (
  "organization_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_templet_id" ON "public"."apps" USING btree (
  "templet_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_updated_at" ON "public"."apps" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_apps_updated_by" ON "public"."apps" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table apps
-- ----------------------------
ALTER TABLE "public"."apps" ADD CONSTRAINT "PK_apps" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table deployments
-- ----------------------------
CREATE INDEX "IX_deployments_app_id" ON "public"."deployments" USING btree (
  "app_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_deployments_created_by" ON "public"."deployments" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_deployments_end_time" ON "public"."deployments" USING btree (
  "end_time" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_deployments_start_time" ON "public"."deployments" USING btree (
  "start_time" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_deployments_status" ON "public"."deployments" USING btree (
  "status" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_deployments_updated_by" ON "public"."deployments" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table deployments
-- ----------------------------
ALTER TABLE "public"."deployments" ADD CONSTRAINT "PK_deployments" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table organization_users
-- ----------------------------
CREATE INDEX "IX_organization_users_created_by" ON "public"."organization_users" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organization_users_organization_id" ON "public"."organization_users" USING btree (
  "organization_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organization_users_updated_by" ON "public"."organization_users" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organization_users_user_id" ON "public"."organization_users" USING btree (
  "user_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Uniques structure for table organization_users
-- ----------------------------
ALTER TABLE "public"."organization_users" ADD CONSTRAINT "AK_organization_users_id" UNIQUE ("id");

-- ----------------------------
-- Primary Key structure for table organization_users
-- ----------------------------
ALTER TABLE "public"."organization_users" ADD CONSTRAINT "PK_organization_users" PRIMARY KEY ("user_id", "organization_id");

-- ----------------------------
-- Indexes structure for table organizations
-- ----------------------------
CREATE INDEX "IX_organizations_created_at" ON "public"."organizations" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_created_by" ON "public"."organizations" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_deleted" ON "public"."organizations" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_label" ON "public"."organizations" USING btree (
  "label" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_name" ON "public"."organizations" USING btree (
  "name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_owner_id" ON "public"."organizations" USING btree (
  "owner_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_updated_at" ON "public"."organizations" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_organizations_updated_by" ON "public"."organizations" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table organizations
-- ----------------------------
ALTER TABLE "public"."organizations" ADD CONSTRAINT "PK_organizations" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table team_users
-- ----------------------------
CREATE INDEX "IX_team_users_team_id" ON "public"."team_users" USING btree (
  "team_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_team_users_user_id" ON "public"."team_users" USING btree (
  "user_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table team_users
-- ----------------------------
ALTER TABLE "public"."team_users" ADD CONSTRAINT "PK_team_users" PRIMARY KEY ("user_id", "team_id");

-- ----------------------------
-- Indexes structure for table teams
-- ----------------------------
CREATE INDEX "IX_teams_created_at" ON "public"."teams" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_teams_created_by" ON "public"."teams" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_teams_deleted" ON "public"."teams" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_teams_organization_id" ON "public"."teams" USING btree (
  "organization_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_teams_updated_at" ON "public"."teams" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_teams_updated_by" ON "public"."teams" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table teams
-- ----------------------------
ALTER TABLE "public"."teams" ADD CONSTRAINT "PK_teams" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table templet_categories
-- ----------------------------
CREATE INDEX "IX_templet_categories_created_at" ON "public"."templet_categories" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templet_categories_created_by" ON "public"."templet_categories" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templet_categories_deleted" ON "public"."templet_categories" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templet_categories_updated_at" ON "public"."templet_categories" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templet_categories_updated_by" ON "public"."templet_categories" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table templet_categories
-- ----------------------------
ALTER TABLE "public"."templet_categories" ADD CONSTRAINT "PK_templet_categories" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table templets
-- ----------------------------
CREATE INDEX "IX_templets_category_id" ON "public"."templets" USING btree (
  "category_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templets_created_at" ON "public"."templets" USING btree (
  "created_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templets_created_by" ON "public"."templets" USING btree (
  "created_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templets_deleted" ON "public"."templets" USING btree (
  "deleted" "pg_catalog"."bool_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templets_updated_at" ON "public"."templets" USING btree (
  "updated_at" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_templets_updated_by" ON "public"."templets" USING btree (
  "updated_by" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table templets
-- ----------------------------
ALTER TABLE "public"."templets" ADD CONSTRAINT "PK_templets" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table users
-- ----------------------------
ALTER TABLE "public"."users" ADD CONSTRAINT "PK_users" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table app_collaborators
-- ----------------------------
ALTER TABLE "public"."app_collaborators" ADD CONSTRAINT "FK_app_collaborators_apps_app_id" FOREIGN KEY ("app_id") REFERENCES "public"."apps" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."app_collaborators" ADD CONSTRAINT "FK_app_collaborators_teams_team_id" FOREIGN KEY ("team_id") REFERENCES "public"."teams" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;
ALTER TABLE "public"."app_collaborators" ADD CONSTRAINT "FK_app_collaborators_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."app_collaborators" ADD CONSTRAINT "FK_app_collaborators_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;
ALTER TABLE "public"."app_collaborators" ADD CONSTRAINT "FK_app_collaborators_users_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table app_settings
-- ----------------------------
ALTER TABLE "public"."app_settings" ADD CONSTRAINT "FK_app_settings_apps_app_id" FOREIGN KEY ("app_id") REFERENCES "public"."apps" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table apps
-- ----------------------------
ALTER TABLE "public"."apps" ADD CONSTRAINT "FK_apps_organizations_organization_id" FOREIGN KEY ("organization_id") REFERENCES "public"."organizations" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."apps" ADD CONSTRAINT "FK_apps_templets_templet_id" FOREIGN KEY ("templet_id") REFERENCES "public"."templets" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."apps" ADD CONSTRAINT "FK_apps_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."apps" ADD CONSTRAINT "FK_apps_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table deployments
-- ----------------------------
ALTER TABLE "public"."deployments" ADD CONSTRAINT "FK_deployments_apps_app_id" FOREIGN KEY ("app_id") REFERENCES "public"."apps" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."deployments" ADD CONSTRAINT "FK_deployments_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."deployments" ADD CONSTRAINT "FK_deployments_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table organization_users
-- ----------------------------
ALTER TABLE "public"."organization_users" ADD CONSTRAINT "FK_organization_users_organizations_organization_id" FOREIGN KEY ("organization_id") REFERENCES "public"."organizations" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."organization_users" ADD CONSTRAINT "FK_organization_users_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."organization_users" ADD CONSTRAINT "FK_organization_users_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;
ALTER TABLE "public"."organization_users" ADD CONSTRAINT "FK_organization_users_users_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table organizations
-- ----------------------------
ALTER TABLE "public"."organizations" ADD CONSTRAINT "FK_organizations_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."organizations" ADD CONSTRAINT "FK_organizations_users_owner_id" FOREIGN KEY ("owner_id") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."organizations" ADD CONSTRAINT "FK_organizations_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table team_users
-- ----------------------------
ALTER TABLE "public"."team_users" ADD CONSTRAINT "FK_team_users_teams_team_id" FOREIGN KEY ("team_id") REFERENCES "public"."teams" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."team_users" ADD CONSTRAINT "FK_team_users_users_user_id" FOREIGN KEY ("user_id") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table teams
-- ----------------------------
ALTER TABLE "public"."teams" ADD CONSTRAINT "FK_teams_organizations_organization_id" FOREIGN KEY ("organization_id") REFERENCES "public"."organizations" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."teams" ADD CONSTRAINT "FK_teams_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."teams" ADD CONSTRAINT "FK_teams_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table templet_categories
-- ----------------------------
ALTER TABLE "public"."templet_categories" ADD CONSTRAINT "FK_templet_categories_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."templet_categories" ADD CONSTRAINT "FK_templet_categories_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table templets
-- ----------------------------
ALTER TABLE "public"."templets" ADD CONSTRAINT "FK_templets_templet_categories_category_id" FOREIGN KEY ("category_id") REFERENCES "public"."templet_categories" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."templets" ADD CONSTRAINT "FK_templets_users_created_by" FOREIGN KEY ("created_by") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."templets" ADD CONSTRAINT "FK_templets_users_updated_by" FOREIGN KEY ("updated_by") REFERENCES "public"."users" ("id") ON DELETE RESTRICT ON UPDATE NO ACTION;
