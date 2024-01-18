using Npgsql;
using pyme_finance_api.Models.Dashboard;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.DashboardService
{

    public interface IDashboardService
    {
        public DahboardResponse getDashboardService();
    }
    public class DashboardService : IDashboardService
    {

        dbconnection myDbconnection = new dbconnection();

        public string OrganizationId { get; set; }


        public DashboardService(string organizationId)
        {
            OrganizationId = organizationId;
        }


        public DahboardResponse getDashboardService()
        {
            DahboardResponse dahboardResponse = new DahboardResponse();

            string slcustomercountquery = "SELECT count(*) FROM \"SLCustomer\" ;";

            string plcustomercountquery = "SELECT count(*) FROM \"PLCustomer\" ;";

            string branchescountquery = "SELECT count(*) FROM \"Branches\" ;";

          string inventorycountquery = "SELECT sum(\"InvtQty\") FROM \"Inventory\"  WHERE \"InvtType\" = 'GOODS'  AND \"InvtQty\" > 0 ;";

            string userscount = "SELECT count(*) FROM \"Users\" ;";

            string current_finacial_period = "SELECT * FROM \"financial_periods\" WHERE \"fp_active\" = '"+true+"' ";

            string settledinvoice = "SELECT sum(\"TotalAmount\") FROM \"SLInvoiceHeader\" WHERE \"TotalBalance\"  = 0  ;";
            string invoicedue = "Select \"count\"(*) FROM \"SLInvoiceHeader\" WHERE TO_CHAR(CURRENT_DATE, 'YYYY-MM-DD')::date > \"DueDate\"   AND  \"TotalBalance\"  >  0 AND \"HasCreditNote\" = '"+false+"' ; ";
            string pendinginvoice = "SELECT SUM(\"TotalAmount\" -  \"TotalBalance\"  ) FROM \"SLInvoiceHeader\" WHERE \"INVTypeRef\"  = 'INV' AND  \"TotalBalance\"  >  0 ;";
            string disputed = "SELECT SUM(\"TotalAmount\") FROM \"SLInvoiceHeader\" WHERE  \"INVTypeRef\"  = 'CRN' ;";

            string monthlypaymentsquery = "SELECT  TO_CHAR(a.\"TranDate\", 'Month') as period, sum(b.\"Dr\"),  a.\"TranDate\" FROM \"NlJournalHeader\" a " +
                "LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" " +
                " WHERE a.\"TranFrom\" = 'SL-PY' AND date_part('year', a.\"TranDate\") = date_part('year', now()) " +
                "GROUP BY  period,a.\"TranDate\" " +
                "ORDER BY a.\"TranDate\" ASC ;";


            string monthlysales = "SELECT  TO_CHAR(a.\"TranDate\", 'Month') period , sum(b.\"Dr\") as amount FROM \"NlJournalHeader\" a \r\n      " +
                "LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" \r\n                " +
                "WHERE a.\"TranFrom\" = 'SL' AND date_part('year', a.\"TranDate\") = date_part('year', now())      " +
                "  GROUP BY  a.\"TranDate\"      " +
                "     ORDER BY a.\"TranDate\" ASC ;";


            string monthlypurchases = "SELECT  TO_CHAR(a.\"TranDate\", 'Month') period , sum(b.\"Cr\") as amount FROM \"NlJournalHeader\" a " +
                " LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\"   WHERE a.\"TranFrom\" = 'PL' AND date_part('year', a.\"TranDate\") = date_part('year', now()) " +
                "              GROUP BY  a.\"TranDate\" " +
                "    ORDER BY a.\"TranDate\" ASC ;";


            string mostitempurchased = "SELECT SUM(b.\"ProdQty\") as units,b.\"ProdId\",c.\"InvtName\" ,SUM(b.\"UnitPrice\" * b.\"ProdQty\") as totals FROM \"PLInvoiceHeader\" a " +
                "LEFT JOIN \"PLInvoiceDetail\" b on a.\"PLJrnlNo\" = b.\"PLJrnlNo\" " +
                "LEFT JOIN \"Inventory\" c on c.\"InvtId\" = b.\"ProdId\" " +
                "WHERE A.\"InvReturned\" = FALSE " +
                "GROUP BY b.\"ProdId\",c.\"InvtName\"; ";
         




            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(slcustomercountquery, cnn).ExecuteReader();
            //// get All SLcustomers
            
            while (sdr0.Read())
            {
                dahboardResponse.totalsalescustomers = (int)(long)sdr0["count"];
            }
            cnn.Close();
            ///get All PlCUSTOMER
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(plcustomercountquery, cnn).ExecuteReader();
            
            while (sdr1.Read())
            {
                dahboardResponse.totalpurchasecustomers = (int)(long)sdr1["count"];
            }
            cnn.Close();
            ///

            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand(userscount, cnn).ExecuteReader();
       
            while (sdr2.Read())
            {
                dahboardResponse.totalUsers= (int)(long)sdr2["count"];
            }
            cnn.Close();

            string salesanalysis = "SELECT SUM( a.\"ItemTotals\") AS total,a.\"ItemId\",b.\"InvtName\",SUM(a.\"ItemQty\") as soldquantity " +
                "FROM \"SLInvoiceDetail\" a " +
                "LEFT JOIN \"Inventory\" b on b.\"InvtId\" = a.\"ItemId\" " +
                " LEFT JOIN \"SLInvoiceHeader\" c on c.\"SLJrnlNo\" = a.\"SLJrnlNo\" " +
                "WHERE date_part('year', c.\"TransDate\") = '"+DateTime.Now.Year+"'  " +
                " GROUP BY b.\"InvtName\",a.\"ItemId\" " +
                " ORDER BY soldquantity DESC " +
                "  LIMIT 10 ;";
            List<Salesanalysis> salesanalyses = new List<Salesanalysis>();
         
                cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand(salesanalysis, cnn).ExecuteReader();
        
            while (sdr3.Read())
            {
                Salesanalysis salesanalysis1 = new Salesanalysis();
                salesanalysis1.name = sdr3["InvtName"] != DBNull.Value ? (string)sdr3["InvtName"] : "UNDEFINDED"; ;
                salesanalysis1.soldquantity =  (int)(long)sdr3["soldquantity"] ;
                salesanalysis1.total = (decimal)sdr3["total"];
                salesanalyses.Add(salesanalysis1);
            }
            cnn.Close();

            dahboardResponse.salesanalysisdata = salesanalyses;

            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(inventorycountquery, cnn).ExecuteReader();

            while (sdr4.Read())
            {
                dahboardResponse.totalinventory = sdr4["sum"] != DBNull.Value ? (int)(long)sdr4["sum"]: 0; 
            }
            cnn.Close();


            cnn.Open();
            NpgsqlDataReader sdr5 = new NpgsqlCommand(branchescountquery, cnn).ExecuteReader();

            while (sdr5.Read())
            {
                dahboardResponse.totalbranches = (int)(long)sdr5["count"];
            }
            cnn.Close();


            List<MonthlyPayments> monthlyPayments = new List<MonthlyPayments>();

            cnn.Open();
            NpgsqlDataReader sdr6 = new NpgsqlCommand(monthlypaymentsquery, cnn).ExecuteReader();

            while (sdr6.Read())
            {
                MonthlyPayments monthpayment = new MonthlyPayments();
                monthpayment.month = sdr6["period"] != DBNull.Value ? (string)sdr6["period"] : "UNDEFINDED"; ;
                monthpayment.amount = (decimal)sdr6["sum"];
                monthlyPayments.Add(monthpayment);
            }
            cnn.Close();
        
            List<MonthlySales> monthlySaleslist = new List<MonthlySales>();
            cnn.Open();
            NpgsqlDataReader sdr21 = new NpgsqlCommand(monthlysales, cnn).ExecuteReader();

            while (sdr21.Read())
            {
                MonthlySales monthpayment = new MonthlySales();
                monthpayment.period = sdr21["period"] != DBNull.Value ? (string)sdr21["period"] : "UNDEFINDED"; ;
                monthpayment.amount = (decimal)sdr21["amount"];
                monthlySaleslist.Add(monthpayment);
            }
            cnn.Close();


            List<MonthlySales> monthlyPurchaseslist = new List<MonthlySales>();
            cnn.Open();
            NpgsqlDataReader sdr22 = new NpgsqlCommand(monthlypurchases, cnn).ExecuteReader();

            while (sdr22.Read())
            {
                MonthlySales monthpayment = new MonthlySales();
                monthpayment.period = sdr22["period"] != DBNull.Value ? (string)sdr22["period"] : "UNDEFINDED"; 
                monthpayment.amount = (decimal)sdr22["amount"];
                monthlyPurchaseslist.Add(monthpayment);
            }
            cnn.Close();

            List<InvoiceData> invoiceDatas = new List<InvoiceData>();
            cnn.Open();
            NpgsqlDataReader sdr7 = new NpgsqlCommand(settledinvoice, cnn).ExecuteReader();

            while (sdr7.Read())
            {

               decimal count = sdr7["sum"] != DBNull.Value ? (decimal)sdr7["sum"] : 0 ;
                InvoiceData invoiceData = new InvoiceData("Settled", count);
                invoiceDatas.Add(invoiceData);
            }
             cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr8 = new NpgsqlCommand(pendinginvoice, cnn).ExecuteReader();

            while (sdr8.Read())
            {

               decimal count = sdr8["sum"] != DBNull.Value ? (decimal)sdr8["sum"] : 0;
                InvoiceData invoiceData = new InvoiceData("Pending", count);
                invoiceDatas.Add(invoiceData);
            }
            cnn.Close();


            cnn.Open();


            NpgsqlDataReader sdr9 = new NpgsqlCommand(disputed, cnn).ExecuteReader();

            while (sdr9.Read())
            {

                decimal count = sdr9["sum"] != DBNull.Value ? (decimal)sdr9["sum"] : 0; ;
                InvoiceData invoiceData = new InvoiceData("Disputed", count);
                invoiceDatas.Add(invoiceData);
            }
            cnn.Close();


            cnn.Open();


            NpgsqlDataReader sdr10 = new NpgsqlCommand(mostitempurchased, cnn).ExecuteReader();
            List<Purchaseanalysis> purchaseanalyses = new List<Purchaseanalysis>();
            //while (sdr10.Read())
            //{
            //    Purchaseanalysis purchaseanalysis = new Purchaseanalysis();

            //    purchaseanalysis.name = (string)sdr10["InvtName"];

            //    purchaseanalysis.soldquantity = (int)(long)sdr10["units"];
            //   purchaseanalysis.total = (decimal)sdr10["totals"];

            //    purchaseanalyses.Add(purchaseanalysis);


            //}
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand(current_finacial_period, cnn).ExecuteReader();
            FinancialPeriod financialPeriod = new FinancialPeriod();
            while (sdr11.Read())
            {


                financialPeriod.fp_name = (string)sdr11["fp_name"];
                financialPeriod.fp_ref = (string)sdr11["fp_ref"];
                financialPeriod.fp_openingdate = (DateTime)sdr11["fp_openingdate"];
                financialPeriod.fp_closingdate = (DateTime)sdr11["fp_closingdate"];





            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr12 = new NpgsqlCommand(invoicedue, cnn).ExecuteReader();

            while (sdr12.Read())
            {
                dahboardResponse.totalinvoicedue = (int)(long)sdr12["count"];
            }
            cnn.Close();






            dahboardResponse.monthlySales = monthlySaleslist;
            dahboardResponse. monthlyPurchases = monthlyPurchaseslist;

            dahboardResponse.purchaseAnalysis = purchaseanalyses;
            dahboardResponse.invoiceDatas = invoiceDatas;
            dahboardResponse.financialPeriod = financialPeriod;


            dahboardResponse.monthlyPayments = monthlyPayments;


            return dahboardResponse;

        }
    }
}
