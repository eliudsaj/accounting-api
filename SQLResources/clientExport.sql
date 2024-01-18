--
-- PostgreSQL database dump
--

-- Dumped from database version 10.15
-- Dumped by pg_dump version 13.1

-- Started on 2021-09-29 08:44:48

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 307 (class 1255 OID 58063)
-- Name: add_user_permission(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.add_user_permission() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
	BEGIN
	
	INSERT INTO "UserPermissions" ( "PUser", "AddCustomer" )
		VALUES
			(NEW.UId, FALSE);
		RETURN NEW;
	
END;
$$;


ALTER FUNCTION public.add_user_permission() OWNER TO postgres;

SET default_tablespace = '';

--
-- TOC entry 252 (class 1259 OID 57798)
-- Name: AllSystemTerms; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AllSystemTerms" (
    "tosID" integer NOT NULL,
    "tosType" character varying(255),
    terms text,
    branch integer
);


ALTER TABLE public."AllSystemTerms" OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 57804)
-- Name: Branches; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Branches" (
    "BrId" integer NOT NULL,
    "BrName" character varying(255),
    "BrLocation" character varying(255),
    "BrCode" character varying(255),
    "ContactStaff" integer,
    "BrActive" boolean
);


ALTER TABLE public."Branches" OWNER TO postgres;

--
-- TOC entry 196 (class 1259 OID 57686)
-- Name: Branches_BrId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Branches_BrId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Branches_BrId_seq" OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 57812)
-- Name: Computers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Computers" (
    "CId" integer NOT NULL,
    "CName" character varying(255),
    "CIP" character varying(255),
    "CMac" character varying(255),
    "CUser" integer,
    "CRegisterDate" date
);


ALTER TABLE public."Computers" OWNER TO postgres;

--
-- TOC entry 197 (class 1259 OID 57688)
-- Name: Computers_CId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq" OWNER TO postgres;

--
-- TOC entry 198 (class 1259 OID 57690)
-- Name: Computers_CId_seq1; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq1"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq1" OWNER TO postgres;

--
-- TOC entry 199 (class 1259 OID 57692)
-- Name: Computers_CId_seq10; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq10"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq10" OWNER TO postgres;

--
-- TOC entry 200 (class 1259 OID 57694)
-- Name: Computers_CId_seq11; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq11"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq11" OWNER TO postgres;

--
-- TOC entry 201 (class 1259 OID 57696)
-- Name: Computers_CId_seq12; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq12"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq12" OWNER TO postgres;

--
-- TOC entry 202 (class 1259 OID 57698)
-- Name: Computers_CId_seq13; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq13"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq13" OWNER TO postgres;

--
-- TOC entry 203 (class 1259 OID 57700)
-- Name: Computers_CId_seq14; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq14"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq14" OWNER TO postgres;

--
-- TOC entry 204 (class 1259 OID 57702)
-- Name: Computers_CId_seq15; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq15"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq15" OWNER TO postgres;

--
-- TOC entry 205 (class 1259 OID 57704)
-- Name: Computers_CId_seq16; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq16"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq16" OWNER TO postgres;

--
-- TOC entry 206 (class 1259 OID 57706)
-- Name: Computers_CId_seq17; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq17"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq17" OWNER TO postgres;

--
-- TOC entry 207 (class 1259 OID 57708)
-- Name: Computers_CId_seq18; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq18"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq18" OWNER TO postgres;

--
-- TOC entry 208 (class 1259 OID 57710)
-- Name: Computers_CId_seq19; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq19"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq19" OWNER TO postgres;

--
-- TOC entry 209 (class 1259 OID 57712)
-- Name: Computers_CId_seq2; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq2"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq2" OWNER TO postgres;

--
-- TOC entry 210 (class 1259 OID 57714)
-- Name: Computers_CId_seq20; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq20"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq20" OWNER TO postgres;

--
-- TOC entry 3293 (class 0 OID 0)
-- Dependencies: 210
-- Name: Computers_CId_seq20; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq20" OWNED BY public."Computers"."CId";


--
-- TOC entry 211 (class 1259 OID 57716)
-- Name: Computers_CId_seq21; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq21"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq21" OWNER TO postgres;

--
-- TOC entry 3294 (class 0 OID 0)
-- Dependencies: 211
-- Name: Computers_CId_seq21; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq21" OWNED BY public."Computers"."CId";


--
-- TOC entry 212 (class 1259 OID 57718)
-- Name: Computers_CId_seq22; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq22"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq22" OWNER TO postgres;

--
-- TOC entry 3295 (class 0 OID 0)
-- Dependencies: 212
-- Name: Computers_CId_seq22; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq22" OWNED BY public."Computers"."CId";


--
-- TOC entry 213 (class 1259 OID 57720)
-- Name: Computers_CId_seq23; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq23"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq23" OWNER TO postgres;

--
-- TOC entry 3296 (class 0 OID 0)
-- Dependencies: 213
-- Name: Computers_CId_seq23; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq23" OWNED BY public."Computers"."CId";


--
-- TOC entry 214 (class 1259 OID 57722)
-- Name: Computers_CId_seq24; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq24"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq24" OWNER TO postgres;

--
-- TOC entry 3297 (class 0 OID 0)
-- Dependencies: 214
-- Name: Computers_CId_seq24; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq24" OWNED BY public."Computers"."CId";


--
-- TOC entry 215 (class 1259 OID 57724)
-- Name: Computers_CId_seq25; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq25"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq25" OWNER TO postgres;

--
-- TOC entry 3298 (class 0 OID 0)
-- Dependencies: 215
-- Name: Computers_CId_seq25; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq25" OWNED BY public."Computers"."CId";


--
-- TOC entry 216 (class 1259 OID 57726)
-- Name: Computers_CId_seq26; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq26"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq26" OWNER TO postgres;

--
-- TOC entry 3299 (class 0 OID 0)
-- Dependencies: 216
-- Name: Computers_CId_seq26; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Computers_CId_seq26" OWNED BY public."Computers"."CId";


--
-- TOC entry 254 (class 1259 OID 57810)
-- Name: Computers_CId_seq27; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Computers" ALTER COLUMN "CId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Computers_CId_seq27"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 217 (class 1259 OID 57728)
-- Name: Computers_CId_seq3; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq3"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq3" OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 57730)
-- Name: Computers_CId_seq4; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq4"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq4" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 57732)
-- Name: Computers_CId_seq5; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq5"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq5" OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 57734)
-- Name: Computers_CId_seq6; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq6"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq6" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 57736)
-- Name: Computers_CId_seq7; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq7"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq7" OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 57738)
-- Name: Computers_CId_seq8; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq8"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq8" OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 57740)
-- Name: Computers_CId_seq9; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Computers_CId_seq9"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Computers_CId_seq9" OWNER TO postgres;

--
-- TOC entry 256 (class 1259 OID 57818)
-- Name: Currencies; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Currencies" (
    "CrId" integer NOT NULL,
    "CrName" character varying(255),
    "CrCode" character varying(255),
    "CrCountry" character varying(255),
    "CrStatus" character varying(255),
    "CrCreatedDate" date,
    "CrModifiedDate" date
);


ALTER TABLE public."Currencies" OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 57824)
-- Name: CustomerClassification; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."CustomerClassification" (
    "ClId" integer NOT NULL,
    "ClName" character varying(255)
);


ALTER TABLE public."CustomerClassification" OWNER TO postgres;

--
-- TOC entry 258 (class 1259 OID 57827)
-- Name: Departments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Departments" (
    "DpId" integer NOT NULL,
    "DpBranch" integer,
    "DpName" character varying(255),
    "DpHead" integer,
    "DpRef" character varying(255)
);


ALTER TABLE public."Departments" OWNER TO postgres;

--
-- TOC entry 259 (class 1259 OID 57833)
-- Name: Discounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Discounts" (
    "DId" integer NOT NULL,
    "DRef" character varying(255),
    "DPerc" real,
    "DSetDate" date,
    "DEndDate" date,
    "DBranch" integer
);


ALTER TABLE public."Discounts" OWNER TO postgres;

--
-- TOC entry 260 (class 1259 OID 57836)
-- Name: Inventory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Inventory" (
    "InvtId" integer NOT NULL,
    "InvtType" character varying(255),
    "InvtName" character varying(255),
    "InvtQty" integer,
    "InvtReorderLevel" integer,
    "InvtDateAdded" date,
    "InvtDateModified" date,
    "InvtAddedBy" integer,
    "InvtModifiedBy" integer,
    "InvtCurrency" integer,
    "InvtVATId" integer,
    "InvtBranch" integer,
    "InvtCategory" integer,
    "InvtProdCode" character varying(255),
    "InvtRef" character varying(255) NOT NULL,
    "InvtBP" numeric,
    "InvtSP" numeric,
    "ProdDesc" text,
    "UOM" character varying(255),
    "Obsolete" boolean,
    "NonStock" boolean,
    "ProdImage" text,
    "BatchRef" character varying(255),
    "BOM" boolean,
    "StkType" character varying(255),
    "PartsPerUnit" integer,
    "UnitSeparator" character varying(255),
    "SupplierRef" character varying(255),
    "LeadTime" integer,
    "SLProdGrpCode" character varying(255),
    "PLProdGrpCode" character varying(255),
    "ProdDiscId" integer,
    "ProdDiscPerc" numeric(255,0),
    "UdCostPrice" numeric(10,2),
    "AvgCostPrice" numeric(10,2),
    "LastPrice" numeric(10,2),
    "Weight" real,
    "LastMovDate" date,
    "LastIssueDate" date,
    "WarehouseRef" character varying(255) DEFAULT NULL::character varying
);


ALTER TABLE public."Inventory" OWNER TO postgres;

--
-- TOC entry 261 (class 1259 OID 57843)
-- Name: LPODetails; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."LPODetails" (
    "PldID" integer,
    "PldRef" integer,
    "VatPerc" character varying(24),
    "VatAmt" numeric(19,4),
    "StkDesc" character varying(250),
    "UserID" integer,
    "ProdQty" integer,
    "Total" numeric(19,0),
    "UnitPrice" numeric(13,2),
    "PldDate" date
);


ALTER TABLE public."LPODetails" OWNER TO postgres;

--
-- TOC entry 262 (class 1259 OID 57846)
-- Name: LPOHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."LPOHeader" (
    "LID" integer NOT NULL,
    "LPOCustID" integer,
    "LPODate" date,
    "TransDate" date,
    "Prefix" character varying(255),
    "DocRef" integer,
    "CurrencyID" integer,
    "LDescription" character varying(255),
    "StaffID" integer,
    "Totals" numeric,
    "Invoiced" boolean,
    "LPOBranch" integer
);


ALTER TABLE public."LPOHeader" OWNER TO postgres;

--
-- TOC entry 263 (class 1259 OID 57852)
-- Name: LPOSettings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."LPOSettings" (
    "LPO_SID" integer NOT NULL,
    "LPO_SPrefix" character varying(100),
    "LPO_StartNO" integer,
    "LPO_NumberingType" character varying(100)
);


ALTER TABLE public."LPOSettings" OWNER TO postgres;

--
-- TOC entry 264 (class 1259 OID 57855)
-- Name: Licence; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Licence" (
    "LsId" integer NOT NULL,
    "LsType" character varying(255),
    "LsCode" character varying(255),
    "LsIssueDate" date,
    "LsExpireDate" date,
    "CompanyName" character varying(255),
    "CompanySlogan" character varying(255),
    "CompanyAdmin" integer,
    "CompanyPostal" character varying(255),
    "CompanyContact" character varying(255),
    "CompanyVAT" character varying(255),
    "PhysicalAddress" character varying(255),
    "CompanyLogo" text,
    "CompanyCurrency" integer
);


ALTER TABLE public."Licence" OWNER TO postgres;

--
-- TOC entry 3300 (class 0 OID 0)
-- Dependencies: 264
-- Name: COLUMN "Licence"."LsType"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public."Licence"."LsType" IS 'Trial or Commercial';


--
-- TOC entry 298 (class 1259 OID 60369)
-- Name: NLAccount; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NLAccount" (
    "NlAccCode" character varying(6) NOT NULL,
    "NlAccName" character varying(50),
    "GroupCode" character varying(6),
    "CurCode" character varying(6),
    "IsMirrorAcc" boolean,
    "MAccCode" character varying(6),
    "AGroupCode" character varying(6),
    "StatBalance" money,
    "LastStatDate" date
);


ALTER TABLE public."NLAccount" OWNER TO postgres;

--
-- TOC entry 265 (class 1259 OID 57867)
-- Name: NLAccountGroup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NLAccountGroup" (
    "GroupCode" character varying(6) NOT NULL,
    "GroupName" character varying(50),
    "PriGroupCode" character varying(6),
    "GroupType" character varying(50),
    "GroupSubType" character varying(50),
    "GroupLevel" integer,
    "UserID" integer,
    "UserName" character varying(100),
    "SwVerNo" character varying(50),
    "ModifiedOn" date,
    "DefaultGroup" integer
);


ALTER TABLE public."NLAccountGroup" OWNER TO postgres;

--
-- TOC entry 266 (class 1259 OID 57870)
-- Name: NLDateSummary; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NLDateSummary" (
    "NLAccCode" character varying(6),
    "MonthEndDate" date,
    "Cr" numeric(19,4),
    "Dr" numeric(19,4),
    "UserID" integer,
    "UserName" character varying(50)
);


ALTER TABLE public."NLDateSummary" OWNER TO postgres;

--
-- TOC entry 301 (class 1259 OID 60422)
-- Name: NLJournalDetails; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NLJournalDetails" (
    "JrnlSlNo" integer,
    "NlAccCode" character varying(6),
    "Dr" money,
    "Cr" money,
    "Amount" money,
    "Narration" character varying(255),
    "SLNarration" character varying(255),
    "IsForex" boolean,
    "FolioNo" character varying(100),
    "IsCleard" boolean,
    "ClearDate" date,
    "FCCleared" boolean,
    "FCClearDate" date,
    "VatAmount" money,
    "NlJrnlNo" bigint NOT NULL
);


ALTER TABLE public."NLJournalDetails" OWNER TO postgres;

--
-- TOC entry 302 (class 1259 OID 60440)
-- Name: NLJournalDetails_NlJrnlNo_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."NLJournalDetails_NlJrnlNo_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."NLJournalDetails_NlJrnlNo_seq" OWNER TO postgres;

--
-- TOC entry 3301 (class 0 OID 0)
-- Dependencies: 302
-- Name: NLJournalDetails_NlJrnlNo_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."NLJournalDetails_NlJrnlNo_seq" OWNED BY public."NLJournalDetails"."NlJrnlNo";


--
-- TOC entry 300 (class 1259 OID 60406)
-- Name: NlJournalHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NlJournalHeader" (
    "NlJrnlNo" bigint NOT NULL,
    "NlJrnlDesc" character varying(50),
    "TranDate" date,
    "MEndDate" date,
    "TranPeriod" numeric(2,0),
    "TranYear" numeric(4,0),
    "TranFrom" character varying(50),
    "TranType" character varying(50),
    "SlJrnlNo" bigint,
    "ModuleId" integer,
    "ModifiedOn" date
);


ALTER TABLE public."NlJournalHeader" OWNER TO postgres;

--
-- TOC entry 299 (class 1259 OID 60404)
-- Name: NlJournalHeader_NlJrnlNo_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."NlJournalHeader_NlJrnlNo_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."NlJournalHeader_NlJrnlNo_seq" OWNER TO postgres;

--
-- TOC entry 3302 (class 0 OID 0)
-- Dependencies: 299
-- Name: NlJournalHeader_NlJrnlNo_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."NlJournalHeader_NlJrnlNo_seq" OWNED BY public."NlJournalHeader"."NlJrnlNo";


--
-- TOC entry 305 (class 1259 OID 60480)
-- Name: PLAnalysisCodes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PLAnalysisCodes" (
    id integer NOT NULL,
    "AnalType" character varying(100),
    "AnalCode" character varying(100),
    "AnalDesc" character varying(100),
    "NLAccCode" character varying(6),
    "ModifiedOn" date
);


ALTER TABLE public."PLAnalysisCodes" OWNER TO postgres;

--
-- TOC entry 304 (class 1259 OID 60478)
-- Name: PLAnalysisCodes_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."PLAnalysisCodes_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."PLAnalysisCodes_id_seq" OWNER TO postgres;

--
-- TOC entry 3303 (class 0 OID 0)
-- Dependencies: 304
-- Name: PLAnalysisCodes_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."PLAnalysisCodes_id_seq" OWNED BY public."PLAnalysisCodes".id;


--
-- TOC entry 267 (class 1259 OID 57879)
-- Name: PLCustomer; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PLCustomer" (
    "PLCustCode" character varying(250) NOT NULL,
    "CustName" character varying(250),
    "PhysicalAddress" character varying(255),
    "PostalAddress" character varying(255),
    "CurrID" integer,
    "VATNo" character varying(50),
    "CustID" integer NOT NULL,
    "RegisterDate" date,
    "StaffID" integer,
    "CustBranch" integer
);


ALTER TABLE public."PLCustomer" OWNER TO postgres;

--
-- TOC entry 268 (class 1259 OID 57885)
-- Name: PLInvoiceDetail; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PLInvoiceDetail" (
    "PLJrnlNo" integer,
    "JrnlPLNo" integer,
    "UnitPrice" numeric(19,4),
    "VatPerc" character varying(24),
    "VatAmt" numeric(19,4),
    "ProdGroupCode" character varying(15),
    "NLAccCode" character varying(15),
    "StkDesc" character varying(100),
    "UserID" integer,
    "ProdQty" integer,
    "DiscountAmt" numeric(19,0),
    "Total" numeric(19,0),
    "ProdId" integer
);


ALTER TABLE public."PLInvoiceDetail" OWNER TO postgres;

--
-- TOC entry 269 (class 1259 OID 57888)
-- Name: PLInvoiceHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PLInvoiceHeader" (
    "PLJrnlNo" integer NOT NULL,
    "NlJrnlNo" integer,
    "PLCustID" integer,
    "TranDate" timestamp(6) without time zone,
    "Period" character varying(250),
    "DocRef" integer,
    "InvDate" date,
    "CurrencyId" integer,
    "PLDescription" character varying(255),
    "StaffId" integer,
    "DocPrefix" character varying(255),
    "HasCreditNote" boolean,
    "DueDate" date,
    "Totals" money,
    "Balance" money,
    "Additionals" character varying(255) DEFAULT ''::character varying,
    "ReturnStatus" character varying(255) DEFAULT NULL::character varying,
    "PLBranch" integer
);


ALTER TABLE public."PLInvoiceHeader" OWNER TO postgres;

--
-- TOC entry 270 (class 1259 OID 57896)
-- Name: PLInvoiceSettings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PLInvoiceSettings" (
    "InvSettingId" integer NOT NULL,
    "InvPrefix" character varying(255),
    "InvStartNumber" integer,
    "InvNumberingType" character varying(100)
);


ALTER TABLE public."PLInvoiceSettings" OWNER TO postgres;

--
-- TOC entry 303 (class 1259 OID 60455)
-- Name: SLAnalysisCodes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLAnalysisCodes" (
    "AnalType" character varying(100),
    "AnalCode" character varying(100),
    "AnalDesc" character varying(100),
    "ModifiedOn" date,
    "NLAccCode" character varying(6),
    id integer NOT NULL
);


ALTER TABLE public."SLAnalysisCodes" OWNER TO postgres;

--
-- TOC entry 306 (class 1259 OID 60486)
-- Name: SLAnalysisCodes_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."SLAnalysisCodes_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."SLAnalysisCodes_id_seq" OWNER TO postgres;

--
-- TOC entry 3304 (class 0 OID 0)
-- Dependencies: 306
-- Name: SLAnalysisCodes_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."SLAnalysisCodes_id_seq" OWNED BY public."SLAnalysisCodes".id;


--
-- TOC entry 225 (class 1259 OID 57744)
-- Name: SLCustomer_SLCustomerSerial_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."SLCustomer_SLCustomerSerial_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."SLCustomer_SLCustomerSerial_seq" OWNER TO postgres;

--
-- TOC entry 271 (class 1259 OID 57899)
-- Name: SLCustomer; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLCustomer" (
    "SLCustomerSerial" integer DEFAULT nextval('public."SLCustomer_SLCustomerSerial_seq"'::regclass) NOT NULL,
    "CustCode" character varying(100) NOT NULL,
    "CustFirstName" character varying(100),
    "Address" character varying(255),
    "PostalAddress" character varying(100),
    "PostalCode" character varying(250),
    "CurCode" integer,
    "CustEmail" character varying(255),
    "CustContact" character varying(100) NOT NULL,
    "SLCTypeID" integer NOT NULL,
    "CustLastName" character varying(100),
    "CustType" character varying(100),
    "CustCompany" character varying(255),
    "VATNo" character varying(255),
    "CustCreditLimit" real,
    "VATpin" character varying(255),
    "CreditTerms" integer,
    "Status" character varying(255),
    "CustRef" character varying(255) NOT NULL,
    "CustBranch" integer,
    "CustomerDept" numeric
);


ALTER TABLE public."SLCustomer" OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 57742)
-- Name: SLCustomerTypes_SLCTypeID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."SLCustomerTypes_SLCTypeID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."SLCustomerTypes_SLCTypeID_seq" OWNER TO postgres;

--
-- TOC entry 272 (class 1259 OID 57906)
-- Name: SLCustomerTypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLCustomerTypes" (
    "SLCTypeID" integer DEFAULT nextval('public."SLCustomerTypes_SLCTypeID_seq"'::regclass) NOT NULL,
    "TypeName" character varying(100)
);


ALTER TABLE public."SLCustomerTypes" OWNER TO postgres;

--
-- TOC entry 273 (class 1259 OID 57910)
-- Name: SLInvoiceDetail; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLInvoiceDetail" (
    "SLJrnlNo" integer,
    "JrnlSLNo" integer,
    "VatCode" text,
    "VatAmt" numeric(24,0),
    "StkDesc" text,
    "UserID" integer,
    "ItemSerial" text NOT NULL,
    "ItemQty" integer,
    "ItemTotals" numeric(24,0),
    "ItemUnitPrice" numeric(24,0),
    "DiscountPerc" numeric(24,0),
    "DiscountAmt" numeric(24,0),
    "AdditionalDetails" character varying(255),
    "ItemId" integer,
    "ItemCode" character varying(255),
    "ItemName" character varying(255),
    "ProdGroupCode" character varying(255)
);


ALTER TABLE public."SLInvoiceDetail" OWNER TO postgres;

--
-- TOC entry 274 (class 1259 OID 57916)
-- Name: SLInvoiceHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLInvoiceHeader" (
    "SLJrnlNo" integer NOT NULL,
    "NlJrnlNo" integer,
    "CustCode" character varying(250),
    "TransDate" timestamp(6) without time zone,
    "Period" character varying(250),
    "DocRef" integer,
    "TotalAmount" numeric(24,0),
    "INVTypeRef" character varying(100),
    "Dispute" boolean DEFAULT false,
    "DeliveryCust" integer,
    "DeliveryAddress" character varying(255),
    "DeliveryDue" date,
    "INVDate" date,
    "CustId" integer,
    "PaymentDays" integer,
    "CustomRef" character varying(255),
    "CurrencyId" integer,
    "DueDate" date,
    "SLDescription" character varying(255),
    "StaffID" integer,
    "DocPrefix" character varying(255),
    "CRNReason" character varying(255),
    "HasCreditNote" boolean,
    "Branch" integer,
    "InvPrinted" boolean,
    "TotalBalance" numeric(255,0)
);


ALTER TABLE public."SLInvoiceHeader" OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 57746)
-- Name: SLInvoiceHeader_SLJrnlNo_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."SLInvoiceHeader_SLJrnlNo_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."SLInvoiceHeader_SLJrnlNo_seq" OWNER TO postgres;

--
-- TOC entry 275 (class 1259 OID 57923)
-- Name: SLInvoiceSettings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLInvoiceSettings" (
    "InvSettingId" integer NOT NULL,
    "InvPrefix" character varying(255),
    "InvStartNumber" integer,
    "InvNumberingType" character varying(100),
    "InvDeliveryNotes" integer,
    "InvType" character varying(255),
    "InvBranch" integer
);


ALTER TABLE public."SLInvoiceSettings" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 57748)
-- Name: SLInvoiceTypes_INVypeID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."SLInvoiceTypes_INVypeID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."SLInvoiceTypes_INVypeID_seq" OWNER TO postgres;

--
-- TOC entry 276 (class 1259 OID 57929)
-- Name: SLInvoiceTypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLInvoiceTypes" (
    "INVypeID" integer DEFAULT nextval('public."SLInvoiceTypes_INVypeID_seq"'::regclass) NOT NULL,
    "INVType" character varying(100),
    "INVComment" text
);


ALTER TABLE public."SLInvoiceTypes" OWNER TO postgres;

--
-- TOC entry 277 (class 1259 OID 57936)
-- Name: SLProformaInvoiceDetails; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLProformaInvoiceDetails" (
    "SLJrnlNo" integer,
    "JrnlSLNo" integer,
    "InvAmt" real,
    "VatCode" text,
    "VatAmt" real,
    "ProdGroupCode" text,
    "NLAccCode" text,
    "StkDesc" text,
    "UserID" integer,
    "ItemSerial" text NOT NULL,
    "ItemQty" integer,
    "ItemTotals" real,
    "ItemUnitPrice" real,
    "DiscountPerc" real,
    "DiscountAmt" real,
    "AdditionalDetails" character varying(255)
);


ALTER TABLE public."SLProformaInvoiceDetails" OWNER TO postgres;

--
-- TOC entry 278 (class 1259 OID 57942)
-- Name: SLProformaInvoiceHeader; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLProformaInvoiceHeader" (
    "SLJrnlNo" integer NOT NULL,
    "NlJrnlNo" integer,
    "CustCode" character varying(250),
    "TransDate" timestamp(6) without time zone,
    "Period" character varying(250),
    "DocRef" integer NOT NULL,
    "TotalAmount" numeric(24,0),
    "INVTypeRef" character varying(255),
    "Dispute" boolean DEFAULT false,
    "DeliveryCust" integer,
    "DeliveryAddress" character varying(255),
    "DeliveryDue" date,
    "INVDate" date,
    "CustId" integer,
    "PaymentDays" integer,
    "CustomRef" character varying(255),
    "CurrencyId" integer,
    "DueDate" date,
    "SLDescription" character varying(255),
    "StaffID" integer,
    "DocPrefix" character varying(255),
    "HasInvoice" boolean,
    "BranchRef" integer
);


ALTER TABLE public."SLProformaInvoiceHeader" OWNER TO postgres;

--
-- TOC entry 279 (class 1259 OID 57949)
-- Name: SLReceipts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SLReceipts" (
    "pyID" integer NOT NULL,
    "pyRef" integer,
    "pyDate" date,
    "pyInvRef" integer,
    "pyPayable" real,
    "pyPaid" real DEFAULT 0,
    "pyBalance" real,
    "pyMode" character varying(255),
    "pyChequeNumber" character varying(255),
    "pyReceivedFrom" character varying(255),
    "pyAdditionalDetails" character varying(255),
    "pyProcessDate" date,
    "pyUser" integer,
    "pyReturned" boolean,
    "pyReturnReason" character varying(255),
    "pyBranch" integer
);


ALTER TABLE public."SLReceipts" OWNER TO postgres;

--
-- TOC entry 281 (class 1259 OID 57958)
-- Name: Settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Settings" (
    "StId" integer NOT NULL,
    "StName" character varying(255),
    "StValue" character varying(255)
);


ALTER TABLE public."Settings" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 57750)
-- Name: Settings_StId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq" OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 57752)
-- Name: Settings_StId_seq1; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq1"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq1" OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 57754)
-- Name: Settings_StId_seq10; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq10"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq10" OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 57756)
-- Name: Settings_StId_seq11; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq11"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq11" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 57758)
-- Name: Settings_StId_seq12; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq12"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq12" OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 57760)
-- Name: Settings_StId_seq13; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq13"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq13" OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 57762)
-- Name: Settings_StId_seq14; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq14"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq14" OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 57764)
-- Name: Settings_StId_seq15; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq15"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq15" OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 57766)
-- Name: Settings_StId_seq16; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq16"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq16" OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 57768)
-- Name: Settings_StId_seq17; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq17"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq17" OWNER TO postgres;

--
-- TOC entry 3305 (class 0 OID 0)
-- Dependencies: 237
-- Name: Settings_StId_seq17; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq17" OWNED BY public."Settings"."StId";


--
-- TOC entry 238 (class 1259 OID 57770)
-- Name: Settings_StId_seq18; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq18"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq18" OWNER TO postgres;

--
-- TOC entry 3306 (class 0 OID 0)
-- Dependencies: 238
-- Name: Settings_StId_seq18; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq18" OWNED BY public."Settings"."StId";


--
-- TOC entry 239 (class 1259 OID 57772)
-- Name: Settings_StId_seq19; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq19"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq19" OWNER TO postgres;

--
-- TOC entry 3307 (class 0 OID 0)
-- Dependencies: 239
-- Name: Settings_StId_seq19; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq19" OWNED BY public."Settings"."StId";


--
-- TOC entry 240 (class 1259 OID 57774)
-- Name: Settings_StId_seq2; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq2"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq2" OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 57776)
-- Name: Settings_StId_seq20; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq20"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq20" OWNER TO postgres;

--
-- TOC entry 3308 (class 0 OID 0)
-- Dependencies: 241
-- Name: Settings_StId_seq20; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq20" OWNED BY public."Settings"."StId";


--
-- TOC entry 242 (class 1259 OID 57778)
-- Name: Settings_StId_seq21; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq21"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq21" OWNER TO postgres;

--
-- TOC entry 3309 (class 0 OID 0)
-- Dependencies: 242
-- Name: Settings_StId_seq21; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq21" OWNED BY public."Settings"."StId";


--
-- TOC entry 243 (class 1259 OID 57780)
-- Name: Settings_StId_seq22; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq22"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq22" OWNER TO postgres;

--
-- TOC entry 3310 (class 0 OID 0)
-- Dependencies: 243
-- Name: Settings_StId_seq22; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq22" OWNED BY public."Settings"."StId";


--
-- TOC entry 244 (class 1259 OID 57782)
-- Name: Settings_StId_seq23; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq23"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq23" OWNER TO postgres;

--
-- TOC entry 3311 (class 0 OID 0)
-- Dependencies: 244
-- Name: Settings_StId_seq23; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Settings_StId_seq23" OWNED BY public."Settings"."StId";


--
-- TOC entry 280 (class 1259 OID 57956)
-- Name: Settings_StId_seq24; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Settings" ALTER COLUMN "StId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Settings_StId_seq24"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 245 (class 1259 OID 57784)
-- Name: Settings_StId_seq3; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq3"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq3" OWNER TO postgres;

--
-- TOC entry 246 (class 1259 OID 57786)
-- Name: Settings_StId_seq4; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq4"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq4" OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 57788)
-- Name: Settings_StId_seq5; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq5"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq5" OWNER TO postgres;

--
-- TOC entry 248 (class 1259 OID 57790)
-- Name: Settings_StId_seq6; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq6"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq6" OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 57792)
-- Name: Settings_StId_seq7; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq7"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq7" OWNER TO postgres;

--
-- TOC entry 250 (class 1259 OID 57794)
-- Name: Settings_StId_seq8; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq8"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq8" OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 57796)
-- Name: Settings_StId_seq9; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Settings_StId_seq9"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public."Settings_StId_seq9" OWNER TO postgres;

--
-- TOC entry 282 (class 1259 OID 57964)
-- Name: UserPermissions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."UserPermissions" (
    "PUser" integer,
    "ViewBranches" boolean DEFAULT false,
    "AddBranch" boolean DEFAULT false,
    "EditBranch" boolean DEFAULT false,
    "DeleteBranch" boolean DEFAULT false,
    "ViewCurrency" boolean,
    "EditCurrency" boolean,
    "DeleteCurrency" boolean,
    "AddCurrency" boolean,
    "PId" integer NOT NULL,
    "ReadCustomer" boolean,
    "AddCustomer" boolean,
    "UpdateCustomer" boolean,
    "DeleteCustomer" boolean,
    "ReadVAT" boolean,
    "AddVAT" boolean,
    "UpdateVAT" boolean,
    "DeleteVAT" boolean,
    "AddInvoice" boolean,
    "AddCredNote" boolean,
    "ReadInventory" boolean,
    "AddInventory" boolean,
    "ModifyInventory" boolean,
    "ReadInvoices" boolean,
    "SLSettings" boolean,
    "PLSettings" boolean,
    "ReadLPO" boolean,
    "ManageLPO" boolean,
    "ReadPurchaseReceipts" boolean,
    "ManagePurchaseReceipts" boolean,
    "ReadPurchaseRequests" boolean,
    "ReceivePurchaseRequest" boolean,
    "PurchaseRequestAction" boolean,
    "CreatePurchaseRequest" boolean,
    "stockTakeRead" boolean,
    "stockTakeCreate" boolean,
    "stockTakeAction" boolean,
    "ApprovePurchaseReturn" boolean,
    "ReadPurchaseReturn" boolean,
    "CreatePurchaseReturn" boolean,
    "ReadDepartments" boolean,
    "ManageDepartments" boolean,
    "ReadUsers" boolean,
    "ManageUsers" boolean,
    "ReadPermissions" boolean,
    "ManagePermissions" boolean,
    "ManageCategories" boolean,
    "ManageFinancialPeriods" boolean,
    "ManageWarehouses" boolean,
    "StockReports" boolean
);


ALTER TABLE public."UserPermissions" OWNER TO postgres;

--
-- TOC entry 283 (class 1259 OID 57971)
-- Name: Users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Users" (
    "UFirstName" character varying(255),
    "ULastName" character varying(255),
    "UEmail" character varying(255),
    "UPassword" character varying(255),
    "UType" character varying(255),
    "UContact" character varying(20),
    "UStatus" character varying(100),
    "UCompany" character varying(255),
    "UFirst" boolean DEFAULT false,
    "UProfile" text,
    "RegistrationDate" date,
    "UBranch" integer,
    "UDepartment" integer,
    "Username" character varying(255),
    "UId" integer NOT NULL,
    "UVAT" character varying(255) DEFAULT ''::character varying,
    "UIdnumber" integer
);


ALTER TABLE public."Users" OWNER TO postgres;

--
-- TOC entry 284 (class 1259 OID 57979)
-- Name: VATs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."VATs" (
    "VtId" integer NOT NULL,
    "VtRef" character varying(255),
    "VtPerc" real,
    "VtSetDate" timestamp(0) without time zone,
    "VtModifyDate" timestamp(0) without time zone,
    "VtActive" boolean,
    "VtBranch" integer
);


ALTER TABLE public."VATs" OWNER TO postgres;

--
-- TOC entry 285 (class 1259 OID 57982)
-- Name: financial_periods; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.financial_periods (
    fp_id integer NOT NULL,
    fp_ref character varying(255),
    fp_name character varying(255),
    fp_trans_date date,
    fp_openingdate date,
    fp_closingdate date,
    fp_active boolean,
    fp_createdby integer,
    fp_closedby integer,
    fp_authorisedby integer,
    fp_createdon date,
    fp_branch integer,
    fp_date_mode character varying(250)
);


ALTER TABLE public.financial_periods OWNER TO postgres;

--
-- TOC entry 286 (class 1259 OID 57988)
-- Name: inventory_category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inventory_category (
    cat_id integer NOT NULL,
    cat_entry_date date,
    cat_name character varying(255),
    cat_ref character varying(255),
    cat_branch integer
);


ALTER TABLE public.inventory_category OWNER TO postgres;

--
-- TOC entry 287 (class 1259 OID 57994)
-- Name: purchase_order_details; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_order_details (
    pod_ref integer,
    pod_itemname character varying(255),
    pod_qty integer,
    pod_unitprice money,
    pod_total money,
    pod_vat_perc numeric,
    pod_vat_amt money,
    pod_itemid integer
);


ALTER TABLE public.purchase_order_details OWNER TO postgres;

--
-- TOC entry 288 (class 1259 OID 58000)
-- Name: purchase_order_header; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_order_header (
    po_id integer NOT NULL,
    po_date date,
    po_prefix character varying(255),
    po_ref integer,
    po_user integer,
    po_total money,
    po_status character varying(255),
    po_approvedby integer,
    po_sender_signature integer,
    po_transdate date,
    po_approval_signature integer,
    po_has_lpo boolean
);


ALTER TABLE public.purchase_order_header OWNER TO postgres;

--
-- TOC entry 289 (class 1259 OID 58006)
-- Name: purchase_receipt_details; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_receipt_details (
    pd_date date,
    pd_ref integer,
    pd_item integer,
    pd_qty integer,
    pd_unitprice money,
    pd_vat_perc character varying(255),
    pd_vat_amt money,
    pd_totals money
);


ALTER TABLE public.purchase_receipt_details OWNER TO postgres;

--
-- TOC entry 290 (class 1259 OID 58009)
-- Name: purchase_receipt_header; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_receipt_header (
    pr_id integer NOT NULL,
    pr_date date,
    pr_ref integer,
    pr_prefix character varying(255),
    pr_customer integer,
    pr_user integer,
    pr_total money,
    pr_currency integer,
    pr_additional character varying(255),
    pr_invoiced boolean,
    pr_transdate date,
    pr_returned boolean
);


ALTER TABLE public.purchase_receipt_header OWNER TO postgres;

--
-- TOC entry 291 (class 1259 OID 58015)
-- Name: purchase_return_details; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_return_details (
    pr_ref character varying(255) NOT NULL,
    pr_pl_invref integer,
    pr_item_name character varying(255),
    pr_item_qty integer,
    pr_reason text
);


ALTER TABLE public.purchase_return_details OWNER TO postgres;

--
-- TOC entry 292 (class 1259 OID 58021)
-- Name: purchase_return_header; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchase_return_header (
    prh_ref character varying(255) NOT NULL,
    prh_date date,
    prh_pljrnl integer,
    returnedby integer,
    returner_signature integer,
    approvedby integer,
    approver_signature integer,
    status character varying(255) DEFAULT NULL::character varying,
    prh_staff integer
);


ALTER TABLE public.purchase_return_header OWNER TO postgres;

--
-- TOC entry 293 (class 1259 OID 58028)
-- Name: signatures; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.signatures (
    sign_id integer NOT NULL,
    sign_date date,
    sign_user integer,
    sign_data text,
    sign_name character varying(255)
);


ALTER TABLE public.signatures OWNER TO postgres;

--
-- TOC entry 294 (class 1259 OID 58039)
-- Name: stock_take_details; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.stock_take_details (
    stk_id integer NOT NULL,
    stk_date date,
    stk_item_id integer,
    stk_item_name character varying(255),
    store_qty integer,
    curr_qty integer,
    stk_ref character varying(255) NOT NULL,
    stk_has_issue boolean
);


ALTER TABLE public.stock_take_details OWNER TO postgres;

--
-- TOC entry 295 (class 1259 OID 58045)
-- Name: stock_take_header; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.stock_take_header (
    sth_id integer NOT NULL,
    sth_date date,
    sth_ref character varying(255),
    sth_name character varying(255),
    sth_staff integer,
    sth_approved boolean,
    approved_by integer,
    approval_date date,
    has_issue boolean,
    approver_signature integer
);


ALTER TABLE public.stock_take_header OWNER TO postgres;

--
-- TOC entry 296 (class 1259 OID 58051)
-- Name: warehouse_summary; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.warehouse_summary (
    ws_id integer NOT NULL,
    prod_id integer,
    wh_ref character varying(255),
    bincode character varying(255),
    openstock integer,
    qty_issued integer,
    qty_received integer,
    qty_adjusted integer,
    qty_allocated integer,
    rt_rct_qty integer,
    rt_issue_qty integer,
    qty_on_order integer,
    physical_qty integer,
    free_qty integer,
    min_stock_qty integer,
    max_stock_qty integer,
    modified_on date,
    ws_branch integer,
    ws_date date
);


ALTER TABLE public.warehouse_summary OWNER TO postgres;

--
-- TOC entry 3312 (class 0 OID 0)
-- Dependencies: 296
-- Name: COLUMN warehouse_summary.rt_rct_qty; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.warehouse_summary.rt_rct_qty IS 'Return receipt quantity';


--
-- TOC entry 297 (class 1259 OID 58057)
-- Name: warehouses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.warehouses (
    wh_ref character varying(255) NOT NULL,
    wh_code character varying(255) NOT NULL,
    wh_desc character varying(255),
    wh_address_1 character varying(255),
    wh_address_2 character varying(255),
    wh_address_3 character varying(255),
    wh_address_4 character varying(255),
    wh_type character varying(255),
    wh_stage character varying(255),
    wh_modifiedon date,
    wh_createdon date,
    wh_branch integer
);


ALTER TABLE public.warehouses OWNER TO postgres;

--
-- TOC entry 3035 (class 2604 OID 60442)
-- Name: NLJournalDetails NlJrnlNo; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NLJournalDetails" ALTER COLUMN "NlJrnlNo" SET DEFAULT nextval('public."NLJournalDetails_NlJrnlNo_seq"'::regclass);


--
-- TOC entry 3034 (class 2604 OID 60409)
-- Name: NlJournalHeader NlJrnlNo; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NlJournalHeader" ALTER COLUMN "NlJrnlNo" SET DEFAULT nextval('public."NlJournalHeader_NlJrnlNo_seq"'::regclass);


--
-- TOC entry 3037 (class 2604 OID 60483)
-- Name: PLAnalysisCodes id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PLAnalysisCodes" ALTER COLUMN id SET DEFAULT nextval('public."PLAnalysisCodes_id_seq"'::regclass);


--
-- TOC entry 3036 (class 2604 OID 60488)
-- Name: SLAnalysisCodes id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."SLAnalysisCodes" ALTER COLUMN id SET DEFAULT nextval('public."SLAnalysisCodes_id_seq"'::regclass);


--
-- TOC entry 3233 (class 0 OID 57798)
-- Dependencies: 252
-- Data for Name: AllSystemTerms; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AllSystemTerms" ("tosID", "tosType", terms, branch) FROM stdin;
2	pl_inv_terms	&lt;blockquote&gt;L.P.O Terms&lt;/blockquote&gt;&lt;p&gt;The following are the &lt;strong&gt;required &lt;/strong&gt;terms:&lt;/p&gt;&lt;ul&gt;&lt;li&gt;&lt;em&gt;Business MUST be between the business hours (8:00am - 5:00pm)&lt;/em&gt;&lt;/li&gt;&lt;li&gt;&lt;em&gt;Transacting 3rd-parties MUST hold national IDs / Passports&lt;/em&gt;&lt;/li&gt;&lt;li&gt;Payments made must done via the following any accounts&lt;/li&gt;&lt;/ul&gt;&lt;p&gt;&lt;img src=&quot;https://www.multiplesgroup.com/wp-content/uploads/2020/01/top-commercial-banks-ranking-kenya-Copy-1024x344.jpg&quot; alt=&quot;banks&quot; width=&quot;293&quot; height=&quot;121&quot;&gt;&lt;/p&gt;&lt;p&gt;&lt;strong&gt;Barclays Bank AC Details&lt;/strong&gt;:&lt;/p&gt;	54751507
1	inv_terms	&lt;p&gt;&lt;br&gt;&lt;/p&gt;&lt;p&gt;&lt;img src=&quot;https://hics.nhif.or.ke/skins/default/images/login_top_icon.jpg&quot; alt=&quot;NGENX Limited&quot; width=&quot;94&quot; height=&quot;26&quot;&gt;&lt;/p&gt;&lt;p&gt;Terms of Sale&lt;/p&gt;&lt;ol&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Goods once sold are NOT returnable&lt;/span&gt;&lt;/li&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Net 7 - Payment seven days after invoice date&lt;/span&gt;&lt;/li&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Net 10 - Payment ten days after invoice date&lt;/span&gt;&lt;/li&gt;&lt;li&gt;&lt;span style=&quot;font-size: 8pt; font-family: Tahoma;&quot;&gt;Net 30 - Payment 30 days after invoice date&lt;/span&gt;&lt;/li&gt;&lt;/ol&gt;&lt;p&gt;&lt;/p&gt;&lt;p&gt;&lt;img src=&quot;https://hatrabbits.com/wp-content/uploads/2017/01/random.jpg&quot; alt=&quot;Random&quot; width=&quot;257&quot; height=&quot;118&quot;&gt;&lt;/p&gt;&lt;p style=&quot;text-align: right;&quot;&gt;&lt;br&gt;&lt;/p&gt;	54751507
\.


--
-- TOC entry 3234 (class 0 OID 57804)
-- Dependencies: 253
-- Data for Name: Branches; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Branches" ("BrId", "BrName", "BrLocation", "BrCode", "ContactStaff", "BrActive") FROM stdin;
\.


--
-- TOC entry 3236 (class 0 OID 57812)
-- Dependencies: 255
-- Data for Name: Computers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Computers" ("CId", "CName", "CIP", "CMac", "CUser", "CRegisterDate") FROM stdin;
\.


--
-- TOC entry 3237 (class 0 OID 57818)
-- Dependencies: 256
-- Data for Name: Currencies; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Currencies" ("CrId", "CrName", "CrCode", "CrCountry", "CrStatus", "CrCreatedDate", "CrModifiedDate") FROM stdin;
9	Kenya Shillings	KES	Kenya	Active	2020-12-23	2020-12-23
61091744	Ugandan Shilling	UGX	Uganda	Active	2021-01-05	2021-01-05
28645294	Tanzania Shilling	TZS	Tanzania	Inactive	2021-01-06	2021-01-06
\.


--
-- TOC entry 3238 (class 0 OID 57824)
-- Dependencies: 257
-- Data for Name: CustomerClassification; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."CustomerClassification" ("ClId", "ClName") FROM stdin;
\.


--
-- TOC entry 3239 (class 0 OID 57827)
-- Dependencies: 258
-- Data for Name: Departments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Departments" ("DpId", "DpBranch", "DpName", "DpHead", "DpRef") FROM stdin;
1	54751507	PROCUREMENT	1	ferdwe-77887w-svnsvs
2	54751507	FINANCE	1	3f0dbc8d-f58c-4103-a8a8-ebb009e3b7dd
\.







--
-- TOC entry 3244 (class 0 OID 57852)
-- Dependencies: 263
-- Data for Name: LPOSettings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."LPOSettings" ("LPO_SID", "LPO_SPrefix", "LPO_StartNO", "LPO_NumberingType") FROM stdin;
1	LPO-	5	Manual
\.



--
-- TOC entry 3279 (class 0 OID 60369)
-- Dependencies: 298
-- Data for Name: NLAccount; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."NLAccount" ("NlAccCode", "NlAccName", "GroupCode", "CurCode", "IsMirrorAcc", "MAccCode", "AGroupCode", "StatBalance", "LastStatDate") FROM stdin;
0002	MISC. CREDITS	RG0003	KES	t	\N	\N	$0.00	2021-08-27
0005	ADVERTISEMET	RG0007	KES	f			$0.00	2021-09-09
0006	AUDIT FEE	RG0007	KES	f			$0.00	2021-09-09
0007	BANK CHARGES	RG0007	KES	f			$0.00	2021-09-15
0008	DIVIDEND	RG0011	KES	f			$0.00	2021-09-22
0001	SALES	RG0006		t			$0.00	2021-08-27
\.



--
-- TOC entry 3247 (class 0 OID 57870)
-- Dependencies: 266
-- Data for Name: NLDateSummary; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."NLDateSummary" ("NLAccCode", "MonthEndDate", "Cr", "Dr", "UserID", "UserName") FROM stdin;
\.





--
-- TOC entry 3286 (class 0 OID 60480)
-- Dependencies: 305
-- Data for Name: PLAnalysisCodes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PLAnalysisCodes" (id, "AnalType", "AnalCode", "AnalDesc", "NLAccCode", "ModifiedOn") FROM stdin;
1	PG	PURCH	business purchases	0001	2021-09-27
\.




--
-- TOC entry 3250 (class 0 OID 57888)
-- Dependencies: 269
-- Data for Name: PLInvoiceHeader; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PLInvoiceHeader" ("PLJrnlNo", "NlJrnlNo", "PLCustID", "TranDate", "Period", "DocRef", "InvDate", "CurrencyId", "PLDescription", "StaffId", "DocPrefix", "HasCreditNote", "DueDate", "Totals", "Balance", "Additionals", "ReturnStatus", "PLBranch") FROM stdin;
\.


--
-- TOC entry 3251 (class 0 OID 57896)
-- Dependencies: 270
-- Data for Name: PLInvoiceSettings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PLInvoiceSettings" ("InvSettingId", "InvPrefix", "InvStartNumber", "InvNumberingType") FROM stdin;
1	INVP	3	Auto
\.





--
-- TOC entry 3253 (class 0 OID 57906)
-- Dependencies: 272
-- Data for Name: SLCustomerTypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."SLCustomerTypes" ("SLCTypeID", "TypeName") FROM stdin;
1	GOLD CUSTOMER
2	SILVER CUSTOMER
\.




--
-- TOC entry 3256 (class 0 OID 57923)
-- Dependencies: 275
-- Data for Name: SLInvoiceSettings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."SLInvoiceSettings" ("InvSettingId", "InvPrefix", "InvStartNumber", "InvNumberingType", "InvDeliveryNotes", "InvType", "InvBranch") FROM stdin;
1	INV	0	Auto	2	INV	54751507
\.


--
-- TOC entry 3257 (class 0 OID 57929)
-- Dependencies: 276
-- Data for Name: SLInvoiceTypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."SLInvoiceTypes" ("INVypeID", "INVType", "INVComment") FROM stdin;
23	CUSTOMER INVOICE	This is the invoice sent to the customer
25	SECURITY INVOICE	Invoice to be left at the gate
24	MASTER INVOICE	Invoice Left at the company
\.






--
-- TOC entry 3262 (class 0 OID 57958)
-- Dependencies: 281
-- Data for Name: Settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Settings" ("StId", "StName", "StValue") FROM stdin;
1	ScreenIdleTimeout	10
\.


--
-- TOC entry 3263 (class 0 OID 57964)
-- Dependencies: 282
-- Data for Name: UserPermissions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."UserPermissions" ("PUser", "ViewBranches", "AddBranch", "EditBranch", "DeleteBranch", "ViewCurrency", "EditCurrency", "DeleteCurrency", "AddCurrency", "PId", "ReadCustomer", "AddCustomer", "UpdateCustomer", "DeleteCustomer", "ReadVAT", "AddVAT", "UpdateVAT", "DeleteVAT", "AddInvoice", "AddCredNote", "ReadInventory", "AddInventory", "ModifyInventory", "ReadInvoices", "SLSettings", "PLSettings", "ReadLPO", "ManageLPO", "ReadPurchaseReceipts", "ManagePurchaseReceipts", "ReadPurchaseRequests", "ReceivePurchaseRequest", "PurchaseRequestAction", "CreatePurchaseRequest", "stockTakeRead", "stockTakeCreate", "stockTakeAction", "ApprovePurchaseReturn", "ReadPurchaseReturn", "CreatePurchaseReturn", "ReadDepartments", "ManageDepartments", "ReadUsers", "ManageUsers", "ReadPermissions", "ManagePermissions", "ManageCategories", "ManageFinancialPeriods", "ManageWarehouses", "StockReports") FROM stdin;
1	t	t	t	t	f	t	t	f	49600761	t	t	t	t	t	t	f	f	t	t	t	t	t	t	f	t	t	t	t	t	t	t	\N	t	t	t	t	t	t	t	t	t	t	t	\N	\N	t	t	t	t
\.








--
-- TOC entry 3313 (class 0 OID 0)
-- Dependencies: 196
-- Name: Branches_BrId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Branches_BrId_seq"', 8, false);


--
-- TOC entry 3314 (class 0 OID 0)
-- Dependencies: 197
-- Name: Computers_CId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq"', 8, false);


--
-- TOC entry 3315 (class 0 OID 0)
-- Dependencies: 198
-- Name: Computers_CId_seq1; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq1"', 8, false);


--
-- TOC entry 3316 (class 0 OID 0)
-- Dependencies: 199
-- Name: Computers_CId_seq10; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq10"', 8, false);


--
-- TOC entry 3317 (class 0 OID 0)
-- Dependencies: 200
-- Name: Computers_CId_seq11; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq11"', 8, false);


--
-- TOC entry 3318 (class 0 OID 0)
-- Dependencies: 201
-- Name: Computers_CId_seq12; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq12"', 8, false);


--
-- TOC entry 3319 (class 0 OID 0)
-- Dependencies: 202
-- Name: Computers_CId_seq13; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq13"', 8, false);


--
-- TOC entry 3320 (class 0 OID 0)
-- Dependencies: 203
-- Name: Computers_CId_seq14; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq14"', 8, false);


--
-- TOC entry 3321 (class 0 OID 0)
-- Dependencies: 204
-- Name: Computers_CId_seq15; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq15"', 8, false);


--
-- TOC entry 3322 (class 0 OID 0)
-- Dependencies: 205
-- Name: Computers_CId_seq16; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq16"', 8, false);


--
-- TOC entry 3323 (class 0 OID 0)
-- Dependencies: 206
-- Name: Computers_CId_seq17; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq17"', 8, false);


--
-- TOC entry 3324 (class 0 OID 0)
-- Dependencies: 207
-- Name: Computers_CId_seq18; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq18"', 8, false);


--
-- TOC entry 3325 (class 0 OID 0)
-- Dependencies: 208
-- Name: Computers_CId_seq19; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq19"', 8, false);


--
-- TOC entry 3326 (class 0 OID 0)
-- Dependencies: 209
-- Name: Computers_CId_seq2; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq2"', 8, false);


--
-- TOC entry 3327 (class 0 OID 0)
-- Dependencies: 210
-- Name: Computers_CId_seq20; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq20"', 8, false);


--
-- TOC entry 3328 (class 0 OID 0)
-- Dependencies: 211
-- Name: Computers_CId_seq21; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq21"', 7, false);


--
-- TOC entry 3329 (class 0 OID 0)
-- Dependencies: 212
-- Name: Computers_CId_seq22; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq22"', 6, false);


--
-- TOC entry 3330 (class 0 OID 0)
-- Dependencies: 213
-- Name: Computers_CId_seq23; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq23"', 5, false);


--
-- TOC entry 3331 (class 0 OID 0)
-- Dependencies: 214
-- Name: Computers_CId_seq24; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq24"', 4, false);


--
-- TOC entry 3332 (class 0 OID 0)
-- Dependencies: 215
-- Name: Computers_CId_seq25; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq25"', 3, false);


--
-- TOC entry 3333 (class 0 OID 0)
-- Dependencies: 216
-- Name: Computers_CId_seq26; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq26"', 2, false);


--
-- TOC entry 3334 (class 0 OID 0)
-- Dependencies: 254
-- Name: Computers_CId_seq27; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq27"', 1, false);


--
-- TOC entry 3335 (class 0 OID 0)
-- Dependencies: 217
-- Name: Computers_CId_seq3; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq3"', 8, false);


--
-- TOC entry 3336 (class 0 OID 0)
-- Dependencies: 218
-- Name: Computers_CId_seq4; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq4"', 8, false);


--
-- TOC entry 3337 (class 0 OID 0)
-- Dependencies: 219
-- Name: Computers_CId_seq5; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq5"', 8, false);


--
-- TOC entry 3338 (class 0 OID 0)
-- Dependencies: 220
-- Name: Computers_CId_seq6; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq6"', 8, false);


--
-- TOC entry 3339 (class 0 OID 0)
-- Dependencies: 221
-- Name: Computers_CId_seq7; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq7"', 8, false);


--
-- TOC entry 3340 (class 0 OID 0)
-- Dependencies: 222
-- Name: Computers_CId_seq8; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq8"', 8, false);


--
-- TOC entry 3341 (class 0 OID 0)
-- Dependencies: 223
-- Name: Computers_CId_seq9; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Computers_CId_seq9"', 8, false);


--
-- TOC entry 3342 (class 0 OID 0)
-- Dependencies: 302
-- Name: NLJournalDetails_NlJrnlNo_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."NLJournalDetails_NlJrnlNo_seq"', 28, true);


--
-- TOC entry 3343 (class 0 OID 0)
-- Dependencies: 299
-- Name: NlJournalHeader_NlJrnlNo_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."NlJournalHeader_NlJrnlNo_seq"', 21, true);


--
-- TOC entry 3344 (class 0 OID 0)
-- Dependencies: 304
-- Name: PLAnalysisCodes_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."PLAnalysisCodes_id_seq"', 1, true);


--
-- TOC entry 3345 (class 0 OID 0)
-- Dependencies: 306
-- Name: SLAnalysisCodes_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."SLAnalysisCodes_id_seq"', 4, true);


--
-- TOC entry 3346 (class 0 OID 0)
-- Dependencies: 224
-- Name: SLCustomerTypes_SLCTypeID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."SLCustomerTypes_SLCTypeID_seq"', 8, false);


--
-- TOC entry 3347 (class 0 OID 0)
-- Dependencies: 225
-- Name: SLCustomer_SLCustomerSerial_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."SLCustomer_SLCustomerSerial_seq"', 8, false);


--
-- TOC entry 3348 (class 0 OID 0)
-- Dependencies: 226
-- Name: SLInvoiceHeader_SLJrnlNo_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."SLInvoiceHeader_SLJrnlNo_seq"', 8, false);


--
-- TOC entry 3349 (class 0 OID 0)
-- Dependencies: 227
-- Name: SLInvoiceTypes_INVypeID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."SLInvoiceTypes_INVypeID_seq"', 8, false);


--
-- TOC entry 3350 (class 0 OID 0)
-- Dependencies: 228
-- Name: Settings_StId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq"', 8, false);


--
-- TOC entry 3351 (class 0 OID 0)
-- Dependencies: 229
-- Name: Settings_StId_seq1; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq1"', 8, false);


--
-- TOC entry 3352 (class 0 OID 0)
-- Dependencies: 230
-- Name: Settings_StId_seq10; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq10"', 8, false);


--
-- TOC entry 3353 (class 0 OID 0)
-- Dependencies: 231
-- Name: Settings_StId_seq11; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq11"', 8, false);


--
-- TOC entry 3354 (class 0 OID 0)
-- Dependencies: 232
-- Name: Settings_StId_seq12; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq12"', 8, false);


--
-- TOC entry 3355 (class 0 OID 0)
-- Dependencies: 233
-- Name: Settings_StId_seq13; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq13"', 8, false);


--
-- TOC entry 3356 (class 0 OID 0)
-- Dependencies: 234
-- Name: Settings_StId_seq14; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq14"', 8, false);


--
-- TOC entry 3357 (class 0 OID 0)
-- Dependencies: 235
-- Name: Settings_StId_seq15; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq15"', 8, false);


--
-- TOC entry 3358 (class 0 OID 0)
-- Dependencies: 236
-- Name: Settings_StId_seq16; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq16"', 8, false);


--
-- TOC entry 3359 (class 0 OID 0)
-- Dependencies: 237
-- Name: Settings_StId_seq17; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq17"', 8, false);


--
-- TOC entry 3360 (class 0 OID 0)
-- Dependencies: 238
-- Name: Settings_StId_seq18; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq18"', 7, false);


--
-- TOC entry 3361 (class 0 OID 0)
-- Dependencies: 239
-- Name: Settings_StId_seq19; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq19"', 6, false);


--
-- TOC entry 3362 (class 0 OID 0)
-- Dependencies: 240
-- Name: Settings_StId_seq2; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq2"', 8, false);


--
-- TOC entry 3363 (class 0 OID 0)
-- Dependencies: 241
-- Name: Settings_StId_seq20; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq20"', 5, false);


--
-- TOC entry 3364 (class 0 OID 0)
-- Dependencies: 242
-- Name: Settings_StId_seq21; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq21"', 4, false);


--
-- TOC entry 3365 (class 0 OID 0)
-- Dependencies: 243
-- Name: Settings_StId_seq22; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq22"', 3, false);


--
-- TOC entry 3366 (class 0 OID 0)
-- Dependencies: 244
-- Name: Settings_StId_seq23; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq23"', 2, false);


--
-- TOC entry 3367 (class 0 OID 0)
-- Dependencies: 280
-- Name: Settings_StId_seq24; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq24"', 1, false);


--
-- TOC entry 3368 (class 0 OID 0)
-- Dependencies: 245
-- Name: Settings_StId_seq3; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq3"', 8, false);


--
-- TOC entry 3369 (class 0 OID 0)
-- Dependencies: 246
-- Name: Settings_StId_seq4; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq4"', 8, false);


--
-- TOC entry 3370 (class 0 OID 0)
-- Dependencies: 247
-- Name: Settings_StId_seq5; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq5"', 8, false);


--
-- TOC entry 3371 (class 0 OID 0)
-- Dependencies: 248
-- Name: Settings_StId_seq6; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq6"', 8, false);


--
-- TOC entry 3372 (class 0 OID 0)
-- Dependencies: 249
-- Name: Settings_StId_seq7; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq7"', 8, false);


--
-- TOC entry 3373 (class 0 OID 0)
-- Dependencies: 250
-- Name: Settings_StId_seq8; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq8"', 8, false);


--
-- TOC entry 3374 (class 0 OID 0)
-- Dependencies: 251
-- Name: Settings_StId_seq9; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Settings_StId_seq9"', 8, false);


--
-- TOC entry 3039 (class 2606 OID 60375)
-- Name: NLAccountGroup NLAccountGroup_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NLAccountGroup"
    ADD CONSTRAINT "NLAccountGroup_pkey" PRIMARY KEY ("GroupCode");


--
-- TOC entry 3045 (class 2606 OID 60373)
-- Name: NLAccount NLAccount_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NLAccount"
    ADD CONSTRAINT "NLAccount_pkey" PRIMARY KEY ("NlAccCode");


--
-- TOC entry 3049 (class 2606 OID 60450)
-- Name: NLJournalDetails NLJournalDetails_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NLJournalDetails"
    ADD CONSTRAINT "NLJournalDetails_pkey" PRIMARY KEY ("NlJrnlNo");


--
-- TOC entry 3047 (class 2606 OID 60411)
-- Name: NlJournalHeader NlJournalHeader_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NlJournalHeader"
    ADD CONSTRAINT "NlJournalHeader_pkey" PRIMARY KEY ("NlJrnlNo");


--
-- TOC entry 3054 (class 2606 OID 60485)
-- Name: PLAnalysisCodes PLAnalysisCodes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PLAnalysisCodes"
    ADD CONSTRAINT "PLAnalysisCodes_pkey" PRIMARY KEY (id);


--
-- TOC entry 3051 (class 2606 OID 60494)
-- Name: SLAnalysisCodes SLAnalysisCodes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."SLAnalysisCodes"
    ADD CONSTRAINT "SLAnalysisCodes_pkey" PRIMARY KEY (id);


--
-- TOC entry 3041 (class 2606 OID 58065)
-- Name: warehouse_summary warehouse_summary_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.warehouse_summary
    ADD CONSTRAINT warehouse_summary_pkey PRIMARY KEY (ws_id);


--
-- TOC entry 3043 (class 2606 OID 58067)
-- Name: warehouses warehouses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.warehouses
    ADD CONSTRAINT warehouses_pkey PRIMARY KEY (wh_code, wh_ref);


--
-- TOC entry 3052 (class 1259 OID 60467)
-- Name: fki_SLAnalysisCodes_fk_NLAccount; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "fki_SLAnalysisCodes_fk_NLAccount" ON public."SLAnalysisCodes" USING btree ("NLAccCode");


--
-- TOC entry 3055 (class 2606 OID 60462)
-- Name: SLAnalysisCodes SLAnalysisCodes_fk_NLAccount; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."SLAnalysisCodes"
    ADD CONSTRAINT "SLAnalysisCodes_fk_NLAccount" FOREIGN KEY ("NLAccCode") REFERENCES public."NLAccount"("NlAccCode") NOT VALID;


-- Completed on 2021-09-29 08:44:50

--
-- PostgreSQL database dump complete
--

