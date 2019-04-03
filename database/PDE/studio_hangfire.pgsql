/*
 Navicat Premium Data Transfer

 Source Server         : Localhost
 Source Server Type    : PostgreSQL
 Source Server Version : 90611
 Source Host           : localhost:5432
 Source Catalog        : studio
 Source Schema         : hangfire

 Target Server Type    : PostgreSQL
 Target Server Version : 90611
 File Encoding         : 65001

 Date: 03/04/2019 13:03:33
*/


-- ----------------------------
-- Sequence structure for counter_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."counter_id_seq";
CREATE SEQUENCE "hangfire"."counter_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."counter_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for hash_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."hash_id_seq";
CREATE SEQUENCE "hangfire"."hash_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."hash_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for job_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."job_id_seq";
CREATE SEQUENCE "hangfire"."job_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."job_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for jobparameter_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."jobparameter_id_seq";
CREATE SEQUENCE "hangfire"."jobparameter_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."jobparameter_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for jobqueue_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."jobqueue_id_seq";
CREATE SEQUENCE "hangfire"."jobqueue_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."jobqueue_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for list_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."list_id_seq";
CREATE SEQUENCE "hangfire"."list_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."list_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for set_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."set_id_seq";
CREATE SEQUENCE "hangfire"."set_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."set_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for state_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "hangfire"."state_id_seq";
CREATE SEQUENCE "hangfire"."state_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "hangfire"."state_id_seq" OWNER TO "postgres";

-- ----------------------------
-- Table structure for counter
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."counter";
CREATE TABLE "hangfire"."counter" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".counter_id_seq'::regclass),
  "key" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "value" int2 NOT NULL,
  "expireat" timestamp(6),
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."counter" OWNER TO "postgres";

-- ----------------------------
-- Table structure for hash
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."hash";
CREATE TABLE "hangfire"."hash" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".hash_id_seq'::regclass),
  "key" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "field" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "value" text COLLATE "pg_catalog"."default",
  "expireat" timestamp(6),
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."hash" OWNER TO "postgres";

-- ----------------------------
-- Table structure for job
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."job";
CREATE TABLE "hangfire"."job" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".job_id_seq'::regclass),
  "stateid" int4,
  "statename" varchar(20) COLLATE "pg_catalog"."default",
  "invocationdata" text COLLATE "pg_catalog"."default" NOT NULL,
  "arguments" text COLLATE "pg_catalog"."default" NOT NULL,
  "createdat" timestamp(6) NOT NULL,
  "expireat" timestamp(6),
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."job" OWNER TO "postgres";

-- ----------------------------
-- Table structure for jobparameter
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."jobparameter";
CREATE TABLE "hangfire"."jobparameter" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".jobparameter_id_seq'::regclass),
  "jobid" int4 NOT NULL,
  "name" varchar(40) COLLATE "pg_catalog"."default" NOT NULL,
  "value" text COLLATE "pg_catalog"."default",
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."jobparameter" OWNER TO "postgres";

-- ----------------------------
-- Table structure for jobqueue
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."jobqueue";
CREATE TABLE "hangfire"."jobqueue" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".jobqueue_id_seq'::regclass),
  "jobid" int4 NOT NULL,
  "queue" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "fetchedat" timestamp(6),
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."jobqueue" OWNER TO "postgres";

-- ----------------------------
-- Table structure for list
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."list";
CREATE TABLE "hangfire"."list" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".list_id_seq'::regclass),
  "key" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "value" text COLLATE "pg_catalog"."default",
  "expireat" timestamp(6),
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."list" OWNER TO "postgres";

-- ----------------------------
-- Table structure for lock
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."lock";
CREATE TABLE "hangfire"."lock" (
  "resource" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."lock" OWNER TO "postgres";

-- ----------------------------
-- Table structure for schema
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."schema";
CREATE TABLE "hangfire"."schema" (
  "version" int4 NOT NULL
)
;
ALTER TABLE "hangfire"."schema" OWNER TO "postgres";

-- ----------------------------
-- Records of schema
-- ----------------------------
BEGIN;
INSERT INTO "hangfire"."schema" VALUES (6);
COMMIT;

-- ----------------------------
-- Table structure for server
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."server";
CREATE TABLE "hangfire"."server" (
  "id" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "data" text COLLATE "pg_catalog"."default",
  "lastheartbeat" timestamp(6) NOT NULL,
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."server" OWNER TO "postgres";

-- ----------------------------
-- Table structure for set
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."set";
CREATE TABLE "hangfire"."set" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".set_id_seq'::regclass),
  "key" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "score" float8 NOT NULL,
  "value" text COLLATE "pg_catalog"."default" NOT NULL,
  "expireat" timestamp(6),
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."set" OWNER TO "postgres";

-- ----------------------------
-- Table structure for state
-- ----------------------------
DROP TABLE IF EXISTS "hangfire"."state";
CREATE TABLE "hangfire"."state" (
  "id" int4 NOT NULL DEFAULT nextval('"hangfire".state_id_seq'::regclass),
  "jobid" int4 NOT NULL,
  "name" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "reason" varchar(100) COLLATE "pg_catalog"."default",
  "createdat" timestamp(6) NOT NULL,
  "data" text COLLATE "pg_catalog"."default",
  "updatecount" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "hangfire"."state" OWNER TO "postgres";

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "hangfire"."counter_id_seq"
OWNED BY "hangfire"."counter"."id";
SELECT setval('"hangfire"."counter_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."hash_id_seq"
OWNED BY "hangfire"."hash"."id";
SELECT setval('"hangfire"."hash_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."job_id_seq"
OWNED BY "hangfire"."job"."id";
SELECT setval('"hangfire"."job_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."jobparameter_id_seq"
OWNED BY "hangfire"."jobparameter"."id";
SELECT setval('"hangfire"."jobparameter_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."jobqueue_id_seq"
OWNED BY "hangfire"."jobqueue"."id";
SELECT setval('"hangfire"."jobqueue_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."list_id_seq"
OWNED BY "hangfire"."list"."id";
SELECT setval('"hangfire"."list_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."set_id_seq"
OWNED BY "hangfire"."set"."id";
SELECT setval('"hangfire"."set_id_seq"', 3, false);
ALTER SEQUENCE "hangfire"."state_id_seq"
OWNED BY "hangfire"."state"."id";
SELECT setval('"hangfire"."state_id_seq"', 3, false);

-- ----------------------------
-- Indexes structure for table counter
-- ----------------------------
CREATE INDEX "ix_hangfire_counter_expireat" ON "hangfire"."counter" USING btree (
  "expireat" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "ix_hangfire_counter_key" ON "hangfire"."counter" USING btree (
  "key" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table counter
-- ----------------------------
ALTER TABLE "hangfire"."counter" ADD CONSTRAINT "counter_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Uniques structure for table hash
-- ----------------------------
ALTER TABLE "hangfire"."hash" ADD CONSTRAINT "hash_key_field_key" UNIQUE ("key", "field");

-- ----------------------------
-- Primary Key structure for table hash
-- ----------------------------
ALTER TABLE "hangfire"."hash" ADD CONSTRAINT "hash_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table job
-- ----------------------------
CREATE INDEX "ix_hangfire_job_statename" ON "hangfire"."job" USING btree (
  "statename" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table job
-- ----------------------------
ALTER TABLE "hangfire"."job" ADD CONSTRAINT "job_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table jobparameter
-- ----------------------------
CREATE INDEX "ix_hangfire_jobparameter_jobidandname" ON "hangfire"."jobparameter" USING btree (
  "jobid" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "name" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table jobparameter
-- ----------------------------
ALTER TABLE "hangfire"."jobparameter" ADD CONSTRAINT "jobparameter_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table jobqueue
-- ----------------------------
CREATE INDEX "ix_hangfire_jobqueue_jobidandqueue" ON "hangfire"."jobqueue" USING btree (
  "jobid" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "queue" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "ix_hangfire_jobqueue_queueandfetchedat" ON "hangfire"."jobqueue" USING btree (
  "queue" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "fetchedat" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table jobqueue
-- ----------------------------
ALTER TABLE "hangfire"."jobqueue" ADD CONSTRAINT "jobqueue_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table list
-- ----------------------------
ALTER TABLE "hangfire"."list" ADD CONSTRAINT "list_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Uniques structure for table lock
-- ----------------------------
ALTER TABLE "hangfire"."lock" ADD CONSTRAINT "lock_resource_key" UNIQUE ("resource");

-- ----------------------------
-- Primary Key structure for table schema
-- ----------------------------
ALTER TABLE "hangfire"."schema" ADD CONSTRAINT "schema_pkey" PRIMARY KEY ("version");

-- ----------------------------
-- Primary Key structure for table server
-- ----------------------------
ALTER TABLE "hangfire"."server" ADD CONSTRAINT "server_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Uniques structure for table set
-- ----------------------------
ALTER TABLE "hangfire"."set" ADD CONSTRAINT "set_key_value_key" UNIQUE ("key", "value");

-- ----------------------------
-- Primary Key structure for table set
-- ----------------------------
ALTER TABLE "hangfire"."set" ADD CONSTRAINT "set_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table state
-- ----------------------------
CREATE INDEX "ix_hangfire_state_jobid" ON "hangfire"."state" USING btree (
  "jobid" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table state
-- ----------------------------
ALTER TABLE "hangfire"."state" ADD CONSTRAINT "state_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table jobparameter
-- ----------------------------
ALTER TABLE "hangfire"."jobparameter" ADD CONSTRAINT "jobparameter_jobid_fkey" FOREIGN KEY ("jobid") REFERENCES "hangfire"."job" ("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- ----------------------------
-- Foreign Keys structure for table state
-- ----------------------------
ALTER TABLE "hangfire"."state" ADD CONSTRAINT "state_jobid_fkey" FOREIGN KEY ("jobid") REFERENCES "hangfire"."job" ("id") ON DELETE CASCADE ON UPDATE CASCADE;
