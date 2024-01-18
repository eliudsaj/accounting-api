/*
 Navicat Premium Data Transfer

 Source Server         : DBSERVER
 Source Server Type    : PostgreSQL
 Source Server Version : 100015
 Source Host           : 192.168.0.134:5432
 Source Catalog        : limoandsons_90509
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 100015
 File Encoding         : 65001

 Date: 19/10/2021 10:07:37
*/


-- ----------------------------
-- Table structure for UnitofMeasure
-- ----------------------------
DROP TABLE IF EXISTS "public"."UnitofMeasure";
CREATE TABLE "public"."UnitofMeasure" (
  "id" int4 NOT NULL DEFAULT nextval('"UnitofMeasure_id_seq"'::regclass),
  "branch_id" int4 NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "created_on" date,
  "created_by" int4,
  "modified_on" date,
  "status" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of UnitofMeasure
-- ----------------------------

-- ----------------------------
-- Indexes structure for table UnitofMeasure
-- ----------------------------
CREATE INDEX "fki_unit_of_measure_fk_branches" ON "public"."UnitofMeasure" USING btree (
  "branch_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table UnitofMeasure
-- ----------------------------
ALTER TABLE "public"."UnitofMeasure" ADD CONSTRAINT "UnitofMeasure_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table UnitofMeasure
-- ----------------------------
ALTER TABLE "public"."UnitofMeasure" ADD CONSTRAINT "unit_of_measure_fk_branches" FOREIGN KEY ("branch_id") REFERENCES "public"."Branches" ("BrId") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."UnitofMeasure" ADD CONSTRAINT "unit_of_measure_fk_user" FOREIGN KEY ("id") REFERENCES "public"."Users" ("UId") ON DELETE NO ACTION ON UPDATE NO ACTION;
