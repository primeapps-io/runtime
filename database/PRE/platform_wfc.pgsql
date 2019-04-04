/*
 Navicat Premium Data Transfer

 Source Server         : PrimeApps-PRE-Dev
 Source Server Type    : PostgreSQL
 Source Server Version : 90611
 Source Host           : primeapps-pre-dev.cde77rccad5m.eu-west-1.rds.amazonaws.com:5432
 Source Catalog        : platform
 Source Schema         : wfc

 Target Server Type    : PostgreSQL
 Target Server Version : 90611
 File Encoding         : 65001

 Date: 25/02/2019 13:06:38
*/


-- ----------------------------
-- Sequence structure for Event_PersistenceId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "wfc"."Event_PersistenceId_seq";
CREATE SEQUENCE "wfc"."Event_PersistenceId_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "wfc"."Event_PersistenceId_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ExecutionError_PersistenceId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "wfc"."ExecutionError_PersistenceId_seq";
CREATE SEQUENCE "wfc"."ExecutionError_PersistenceId_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "wfc"."ExecutionError_PersistenceId_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ExecutionPointer_PersistenceId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "wfc"."ExecutionPointer_PersistenceId_seq";
CREATE SEQUENCE "wfc"."ExecutionPointer_PersistenceId_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "wfc"."ExecutionPointer_PersistenceId_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for ExtensionAttribute_PersistenceId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "wfc"."ExtensionAttribute_PersistenceId_seq";
CREATE SEQUENCE "wfc"."ExtensionAttribute_PersistenceId_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "wfc"."ExtensionAttribute_PersistenceId_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for Subscription_PersistenceId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "wfc"."Subscription_PersistenceId_seq";
CREATE SEQUENCE "wfc"."Subscription_PersistenceId_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "wfc"."Subscription_PersistenceId_seq" OWNER TO "postgres";

-- ----------------------------
-- Sequence structure for Workflow_PersistenceId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "wfc"."Workflow_PersistenceId_seq";
CREATE SEQUENCE "wfc"."Workflow_PersistenceId_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;
ALTER SEQUENCE "wfc"."Workflow_PersistenceId_seq" OWNER TO "postgres";

-- ----------------------------
-- Table structure for Event
-- ----------------------------
DROP TABLE IF EXISTS "wfc"."Event";
CREATE TABLE "wfc"."Event" (
  "PersistenceId" int8 NOT NULL DEFAULT nextval('"wfc"."Event_PersistenceId_seq"'::regclass),
  "EventData" text COLLATE "pg_catalog"."default",
  "EventId" uuid NOT NULL,
  "EventKey" varchar(200) COLLATE "pg_catalog"."default",
  "EventName" varchar(200) COLLATE "pg_catalog"."default",
  "EventTime" timestamp(6) NOT NULL,
  "IsProcessed" bool NOT NULL
)
;
ALTER TABLE "wfc"."Event" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ExecutionError
-- ----------------------------
DROP TABLE IF EXISTS "wfc"."ExecutionError";
CREATE TABLE "wfc"."ExecutionError" (
  "PersistenceId" int8 NOT NULL DEFAULT nextval('"wfc"."ExecutionError_PersistenceId_seq"'::regclass),
  "ErrorTime" timestamp(6) NOT NULL,
  "ExecutionPointerId" varchar(100) COLLATE "pg_catalog"."default",
  "Message" text COLLATE "pg_catalog"."default",
  "WorkflowId" varchar(100) COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "wfc"."ExecutionError" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ExecutionPointer
-- ----------------------------
DROP TABLE IF EXISTS "wfc"."ExecutionPointer";
CREATE TABLE "wfc"."ExecutionPointer" (
  "PersistenceId" int8 NOT NULL DEFAULT nextval('"wfc"."ExecutionPointer_PersistenceId_seq"'::regclass),
  "Active" bool NOT NULL,
  "RetryCount" int4 NOT NULL,
  "EndTime" timestamp(6),
  "EventData" text COLLATE "pg_catalog"."default",
  "EventKey" varchar(100) COLLATE "pg_catalog"."default",
  "EventName" varchar(100) COLLATE "pg_catalog"."default",
  "EventPublished" bool NOT NULL,
  "Id" varchar(50) COLLATE "pg_catalog"."default",
  "PersistenceData" text COLLATE "pg_catalog"."default",
  "SleepUntil" timestamp(6),
  "StartTime" timestamp(6),
  "StepId" int4 NOT NULL,
  "StepName" varchar(100) COLLATE "pg_catalog"."default",
  "WorkflowId" int8 NOT NULL,
  "Children" text COLLATE "pg_catalog"."default",
  "ContextItem" text COLLATE "pg_catalog"."default",
  "PredecessorId" varchar(100) COLLATE "pg_catalog"."default",
  "Outcome" text COLLATE "pg_catalog"."default",
  "Scope" text COLLATE "pg_catalog"."default",
  "Status" int4 NOT NULL DEFAULT 0
)
;
ALTER TABLE "wfc"."ExecutionPointer" OWNER TO "postgres";

-- ----------------------------
-- Table structure for ExtensionAttribute
-- ----------------------------
DROP TABLE IF EXISTS "wfc"."ExtensionAttribute";
CREATE TABLE "wfc"."ExtensionAttribute" (
  "PersistenceId" int8 NOT NULL DEFAULT nextval('"wfc"."ExtensionAttribute_PersistenceId_seq"'::regclass),
  "AttributeKey" varchar(100) COLLATE "pg_catalog"."default",
  "AttributeValue" text COLLATE "pg_catalog"."default",
  "ExecutionPointerId" int8 NOT NULL
)
;
ALTER TABLE "wfc"."ExtensionAttribute" OWNER TO "postgres";

-- ----------------------------
-- Table structure for Subscription
-- ----------------------------
DROP TABLE IF EXISTS "wfc"."Subscription";
CREATE TABLE "wfc"."Subscription" (
  "PersistenceId" int8 NOT NULL DEFAULT nextval('"wfc"."Subscription_PersistenceId_seq"'::regclass),
  "EventKey" varchar(200) COLLATE "pg_catalog"."default",
  "EventName" varchar(200) COLLATE "pg_catalog"."default",
  "StepId" int4 NOT NULL,
  "SubscriptionId" uuid NOT NULL,
  "WorkflowId" varchar(200) COLLATE "pg_catalog"."default",
  "SubscribeAsOf" timestamp(6) NOT NULL DEFAULT '0001-01-01 00:00:00'::timestamp without time zone
)
;
ALTER TABLE "wfc"."Subscription" OWNER TO "postgres";

-- ----------------------------
-- Table structure for Workflow
-- ----------------------------
DROP TABLE IF EXISTS "wfc"."Workflow";
CREATE TABLE "wfc"."Workflow" (
  "PersistenceId" int8 NOT NULL DEFAULT nextval('"wfc"."Workflow_PersistenceId_seq"'::regclass),
  "CompleteTime" timestamp(6),
  "CreateTime" timestamp(6) NOT NULL,
  "Data" text COLLATE "pg_catalog"."default",
  "Description" varchar(500) COLLATE "pg_catalog"."default",
  "InstanceId" uuid NOT NULL,
  "NextExecution" int8,
  "Status" int4 NOT NULL,
  "Version" int4 NOT NULL,
  "WorkflowDefinitionId" varchar(200) COLLATE "pg_catalog"."default",
  "Reference" varchar(200) COLLATE "pg_catalog"."default"
)
;
ALTER TABLE "wfc"."Workflow" OWNER TO "postgres";

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "wfc"."Event_PersistenceId_seq"
OWNED BY "wfc"."Event"."PersistenceId";
SELECT setval('"wfc"."Event_PersistenceId_seq"', 4, true);
ALTER SEQUENCE "wfc"."ExecutionError_PersistenceId_seq"
OWNED BY "wfc"."ExecutionError"."PersistenceId";
SELECT setval('"wfc"."ExecutionError_PersistenceId_seq"', 4, false);
ALTER SEQUENCE "wfc"."ExecutionPointer_PersistenceId_seq"
OWNED BY "wfc"."ExecutionPointer"."PersistenceId";
SELECT setval('"wfc"."ExecutionPointer_PersistenceId_seq"', 7, true);
ALTER SEQUENCE "wfc"."ExtensionAttribute_PersistenceId_seq"
OWNED BY "wfc"."ExtensionAttribute"."PersistenceId";
SELECT setval('"wfc"."ExtensionAttribute_PersistenceId_seq"', 4, false);
ALTER SEQUENCE "wfc"."Subscription_PersistenceId_seq"
OWNED BY "wfc"."Subscription"."PersistenceId";
SELECT setval('"wfc"."Subscription_PersistenceId_seq"', 4, true);
ALTER SEQUENCE "wfc"."Workflow_PersistenceId_seq"
OWNED BY "wfc"."Workflow"."PersistenceId";
SELECT setval('"wfc"."Workflow_PersistenceId_seq"', 4, true);

-- ----------------------------
-- Indexes structure for table Event
-- ----------------------------
CREATE UNIQUE INDEX "IX_Event_EventId" ON "wfc"."Event" USING btree (
  "EventId" "pg_catalog"."uuid_ops" ASC NULLS LAST
);
CREATE INDEX "IX_Event_EventName_EventKey" ON "wfc"."Event" USING btree (
  "EventName" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "EventKey" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_Event_EventTime" ON "wfc"."Event" USING btree (
  "EventTime" "pg_catalog"."timestamp_ops" ASC NULLS LAST
);
CREATE INDEX "IX_Event_IsProcessed" ON "wfc"."Event" USING btree (
  "IsProcessed" "pg_catalog"."bool_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table Event
-- ----------------------------
ALTER TABLE "wfc"."Event" ADD CONSTRAINT "PK_Event" PRIMARY KEY ("PersistenceId");

-- ----------------------------
-- Primary Key structure for table ExecutionError
-- ----------------------------
ALTER TABLE "wfc"."ExecutionError" ADD CONSTRAINT "PK_ExecutionError" PRIMARY KEY ("PersistenceId");

-- ----------------------------
-- Indexes structure for table ExecutionPointer
-- ----------------------------
CREATE INDEX "IX_ExecutionPointer_WorkflowId" ON "wfc"."ExecutionPointer" USING btree (
  "WorkflowId" "pg_catalog"."int8_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ExecutionPointer
-- ----------------------------
ALTER TABLE "wfc"."ExecutionPointer" ADD CONSTRAINT "PK_ExecutionPointer" PRIMARY KEY ("PersistenceId");

-- ----------------------------
-- Indexes structure for table ExtensionAttribute
-- ----------------------------
CREATE INDEX "IX_ExtensionAttribute_ExecutionPointerId" ON "wfc"."ExtensionAttribute" USING btree (
  "ExecutionPointerId" "pg_catalog"."int8_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table ExtensionAttribute
-- ----------------------------
ALTER TABLE "wfc"."ExtensionAttribute" ADD CONSTRAINT "PK_ExtensionAttribute" PRIMARY KEY ("PersistenceId");

-- ----------------------------
-- Indexes structure for table Subscription
-- ----------------------------
CREATE INDEX "IX_Subscription_EventKey" ON "wfc"."Subscription" USING btree (
  "EventKey" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "IX_Subscription_EventName" ON "wfc"."Subscription" USING btree (
  "EventName" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "IX_Subscription_SubscriptionId" ON "wfc"."Subscription" USING btree (
  "SubscriptionId" "pg_catalog"."uuid_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table Subscription
-- ----------------------------
ALTER TABLE "wfc"."Subscription" ADD CONSTRAINT "PK_Subscription" PRIMARY KEY ("PersistenceId");

-- ----------------------------
-- Indexes structure for table Workflow
-- ----------------------------
CREATE UNIQUE INDEX "IX_Workflow_InstanceId" ON "wfc"."Workflow" USING btree (
  "InstanceId" "pg_catalog"."uuid_ops" ASC NULLS LAST
);
CREATE INDEX "IX_Workflow_NextExecution" ON "wfc"."Workflow" USING btree (
  "NextExecution" "pg_catalog"."int8_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table Workflow
-- ----------------------------
ALTER TABLE "wfc"."Workflow" ADD CONSTRAINT "PK_Workflow" PRIMARY KEY ("PersistenceId");

-- ----------------------------
-- Foreign Keys structure for table ExecutionPointer
-- ----------------------------
ALTER TABLE "wfc"."ExecutionPointer" ADD CONSTRAINT "FK_ExecutionPointer_Workflow_WorkflowId" FOREIGN KEY ("WorkflowId") REFERENCES "wfc"."Workflow" ("PersistenceId") ON DELETE CASCADE ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table ExtensionAttribute
-- ----------------------------
ALTER TABLE "wfc"."ExtensionAttribute" ADD CONSTRAINT "FK_ExtensionAttribute_ExecutionPointer_ExecutionPointerId" FOREIGN KEY ("ExecutionPointerId") REFERENCES "wfc"."ExecutionPointer" ("PersistenceId") ON DELETE CASCADE ON UPDATE NO ACTION;
