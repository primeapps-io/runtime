/*
 Navicat Premium Data Transfer

 Source Server         : Localhost
 Source Server Type    : PostgreSQL
 Source Server Version : 90611
 Source Host           : localhost:5432
 Source Catalog        : auth0
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 90611
 File Encoding         : 65001

 Date: 02/04/2019 11:03:32
*/


-- ----------------------------
-- Sequence structure for ApiClaims_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ApiClaims_Id_seq";
CREATE SEQUENCE "public"."ApiClaims_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ApiClaims_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ApiProperties_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ApiProperties_Id_seq";
CREATE SEQUENCE "public"."ApiProperties_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ApiProperties_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ApiResources_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ApiResources_Id_seq";
CREATE SEQUENCE "public"."ApiResources_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ApiResources_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ApiScopeClaims_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ApiScopeClaims_Id_seq";
CREATE SEQUENCE "public"."ApiScopeClaims_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ApiScopeClaims_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ApiScopes_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ApiScopes_Id_seq";
CREATE SEQUENCE "public"."ApiScopes_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ApiScopes_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ApiSecrets_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ApiSecrets_Id_seq";
CREATE SEQUENCE "public"."ApiSecrets_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ApiSecrets_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for AspNetRoleClaims_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."AspNetRoleClaims_Id_seq";
CREATE SEQUENCE "public"."AspNetRoleClaims_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."AspNetRoleClaims_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for AspNetUserClaims_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."AspNetUserClaims_Id_seq";
CREATE SEQUENCE "public"."AspNetUserClaims_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."AspNetUserClaims_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientClaims_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientClaims_Id_seq";
CREATE SEQUENCE "public"."ClientClaims_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientClaims_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientCorsOrigins_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientCorsOrigins_Id_seq";
CREATE SEQUENCE "public"."ClientCorsOrigins_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientCorsOrigins_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientGrantTypes_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientGrantTypes_Id_seq";
CREATE SEQUENCE "public"."ClientGrantTypes_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientGrantTypes_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientIdPRestrictions_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientIdPRestrictions_Id_seq";
CREATE SEQUENCE "public"."ClientIdPRestrictions_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientIdPRestrictions_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientPostLogoutRedirectUris_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientPostLogoutRedirectUris_Id_seq";
CREATE SEQUENCE "public"."ClientPostLogoutRedirectUris_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientPostLogoutRedirectUris_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientProperties_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientProperties_Id_seq";
CREATE SEQUENCE "public"."ClientProperties_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientProperties_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientRedirectUris_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientRedirectUris_Id_seq";
CREATE SEQUENCE "public"."ClientRedirectUris_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientRedirectUris_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientScopes_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientScopes_Id_seq";
CREATE SEQUENCE "public"."ClientScopes_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientScopes_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ClientSecrets_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."ClientSecrets_Id_seq";
CREATE SEQUENCE "public"."ClientSecrets_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."ClientSecrets_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for Clients_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."Clients_Id_seq";
CREATE SEQUENCE "public"."Clients_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."Clients_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for IdentityClaims_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."IdentityClaims_Id_seq";
CREATE SEQUENCE "public"."IdentityClaims_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."IdentityClaims_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for IdentityProperties_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."IdentityProperties_Id_seq";
CREATE SEQUENCE "public"."IdentityProperties_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."IdentityProperties_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for IdentityResources_Id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."IdentityResources_Id_seq";
CREATE SEQUENCE "public"."IdentityResources_Id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "public"."IdentityResources_Id_seq" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ApiClaims
-- ----------------------------
DROP TABLE IF EXISTS "public"."ApiClaims";
CREATE TABLE "public"."ApiClaims" (
  "Id" int4 NOT NULL DEFAULT nextval('"ApiClaims_Id_seq"'::regclass),
  "ApiResourceId" int4 NOT NULL,
  "Type" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ApiClaims" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ApiProperties
-- ----------------------------
DROP TABLE IF EXISTS "public"."ApiProperties";
CREATE TABLE "public"."ApiProperties" (
  "Id" int4 NOT NULL DEFAULT nextval('"ApiProperties_Id_seq"'::regclass),
  "Key" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "Value" varchar(2000) COLLATE "pg_catalog"."default" NOT NULL,
  "ApiResourceId" int4 NOT NULL
)
;
ALTER TABLE "public"."ApiProperties" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ApiResources
-- ----------------------------
DROP TABLE IF EXISTS "public"."ApiResources";
CREATE TABLE "public"."ApiResources" (
  "Id" int4 NOT NULL DEFAULT nextval('"ApiResources_Id_seq"'::regclass),
  "Description" varchar(1000) COLLATE "pg_catalog"."default",
  "DisplayName" varchar(200) COLLATE "pg_catalog"."default",
  "Enabled" bool NOT NULL,
  "Name" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "Created" timestamp(6) NOT NULL DEFAULT '0001-01-01 00:00:00'::timestamp without time zone,
  "LastAccessed" timestamp(6),
  "NonEditable" bool NOT NULL DEFAULT false,
  "Updated" timestamp(6)
)
;
ALTER TABLE "public"."ApiResources" OWNER TO "postgres";

-- ----------------------------
-- Records of ApiResources
-- ----------------------------
BEGIN;
INSERT INTO "public"."ApiResources" VALUES (1, NULL, 'PrimeApps Api', 't', 'api1', '0001-01-01 00:00:00', NULL, 'f', NULL);
COMMIT;

-- ----------------------------
-- Table structure for ApiScopeClaims
-- ----------------------------
DROP TABLE IF EXISTS "public"."ApiScopeClaims";
CREATE TABLE "public"."ApiScopeClaims" (
  "Id" int4 NOT NULL DEFAULT nextval('"ApiScopeClaims_Id_seq"'::regclass),
  "ApiScopeId" int4 NOT NULL,
  "Type" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ApiScopeClaims" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ApiScopes
-- ----------------------------
DROP TABLE IF EXISTS "public"."ApiScopes";
CREATE TABLE "public"."ApiScopes" (
  "Id" int4 NOT NULL DEFAULT nextval('"ApiScopes_Id_seq"'::regclass),
  "ApiResourceId" int4 NOT NULL,
  "Description" varchar(1000) COLLATE "pg_catalog"."default",
  "DisplayName" varchar(200) COLLATE "pg_catalog"."default",
  "Emphasize" bool NOT NULL,
  "Name" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "Required" bool NOT NULL,
  "ShowInDiscoveryDocument" bool NOT NULL
)
;
ALTER TABLE "public"."ApiScopes" OWNER TO "postgres";

-- ----------------------------
-- Records of ApiScopes
-- ----------------------------
BEGIN;
INSERT INTO "public"."ApiScopes" VALUES (1, 1, NULL, 'PrimeApps Api', 'f', 'api1', 'f', 't');
COMMIT;

-- ----------------------------
-- Table structure for ApiSecrets
-- ----------------------------
DROP TABLE IF EXISTS "public"."ApiSecrets";
CREATE TABLE "public"."ApiSecrets" (
  "Id" int4 NOT NULL DEFAULT nextval('"ApiSecrets_Id_seq"'::regclass),
  "ApiResourceId" int4 NOT NULL,
  "Description" varchar(1000) COLLATE "pg_catalog"."default",
  "Expiration" timestamp(6),
  "Type" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "Value" varchar(4000) COLLATE "pg_catalog"."default" NOT NULL,
  "Created" timestamp(6) NOT NULL DEFAULT '0001-01-01 00:00:00'::timestamp without time zone
)
;
ALTER TABLE "public"."ApiSecrets" OWNER TO "postgres";

-- ----------------------------
-- Table structure for AspNetRoleClaims
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetRoleClaims";
CREATE TABLE "public"."AspNetRoleClaims" (
  "Id" int4 NOT NULL DEFAULT nextval('"AspNetRoleClaims_Id_seq"'::regclass),
  "ClaimType" text COLLATE "pg_catalog"."default",
  "ClaimValue" text COLLATE "pg_catalog"."default",
  "RoleId" text COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."AspNetRoleClaims" OWNER TO "postgres";

-- ----------------------------
-- Table structure for AspNetRoles
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetRoles";
CREATE TABLE "public"."AspNetRoles" (
  "Id" text COLLATE "pg_catalog"."default" NOT NULL,
  "ConcurrencyStamp" text COLLATE "pg_catalog"."default",
  "Name" varchar(256) COLLATE "pg_catalog"."default",
  "NormalizedName" varchar(256) COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."AspNetRoles" OWNER TO "postgres";

-- ----------------------------
-- Table structure for AspNetUserClaims
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetUserClaims";
CREATE TABLE "public"."AspNetUserClaims" (
  "Id" int4 NOT NULL DEFAULT nextval('"AspNetUserClaims_Id_seq"'::regclass),
  "ClaimType" text COLLATE "pg_catalog"."default",
  "ClaimValue" text COLLATE "pg_catalog"."default",
  "UserId" text COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."AspNetUserClaims" OWNER TO "postgres";

-- ----------------------------
-- Records of AspNetUserClaims
-- ----------------------------
BEGIN;
INSERT INTO "public"."AspNetUserClaims" VALUES (3, 'name', 'Master User', '0d7cd43e-deef-431a-a0d1-f20f6fe921e3');
INSERT INTO "public"."AspNetUserClaims" VALUES (4, 'given_name', 'Master', '0d7cd43e-deef-431a-a0d1-f20f6fe921e3');
INSERT INTO "public"."AspNetUserClaims" VALUES (5, 'family_name', 'User', '0d7cd43e-deef-431a-a0d1-f20f6fe921e3');
INSERT INTO "public"."AspNetUserClaims" VALUES (6, 'email', 'app@primeapps.io', '0d7cd43e-deef-431a-a0d1-f20f6fe921e3');
INSERT INTO "public"."AspNetUserClaims" VALUES (7, 'email_verified', 'false', '0d7cd43e-deef-431a-a0d1-f20f6fe921e3');
COMMIT;

-- ----------------------------
-- Table structure for AspNetUserLogins
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetUserLogins";
CREATE TABLE "public"."AspNetUserLogins" (
  "LoginProvider" text COLLATE "pg_catalog"."default" NOT NULL,
  "ProviderKey" text COLLATE "pg_catalog"."default" NOT NULL,
  "ProviderDisplayName" text COLLATE "pg_catalog"."default",
  "UserId" text COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."AspNetUserLogins" OWNER TO "postgres";

-- ----------------------------
-- Table structure for AspNetUserRoles
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetUserRoles";
CREATE TABLE "public"."AspNetUserRoles" (
  "UserId" text COLLATE "pg_catalog"."default" NOT NULL,
  "RoleId" text COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."AspNetUserRoles" OWNER TO "postgres";

-- ----------------------------
-- Table structure for AspNetUserTokens
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetUserTokens";
CREATE TABLE "public"."AspNetUserTokens" (
  "UserId" text COLLATE "pg_catalog"."default" NOT NULL,
  "LoginProvider" text COLLATE "pg_catalog"."default" NOT NULL,
  "Name" text COLLATE "pg_catalog"."default" NOT NULL,
  "Value" text COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."AspNetUserTokens" OWNER TO "postgres";

-- ----------------------------
-- Table structure for AspNetUsers
-- ----------------------------
DROP TABLE IF EXISTS "public"."AspNetUsers";
CREATE TABLE "public"."AspNetUsers" (
  "Id" text COLLATE "pg_catalog"."default" NOT NULL,
  "AccessFailedCount" int4 NOT NULL,
  "ConcurrencyStamp" text COLLATE "pg_catalog"."default",
  "Email" varchar(256) COLLATE "pg_catalog"."default",
  "EmailConfirmed" bool NOT NULL,
  "LockoutEnabled" bool NOT NULL,
  "LockoutEnd" timestamptz(6),
  "NormalizedEmail" varchar(256) COLLATE "pg_catalog"."default",
  "NormalizedUserName" varchar(256) COLLATE "pg_catalog"."default",
  "PasswordHash" text COLLATE "pg_catalog"."default",
  "PhoneNumber" text COLLATE "pg_catalog"."default",
  "PhoneNumberConfirmed" bool NOT NULL,
  "SecurityStamp" text COLLATE "pg_catalog"."default",
  "TwoFactorEnabled" bool NOT NULL,
  "UserName" varchar(256) COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "public"."AspNetUsers" OWNER TO "postgres";

-- ----------------------------
-- Records of AspNetUsers
-- ----------------------------
BEGIN;
INSERT INTO "public"."AspNetUsers" VALUES ('0d7cd43e-deef-431a-a0d1-f20f6fe921e3', 0, '179be37c-65c9-4b86-bfe3-c7c52704ad23', 'app@primeapps.io', 'f', 't', NULL, 'APP@PRIMEAPPS.IO', 'APP@PRIMEAPPS.IO', 'AQAAAAEAACcQAAAAEBdBU5Lii7pd5iC9tMbg4pUxxBcetHfeNmHvu7SZ1iJpZhFKTwzV7Pu1hG2HnP7YbQ==', NULL, 'f', 'MPJYHTSMCS6USI5ENUQNSQBVL6IVC4DN', 'f', 'app@primeapps.io');
COMMIT;

-- ----------------------------
-- Table structure for ClientClaims
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientClaims";
CREATE TABLE "public"."ClientClaims" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientClaims_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "Type" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "Value" varchar(250) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientClaims" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ClientCorsOrigins
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientCorsOrigins";
CREATE TABLE "public"."ClientCorsOrigins" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientCorsOrigins_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "Origin" varchar(150) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientCorsOrigins" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ClientGrantTypes
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientGrantTypes";
CREATE TABLE "public"."ClientGrantTypes" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientGrantTypes_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "GrantType" varchar(250) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientGrantTypes" OWNER TO "postgres";

-- ----------------------------
-- Records of ClientGrantTypes
-- ----------------------------
BEGIN;
INSERT INTO "public"."ClientGrantTypes" VALUES (1, 1, 'hybrid');
INSERT INTO "public"."ClientGrantTypes" VALUES (2, 1, 'client_credentials');
INSERT INTO "public"."ClientGrantTypes" VALUES (3, 2, 'hybrid');
INSERT INTO "public"."ClientGrantTypes" VALUES (4, 2, 'client_credentials');
INSERT INTO "public"."ClientGrantTypes" VALUES (5, 3, 'hybrid');
INSERT INTO "public"."ClientGrantTypes" VALUES (6, 3, 'client_credentials');
COMMIT;

-- ----------------------------
-- Table structure for ClientIdPRestrictions
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientIdPRestrictions";
CREATE TABLE "public"."ClientIdPRestrictions" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientIdPRestrictions_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "Provider" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientIdPRestrictions" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ClientPostLogoutRedirectUris
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientPostLogoutRedirectUris";
CREATE TABLE "public"."ClientPostLogoutRedirectUris" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientPostLogoutRedirectUris_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "PostLogoutRedirectUri" varchar(2000) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientPostLogoutRedirectUris" OWNER TO "postgres";

-- ----------------------------
-- Records of ClientPostLogoutRedirectUris
-- ----------------------------
BEGIN;
INSERT INTO "public"."ClientPostLogoutRedirectUris" VALUES (1, 1, 'https://auth.primeapps.io/signout-callback-oidc');
INSERT INTO "public"."ClientPostLogoutRedirectUris" VALUES (2, 2, 'https://auth.primeapps.io/signout-callback-oidc');
INSERT INTO "public"."ClientPostLogoutRedirectUris" VALUES (3, 3, 'https://auth.primeapps.io/signout-callback-oidc');
COMMIT;

-- ----------------------------
-- Table structure for ClientProperties
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientProperties";
CREATE TABLE "public"."ClientProperties" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientProperties_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "Key" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "Value" varchar(2000) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientProperties" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ClientRedirectUris
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientRedirectUris";
CREATE TABLE "public"."ClientRedirectUris" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientRedirectUris_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "RedirectUri" varchar(2000) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientRedirectUris" OWNER TO "postgres";

-- ----------------------------
-- Records of ClientRedirectUris
-- ----------------------------
BEGIN;
INSERT INTO "public"."ClientRedirectUris" VALUES (1, 1, 'https://app.primeapps.io/signin-oidc');
INSERT INTO "public"."ClientRedirectUris" VALUES (2, 2, 'https://studio.primeapps.io/signin-oidc');
INSERT INTO "public"."ClientRedirectUris" VALUES (3, 3, 'https://preview.primeapps.io/signin-oidc');
COMMIT;

-- ----------------------------
-- Table structure for ClientScopes
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientScopes";
CREATE TABLE "public"."ClientScopes" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientScopes_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "Scope" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."ClientScopes" OWNER TO "postgres";

-- ----------------------------
-- Records of ClientScopes
-- ----------------------------
BEGIN;
INSERT INTO "public"."ClientScopes" VALUES (1, 1, 'email');
INSERT INTO "public"."ClientScopes" VALUES (2, 1, 'profile');
INSERT INTO "public"."ClientScopes" VALUES (3, 1, 'openid');
INSERT INTO "public"."ClientScopes" VALUES (4, 1, 'api1');
INSERT INTO "public"."ClientScopes" VALUES (6, 2, 'openid');
INSERT INTO "public"."ClientScopes" VALUES (7, 2, 'profile');
INSERT INTO "public"."ClientScopes" VALUES (8, 2, 'email');
INSERT INTO "public"."ClientScopes" VALUES (9, 2, 'api1');
INSERT INTO "public"."ClientScopes" VALUES (10, 3, 'openid');
INSERT INTO "public"."ClientScopes" VALUES (11, 3, 'profile');
INSERT INTO "public"."ClientScopes" VALUES (12, 3, 'email');
INSERT INTO "public"."ClientScopes" VALUES (13, 3, 'api1');
COMMIT;

-- ----------------------------
-- Table structure for ClientSecrets
-- ----------------------------
DROP TABLE IF EXISTS "public"."ClientSecrets";
CREATE TABLE "public"."ClientSecrets" (
  "Id" int4 NOT NULL DEFAULT nextval('"ClientSecrets_Id_seq"'::regclass),
  "ClientId" int4 NOT NULL,
  "Description" varchar(2000) COLLATE "pg_catalog"."default",
  "Expiration" timestamp(6),
  "Type" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "Value" varchar(4000) COLLATE "pg_catalog"."default" NOT NULL,
  "Created" timestamp(6) NOT NULL DEFAULT '0001-01-01 00:00:00'::timestamp without time zone
)
;
ALTER TABLE "public"."ClientSecrets" OWNER TO "postgres";

-- ----------------------------
-- Records of ClientSecrets
-- ----------------------------
BEGIN;
INSERT INTO "public"."ClientSecrets" VALUES (1, 1, NULL, NULL, 'SharedSecret', 'K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=', '0001-01-01 00:00:00');
INSERT INTO "public"."ClientSecrets" VALUES (3, 2, NULL, NULL, 'SharedSecret', 'K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=', '2019-02-17 22:41:42.927658');
INSERT INTO "public"."ClientSecrets" VALUES (4, 3, NULL, NULL, 'SharedSecret', 'K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=', '2019-02-17 22:45:10.603686');
COMMIT;

-- ----------------------------
-- Table structure for Clients
-- ----------------------------
DROP TABLE IF EXISTS "public"."Clients";
CREATE TABLE "public"."Clients" (
  "Id" int4 NOT NULL DEFAULT nextval('"Clients_Id_seq"'::regclass),
  "AbsoluteRefreshTokenLifetime" int4 NOT NULL,
  "AccessTokenLifetime" int4 NOT NULL,
  "AccessTokenType" int4 NOT NULL,
  "AllowAccessTokensViaBrowser" bool NOT NULL,
  "AllowOfflineAccess" bool NOT NULL,
  "AllowPlainTextPkce" bool NOT NULL,
  "AllowRememberConsent" bool NOT NULL,
  "AlwaysIncludeUserClaimsInIdToken" bool NOT NULL,
  "AlwaysSendClientClaims" bool NOT NULL,
  "AuthorizationCodeLifetime" int4 NOT NULL,
  "BackChannelLogoutSessionRequired" bool NOT NULL,
  "BackChannelLogoutUri" varchar(2000) COLLATE "pg_catalog"."default",
  "ClientClaimsPrefix" varchar(200) COLLATE "pg_catalog"."default",
  "ClientId" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "ClientName" varchar(200) COLLATE "pg_catalog"."default",
  "ClientUri" varchar(2000) COLLATE "pg_catalog"."default",
  "ConsentLifetime" int4,
  "Description" varchar(1000) COLLATE "pg_catalog"."default",
  "EnableLocalLogin" bool NOT NULL,
  "Enabled" bool NOT NULL,
  "FrontChannelLogoutSessionRequired" bool NOT NULL,
  "FrontChannelLogoutUri" varchar(2000) COLLATE "pg_catalog"."default",
  "IdentityTokenLifetime" int4 NOT NULL,
  "IncludeJwtId" bool NOT NULL,
  "LogoUri" varchar(2000) COLLATE "pg_catalog"."default",
  "PairWiseSubjectSalt" varchar(200) COLLATE "pg_catalog"."default",
  "ProtocolType" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "RefreshTokenExpiration" int4 NOT NULL,
  "RefreshTokenUsage" int4 NOT NULL,
  "RequireClientSecret" bool NOT NULL,
  "RequireConsent" bool NOT NULL,
  "RequirePkce" bool NOT NULL,
  "SlidingRefreshTokenLifetime" int4 NOT NULL,
  "UpdateAccessTokenClaimsOnRefresh" bool NOT NULL,
  "Created" timestamp(6) NOT NULL DEFAULT '0001-01-01 00:00:00'::timestamp without time zone,
  "DeviceCodeLifetime" int4 NOT NULL DEFAULT 0,
  "LastAccessed" timestamp(6),
  "NonEditable" bool NOT NULL DEFAULT false,
  "Updated" timestamp(6),
  "UserCodeType" varchar(100) COLLATE "pg_catalog"."default",
  "UserSsoLifetime" int4
)
;
ALTER TABLE "public"."Clients" OWNER TO "postgres";

-- ----------------------------
-- Records of Clients
-- ----------------------------
BEGIN;
INSERT INTO "public"."Clients" VALUES (2, 2592000, 864000, 0, 'f', 'f', 'f', 'f', 'f', 't', 300, 't', NULL, 'client_', 'primeapps_studio', 'PrimeApps Studio', NULL, NULL, NULL, 't', 't', 't', NULL, 300, 'f', NULL, NULL, 'oidc', 1, 1, 't', 'f', 'f', 1296000, 'f', '2019-02-17 22:41:42.926301', 300, NULL, 'f', NULL, NULL, NULL);
INSERT INTO "public"."Clients" VALUES (1, 2592000, 864000, 0, 'f', 'f', 'f', 'f', 'f', 't', 300, 't', NULL, 'client_', 'primeapps_app', 'PrimeApps App', '', NULL, NULL, 't', 't', 't', NULL, 300, 'f', NULL, NULL, 'oidc', 1, 1, 't', 'f', 'f', 1296000, 'f', '0001-01-01 00:00:00', 0, NULL, 'f', NULL, NULL, NULL);
INSERT INTO "public"."Clients" VALUES (3, 2592000, 864000, 0, 'f', 'f', 'f', 'f', 'f', 't', 300, 't', NULL, 'client_', 'primeapps_preview', 'PrimeApps Preview', NULL, NULL, NULL, 't', 't', 't', NULL, 300, 'f', NULL, NULL, 'oidc', 1, 1, 't', 'f', 'f', 1296000, 'f', '2019-02-17 22:45:10.602295', 300, NULL, 'f', NULL, NULL, NULL);
COMMIT;

-- ----------------------------
-- Table structure for DeviceCodes
-- ----------------------------
DROP TABLE IF EXISTS "public"."DeviceCodes";
CREATE TABLE "public"."DeviceCodes" (
  "UserCode" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "DeviceCode" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "SubjectId" varchar(200) COLLATE "pg_catalog"."default",
  "ClientId" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "CreationTime" timestamp(6) NOT NULL,
  "Expiration" timestamp(6) NOT NULL,
  "Data" varchar(50000) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."DeviceCodes" OWNER TO "postgres";

-- ----------------------------
-- Table structure for IdentityClaims
-- ----------------------------
DROP TABLE IF EXISTS "public"."IdentityClaims";
CREATE TABLE "public"."IdentityClaims" (
  "Id" int4 NOT NULL DEFAULT nextval('"IdentityClaims_Id_seq"'::regclass),
  "IdentityResourceId" int4 NOT NULL,
  "Type" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."IdentityClaims" OWNER TO "postgres";

-- ----------------------------
-- Records of IdentityClaims
-- ----------------------------
BEGIN;
INSERT INTO "public"."IdentityClaims" VALUES (1, 1, 'sub');
INSERT INTO "public"."IdentityClaims" VALUES (2, 3, 'zoneinfo');
INSERT INTO "public"."IdentityClaims" VALUES (3, 3, 'birthdate');
INSERT INTO "public"."IdentityClaims" VALUES (4, 3, 'gender');
INSERT INTO "public"."IdentityClaims" VALUES (5, 3, 'website');
INSERT INTO "public"."IdentityClaims" VALUES (6, 3, 'picture');
INSERT INTO "public"."IdentityClaims" VALUES (7, 3, 'profile');
INSERT INTO "public"."IdentityClaims" VALUES (8, 3, 'locale');
INSERT INTO "public"."IdentityClaims" VALUES (9, 3, 'preferred_username');
INSERT INTO "public"."IdentityClaims" VALUES (10, 3, 'middle_name');
INSERT INTO "public"."IdentityClaims" VALUES (11, 3, 'given_name');
INSERT INTO "public"."IdentityClaims" VALUES (12, 3, 'family_name');
INSERT INTO "public"."IdentityClaims" VALUES (13, 3, 'name');
INSERT INTO "public"."IdentityClaims" VALUES (14, 2, 'email_verified');
INSERT INTO "public"."IdentityClaims" VALUES (15, 2, 'email');
INSERT INTO "public"."IdentityClaims" VALUES (16, 3, 'nickname');
INSERT INTO "public"."IdentityClaims" VALUES (17, 3, 'updated_at');
COMMIT;

-- ----------------------------
-- Table structure for IdentityProperties
-- ----------------------------
DROP TABLE IF EXISTS "public"."IdentityProperties";
CREATE TABLE "public"."IdentityProperties" (
  "Id" int4 NOT NULL DEFAULT nextval('"IdentityProperties_Id_seq"'::regclass),
  "Key" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "Value" varchar(2000) COLLATE "pg_catalog"."default" NOT NULL,
  "IdentityResourceId" int4 NOT NULL
)
;
ALTER TABLE "public"."IdentityProperties" OWNER TO "postgres";

-- ----------------------------
-- Table structure for IdentityResources
-- ----------------------------
DROP TABLE IF EXISTS "public"."IdentityResources";
CREATE TABLE "public"."IdentityResources" (
  "Id" int4 NOT NULL DEFAULT nextval('"IdentityResources_Id_seq"'::regclass),
  "Description" varchar(1000) COLLATE "pg_catalog"."default",
  "DisplayName" varchar(200) COLLATE "pg_catalog"."default",
  "Emphasize" bool NOT NULL,
  "Enabled" bool NOT NULL,
  "Name" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "Required" bool NOT NULL,
  "ShowInDiscoveryDocument" bool NOT NULL,
  "Created" timestamp(6) NOT NULL DEFAULT '0001-01-01 00:00:00'::timestamp without time zone,
  "NonEditable" bool NOT NULL DEFAULT false,
  "Updated" timestamp(6)
)
;
ALTER TABLE "public"."IdentityResources" OWNER TO "postgres";

-- ----------------------------
-- Records of IdentityResources
-- ----------------------------
BEGIN;
INSERT INTO "public"."IdentityResources" VALUES (1, NULL, 'Your user identifier', 'f', 't', 'openid', 't', 't', '0001-01-01 00:00:00', 'f', NULL);
INSERT INTO "public"."IdentityResources" VALUES (2, NULL, 'Your email address', 't', 't', 'email', 'f', 't', '0001-01-01 00:00:00', 'f', NULL);
INSERT INTO "public"."IdentityResources" VALUES (3, 'Your user profile information (first name, last name, etc.)', 'User profile', 't', 't', 'profile', 'f', 't', '0001-01-01 00:00:00', 'f', NULL);
COMMIT;

-- ----------------------------
-- Table structure for PersistedGrants
-- ----------------------------
DROP TABLE IF EXISTS "public"."PersistedGrants";
CREATE TABLE "public"."PersistedGrants" (
  "Key" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "ClientId" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "CreationTime" timestamp(6) NOT NULL,
  "Data" varchar(50000) COLLATE "pg_catalog"."default" NOT NULL,
  "Expiration" timestamp(6),
  "SubjectId" varchar(200) COLLATE "pg_catalog"."default",
  "Type" varchar(50) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."PersistedGrants" OWNER TO "postgres";

-- ----------------------------
-- Table structure for __EFMigrationsHistory
-- ----------------------------
DROP TABLE IF EXISTS "public"."__EFMigrationsHistory";
CREATE TABLE "public"."__EFMigrationsHistory" (
  "MigrationId" varchar(150) COLLATE "pg_catalog"."default" NOT NULL,
  "ProductVersion" varchar(32) COLLATE "pg_catalog"."default" NOT NULL
)
;
ALTER TABLE "public"."__EFMigrationsHistory" OWNER TO "postgres";

-- ----------------------------
-- Records of __EFMigrationsHistory
-- ----------------------------
BEGIN;
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('00000000000000_CreateIdentitySchema', '2.2.0-rtm-35687');
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('20180508135334_InitialIdentityServerPersistedGrantDbMigration', '2.2.0-rtm-35687');
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('20180531085523_IdentityServerPersistedGrantDbMigrationVersionUpgrade', '2.2.0-rtm-35687');
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('20190102170309_PersistedGrantDbContextUpgrade2', '2.2.0-rtm-35687');
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('20180508135535_InitialIdentityServerConfigurationDbMigration', '2.2.0-rtm-35687');
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('20180531085557_IdentityServerConfigurationDbMigrationVersionUpgrade', '2.2.0-rtm-35687');
INSERT INTO "public"."__EFMigrationsHistory" VALUES ('20190102170417_ConfigurationDbContextUpgrade2', '2.2.0-rtm-35687');
COMMIT;

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."ApiClaims_Id_seq"
OWNED BY "public"."ApiClaims"."Id";
SELECT setval('"public"."ApiClaims_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ApiProperties_Id_seq"
OWNED BY "public"."ApiProperties"."Id";
SELECT setval('"public"."ApiProperties_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ApiResources_Id_seq"
OWNED BY "public"."ApiResources"."Id";
SELECT setval('"public"."ApiResources_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ApiScopeClaims_Id_seq"
OWNED BY "public"."ApiScopeClaims"."Id";
SELECT setval('"public"."ApiScopeClaims_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ApiScopes_Id_seq"
OWNED BY "public"."ApiScopes"."Id";
SELECT setval('"public"."ApiScopes_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ApiSecrets_Id_seq"
OWNED BY "public"."ApiSecrets"."Id";
SELECT setval('"public"."ApiSecrets_Id_seq"', 5, false);
ALTER SEQUENCE "public"."AspNetRoleClaims_Id_seq"
OWNED BY "public"."AspNetRoleClaims"."Id";
SELECT setval('"public"."AspNetRoleClaims_Id_seq"', 5, false);
ALTER SEQUENCE "public"."AspNetUserClaims_Id_seq"
OWNED BY "public"."AspNetUserClaims"."Id";
SELECT setval('"public"."AspNetUserClaims_Id_seq"', 9, true);
ALTER SEQUENCE "public"."ClientClaims_Id_seq"
OWNED BY "public"."ClientClaims"."Id";
SELECT setval('"public"."ClientClaims_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ClientCorsOrigins_Id_seq"
OWNED BY "public"."ClientCorsOrigins"."Id";
SELECT setval('"public"."ClientCorsOrigins_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ClientGrantTypes_Id_seq"
OWNED BY "public"."ClientGrantTypes"."Id";
SELECT setval('"public"."ClientGrantTypes_Id_seq"', 11, true);
ALTER SEQUENCE "public"."ClientIdPRestrictions_Id_seq"
OWNED BY "public"."ClientIdPRestrictions"."Id";
SELECT setval('"public"."ClientIdPRestrictions_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ClientPostLogoutRedirectUris_Id_seq"
OWNED BY "public"."ClientPostLogoutRedirectUris"."Id";
SELECT setval('"public"."ClientPostLogoutRedirectUris_Id_seq"', 8, true);
ALTER SEQUENCE "public"."ClientProperties_Id_seq"
OWNED BY "public"."ClientProperties"."Id";
SELECT setval('"public"."ClientProperties_Id_seq"', 5, false);
ALTER SEQUENCE "public"."ClientRedirectUris_Id_seq"
OWNED BY "public"."ClientRedirectUris"."Id";
SELECT setval('"public"."ClientRedirectUris_Id_seq"', 7, true);
ALTER SEQUENCE "public"."ClientScopes_Id_seq"
OWNED BY "public"."ClientScopes"."Id";
SELECT setval('"public"."ClientScopes_Id_seq"', 20, true);
ALTER SEQUENCE "public"."ClientSecrets_Id_seq"
OWNED BY "public"."ClientSecrets"."Id";
SELECT setval('"public"."ClientSecrets_Id_seq"', 8, true);
ALTER SEQUENCE "public"."Clients_Id_seq"
OWNED BY "public"."Clients"."Id";
SELECT setval('"public"."Clients_Id_seq"', 5, false);
ALTER SEQUENCE "public"."IdentityClaims_Id_seq"
OWNED BY "public"."IdentityClaims"."Id";
SELECT setval('"public"."IdentityClaims_Id_seq"', 5, false);
ALTER SEQUENCE "public"."IdentityProperties_Id_seq"
OWNED BY "public"."IdentityProperties"."Id";
SELECT setval('"public"."IdentityProperties_Id_seq"', 5, false);
ALTER SEQUENCE "public"."IdentityResources_Id_seq"
OWNED BY "public"."IdentityResources"."Id";
SELECT setval('"public"."IdentityResources_Id_seq"', 5, false);

-- ----------------------------
-- Indexes structure for table ApiClaims
-- ----------------------------
CREATE INDEX "IX_ApiClaims_ApiResourceId" ON "public"."ApiClaims" USING btree (
  "ApiResourceId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ApiClaims
-- ----------------------------
ALTER TABLE "public"."ApiClaims" ADD CONSTRAINT "PK_ApiClaims" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ApiProperties
-- ----------------------------
CREATE INDEX "IX_ApiProperties_ApiResourceId" ON "public"."ApiProperties" USING btree (
  "ApiResourceId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ApiProperties
-- ----------------------------
ALTER TABLE "public"."ApiProperties" ADD CONSTRAINT "PK_ApiProperties" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ApiResources
-- ----------------------------
CREATE UNIQUE INDEX "IX_ApiResources_Name" ON "public"."ApiResources" USING btree (
  "Name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ApiResources
-- ----------------------------
ALTER TABLE "public"."ApiResources" ADD CONSTRAINT "PK_ApiResources" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ApiScopeClaims
-- ----------------------------
CREATE INDEX "IX_ApiScopeClaims_ApiScopeId" ON "public"."ApiScopeClaims" USING btree (
  "ApiScopeId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ApiScopeClaims
-- ----------------------------
ALTER TABLE "public"."ApiScopeClaims" ADD CONSTRAINT "PK_ApiScopeClaims" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ApiScopes
-- ----------------------------
CREATE INDEX "IX_ApiScopes_ApiResourceId" ON "public"."ApiScopes" USING btree (
  "ApiResourceId" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "IX_ApiScopes_Name" ON "public"."ApiScopes" USING btree (
  "Name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ApiScopes
-- ----------------------------
ALTER TABLE "public"."ApiScopes" ADD CONSTRAINT "PK_ApiScopes" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ApiSecrets
-- ----------------------------
CREATE INDEX "IX_ApiSecrets_ApiResourceId" ON "public"."ApiSecrets" USING btree (
  "ApiResourceId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ApiSecrets
-- ----------------------------
ALTER TABLE "public"."ApiSecrets" ADD CONSTRAINT "PK_ApiSecrets" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table AspNetRoleClaims
-- ----------------------------
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "public"."AspNetRoleClaims" USING btree (
  "RoleId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AspNetRoleClaims
-- ----------------------------
ALTER TABLE "public"."AspNetRoleClaims" ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table AspNetRoles
-- ----------------------------
CREATE INDEX "RoleNameIndex" ON "public"."AspNetRoles" USING btree (
  "NormalizedName" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AspNetRoles
-- ----------------------------
ALTER TABLE "public"."AspNetRoles" ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table AspNetUserClaims
-- ----------------------------
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "public"."AspNetUserClaims" USING btree (
  "UserId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AspNetUserClaims
-- ----------------------------
ALTER TABLE "public"."AspNetUserClaims" ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table AspNetUserLogins
-- ----------------------------
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "public"."AspNetUserLogins" USING btree (
  "UserId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AspNetUserLogins
-- ----------------------------
ALTER TABLE "public"."AspNetUserLogins" ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");

-- ----------------------------
-- Indexes structure for table AspNetUserRoles
-- ----------------------------
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "public"."AspNetUserRoles" USING btree (
  "RoleId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_AspNetUserRoles_UserId" ON "public"."AspNetUserRoles" USING btree (
  "UserId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AspNetUserRoles
-- ----------------------------
ALTER TABLE "public"."AspNetUserRoles" ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");

-- ----------------------------
-- Primary Key structure for table AspNetUserTokens
-- ----------------------------
ALTER TABLE "public"."AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");

-- ----------------------------
-- Indexes structure for table AspNetUsers
-- ----------------------------
CREATE INDEX "EmailIndex" ON "public"."AspNetUsers" USING btree (
  "NormalizedEmail" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "UserNameIndex" ON "public"."AspNetUsers" USING btree (
  "NormalizedUserName" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AspNetUsers
-- ----------------------------
ALTER TABLE "public"."AspNetUsers" ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientClaims
-- ----------------------------
CREATE INDEX "IX_ClientClaims_ClientId" ON "public"."ClientClaims" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientClaims
-- ----------------------------
ALTER TABLE "public"."ClientClaims" ADD CONSTRAINT "PK_ClientClaims" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientCorsOrigins
-- ----------------------------
CREATE INDEX "IX_ClientCorsOrigins_ClientId" ON "public"."ClientCorsOrigins" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientCorsOrigins
-- ----------------------------
ALTER TABLE "public"."ClientCorsOrigins" ADD CONSTRAINT "PK_ClientCorsOrigins" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientGrantTypes
-- ----------------------------
CREATE INDEX "IX_ClientGrantTypes_ClientId" ON "public"."ClientGrantTypes" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientGrantTypes
-- ----------------------------
ALTER TABLE "public"."ClientGrantTypes" ADD CONSTRAINT "PK_ClientGrantTypes" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientIdPRestrictions
-- ----------------------------
CREATE INDEX "IX_ClientIdPRestrictions_ClientId" ON "public"."ClientIdPRestrictions" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientIdPRestrictions
-- ----------------------------
ALTER TABLE "public"."ClientIdPRestrictions" ADD CONSTRAINT "PK_ClientIdPRestrictions" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientPostLogoutRedirectUris
-- ----------------------------
CREATE INDEX "IX_ClientPostLogoutRedirectUris_ClientId" ON "public"."ClientPostLogoutRedirectUris" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientPostLogoutRedirectUris
-- ----------------------------
ALTER TABLE "public"."ClientPostLogoutRedirectUris" ADD CONSTRAINT "PK_ClientPostLogoutRedirectUris" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientProperties
-- ----------------------------
CREATE INDEX "IX_ClientProperties_ClientId" ON "public"."ClientProperties" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientProperties
-- ----------------------------
ALTER TABLE "public"."ClientProperties" ADD CONSTRAINT "PK_ClientProperties" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientRedirectUris
-- ----------------------------
CREATE INDEX "IX_ClientRedirectUris_ClientId" ON "public"."ClientRedirectUris" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientRedirectUris
-- ----------------------------
ALTER TABLE "public"."ClientRedirectUris" ADD CONSTRAINT "PK_ClientRedirectUris" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientScopes
-- ----------------------------
CREATE INDEX "IX_ClientScopes_ClientId" ON "public"."ClientScopes" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientScopes
-- ----------------------------
ALTER TABLE "public"."ClientScopes" ADD CONSTRAINT "PK_ClientScopes" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table ClientSecrets
-- ----------------------------
CREATE INDEX "IX_ClientSecrets_ClientId" ON "public"."ClientSecrets" USING btree (
  "ClientId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ClientSecrets
-- ----------------------------
ALTER TABLE "public"."ClientSecrets" ADD CONSTRAINT "PK_ClientSecrets" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table Clients
-- ----------------------------
CREATE UNIQUE INDEX "IX_Clients_ClientId" ON "public"."Clients" USING btree (
  "ClientId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table Clients
-- ----------------------------
ALTER TABLE "public"."Clients" ADD CONSTRAINT "PK_Clients" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table DeviceCodes
-- ----------------------------
CREATE UNIQUE INDEX "IX_DeviceCodes_DeviceCode" ON "public"."DeviceCodes" USING btree (
  "DeviceCode" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table DeviceCodes
-- ----------------------------
ALTER TABLE "public"."DeviceCodes" ADD CONSTRAINT "PK_DeviceCodes" PRIMARY KEY ("UserCode");

-- ----------------------------
-- Indexes structure for table IdentityClaims
-- ----------------------------
CREATE INDEX "IX_IdentityClaims_IdentityResourceId" ON "public"."IdentityClaims" USING btree (
  "IdentityResourceId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table IdentityClaims
-- ----------------------------
ALTER TABLE "public"."IdentityClaims" ADD CONSTRAINT "PK_IdentityClaims" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table IdentityProperties
-- ----------------------------
CREATE INDEX "IX_IdentityProperties_IdentityResourceId" ON "public"."IdentityProperties" USING btree (
  "IdentityResourceId" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table IdentityProperties
-- ----------------------------
ALTER TABLE "public"."IdentityProperties" ADD CONSTRAINT "PK_IdentityProperties" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table IdentityResources
-- ----------------------------
CREATE UNIQUE INDEX "IX_IdentityResources_Name" ON "public"."IdentityResources" USING btree (
  "Name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table IdentityResources
-- ----------------------------
ALTER TABLE "public"."IdentityResources" ADD CONSTRAINT "PK_IdentityResources" PRIMARY KEY ("Id");

-- ----------------------------
-- Indexes structure for table PersistedGrants
-- ----------------------------
CREATE INDEX "IX_PersistedGrants_SubjectId_ClientId_Type" ON "public"."PersistedGrants" USING btree (
  "SubjectId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "ClientId" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "Type" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table PersistedGrants
-- ----------------------------
ALTER TABLE "public"."PersistedGrants" ADD CONSTRAINT "PK_PersistedGrants" PRIMARY KEY ("Key");

-- ----------------------------
-- Primary Key structure for table __EFMigrationsHistory
-- ----------------------------
ALTER TABLE "public"."__EFMigrationsHistory" ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");

-- ----------------------------
-- Foreign Keys structure for table ApiClaims
-- ----------------------------
ALTER TABLE "public"."ApiClaims" ADD CONSTRAINT "FK_ApiClaims_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "public"."ApiResources" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ApiProperties
-- ----------------------------
ALTER TABLE "public"."ApiProperties" ADD CONSTRAINT "FK_ApiProperties_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "public"."ApiResources" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ApiScopeClaims
-- ----------------------------
ALTER TABLE "public"."ApiScopeClaims" ADD CONSTRAINT "FK_ApiScopeClaims_ApiScopes_ApiScopeId" FOREIGN KEY ("ApiScopeId") REFERENCES "public"."ApiScopes" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ApiScopes
-- ----------------------------
ALTER TABLE "public"."ApiScopes" ADD CONSTRAINT "FK_ApiScopes_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "public"."ApiResources" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ApiSecrets
-- ----------------------------
ALTER TABLE "public"."ApiSecrets" ADD CONSTRAINT "FK_ApiSecrets_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "public"."ApiResources" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table AspNetRoleClaims
-- ----------------------------
ALTER TABLE "public"."AspNetRoleClaims" ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "public"."AspNetRoles" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table AspNetUserClaims
-- ----------------------------
ALTER TABLE "public"."AspNetUserClaims" ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "public"."AspNetUsers" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table AspNetUserLogins
-- ----------------------------
ALTER TABLE "public"."AspNetUserLogins" ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "public"."AspNetUsers" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table AspNetUserRoles
-- ----------------------------
ALTER TABLE "public"."AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "public"."AspNetRoles" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;
ALTER TABLE "public"."AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "public"."AspNetUsers" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientClaims
-- ----------------------------
ALTER TABLE "public"."ClientClaims" ADD CONSTRAINT "FK_ClientClaims_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientCorsOrigins
-- ----------------------------
ALTER TABLE "public"."ClientCorsOrigins" ADD CONSTRAINT "FK_ClientCorsOrigins_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientGrantTypes
-- ----------------------------
ALTER TABLE "public"."ClientGrantTypes" ADD CONSTRAINT "FK_ClientGrantTypes_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientIdPRestrictions
-- ----------------------------
ALTER TABLE "public"."ClientIdPRestrictions" ADD CONSTRAINT "FK_ClientIdPRestrictions_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientPostLogoutRedirectUris
-- ----------------------------
ALTER TABLE "public"."ClientPostLogoutRedirectUris" ADD CONSTRAINT "FK_ClientPostLogoutRedirectUris_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientProperties
-- ----------------------------
ALTER TABLE "public"."ClientProperties" ADD CONSTRAINT "FK_ClientProperties_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientRedirectUris
-- ----------------------------
ALTER TABLE "public"."ClientRedirectUris" ADD CONSTRAINT "FK_ClientRedirectUris_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientScopes
-- ----------------------------
ALTER TABLE "public"."ClientScopes" ADD CONSTRAINT "FK_ClientScopes_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ClientSecrets
-- ----------------------------
ALTER TABLE "public"."ClientSecrets" ADD CONSTRAINT "FK_ClientSecrets_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "public"."Clients" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table IdentityClaims
-- ----------------------------
ALTER TABLE "public"."IdentityClaims" ADD CONSTRAINT "FK_IdentityClaims_IdentityResources_IdentityResourceId" FOREIGN KEY ("IdentityResourceId") REFERENCES "public"."IdentityResources" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table IdentityProperties
-- ----------------------------
ALTER TABLE "public"."IdentityProperties" ADD CONSTRAINT "FK_IdentityProperties_IdentityResources_IdentityResourceId" FOREIGN KEY ("IdentityResourceId") REFERENCES "public"."IdentityResources" ("Id") ON DELETE CASCADE ON UPDATE NO ACTION;
