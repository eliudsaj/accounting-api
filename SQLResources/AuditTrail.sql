/*
 Navicat Premium Data Transfer

 Source Server         : DBSERVER
 Source Server Type    : PostgreSQL
 Source Server Version : 100015
 Source Host           : 192.168.0.134:5432
 Source Catalog        : itsolutions_62840
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 100015
 File Encoding         : 65001

 Date: 22/10/2021 13:09:34
*/


-- ----------------------------
-- Table structure for AuditTrail
-- ----------------------------
DROP TABLE IF EXISTS "public"."AuditTrail";
CREATE TABLE "public"."AuditTrail" (
  "id" int4 NOT NULL DEFAULT nextval('"AuditTrail_id_seq"'::regclass),
  "userid" int4,
  "module" varchar(100) COLLATE "pg_catalog"."default",
  "action" varchar(100) COLLATE "pg_catalog"."default",
  "createdon" time(6)
)
;

-- ----------------------------
-- Indexes structure for table AuditTrail
-- ----------------------------
CREATE INDEX "fki_AuditTrail_fk_user" ON "public"."AuditTrail" USING btree (
  "userid" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table AuditTrail
-- ----------------------------
ALTER TABLE "public"."AuditTrail" ADD CONSTRAINT "AuditTrail_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table AuditTrail
-- ----------------------------
ALTER TABLE "public"."AuditTrail" ADD CONSTRAINT "AuditTrail_fk_user" FOREIGN KEY ("userid") REFERENCES "public"."Users" ("UId") ON DELETE NO ACTION ON UPDATE NO ACTION;
