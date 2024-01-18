/*
 Navicat Premium Data Transfer

 Source Server         : DBSERVER
 Source Server Type    : PostgreSQL
 Source Server Version : 100015
 Source Host           : 192.168.0.134:5432
 Source Catalog        : testAPI
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 100015
 File Encoding         : 65001

 Date: 29/09/2021 09:04:47
*/

-- ----------------------------
-- Sequence structure for Branches_BrId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."Branches_BrId_seq";
CREATE SEQUENCE "public"."Branches_BrId_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;




-- ----------------------------
-- Sequence structure for NLJournalDetails_NlJrnlNo_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."NLJournalDetails_NlJrnlNo_seq";
CREATE SEQUENCE "public"."NLJournalDetails_NlJrnlNo_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for NlJournalHeader_NlJrnlNo_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."NlJournalHeader_NlJrnlNo_seq" CASCADE;;
CREATE SEQUENCE "public"."NlJournalHeader_NlJrnlNo_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 9223372036854775807
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for PLAnalysisCodes_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."PLAnalysisCodes_id_seq";
CREATE SEQUENCE "public"."PLAnalysisCodes_id_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for SLAnalysisCodes_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."SLAnalysisCodes_id_seq";
CREATE SEQUENCE "public"."SLAnalysisCodes_id_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;


-- ----------------------------
-- Sequence structure for SLCustomer_SLCustomerSerial_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."SLCustomer_SLCustomerSerial_seq" CASCADE;
CREATE SEQUENCE "public"."SLCustomer_SLCustomerSerial_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for SLCustomerTypes_SLCTypeID_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."SLCustomerTypes_SLCTypeID_seq" CASCADE;
CREATE SEQUENCE "public"."SLCustomerTypes_SLCTypeID_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for SLInvoiceHeader_SLJrnlNo_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."SLInvoiceHeader_SLJrnlNo_seq" CASCADE;
CREATE SEQUENCE "public"."SLInvoiceHeader_SLJrnlNo_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for SLInvoiceTypes_INVypeID_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."SLInvoiceTypes_INVypeID_seq" CASCADE;
CREATE SEQUENCE "public"."SLInvoiceTypes_INVypeID_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for Settings_StId_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."Settings_StId_seq" CASCADE;
CREATE SEQUENCE "public"."Settings_StId_seq"
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;







-- ----------------------------
-- Table structure for AllSystemTerms
-- ----------------------------
DROP TABLE IF EXISTS "public"."AllSystemTerms";
CREATE TABLE "public"."AllSystemTerms" (
  "tosID" serial,
  "tosType" varchar(900) COLLATE "pg_catalog"."default",
  "terms" text COLLATE "pg_catalog"."default",
  "branch" int4
)
;

-- ----------------------------
-- Records of AllSystemTerms
-- ----------------------------
INSERT INTO "public"."AllSystemTerms" VALUES (2, 'pl_inv_terms', '&lt;blockquote&gt;L.P.O Terms&lt;/blockquote&gt;&lt;p&gt;The following are the &lt;strong&gt;required &lt;/strong&gt;terms:&lt;/p&gt;&lt;ul&gt;&lt;li&gt;&lt;em&gt;Business MUST be between the business hours (8:00am - 5:00pm)&lt;/em&gt;&lt;/li&gt;&lt;li&gt;&lt;em&gt;Transacting 3rd-parties MUST hold national IDs / Passports&lt;/em&gt;&lt;/li&gt;&lt;li&gt;Payments made must done via the following any accounts&lt;/li&gt;&lt;/ul&gt;&lt;p&gt;&lt;img src=&quot;https://www.multiplesgroup.com/wp-content/uploads/2020/01/top-commercial-banks-ranking-kenya-Copy-1024x344.jpg&quot; alt=&quot;banks&quot; width=&quot;293&quot; height=&quot;121&quot;&gt;&lt;/p&gt;&lt;p&gt;&lt;strong&gt;&lt;/strong&gt;:&lt;/p&gt;',1);
INSERT INTO "public"."AllSystemTerms" VALUES (1, '&lt;p&gt;&lt;br&gt;&lt;/p&gt;&lt;p&gt;Terms of Sale&lt;/p&gt;&lt;ol&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Goods once sold are NOT returnable&lt;/span&gt;&lt;/li&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Net 7 - Payment seven days after invoice date&lt;/span&gt;&lt;/li&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Net 10 - Payment ten days after invoice date&lt;/span&gt;&lt;/li&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Net 30 - Payment 30 days after invoice date&lt;/span&gt;&lt;/li&gt;&lt;/ol&gt;&lt;p&gt;&lt;br&gt;&lt;/p&gt;&lt;p style=&quot;text-align: right;&quot;&gt;&lt;br&gt;&lt;/p&gt;',1);

-- ----------------------------
-- Table structure for Branches
-- ----------------------------
DROP TABLE IF EXISTS "public"."Branches";
CREATE TABLE "public"."Branches" (
  "BrId" serial,
  "BrName" varchar(255) COLLATE "pg_catalog"."default",
  "BrLocation" varchar(255) COLLATE "pg_catalog"."default",
  "BrCode" varchar(255) COLLATE "pg_catalog"."default",
  "ContactStaff" int4,
  "BrActive" bool
)
;
INSERT INTO "public"."Branches" VALUES (1, 'DefaultBranch', 'Nairobi', '001', 1, 'true');

-- ----------------------------
-- Records of Branches
-- ----------------------------

-- ----------------------------
-- Table structure for Computers
-- ----------------------------
DROP TABLE IF EXISTS "public"."Computers";
CREATE TABLE "public"."Computers" (
  "CId" serial,
  "CName" varchar(255) COLLATE "pg_catalog"."default",
  "CIP" varchar(255) COLLATE "pg_catalog"."default",
  "CMac" varchar(255) COLLATE "pg_catalog"."default",
  "CUser" int4,
  "CRegisterDate" date
)
;

-- ----------------------------
-- Records of Computers
-- ----------------------------

-- ----------------------------
-- Table structure for Currencies
-- ----------------------------
DROP TABLE IF EXISTS "public"."Currencies";
CREATE TABLE "public"."Currencies" (
  "CrId" serial ,
  "CrName" varchar(255) COLLATE "pg_catalog"."default",
  "CrCode" varchar(255) COLLATE "pg_catalog"."default",
  "CrCountry" varchar(255) COLLATE "pg_catalog"."default",
  "CrStatus" varchar(255) COLLATE "pg_catalog"."default",
  "CrCreatedDate" date,
  "CrModifiedDate" date
)
;

-- ----------------------------
-- Records of Currencies
-- ----------------------------
INSERT INTO "public"."Currencies" VALUES (1, 'Kenya Shillings', 'KES', 'Kenya', 'Active', '2020-12-23', '2020-12-23');
INSERT INTO "public"."Currencies" VALUES (2, 'Ugandan Shilling', 'UGX', 'Uganda', 'Active', '2021-01-05', '2021-01-05');
INSERT INTO "public"."Currencies" VALUES (3, 'Tanzania Shilling', 'TZS', 'Tanzania', 'Inactive', '2021-01-06', '2021-01-06');

-- ----------------------------
-- Table structure for CustomerClassification
-- ----------------------------
DROP TABLE IF EXISTS "public"."CustomerClassification";
CREATE TABLE "public"."CustomerClassification" (
  "ClId" serial,
  "ClName" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of CustomerClassification
-- ----------------------------

-- ----------------------------
-- Table structure for Departments
-- ----------------------------
DROP TABLE IF EXISTS "public"."Departments";
CREATE TABLE "public"."Departments" (
  "DpId" serial,
  "DpBranch" int4,
  "DpName" varchar(255) COLLATE "pg_catalog"."default",
  "DpHead" int4,
  "DpRef" varchar(255) COLLATE "pg_catalog"."default"
)
;

INSERT INTO "public"."Departments" VALUES (1,1,'IT',1,'001');


-- ----------------------------
-- Table structure for Discounts
-- ----------------------------
DROP TABLE IF EXISTS "public"."Discounts";
CREATE TABLE "public"."Discounts" (
  "DId" serial,
  "DRef" varchar(255) COLLATE "pg_catalog"."default",
  "DPerc" float4,
  "DSetDate" date,
  "DEndDate" date,
  "DBranch" int4
)
;


-- ----------------------------
-- Table structure for Inventory
-- ----------------------------
DROP TABLE IF EXISTS "public"."Inventory";
CREATE TABLE "public"."Inventory" (
  "InvtId" serial,
  "InvtType" varchar(255) COLLATE "pg_catalog"."default",
  "InvtName" varchar(255) COLLATE "pg_catalog"."default",
  "InvtQty" int4,
  "InvtReorderLevel" int4,
  "InvtDateAdded" date,
  "InvtDateModified" date,
  "InvtAddedBy" int4,
  "InvtModifiedBy" int4,
  "InvtCurrency" int4,
  "InvtVATId" int4,
  "InvtBranch" int4,
  "InvtCategory" int4,
  "InvtProdCode" varchar(255) COLLATE "pg_catalog"."default",
  "InvtRef" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "InvtBP" numeric,
  "InvtSP" numeric,
  "ProdDesc" text COLLATE "pg_catalog"."default",
  "UOM" int4,
  "Obsolete" bool,
  "NonStock" bool,
  "ProdImage" text COLLATE "pg_catalog"."default",
  "BatchRef" varchar(255) COLLATE "pg_catalog"."default",
  "BOM" bool,
  "StkType" varchar(255) COLLATE "pg_catalog"."default",
  "PartsPerUnit" int4,
  "UnitSeparator" varchar(255) COLLATE "pg_catalog"."default",
  "SupplierRef" varchar(255) COLLATE "pg_catalog"."default",
  "LeadTime" int4,
  "SLProdGrpCode" varchar(255) COLLATE "pg_catalog"."default",
  "PLProdGrpCode" varchar(255) COLLATE "pg_catalog"."default",
  "ProdDiscId" int4,
  "ProdDiscPerc" numeric(255,0),
  "UdCostPrice" numeric(10,2),
  "AvgCostPrice" numeric(10,2),
  "LastPrice" numeric(10,2),
  "Weight" float4,
  "LastMovDate" date,
  "LastIssueDate" date,
  "WarehouseRef" varchar(255) COLLATE "pg_catalog"."default" DEFAULT NULL::character varying
)
;


-- ----------------------------
-- Table structure for LPODetails
-- ----------------------------
DROP TABLE IF EXISTS "public"."LPODetails";
CREATE TABLE "public"."LPODetails" (
  "PldID" serial,
  "PldRef" int4,
  "VatPerc" varchar(24) COLLATE "pg_catalog"."default",
  "VatAmt" numeric(19,4),
  "StkDesc" varchar(250) COLLATE "pg_catalog"."default",
  "UserID" int4,
  "ProdQty" int4,
  "Total" numeric(19,0),
  "UnitPrice" numeric(13,2),
  "PldDate" date
)
;



-- ----------------------------
-- Table structure for LPOHeader
-- ----------------------------
DROP TABLE IF EXISTS "public"."LPOHeader";
CREATE TABLE "public"."LPOHeader" (
  "LID" serial,
  "LPOCustID" int4,
  "LPODate" date,
  "TransDate" date,
  "Prefix" varchar(255) COLLATE "pg_catalog"."default",
  "requested_from" varchar(255) COLLATE "pg_catalog"."default",
  "received_batchno" varchar(255) COLLATE "pg_catalog"."default",
  "DocRef" int4,
  "CurrencyID" int4,
  "LDescription" varchar(255) COLLATE "pg_catalog"."default",
  "StaffID" int4,
  "Totals" numeric,
  "invoiceid" int4,
  "Invoiced" bool,
  "LPOBranch" int4
)
;

-- ----------------------------
-- Records of LPOHeader
-- ----------------------------


-- ----------------------------
-- Table structure for LPOSettings
-- ----------------------------
DROP TABLE IF EXISTS "public"."LPOSettings";
CREATE TABLE "public"."LPOSettings" (
  "LPO_SID" serial,
  "LPO_SPrefix" varchar(100) COLLATE "pg_catalog"."default",
  "LPO_StartNO" int4,
  "LPO_NumberingType" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of LPOSettings
-- ----------------------------
INSERT INTO "public"."LPOSettings" VALUES (1, 'LPO-', 1, 'Manual');

-- ----------------------------
-- Table structure for Licence
-- ----------------------------
DROP TABLE IF EXISTS "public"."Licence";
CREATE TABLE "public"."Licence" (
  "LsId" serial,
  "LsType" varchar(255) COLLATE "pg_catalog"."default",
  "LsCode" varchar(255) COLLATE "pg_catalog"."default",
  "LsIssueDate" date,
  "LsExpireDate" date,
  "CompanyName" varchar(255) COLLATE "pg_catalog"."default",
  "CompanySlogan" varchar(255) COLLATE "pg_catalog"."default",
  "CompanyAdmin" int4,
  "CompanyPostal" varchar(255) COLLATE "pg_catalog"."default",
  "CompanyContact" varchar(255) COLLATE "pg_catalog"."default",
  "CompanyVAT" varchar(255) COLLATE "pg_catalog"."default",
  "PhysicalAddress" varchar(255) COLLATE "pg_catalog"."default",
  "CompanyLogo" text COLLATE "pg_catalog"."default",
  "CompanyCurrency" int4
)
;
COMMENT ON COLUMN "public"."Licence"."LsType" IS 'Trial or Commercial';

-- ----------------------------
-- Records of Licence
-- ----------------------------
--INSERT INTO "public"."Licence" VALUES (49600717, 'Trial', 'B86CA9BF-7836-4162-B762-B5886324', '2021-01-05', '2023-09-28', 'NGENX Solutions Limited', 'Dev stored slogan', 1, '56-10306 Kilimani', '+254792898262, +25470023767', 'A00YTEG78778', 'National park Road, MSA Road', 'https://www.kenyabuzz.com/lifestyle/wp-content/uploads/2017/09/NSSF-Kenya-e1506681872235.jpg', 9);

-- ----------------------------
-- Table structure for NLAccount
-- ----------------------------
DROP TABLE IF EXISTS "public"."NLAccount";
CREATE TABLE "public"."NLAccount" (
  "NlAccCode" varchar(6) COLLATE "pg_catalog"."default" NOT NULL,
  "NlAccName" varchar(50) COLLATE "pg_catalog"."default",
  "GroupCode" varchar(6) COLLATE "pg_catalog"."default",
  "CurCode" varchar(6) COLLATE "pg_catalog"."default",
  "IsMirrorAcc" bool,
  "MAccCode" varchar(6) COLLATE "pg_catalog"."default",
  "AGroupCode" varchar(6) COLLATE "pg_catalog"."default",
  "StatBalance" money,
  "LastStatDate" date
)
;

-- ----------------------------
-- Records of NLAccount
-- ----------------------------
--INSERT INTO "public"."NLAccount" VALUES ('0002', 'MISC. CREDITS', 'RG0003', 'KES', 't', NULL, NULL, '$0.00', '2021-08-27');
--INSERT INTO "public"."NLAccount" VALUES ('0005', 'ADVERTISEMET', 'RG0007', 'KES', 'f', '', '', '$0.00', '2021-09-09');
--INSERT INTO "public"."NLAccount" VALUES ('0006', 'AUDIT FEE', 'RG0007', 'KES', 'f', '', '', '$0.00', '2021-09-09');
--INSERT INTO "public"."NLAccount" VALUES ('0007', 'BANK CHARGES', 'RG0007', 'KES', 'f', '', '', '$0.00', '2021-09-15');
--INSERT INTO "public"."NLAccount" VALUES ('0008', 'DIVIDEND', 'RG0011', 'KES', 'f', '', '', '$0.00', '2021-09-22');
--INSERT INTO "public"."NLAccount" VALUES ('0001', 'SALES', 'RG0006', '', 't', '', '', '$0.00', '2021-08-27');

-- ----------------------------
-- Table structure for NLAccountGroup
-- ----------------------------
DROP TABLE IF EXISTS "public"."NLAccountGroup";
CREATE TABLE "public"."NLAccountGroup" (
  "GroupCode" varchar(6) COLLATE "pg_catalog"."default" NOT NULL,
  "GroupName" varchar(50) COLLATE "pg_catalog"."default",
  "PriGroupCode" varchar(6) COLLATE "pg_catalog"."default",
  "GroupType" varchar(50) COLLATE "pg_catalog"."default",
  "GroupSubType" varchar(50) COLLATE "pg_catalog"."default",
  "GroupLevel" int4,
  "UserID" int4,
  "UserName" varchar(100) COLLATE "pg_catalog"."default",
  "SwVerNo" varchar(50) COLLATE "pg_catalog"."default",
  "ModifiedOn" date,
  "DefaultGroup" int4
)
;

-- ----------------------------
-- Records of NLAccountGroup
-- ----------------------------
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0003', 'Liability', '', 'L', 'LN', 1, NULL, NULL, NULL, '2021-08-27', 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0006', 'Income', '', 'I', 'SN', 1, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0007', 'Expenditure', '', 'E', 'SN', 1, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0008', 'Sales', 'RG0006', 'I', 'SN', 2, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0005', 'Loans', 'RG0003', 'L', 'LN', 2, NULL, NULL, NULL, '2021-09-07', 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0009', 'Equity', 'RG0003', 'L', 'LN', 2, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0010', 'Services', 'RG0006', 'I', 'SN', 2, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0011', 'Other Income', 'RG0006', 'I', 'SN', 2, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0012', 'Asset', '', 'A', 'LN', 1, NULL, NULL, NULL, '2021-09-21', 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0013', 'Fixed Assets', 'RG0012', 'A', 'LN', 2, NULL, NULL, NULL, NULL, 1);
--INSERT INTO "public"."NLAccountGroup" VALUES ('RG0014', 'Current Assets', 'RG0012', 'A', 'LN', 2, NULL, NULL, NULL, NULL, 1);

-- ----------------------------
-- Table structure for NLDateSummary
-- ----------------------------
DROP TABLE IF EXISTS "public"."NLDateSummary";
CREATE TABLE "public"."NLDateSummary" (
  "NLAccCode" varchar(6) COLLATE "pg_catalog"."default",
  "MonthEndDate" date,
  "Cr" numeric(19,4),
  "Dr" numeric(19,4),
  "UserID" int4,
  "UserName" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of NLDateSummary
-- ----------------------------

-- ----------------------------
-- Table structure for NLJournalDetails
-- ----------------------------
DROP TABLE IF EXISTS "public"."NLJournalDetails";
CREATE TABLE "public"."NLJournalDetails" (
  "JrnlSlNo"  int4,
  "NlAccCode" varchar(6) COLLATE "pg_catalog"."default",
  "Dr" money,
  "Cr" money,
  "Amount" money,
  "Narration" varchar(255) COLLATE "pg_catalog"."default",
  "SLNarration" varchar(255) COLLATE "pg_catalog"."default",
  "IsForex" bool,
  "FolioNo" varchar(100) COLLATE "pg_catalog"."default",
  "IsCleard" bool,
  "ClearDate" date,
  "FCCleared" bool,
  "FCClearDate" date,
  "VatAmount" money,
  "NlJrnlNo" int8 NOT NULL DEFAULT nextval('"NLJournalDetails_NlJrnlNo_seq"'::regclass)
)
;

-- ----------------------------
-- Records of NLJournalDetails
-- ----------------------------


-- ----------------------------
-- Table structure for NlJournalHeader
-- ----------------------------
DROP TABLE IF EXISTS "public"."NlJournalHeader";
CREATE TABLE "public"."NlJournalHeader" (
  "NlJrnlNo" serial,
  "NlJrnlDesc" varchar(50) COLLATE "pg_catalog"."default",
  "TranDate" date,
  "MEndDate" date,
  "TranPeriod" numeric(2,0),
  "TranYear" numeric(4,0),
  "TranFrom" varchar(50) COLLATE "pg_catalog"."default",
  "TranType" varchar(50) COLLATE "pg_catalog"."default",
  "SlJrnlNo" int8,
  "PlJrnlNo" int8,
  "ModuleId" int4,
  "ModifiedOn" date
)
;

-- ----------------------------
-- Records of NlJournalHeader
-- ----------------------------


-- ----------------------------
-- Table structure for PLAnalysisCodes
-- ----------------------------
DROP TABLE IF EXISTS "public"."PLAnalysisCodes";
CREATE TABLE "public"."PLAnalysisCodes" (
  "id" serial,
  "AnalType" varchar(100) COLLATE "pg_catalog"."default",
  "AnalCode" varchar(100) COLLATE "pg_catalog"."default",
  "AnalDesc" varchar(100) COLLATE "pg_catalog"."default",
  "NLAccCode" varchar(6) COLLATE "pg_catalog"."default",
  "ModifiedOn" date
)
;

-- ----------------------------
-- Records of PLAnalysisCodes
-- ----------------------------


-- ----------------------------
-- Table structure for PLCustomer
-- ----------------------------
DROP TABLE IF EXISTS "public"."PLCustomer";
CREATE TABLE "public"."PLCustomer" (
  "PLCustCode" varchar(250) COLLATE "pg_catalog"."default" NOT NULL,
  "CustName" varchar(250) COLLATE "pg_catalog"."default",
  "PhysicalAddress" varchar(255) COLLATE "pg_catalog"."default",
  "PostalAddress" varchar(255) COLLATE "pg_catalog"."default",
  "ContactPerson" varchar(255),
  "ContactPhone" varchar (255),
  "CurrID" int4,
  "VATNo" varchar(50) COLLATE "pg_catalog"."default",
  "CustID" int4 NOT NULL,
  "RegisterDate" date,
  "StaffID" int4,
  "CustBranch" int4
)
;

-- ----------------------------
-- Records of PLCustomer
-- ----------------------------


-- ----------------------------
-- Table structure for PLInvoiceDetail
-- ----------------------------
DROP TABLE IF EXISTS "public"."PLInvoiceDetail";
CREATE TABLE "public"."PLInvoiceDetail" (
  "PLJrnlNo" int4,
  "JrnlPLNo" int4,
  "UnitPrice" numeric(19,4),
  "VatPerc" varchar(24) COLLATE "pg_catalog"."default",
  "VatAmt" numeric(19,4),
  "ProdGroupCode" varchar(15) COLLATE "pg_catalog"."default",
  "NLAccCode" varchar(15) COLLATE "pg_catalog"."default",
  "StkDesc" varchar(100) COLLATE "pg_catalog"."default",
  "UserID" int4,
  "ProdQty" int4,
  "DiscountAmt" numeric(19,0),
  "Total" numeric(19,0),
  "ProdId" int4
)
;

-- ----------------------------
-- Records of PLInvoiceDetail
-- ----------------------------

-- ----------------------------
-- Table structure for PLInvoiceHeader
-- ----------------------------
DROP TABLE IF EXISTS "public"."PLInvoiceHeader";
CREATE TABLE "public"."PLInvoiceHeader" (
  "PLJrnlNo" int4 NOT NULL,
  "NlJrnlNo" int4,
  "PLCustID" int4,
  "TranDate" timestamp(6),
  "Period" varchar(250) COLLATE "pg_catalog"."default",
  "DocRef" int4,
  "InvDate" date,
  "CurrencyId" int4,
  "PLDescription" varchar(255) COLLATE "pg_catalog"."default",
  "StaffId" int4,
  "DocPrefix" varchar(255) COLLATE "pg_catalog"."default",
  "current_cust_name" varchar(255) COLLATE "pg_catalog"."default",
  "HasCreditNote" bool,
  "DueDate" date,
  "CRNDate" date,
  "Totals" money,
  "TotalDiscount" money,
  "Balance" money,
  "Additionals" varchar(255) COLLATE "pg_catalog"."default" DEFAULT ''::character varying,
  "InvReturned" bool,
  "PLBranch" int4
)
;

-- ----------------------------
-- Records of PLInvoiceHeader
-- ----------------------------

-- ----------------------------
-- Table structure for PLInvoiceSettings
-- ----------------------------
DROP TABLE IF EXISTS "public"."PLInvoiceSettings";
CREATE TABLE "public"."PLInvoiceSettings" (
  "InvSettingId" serial,
  "InvPrefix" varchar(255) COLLATE "pg_catalog"."default",
  "InvStartNumber" int4,
  "InvNumberingType" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of PLInvoiceSettings
-- ----------------------------
INSERT INTO "public"."PLInvoiceSettings" VALUES (1, 'INVP', 3, 'Auto');

-- ----------------------------
-- Table structure for SLAnalysisCodes
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLAnalysisCodes";
CREATE TABLE "public"."SLAnalysisCodes" (
  "AnalType" varchar(100) COLLATE "pg_catalog"."default",
  "AnalCode" varchar(100) COLLATE "pg_catalog"."default",
  "AnalDesc" varchar(100) COLLATE "pg_catalog"."default",
  "ModifiedOn" date,
  "NLAccCode" varchar(6) COLLATE "pg_catalog"."default",
  "id" int4 NOT NULL DEFAULT nextval('"SLAnalysisCodes_id_seq"'::regclass)
)
;

-- ----------------------------
-- Records of SLAnalysisCodes
-- ----------------------------


-- ----------------------------
-- Table structure for SLCustomer
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLCustomer";
CREATE TABLE "public"."SLCustomer" (
  "SLCustomerSerial" serial,
  "CustCode" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "CustFirstName" varchar(100) COLLATE "pg_catalog"."default",
  "Address" varchar(255) COLLATE "pg_catalog"."default",
  "PostalAddress" varchar(100) COLLATE "pg_catalog"."default",
  "PostalCode" varchar(250) COLLATE "pg_catalog"."default",
  "CurCode" int4,
  "CustEmail" varchar(255) COLLATE "pg_catalog"."default",
  "CustContact" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "SLCTypeID" int4 NOT NULL,
  "CustLastName" varchar(100) COLLATE "pg_catalog"."default",
  "CustType" varchar(100) COLLATE "pg_catalog"."default",
  "CustCompany" varchar(255) COLLATE "pg_catalog"."default",
  "VATNo" varchar(255) COLLATE "pg_catalog"."default",
  "CustCreditLimit" float4,
  "VATpin" varchar(255) COLLATE "pg_catalog"."default",
  "CreditTerms" int4,
  "Status" varchar(255) COLLATE "pg_catalog"."default",
  "CustRef" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "CustBranch" int4,
  "CustomerDept" numeric
)
;

-- ----------------------------
-- Records of SLCustomer
-- ----------------------------


-- ----------------------------
-- Table structure for SLCustomerTypes
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLCustomerTypes";
CREATE TABLE "public"."SLCustomerTypes" (
  "SLCTypeID" serial,
  "TypeName" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of SLCustomerTypes
-- ----------------------------
INSERT INTO "public"."SLCustomerTypes" VALUES (1, 'GOLD CUSTOMER');
INSERT INTO "public"."SLCustomerTypes" VALUES (2, 'SILVER CUSTOMER');

-- ----------------------------
-- Table structure for SLInvoiceDetail
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLInvoiceDetail";
CREATE TABLE "public"."SLInvoiceDetail" (
  "SLJrnlNo" int4,
  "JrnlSLNo" int4,
  "VatCode" text COLLATE "pg_catalog"."default",
  "VatAmt" numeric(24,0),
  "StkDesc" text COLLATE "pg_catalog"."default",
  "UserID" int4,
  "ItemSerial" text COLLATE "pg_catalog"."default" NOT NULL,
  "ItemQty" int4,
  "ItemTotals" numeric(24,0),
  "ItemUnitPrice" numeric(24,0),
  "DiscountPerc" numeric(24,0),
  "DiscountAmt" numeric(24,0),
  "AdditionalDetails" varchar(255) COLLATE "pg_catalog"."default",
  "ItemId" int4,
  "ItemCode" varchar(255) COLLATE "pg_catalog"."default",
  "ItemName" varchar(255) COLLATE "pg_catalog"."default",
  "ProdGroupCode" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of SLInvoiceDetail
-- ----------------------------



-- ----------------------------
-- Table structure for SLInvoiceHeader
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLInvoiceHeader";
CREATE TABLE "public"."SLInvoiceHeader" (
  "SLJrnlNo"  serial,
  "NlJrnlNo" int4,
  "CustCode" varchar(250) COLLATE "pg_catalog"."default",
  "TransDate" timestamp(6),
  "Period" varchar(250) COLLATE "pg_catalog"."default",
  "DocRef" int4,
  "TotalAmount" numeric(24,0),
  "INVTypeRef" varchar(100) COLLATE "pg_catalog"."default",
  "Dispute" bool DEFAULT false,
  "DeliveryCust" int4,
  "DeliveryAddress" varchar(255) COLLATE "pg_catalog"."default",
  "DeliveryDue" date,
  "INVDate" date,
  "CustId" int4,
  "PaymentDays" int4,
  "CustomRef" varchar(255) COLLATE "pg_catalog"."default",
  "CurrencyId" int4,
  "DueDate" date,
  "SLDescription" varchar(255) COLLATE "pg_catalog"."default",
  "StaffID" int4,
  "DocPrefix" varchar(255) COLLATE "pg_catalog"."default",
  "CRNReason" varchar(255) COLLATE "pg_catalog"."default",
  "HasCreditNote" bool,
  "Branch" int4,
  "InvPrinted" bool,
  "TotalBalance" numeric(255,0)
)
;

-- ----------------------------
-- Records of SLInvoiceHeader
-- ----------------------------




-- ----------------------------
-- Table structure for SLInvoiceSettings
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLInvoiceSettings";
CREATE TABLE "public"."SLInvoiceSettings" (
  "InvSettingId" serial,
  "InvPrefix" varchar(255) COLLATE "pg_catalog"."default",
  "InvStartNumber" int4,
  "InvNumberingType" varchar(100) COLLATE "pg_catalog"."default",
  "InvDeliveryNotes" int4,
  "InvType" varchar(255) COLLATE "pg_catalog"."default",
  "InvBranch" int4
)
;

-- ----------------------------
-- Records of SLInvoiceSettings
-- ----------------------------
INSERT INTO "public"."SLInvoiceSettings" VALUES (1, 'INV', 0, 'Auto', 2, 'INV',1);

-- ----------------------------
-- Table structure for SLInvoiceTypes
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLInvoiceTypes";
CREATE TABLE "public"."SLInvoiceTypes" (
  "INVypeID" int4 NOT NULL DEFAULT nextval('"SLInvoiceTypes_INVypeID_seq"'::regclass),
  "INVType" varchar(100) COLLATE "pg_catalog"."default",
  "INVComment" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of SLInvoiceTypes
-- ----------------------------
INSERT INTO "public"."SLInvoiceTypes" VALUES (23, 'CUSTOMER INVOICE', 'This is the invoice sent to the customer');
INSERT INTO "public"."SLInvoiceTypes" VALUES (25, 'SECURITY INVOICE', 'Invoice to be left at the gate');
INSERT INTO "public"."SLInvoiceTypes" VALUES (24, 'MASTER INVOICE', 'Invoice Left at the company');

-- ----------------------------
-- Table structure for SLProformaInvoiceDetails
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLProformaInvoiceDetails";
CREATE TABLE "public"."SLProformaInvoiceDetails" (
  "SLJrnlNo"  serial,
  "JrnlSLNo" int4,
  "InvAmt" float4,
  "VatCode" text COLLATE "pg_catalog"."default",
  "VatAmt" float4,
  "ProdGroupCode" text COLLATE "pg_catalog"."default",
  "NLAccCode" text COLLATE "pg_catalog"."default",
  "StkDesc" text COLLATE "pg_catalog"."default",
  "UserID" int4,
  "ItemSerial" text COLLATE "pg_catalog"."default" NOT NULL,
  "ItemQty" int4,
  "ItemTotals" float4,
  "ItemUnitPrice" float4,
  "DiscountPerc" float4,
  "DiscountAmt" float4,
  "AdditionalDetails" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of SLProformaInvoiceDetails
-- ----------------------------


-- ----------------------------
-- Table structure for SLProformaInvoiceHeader
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLProformaInvoiceHeader";
CREATE TABLE "public"."SLProformaInvoiceHeader" (
  "SLJrnlNo" int4 NOT NULL,
  "NlJrnlNo" int4,
  "CustCode" varchar(250) COLLATE "pg_catalog"."default",
  "TransDate" timestamp(6),
  "Period" varchar(250) COLLATE "pg_catalog"."default",
  "DocRef" int4 NOT NULL,
  "TotalAmount" numeric(24,0),
  "INVTypeRef" varchar(255) COLLATE "pg_catalog"."default",
  "Dispute" bool DEFAULT false,
  "DeliveryCust" int4,
  "DeliveryAddress" varchar(255) COLLATE "pg_catalog"."default",
  "DeliveryDue" date,
  "INVDate" date,
  "CustId" int4,
  "PaymentDays" int4,
  "CustomRef" varchar(255) COLLATE "pg_catalog"."default",
  "CurrencyId" int4,
  "DueDate" date,
  "SLDescription" varchar(255) COLLATE "pg_catalog"."default",
  "StaffID" int4,
  "DocPrefix" varchar(255) COLLATE "pg_catalog"."default",
  "HasInvoice" bool,
  "BranchRef" int4
)
;

-- ----------------------------
-- Records of SLProformaInvoiceHeader
-- ----------------------------



-- ----------------------------
-- Table structure for SLReceipts
-- ----------------------------
DROP TABLE IF EXISTS "public"."SLReceipts";
CREATE TABLE "public"."SLReceipts" (
  "pyID" serial,
  "pyRef" int4,
  "pyDate" date,
  "pyInvRef" int4,
  "pyPayable" float4,
  "pyPaid" float4 DEFAULT 0,
  "pyBalance" float4,
  "pyMode" varchar(255) COLLATE "pg_catalog"."default",
  "period_ref" varchar(255) COLLATE "pg_catalog"."default",
  "currentCustName" varchar(255) COLLATE "pg_catalog"."default",
  "pyChequeNumber" varchar(255) COLLATE "pg_catalog"."default",
  "pyReceivedFrom" varchar(255) COLLATE "pg_catalog"."default",
  "pyAdditionalDetails" varchar(255) COLLATE "pg_catalog"."default",
  "pyProcessDate" date,
  "pyUser" int4,
  "pyReturned" bool,
  "pyReturnReason" varchar(255) COLLATE "pg_catalog"."default",
  "pyBranch" int4
)
;

-- ----------------------------
-- Records of SLReceipts
-- ----------------------------










-- ----------------------------
-- Table structure for PLReceipts
-- ----------------------------
DROP TABLE IF EXISTS "public"."PLReceipts";
CREATE TABLE "public"."PLReceipts" (
  "pyID" serial,
  "pyRef" int4,
  "pyDate" date,
  "pyInvRef" int4,
  "pyPayable" float4,
  "pyPaid" float4 DEFAULT 0,
  "pyBalance" float4,
  "period_ref" varchar(255) COLLATE "pg_catalog"."default",
  "pyMode" varchar(255) COLLATE "pg_catalog"."default",
  "pyChequeNumber" varchar(255) COLLATE "pg_catalog"."default",
  "pyReceivedBy" varchar(255) COLLATE "pg_catalog"."default",
  "pyAdditionalDetails" varchar(255) COLLATE "pg_catalog"."default",
  "pyProcessDate" date,
  "pyUser" int4,
  "pyReturned" bool,
  "pyReturnReason" varchar(255) COLLATE "pg_catalog"."default",
  "pyBranch" int4,
  "UFirstName" varchar(255) COLLATE "pg_catalog"."default",
   "ULastName" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of PLReceipts
-- ---------------------------

-- ----------------------------
-- Table structure for Settings
-- ----------------------------
DROP TABLE IF EXISTS "public"."Settings";
CREATE TABLE "public"."Settings" (
  "StId" int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY (
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
),
  "StName" varchar(255) COLLATE "pg_catalog"."default",
  "StValue" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of Settings
-- ----------------------------
INSERT INTO "public"."Settings" VALUES (1, 'ScreenIdleTimeout', '10');

-- ----------------------------
-- Table structure for UserPermissions
-- ----------------------------
DROP TABLE IF EXISTS "public"."UserPermissions";
CREATE TABLE "public"."UserPermissions" (
  "PUser" int4,
  "ViewBranches" bool DEFAULT false,
  "AddBranch" bool DEFAULT false,
  "EditBranch" bool DEFAULT false,
  "DeleteBranch" bool DEFAULT false,
  "ViewCurrency" bool DEFAULT false,
  "EditCurrency" bool DEFAULT false,
  "DeleteCurrency" bool DEFAULT false,
  "AddCurrency" bool DEFAULT false,
  "PId"  serial,
  "ReadCustomer" bool DEFAULT false,
  "AddCustomer" bool DEFAULT false,
  "UpdateCustomer" bool DEFAULT false,
  "DeleteCustomer" bool DEFAULT false,
  "ReadVAT" bool DEFAULT false,
  "AddVAT" bool DEFAULT false,
  "UpdateVAT" bool DEFAULT false,
  "DeleteVAT" bool DEFAULT false,
  "AddInvoice" bool DEFAULT false,
  "AddCredNote" bool DEFAULT false,
  "ReadCreditNote" bool DEFAULT false,
  "ReadInventory" bool DEFAULT false,
  "AddInventory" bool DEFAULT false,
  "ModifyInventory" bool DEFAULT false,
  "ReadInvoices" bool DEFAULT false,
  "SLSettings" bool DEFAULT false,
  "PLSettings" bool DEFAULT false,
  "ReadLPO" bool DEFAULT false,
  "ManageLPO" bool DEFAULT false,
  "ReadPurchaseCustomer" bool DEFAULT false,
  "ReadPurchaseReceipts" bool DEFAULT false,
  "ManagePurchaseReceipts" bool DEFAULT false,
  "ReadPurchaseRequests" bool DEFAULT false,
  "ReceivePurchaseRequest" bool DEFAULT false,
  "PurchaseRequestAction" bool DEFAULT false,
  "CreatePurchaseRequest" bool DEFAULT false,
  "stockTakeRead" bool DEFAULT false,
  "stockTakeCreate" bool DEFAULT false,
  "stockTakeAction" bool DEFAULT false,
  "ApprovePurchaseReturn" bool DEFAULT false,
  "ReadPurchaseReturn" bool DEFAULT false,
  "CreatePurchaseReturn" bool DEFAULT false,
  "ReadDepartments" bool DEFAULT false,
  "ManageDepartments" bool DEFAULT false,
  "ReadUsers" bool DEFAULT false,
  "ManageUsers" bool DEFAULT false,
  "ReadPermissions" bool DEFAULT false,
  "ManagePermissions" bool DEFAULT false,
  "ManageCategories" bool DEFAULT false,
   "ManageUnits" bool DEFAULT false,
  "ManageFinancialPeriods" bool DEFAULT false,
  "ManageWarehouses" bool DEFAULT false,
  "CreateQuotation" bool DEFAULT false,
  "StockReports" bool DEFAULT false,
  "CreateUserGroup" bool DEFAULT false,
  "EditUserGroup" bool DEFAULT false,
  "ViewUserGroup" bool DEFAULT false
)
;

-- ----------------------------
-- Records of UserPermissions
-- ----------------------------
INSERT INTO "public"."UserPermissions" VALUES (1, 't', 't', 't', 't', 'f', 't', 't', 'f', 1, 't', 't',
												  't', 't', 't', 't', 'f', 'f', 't', 't', 't', 't', 't',
												  't', 'f', 't', 't', 't', 't', 't', 't', 't', 't', 't', 
												  't', 't', 't', 't', 't', 't', 't', 't', 't', 't', 't', 
												  't', 't', 't', 't', 't','t','t');

-- ----------------------------
-- Table structure for Users
-- ----------------------------
DROP TABLE IF EXISTS "public"."Users";
CREATE TABLE "public"."Users" (
  "UFirstName" varchar(255) COLLATE "pg_catalog"."default",
  "ULastName" varchar(255) COLLATE "pg_catalog"."default",
  "UEmail" varchar(255) COLLATE "pg_catalog"."default",
  "UPassword" varchar(255) COLLATE "pg_catalog"."default",
  "UType" varchar(255) COLLATE "pg_catalog"."default",
  "UContact" varchar(20) COLLATE "pg_catalog"."default",
  "UStatus" varchar(100) COLLATE "pg_catalog"."default",
  "UCompany" varchar(255) COLLATE "pg_catalog"."default",
  "UFirst" bool DEFAULT false,
  "UProfile" text COLLATE "pg_catalog"."default",
  "RegistrationDate" date,
  "UBranch" int4,
  "UDepartment" int4,
  "Username" varchar(255) COLLATE "pg_catalog"."default",
  "UId" serial,
  "UVAT" varchar(255) COLLATE "pg_catalog"."default" DEFAULT ''::character varying,
  "UIdnumber" int4
)
;

-- ----------------------------
-- Records of Users
-- ----------------------------

--INSERT INTO "public"."Users" VALUES ('Peter', 'njuguna', 'Pkamau00@gmail.com', 'eXY1bXJJZz19SCZdXysucS0tUXlyOypLJjZDT1lTLXRpck10Zj8ueGlvYDJAOHx+NmltXkRKeVQmfiYsV1tLVHxSOTdCZCk=', 'User', '254792898262', 'ACTIVE', 'NGX20', 't', 'default-user-icon-4.jpg', '2021-03-15', 54751507, 2, 'Dannie', 2, '2', 546464646);
--INSERT INTO "public"."Users" VALUES ('Justus', 'Mwangi', 'mwangijustus121@gmail.com', 'anVzdG9ofUgmXV8rLnEtLVF5cjsqSyY2Q09ZUy10aXJNdGY/Lnhpb2AyQDh8fjZpbV5ESnlUJn4mLFdbS1R8Ujk3QmQp', 'Administrator', '0792898262', 'ACTIVE', 'NGX20', 't', 'justus.jpg', '2020-12-02', 54751507, 1, 'justus77', 1, 'P898776TYGF', 12345678);
--INSERT INTO "public"."Users" VALUES ('BRIAN', 'WANGOCHO', 'mwangijustus12@gmail.com', 'WDg3U05YVzV9SCZdXysucS0tUXlyOypLJjZDT1lTLXRpck10Zj8ueGlvYDJAOHx+NmltXkRKeVQmfiYsV1tLVHxSOTdCZCk=', 'User', '254792898262', 'ACTIVE', 'NGX20', 't', 'default-user-icon-4.jpg', '2021-03-16', 54751507, 2, 'brian_', 3, 'YU7865445', 55555555);

-- ----------------------------
-- Table structure for VATs
-- ----------------------------
DROP TABLE IF EXISTS "public"."VATs";
CREATE TABLE "public"."VATs" (
  "VtId" serial,
  "VtRef" varchar(255) COLLATE "pg_catalog"."default",
  "VtPerc" float4,
  "VtSetDate" timestamp(0),
  "VtModifyDate" timestamp(0),
  "VtActive" bool,
  "VtBranch" int4
)
;

-- ----------------------------
-- Records of VATs
-- ----------------------------
INSERT INTO "public"."VATs" VALUES (1, 'CF', 14, '2021-01-06 13:00:00', '2021-01-06 13:16:35', 't', 1);
INSERT INTO "public"."VATs" VALUES (2, 'El', 16, '2021-01-06 16:10:13', '2021-01-06 16:10:13', 't', 1);
INSERT INTO "public"."VATs" VALUES (3, 'ES', 0, '2021-01-07 12:07:26', '2021-01-07 12:07:26', 't', 1);

-- ----------------------------
-- Table structure for financial_periods
-- ----------------------------
DROP TABLE IF EXISTS "public"."financial_periods";
CREATE TABLE "public"."financial_periods" (
  "fp_id" serial,
  "fp_ref" varchar(255) COLLATE "pg_catalog"."default",
  "fp_name" varchar(255) COLLATE "pg_catalog"."default",
  "fp_trans_date" date,
  "fp_openingdate" date,
  "fp_closingdate" date,
  "fp_active" bool,
  "fp_createdby" int4,
  "fp_closedby" int4,
  "fp_authorisedby" int4,
  "fp_createdon" date,
  "fp_branch" int4,
  "fp_date_mode" varchar(250) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of financial_periods
-- ----------------------------



-- ----------------------------
-- Table structure for inventory_category
-- ----------------------------
DROP TABLE IF EXISTS "public"."inventory_category";
CREATE TABLE "public"."inventory_category" (
  "cat_id" serial,
  "cat_entry_date" date,
  "cat_name" varchar(255) COLLATE "pg_catalog"."default",
  "cat_ref" varchar(255) COLLATE "pg_catalog"."default",
  "cat_branch" int4
)
;

-- ----------------------------
-- Records of inventory_category
-- ----------------------------



-- ----------------------------
-- Table structure for purchase_order_details
-- ----------------------------
DROP TABLE IF EXISTS "public"."purchase_order_details";
CREATE TABLE "public"."purchase_order_details" (
  "pod_ref" serial,
  "pod_itemname" varchar(255) COLLATE "pg_catalog"."default",
  "pod_qty" int4,
  "pod_unitprice" money,
  "pod_total" money,
  "pod_vat_perc" numeric,
  "pod_vat_amt" money,
  "pod_itemid" int4
)
;

-- ----------------------------
-- Records of purchase_order_details
-- ----------------------------




-- ----------------------------
-- Table structure for purchase_order_header
-- ----------------------------
DROP TABLE IF EXISTS "public"."purchase_order_header";
CREATE TABLE "public"."purchase_order_header" (
  "po_id" serial,
  "po_date" date,
  "po_prefix" varchar(255) COLLATE "pg_catalog"."default",
  "received_batchno" varchar(255) COLLATE "pg_catalog"."default",
  "request_from" varchar(255) COLLATE "pg_catalog"."default",
  "po_ref" int4,
  "po_user" int4,
  "po_total" money,
  "po_status" varchar(255) COLLATE "pg_catalog"."default",
  "po_approvedby" int4,
  "po_sender_signature" int4,
  "po_transdate" date,
  "po_approval_signature" int4,
  "po_has_lpo" bool
)
;

-- ----------------------------
-- Records of purchase_order_header
-- ----------------------------



-- ----------------------------
-- Table structure for purchase_receipt_details
-- ----------------------------
DROP TABLE IF EXISTS "public"."purchase_receipt_details";
CREATE TABLE "public"."purchase_receipt_details" (
  "pd_date" date,
  "pd_ref" int4,
  "pd_item" int4,
  "pd_qty" int4,
  "pd_unitprice" money,
  "pd_vat_perc" varchar(255) COLLATE "pg_catalog"."default",
  "pd_vat_amt" money,
  "pd_totals" money
)
;

-- ----------------------------
-- Records of purchase_receipt_details
-- ----------------------------




-- ----------------------------
-- Table structure for purchase_receipt_header
-- ----------------------------
DROP TABLE IF EXISTS "public"."purchase_receipt_header";
CREATE TABLE "public"."purchase_receipt_header" (
  "pr_id" serial,
  "pr_date" date,
  "pr_ref" int4,
  "pr_prefix" varchar(255) COLLATE "pg_catalog"."default",
  "pr_customer" int4,
  "period_ref" varchar(255) COLLATE "pg_catalog"."default",
  "pr_user" int4,
  "pr_total" money,
  "pr_currency" int4,
  "pr_additional" varchar(255) COLLATE "pg_catalog"."default",
  "pr_invoiced" bool,
  "pr_transdate" date,
  "pr_returned" bool
)
;

-- ----------------------------
-- Records of purchase_receipt_header
-- ----------------------------




-- ----------------------------
-- Table structure for purchase_return_details
-- ----------------------------
DROP TABLE IF EXISTS "public"."purchase_return_details";
CREATE TABLE "public"."purchase_return_details" (
  "pr_ref" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "pr_pl_invref" int4,
  "pr_item_name" varchar(255) COLLATE "pg_catalog"."default",
  "pr_item_qty" int4,
  "pr_reason" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of purchase_return_details
-- ----------------------------




-- ----------------------------
-- Table structure for purchase_return_header
-- ----------------------------
DROP TABLE IF EXISTS "public"."purchase_return_header";
CREATE TABLE "public"."purchase_return_header" (
  "prh_ref" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "prh_date" date,
  "prh_pljrnl" int4,
  "returnedby" int4,
  "returner_signature" int4,
  "approvedby" int4,
  "approver_signature" int4,
  "status" varchar(255) COLLATE "pg_catalog"."default" DEFAULT NULL::character varying,
  "prh_staff" int4
)
;

-- ----------------------------
-- Records of purchase_return_header
-- ----------------------------




-- ----------------------------
-- Table structure for signatures
-- ----------------------------
DROP TABLE IF EXISTS "public"."signatures";
CREATE TABLE "public"."signatures" (
  "sign_id" serial,
  "sign_date" date,
  "sign_user" int4,
  "sign_data" text COLLATE "pg_catalog"."default",
  "sign_name" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Records of signatures
-- ----------------------------



-- ----------------------------
-- Table structure for stock_take_details
-- ----------------------------
DROP TABLE IF EXISTS "public"."stock_take_details";
CREATE TABLE "public"."stock_take_details" (
  "stk_id" serial,
  "stk_date" date,
  "stk_item_id" int4,
  "stk_item_name" varchar(255) COLLATE "pg_catalog"."default",
  "store_qty" int4,
  "curr_qty" int4,
  "stk_ref" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "stk_has_issue" bool
)
;

-- ----------------------------
-- Records of stock_take_details
-- ----------------------------




-- ----------------------------
-- Table structure for stock_take_header
-- ----------------------------
DROP TABLE IF EXISTS "public"."stock_take_header";
CREATE TABLE "public"."stock_take_header" (
  "sth_id" serial,
  "sth_date" date,
  "sth_ref" varchar(255) COLLATE "pg_catalog"."default",
  "sth_name" varchar(255) COLLATE "pg_catalog"."default",
  "sth_staff" int4,
  "sth_approved" bool,
  "approved_by" int4,
  "approval_date" date,
  "has_issue" bool,
  "approver_signature" int4
)
;

-- ----------------------------
-- Records of stock_take_header
-- ----------------------------



-- ----------------------------
-- Table structure for warehouse_summary
-- ----------------------------
DROP TABLE IF EXISTS "public"."warehouse_summary";
CREATE TABLE "public"."warehouse_summary" (
  "ws_id" serial,
  "prod_id" int4,
  "wh_ref" varchar(255) COLLATE "pg_catalog"."default",
  "bincode" varchar(255) COLLATE "pg_catalog"."default",
  "openstock" int4,
  "qty_issued" int4,
  "qty_received" int4,
  "qty_adjusted" int4,
  "qty_allocated" int4,
  "rt_rct_qty" int4,
  "rt_issue_qty" int4,
  "qty_on_order" int4,
  "physical_qty" int4,
  "free_qty" int4,
  "min_stock_qty" int4,
  "max_stock_qty" int4,
  "modified_on" date,
  "ws_branch" int4,
  "ws_date" date
)
;
COMMENT ON COLUMN "public"."warehouse_summary"."rt_rct_qty" IS 'Return receipt quantity';

-- ----------------------------
-- Records of warehouse_summary
-- ----------------------------



-- ----------------------------
-- Table structure for warehouses
-- ----------------------------
DROP TABLE IF EXISTS "public"."warehouses";
CREATE TABLE "public"."warehouses" (
  "wh_ref" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "wh_code" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "wh_desc" varchar(255) COLLATE "pg_catalog"."default",
  "wh_address_1" varchar(255) COLLATE "pg_catalog"."default",
  "wh_address_2" varchar(255) COLLATE "pg_catalog"."default",
  "wh_address_3" varchar(255) COLLATE "pg_catalog"."default",
  "wh_address_4" varchar(255) COLLATE "pg_catalog"."default",
  "wh_type" varchar(255) COLLATE "pg_catalog"."default",
  "wh_stage" varchar(255) COLLATE "pg_catalog"."default",
  "wh_modifiedon" date,
  "wh_createdon" date,
  "wh_branch" int4
)
;

-- ----------------------------
-- Records of warehouses
-- ----------------------------


-- ----------------------------
-- Table structure for UnitofMeasure
-- ----------------------------
DROP TABLE IF EXISTS "public"."UnitofMeasure";
CREATE TABLE "public"."UnitofMeasure" (
  "id" serial NOT NULL ,
  "branch_id" int4 NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "created_on" date,
  "created_by" int4,
  "modified_on" date,
  "status" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for UnitofMeasure
-- ----------------------------

-- ----------------------------
-- Table structure for Groups
-- ----------------------------
DROP TABLE IF EXISTS "public"."Groups";
CREATE TABLE "public"."Groups" (
  "id" serial NOT NULL ,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "created_on" date,
  "created_by" int4,
  "status" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for Groups
-- ----------------------------

-- ----------------------------
-- Table structure for UserGroups
-- ----------------------------
DROP TABLE IF EXISTS "public"."UserGroups";
CREATE TABLE "public"."UserGroups" (
  "group_id" int4 NOT NULL ,
  "user_id" int4 NOT NULL

)
;

-- ----------------------------
-- Table structure for UserGroups
-- ----------------------------


-- ----------------------------
-- Table structure for UserGroupsPermission
-- ----------------------------
DROP TABLE IF EXISTS "public"."UserGroupsPermission";
CREATE TABLE "public"."UserGroupsPermission" (
  "group_id" int4 NOT NULL ,
  "permission" varchar(50) COLLATE "pg_catalog"."default"

)
;

-- ----------------------------
-- Table structure for UserGroupsPermission
-- ----------------------------











-- Indexes structure for table UnitofMeasure
-- ----------------------------
CREATE INDEX "fki_unit_of_measure_fk_branches" ON "public"."UnitofMeasure" USING btree (
  "branch_id" "pg_catalog"."int4_ops" ASC NULLS LAST
);


-- ----------------------------
-- Table structure for AuditTrail
-- ----------------------------
DROP TABLE IF EXISTS "public"."AuditTrail";
CREATE TABLE "public"."AuditTrail" (
  "id" serial NOT NULL,
  "userid" int4,
  "module" varchar(100) COLLATE "pg_catalog"."default",
  "action" varchar(100) COLLATE "pg_catalog"."default",
  "createdon" date
)
;

-- ----------------------------
-- Indexes structure for table AuditTrail
-- ----------------------------
CREATE INDEX "fki_AuditTrail_fk_user" ON "public"."AuditTrail" USING btree (
  "userid" "pg_catalog"."int4_ops" ASC NULLS LAST
);




-- ----------------------------
-- Table structure for purchase_return_details
-- ----------------------------
DROP TABLE IF EXISTS "public"."payment_modes";
CREATE TABLE "public"."payment_modes" (
  "id" serial NOT NULL,
  "bank_name" varchar(255) COLLATE "pg_catalog"."default",
  "nl_account_ref" varchar(255) COLLATE "pg_catalog"."default",
  "created_by" int4,
   "created_on" date

)
;

-- ----------------------------
-- Records of purchase_return_details
-- ----------------------------






-- ----------------------------
-- Function structure for add_user_permission
-- ----------------------------
--DROP FUNCTION IF EXISTS "public"."add_user_permission"();
--CREATE OR REPLACE FUNCTION "public"."add_user_permission"()
--  RETURNS "pg_catalog"."trigger" AS $BODY$
--	BEGIN

--	INSERT INTO "UserPermissions" ( "PUser", "AddCustomer" )
--		VALUES
--			(NEW.UId, FALSE);
--		RETURN NEW;

--END;
--$BODY$
 -- LANGUAGE plpgsql VOLATILE
 -- COST 100;

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
SELECT setval('"public"."Branches_BrId_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq1"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq10"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq11"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq12"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq13"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq14"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq15"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq16"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq17"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq18"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq19"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq2"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq20"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq20"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq21"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq21"', 8, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq22"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq22"', 7, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq23"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq23"', 6, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq24"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq24"', 5, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq25"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq25"', 4, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq26"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq26"', 3, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Computers_CId_seq27"
--OWNED BY "public"."Computers"."CId";
--SELECT setval('"public"."Computers_CId_seq27"', 2, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq3"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq4"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq5"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq6"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq7"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq8"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Computers_CId_seq9"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."NLJournalDetails_NlJrnlNo_seq"
--OWNED BY "public"."NLJournalDetails"."NlJrnlNo";
--SELECT setval('"public"."NLJournalDetails_NlJrnlNo_seq"', 29, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."NlJournalHeader_NlJrnlNo_seq"
--OWNED BY "public"."NlJournalHeader"."NlJrnlNo";
--SELECT setval('"public"."NlJournalHeader_NlJrnlNo_seq"', 22, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."PLAnalysisCodes_id_seq"
--OWNED BY "public"."PLAnalysisCodes"."id";
--SELECT setval('"public"."PLAnalysisCodes_id_seq"', 2, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."SLAnalysisCodes_id_seq"
OWNED BY "public"."SLAnalysisCodes"."id";
SELECT setval('"public"."SLAnalysisCodes_id_seq"', 5, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
SELECT setval('"public"."SLCustomerTypes_SLCTypeID_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."SLCustomer_SLCustomerSerial_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
SELECT setval('"public"."SLInvoiceHeader_SLJrnlNo_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."SLInvoiceTypes_INVypeID_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq1"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq10"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq11"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq12"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq13"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq14"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq15"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq16"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq17"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq17"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq18"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq18"', 8, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq19"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq19"', 7, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq2"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq20"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq20"', 6, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq21"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq21"', 5, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq22"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq22"', 4, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq23"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq23"', 3, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--ALTER SEQUENCE "public"."Settings_StId_seq24"
--OWNED BY "public"."Settings"."StId";
--SELECT setval('"public"."Settings_StId_seq24"', 2, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq3"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq4"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq5"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq6"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq7"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq8"', 9, false);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
--SELECT setval('"public"."Settings_StId_seq9"', 9, false);




-- ----------------------------
-- Primary Key structure for table Users
-- ----------------------------
ALTER TABLE "public"."Users" ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("UId");





-- ----------------------------
-- Primary Key structure for PLReceipts
-- ----------------------------
ALTER TABLE "public"."PLReceipts" ADD CONSTRAINT "PLReceipts_pkey" PRIMARY KEY ("pyID");

-- ----------------------------
-- Primary Key structure for SLInvoiceHeader
-- ----------------------------
ALTER TABLE "public"."SLInvoiceHeader" ADD CONSTRAINT "SLInvoiceHeader_pkey" PRIMARY KEY ("SLJrnlNo");


-- ----------------------------
-- Primary Key structure for UserPermissions
-- ----------------------------
ALTER TABLE "public"."UserPermissions"  ADD CONSTRAINT "UserPermissions_pkey" PRIMARY KEY ("PId");

-- ----------------------------
-- Primary Key structure for SLProformaInvoiceDetails
-- ----------------------------
ALTER TABLE "public"."SLProformaInvoiceDetails" ADD CONSTRAINT "SLProformaInvoiceDetails_pkey" PRIMARY KEY ("SLJrnlNo");


-- ----------------------------
-- Primary Key structure for table Branches
-- ----------------------------
ALTER TABLE "public"."Branches" ADD CONSTRAINT "Branches_pkey" PRIMARY KEY ("BrId");

-- ----------------------------
-- Primary Key structure for table Licence
-- ----------------------------
ALTER TABLE "public"."Licence" ADD CONSTRAINT "Licence_pkey" PRIMARY KEY ("LsId");

-- ----------------------------
-- Primary Key structure for table NLAccount
-- ----------------------------
ALTER TABLE "public"."NLAccount" ADD CONSTRAINT "NLAccount_pkey" PRIMARY KEY ("NlAccCode");

-- ----------------------------
-- Primary Key structure for table NLAccountGroup
-- ----------------------------
ALTER TABLE "public"."NLAccountGroup" ADD CONSTRAINT "NLAccountGroup_pkey" PRIMARY KEY ("GroupCode");

-- ----------------------------
-- Primary Key structure for table NLJournalDetails
-- ----------------------------
ALTER TABLE "public"."NLJournalDetails" ADD CONSTRAINT "NLJournalDetails_pkey" PRIMARY KEY ("NlJrnlNo");

-- ----------------------------
-- Primary Key structure for table NlJournalHeader
-- ----------------------------
ALTER TABLE "public"."NlJournalHeader" ADD CONSTRAINT "NlJournalHeader_pkey" PRIMARY KEY ("NlJrnlNo");

-- ----------------------------
-- Primary Key structure for table PLAnalysisCodes
-- ----------------------------
ALTER TABLE "public"."PLAnalysisCodes" ADD CONSTRAINT "PLAnalysisCodes_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table SLAnalysisCodes
-- ----------------------------
CREATE INDEX "fki_SLAnalysisCodes_fk_NLAccount" ON "public"."SLAnalysisCodes" USING btree (
  "NLAccCode" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table SLAnalysisCodes
-- ----------------------------
ALTER TABLE "public"."SLAnalysisCodes" ADD CONSTRAINT "SLAnalysisCodes_pkey" PRIMARY KEY ("id");


-- ----------------------------
-- Primary Key structure for table Inventory
-- ----------------------------
ALTER TABLE "public"."Inventory" ADD CONSTRAINT "Inventory_pkey" PRIMARY KEY ("InvtId");


-- ----------------------------
-- Primary Key structure for table warehouse_summary
-- ----------------------------
ALTER TABLE "public"."warehouse_summary" ADD CONSTRAINT "warehouse_summary_pkey" PRIMARY KEY ("ws_id");

-- ----------------------------
-- Primary Key structure for table warehouses
-- ----------------------------
ALTER TABLE "public"."warehouses" ADD CONSTRAINT "warehouses_pkey" PRIMARY KEY ("wh_code", "wh_ref");

-- ----------------------------
-- Primary Key structure for table UnitofMeasure
-- ----------------------------
ALTER TABLE "public"."UnitofMeasure" ADD CONSTRAINT "UnitofMeasure_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table UnitofMeasure
-- ----------------------------
ALTER TABLE "public"."UnitofMeasure" ADD CONSTRAINT "unit_of_measure_fk_branches" FOREIGN KEY ("branch_id") REFERENCES "public"."Branches" ("BrId") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."UnitofMeasure" ADD CONSTRAINT "unit_of_measure_fk_user" FOREIGN KEY ("created_by") REFERENCES "public"."Users" ("UId") ON DELETE NO ACTION ON UPDATE NO ACTION;




-- ----------------------------
-- Foreign Keys structure for table SLAnalysisCodes
-- ----------------------------
ALTER TABLE "public"."SLAnalysisCodes" ADD CONSTRAINT "SLAnalysisCodes_fk_NLAccount" FOREIGN KEY ("NLAccCode") REFERENCES "public"."NLAccount" ("NlAccCode") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Primary Key structure for table AuditTrail
-- ----------------------------
ALTER TABLE "public"."AuditTrail" ADD CONSTRAINT "AuditTrail_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table AuditTrail
-- ----------------------------
ALTER TABLE "public"."AuditTrail" ADD CONSTRAINT "AuditTrail_fk_user" FOREIGN KEY ("userid") REFERENCES "public"."Users" ("UId") ON DELETE NO ACTION ON UPDATE NO ACTION;


-- ----------------------------
-- Primary Key structure for table Groups
-- ----------------------------
ALTER TABLE "public"."Groups" ADD CONSTRAINT "Groups_pkey" PRIMARY KEY ("id");



-- ----------------------------
-- Primary Key structure for table UserGroups
-- ----------------------------
ALTER TABLE "public"."UserGroups" ADD CONSTRAINT "UserGroups_fk_user" FOREIGN KEY ("user_id") REFERENCES "public"."Users" ("UId") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."UserGroups" ADD CONSTRAINT "UserGroups_fk_Groups" FOREIGN KEY ("group_id") REFERENCES "public"."Groups" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;


-- ----------------------------
-- Primary Key structure for table PaymentModes
-- ----------------------------
ALTER TABLE "public"."payment_modes" ADD CONSTRAINT "PModes_pkey" PRIMARY KEY ("id");
ALTER TABLE "public"."payment_modes" ADD CONSTRAINT "PModes_fk_NLaccount" FOREIGN KEY ("nl_account_ref") REFERENCES "public"."NLAccount" ("NlAccCode") ON DELETE NO ACTION ON UPDATE NO ACTION;



