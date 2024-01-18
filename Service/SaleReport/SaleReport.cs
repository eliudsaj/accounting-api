using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.CustomerSalesLedger;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.SaleReport
{
    public interface ISaleReport
    {
        List<CustomerStatementModel> sendCusromerStatement(int cust_id);
        License GetCompanyDetails();
    }
    public class SaleReport : ISaleReport
    {
        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }
        public SaleReport(string organizationId)
        {
            OrganizationId = organizationId;
        }
        public List<CustomerStatementModel> sendCusromerStatement(int cust_id)
        {
            string trans_from = "SL";
            string debtors = "debtor";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //INVOICE QUERY
            string invoicequery = "SELECT b.\"NlJrnlNo\",a.\"SLDescription\", a.\"StatementDescription\", a.\"INVDate\" as invoicedate,  a.\"DocRef\" AS documentref,a.\"post_number\" ,a.\"origin_status\", a.\"HasCreditNote\",b.\"TranFrom\", concat ( a.\"INVTypeRef\", a.\"SLJrnlNo\" )  as docref,   " +
                " b.\"TranDate\",d.\"NlAccName\",c.\"Dr\",c.\"Cr\", a.\"TotalBalance\"   " +
                "FROM \"SLInvoiceHeader\" a LEFT JOIN \"NlJournalHeader\" b on b.\"SlJrnlNo\"  = a.\"SLJrnlNo\" LEFT JOIN \"NLJournalDetails\" c on c.\"JrnlSlNo\" = b.\"NlJrnlNo\" LEFT JOIN \"SLReceipts\" e on e.\"pyInvRef\" = a.\"SLJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = c.\"NlAccCode\"    " +
                "WHERE a.\"CustId\" = '"+cust_id+"' AND b.\"TranFrom\" = 'SL' and d.\"NlAccName\" ~* 'DEBTORS' and a.\"TotalBalance\"::numeric > 0 and a.\"IsReversed\" = 'f'   " +
                "GROUP BY c.\"Dr\",c.\"Cr\",invoicedate, documentref, a.\"HasCreditNote\", a.\"SLDescription\",a.\"StatementDescription\" ,b.\"TranFrom\",docref,b.\"TranDate\",d.\"NlAccName\",b.\"NlJrnlNo\",a.\"post_number\" ,a.\"origin_status\", a.\"TotalBalance\" ORDER BY b.\"NlJrnlNo\" ASC ; ";

            List<CustomerStatementModel> customerStatementModels = new List<CustomerStatementModel>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(invoicequery, cnn).ExecuteReader();         

            while (sdr0.Read())
            {
                CustomerStatementModel statementModel = new CustomerStatementModel();
                statementModel.Invoicedate = sdr0["invoicedate"] != DBNull.Value ? (DateTime)sdr0["invoicedate"] : DateTime.Today;
                //statementModel.Invoicetotalamount = sdr0["invoicetotalamount"] != DBNull.Value ? (decimal)sdr0["invoicetotalamount"] : 0;
                statementModel.Docref = sdr0["docref"] != DBNull.Value ? (string)sdr0["docref"] : null;
              // statementModel.DocumentRef = (int) sdr0["documentref"];
                //statementModel.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                statementModel.HasCreditNote = (bool)sdr0["HasCreditNote"];
                //statementModel.NlaccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : null;
                statementModel.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Today;
                statementModel.NlAccName = sdr0["NlAccName"] != DBNull.Value ? (string)sdr0["NlAccName"] : null;
                statementModel.paymentDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Today;
                statementModel.TranFrom = sdr0["TranFrom"] != DBNull.Value ? (string)sdr0["TranFrom"] :"";
                //statementModel.paymentId = sdr0["pyID"] != DBNull.Value ? (int)sdr0["pyID"] : 0;
                statementModel.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                statementModel.Dr = sdr0["TotalBalance"] != DBNull.Value ? (decimal)sdr0["TotalBalance"] : 0;
                statementModel.description = sdr0["SLDescription"] != DBNull.Value ? (string)sdr0["SLDescription"]:"";
                statementModel.journalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                statementModel.origin_status = (string)sdr0["origin_status"];
                statementModel.invoicenumber = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                statementModel.StatementDescription = sdr0["StatementDescription"] != DBNull.Value ? (string)sdr0["StatementDescription"] : "";
                customerStatementModels.Add(statementModel);
            }
            cnn.Close();
            var unallocatedpayment = getCustomerLedgerCardUnAllocatedpayments(cust_id);
            customerStatementModels.AddRange(unallocatedpayment);
            customerStatementModels.Sort((a, b) => a.journalId.CompareTo(b.journalId));
            return customerStatementModels; 
        }
        public List<CustomerStatementModel> getCustomerLedgerCardUnAllocatedpayments(int Ref)
        {
            //PAYMENT QUERY
            string paymentquery = "SELECT c.\"NlJrnlNo\",a.\"Cr\",a.\"Dr\" , receipts1.\"pyInvRef\", receipts1.\"pyProcessDate\", c.\"TranFrom\" , receipts1.\"pyChequeNumber\" as docref, receipts1.\"pyPaid\", receipts1.\"pyBalance\", receipts1.allocation_remainder    " +
                "FROM \"NLJournalDetails\" a LEFT JOIN \"NLAccount\" b ON b.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"NlJournalHeader\" c ON c.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLReceipts\" receipts1 ON receipts1.\"journal_id\" = c.\"NlJrnlNo\"     " +
                "WHERE receipts1.\"cust_id\" = '"+Ref+"' AND b.\"NlAccName\" ~* 'DEBTORS' and receipts1.\"allocation_remainder\"::numeric > 0 ORDER BY  c.\"NlJrnlNo\" ASC; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CustomerStatementModel> salesLedgers = new List<CustomerStatementModel>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(paymentquery, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                CustomerStatementModel unallocatedsalesledger = new CustomerStatementModel();
                unallocatedsalesledger.Cr = sdr0["allocation_remainder"] != DBNull.Value ? (decimal)sdr0["allocation_remainder"] : 0;
                unallocatedsalesledger.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                unallocatedsalesledger.Docref = sdr0["docref"] != DBNull.Value ? (string)sdr0["docref"] : null;
                //unallocatedsalesledger.SLJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                unallocatedsalesledger.TranFrom = sdr0["TranFrom"] != DBNull.Value ? (string)sdr0["TranFrom"] : "";
                unallocatedsalesledger.TranDate = (DateTime)sdr0["pyProcessDate"];
                unallocatedsalesledger.Invoicedate = (DateTime)sdr0["pyProcessDate"];
                unallocatedsalesledger.journalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                salesLedgers.Add(unallocatedsalesledger);
            }
            cnn.Close();
            return salesLedgers;
        }
        public List<AccountsReceivableAgeingReport> getAccountReceivableAgeingReport()
        {
            // string query = "select a.\"CustId\", a.\"CustCode\" ,CASE WHEN b.\"CustCompany\" = ''THEN  b.\"CustFirstName\" ELSE b.\"CustCompany\" END AS customer,\r\n( SELECT CAST (SUM(\"TotalBalance\") AS INTEGER)  from \"SLInvoiceHeader\"     WHERE  CURRENT_DATE -  \"TransDate\"::date > 91  AND \"TotalBalance\" >0 AND  \"CustId\" =  a.\"CustId\" AND \"TotalBalance\" != \"CreditNoteAmount\"  GROUP BY \"SLInvoiceHeader\".\"CustCode\" ) as \"Over90Days\", \r\n( SELECT CAST (SUM(\"TotalBalance\") AS INTEGER)  from \"SLInvoiceHeader\"  WHERE  ( CURRENT_DATE - \"TransDate\"::date) BETWEEN 61 AND 90 AND  \"CustId\" =  a.\"CustId\"\r\n\t\t\t\t  AND \"TotalBalance\" >0 AND \"TotalBalance\" != \"CreditNoteAmount\"\r\n\t\t\t\t \t\tGROUP BY \"SLInvoiceHeader\".\"CustCode\"\r\n\t\t\t\t ) as \"61-90 days\" \t,\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t  \r\n\t\t\t\t ( SELECT  CAST (SUM(\"TotalBalance\") AS INTEGER)\r\n\t\t\t\t  from \"SLInvoiceHeader\" \r\n\t\t\t\t  WHERE  ( CURRENT_DATE - \"TransDate\"::date) BETWEEN 31 AND 60  AND  \"CustId\" =  a.\"CustId\"   AND \"TotalBalance\" >0 AND \"TotalBalance\" != \"CreditNoteAmount\" AND \"TotalBalance\" != \"CreditNoteAmount\"  GROUP BY \"SLInvoiceHeader\".\"CustCode\") as \"31-60 days\",\r\n\t\t\t\t  ( SELECT CAST (SUM(\"TotalBalance\") AS INTEGER) from \"SLInvoiceHeader\"  WHERE  ( CURRENT_DATE - \"TransDate\"::date) BETWEEN 1 AND 30  AND   \"CustId\" =  a.\"CustId\" AND \"TotalBalance\" >0 AND \"TotalBalance\" != \"CreditNoteAmount\"   GROUP BY \"SLInvoiceHeader\".\"CustCode\")   as \"1-30days\",\r\n\t\t\t\t  ( SELECT  CAST (SUM(\"TotalBalance\") AS INTEGER) from \"SLInvoiceHeader\"   WHERE  CURRENT_DATE  = \"TransDate\"::date AND  \"CustId\" =  a.\"CustId\" GROUP BY \"SLInvoiceHeader\".\"CustCode\" ) as \"Current\" \r\n\t\t\t\t  from \"SLInvoiceHeader\" a \r\nLEFT join \"SLCustomer\" b  on b.\"CustCode\" = a.\"CustCode\" GROUP by  a.\"CustCode\",customer,a.\"CustId\";";

            string query2 = "select a.\"CustId\", a.\"CustCode\" ,CASE WHEN b.\"CustCompany\" = ''THEN  b.\"CustFirstName\" ELSE b.\"CustCompany\" END AS customer, ( SELECT CAST (SUM(\"TotalBalance\") AS INTEGER)    " +
                "from \"SLInvoiceHeader\" WHERE  CURRENT_DATE -  \"TransDate\"::date > 91  AND \"TotalBalance\" >0 AND  \"CustId\" =  a.\"CustId\" AND \"TotalBalance\" != \"CreditNoteAmount\"   ) as \"Over90Days\", ( SELECT CAST (SUM(\"TotalBalance\") AS INTEGER)  " +
                "from \"SLInvoiceHeader\"  WHERE  ( CURRENT_DATE - \"TransDate\"::date) BETWEEN 61 AND 90 AND  \"CustId\" =  a.\"CustId\"  AND \"TotalBalance\" >0 AND \"TotalBalance\" != \"CreditNoteAmount\" OR \"CreditNoteAmount\" = NULL ) as \"61-90 days\",  " +
                "( SELECT  CAST (SUM(\"TotalBalance\") AS INTEGER) from \"SLInvoiceHeader\" WHERE  ( CURRENT_DATE - \"TransDate\"::date) BETWEEN 31 AND 60  AND  \"CustId\" =  a.\"CustId\"   AND \"TotalBalance\" >0 OR \"TotalBalance\" != \"CreditNoteAmount\"  ) as \"31-60 days\"," +
                " ( SELECT CAST (SUM(\"TotalBalance\") AS INTEGER) from \"SLInvoiceHeader\"  WHERE  ( CURRENT_DATE - \"TransDate\"::date) BETWEEN 1 AND 30  AND   \"CustId\" =  a.\"CustId\" AND \"TotalBalance\" >0  OR \"TotalBalance\" != \"CreditNoteAmount\")   as \"1-30days\",   " +
                "( SELECT  CAST (SUM(\"TotalBalance\") AS INTEGER) from \"SLInvoiceHeader\"   WHERE  CURRENT_DATE  = \"TransDate\"::date AND  \"CustId\" =  a.\"CustId\" ) as \"Current\" from \"SLInvoiceHeader\" a LEFT join \"SLCustomer\" b  on b.\"CustCode\" = a.\"CustCode\"   " +
                "GROUP by  a.\"CustCode\",customer,a.\"CustId\";";

            string query3 = "SELECT main.\"CustCode\", main.\"customer\",SUM(main.\"61-90 days\") as \"61-90 days\" , SUM(main.\"31-60 days\") as \"31-60 days\" , SUM(main.current) as Current, SUM(main.\"Over90Days\") as \"Over90Days\" FROM " +
                "(SELECT \"SLCustomer\".\"CustCode\",CASE WHEN \"SLCustomer\".\"CustCompany\" = ''THEN  \"SLCustomer\".\"CustFirstName\" ELSE \"SLCustomer\".\"CustCompany\" END AS customer,   " +
                "CASE WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) BETWEEN 0 and 30 THEN  SUM(\"TotalBalance\") ELSE  0 END AS current,   " +
                "CASE WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) BETWEEN 31 and 60 THEN  SUM(\"TotalBalance\") ELSE  0 END AS \"31-60 days\",    " +
                "CASE WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) BETWEEN 61 and 90 THEN  SUM(\"TotalBalance\") ELSE  0 END AS \"61-90 days\", " +
                "CASE WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) > 90 THEN  SUM(\"TotalBalance\") ELSE  0 END AS \"Over90Days\" FROM \"SLInvoiceHeader\" LEFT JOIN \"SLCustomer\" on \"SLCustomer\".\"CustCode\" = \"SLInvoiceHeader\".\"CustCode\"   " +
                "GROUP by  \"TransDate\",\"SLCustomer\".\"CustCode\",customer) main GROUP BY main.\"CustCode\", main.\"customer\"  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query3, cnn).ExecuteReader();
            List<AccountsReceivableAgeingReport> accountsreceivableagingreport = new List<AccountsReceivableAgeingReport>();
            while (sdr0.Read())
            {
                AccountsReceivableAgeingReport ageingreport = new AccountsReceivableAgeingReport();
                ageingreport.CustCode = sdr0["CustCode"] != DBNull.Value ? (string)sdr0["CustCode"] : "";
                ageingreport.customer = sdr0["customer"] != DBNull.Value ? (string)sdr0["customer"] : "";
                ageingreport.Over90Days = sdr0["Over90Days"] != DBNull.Value ? (Decimal)sdr0["Over90Days"] : 0;
                ageingreport.sixtoneto90days = sdr0["61-90 days"] != DBNull.Value ? (Decimal)sdr0["61-90 days"] : 0;
                ageingreport.thirtyoneto60days = sdr0["31-60 days"] != DBNull.Value ? (Decimal)sdr0["31-60 days"] : 0;
                //ageingreport.oneto30days = sdr0["1-30days"] != DBNull.Value ? (int)sdr0["1-30days"] : 0;
                ageingreport.Current = sdr0["Current"] != DBNull.Value ? (Decimal)sdr0["Current"] : 0;
                accountsreceivableagingreport.Add(ageingreport);
            }
            cnn.Close();
            return accountsreceivableagingreport;
        }
        public List<DetailedAccountsReceivableAgeingReport> detaileduserAgeingReport(string userId)
        {
            List< DetailedAccountsReceivableAgeingReport > detailedAccountsReceivableAgeingReports = new List<DetailedAccountsReceivableAgeingReport> ();   
            string query = $"SELECT * FROM (SELECT \"SLInvoiceHeader\".\"TransDate\",\"SLInvoiceHeader\".\"CustId\",\"SLInvoiceHeader\".\"DueDate\",\"SLInvoiceHeader\".\"TotalBalance\",\"SLInvoiceHeader\".\"TotalAmount\",   " +
                $"CASE WHEN (\"SLInvoiceHeader\".\"CreditNoteAmount\" IS NULL) THEN '0' ELSE \"SLInvoiceHeader\".\"CreditNoteAmount\" END as cramount ,concat('INV',\"SLInvoiceHeader\".\"SLJrnlNo\") as invoice,    " +
                $"CASE WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) BETWEEN 0 and 30 THEN  'current' WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) BETWEEN 31 and 60 THEN  '31-60 days' WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) BETWEEN 61 and 90 THEN  '61-90 days' WHEN extract(day from CURRENT_DATE - \"TransDate\"::timestamp) >90 THEN  'Over90Days' ELSE  '' END AS timeframe FROM \"SLInvoiceHeader\" WHERE \"SLInvoiceHeader\".\"CustCode\" = 'CUST0009' ) as main WHERE main.\"TotalAmount\" != main.\"cramount\"  ; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                DetailedAccountsReceivableAgeingReport ageingreport = new DetailedAccountsReceivableAgeingReport();
                ageingreport.DueDate = (DateTime)sdr0["DueDate"];
                ageingreport.TimeFrame = (string)sdr0["timeframe"] ;
                ageingreport.Balance = (decimal)sdr0["TotalBalance"];
                ageingreport.Invoice = (string)sdr0["invoice"];
                detailedAccountsReceivableAgeingReports.Add(ageingreport);
            }
            cnn.Close();
            return detailedAccountsReceivableAgeingReports;
        }
        public License GetCompanyDetails()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            License lic = new License();
            cnn.Open();
            string query = "Select * From \"Licence\" LIMIT 1; ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                lic.CompanyName = (string)sdr1["CompanyName"];
                lic.CompanySlogan = (string)sdr1["CompanySlogan"];
                lic.CompanyPostal = (string)sdr1["CompanyPostal"];
                lic.CompanyContact = (string)sdr1["CompanyContact"];
                lic.CompanyVAT = (string)sdr1["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr1["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr1["CompanyLogo"];
            }
            cnn.Close();
            return lic;
        }
    }
}
