using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Models.CustomerSalesLedger;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Purchases.Customers;
using pyme_finance_api.Models.Purchases.PurchaseReceipt;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Models.StockInv;
using pyme_finance_api.Models.Vat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.VatService
{


    public interface IVatAnalysis
    {
        public List<VatAnalysis> vatanalysis(DateTime from, DateTime to);
        public List<VatAnalysis> vatanalysisPeriod(string period_from, string period_to);
        public List<VatAnalysis> vatPurchaseanalysis(DateTime from, DateTime to);
        public List<VatAnalysis> vatPurchaseanalysisByPeriod(string period_from, string period_to);
        public List<Vat3BReport> PurchaseAuditTrailPeriod(string transaction_type, string period_from, string period_to);
        public List<Vat3BReport> PurchaseAuditTrail(string transaction_type, DateTime from, DateTime to);
        public List<Vat3BReport> PurchaseTransactionListingPeriod(string transaction_type, string period_from, string period_to, string no);
        public List<Vat3BReport> PurchaseTransactionListing(string transaction_type, DateTime from, DateTime to, string no);
        List<Vat3BReport> SaleAuditTrailByPeriod(string transaction_type, string period_from, string period_to);
        List<Vat3BReport> SalesAuditTrail(string transaction_type, DateTime from, DateTime to);
        List<Vat3BReport> SalesTransactionListing(string transaction_type, DateTime from, DateTime to, string no);
        List<Vat3BReport> SalesTransactionListingPeriod(string transaction_type, string period_from, string period_to, string no);
        List<Vat3BReport> VAT3BReport(DateTime from, DateTime to);
        List<Vat3BReport> VAT3AReport(DateTime from, DateTime to);
        List<Vat3BReport> Vat3BReportByPeriod(string period_from, string period_to);
        List<Vat3BReport> SalesCustomerActivity(int custid, DateTime from, DateTime to);
        List<Vat3BReport> VAT3BbyPeriod(string from_period, string to_period);
        List<CustomerSalesLedger> CustomerActivityForSales(int custId, DateTime from, DateTime to);
        public TaxSetup getVatById(int id);
        public MyResponse editVatItem(TaxSetup taxSetup);
        public MyResponse activateVATItem(int id);
        public MyResponse deactivateVATItem(int id);
    }
    public class VatAnalysisService : IVatAnalysis
    {
        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }
        public VatAnalysisService(string organizationId)
        {
            OrganizationId = organizationId;
        }
        public bool checkIfTaxItemExists(string reference)
        {
            string query = " SELECT * FROM  \"VATs\" WHERE \"VtRef\"  = '" + reference + "' ; ";
            int count = myDbconnection.CheckRowExists(query, OrganizationId);
            if (count >= 1)
            {
                return true;
            }
            return false;
        }
        public List<VatAnalysis> vatanalysis(DateTime from, DateTime to)
        {
            //SUBQUERY FOR BOTH INVOICES AND CREDIT NOTE QUERY
            string query = "select X.vatcode, COALESCE(SUM(X.vattotal), 0) AS vatTotal, COALESCE(sum(x.goodstotal), 0) as goodsTotal from(  " +
                "SELECT b.\"VatCode\" as vatcode, SUM(b.\"VatAmt\") AS vattotal, SUM ( b.\"ItemUnitPrice\" * b.\"ItemQty\"  ) AS goodstotal  FROM \"SLInvoiceHeader\" A LEFT JOIN \"SLInvoiceDetail\" b ON b.\"SLJrnlNo\" = A.\"SLJrnlNo\" left join \"NlJournalHeader\" c on c.\"SlJrnlNo\" = A.\"SLJrnlNo\"  " +
                "WHERE A.\"TransDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"' AND A.\"TransDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL' and A.\"IsReversed\" = 'f'    GROUP BY vatcode    " +
                //"select  a.\"CrnVatPercent\" as vatcode, sum(a.\"CRNVat\" * -1) as vattotal, sum((a.\"CreditNoteAmount\" * -1) - (a.\"CRNVat\" * -1)) as goodstotal from \"SLInvoiceHeader\" a left join \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                //"where b.\"TranFrom\" = 'SL-CRN' and a.\"CRNDate\"::date >= '"+from.ToString("yyyy-MM-dd")+"' and a.\"CRNDate\"::date <= '"+to.ToString("yyyy-MM-dd")+"' and a.\"IsReversed\" = 'f' group by vatcode   " +
                ") as x group by x.vatcode ";
            //UPDATED INVOICE ANALYSIS QUERY REPORT
            string InvoiceAnalysisQuery = "select X.vatcode, COALESCE(SUM(X.vattotal), 0) AS vatTotal, COALESCE(sum(x.goodstotal), 0) as goodsTotal from(   " +
                "SELECT b.\"VatCode\" as vatcode, SUM(b.\"VatAmt\") AS vattotal, SUM(b.\"ItemTotals\" - b.\"VatAmt\") AS goodstotal  FROM \"SLInvoiceHeader\" A LEFT JOIN \"SLInvoiceDetail\" b ON b.\"SLJrnlNo\" = A.\"SLJrnlNo\" left join \"NlJournalHeader\" c on c.\"SlJrnlNo\" = A.\"SLJrnlNo\"     " +
                "WHERE A.\"INVDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"' AND A.\"INVDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL' and A.\"IsReversed\" = 'f' and b.\"VatAmt\" > 0   GROUP BY vatcode  union         " +
                "SELECT b.\"VatCode\" as vatcode, SUM(b.\"VatAmt\") AS vattotal, SUM(b.\"ItemTotals\" - b.\"VatAmt\") AS goodstotal  FROM \"SLInvoiceHeader\" A LEFT JOIN \"SLInvoiceDetail\" b ON b.\"SLJrnlNo\" = A.\"SLJrnlNo\" left join \"NlJournalHeader\" c on c.\"SlJrnlNo\" = A.\"SLJrnlNo\"     " +
                "WHERE A.\"INVDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"' AND A.\"INVDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL' and A.\"IsReversed\" = 'f' and b.\"VatAmt\" <= 0 GROUP BY vatcode) as x group by x.vatcode;   ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(InvoiceAnalysisQuery, cnn).ExecuteReader();
            List<VatAnalysis> vatAnalyses = new List<VatAnalysis>();
            while (sdr0.Read())
            {
                VatAnalysis vatAnalysis = new VatAnalysis();
                vatAnalysis.goods = sdr0["goodsTotal"] != DBNull.Value ? (decimal)sdr0["goodsTotal"] : 0;
                vatAnalysis.vatcode = (string)sdr0["vatcode"];
                vatAnalysis.vat = sdr0["vatTotal"] != DBNull.Value ? (decimal)sdr0["vatTotal"] : 0;
                vatAnalyses.Add(vatAnalysis);
            }
            cnn.Close();
            return vatAnalyses;
        }
        public List<VatAnalysis> vatanalysisPeriod(string period_from, string period_to)
        {
            //SUBQUERY FOR BOTH INVOICES AND CREDIT NOTE QUERY
            string query = "select x.vatcode, COALESCE(sum(x.vattotal), 0) as vattotal, COALESCE(sum(x.goodstotal), 0) as goodtotal FROM   " +
                "(SELECT b.\"VatCode\" as vatcode, SUM(b.\"VatAmt\") AS vattotal, SUM (b.\"ItemUnitPrice\"*b.\"ItemQty\") AS goodstotal FROM \"SLInvoiceHeader\" A LEFT JOIN \"SLInvoiceDetail\" b ON b.\"SLJrnlNo\" = A.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\"  " +
                "WHERE c.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' AND c.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and c.\"TranFrom\" = 'SL' and A.\"IsReversed\" = 'f'  GROUP BY vatcode   " +
                //"select a.\"CrnVatPercent\" as vatcode, sum(a.\"CRNVat\" * -1) as vattotal, sum((a.\"CreditNoteAmount\" * -1) - (a.\"CRNVat\" * -1)) as goodstotal from \"SLInvoiceHeader\" a left join \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"SLJrnlNo\"   " +
                //"WHERE b.\"TranFrom\" = 'SL-CRN' and b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' and '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' and '"+period_to.Split("/")[1]+"' GROUP BY vatcode  " +
                ") as x GROUP BY x.vatcode ";
            string invoiceAnalysis = "select x.vatcode, COALESCE(sum(x.vattotal), 0) as vattotal, COALESCE(sum(x.goodstotal), 0) as goodtotal FROM   (" +
                "SELECT b.\"VatCode\" as vatcode, SUM(b.\"VatAmt\") AS vattotal, SUM (b.\"ItemTotals\" - b.\"VatAmt\") AS goodstotal FROM \"SLInvoiceHeader\" A LEFT JOIN \"SLInvoiceDetail\" b ON b.\"SLJrnlNo\" = A.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\"  " +
                "WHERE c.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' AND c.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and c.\"TranFrom\" = 'SL' and A.\"IsReversed\" = 'f' and b.\"VatAmt\" > 0  GROUP BY vatcode union      " +
                "SELECT b.\"VatCode\" as vatcode, SUM(b.\"VatAmt\") AS vattotal, SUM(b.\"ItemTotals\" - b.\"VatAmt\") AS goodstotal FROM \"SLInvoiceHeader\" A LEFT JOIN \"SLInvoiceDetail\" b ON b.\"SLJrnlNo\" = A.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE c.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' AND c.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and c.\"TranFrom\" = 'SL' and A.\"IsReversed\" = 'f' and b.\"VatAmt\" <= 0  GROUP BY vatcode) as x GROUP BY x.vatcode;  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(invoiceAnalysis, cnn).ExecuteReader();
            List<VatAnalysis> vatAnalyses = new List<VatAnalysis>();
            while (sdr0.Read())
            {
                VatAnalysis vatAnalysis = new VatAnalysis();
                vatAnalysis.goods = sdr0["goodtotal"] != DBNull.Value ? (decimal)sdr0["goodtotal"] : 0;
                vatAnalysis.vatcode = (string)sdr0["vatcode"];
                vatAnalysis.vat = sdr0["vattotal"] != DBNull.Value ? (decimal)sdr0["vattotal"] : 0;
                vatAnalyses.Add(vatAnalysis);
            }
            cnn.Close();
            return vatAnalyses;
        }
        public TaxSetup getVatById(int id)
        {
            string query = " SELECT * FROM  \"VATs\" WHERE \"VtId\"  =" + id + "; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr_vt = new NpgsqlCommand(query, cnn).ExecuteReader();
            TaxSetup tx = new TaxSetup();
            while (sdr_vt.Read())
            {
                tx.VtId = sdr_vt["VtId"] != DBNull.Value ? (int)sdr_vt["VtId"] : 0;
                tx.VtRef = sdr_vt["VtRef"] != DBNull.Value ? (string)sdr_vt["VtRef"] : null;
                tx.VtPerc = sdr_vt["VtPerc"] != DBNull.Value ? (float)sdr_vt["VtPerc"] : 0;
                tx.VtSetDate = sdr_vt["VtSetDate"] != DBNull.Value ? (DateTime)sdr_vt["VtSetDate"] : DateTime.Now;
                tx.VtModifyDate = sdr_vt["VtModifyDate"] != DBNull.Value ? (DateTime)sdr_vt["VtModifyDate"] : DateTime.Now;
                tx.VtActive = sdr_vt["VtActive"] != DBNull.Value ? (bool)sdr_vt["VtActive"] : false;
                tx.VtBranch = sdr_vt["VtBranch"] != DBNull.Value ? (int)sdr_vt["VtBranch"] : 0;
            }
            cnn.Close();
            return tx;
        }
        public MyResponse editVatItem(TaxSetup taxSetup)
        {
            MyResponse response = new MyResponse();
            string updtQ = "UPDATE \"VATs\" SET \"VtRef\" = '" + taxSetup.VtRef + "',\"VtPerc\"='" + taxSetup.VtPerc + "'  WHERE \"VtId\" = '" + taxSetup.VtId + "' ;";
            if (checkIfTaxItemExists(taxSetup.VtRef))
            {
                response.Httpcode = 400;
                response.Message = "Reference already exists.";
                return response;
            }
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "VAT updated successfully";
            }
            return response;
        }
        public List<Vat3BReport> VAT3BReport(DateTime from, DateTime to)
        {
            //INVOICE QUERY
            string invoicequery = "select a.\"TotalAmount\",a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", a.\"origin_status\", a.\"post_number\" ,concat('INV',a.\"SLJrnlNo\")AS inv,SUM(d.\"VatAmt\") AS vatamount, a.invoicenumber, " +
                "case  when b.\"CustCompany\" = '' then b.\"CustFirstName\" when  b.\"CustCompany\" NOTNULL then b.\"CustCompany\" else 'not running'end as custname, c.\"TranFrom\", concat(c.\"TranPeriod\",'/',c.\"TranYear\") as period, a.\"StatementDescription\"   " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" b on b.\"CustCode\" = a.\"CustCode\" LEFT JOIN \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\"     " +
                "WHERE a.\"INVDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL' and a.\"IsReversed\" = 'f'     " +
                "GROUP BY a.\"TotalAmount\", a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", inv,custname, a.\"origin_status\", a.\"post_number\", c.\"TranFrom\", a.invoicenumber, c.\"TranPeriod\", c.\"TranYear\", a.\"StatementDescription\"; ";
            string invoice3bReport = "select sum(d.\"ItemTotals\") as TotalAmount,a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", a.\"origin_status\", a.\"post_number\" ,concat('INV',a.\"SLJrnlNo\")AS inv,SUM(d.\"VatAmt\") AS vatamount, a.invoicenumber,     " +
                "case  when b.\"CustCompany\" = '' then b.\"CustFirstName\" when  b.\"CustCompany\" NOTNULL then b.\"CustCompany\" else 'not running'end as custname, c.\"TranFrom\", \r\nconcat(c.\"TranPeriod\",'/',c.\"TranYear\") as period, a.\"StatementDescription\"      " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" b on b.\"CustCode\" = a.\"CustCode\" LEFT JOIN \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\" \r\nLEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\"       " +
                "WHERE a.\"INVDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL' and a.\"IsReversed\" = 'f' and d.\"VatAmt\" > 0      " +
                "GROUP BY a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", inv,custname, a.\"origin_status\", a.\"post_number\", c.\"TranFrom\", a.invoicenumber, c.\"TranPeriod\", c.\"TranYear\", a.\"StatementDescription\";  ";
            string invoice3bReportLess = "select sum(d.\"ItemTotals\") as TotalAmount, a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", a.\"origin_status\", a.\"post_number\" ,concat('INV',a.\"SLJrnlNo\")AS inv,SUM(d.\"VatAmt\") AS vatamount, a.invoicenumber,    " +
                "case  when b.\"CustCompany\" = '' then b.\"CustFirstName\" when  b.\"CustCompany\" NOTNULL then b.\"CustCompany\" else 'not running'end as custname, c.\"TranFrom\", concat(c.\"TranPeriod\",'/',c.\"TranYear\") as period, a.\"StatementDescription\"      " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" b on b.\"CustCode\" = a.\"CustCode\" LEFT JOIN \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\"       " +
                "WHERE a.\"INVDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL' and a.\"IsReversed\" = 'f' and d.\"VatAmt\" <= 0      " +
                "GROUP BY a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", inv,custname, a.\"origin_status\", a.\"post_number\", c.\"TranFrom\", a.invoicenumber, c.\"TranPeriod\", c.\"TranYear\", a.\"StatementDescription\";   ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(invoice3bReport, cnn).ExecuteReader();
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            while (sdr0.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = sdr0["TotalAmount"] != DBNull.Value ? (decimal)sdr0["TotalAmount"] : 0;
                vat3b.VATpin = (string)sdr0["VATpin"];
                vat3b.custname = (string)sdr0["custname"];
                vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                vat3b.origin_status = sdr0["origin_status"] != DBNull.Value ? (string)sdr0["origin_status"] : "";
                vat3b.Invoice_no = sdr0["invoicenumber"] != DBNull.Value ? (int)sdr0["invoicenumber"] : 0;
                vat3b.TransDate = (DateTime)sdr0["INVDate"];
                vat3b.period = sdr0["period"] != DBNull.Value ? (string)sdr0["period"] : "";
                vat3b.Description = sdr0["StatementDescription"] != DBNull.Value ? (string)sdr0["StatementDescription"] : "";
                vat3b.TransFrom = (string)sdr0["TranFrom"];
                if(vat3b.origin_status == "Posted")
                {
                    vat3b.inv = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : (string)sdr0["inv"];
                }else if(vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                {
                    vat3b.inv = sdr0["inv"] != DBNull.Value ? (string)sdr0["inv"] : "";
                }
                else
                {
                    vat3b.inv = vat3b.Invoice_no.ToString();
                }
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoice3bReportLess, cnn).ExecuteReader();
            List<Vat3BReport> AnalysesReport = new List<Vat3BReport>();
            while (reader.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = reader["TotalAmount"] != DBNull.Value ? (decimal)reader["TotalAmount"] : 0;
                vat3b.VATpin = (string)reader["VATpin"];
                vat3b.custname = (string)reader["custname"];
                vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                vat3b.origin_status = reader["origin_status"] != DBNull.Value ? (string)reader["origin_status"] : "";
                vat3b.Invoice_no = reader["invoicenumber"] != DBNull.Value ? (int)reader["invoicenumber"] : 0;
                vat3b.TransDate = (DateTime)reader["INVDate"];
                vat3b.period = reader["period"] != DBNull.Value ? (string)reader["period"] : "";
                vat3b.Description = reader["StatementDescription"] != DBNull.Value ? (string)reader["StatementDescription"] : "";
                vat3b.TransFrom = (string)reader["TranFrom"];
                if (vat3b.origin_status == "Posted")
                {
                    vat3b.inv = reader["post_number"] != DBNull.Value ? (string)reader["post_number"] : (string)reader["inv"];
                }
                else if (vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                {
                    vat3b.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                }
                else
                {
                    vat3b.inv = vat3b.Invoice_no.ToString();
                }
                AnalysesReport.Add(vat3b);
            }
            cnn.Close();
            vatAnalyses.AddRange(AnalysesReport);
            vatAnalyses.Sort((x, y) => x.TransDate.CompareTo(y.TransDate));
            return vatAnalyses;
        }
        public List<Vat3BReport> VAT3BbyPeriod(string from_period, string to_period)
        {
            //INVOICE QUERY
            string invoiceQuery = "select a.\"TotalAmount\", a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", a.\"origin_status\", a.\"post_number\",concat('INV',a.\"SLJrnlNo\")AS inv, SUM(d.\"VatAmt\") AS vatamount, a.\"StatementDescription\",    " +
                "case when b.\"CustCompany\" = '' then b.\"CustFirstName\"  when  b.\"CustCompany\" NOTNULL then b.\"CustCompany\"  else    'not running'end as custname, e.\"TranFrom\", a.invoicenumber      " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" b on b.\"CustCode\" = a.\"CustCode\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" e on  e.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE e.\"TranPeriod\" BETWEEN  '"+from_period.Split("/")[0]+"' AND  '"+to_period.Split("/")[0]+"' AND e.\"TranYear\" BETWEEN '"+from_period.Split("/")[1]+"' AND '"+to_period.Split("/")[1]+"' and e.\"TranFrom\" = 'SL' and a.\"IsReversed\" = 'f'       " +
                "GROUP BY a.\"TotalAmount\", a.\"INVDate\",b.\"VATpin\",a.\"CustCode\",inv,custname,a.\"origin_status\",a.\"post_number\", e.\"TranFrom\", a.invoicenumber, a.\"StatementDescription\";  ";

            string invoiveReportMore = "select sum(d.\"ItemTotals\") as TotalAmount, a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", a.\"origin_status\", a.\"post_number\",concat('INV',a.\"SLJrnlNo\")AS inv, SUM(d.\"VatAmt\") AS vatamount, a.\"StatementDescription\",    " +
                "case when b.\"CustCompany\" = '' then b.\"CustFirstName\"  when  b.\"CustCompany\" NOTNULL then b.\"CustCompany\"  else    'not running'end as custname, e.\"TranFrom\", a.invoicenumber      " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" b on b.\"CustCode\" = a.\"CustCode\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" e on  e.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE e.\"TranPeriod\" BETWEEN  '"+from_period.Split("/")[0]+"' AND  '"+to_period.Split("/")[0]+"' AND e.\"TranYear\" BETWEEN '"+from_period.Split("/")[1]+"' AND '"+to_period.Split("/")[1]+"' and e.\"TranFrom\" = 'SL' and a.\"IsReversed\" = 'f'  and d.\"VatAmt\" > 0      " +
                "GROUP BY a.\"INVDate\",b.\"VATpin\",a.\"CustCode\",inv,custname,a.\"origin_status\",a.\"post_number\", e.\"TranFrom\", a.invoicenumber, a.\"StatementDescription\";  ";
            string invoiceReportLess = "select sum(d.\"ItemTotals\") as TotalAmount, a.\"INVDate\", b.\"VATpin\", a.\"CustCode\", a.\"origin_status\", a.\"post_number\",concat('INV',a.\"SLJrnlNo\")AS inv, SUM(d.\"VatAmt\") AS vatamount, a.\"StatementDescription\",    " +
                "case when b.\"CustCompany\" = '' then b.\"CustFirstName\"  when  b.\"CustCompany\" NOTNULL then b.\"CustCompany\"  else    'not running'end as custname, e.\"TranFrom\", a.invoicenumber, concat(e.\"TranPeriod\",'/',e.\"TranYear\") as period     " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" b on b.\"CustCode\" = a.\"CustCode\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" e on  e.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE e.\"TranPeriod\" BETWEEN  '"+from_period.Split("/")[0]+"' AND  '"+to_period.Split("/")[0]+"' AND e.\"TranYear\" BETWEEN '"+from_period.Split("/")[1]+"' AND '"+to_period.Split("/")[1]+"' and e.\"TranFrom\" = 'SL' and a.\"IsReversed\" = 'f' and d.\"VatAmt\" <= 0     " +
                "GROUP BY  a.\"INVDate\",b.\"VATpin\",a.\"CustCode\",inv,custname,a.\"origin_status\",a.\"post_number\", e.\"TranFrom\", a.invoicenumber, a.\"StatementDescription\", period;  ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(invoiveReportMore, cnn).ExecuteReader();
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            while (sdr0.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = sdr0["TotalAmount"] != DBNull.Value ? (decimal)sdr0["TotalAmount"] : 0;
                vat3b.VATpin = (string)sdr0["VATpin"];
                vat3b.custname = (string)sdr0["custname"];
                vat3b.TransFrom = (string)sdr0["TranFrom"];
                //vat3b.inv = (string)sdr0["inv"];
                vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                vat3b.origin_status = (string)sdr0["origin_status"];
                //vat3b.invoicenumber = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                vat3b.TransDate = (DateTime)sdr0["INVDate"];
                vat3b.Description = sdr0["StatementDescription"] != DBNull.Value ? (string)sdr0["StatementDescription"] : "";
                vat3b.Invoice_no = sdr0["invoicenumber"] != DBNull.Value ? (int)sdr0["invoicenumber"] : 0;
                if(vat3b.origin_status == "Posted")
                {
                    vat3b.inv = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : (string)sdr0["inv"];
                }else if(vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                {
                    vat3b.inv = sdr0["inv"] != DBNull.Value ? (string)sdr0["inv"] : "";
                }
                else
                {
                    vat3b.inv = "INV" + vat3b.Invoice_no.ToString();
                }
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoiceReportLess, cnn).ExecuteReader();
            List<Vat3BReport> AnalysesReport = new List<Vat3BReport>();
            while (reader.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = reader["TotalAmount"] != DBNull.Value ? (decimal)reader["TotalAmount"] : 0;
                vat3b.VATpin = (string)reader["VATpin"];
                vat3b.custname = (string)reader["custname"];
                vat3b.TransFrom = (string)reader["TranFrom"];
                //vat3b.inv = (string)sdr0["inv"];
                vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                vat3b.origin_status = (string)reader["origin_status"];
                //vat3b.invoicenumber = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                vat3b.TransDate = (DateTime)reader["INVDate"];
                vat3b.Description = reader["StatementDescription"] != DBNull.Value ? (string)reader["StatementDescription"] : "";
                vat3b.Invoice_no = reader["invoicenumber"] != DBNull.Value ? (int)reader["invoicenumber"] : 0;
                if (vat3b.origin_status == "Posted")
                {
                    vat3b.inv = reader["post_number"] != DBNull.Value ? (string)reader["post_number"] : (string)reader["inv"];
                }
                else if (vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                {
                    vat3b.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                }
                else
                {
                    vat3b.inv = "INV" + vat3b.Invoice_no.ToString();
                }
                AnalysesReport.Add(vat3b);
            }
            cnn.Close();
            vatAnalyses.AddRange(AnalysesReport);
            vatAnalyses.Sort((x, y) => x.TransDate.CompareTo(y.TransDate));
            return vatAnalyses;
        }
        public List<CustomerSalesLedger> CustomerActivityForSales(int custId, DateTime from, DateTime to)
        {
            string invoiceQuery = "SELECT a.\"Dr\"::numeric, a.\"Cr\"::numeric, c.\"INVDate\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, c.origin_status, c.post_number, c.entry_number, c.invoicenumber, b.\"TranFrom\", b.\"NlJrnlNo\", c.\"SLJrnlNo\",  c.\"StatementDescription\", c.\"TotalBalance\"    " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = b.\"SlJrnlNo\"   LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = a.\"NlAccCode\"    " +
                "WHERE c.\"CustId\" = '"+custId+"' AND d.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL' and c.\"INVDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"';   ";

            string paymentQuery = "select a.\"Dr\", a.\"Cr\", b.\"TranFrom\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, d.\"pyDate\", d.\"pyChequeNumber\", b.\"NlJrnlNo\",  d.\"pyAdditionalDetails\", d.\"allocation_remainder\"  " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" left JOIN \"NLAccount\" c on c.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"SLReceipts\" d on d.\"pyID\" = b.\"SlJrnlNo\"   " +
                "WHERE d.cust_id = '"+custId+"' and c.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-PY' and d.\"pyDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'; ";

            string crnQuery = "SELECT a.\"Dr\"::numeric, a.\"Cr\"::numeric, c.\"INVDate\", c.\"CRNDate\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, c.origin_status, c.post_number,    c.entry_number, c.invoicenumber, b.\"TranFrom\", b.\"NlJrnlNo\", c.\"SLJrnlNo\",  c.\"CRNReason\"   " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = b.\"SlJrnlNo\"   LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = a.\"NlAccCode\"   " +
                "WHERE c.\"CustId\" = '"+custId+"' AND d.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-CRN' and c.\"CRNDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"';   ";

            string invoiceReversalQuery = "SELECT a.\"Dr\"::numeric, a.\"Cr\"::numeric, c.\"INVDate\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, c.origin_status, c.post_number, c.entry_number, c.invoicenumber, b.\"TranFrom\", b.\"NlJrnlNo\", c.\"SLJrnlNo\", c.\"StatementDescription\"    " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = b.\"SlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = a.\"NlAccCode\"     " +
                "WHERE c.\"CustId\" = '"+custId+"' AND d.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-REVERSAL' and c.\"INVDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"';    ";

            string paymentReversalQuery = "select a.\"Dr\", a.\"Cr\", b.\"TranFrom\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, d.\"pyDate\", d.\"pyChequeNumber\", b.\"NlJrnlNo\", d.\"pyAdditionalDetails\"   " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" left JOIN \"NLAccount\" c on c.\"NlAccCode\" = a.\"NlAccCode\"   LEFT JOIN \"SLReceipts\" d on d.\"pyID\" = b.\"SlJrnlNo\"    " +
                "WHERE d.cust_id = '"+custId+"' and c.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-PY-REVERSAL' and d.\"isReversed\" = TRUE and d.\"pyDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"';   ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<CustomerSalesLedger> customerSalesLedgers = new List<CustomerSalesLedger>();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoiceQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                CustomerSalesLedger salesLedger = new CustomerSalesLedger();
                salesLedger.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                salesLedger.Cr = reader["Cr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                salesLedger.JournalId = reader["NlJrnlNo"] != DBNull.Value ? (int)reader["NlJrnlNo"] : 0;
                salesLedger.DocumentRef = reader["invoicenumber"] != DBNull.Value ? (int)reader["invoicenumber"] : 0;
                salesLedger.TransactionType = (string)reader["TranFrom"];
                salesLedger.SLJrnlNo = (int)reader["SLJrnlNo"];
                salesLedger.TransactionDate = (DateTime)reader["INVDate"];
                salesLedger.origin_status = (string)reader["origin_status"];
                salesLedger.invoicenumber = reader["post_number"] != DBNull.Value ? (string)reader["post_number"] : "";
                salesLedger.Description = reader["StatementDescription"] != DBNull.Value ? (string)reader["StatementDescription"] : "";
                salesLedger.Balance = reader["TotalBalance"] != DBNull.Value ? (decimal)reader["TotalBalance"] : 0;
                customerSalesLedgers.Add(salesLedger);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand(paymentQuery, cnn).ExecuteReader();
            while (reader1.Read())
            {
                CustomerSalesLedger cl = new CustomerSalesLedger();
                cl.Dr = reader1["Dr"] != DBNull.Value ? (decimal)reader1["DR"] : 0;
                cl.Cr = reader1["Cr"] != DBNull.Value ? (decimal)reader1["CR"] : 0;
                cl.DocRef = reader1["pyChequeNumber"] != DBNull.Value ? (string)reader1["pyChequeNumber"] : "";
                cl.TransactionType = reader1["TranFrom"] != DBNull.Value ? (string)reader1["TranFrom"] : "";
                cl.TransactionDate = (DateTime)reader1["pyDate"];
                cl.JournalId = (int)reader1["NlJrnlNo"];
                cl.Description = reader1["pyAdditionalDetails"] != DBNull.Value ? (string)reader1["pyAdditionalDetails"] : "";
                cl.Balance = reader1["allocation_remainder"] != DBNull.Value ? (decimal)reader1["allocation_remainder"] : 0;
                customerSalesLedgers.Add(cl);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader2 = new NpgsqlCommand(crnQuery, cnn).ExecuteReader();
            while (reader2.Read())
            {
                CustomerSalesLedger salesLedger = new CustomerSalesLedger();
                salesLedger.Dr = reader2["Dr"] != DBNull.Value ? (decimal)reader2["Dr"] : 0;
                salesLedger.Cr = reader2["Cr"] != DBNull.Value ? (decimal)reader2["Cr"] : 0;
                salesLedger.JournalId = reader2["NlJrnlNo"] != DBNull.Value ? (int)reader2["NlJrnlNo"] : 0;
                salesLedger.DocumentRef = reader2["invoicenumber"] != DBNull.Value ? (int)reader2["invoicenumber"] : 0;
                salesLedger.TransactionType = (string)reader2["TranFrom"];
                salesLedger.SLJrnlNo = (int)reader2["SLJrnlNo"];
                salesLedger.TransactionDate = (DateTime)reader2["CRNDate"];
                salesLedger.origin_status = (string)reader2["origin_status"];
                salesLedger.invoicenumber = reader2["post_number"] != DBNull.Value ? (string)reader2["post_number"] : "";
                salesLedger.Description = reader2["CRNReason"] != DBNull.Value ? (string)reader2["CRNReason"] : "";
                customerSalesLedgers.Add(salesLedger);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader3 = new NpgsqlCommand(invoiceReversalQuery, cnn).ExecuteReader();
            while (reader3.Read())
            {
                CustomerSalesLedger salesLedger = new CustomerSalesLedger();
                salesLedger.Dr = reader3["Dr"] != DBNull.Value ? (decimal)reader3["Dr"] : 0;
                salesLedger.Cr = reader3["Cr"] != DBNull.Value ? (decimal)reader3["Cr"] : 0;
                salesLedger.JournalId = reader3["NlJrnlNo"] != DBNull.Value ? (int)reader3["NlJrnlNo"] : 0;
                salesLedger.DocumentRef = reader3["invoicenumber"] != DBNull.Value ? (int)reader3["invoicenumber"] : 0;
                salesLedger.TransactionType = (string)reader3["TranFrom"];
                salesLedger.SLJrnlNo = (int)reader3["SLJrnlNo"];
                salesLedger.TransactionDate = (DateTime)reader3["INVDate"];
                salesLedger.origin_status = (string)reader3["origin_status"];
                salesLedger.invoicenumber = reader3["post_number"] != DBNull.Value ? (string)reader3["post_number"] : "";
                salesLedger.Description = reader3["StatementDescription"] != DBNull.Value ? (string)reader3["StatementDescription"] : "";
                customerSalesLedgers.Add(salesLedger);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader4 = new NpgsqlCommand(paymentReversalQuery, cnn).ExecuteReader();
            while (reader4.Read())
            {
                CustomerSalesLedger cl = new CustomerSalesLedger();
                cl.Dr = reader4["Dr"] != DBNull.Value ? (decimal)reader4["DR"] : 0;
                cl.Cr = reader4["Cr"] != DBNull.Value ? (decimal)reader4["CR"] : 0;
                cl.DocRef = reader4["pyChequeNumber"] != DBNull.Value ? (string)reader4["pyChequeNumber"] : "";
                cl.TransactionType = reader4["TranFrom"] != DBNull.Value ? (string)reader4["TranFrom"] : "";
                cl.TransactionDate = (DateTime)reader4["pyDate"];
                cl.JournalId = (int)reader4["NlJrnlNo"];
                cl.Description = reader4["pyAdditionalDetails"] != DBNull.Value ? (string)reader4["pyAdditionalDetails"] : "";
                customerSalesLedgers.Add(cl);
            }
            cnn.Close();

            customerSalesLedgers.Sort((x,y) => y.TransactionDate.CompareTo(x.TransactionDate)); 
            return customerSalesLedgers;
        }
        public List<Vat3BReport> SalesCustomerActivity(int custid, DateTime from, DateTime to)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //Invoice Query
            string queryInvoice = "SELECT a.\"Dr\"::numeric, a.\"Cr\"::numeric, c.\"INVDate\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, c.origin_status, c.post_number, c.entry_number, c.invoicenumber, b.\"TranFrom\", b.\"NlJrnlNo\", e.\"CustCompany\",   " +
                " e.\"VATpin\", e.\"PostalAddress\", e.\"CustCode\", e.\"Address\", c.\"StatementDescription\"   " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = b.\"SlJrnlNo\"   " +
                "LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"SLCustomer\" e on e.\"SLCustomerSerial\" = c.\"CustId\"   " +
                "WHERE c.\"CustId\" = '"+custid+"' AND d.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL' and c.\"INVDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"';    ";
            //Payment Query
            string queryPayment = "select a.\"Dr\", a.\"Cr\", b.\"TranFrom\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, d.\"pyDate\", d.\"pyChequeNumber\", d.\"currentCustName\", e.\"VATpin\", e.\"Address\", e.\"PostalAddress\", d.\"pyAdditionalDetails\", e.\"CustCode\"     " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" left JOIN \"NLAccount\" c on c.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"SLReceipts\" d on d.\"pyID\" = b.\"SlJrnlNo\" LEFT JOIN \"SLCustomer\" e on e.\"SLCustomerSerial\" = d.cust_id   " +
                "WHERE d.cust_id = '"+custid+"' and c.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-PY' and d.\"pyDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'; ";
            //Credit Note Query
            string queryCreditNote = "SELECT a.\"Dr\"::numeric, a.\"Cr\"::numeric, c.\"INVDate\", c.\"CRNDate\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, c.origin_status, c.post_number,    " +
                "c.entry_number, c.invoicenumber, b.\"TranFrom\", b.\"NlJrnlNo\", e.\"CustCompany\", e.\"VATpin\", e.\"PostalAddress\", e.\"CustCode\", e.\"Address\", c.\"CRNReason\"   " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = b.\"SlJrnlNo\"   " +
                "LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"SLCustomer\" e on e.\"SLCustomerSerial\" = c.\"CustId\"   " +
                "WHERE c.\"CustId\" = '"+custid+"' AND d.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-CRN' and c.\"CRNDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'; ";
            //Reversal Query
            string queryReversal = "SELECT a.\"Dr\"::numeric, a.\"Cr\"::numeric, c.\"INVDate\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, c.origin_status, c.post_number, c.entry_number, c.invoicenumber, b.\"TranFrom\", b.\"NlJrnlNo\", e.\"CustCompany\",  " +
                " e.\"VATpin\", e.\"PostalAddress\", e.\"CustCode\", e.\"Address\", c.\"StatementDescription\"     " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = b.\"SlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = a.\"NlAccCode\" " +
                "LEFT JOIN \"SLCustomer\" e on e.\"SLCustomerSerial\" = c.\"CustId\"  WHERE c.\"CustId\" = '"+custid+"' AND d.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-REVERSAL' and c.\"INVDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'; ";
            //Payment Reversal Query
            string paymentReversalQuery = "select a.\"Dr\", a.\"Cr\", b.\"TranFrom\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") as period, d.\"pyDate\", d.\"pyChequeNumber\", d.\"currentCustName\", e.\"VATpin\", e.\"Address\", e.\"PostalAddress\", d.\"pyAdditionalDetails\", e.\"CustCode\"      " +
                "from \"NLJournalDetails\" a LEFT JOIN \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.\"JrnlSlNo\" left JOIN \"NLAccount\" c on c.\"NlAccCode\" = a.\"NlAccCode\"   LEFT JOIN \"SLReceipts\" d on d.\"pyID\" = b.\"SlJrnlNo\" LEFT JOIN \"SLCustomer\" e on e.\"SLCustomerSerial\" = d.cust_id    " +
                " WHERE d.cust_id = '"+custid+"' and c.\"NlAccName\"~*'debtors' and b.\"TranFrom\" = 'SL-PY-REVERSAL' and d.\"isReversed\" = TRUE and d.\"pyDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"';  ";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(queryInvoice, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.VATpin = sdr1["VATpin"] != DBNull.Value ? (string)sdr1["VATpin"] : "";
                vat3b.custname = sdr1["CustCompany"] != DBNull.Value ? (string)sdr1["CustCompany"] : "";
                vat3b.period = sdr1["period"] != DBNull.Value ? (string)sdr1["period"] : "";
                vat3b.EN = sdr1["entry_number"] != DBNull.Value ? (int)sdr1["entry_number"] : 0;
                vat3b.Invoice_no = sdr1["invoicenumber"] != DBNull.Value ? (int)sdr1["invoicenumber"] : 0;
                vat3b.PostNumber = sdr1["post_number"] != DBNull.Value ? (string)sdr1["post_number"] : "";
                vat3b.TransDate = (DateTime)sdr1["INVDate"];
                vat3b.CustomerCode = sdr1["CustCode"] != DBNull.Value ? (string)sdr1["CustCode"] : "";
                vat3b.PostAddress = sdr1["PostalAddress"] != DBNull.Value ? (string)sdr1["PostalAddress"] : "";
                vat3b.PhysicalAddress = sdr1["Address"] != DBNull.Value ? (string)sdr1["Address"] : "";
                vat3b.Description = sdr1["StatementDescription"] != DBNull.Value ? (string)sdr1["StatementDescription"] : "";
                vat3b.origin_status = sdr1["origin_status"] != DBNull.Value ? (string)sdr1["origin_status"] : "";
                vat3b.TransFrom = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : "";
                vat3b.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                vat3b.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                //vat3b.Inv_Doc = string.Concat("INV", vat3b.EN);
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand(queryCreditNote, cnn).ExecuteReader();
            while (sdr2.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.VATpin = sdr2["VATpin"] != DBNull.Value ? (string)sdr2["VATpin"] : "";
                vat3b.custname = sdr2["CustCompany"] != DBNull.Value ? (string)sdr2["CustCompany"] : "";
                vat3b.period = sdr2["period"] != DBNull.Value ? (string)sdr2["period"] : "";
                vat3b.EN = sdr2["entry_number"] != DBNull.Value ? (int)sdr2["entry_number"] : 0;
                vat3b.Invoice_no = sdr2["invoicenumber"] != DBNull.Value ? (int)sdr2["invoicenumber"] : 0;
                vat3b.PostNumber = sdr2["post_number"] != DBNull.Value ? (string)sdr2["post_number"] : "";
                vat3b.TransDate = (DateTime)sdr2["CRNDate"];
                vat3b.CustomerCode = sdr2["CustCode"] != DBNull.Value ? (string)sdr2["CustCode"] : "";
                vat3b.PostAddress = sdr2["PostalAddress"] != DBNull.Value ? (string)sdr2["PostalAddress"] : "";
                vat3b.PhysicalAddress = sdr2["Address"] != DBNull.Value ? (string)sdr2["Address"] : "";
                vat3b.Description = sdr2["CRNReason"] != DBNull.Value ? (string)sdr2["CRNReason"] : "";
                vat3b.origin_status = sdr2["origin_status"] != DBNull.Value ? (string)sdr2["origin_status"] : "";
                vat3b.TransFrom = sdr2["TranFrom"] != DBNull.Value ? (string)sdr2["TranFrom"] : "";
                vat3b.Dr = sdr2["Dr"] != DBNull.Value ? (decimal)sdr2["Dr"] : 0;
                vat3b.Cr = sdr2["Cr"] != DBNull.Value ? (decimal)sdr2["Cr"] : 0;
                //vat3b.Inv_Doc = string.Concat("CRN", vat3b.EN);
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand(paymentReversalQuery, cnn).ExecuteReader();
            while (reader1.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.VATpin = reader1["VATpin"] != DBNull.Value ? (string)reader1["VATpin"] : "";
                vat3b.custname = reader1["currentCustName"] != DBNull.Value ? (string)reader1["currentCustName"] : "";
                vat3b.inv = reader1["pyChequeNumber"] != DBNull.Value ? (string)reader1["pyChequeNumber"] : "";
                vat3b.period = reader1["period"] != DBNull.Value ? (string)reader1["period"] : "";
                vat3b.TransDate = (DateTime)reader1["pyDate"];
                vat3b.CustomerCode = reader1["CustCode"] != DBNull.Value ? (string)reader1["CustCode"] : "";
                vat3b.PostAddress = reader1["PostalAddress"] != DBNull.Value ? (string)reader1["PostalAddress"] : "";
                vat3b.PhysicalAddress = reader1["Address"] != DBNull.Value ? (string)reader1["Address"] : "";
                vat3b.Description = reader1["pyAdditionalDetails"] != DBNull.Value ? (string)reader1["pyAdditionalDetails"] : "";
                vat3b.TransFrom = reader1["TranFrom"] != DBNull.Value ? (string)reader1["TranFrom"] : "";
                vat3b.Dr = reader1["Dr"] != DBNull.Value ? (decimal)reader1["Dr"] : 0;
                vat3b.Cr = reader1["Cr"] != DBNull.Value ? (decimal)reader1["Cr"] : 0;
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand(queryPayment, cnn).ExecuteReader();
            while (sdr3.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.VATpin = sdr3["VATpin"] != DBNull.Value ? (string)sdr3["VATpin"] : "";
                vat3b.custname = sdr3["currentCustName"] != DBNull.Value ? (string)sdr3["currentCustName"] : "";
                vat3b.inv = sdr3["pyChequeNumber"] != DBNull.Value ? (string)sdr3["pyChequeNumber"] : "";
                vat3b.period = sdr3["period"] != DBNull.Value ? (string)sdr3["period"] : "";
                vat3b.TransDate = (DateTime)sdr3["pyDate"];
                vat3b.CustomerCode = sdr3["CustCode"] != DBNull.Value ? (string)sdr3["CustCode"] : "";
                vat3b.PostAddress = sdr3["PostalAddress"] != DBNull.Value ? (string)sdr3["PostalAddress"] : "";
                vat3b.PhysicalAddress = sdr3["Address"] != DBNull.Value ? (string)sdr3["Address"] : "";
                vat3b.Description = sdr3["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3["pyAdditionalDetails"] : "";
                vat3b.TransFrom = sdr3["TranFrom"] != DBNull.Value ? (string)sdr3["TranFrom"] : "";
                vat3b.Dr = sdr3["Dr"] != DBNull.Value ? (decimal)sdr3["Dr"] : 0;
                vat3b.Cr = sdr3["Cr"] != DBNull.Value ? (decimal)sdr3["Cr"] : 0;
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(queryReversal, cnn).ExecuteReader();
            while (reader.Read())
            {
                Vat3BReport report = new Vat3BReport();
                report.VATpin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : null;
                report.custname = reader["CustCompany"] != DBNull.Value ? (string)reader["CustCompany"] : null;
                report.period = reader["period"] != DBNull.Value ? (string)reader["period"] : null;
                report.EN = reader["entry_number"] != DBNull.Value ? (int)reader["entry_number"] : 0;
                report.Invoice_no = reader["invoicenumber"] != DBNull.Value ? (int)reader["invoicenumber"] : 0;
                report.PostNumber = reader["post_number"] != DBNull.Value ? (string)reader["post_number"] : null;
                report.TransDate = (DateTime)reader["INVDate"];
                report.CustomerCode = reader["CustCode"] != DBNull.Value ? (string)reader["CustCode"] : null;
                report.PostAddress = reader["PostalAddress"] != DBNull.Value ? (string)reader["PostalAddress"] : null;
                report.PhysicalAddress = reader["Address"] != DBNull.Value ? (string)reader["Address"] : null;
                report.Description = reader["StatementDescription"] != DBNull.Value ? (string)reader["StatementDescription"] : null;
                report.origin_status = reader["origin_status"] != DBNull.Value ? (string)reader["origin_status"] : null;
                report.TransFrom = reader["TranFrom"] != DBNull.Value ? (string)reader["TranFrom"] : null;
                report.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                report.Cr = reader["Cr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                vatAnalyses.Add(report);
            }
            cnn.Close();
            vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
            return vatAnalyses;
        }
        public List<Vat3BReport> SaleAuditTrailByPeriod(string transaction_type, string period_from, string period_to)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //invoice query
            string invoicequery = "select a.\"TotalAmount\" as Totals, sum(b.\"VatAmt\") as vatamount, a.\"TransDate\" as TransDate, e.fp_openingdate, e.fp_closingdate, concat('INV', a.\"SLJrnlNo\") as inv, concat(d.\"TranPeriod\", '/', d.\"TranYear\") as period1, a.\"Period\", a.\"SLJrnlNo\", " +
                " case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname,  c.\"VATNo\" as VATpin" +
                " from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" b on b.\"SLJrnlNo\" = a.\"SLJrnlNo\" INNER JOIN \"NlJournalHeader\" d on d.\"SlJrnlNo\" = a.\"SLJrnlNo\" INNER JOIN financial_periods e on e.fp_ref = a.\"Period\"" +
                " where d.\"TranPeriod\" BETWEEN '" + period_from.Split("/")[0] + "' AND '" + period_to.Split("/")[0] + "' and d.\"TranYear\" BETWEEN '" + period_from.Split("/")[1] + "' AND '" + period_to.Split("/")[1] + "' and d.\"TranFrom\" = 'SL' " +
                " GROUP BY a.\"TotalAmount\", TransDate,e.\"fp_openingdate\",e.\"fp_closingdate\",a.\"Period\",a.\"SLJrnlNo\", inv, custname,  VATpin, d.\"TranPeriod\", d.\"TranYear\"";
            //invoice reversal query
            string ReveresedInvoicequery = "select a.\"TotalAmount\" as Totals, sum(b.\"VatAmt\") as vatamount, a.\"TransDate\" as TransDate, e.fp_openingdate, e.fp_closingdate, concat('INV', a.\"SLJrnlNo\") as inv, concat(d.\"TranPeriod\", '/', d.\"TranYear\") as period1, a.\"Period\", a.\"SLJrnlNo\", " +
               " case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname,  c.\"VATNo\" as VATpin" +
               " from \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" b on b.\"SLJrnlNo\" = a.\"SLJrnlNo\" INNER JOIN \"NlJournalHeader\" d on d.\"SlJrnlNo\" = a.\"SLJrnlNo\" INNER JOIN financial_periods e on e.fp_ref = a.\"Period\"" +
               " where d.\"TranPeriod\" BETWEEN '" + period_from.Split("/")[0] + "' AND '" + period_to.Split("/")[0] + "' and d.\"TranYear\" BETWEEN '" + period_from.Split("/")[1] + "' AND '" + period_to.Split("/")[1] + "' and d.\"TranFrom\" = 'SL-REVERSAL' " +
               " GROUP BY a.\"TotalAmount\", TransDate,e.\"fp_openingdate\",e.\"fp_closingdate\",a.\"Period\",a.\"SLJrnlNo\", inv, custname,  VATpin, d.\"TranPeriod\", d.\"TranYear\"";
            //credit note query            
            string creditquery = "select a.\"CreditNoteAmount\" as Totals, A.\"CRNVat\" as vatamount, A.\"CRNDate\" as TransDate, a.\"SLJrnlNo\", concat(d.\"TranPeriod\",'/',d.\"TranYear\") as period1, a.\"Period\", concat('CRN', a.\"SLJrnlNo\") as inv,   " +
                "case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\" else ''end as custname, c.\"VATNo\" as VATpin     " +
                "from \"SLInvoiceHeader\" a LEFT JOIN \"SLInvoiceDetail\" b on b.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN financial_periods e on e.fp_ref = a.\"Period\" LEFT JOIN \"NlJournalHeader\" d on d.\"SlJrnlNo\" = a.\"SLJrnlNo\"   " +
                "where d.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and d.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and d.\"TranFrom\" = 'SL-CRN'    " +
                "GROUP BY a.\"CreditNoteAmount\", TransDate, a.\"Period\",a.\"SLJrnlNo\", inv, custname,  VATpin, d.\"TranPeriod\", d.\"TranYear\"   "; 
            //payment query
            string paymentquery = "select a.\"pyPaid\" As Totals,a.\"pyProcessDate\" as TransDate,a.\"pyID\" as SLJrnlNo,a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname, c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount  " +
                "from \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\" " +
                "where b.\"TranPeriod\" BETWEEN '" + period_from.Split("/")[0] + "' AND '" + period_to.Split("/")[0] + "' and b.\"TranYear\" BETWEEN '" + period_from.Split("/")[1] + "' AND '" + period_to.Split("/")[1] + "' and b.\"TranFrom\" = 'SL-PY' and a.\"isReversed\" = FALSE " +
                "GROUP BY a.\"pyPaid\",a.\"pyProcessDate\",a.\"pyID\",inv,  VATpin,custname, vatamount, b.\"TranPeriod\", b.\"TranYear\"  ";
            //payment reversal query
            string paymentReversalQuery = "select a.\"pyPaid\" As Totals, a.\"pyDate\", a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname, c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, " +
                " 0.00 as vatamount, a.\"currentCustName\"     " +
                "from \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"    " +
                "where b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and b.\"TranFrom\" = 'SL-PY-REVERSAL' and a.\"isReversed\" = TRUE   " +
                "GROUP BY a.\"pyPaid\",a.\"pyProcessDate\",a.\"pyID\",inv,  VATpin,custname, vatamount, b.\"TranPeriod\", b.\"TranYear\", a.\"currentCustName\", a.\"pyDate\"    ";
            
            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                cnn.Open();
                query = invoicequery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);

                }
                cnn.Close();

                //reversed Invoice
                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(ReveresedInvoicequery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3B = new Vat3BReport();
                    vat3B.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3B.VATpin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : "";
                    vat3B.custname = reader["custname"] != DBNull.Value ? (string)reader["custname"] : "";
                    vat3B.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                    vat3B.period = reader["period1"] != DBNull.Value ? (string)reader["period1"] : "";
                    vat3B.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3B.SLJrnlNo = reader["SLJrnlNo"] != DBNull.Value ? (int)reader["SLJrnlNo"] : 0;
                    vat3B.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3B);
                }
                cnn.Close();
            }
            else if (transaction_type == "CRN")
            {
                cnn.Open();
                query = creditquery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else if (transaction_type == "PYT")
            {
                cnn.Open();
                query = paymentquery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? Convert.ToDecimal(sdr0["Totals"]) : 0;
                    vat3b.VATpin = sdr0["VATpin"] != DBNull.Value ? (string)sdr0["VATpin"] : "";
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                //payment reversals
                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(paymentReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? Convert.ToDecimal(reader["Totals"]) : 0) *-1;
                    vat3b.VATpin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : "";
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)reader["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.SLJrnlNo = (int)sdr1["SLJrnlNo"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr2["Totals"] != DBNull.Value ? (decimal)sdr2["Totals"] : 0;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = (string)sdr2["inv"];
                    vat3b.period = (string)sdr2["period1"];
                    vat3b.SLJrnlNo = (int)sdr2["SLJrnlNo"];
                    vat3b.VatAmount = sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                // vatAnalyses.OrderByDescending(x => x.TransDate);
                //return vatAnalyses;
            }
            vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
            return vatAnalyses;
        }
        public List<Vat3BReport> SalesAuditTrail(string transaction_type, DateTime from, DateTime to)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //invoice query
            string invoicequery = "select a.\"TotalAmount\" As Totals,sum(d.\"VatAmt\") AS vatamount, a.\"TransDate\" as TransDate,e.\"fp_openingdate\",e.\"fp_closingdate\",a.\"SLJrnlNo\", concat(12 - DATE_PART('month', AGE(e.\"fp_closingdate\",a.\"TransDate\" )),'/', date_part('year', CURRENT_DATE)) AS period1,  " +
                " a.\"Period\",e.\"fp_openingdate\", concat('INV',a.\"SLJrnlNo\")AS inv,e.\"fp_closingdate\",case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname, c.\"VATNo\" as VATpin   " +
                "FROM \"SLInvoiceHeader\" a  LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"SlJrnlNo\" = a.\"SLJrnlNo\"   " +
                "WHERE a.\"TransDate\" BETWEEN  '" + from.ToString("yyyy-MM-dd") + "' AND '" + to.ToString("yyyy-MM-dd") + "'    AND a.\"INVTypeRef\" = 'INV' AND f.\"TranFrom\" = 'SL'   " +
                "GROUP BY a.\"TotalAmount\", TransDate,e.\"fp_openingdate\",e.\"fp_closingdate\",a.\"Period\",a.\"SLJrnlNo\",e.\"fp_openingdate\", inv,e.\"fp_closingdate\", custname,  VATpin   ";
            //invoice reversal query
            string ReveresedInvoicequery = "select a.\"TotalAmount\" As Totals,sum(d.\"VatAmt\") AS vatamount, a.\"TransDate\" as TransDate,e.\"fp_openingdate\",e.\"fp_closingdate\",a.\"SLJrnlNo\", concat(12 - DATE_PART('month', AGE(e.\"fp_closingdate\",a.\"TransDate\" )),'/', date_part('year', CURRENT_DATE)) AS period1, a.\"Period\",e.\"fp_openingdate\"," +
                " concat('INV',a.\"SLJrnlNo\")AS inv,e.\"fp_closingdate\",case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname, c.\"VATNo\" as VATpin   " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"SlJrnlNo\" = a.\"SLJrnlNo\" " +
                "WHERE a.\"TransDate\" BETWEEN  '" + from.ToString("yyyy-MM-dd") + "' AND '" + to.ToString("yyyy-MM-dd") + "'    AND a.\"INVTypeRef\" = 'INV' AND f.\"TranFrom\" = 'SL-REVERSAL'  " +
                "GROUP BY a.\"TotalAmount\", TransDate,e.\"fp_openingdate\",e.\"fp_closingdate\",a.\"Period\",a.\"SLJrnlNo\",e.\"fp_openingdate\", inv,e.\"fp_closingdate\", custname,  VATpin";
            //credit note query
            string creditquery = " select a.\"CreditNoteAmount\" As Totals, a.\"CRNVat\" AS vatamount, a.\"CRNDate\" as TransDate, a.entry_number, a.post_number, a.invoicenumber, concat(f.\"TranPeriod\",'/',f.\"TranYear\") AS period, concat('CRN', a.\"entry_number\")AS inv, c.\"VATNo\" as VATpin ,  " +
                "case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname   " +
                "FROM \"SLInvoiceHeader\" a  LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"'   AND a.\"INVTypeRef\" = 'CRN' and f.\"TranFrom\" = 'SL-CRN'     " +
                "GROUP BY a.\"CreditNoteAmount\", a.\"CRNVat\", TransDate, a.\"Period\", inv, custname,  VATpin, a.\"CRNDate\", f.\"TranPeriod\", f.\"TranYear\", a.entry_number, a.post_number, a.invoicenumber ";
            //payment query
            string paymentquery = "select a.\"pyPaid\" As Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname, c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount, a.journal_id   " +
                "FROM \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"    " +
                "WHERE a.\"pyProcessDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'SL-PY'    " +
                "GROUP BY a.\"pyPaid\", a.\"pyProcessDate\", a.\"pyID\", inv, VATpin, b.\"TranPeriod\", b.\"TranYear\", vatamount, a.\"pyChequeNumber\", a.\"currentCustName\", a.journal_id ";
            //payment reversal query
            string paymentReversalQuery = "select a.\"pyPaid\" As Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname, c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount, a.journal_id   " +
                "FROM \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"      " +
                "WHERE a.\"pyProcessDate\" BETWEEN  '2023-09-01' AND '2023-10-31' and b.\"TranFrom\" = 'SL-PY-REVERSAL' and a.\"isReversed\" = TRUE      " +
                "GROUP BY a.\"pyPaid\", a.\"pyProcessDate\", a.\"pyID\", VATpin, b.\"TranPeriod\", b.\"TranYear\", vatamount, a.\"pyChequeNumber\", a.\"currentCustName\", a.journal_id ";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                cnn.Open();
                query = invoicequery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);

                }
                cnn.Close();

                //reversed Invoices Here
                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(ReveresedInvoicequery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3B = new Vat3BReport();
                    vat3B.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3B.VATpin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : "";
                    vat3B.custname = reader["custname"] != DBNull.Value ? (string)reader["custname"] : "";
                    vat3B.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                    vat3B.period = reader["period1"] != DBNull.Value ? (string)reader["period1"] : "";
                    vat3B.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) *-1;
                    vat3B.SLJrnlNo = reader["SLJrnlNo"] != DBNull.Value ? (int)reader["SLJrnlNo"] : 0;
                    vat3B.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3B);
                }
                cnn.Close();
            }
            else if (transaction_type == "CRN")
            {
                cnn.Open();
                query = creditquery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    //vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else if (transaction_type == "PYT")
            {
                cnn.Open();
                query = paymentquery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? Convert.ToDecimal(sdr0["Totals"]) : 0;
                    vat3b.VATpin = sdr0["VATpin"] != DBNull.Value ? (string)sdr0["VATpin"] : "";
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = sdr0["inv"] != DBNull.Value ? (string)sdr0["inv"] : "";
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(paymentReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? Convert.ToDecimal(reader["Totals"]) : 0) * -1;
                    vat3b.VATpin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : "";
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : null;
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)reader["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.SLJrnlNo = (int)sdr1["SLJrnlNo"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr2["Totals"] != DBNull.Value ? (decimal)sdr2["Totals"] : 0;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = (string)sdr2["inv"];
                    vat3b.period = (string)sdr2["period"];
                    //vat3b.SLJrnlNo = (int)sdr2["SLJrnlNo"];
                    vat3b.VatAmount = sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                // vatAnalyses.OrderByDescending(x => x.TransDate);
                //return vatAnalyses;
            }
            vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
            return vatAnalyses;
        }
        public List<DeliveryNoteReport> DeliveryNoteAuditByDate(string transaction_type, DateTime from, DateTime to)
        {
            List<DeliveryNoteReport> deliveryNotes = new List<DeliveryNoteReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //DELIVERY NOTE QUERY
            string deliveryNoteQuery = "select a.\"Id\", a.\"CreatedOn\", a.\"Lpo\", b.\"StkDesc\", b.\"ProdQty\", c.\"PostalAddress\", c.\"VATpin\",  case when c.\"CustCompany\" = '' then concat(c.\"CustFirstName\", ' ', c.\"CustLastName\") else  c.\"CustCompany\" end as AccountHolder   " +
                "from \"DeliveryNotes\" a left join \"DeliveryNoteDetails\" b on b.\"DeliveryNote_ref\" = a.\"Id\" left join \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustomerId\"   " +
                "where a.\"CreatedOn\" between  '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'   "; 
            if(transaction_type == "DNOTE")
            {
                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(deliveryNoteQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    DeliveryNoteReport report = new DeliveryNoteReport();
                    report.DNoteDate = (DateTime)reader["CreatedOn"];
                    report.LpoNumber = reader["Lpo"] != DBNull.Value ? (string)reader["Lpo"] : "";
                    report.StockDescription = reader["StkDesc"] != DBNull.Value ? (string)reader["StkDesc"] : "";
                    report.ProductQty = reader["ProdQty"] != DBNull.Value ? (int)reader["ProdQty"] : 0;
                    report.CustomerName = reader["AccountHolder"] != DBNull.Value ? (string)reader["AccountHolder"] : "";
                    report.PostalAddress = reader["PostalAddress"] != DBNull.Value ? (string)reader["PostalAddress"] : "";
                    report.VatPin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : "";
                    deliveryNotes.Add(report);
                }
                cnn.Close();
            }
            else
            {
                return null;
            }
            
            return deliveryNotes;
        }
        public List<Vat3BReport> SalesTransactionListing(string transaction_type, DateTime from, DateTime to, string no)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //invoice query
            string invoicequery = "select a.\"TotalAmount\" As Totals,sum(d.\"VatAmt\") AS vatamount,a.\"INVDate\" as TransDate, a.\"SLJrnlNo\", a.invoicenumber, a.post_number, concat(f.\"TranPeriod\",'/',f.\"TranYear\") AS period1, c.\"VATNo\" as VATpin, a.origin_status,concat('INV',a.\"SLJrnlNo\") AS inv,  " +
                "case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname   " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE a.\"INVDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"'    AND a.\"INVTypeRef\" = 'INV' and f.\"TranFrom\" = 'SL'   " +
                "GROUP BY a.\"TotalAmount\", TransDate, a.\"Period\", a.\"SLJrnlNo\", inv, custname,  VATpin, f.\"TranPeriod\" ,f.\"TranYear\" ";
            //invoice reversal query
            string invoiceReversalQuery = "select a.\"TotalAmount\" As Totals,sum(d.\"VatAmt\") AS vatamount,a.\"TransDate\" as TransDate, a.\"SLJrnlNo\", a.invoicenumber, a.post_number, concat(f.\"TranPeriod\",'/',f.\"TranYear\") AS period1, c.\"VATNo\" as VATpin, a.origin_status,    " +
                "concat('INV',a.\"SLJrnlNo\") AS inv,  case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname    " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"SlJrnlNo\" = a.\"SLJrnlNo\"    " +
                "WHERE a.\"TransDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and f.\"TranFrom\" = 'SL-REVERSAL' GROUP BY a.\"TotalAmount\", TransDate, a.\"Period\", a.\"SLJrnlNo\", inv, custname,  VATpin, f.\"TranPeriod\" ,f.\"TranYear\"  ";
            //credit note query           
            string creditquery = "select a.\"CreditNoteAmount\" As Totals, a.\"CRNVat\" AS vatamount, a.\"CRNDate\" as TransDate, a.\"SLJrnlNo\",   concat(f.\"TranPeriod\",'/',f.\"TranYear\") AS period1, a.\"Period\", concat('CRN',a.\"SLJrnlNo\")AS inv, " +
                "case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname,  c.\"VATNo\" as VATpin    " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"SlJrnlNo\" = a.\"SLJrnlNo\"  " +
                "WHERE a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' AND a.\"INVTypeRef\" = 'CRN' and f.\"TranFrom\" = 'SL-CRN'    " +
                "GROUP BY a.\"TotalAmount\", TransDate,a.\"Period\",a.\"SLJrnlNo\", inv, custname,  VATpin, f.\"TranPeriod\", f.\"TranYear\", vatamount   ";
            //payment query
            string paymentquery = "select a.\"pyPaid\" As Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname,  c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount " +
                "FROM \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"  " +
                "WHERE a.\"pyProcessDate\" BETWEEN  '" + from + "' AND '" + to + "' AND b.\"TranFrom\" = 'SL-PY'     " +
                "GROUP BY a.\"pyPaid\", a.\"pyProcessDate\", a.\"pyID\", inv,  VATpin, custname, b.\"TranPeriod\", b.\"TranYear\", vatamount ";
            //payment reversal query
            string paymentReversalQuery = "select a.\"pyPaid\" As Totals, a.\"pyDate\" as TransDate, a.\"pyID\" as SLJrnlNo,  a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname,  c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount   " +
                "FROM \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"     " +
                "WHERE a.\"pyDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' AND b.\"TranFrom\" = 'SL-PY-REVERSAL' and a.\"isReversed\" = TRUE   GROUP BY a.\"pyPaid\", a.\"pyDate\", a.\"pyID\", inv,  VATpin, custname, b.\"TranPeriod\", b.\"TranYear\", vatamount  ";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                query = invoicequery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.origin_status = sdr0["origin_status"] != DBNull.Value ? (string)sdr0["origin_status"] : "";

                    //vat3b.PostNumber = (string)sdr0["post_number"];
                    vat3b.Invoice_no = sdr0["invoicenumber"] != DBNull.Value ? (int)sdr0["invoicenumber"] : 0;

                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];

                    if(vat3b.origin_status == "Posted")
                    {
                        vat3b.inv = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                    }else if(vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                    {
                        vat3b.inv = sdr0["inv"] != DBNull.Value ? (string)sdr0["inv"] : "";
                    }
                    else
                    {
                        vat3b.inv = vat3b.Invoice_no.ToString();
                    }
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoiceReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) *-1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.origin_status = reader["origin_status"] != DBNull.Value ? (string)reader["origin_status"] : "";

                    //vat3b.PostNumber = (string)sdr0["post_number"];
                    vat3b.Invoice_no = reader["invoicenumber"] != DBNull.Value ? (int)reader["invoicenumber"] : 0;

                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.SLJrnlNo = (int)reader["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];

                    if (vat3b.origin_status == "Posted")
                    {
                        vat3b.inv = reader["post_number"] != DBNull.Value ? (string)reader["post_number"] : "";
                    }
                    else if (vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                    {
                        vat3b.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                    }
                    else
                    {
                        vat3b.inv = vat3b.Invoice_no.ToString();
                    }
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "CRN")
            {
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "PYT")
            {
                query = paymentquery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? Convert.ToDecimal(sdr0["Totals"]) : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(paymentReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? Convert.ToDecimal(reader["Totals"]) : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)reader["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.SLJrnlNo = (int)sdr1["SLJrnlNo"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr2["Totals"] != DBNull.Value ? (decimal)sdr2["Totals"] : 0;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = (string)sdr2["inv"];
                    vat3b.period = (string)sdr2["period1"];
                    vat3b.SLJrnlNo = (int)sdr2["SLJrnlNo"];
                    vat3b.VatAmount = sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.SLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
                // vatAnalyses.OrderByDescending(x => x.TransDate);
                return vatAnalyses;
            }
            return vatAnalyses;
        }
        public List<Vat3BReport> SalesTransactionListingPeriod(string transaction_type, string period_from, string period_to, string no)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //INVOICE QUERY
            string invoicequery = "select a.\"TotalAmount\" As Totals, sum(d.\"VatAmt\") AS vatamount, a.\"INVDate\" as TransDate, a.\"SLJrnlNo\", a.origin_status, a.invoicenumber, a.post_number, concat(b.\"TranPeriod\",'/',b.\"TranYear\") AS period1, a.\"Period\", concat('INV',a.\"SLJrnlNo\")AS inv,   " +
                "case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname,  c.\"VATNo\" as VATpin    " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"SLJrnlNo\"   " +
                "WHERE b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and b.\"TranFrom\" = 'SL'   " +
                "GROUP BY a.\"TotalAmount\", TransDate, a.\"Period\", a.\"SLJrnlNo\", inv, custname,  VATpin, b.\"TranPeriod\", b.\"TranYear\"  ";
            //INVOICE REVERSAL QUERY
            string invoicereversalquery = "select a.\"TotalAmount\" As Totals, sum(d.\"VatAmt\") AS vatamount, a.\"INVDate\" as TransDate, a.origin_status, a.\"SLJrnlNo\", a.invoicenumber, a.post_number, concat(b.\"TranPeriod\",'/',b.\"TranYear\") AS period1, a.\"Period\",   " +
                "concat('INV',a.\"SLJrnlNo\")AS inv, case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\"   else  ''end as custname,  c.\"VATNo\" as VATpin    " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"SLJrnlNo\"   " +
                "WHERE b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and b.\"TranFrom\" = 'SL-REVERSAL'   " +
                "GROUP BY a.\"TotalAmount\", TransDate, a.\"Period\", a.\"SLJrnlNo\", inv, custname,  VATpin, b.\"TranPeriod\", b.\"TranYear\"   ";
            //CREDIT NOTE QUERY            
            string creditquery = "select a.\"CreditNoteAmount\" As Totals, a.\"CRNVat\" AS vatamount, a.\"CRNDate\" as TransDate, a.\"SLJrnlNo\", concat(b.\"TranPeriod\",'/', b.\"TranYear\") AS period1, a.\"Period\", concat('CRN',a.\"SLJrnlNo\")AS inv,    " +
                "case when c.\"CustCompany\" = '' then c.\"CustFirstName\" when  c.\"CustCompany\" NOTNULL then c.\"CustCompany\" else ''end as custname,  c.\"VATNo\" as VATpin     " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"CustId\" LEFT JOIN \"SLInvoiceDetail\" d on d.\"SLJrnlNo\" = a.\"SLJrnlNo\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"SLJrnlNo\"     " +
                "WHERE b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and b.\"TranFrom\" = 'SL-CRN'     " +
                "GROUP BY a.\"TotalAmount\", TransDate, a.\"Period\", a.\"SLJrnlNo\", inv, custname,  VATpin, b.\"TranPeriod\", b.\"TranYear\"   ";
            //PAYMENT QUERY
            string paymentquery = "select a.\"pyPaid\" As Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname,  c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount     " +
                "FROM \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"     " +
                "WHERE b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and b.\"TranFrom\" = 'SL-PY' and a.\"isReversed\" = FALSE     " +
                "GROUP BY a.\"pyPaid\",a.\"pyProcessDate\",a.\"pyID\",inv,  VATpin,custname, b.\"TranPeriod\", b.\"TranYear\", vatamount;   ";
            //PAYMENT REVERSAL QUERY
            string paymentReversalQuery = "select a.\"pyPaid\" As Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" AS inv, a.\"currentCustName\" as custname,  c.\"VATNo\" as VATpin, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, 0.00 as vatamount   " +
                "FROM \"SLReceipts\" a LEFT JOIN \"SLCustomer\" c on c.\"SLCustomerSerial\" = a.\"cust_id\" LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\" = a.\"pyID\"   " +
                "WHERE b.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and b.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and b.\"TranFrom\" = 'SL-PY-REVERSAL' and a.\"isReversed\" = TRUE  " +
                "GROUP BY a.\"pyPaid\",a.\"pyProcessDate\",a.\"pyID\",inv,  VATpin,custname, b.\"TranPeriod\", b.\"TranYear\", vatamount;   ";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                query = invoicequery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.origin_status = sdr0["origin_status"] != DBNull.Value ? (string)sdr0["origin_status"] : "";
                    vat3b.Invoice_no = sdr0["invoicenumber"] != DBNull.Value ? (int)sdr0["invoicenumber"] : 0;
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    if(vat3b.origin_status == "Posted")
                    {
                        vat3b.inv = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                    }else if(vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                    {
                        vat3b.inv = sdr0["inv"] != DBNull.Value ? (string)sdr0["inv"] : "";
                    }
                    else
                    {
                        vat3b.inv = vat3b.Invoice_no.ToString();
                    }
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoicereversalquery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.origin_status = reader["origin_status"] != DBNull.Value ? (string)reader["origin_status"] : "";
                    vat3b.Invoice_no = reader["invoicenumber"] != DBNull.Value ? (int)reader["invoicenumber"] : 0;
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.SLJrnlNo = (int)reader["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    if (vat3b.origin_status == "Posted")
                    {
                        vat3b.inv = reader["post_number"] != DBNull.Value ? (string)reader["post_number"] : "";
                    }
                    else if (vat3b.origin_status == "Created" && vat3b.Invoice_no == 0)
                    {
                        vat3b.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                    }
                    else
                    {
                        vat3b.inv = vat3b.Invoice_no.ToString();
                    }
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "CRN")
            {
                query = creditquery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "PYT")
            {
                query = paymentquery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? Convert.ToDecimal(sdr0["Totals"]) : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(paymentReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? Convert.ToDecimal(reader["Totals"]) : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                    vat3b.SLJrnlNo = (int)reader["SLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.SLJrnlNo = (int)sdr1["SLJrnlNo"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr2["Totals"] != DBNull.Value ? (decimal)sdr2["Totals"] : 0;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = (string)sdr2["inv"];
                    vat3b.period = (string)sdr2["period1"];
                    vat3b.SLJrnlNo = (int)sdr2["SLJrnlNo"];
                    vat3b.VatAmount = sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
                if (no == "")
                {
                    int number = Int32.Parse(no);
                    vatAnalyses = vatAnalyses.Where(x => x.SLJrnlNo == number).ToList();
                    return vatAnalyses;
                }
                // vatAnalyses.OrderByDescending(x => x.TransDate);
                return vatAnalyses;
            }
            //cnn.Open();
            //NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            //while (sdr0.Read())
            //{
            //    Vat3BReport vat3b = new Vat3BReport();
            //    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
            //    vat3b.VATpin = (string)sdr0["VATpin"];
            //    vat3b.custname = (string)sdr0["custname"];
            //    vat3b.inv = (string)sdr0["inv"];
            //    vat3b.period = (string)sdr0["period1"];
            //    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
            //    vat3b.SLJrnlNo = (int)sdr0["SLJrnlNo"];
            //    vat3b.TransDate = (DateTime)sdr0["TransDate"];
            //    vatAnalyses.Add(vat3b);
            //}
            //cnn.Close();

            //if (no == "")
            //{
            //    int number = Int32.Parse(no);

            //    vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();

            //    return vatAnalyses;
            //}
            return vatAnalyses;
        }
        public List<Vat3BReport> PurchaseTransactionListing(string transaction_type, DateTime from, DateTime to, string no)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //INVOICE QUERY
            string invoicequery = "select a.\"Totals\"::numeric, sum(d.\"VatAmt\") AS vatamount, a.\"InvDate\" as TransDate, a.\"PLJrnlNo\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") AS period1, concat('INV',a.\"PLJrnlNo\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin     " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\"    " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL'    " +
                "GROUP BY a.\"Totals\", TransDate,a.\"Period\",a.\"PLJrnlNo\", inv, custname,  VATpin, b.\"TranPeriod\", b.\"TranYear\"    ";
            //INVOICE REVERSAL QUERY
            string invoicereversalquery = "select a.\"Totals\"::numeric, sum(d.\"VatAmt\") AS vatamount, a.\"InvDate\" as TransDate, a.\"PLJrnlNo\", concat(b.\"TranPeriod\",'/',b.\"TranYear\") AS period1, concat('INV',a.\"PLJrnlNo\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin    " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\"   " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL-REVERSAL'   " +
                "GROUP BY a.\"Totals\", TransDate,a.\"Period\",a.\"PLJrnlNo\", inv, custname,  VATpin, b.\"TranPeriod\", b.\"TranYear\" ";
            //CREDIT NOTE QUERY
            string creditquery = "select a.\"CRNTotal\"::numeric, a.\"CRNVat\" AS vatamount,a.\"CRNDate\" as TransDate, a.\"Period\", a.\"CRNReference\", concat('CRN',a.\"PLJrnlNo\")AS inv, concat(b.\"TranPeriod\", '/', b.\"TranYear\") AS period1, c.\"CustName\" as custname, c.\"VATNo\" as VATpin  " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\"   " +
                "WHERE a.\"HasCreditNote\" = TRUE  AND  a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL-CRN'  " +
                "GROUP BY a.\"CRNTotal\", a.\"CRNVat\", a.\"CRNDate\",a.\"PLJrnlNo\", a.\"Period\", a.\"CRNReference\", b.\"TranPeriod\", b.\"TranYear\", c.\"CustName\", c.\"VATNo\" ";
            // PAYMENT QUERY
            string paymentquery = "select a.\"pyPaid\"::numeric as Totals, a.\"pyProcessDate\" as TransDate, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, c.\"CustName\" as custname, a.\"pyRef\", a.\"pyChequeNumber\" AS inv, c.\"VATNo\" as VATpin, 0.00 as vatamount   " +
                "FROM \"PLReceipts\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"pyID\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"supplier_id\" LEFT JOIN \"PLInvoiceHeader\" d on d.\"PLJrnlNo\" = a.\"pyID\"    " +
                "WHERE a.\"pyProcessDate\" BETWEEN  '" + from.ToString("yyyy-MM-dd") + "' AND '" + to.ToString("yyyy-MM-dd") + "' AND b.\"TranFrom\" = 'PL-PY'    " +
                "GROUP BY  a.\"pyPaid\",a.\"pyProcessDate\", b.\"TranPeriod\", b.\"TranYear\",inv, custname,c.\"VATNo\",a.\"pyRef\", vatamount";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                query = invoicequery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.PLJrnlNo = (int)sdr0["PLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoicereversalquery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.PLJrnlNo = (int)reader["PLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "CRN")
            {
                query = creditquery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["CRNTotal"] != DBNull.Value ? (decimal)sdr0["CRNTotal"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = sdr0["CRNReference"] != DBNull.Value ? (string)sdr0["CRNReference"] : (string)sdr0["inv"];
                    vat3b.period = sdr0["period1"] != DBNull.Value ? (string)sdr0["period1"] : null;
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "PYT")
            {
                cnn.Open();
                NpgsqlDataReader sdr4 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
                while (sdr4.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr4["Totals"] != DBNull.Value ? (decimal)sdr4["Totals"] : 0;
                    vat3b.VATpin = sdr4["VATpin"] != DBNull.Value ? (string)sdr4["VATpin"] : "";
                    vat3b.custname = sdr4["custname"] != DBNull.Value ? (string)sdr4["custname"] : "";
                    vat3b.inv = sdr4["inv"] != DBNull.Value ? (string)sdr4["inv"] : "";
                    vat3b.period = sdr4["period1"] != DBNull.Value ? (string)sdr4["period1"] : "";
                    vat3b.VatAmount = sdr4["vatamount"] != DBNull.Value ? (decimal)sdr4["vatamount"] : 0;
                    vat3b.PLJrnlNo = sdr4["pyRef"] != DBNull.Value ? (int)sdr4["pyRef"] : 0;
                    vat3b.TransDate = (DateTime)sdr4["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    return vatAnalyses;
                }
                return vatAnalyses;
            }
            else
            {                
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    //vat3b.PLJrnlNo = (int)sdr1[" PLJrnlNo"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoicereversalquery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.PLJrnlNo = (int)reader["PLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr2["CRNTotal"] != DBNull.Value ? (decimal)sdr2["CRNTotal"] : 0) * -1;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = sdr2["CRNReference"] != DBNull.Value ? (string)sdr2["CRNReference"] : (string)sdr2["inv"];
                    vat3b.period = sdr2["period1"] != DBNull.Value ? (string)sdr2["period1"] : null;
                    vat3b.VatAmount = (sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr3 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
                while (sdr3.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr3["Totals"] != DBNull.Value ? (decimal)sdr3["Totals"] : 0) * -1;
                    vat3b.VATpin = sdr3["VATpin"] != DBNull.Value ? (string)sdr3["VATpin"] : "";
                    vat3b.custname = sdr3["custname"] != DBNull.Value ? (string)sdr3["custname"] : "";
                    vat3b.inv = sdr3["inv"] != DBNull.Value ? (string)sdr3["inv"] : "";
                    vat3b.period = sdr3["period1"] != DBNull.Value ? (string)sdr3["period1"] : "";
                    vat3b.VatAmount = (sdr3["vatamount"] != DBNull.Value ? (decimal)sdr3["vatamount"] : 0) * -1;
                    vat3b.PLJrnlNo = sdr3["pyRef"] != DBNull.Value ? (int)sdr3["pyRef"] : 0;
                    vat3b.TransDate = (DateTime)sdr3["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    //return vatAnalyses;
                }
                // vatAnalyses.OrderByDescending(x => x.TransDate);
                //return vatAnalyses;
            }
            vatAnalyses.Sort((x, y) => x.TransDate.CompareTo(y.TransDate));
            return vatAnalyses;
        }
        public List<Vat3BReport> PurchaseTransactionListingPeriod(string transaction_type, string period_from, string period_to, string no)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //INVOICE QUERY
            string invoicequery = "select a.\"Totals\"::numeric, sum(b.\"VatAmt\") as vatamount, a.\"TranDate\" as TransDate, a.\"PLJrnlNo\", concat(d.\"TranPeriod\",'/',d.\"TranYear\") as period1, a.\"Period\", concat('INV',a.\"PLJrnlNo\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, d.\"TranFrom\"   " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" d on d.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN financial_periods e on e.fp_ref = a.\"Period\"    " +
                "where d.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and d.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and d.\"TranFrom\" = 'PL'     " +
                "GROUP BY a.\"Totals\", a.\"TranDate\", a.\"PLJrnlNo\", d.\"TranPeriod\", d.\"TranYear\", a.\"Period\", inv, c.\"CustName\", c.\"VATNo\", d.\"TranFrom\";   ";
            //INVOICE REVERSAL QUERY
            string invoicereversalquery = "select a.\"Totals\"::numeric, sum(b.\"VatAmt\") as vatamount, a.\"TranDate\" as TransDate, a.\"PLJrnlNo\", concat(d.\"TranPeriod\",'/',d.\"TranYear\") as period1, a.\"Period\", concat('INV',a.\"PLJrnlNo\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, d.\"TranFrom\"     " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" d on d.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN financial_periods e on e.fp_ref = a.\"Period\"     " +
                "where d.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and d.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and d.\"TranFrom\" = 'PL-REVERSAL'    " +
                "GROUP BY a.\"Totals\", a.\"TranDate\", a.\"PLJrnlNo\", d.\"TranPeriod\", d.\"TranYear\", a.\"Period\", inv, c.\"CustName\", c.\"VATNo\", d.\"TranFrom\";  ";
            //CREDIT NOTE QUERY
            string creditquery = "select a.\"CRNTotal\":: numeric, a.\"CRNVat\" as vatamount, a.\"CRNDate\" as TransDate, a.\"CRNReference\", concat('CRN', a.\"PLJrnlNo\") as inv, concat(d.\"TranPeriod\",'/',d.\"TranYear\") as period1, c.\"CustName\" as custname, c.\"VATNo\" as VATpin   " +
                "from \"PLInvoiceHeader\" a  LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN  \"NlJournalHeader\" d on d.\"PlJrnlNo\" = a.\"PLJrnlNo\"  " +
                "where d.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' AND  d.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and d.\"TranFrom\" = 'PL-CRN'  " +
                "GROUP BY a.\"CRNTotal\":: numeric, a.\"CRNVat\", a.\"CRNDate\", a.\"CRNReference\", a.\"PLJrnlNo\", d.\"TranPeriod\",d.\"TranYear\", c.\"CustName\", c.\"VATNo\"    ";
            // PAYMENT QUERY
            string paymentquery = "select a.\"pyPaid\"::numeric as Totals, a.\"pyProcessDate\" as TransDate, concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period1, c.\"CustName\" as custname,a.\"pyRef\", a.\"pyChequeNumber\" AS inv,c.\"VATNo\" as VATpin, 0.00 as vatamount  " +
                "FROM \"PLReceipts\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"pyID\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"supplier_id\" LEFT JOIN  \"PLInvoiceHeader\" d on d.\"PLJrnlNo\" = a.\"pyID\" " +
                "WHERE b.\"TranPeriod\" BETWEEN '" + period_from.Split("/")[0] + "' AND '" + period_to.Split("/")[0] + "' AND  b.\"TranYear\" BETWEEN '" + period_from.Split("/")[1] + "' AND '" + period_to.Split("/")[1] + "' AND b.\"TranFrom\" = 'PL-PY'     " +
                "GROUP BY  a.\"pyPaid\", a.\"pyProcessDate\", b.\"TranPeriod\", b.\"TranYear\", inv, custname, c.\"VATNo\", a.\"pyRef\", vatamount  ";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                query = invoicequery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = (string)sdr0["inv"];
                    vat3b.period = (string)sdr0["period1"];
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.PLJrnlNo = (int)sdr0["PLJrnlNo"];
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoicereversalquery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.PLJrnlNo = (int)reader["PLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "CRN")
            {
                query = creditquery;
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["CRNTotal"] != DBNull.Value ? (decimal)sdr0["CRNTotal"] : 0;
                    vat3b.VATpin = (string)sdr0["VATpin"];
                    vat3b.custname = (string)sdr0["custname"];
                    vat3b.inv = sdr0["CRNReference"] != DBNull.Value ? (string)sdr0["CRNReference"] : (string)sdr0["inv"];
                    vat3b.period = sdr0["period1"] != DBNull.Value ? (string)sdr0["period1"] : null;
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    return vatAnalyses;
                }
            }
            else if (transaction_type == "PYT")
            {
                cnn.Open();
                NpgsqlDataReader sdr4 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
                while (sdr4.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr4["Totals"] != DBNull.Value ? (decimal)sdr4["Totals"] : 0;
                    vat3b.VATpin = sdr4["VATpin"] != DBNull.Value ? (string)sdr4["VATpin"] : "";
                    vat3b.custname = sdr4["custname"] != DBNull.Value ? (string)sdr4["custname"] : "";
                    vat3b.inv = sdr4["inv"] != DBNull.Value ? (string)sdr4["inv"] : "";
                    vat3b.period = sdr4["period1"] != DBNull.Value ? (string)sdr4["period1"] : "";
                    vat3b.VatAmount = sdr4["vatamount"] != DBNull.Value ? (decimal)sdr4["vatamount"] : 0;
                    vat3b.PLJrnlNo = sdr4["pyRef"] != DBNull.Value ? (int)sdr4["pyRef"] : 0;
                    vat3b.TransDate = (DateTime)sdr4["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    return vatAnalyses;
                }
                return vatAnalyses;
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    //vat3b.PLJrnlNo = (int)sdr1[" PLJrnlNo"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoicereversalquery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.PLJrnlNo = (int)reader["PLJrnlNo"];
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr2["CRNTotal"] != DBNull.Value ? (decimal)sdr2["CRNTotal"] : 0) * -1;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = sdr2["CRNReference"] != DBNull.Value ? (string)sdr2["CRNReference"] : (string)sdr2["inv"];
                    vat3b.period = sdr2["period1"] != DBNull.Value ? (string)sdr2["period1"] : null;
                    vat3b.VatAmount = (sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                cnn.Open();
                NpgsqlDataReader sdr3 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
                while (sdr3.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr3["Totals"] != DBNull.Value ? (decimal)sdr3["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)sdr3["VATpin"];
                    vat3b.custname = (string)sdr3["custname"];
                    vat3b.inv = sdr3["inv"] != DBNull.Value ? (string)sdr3["inv"] : "";
                    vat3b.period = sdr3["period1"] != DBNull.Value ? (string)sdr3["period1"] : "";
                    vat3b.VatAmount = (sdr3["vatamount"] != DBNull.Value ? (decimal)sdr3["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)sdr3["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
                vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
                if (no != "")
                {
                    int number = Int32.Parse(no);
                    if (number > 0)
                    {
                        vatAnalyses = vatAnalyses.Where(x => x.PLJrnlNo == number).ToList();
                    }
                    //return vatAnalyses;
                }
                // vatAnalyses.OrderByDescending(x => x.TransDate);
                //return vatAnalyses;
            }
            vatAnalyses.Sort((x, y) => x.TransDate.CompareTo(y.TransDate));
            return vatAnalyses;
        }
        public List<Vat3BReport> PurchaseAuditTrailPeriod(string transaction_type, string period_from, string period_to)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //INVOICE QUERY
            string invoicequery = "select a.\"Totals\"::numeric, sum(b.\"VatAmt\") as vatamount, a.\"InvDate\" as TransDate,concat(f.\"TranPeriod\",'/',f.\"TranYear\") as period1, a.\"Period\", concat('INV', a.\"PLJrnlNo\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin   " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"PLJrnlNo\"    " +
                "where f.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and f.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' AND f.\"TranFrom\" = 'PL'     " +
                "GROUP BY a.\"Totals\", a.\"InvDate\", a.\"Period\",inv,c.\"CustName\", c.\"VATNo\", f.\"TranPeriod\", f.\"TranYear\";   ";
            //INVOICE REVERSAL QUERY
            string invoiceReversalQuery = "select a.\"Totals\"::numeric, sum(b.\"VatAmt\") as vatamount, a.\"InvDate\" as TransDate,concat(f.\"TranPeriod\",'/',f.\"TranYear\") as period1, a.\"Period\", concat('INV', a.\"PLJrnlNo\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin    " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"PLJrnlNo\"     " +
                "where f.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and f.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' AND f.\"TranFrom\" = 'PL-REVERSAL'     " +
                "GROUP BY a.\"Totals\", a.\"InvDate\", a.\"Period\",inv,c.\"CustName\", c.\"VATNo\", f.\"TranPeriod\", f.\"TranYear\"; ";
            //CREDIT NOTE QUERY
            string creditquery = "select a.\"CRNTotal\", a.\"CRNVat\", a.\"CRNDate\" as TransDate, a.\"CRNReference\", concat('CRN', a.\"PLJrnlNo\") as inv, concat(f.\"TranPeriod\", '/', f.\"TranYear\") as period1, c.\"CustName\" as custname, c.\"VATNo\" as VATpin    " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = \"PLCustID\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"PLJrnlNo\"       " +
                "where f.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' AND f.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' AND f.\"TranFrom\" = 'PL-CRN'       " +
                "GROUP BY a.\"CRNTotal\", a.\"CRNVat\", a.\"CRNDate\", a.\"Period\", a.\"CRNReference\", a.\"PLJrnlNo\", f.\"TranPeriod\", f.\"TranYear\", c.\"CustName\", c.\"VATNo\";    ";
            //PAYMENT QUERY
            string paymentquery = "select a.\"pyPaid\"::numeric as Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" as inv, b.\"CustName\" as custname, b.\"VATNo\" as VATpin, concat(c.\"TranPeriod\", '/', c.\"TranYear\") as period1, 0.00 as vatamount " +
                "from \"PLReceipts\" a LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\" LEFT JOIN \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"pyID\" " +
                "where c.\"TranPeriod\" BETWEEN '" + period_from.Split("/")[0] + "' AND '" + period_to.Split("/")[0] + "' and c.\"TranYear\" BETWEEN '" + period_from.Split("/")[1] + "' AND '" + period_to.Split("/")[1] + "' AND c.\"TranFrom\" = 'PL-PY'  " +
                "GROUP BY a.\"pyPaid\", a.\"pyProcessDate\", a.\"pyID\", b.\"CustName\", b.\"VATNo\", c.\"TranPeriod\", c.\"TranYear\"";

             List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                cnn.Open();
                query = invoicequery;
                NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoiceReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else if (transaction_type == "CRN")
            {
                cnn.Open();
                query = creditquery;
                NpgsqlDataReader sdr2 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr2["CRNTotal"] != DBNull.Value ? (decimal)sdr2["CRNTotal"] : 0;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = sdr2["CRNReference"] != DBNull.Value ? (string)sdr2["CRNReference"] : (string)sdr2["inv"];
                    vat3b.period = sdr2["period1"] != DBNull.Value ? (string)sdr2["period1"] : null;
                    vat3b.VatAmount = sdr2["CRNVat"] != DBNull.Value ? (decimal)sdr2["CRNVat"] : 0;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else if (transaction_type == "PYT")
            {
                cnn.Open();
                query = paymentquery;
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? Convert.ToDecimal(sdr0["Totals"]) : 0;
                    vat3b.VATpin = sdr0["VATpin"] != DBNull.Value ? (string)sdr0["VATpin"] : "";
                    vat3b.custname = sdr0["custname"] != DBNull.Value ? (string)sdr0["custname"] : "";
                    vat3b.inv = sdr0["inv"] != DBNull.Value ? (string)sdr0["inv"] : "";
                    vat3b.period = sdr0["period1"] != DBNull.Value ? (string)sdr0["period1"] : "";
                    vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr0["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoiceReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr2["CRNTotal"] != DBNull.Value ? (decimal)sdr2["CRNTotal"] : 0) * -1;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = sdr2["CRNReference"] != DBNull.Value ? (string)sdr2["CRNReference"] : (string)sdr2["inv"];
                    vat3b.period = sdr2["period1"] != DBNull.Value ? (string)sdr2["period1"] : null;
                    vat3b.VatAmount = (sdr2["CRNVat"] != DBNull.Value ? (decimal)sdr2["CRNVat"] : 0) * -1;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();             
                
                cnn.Open();
                NpgsqlDataReader reader1 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
                while (reader1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader1["Totals"] != DBNull.Value ? Convert.ToDecimal(reader1["Totals"]) : 0) * -1;
                    vat3b.VATpin = reader1["VATpin"] != DBNull.Value ? (string)reader1["VATpin"] : "";
                    vat3b.custname = reader1["custname"] != DBNull.Value ? (string)reader1["custname"] : "";
                    vat3b.inv = reader1["inv"] != DBNull.Value ? (string)reader1["inv"] : "";
                    vat3b.period = reader1["period1"] != DBNull.Value ? (string)reader1["period1"] : "";
                    vat3b.VatAmount = (reader1["vatamount"] != DBNull.Value ? (decimal)reader1["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)reader1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
            return vatAnalyses;
        }
        public List<Vat3BReport> PurchaseAuditTrail(string transaction_type, DateTime from, DateTime to)
        {
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //INVOICE QUERY
            string invoicequery = "select a.\"Totals\"::numeric,sum(d.\"VatAmt\") AS vatamount, a.\"InvDate\" as TransDate, concat(f.\"TranPeriod\",'/',f.\"TranYear\") AS period1, a.\"Period\", concat('INV',a.\"PLJrnlNo\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin     " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"PLJrnlNo\"       " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and f.\"TranFrom\" = 'PL'       " +
                "GROUP BY a.\"Totals\", TransDate, f.\"TranPeriod\", f.\"TranYear\", a.\"Period\", inv, custname,  VATpin; ";
            //invoice reversal query
            string invoiceReversalQuery = "select a.\"Totals\"::numeric,sum(d.\"VatAmt\") AS vatamount, a.\"InvDate\" as TransDate, concat(f.\"TranPeriod\",'/',f.\"TranYear\") AS period1, a.\"Period\", concat('INV',a.\"PLJrnlNo\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin    " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"PLJrnlNo\"     " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and f.\"TranFrom\" = 'PL-REVERSAL'      " +
                "GROUP BY a.\"Totals\", TransDate, f.\"TranPeriod\", f.\"TranYear\", a.\"Period\", inv, custname,  VATpin; ";
            //CREDIT NOTE QUERY
            string creditquery = "select a.\"CRNTotal\"::numeric, a.\"CRNVat\" AS vatamount,a.\"CRNDate\" as TransDate, a.\"Period\", a.\"CRNReference\", concat('CRN',a.\"PLJrnlNo\")AS inv, concat(f.\"TranPeriod\", '/', f.\"TranYear\") AS period1, c.\"CustName\" as custname,c.\"VATNo\" as VATpin   " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"financial_periods\" e on e.\"fp_ref\" = a.\"Period\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\"  LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"PLJrnlNo\"    " +
                "WHERE a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and f.\"TranFrom\" = 'PL-CRN'   " +
                "GROUP BY a.\"CRNTotal\", a.\"CRNVat\",a.\"CRNDate\" , a.\"Period\", a.\"PLJrnlNo\", f.\"TranPeriod\", f.\"TranYear\", c.\"CustName\", c.\"VATNo\",a.\"CRNReference\"; ";
            //PAYMENT QUERY
            string paymentquery = "select a.\"pyPaid\"::numeric as Totals, a.\"pyProcessDate\" as TransDate, a.\"pyID\" as SLJrnlNo, a.\"pyChequeNumber\" as inv, b.\"CustName\" as custname, b.\"VATNo\" as VATpin, concat(f.\"TranPeriod\", '/', f.\"TranYear\") as period1, 0.00 as vatamount " +
                "from \"PLReceipts\" a LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\" LEFT JOIN \"NlJournalHeader\" f on f.\"PlJrnlNo\" = a.\"pyID\"  " +
                "where a.\"pyProcessDate\" BETWEEN '" + from.ToString("yyyy-MM-dd") + "' AND '" + to.ToString("yyyy-MM-dd") + "' and f.\"TranFrom\" = 'PL-PY'   " +
                "GROUP BY a.\"pyPaid\", a.\"pyProcessDate\", a.\"pyID\", inv, b.\"CustName\", b.\"VATNo\", f.\"TranPeriod\", f.\"TranYear\", vatamount";

            List<Vat3BReport> auditTrail = new List<Vat3BReport>();
            string query = "";
            if (transaction_type == "INV")
            {
                cnn.Open();
                query = invoicequery;
                NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoiceReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else if (transaction_type == "CRN")
            {
                cnn.Open();
                query = creditquery;
                NpgsqlDataReader sdr2 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr2["CRNTotal"] != DBNull.Value ? (decimal)sdr2["CRNTotal"] : 0);
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = sdr2["CRNReference"] != DBNull.Value ? (string)sdr2["CRNReference"] : (string)sdr2["inv"];
                    vat3b.period = sdr2["period1"] != DBNull.Value ? (string)sdr2["period1"] : null;
                    vat3b.VatAmount = (sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0);
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();
            }
            else if (transaction_type == "PYT")
            {
                cnn.Open();
                query = paymentquery;
                NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3B = new Vat3BReport();
                    vat3B.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0);
                    vat3B.VATpin = reader["VATpin"] != DBNull.Value ? (string)reader["VATpin"] : "";
                    vat3B.custname = reader["custname"] != DBNull.Value ? (string)reader["custname"] : "";
                    vat3B.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                    vat3B.period = reader["period1"] != DBNull.Value ? (string)reader["period1"] : "";
                    vat3B.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0);
                    vat3B.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3B);
                }
                cnn.Close();
            }
            else
            {
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = sdr1["Totals"] != DBNull.Value ? (decimal)sdr1["Totals"] : 0;
                    vat3b.VATpin = (string)sdr1["VATpin"];
                    vat3b.custname = (string)sdr1["custname"];
                    vat3b.inv = (string)sdr1["inv"];
                    vat3b.period = (string)sdr1["period1"];
                    vat3b.VatAmount = sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0;
                    vat3b.TransDate = (DateTime)sdr1["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader = new NpgsqlCommand(invoiceReversalQuery, cnn).ExecuteReader();
                while (reader.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0) * -1;
                    vat3b.VATpin = (string)reader["VATpin"];
                    vat3b.custname = (string)reader["custname"];
                    vat3b.inv = (string)reader["inv"];
                    vat3b.period = (string)reader["period1"];
                    vat3b.VatAmount = (reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)reader["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(creditquery, cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    Vat3BReport vat3b = new Vat3BReport();
                    vat3b.TotalAmount = (sdr2["CRNTotal"] != DBNull.Value ? (decimal)sdr2["CRNTotal"] : 0) * -1;
                    vat3b.VATpin = (string)sdr2["VATpin"];
                    vat3b.custname = (string)sdr2["custname"];
                    vat3b.inv = sdr2["CRNReference"] != DBNull.Value ? (string)sdr2["CRNReference"] : (string)sdr2["inv"];
                    vat3b.period = sdr2["period1"] != DBNull.Value ? (string)sdr2["period1"] : null;
                    vat3b.VatAmount = (sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0) * -1;
                    vat3b.TransDate = (DateTime)sdr2["TransDate"];
                    vatAnalyses.Add(vat3b);
                }
                cnn.Close();

                cnn.Open();
                NpgsqlDataReader reader1 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
                while (reader1.Read())
                {
                    Vat3BReport vat3B = new Vat3BReport();
                    vat3B.TotalAmount = (reader1["Totals"] != DBNull.Value ? (decimal)reader1["Totals"] : 0) * -1;
                    vat3B.VATpin = reader1["VATpin"] != DBNull.Value ? (string)reader1["VATpin"] : "";
                    vat3B.custname = reader1["custname"] != DBNull.Value ? (string)reader1["custname"] : "";
                    vat3B.inv = reader1["inv"] != DBNull.Value ? (string)reader1["inv"] : "";
                    vat3B.period = reader1["period1"] != DBNull.Value ? (string)reader1["period1"] : "";
                    vat3B.VatAmount = (reader1["vatamount"] != DBNull.Value ? (decimal)reader1["vatamount"] : 0) * -1;
                    vat3B.TransDate = (DateTime)reader1["TransDate"];
                    vatAnalyses.Add(vat3B);
                }
                cnn.Close();
            }
            vatAnalyses.Sort((x, y) => y.TransDate.CompareTo(x.TransDate));
            return vatAnalyses;
        }        
        public List<Vat3BReport> Vat3BReportByPeriod(string period_from, string period_to)
        {
            //INVOICE QUERY
            var invoicequery = "select a.\"Totals\"::numeric, sum(d.\"VatAmt\") as vatamount, a.\"InvDate\" as TransDate, a.\"Period\", concat(' ', a.\"DocRef\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, concat(j.\"TranPeriod\", '/', j.\"TranYear\") as period1, a.\"PLDescription\", j.\"TranFrom\"  " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" j on j.\"PlJrnlNo\" = a.\"PLJrnlNo\"   " +
                "where j.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and j.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and j.\"TranFrom\" = 'PL'    " +
                "GROUP BY a.\"Totals\", a.\"InvDate\", j.\"TranPeriod\", j.\"TranYear\", a.\"Period\", inv, c.\"CustName\", c.\"VATNo\", a.\"PLDescription\", j.\"TranFrom\";  ";

            string invoiveVatBigQuery = "select sum(d.\"Total\") as Totals, sum(d.\"VatAmt\") as vatamount, a.\"InvDate\" as TransDate, a.\"Period\", concat(' ', a.\"DocRef\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, concat(j.\"TranPeriod\", '/', j.\"TranYear\") as period1, a.\"PLDescription\", j.\"TranFrom\"  " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" j on j.\"PlJrnlNo\" = a.\"PLJrnlNo\"  " +
                " where j.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and j.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and j.\"TranFrom\" = 'PL' and d.\"VatAmt\" > 0    GROUP BY a.\"InvDate\", j.\"TranPeriod\", j.\"TranYear\", a.\"Period\", inv, c.\"CustName\", c.\"VATNo\", a.\"PLDescription\", j.\"TranFrom\";    ";
            string invoiceVatLessQuery = "select sum(d.\"Total\") as Totals, sum(d.\"VatAmt\") as vatamount, a.\"InvDate\" as TransDate, a.\"Period\", concat(' ', a.\"DocRef\") as inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, concat(j.\"TranPeriod\", '/', j.\"TranYear\") as period1, a.\"PLDescription\", j.\"TranFrom\"  " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" j on j.\"PlJrnlNo\" = a.\"PLJrnlNo\"   " +
                "where j.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and j.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and j.\"TranFrom\" = 'PL' and d.\"VatAmt\" <= 0    GROUP BY a.\"InvDate\", j.\"TranPeriod\", j.\"TranYear\", a.\"Period\", inv, c.\"CustName\", c.\"VATNo\", a.\"PLDescription\", j.\"TranFrom\";    ";
            //CREDIT NOTE QUERY
            var creditnotequery = "select a.\"CRNTotal\"::numeric, a.\"CRNVat\" as vatamount, a.\"CRNDate\" as TransDate, a.\"CRNReference\" as inv, a.\"DocRef\", c.\"CustName\" as custname, c.\"VATNo\" as VATpin, concat(j.\"TranPeriod\", '/', j.\"TranYear\") as period1, a.\"Additionals\", j.\"TranFrom\"    " +
                "from \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"NlJournalHeader\" j on j.\"PlJrnlNo\" = a.\"PLJrnlNo\"   " +
                "where j.\"TranPeriod\" BETWEEN '"+period_from.Split("/")[0]+"' AND '"+period_to.Split("/")[0]+"' and j.\"TranYear\" BETWEEN '"+period_from.Split("/")[1]+"' AND '"+period_to.Split("/")[1]+"' and j.\"TranFrom\" = 'PL-CRN'   " +
                "GROUP BY a.\"CRNTotal\", a.\"CRNVat\", a.\"CRNDate\", j.\"TranPeriod\", j.\"TranYear\", a.\"Period\", inv, c.\"CustName\", c.\"VATNo\", a.\"Additionals\", j.\"TranFrom\", a.\"DocRef\";  ";

            List<Vat3BReport> vatAnalysis = new List<Vat3BReport>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));            
            cnn.Open();
            NpgsqlDataReader sqr0 = new NpgsqlCommand(invoiveVatBigQuery, cnn).ExecuteReader();
            while (sqr0.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = sqr0["Totals"] != DBNull.Value ? (decimal)sqr0["Totals"] : 0;
                vat3b.VATpin = (string)sqr0["VATpin"];
                vat3b.custname = (string)sqr0["custname"];
                vat3b.inv = (string)sqr0["inv"];
                vat3b.period = (string)sqr0["period1"];
                vat3b.VatAmount = sqr0["vatamount"] != DBNull.Value ? (decimal)sqr0["vatamount"] : 0;
                vat3b.TransDate = (DateTime)sqr0["TransDate"];
                vat3b.Description = sqr0["PLDescription"] != DBNull.Value ? (string)sqr0["PLDescription"] : "";
                vat3b.TransFrom = sqr0["TranFrom"] != DBNull.Value ? (string)sqr0["TranFrom"] : "";
                vatAnalysis.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoiceVatLessQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0;
                vat3b.VATpin = (string)reader["VATpin"];
                vat3b.custname = (string)reader["custname"];
                vat3b.inv = (string)reader["inv"];
                vat3b.period = (string)reader["period1"];
                vat3b.VatAmount = reader["vatamount"] != DBNull.Value ? (decimal)reader["vatamount"] : 0;
                vat3b.TransDate = (DateTime)reader["TransDate"];
                vat3b.Description = reader["PLDescription"] != DBNull.Value ? (string)reader["PLDescription"] : "";
                vat3b.TransFrom = reader["TranFrom"] != DBNull.Value ? (string)reader["TranFrom"] : "";
                vatAnalysis.Add(vat3b);
            }
            cnn.Close();

            /// get the credit notes for the purchase invoice 
            List<Vat3BReport> vat3BReports = new List<Vat3BReport>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(creditnotequery, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = (sdr1["CRNTotal"] != DBNull.Value ? (decimal)sdr1["CRNTotal"] : 0);
                //vat3b.TotalAmount = vat3b.TotalAmount * -1;
                vat3b.VATpin = (string)sdr1["VATpin"];
                vat3b.custname = (string)sdr1["custname"];
                vat3b.inv = sdr1["inv"] != DBNull.Value ? (string)sdr1["inv"] : (string)sdr1["DocRef"];
                vat3b.VatAmount = (sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0);
                //vat3b.VatAmount = vat3b.VatAmount * -1;
                vat3b.TransDate = (DateTime)sdr1["TransDate"];
                vat3b.Description = sdr1["Additionals"] != DBNull.Value ? (string)sdr1["Additionals"] : "";
                vat3b.period = sdr1["period1"] != DBNull.Value ? (string)sdr1["period1"] : "";
                vat3b.TransFrom = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : "";
                vat3BReports.Add(vat3b);
            }
            cnn.Close();
            vatAnalysis.AddRange(vat3BReports);
            vatAnalysis.Sort((x, y) => x.TransDate.CompareTo(y.TransDate));
            return vatAnalysis;
        }
        public List<Vat3BReport> VAT3AReport(DateTime from, DateTime to)
        {
            //INVOICE QUERY
            string invoiceQuery = "select a.\"Totals\"::numeric, sum(d.\"VatAmt\") AS vatamount, a.\"InvDate\" as TransDate, concat(' ',a.\"DocRef\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, b.\"TranFrom\", concat(b.\"TranPeriod\", '/',b.\"TranYear\") as period, a.\"PLDescription\"   " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\"      " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL'  " +
                "GROUP BY a.\"Totals\", TransDate, inv, custname,  VATpin, b.\"TranFrom\", b.\"TranPeriod\", b.\"TranYear\", a.\"PLDescription\"; ";
            string InvoiceVatMoreQuery = "select sum(d.\"Total\") AS Totals, sum(d.\"VatAmt\") AS vatamount, a.\"InvDate\" as TransDate, concat(' ',a.\"DocRef\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, b.\"TranFrom\", concat(b.\"TranPeriod\", '/',b.\"TranYear\") as period, a.\"PLDescription\"   " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\"      " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL' and d.\"VatAmt\" > 0  GROUP BY TransDate, inv, custname,  VATpin, b.\"TranFrom\", b.\"TranPeriod\", b.\"TranYear\", a.\"PLDescription\";   ";
            string invoiceVatLessQuery = "select sum(d.\"Total\") AS Totals, d.\"VatAmt\" AS vatamount, a.\"InvDate\" as TransDate, concat(' ',a.\"DocRef\")AS inv, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, b.\"TranFrom\", concat(b.\"TranPeriod\", '/',b.\"TranYear\") as period, a.\"PLDescription\"   " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\"      " +
                "WHERE a.\"InvDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL' and d.\"VatAmt\" <= 0  GROUP BY TransDate, inv, custname,  VATpin, b.\"TranFrom\", b.\"TranPeriod\", b.\"TranYear\", a.\"PLDescription\", d.\"VatAmt\";  ";
            //CREDIT NOTE QUERY
            string creditnoteQuery = "select a.\"CRNTotal\"::numeric, a.\"CRNVat\" AS vatamount, a.\"CRNDate\" as TransDate, a.\"CRNReference\" AS inv, concat('CRN', a.\"PLJrnlNo\") as docRef, c.\"CustName\" as custname, c.\"VATNo\" as VATpin, b.\"TranFrom\",     " +
                "concat(b.\"TranPeriod\", '/',b.\"TranYear\") as period, a.\"Additionals\"     " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN \"PLInvoiceDetail\" d on d.\"PLJrnlNo\" = a.\"PLJrnlNo\"       " +
                "WHERE a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and b.\"TranFrom\" = 'PL-CRN'    " +
                "GROUP BY a.\"CRNTotal\", a.\"CRNVat\", TransDate, inv, custname,  VATpin, b.\"TranFrom\", b.\"TranPeriod\", b.\"TranYear\", a.\"Additionals\", docRef;    ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(InvoiceVatMoreQuery, cnn).ExecuteReader();
            List<Vat3BReport> vatAnalyses = new List<Vat3BReport>();
            while (sdr0.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                vat3b.VATpin = (string)sdr0["VATpin"];
                vat3b.custname = (string)sdr0["custname"];
                vat3b.inv = (string)sdr0["inv"];
                vat3b.VatAmount = sdr0["vatamount"] != DBNull.Value ? (decimal)sdr0["vatamount"] : 0;
                vat3b.period = sdr0["period"] != DBNull.Value ? (string)sdr0["period"] : "";
                vat3b.Description = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : "";
                vat3b.TransFrom = sdr0["TranFrom"] != DBNull.Value ? (string)sdr0["TranFrom"] : "";
                vat3b.TransDate = (DateTime)sdr0["TransDate"];
                vatAnalyses.Add(vat3b);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand(invoiceVatLessQuery, cnn).ExecuteReader();
            List<Vat3BReport> analysisVat = new List<Vat3BReport>();
            while (sdr2.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = sdr2["Totals"] != DBNull.Value ? (decimal)sdr2["Totals"] : 0;
                vat3b.VATpin = (string)sdr2["VATpin"];
                vat3b.custname = (string)sdr2["custname"];
                vat3b.inv = (string)sdr2["inv"];
                vat3b.VatAmount = sdr2["vatamount"] != DBNull.Value ? (decimal)sdr2["vatamount"] : 0;
                vat3b.period = sdr2["period"] != DBNull.Value ? (string)sdr2["period"] : "";
                vat3b.Description = sdr2["PLDescription"] != DBNull.Value ? (string)sdr2["PLDescription"] : "";
                vat3b.TransFrom = sdr2["TranFrom"] != DBNull.Value ? (string)sdr2["TranFrom"] : "";
                vat3b.TransDate = (DateTime)sdr2["TransDate"];
                analysisVat.Add(vat3b);
            }
            cnn.Close();

            /// get the credit notes for the purchase invoice
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(creditnoteQuery, cnn).ExecuteReader();
            List<Vat3BReport> vat3BReports = new List<Vat3BReport>();
            while (sdr1.Read())
            {
                Vat3BReport vat3b = new Vat3BReport();
                vat3b.TotalAmount = (sdr1["CRNTotal"] != DBNull.Value ? (decimal)sdr1["CRNTotal"] : 0);
                //vat3b.TotalAmount = vat3b.TotalAmount * -1;
                vat3b.VATpin = (string)sdr1["VATpin"];
                vat3b.custname = (string)sdr1["custname"];
                vat3b.inv = sdr1["inv"] != DBNull.Value ? (string)sdr1["inv"] : (string)sdr1["docRef"];
                vat3b.VatAmount = (sdr1["vatamount"] != DBNull.Value ? (decimal)sdr1["vatamount"] : 0);
                vat3b.TransFrom = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : "";
                vat3b.Description = sdr1["Additionals"] != DBNull.Value ? (string)sdr1["Additionals"] : "";
                vat3b.period = sdr1["period"] != DBNull.Value ? (string)sdr1["period"] : "";
                vat3b.TransDate = (DateTime)sdr1["TransDate"];
                vat3BReports.Add(vat3b);
            }
            cnn.Close();
            vatAnalyses.AddRange(vat3BReports);
            vatAnalyses.AddRange(analysisVat);
            vatAnalyses.Sort((x, y) => x.TransDate.CompareTo(y.TransDate));
            return vatAnalyses;
        }
        public MyResponse activateVATItem(int id)
        {
            MyResponse response = new MyResponse();
            string updtQ = "UPDATE \"VATs\" SET \"VtActive\" = " + true + "  WHERE \"VtId\" = '" + id + "' ;";
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "VAT activated successfully";
            }
            return response;
        }
        public MyResponse deactivateVATItem(int id)
        {
            MyResponse response = new MyResponse();
            string updtQ = "UPDATE \"VATs\" SET \"VtActive\" = " + false + "  WHERE \"VtId\" = '" + id + "' ;";
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "VAT has been deactivated";
            }
            return response;
        }
        public List<VatAnalysis> vatPurchaseanalysis(DateTime from, DateTime to)
        {
            //SUBQUERY FOR BOTH INVOICE AND CREDIT NOTE
            string query = "SELECT x.vatperc, COALESCE(sum(x.vattotal), 0) as vat, COALESCE(sum(x.goodstotal), 0) as total  FROM" +
                "(SELECT B.\"VatPerc\" as vatperc,SUM(B.\"VatAmt\") as vattotal, SUM((B.\"UnitPrice\"*B.\"ProdQty\"))AS goodstotal FROM \"PLInvoiceHeader\" A LEFT JOIN  \"PLInvoiceDetail\" B ON B.\"PLJrnlNo\" = A.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" C ON C.\"PlJrnlNo\" = A.\"PLJrnlNo\"  " +
                "WHERE A.\"TranDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"'  AND A.\"TranDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' AND C.\"TranFrom\" = 'PL'   GROUP BY vatperc UNION   " +
                "SELECT A.\"CrnVatPercent\" AS vatperc, SUM(A.\"CRNVat\" * -1) as vattotal, sum((A.\"CRNTotal\" - A.\"CRNVat\") * -1) as goodstotal  FROM \"PLInvoiceHeader\" A LEFT JOIN \"NlJournalHeader\" B ON B.\"PlJrnlNo\" = A.\"PLJrnlNo\"    " +
                "WHERE A.\"CRNDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"' AND A.\"CRNDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' AND B.\"TranFrom\" = 'PL-CRN' GROUP BY vatperc ) as x GROUP BY x.vatperc;   ";

            string finalQuery = "SELECT x.vatperc, COALESCE(sum(x.vattotal), 0) as vat, COALESCE(sum(x.goodstotal), 0) as total FROM(   " +
                "SELECT B.\"VatPerc\" as vatperc,SUM(B.\"VatAmt\") as vattotal, SUM(b.\"Total\" - b.\"VatAmt\")AS goodstotal FROM \"PLInvoiceHeader\" A LEFT JOIN  \"PLInvoiceDetail\" B ON B.\"PLJrnlNo\" = A.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" C ON C.\"PlJrnlNo\" = A.\"PLJrnlNo\"  " +
                "WHERE A.\"InvDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"'  AND A.\"InvDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' AND C.\"TranFrom\" = 'PL' and b.\"VatAmt\" > 0   GROUP BY vatperc     UNION     " +
                "SELECT A.\"CrnVatPercent\" AS vatperc, SUM(A.\"CRNVat\" * -1) as vattotal, sum((A.\"CRNTotal\" - A.\"CRNVat\") * -1) as goodstotal  FROM \"PLInvoiceHeader\" A LEFT JOIN \"NlJournalHeader\" B ON B.\"PlJrnlNo\" = A.\"PLJrnlNo\"    " +
                "WHERE A.\"CRNDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"' AND A.\"CRNDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' AND B.\"TranFrom\" = 'PL-CRN' GROUP BY vatperc     union              " +
                "SELECT B.\"VatPerc\" as vatperc,SUM(B.\"VatAmt\") as vattotal, SUM(b.\"Total\" - b.\"VatAmt\")AS goodstotal FROM \"PLInvoiceHeader\" A LEFT JOIN  \"PLInvoiceDetail\" B ON B.\"PLJrnlNo\" = A.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" C ON C.\"PlJrnlNo\" = A.\"PLJrnlNo\"    " +
                "WHERE A.\"InvDate\"::DATE >= '"+from.ToString("yyyy-MM-dd")+"'  AND A.\"InvDate\"::DATE <= '"+to.ToString("yyyy-MM-dd")+"' AND C.\"TranFrom\" = 'PL' and b.\"VatAmt\" <= 0   GROUP BY vatperc ) as x GROUP BY x.vatperc;   ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(finalQuery,  cnn).ExecuteReader();
            List<VatAnalysis> vatAnalyses = new List<VatAnalysis>();
            while (sdr0.Read())
            {
                VatAnalysis vatAnalysis = new VatAnalysis();
                vatAnalysis.goods = sdr0["total"] != DBNull.Value ? (decimal)sdr0["total"] : 0;
                vatAnalysis.vatcode = sdr0["vatperc"] != DBNull.Value ? (string)sdr0["vatperc"] : null;
                vatAnalysis.vat = sdr0["vat"] != DBNull.Value ? (decimal)sdr0["vat"] : 0;
                vatAnalyses.Add(vatAnalysis);
            }
            cnn.Close();
            return vatAnalyses;
        }
        public List<VatAnalysis> vatPurchaseanalysisByPeriod(string period_from, string period_to)
        {
            //SUBQUERY FOR BOTH INVOICE AND CREDIT NOTE
            string finalQuery = "select x.\"VatPerc\", coalesce(sum(vat), 0) as vat,  coalesce(sum(total), 0) as totalAmount " +
                "from(select b.\"VatPerc\", sum(b.\"VatAmt\") as vat, sum((b.\"UnitPrice\" * b.\"ProdQty\")) as total from \"PLInvoiceHeader\" a left join \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" left join \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"PLJrnlNo\" " +
                " where c.\"TranFrom\" = 'PL' and c.\"TranPeriod\" between '"+period_from.Split("/")[0]+"' and '"+period_to.Split("/")[0]+"' and c.\"TranYear\" between '"+period_from.Split("/")[1]+"' and '"+period_to.Split("/")[1]+"'  group by b.\"VatPerc\" union   " +
                "select a.\"CrnVatPercent\" as VatPerc, sum((a.\"CRNVat\") * -1) as vat, sum( (a.\"CRNTotal\" - a.\"CRNVat\") * -1) as total from \"PLInvoiceHeader\" a left join \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"PLJrnlNo\"" +
                " where c.\"TranFrom\" = 'PL-CRN' and c.\"TranPeriod\" between '"+period_from.Split("/")[0]+"' and '"+period_to.Split("/")[0]+"' and c.\"TranYear\" between '"+period_from.Split("/")[1]+"' and '"+period_to.Split("/")[1]+"'  group by VatPerc ) as x group by x.\"VatPerc\";   ";

            string queryComplete = "select x.\"VatPerc\", coalesce(sum(vat), 0) as vat,  coalesce(sum(total), 0) as totalAmount from(" +
                "select b.\"VatPerc\", sum(b.\"VatAmt\") as vat, sum(b.\"Total\" - b.\"VatAmt\") as total from \"PLInvoiceHeader\" a left join \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" left join \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"PLJrnlNo\"  " +
                "where c.\"TranFrom\" = 'PL' and c.\"TranPeriod\" between '"+period_from.Split("/")[0]+"' and '"+period_to.Split("/")[0]+"' and c.\"TranYear\" between '"+period_from.Split("/")[1]+"' and '"+period_to.Split("/")[1]+"' and b.\"VatAmt\" > 0  group by b.\"VatPerc\"  union      " +
                "select b.\"VatPerc\", sum(b.\"VatAmt\") as vat, sum(b.\"Total\" - b.\"VatAmt\") as total from \"PLInvoiceHeader\" a left join \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" left join \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"PLJrnlNo\"  " +
                "where c.\"TranFrom\" = 'PL' and c.\"TranPeriod\" between '"+period_from.Split("/")[0]+"' and '"+period_to.Split("/")[0]+"' and c.\"TranYear\" between '"+period_from.Split("/")[1]+"' and '"+period_to.Split("/")[1]+"' and b.\"VatAmt\" <= 0 group by b.\"VatPerc\"   union         " +
                "select a.\"CrnVatPercent\" as VatPerc, sum((a.\"CRNVat\") * -1) as vat, sum( (a.\"CRNTotal\" - a.\"CRNVat\") * -1) as total from \"PLInvoiceHeader\" a left join \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"PLJrnlNo\"     " +
                "where c.\"TranFrom\" = 'PL-CRN' and c.\"TranPeriod\" between '"+period_from.Split("/")[0]+"' and '"+period_to.Split("/")[0]+"' and c.\"TranYear\" between '"+period_from.Split("/")[1]+"' and '"+period_to.Split("/")[1]+"'  group by VatPerc ) as x group by x.\"VatPerc\";   ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<VatAnalysis> vatAnalyses = new List<VatAnalysis>();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(queryComplete, cnn).ExecuteReader();
            while (reader.Read())
            {
                VatAnalysis vat = new VatAnalysis();
                vat.vatcode = reader["VatPerc"] != DBNull.Value ? (string)reader["VatPerc"] : null;
                vat.vat = (reader["vat"] != DBNull.Value ? (decimal)reader["vat"] : 0);
                vat.goods = (reader["totalAmount"] != DBNull.Value ? (decimal)reader["totalAmount"] : 0);
                vatAnalyses.Add(vat);
            }
            cnn.Close();
             return vatAnalyses;
        }
    }
}
