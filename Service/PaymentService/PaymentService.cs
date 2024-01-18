using Microsoft.VisualBasic;
using Npgsql;
using Org.BouncyCastle.Ocsp;
using pyme_finance_api.Common;
using pyme_finance_api.Controllers.NlController;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Purchases.Customers;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.PlService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.PaymentService
{
    public interface IPaymentsService
    {
        Task<MyResponse> allocatedInvoicesV3(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch);
    }
    public class PaymentService : IPaymentsService
    {
        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }
        public PaymentService(string organizationId)
        {
            OrganizationId = organizationId;
        }
        //public MyResponse savepayment(ReceivePaymentRequest receivePaymentRequest ,int userId,int staff_branch)
        //{
        //    MyResponse response = new MyResponse();
        //    ///check if there was allocation of this receipt
        //    if (receivePaymentRequest.TotalAllocated == 0)
        //    {
        //        ///NOT ALLOCATED
        //        ///


        //        NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

        //        //get last pyid
        //        int lastpyid = 0;
        //        cnn.Open();
        //        NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyID\"), 0) as ref From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
        //        while (sdra.Read())
        //        {
        //            lastpyid = (int)sdra["ref"];

        //        }
        //        cnn.Close();

        //        cnn.Open();
        //        ///UPDATE CUSTOMER RUNNIG BALANCE
        //        AddCustomer addCust = new AddCustomer();
        //        string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";


        //        decimal cust_running_balance = 0;
        //        NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
        //        while (sdr4.Read())
        //        {
        //            addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
        //            addCust.CustCode = sdr4["CustCode"].ToString();
        //            addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;

        //        }
        //        cnn.Close();

        //        cust_running_balance = addCust.CustomerDept - receivePaymentRequest.Amount;

        //        string update_customer_balance = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + cust_running_balance + "' WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "'; ";



        //        cnn.OpenAsync();
        //        int id;
        //        using (var trans = cnn.BeginTransaction())
        //        {

        //            try
        //            {
        //                string status = "NOT ALLOCATED";
        //                //save details

        //                //// save in JOURNAL
        //                ///

        //                NlJournalHeader nlJournalHeader = new NlJournalHeader();

        //                nlJournalHeader.NlJrnlDesc = "Received payment";
        //                nlJournalHeader.TranDate = DateTime.Now;
        //                nlJournalHeader.MEndDate = DateTime.Now;
        //                nlJournalHeader.TranYear = DateTime.Now.Year;
        //                nlJournalHeader.TranPeriod = DateTime.Now.Month;
        //                nlJournalHeader.TranType = "NT";
        //                nlJournalHeader.TranFrom = "SL-PY";
        //                nlJournalHeader.ModuleId = null;
        //                nlJournalHeader.SlJrnlNo = 0;


        //                NlService nlService = new NlService(OrganizationId);


        //                long insertedId = 0L;
        //                string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
        //                "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";


        //                var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);

        //                id = int.Parse(cmd.ExecuteScalar().ToString());


        //                /// credit payment that is not allocated
        //                string creditentry1 = "INSERT INTO \"NLJournalDetails\" " +
        //              "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
        //              "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
        //              "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.PaymentMethod.ToUpper()).NlaccCode + "','0', '" + receivePaymentRequest.Amount + "', '" + (receivePaymentRequest.Amount) + "', " +
        //              "'Received payment without allocation','','false','','false','false','" + 0 + "');";



        //                string insertQuery = "INSERT INTO \"SLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", " +
        //                    "\"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\" ,\"journal_id\") " +
        //                    "VALUES(" + (lastpyid + 1) + ", " + (lastpyid + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + 0 + ", " + 0 + ", " + receivePaymentRequest.Amount + "," + 0 + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + " ,"+id+"); ";




        //               // AddCustomer addCust = new AddCustomer();
        //               // string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";


        //               // decimal cust_running_balance = 0;
        //               // NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
        //               // while (sdr4.Read())
        //               // {
        //               //     addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
        //               //     addCust.CustCode = sdr4["CustCode"].ToString();
        //               //     addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;

        //               // }


        //               // cust_running_balance = addCust.CustomerDept - receivePaymentRequest.Amount;

        //               //string update_customer_balance = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + cust_running_balance + "' WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "'; ";



        //                cmd.CommandText = creditentry1 +insertQuery+update_customer_balance;


        //                cmd.ExecuteNonQuery();
        //                trans.Commit();
        //                response.Httpcode = 200;
        //                response.Message = "success";


        //            }
        //            catch(Exception E)
        //            {
        //                throw E;
        //                trans.Rollback();
        //                response.Httpcode = 400;
        //                response.Message = "";                    
        //            }
        //            cnn.CloseAsync();

        //        }


        //    }
        //    else
        //    {
        //        //ALLOCATED INVOICE 
        //     response =    allocatedInvoices(receivePaymentRequest, userId, staff_branch);


        //    }


        //    return response;
        //}














        //    public MyResponse allocatedInvoices(ReceivePaymentRequest receivePaymentRequest,int userId, int staff_branch)
        //{
        //    // settle all invoices allocated

        //    // add payments to receipts

        //    var myalllocatedinvoices = receivePaymentRequest.AllocatedInvoices.Where(x => x.AR == x.Balance).ToList();

        //    StringBuilder updateinvoicequery = new StringBuilder();
        //    StringBuilder receiptsquery = new StringBuilder();
        //    StringBuilder journalheaderquery = new StringBuilder();
        //    string status = "allocated";
        //    bool hasunallocatedquery = false;
        //    string unallocatedbalancequery ="";
        //    MyResponse myResponse = new MyResponse();





        //    NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));



        //    //get last pyid
        //    int lastpyid = 0;
        //    int lastref = 0;
        //    decimal unallocatedbalance = receivePaymentRequest.Amount - receivePaymentRequest.TotalAllocated;
        //    cnn.OpenAsync();
        //    //var trans = cnn.BeginTransaction();


        //    try {

        //            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyRef\"), 0) as ref From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
        //            while (sdra.Read())
        //            {
        //                lastref = (int)sdra["ref"];

        //            }
        //           cnn.Close();

        //        foreach (var data in myalllocatedinvoices)
        //        {

        //            //if ar == balance then its settled
        //            if (data.AR == data.Balance)
        //            {
        //                updateinvoicequery.Append("UPDATE \"SLInvoiceHeader\" SET \"TotalBalance\" =  " + 0 + "  WHERE \"SLJrnlNo\" = " + data.InvoiceRef + "; ");


        //                receiptsquery.Append("INSERT INTO \"SLReceipts\" ( \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\" ) " +
        //                    "VALUES(" + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + data.InvoiceRef + ", " + data.AR + ", " + data.AR + "," + 0 + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + " ); ");

        //                NlJournalHeader nlJournalHeader = new NlJournalHeader();

        //                nlJournalHeader.NlJrnlDesc = "allocating payment";
        //                nlJournalHeader.TranDate = DateTime.Now;
        //                nlJournalHeader.MEndDate = DateTime.Now;
        //                nlJournalHeader.TranYear = DateTime.Now.Year;
        //                nlJournalHeader.TranPeriod = DateTime.Now.Month;
        //                nlJournalHeader.TranType = "NT";
        //                nlJournalHeader.TranFrom = "SL-PY";
        //                nlJournalHeader.ModuleId = null;
        //                nlJournalHeader.SlJrnlNo = data.InvoiceRef;

        //                /// create journal header
        //                journalheaderquery.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
        //               "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' )  ;");




        //            }
        //        }
        //        string query =  journalheaderquery.ToString() + receiptsquery.ToString()+ updateinvoicequery.ToString() ;
        //        cnn.OpenAsync();
        //         //int status_response =    saveJournalAndReceipt(journalheaderquery.ToString(), receiptsquery.ToString(), updateinvoicequery.ToString() ,cnn).Result;



        //        using (var trans = cnn.BeginTransaction())
        //        {

        //            try
        //            {

        //                var cmd = new NpgsqlCommand(null, cnn, trans);
        //                cmd.CommandText = query;

        //                cmd.ExecuteNonQuery();

        //                trans.Commit();
        //                // trans.Dispose();
        //                //return 1;
        //                ////GO TO UPDATE THE NL DETAILS


        //            }
        //            catch (Exception ex)
        //            {

        //                trans.Rollback();
        //                trans.Dispose();
        //                throw ex;
        //              //  return 0;

        //            }





        //        }
        //        cnn.Close();




        //        //if (status_response == 0)
        //        //{
        //        //    myResponse.Message = "Error,please try again later";
        //        //    myResponse.Httpcode = 400;

        //        //    return myResponse;


        //        //}
        //        //else
        //        //{


        //            // debit method of payment - Done

        //            ///credit debtors account - Done


        //            //reduce customer running balance - Done

        //            decimal zero = 0;
        //            int receiptref = lastref + 1;

        //            if (unallocatedbalance > zero)
        //            {

        //                string unallocatedstatus = "NOT ALLOCATED";
        //                unallocatedbalancequery = "INSERT INTO \"SLReceipts\" (\"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\" ) " +
        //                    "VALUES( " + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + 0 + ", " + 0 + ", " + unallocatedbalance + "," + 0 + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + unallocatedstatus + "'," + receivePaymentRequest.CustId + " ); ";

        //            }
        //            int response = updatenlDetails(myalllocatedinvoices, receivePaymentRequest.PaymentMethod, receivePaymentRequest.CustId, unallocatedbalance, receiptref, staff_branch, receivePaymentRequest.CurrenctCustomerName, receivePaymentRequest.ReceivedFrom, receivePaymentRequest.ChequeNumber, userId, receivePaymentRequest.AdditionalDetails);

        //            if (response == 0)
        //            {
        //                //  trans.Rollback();
        //                myResponse.Message = "Error,please try again later";
        //                myResponse.Httpcode = 400;
        //            }
        //            else
        //            {
        //                myResponse.Message = "Success";
        //                myResponse.Httpcode = 200;
        //            }
        //       // }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(Environment.StackTrace);
        //        throw ex;
        //        myResponse.Message = "Error,please try again later";
        //        myResponse.Httpcode = 400;
        //        return myResponse;
        //    }
        //    return myResponse;
        //}
        public async Task<MyResponse> allocatedPurchaseInvoicesV4(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();
            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            string insertQuery = "";
            ///GET RECEIPT DETAILS
            int lastref = 0;
            string query = "";
            MyResponse myResponse = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NlService nlService = new NlService(OrganizationId);
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;
            //get last pyid
            int last_pyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];
            }
            cnn.Close();
            cnn.OpenAsync();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices)
                    {
                        id++;
                        nlJournalHeader.NlJrnlNo = id;
                        nlJournalHeader.NlJrnlDesc = "allocating purchase payment";
                        nlJournalHeader.TranDate = DateTime.Now;
                        nlJournalHeader.MEndDate = DateTime.Now;
                        nlJournalHeader.TranYear = DateTime.Now.Year;
                        nlJournalHeader.TranPeriod = DateTime.Now.Month;
                        nlJournalHeader.TranType = "NT";
                        nlJournalHeader.TranFrom = "PY-PY";
                        nlJournalHeader.ModuleId = null;
                        nlJournalHeader.PlJrnlNo = invoice.InvoiceRef;

                        nljournalHeaderQuery.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                            "VALUES('" + nlJournalHeader.NlJrnlNo + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', " +
                            " '" + nlJournalHeader.TranYear + "','PL-PY','NT','" + 0 + "'," + nlJournalHeader.PlJrnlNo + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\"  ");

                      //  decimal amounttopay = Math.Min(invoice.Balance, paymentAmount);
                        decimal invoiceremainder = invoice.Balance - invoice.ALLOCATE;
                        string status = "";
                        ///query1
                        updateinvoicequery.Append("UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  " + invoiceremainder + "  WHERE \"PLJrnlNo\" = " + invoice.InvoiceRef + "; ");

                        insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\", \"rate\" )" +
                           " VALUES(" + (last_pyid + 1) + ", " + (last_pyid + 1) + ", '" + DateTime.Now + "', " + invoice.InvoiceRef + ", " + invoice.Balance + ", " + invoice.ALLOCATE + "," + invoiceremainder + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "'," +
                           " '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ", "+ receivePaymentRequest.rate +" ); ";

                        if (receivePaymentRequest.PaymentMethod == "CASH")
                        {
                            crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")  " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + invoice.ALLOCATE + "', '" + invoice.ALLOCATE + "', 'payment by cash','','false','','false','false','" + 0 + "');");
                        }
                        else
                        {
                            crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")  " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + invoice.ALLOCATE + "', '" + invoice.ALLOCATE + "', 'payment by cheque','','false','','false','false','" + 0 + "');");
                        }
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + invoice.ALLOCATE + "', '0', '" + (invoice.ALLOCATE) + "', 'payment by cash','','false','','false','false','" + 0 + "');");
                         //paymentAmount -= amounttopay;
                        ///if payment amount is negative break the code and this means the customer owes us money
                        //if (paymentAmount <= 0) break;
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    //// CHECK IF THERE IS A REMAINDER
                    ///
                    //if (remainingBalance < 0)
                    //{
                    //    drnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
                    //    "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                    //    "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                    //    "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.PaymentMethod.ToUpper()).NlaccCode + "','" + receivePaymentRequest.Amount + "', '0', '" + (receivePaymentRequest.Amount) + "', " +
                    //    "'ammount remained from purchase invoice payment','','false','','false','false','" + 0 + "');");
                    //}
                    query = nljournalHeaderQuery.ToString() + insertQuery + updateinvoicequery.ToString() + receiptsquery.ToString() + drnldetails.ToString() + crnldetails.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            cnn.CloseAsync();
            return null;
        }
        public async Task<MyResponse> allocatedPurchaseInvoicesV2(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();
            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            string insertQuery = "";
            ///GET RECEIPT DETAILS
            int lastref = 0;
            string query = "";
            MyResponse myResponse = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NlService nlService = new NlService(OrganizationId);
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;     

            //get last pyid
            int last_pyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];
            }
            cnn.Close();
            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;  
                    var cmd = new NpgsqlCommand("", cnn, trans); 
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x => x.Balance).ToList())
                    {
                        id++;
                        nlJournalHeader.NlJrnlNo = id;
                        nlJournalHeader.NlJrnlDesc = "allocating purchase payment";
                        nlJournalHeader.TranDate = DateTime.Now;
                        nlJournalHeader.MEndDate = DateTime.Now;
                        nlJournalHeader.TranYear = DateTime.Now.Year;
                        nlJournalHeader.TranPeriod = DateTime.Now.Month;
                        nlJournalHeader.TranType = "NT";
                        nlJournalHeader.TranFrom = "PY-PY";
                        nlJournalHeader.ModuleId = null;
                        nlJournalHeader.PlJrnlNo = invoice.InvoiceRef;

                        nljournalHeaderQuery.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                            "VALUES('" + nlJournalHeader.NlJrnlNo + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','PL-PY','NT','" + 0 + "'," + nlJournalHeader.PlJrnlNo + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;");

                        decimal amounttopay = Math.Min(invoice.Balance, paymentAmount);
                        decimal invoiceremainder = invoice.Balance - amounttopay;
                        string status = "";
                        ///query1
                        updateinvoicequery.Append("UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  " + invoiceremainder + "  WHERE \"PLJrnlNo\" = " + invoice.InvoiceRef + "; ");

                         insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\",\"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\", \"rate\",\"supplier_id\", \"allocation_remainder\" )" +
                            " VALUES(" + (last_pyid + 1) + ", " + (last_pyid + 1) + ", '" + DateTime.Now + "', " + invoice.InvoiceRef + ", " + invoice.Balance + ", " + amounttopay + "," + invoiceremainder + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "'," +
                            " '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ", '"+ receivePaymentRequest.rate +"', '"+ receivePaymentRequest.CustId +"', '"+ invoiceremainder +"' ); ";

                        if (receivePaymentRequest.PaymentMethod == "CASH")
                        {
                            crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                                "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount + "', '" + receivePaymentRequest.Amount + "', 'payment by cash','','false','','false','false','" + 0 + "');");
                        }
                        else
                        {
                            crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount + "', '" + receivePaymentRequest.Amount + "', 'payment by cheque','','false','','false','false','" + 0 + "');");
                        }
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + receivePaymentRequest.Amount + "', '0', '" + (receivePaymentRequest.Amount) + "', 'payment by cash','','false','','false','false','" + 0 + "');");
                        paymentAmount -= amounttopay;
                        /// if payment amount is negative break the code and this means the customer owes us money
                        if (paymentAmount <= 0) break;
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;                
                    //// CHECK IF THERE IS A REMAINDER
                    ///
                    //if (remainingBalance < 0)
                    //{
                    //    drnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
                    //    "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                    //    "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                    //    "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.PaymentMethod.ToUpper()).NlaccCode + "','" + receivePaymentRequest.Amount + "', '0', '" + (receivePaymentRequest.Amount) + "', " +
                    //    "'ammount remained from purchase invoice payment','','false','','false','false','" + 0 + "');");
                    //}
                    query = nljournalHeaderQuery.ToString() + insertQuery + updateinvoicequery.ToString() + receiptsquery.ToString() + drnldetails.ToString() + crnldetails.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    return myResponse;
                }
                catch (Exception ex) {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            cnn.Close();
            return null;
        }
        public async Task<MyResponse> allocatedInvoicesV4(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();
            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            MyResponse myResponse = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NlService nlService = new NlService(OrganizationId);
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;
            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();
            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";
            string status = "allocated";
            string query = "";
            ///GET RECEIPT DETAILS
            int lastref = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyRef\"), 0) as ref From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastref = (int)sdra["ref"];
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];
            }
            cnn.Close();
            decimal cust_running_balance = 0;

            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;
            }
            cnn.Close();
            ////OPTIMIZE CODE
            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    //id = int.Parse(cmd.ExecuteScalar().ToString());
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x => x.Balance).ToList())
                    {
                        id++;
                        nlJournalHeader.NlJrnlNo = id;
                        nlJournalHeader.NlJrnlDesc = "allocating payment";
                        nlJournalHeader.TranDate = DateTime.Now;
                        nlJournalHeader.MEndDate = DateTime.Now;
                        nlJournalHeader.TranYear = DateTime.Now.Year;
                        nlJournalHeader.TranPeriod = DateTime.Now.Month;
                        nlJournalHeader.TranType = "NT";
                        nlJournalHeader.TranFrom = "SL-PY";
                        nlJournalHeader.ModuleId = null;
                        nlJournalHeader.SlJrnlNo = invoice.InvoiceRef;

                        string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                        "VALUES('" + id + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT'," +
                        " '" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ; ";
                        nljournalHeaderQuery.Append(insertQuery1);                
                        ///query1
                        updateinvoicequery.Append("UPDATE \"SLInvoiceHeader\" SET \"TotalBalance\" =  \"TotalBalance\"  - " + invoice.AR + "  WHERE \"SLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        //query2
                        receiptsquery.Append("INSERT INTO \"SLReceipts\" ( \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", " +
                            "\"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\",\"rate\" ) " +
                                   "VALUES(" + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + invoice.InvoiceRef + ", " + invoice.Balance + ", " + invoice.AR + "," + (invoice.Balance - invoice.AR) + "," +
                                   " '" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', " +
                                   " '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + ", '"+receivePaymentRequest.rate+"' ); ");
                         
                        if (receivePaymentRequest.PaymentMethod == "CASH")
                        {
                            drnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
                             "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                             "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CASH").NlaccCode + "','" + invoice.AR + "', '0', '" + (invoice.AR) + "', " +
                             "'payment by cash','','false','','false','false','" + 0 + "');");
                        }
                        else
                        {
                            drnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
                             "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                             "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + invoice.AR + "', '0', '" + (invoice.AR) + "', " +
                             "'payment by cheque','','false','','false','false','" + 0 + "');");
                        }
                        /// credit debtors
                        crnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
                          "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                          "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                          "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + invoice.AR + "', '" + (invoice.AR) + "', " +
                          "'debt reduced','','false','','false','false','" + 0 + "');");
                        /// if payment amount is negative break the code and this means the customer owes us money
                        if (paymentAmount <= 0) break;
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    ////// update customer balance
                    ///USE CASE 1 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID 10,000 THEN REMAINING BALANCE IS 5000 
                    /// SO SINCE CUSTOMER RUNNING BALANCE IN THIS CASE IS 15000 WE DEDUCT 10000 MAKING CURRENT BALANCE IS 5000
                    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// USE CASE 2    /////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID IS 16,000 THEN REMAINING BALANCE IS -1000
                    /// SO SINCE THE CUSTOMER OVER PAID MAKING CURRENT BALANCE BE 15000-16000 = -1000
                    /// 
                    ////GET SUM AR AMOUNT TO MINUS THE CURRENT AMOUNT

                    decimal sum = receivePaymentRequest.AllocatedInvoices.Sum(A => A.AR);
                    decimal total_diff = receivePaymentRequest.Amount  - sum;
                    cust_running_balance = addCust.CustomerDept - total_diff;
                    //query3
                    string update_customer_balance = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + cust_running_balance + "' WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "'; ";

                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString() + update_customer_balance + drnldetails.ToString() + crnldetails.ToString() + updateinvoicequery.ToString();

                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }                
            }
            cnn.Close();
            return myResponse;
        }
        public async Task<MyResponse> allocatedPurchaseInvoicesV3(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();
            string drnldetails = "";
            string crnldetails = "";
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            StringBuilder nljournalHeaderQuery1 = new StringBuilder();
            string insertQuery = "";

            ///GET RECEIPT DETAILS
            int lastref = 0;
            string query = "";
            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;
            //get last pyid
            int last_pyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];
            }
            cnn.Close();
            cnn.OpenAsync();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    int period_month = Int32.Parse(receivePaymentRequest.Period.Split("/")[0]);
                    int period_year = Int32.Parse(receivePaymentRequest.Period.Split("/")[1]);
                    //decimal total_amount_received = receivePaymentRequest.AllocatedInvoices.Sum(x => x.AR);
                    //string reference;
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x=>x.Balance).ToList())
                    {
                        if (invoice.AR > 0)
                        {
                            id++;
                            nlJournalHeader.NlJrnlNo = id;
                            nlJournalHeader.NlJrnlDesc = "allocating purchase payment";
                            nlJournalHeader.TranDate = DateTime.Now;
                            nlJournalHeader.MEndDate = DateTime.Now;
                            nlJournalHeader.TranYear = period_year;
                            nlJournalHeader.TranPeriod = period_month;
                            nlJournalHeader.TranType = "NT";
                            nlJournalHeader.TranFrom = "PL-PY";
                            nlJournalHeader.ModuleId = null;
                            nlJournalHeader.PlJrnlNo = last_pyid + 1;

                            decimal payable = invoice.Balance;
                            decimal paid = receivePaymentRequest.Amount;
                            decimal pyBalance = payable - paid;                            

                            nljournalHeaderQuery.Append("INSERT INTO \"PLReceiptsDetails\" (\"invoice_id\",\"allocated_amount\",\"allocated_by\",\"allocated_on\",\"receipt_id\",\"invoice_balance\")   " +
                                "VALUES ('" + invoice.InvoiceRef + "','" + invoice.AR + "','" + userId + "','" + DateTime.Now + "','" + (last_pyid + 1) + "','" + invoice.Balance + "'); ");

                            insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\",  \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"rate\",\"supplier_id\" )" +
                                " VALUES(" + (last_pyid + 1) + ", " + (last_pyid + 1) + ", '" + DateTime.Now + "',  " + invoice.Balance + ", " + receivePaymentRequest.Amount + "," + (invoice.Balance - receivePaymentRequest.Amount) + ",'" + receivePaymentRequest.PaymentMethod + "',  " +
                                " '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + "," + receivePaymentRequest.rate + " ," + receivePaymentRequest.CustId + "); ";

                            nljournalHeaderQuery1.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                                "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','PL-PY','NT','" + 0 + "'," + nlJournalHeader.PlJrnlNo + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;");

                            //  decimal amounttopay = Math.Min(invoice.Balance, paymentAmount);
                            decimal invoiceremainder = invoice.Balance - invoice.AR;
                            string status = "";
                            ///query1
                            updateinvoicequery.Append("UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  " + invoiceremainder + "  WHERE \"PLJrnlNo\" = " + invoice.InvoiceRef + "; ");

                            if (receivePaymentRequest.PaymentMethod == "CASH")
                            {
                                crnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                                    "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + invoice.AR * receivePaymentRequest.rate + "', '" + invoice.AR * receivePaymentRequest.rate + "', 'payment by cash','','false','','false','false','" + 0 + "');";
                            }
                            else
                            {
                                crnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                                 "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + invoice.AR * receivePaymentRequest.rate + "', '" + invoice.AR * receivePaymentRequest.rate + "','payment by cheque','','false','','false','false','" + 0 + "');";
                            }
                            drnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + invoice.AR * receivePaymentRequest.rate + "', '0', '" + (invoice.AR * receivePaymentRequest.rate) + "', 'reduce creditors account','','false','','false','false','" + 0 + "');";
                        }
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    query = nljournalHeaderQuery.ToString() + nljournalHeaderQuery1 + insertQuery + updateinvoicequery.ToString() + receiptsquery.ToString() + drnldetails + crnldetails;
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    myResponse.Id = (last_pyid + 1);
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            cnn.CloseAsync();
            return null;
        }
        public async Task<MyResponse> allocatedInvoicesV3(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();

            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;
            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();
            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";
            string status = "allocated";
            string query = "";
            ///GET RECEIPT DETAILS
            int lastref = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyID\"), 0) as pyID From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastref = (int)sdra["pyID"];
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];

            }
            cnn.Close();
            decimal cust_running_balance = 0;
            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;
            }
            cnn.Close();
            ////OPTIMIZE CODE

            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    int period_year = Int32.Parse(receivePaymentRequest.Period.Split("/")[1]);
                    int period_month = Int32.Parse(receivePaymentRequest.Period.Split("/")[0]);
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x => x.Balance).ToList())
                    {
                        id++;
                        nlJournalHeader.NlJrnlNo = id;
                        nlJournalHeader.NlJrnlDesc = "allocating payment";
                        nlJournalHeader.TranDate = DateTime.Now;
                        nlJournalHeader.MEndDate = DateTime.Now;
                        nlJournalHeader.TranYear = period_year;
                        nlJournalHeader.TranPeriod = period_month;
                        nlJournalHeader.TranType = "NT";
                        nlJournalHeader.TranFrom = "SL-PY";
                        nlJournalHeader.ModuleId = null;
                        nlJournalHeader.SlJrnlNo = (lastref + 1);
                        string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                        "VALUES('" + id + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";

                        nljournalHeaderQuery.Append(insertQuery1);
                        ///query1
                        updateinvoicequery.Append("UPDATE \"SLInvoiceHeader\" SET \"TotalBalance\" =  \"TotalBalance\"  - " + invoice.AR + "  WHERE \"SLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        //query2
                        receiptsquery.Append("INSERT INTO \"SLReceipts\" ( \"pyID\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\" ,\"rate\",\"journal_id\") " +
                                   "VALUES(" + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + invoice.InvoiceRef + ", " + invoice.Balance + ", " + invoice.AR + "," + (invoice.Balance - invoice.AR) + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "',  " +
                                   " '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + ", '"+receivePaymentRequest.rate+"' ," + id + "); ");
                                               
                        if (receivePaymentRequest.PaymentMethod == "CASH")
                        {
                            drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + (invoice.AR * receivePaymentRequest.rate) + "', '0', '" + (invoice.AR * receivePaymentRequest.rate) + "', " +
                             "'payment by cash','','false','','false','false','" + 0 + "');");
                        }
                        else
                        {
                            drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + (invoice.AR * receivePaymentRequest.rate) + "', '0', '" + (invoice.AR * receivePaymentRequest.rate) + "', " +
                             "'payment by cheque','','false','','false','false','" + 0 + "');");
                        }
                        /// credit debtors
                        crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                      "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + (invoice.AR * receivePaymentRequest.rate) + "', '" + (invoice.AR * receivePaymentRequest.rate) + "', 'debt reduced','','false','','false','false','" + 0 + "');");
                        /// if payment amount is negative break the code and this means the customer owes us money
                        if (paymentAmount <= 0) break;
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;

                    decimal sum = receivePaymentRequest.AllocatedInvoices.Sum(A => A.AR);
                    decimal total_diff = receivePaymentRequest.Amount - sum;
                    cust_running_balance = addCust.CustomerDept - total_diff;
                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString() + drnldetails.ToString() + crnldetails.ToString() + updateinvoicequery.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
                cnn.Close();
            }
            return myResponse;
        }
        public async Task<MyResponse> allocatedInvoicesV2(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();
            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            MyResponse myResponse = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NlService nlService = new NlService(OrganizationId);
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;         
            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();
            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";
            string status = "allocated";
            string query = ""; 
            ///GET RECEIPT DETAILS
            int lastref = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyRef\"), 0) as ref From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastref = (int)sdra["ref"];
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
               id = (int)sdrb["ref"];
            }
            cnn.Close();
            decimal cust_running_balance = 0;
            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;
            }
            cnn.Close();
            ////OPTIMIZE CODE
            cnn.OpenAsync();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    //long insertedId = 0L;
                    //string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                    //"VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";

                    var cmd = new NpgsqlCommand("", cnn, trans);
                    //id = int.Parse(cmd.ExecuteScalar().ToString());
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x =>x.Balance).ToList())
                    {
                        id++;
                        nlJournalHeader.NlJrnlNo = id;
                        nlJournalHeader.NlJrnlDesc = "allocating payment";
                        nlJournalHeader.TranDate = DateTime.Now;
                        nlJournalHeader.MEndDate = DateTime.Now;
                        nlJournalHeader.TranYear = DateTime.Now.Year;
                        nlJournalHeader.TranPeriod = DateTime.Now.Month;
                        nlJournalHeader.TranType = "NT";
                        nlJournalHeader.TranFrom = "SL-PY";
                        nlJournalHeader.ModuleId = null;
                        nlJournalHeader.SlJrnlNo = invoice.InvoiceRef;
                    
                        string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                        "VALUES('" + id + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";

                        nljournalHeaderQuery.Append(insertQuery1);
                        decimal amounttopay = Math.Min(invoice.Balance, paymentAmount);
                        decimal invoiceremainder = invoice.Balance - amounttopay;
                        ///query1
                        updateinvoicequery.Append("UPDATE \"SLInvoiceHeader\" SET \"TotalBalance\" =  " + invoiceremainder + "  WHERE \"SLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        //query2
                        receiptsquery.Append("INSERT INTO \"SLReceipts\" ( \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\" ) " +
                                   "VALUES(" + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + invoice.InvoiceRef + ", " + invoice.Balance + ", " + amounttopay + "," + invoiceremainder + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + ", '"+ receivePaymentRequest +"' ); ");

                        //query3
                        paymentAmount -= amounttopay;

                        if (receivePaymentRequest.PaymentMethod == "CASH")
                        {
                            drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + receivePaymentRequest.Amount + "', '0', '" + (receivePaymentRequest.Amount) + "', " +
                             "'payment by cash','','false','','false','false','" + 0 + "');"); 
                        }
                        else
                        {
                            drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + receivePaymentRequest.Amount + "', '0', '" + (receivePaymentRequest.Amount) + "', " +
                             "'payment by cheque','','false','','false','false','" + 0 + "');");
                        }
                        /// credit debtors
                        crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                          "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + receivePaymentRequest.Amount + "', '" + (receivePaymentRequest.Amount) + "', " +
                          "'debt reduced','','false','','false','false','" + 0 + "');");

                        /// if payment amount is negative break the code and this means the customer owes us money
                        if (paymentAmount <= 0) break;                   
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    ////// update customer balance
                    ///USE CASE 1 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID 10,000 THEN REMAINING BALANCE IS 5000 
                    /// SO SINCE CUSTOMER RUNNING BALANCE IN THIS CASE IS 15000 WE DEDUCT 10000 MAKING CURRENT BALANCE IS 5000
                    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// USE CASE 2    /////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID IS 16,000 THEN REMAINING BALANCE IS -1000
                    /// SO SINCE THE CUSTOMER OVER PAID MAKING CURRENT BALANCE BE 15000-16000 = -1000
                    cust_running_balance = addCust.CustomerDept - receivePaymentRequest.Amount;
                    //query3
                    string update_customer_balance = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + cust_running_balance + "' WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "'; ";
                    ///CHECK IF AMOUNT IS NOT ALLOCATED WHEN ITS NEGATIVE MOSTLY
                    //if(remainingBalance > 0)
                    //{
                    //    /// credit payment that is not allocated
                    //    crnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
                    //  "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                    //  "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                    //  "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.PaymentMethod.ToUpper()).NlaccCode + "','0', '" + rema + "', '" + (receivePaymentRequest.Amount) + "', " +
                    //  "'Received payment without allocation','','false','','false','false','" + 0 + "');");
                    //}
                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString()+ update_customer_balance + drnldetails.ToString() + crnldetails.ToString() + updateinvoicequery.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse ;
                }
                cnn.CloseAsync();
            }
            return myResponse;
        }
        public async Task< int> saveJournalAndReceipt(string query1 ,string query2, string query3, NpgsqlConnection cnn)
        {
            //NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

         //  await cnn.OpenAsync();
            
  
            using (var trans = cnn.BeginTransaction())
            {

                try
                {

                    var cmd = new NpgsqlCommand(null, cnn, trans);
                     cmd.CommandText = query3+query1+query2;

                   cmd.ExecuteNonQuery();
                    
                    trans.Commit();
                   // trans.Dispose();
                    return 1;
                    ////GO TO UPDATE THE NL DETAILS


                }
                catch (Exception ex)
                {

                    trans.Rollback();
                    trans.Dispose();
                    throw ex;
                    return 0;

                }





            }
            cnn.Close();



        }
        //private int updatenlDetails(List<AllocatedInvoice> myalllocatedinvoices,string paymentmethod,int customerid,decimal unallocatedbalance,int receiptref,int staffbranch,string custname,string receivedfrom,string chequenumber,int userId,string details)
        //{
        //    NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
        //    StringBuilder drnldetails = new StringBuilder();
        //    StringBuilder crnldetails = new StringBuilder();
        //    NlService nlService = new NlService(OrganizationId);
        //    decimal totalused = 0;
        //    int returnvalue = 0;

        //    foreach (var data in myalllocatedinvoices.Where(x =>x.AR ==x.Balance).ToList())
        //    {

        //        if (data.AR == data.Balance)
        //        {
        //            string code = "SL-PY";
        //            totalused = data.Balance + totalused;
        //            string query = $"Select  * From \"NlJournalHeader\" WHERE \"SlJrnlNo\" = {data.InvoiceRef} AND \"TranFrom\" = '"+code+"' ";
        //            var id = 0;
        //            cnn.OpenAsync();
        //            NpgsqlDataReader sdra = new NpgsqlCommand(query, cnn).ExecuteReader();
        //            while (sdra.Read())
        //            {
        //                id = (int)sdra["NlJrnlNo"];
        //            }
        //            cnn.CloseAsync();
        //                if (paymentmethod == "CASH")
        //                {
        //                    drnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
        //                 "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
        //                 "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
        //                 "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CASH").NlaccCode + "','" + data.AR + "', '0', '" + (data.AR) + "', " +
        //                 "'payment by cash','','false','','false','false','" + 0 + "');");
        //                }
        //                else
        //                {
        //                    drnldetails.Append("INSERT INTO \"NLJournalDetails\" " +
        //                     "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
        //                     "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
        //                     "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("BANK").NlaccCode + "','" + data.AR + "', '0', '" + (data.AR) + "', " +
        //                     "'payment by cheque','','false','','false','false','" + 0 + "');");
        //                }

        //                /// credit debtors
        //                crnldetails.Append( "INSERT INTO \"NLJournalDetails\" " +
        //              "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
        //              "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
        //              "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + data.AR + "', '" + (data.AR) + "', " +
        //              "'debt reduced','','false','','false','false','" + 0 + "');");


        //        }
        //    }
        //    cnn.Close();
        //    ///update customer running balance
        //    ///
        //    try { 

        //    string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + customerid + "' ";

        //    AddCustomer addCust = new AddCustomer();

        //    cnn.OpenAsync();

        //    NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
        //    while (sdr4.Read())
        //    {
        //        addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
        //        addCust.CustCode = sdr4["CustCode"].ToString();
        //            addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;



        //    }

        //    cnn.Close();
        //        decimal new_credit_limit = 0;
        //        if (unallocatedbalance > 0)
        //        {
        //            new_credit_limit =  addCust.CustomerDept - totalused - unallocatedbalance;
        //        }
        //        else
        //        {
        //            new_credit_limit = addCust.CustomerDept - totalused ;
        //        }

        //    string update_customer_credit = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + new_credit_limit + "' WHERE \"SLCustomerSerial\" = '" + customerid + "'; ";
        //    string creditentry1 = "";
        //    cnn.OpenAsync();
        //        int nlid = 0;


        //    using (var trans = cnn.BeginTransaction())
        //    {
        //        try
        //        {
        //            if (unallocatedbalance > 0)
        //            {


        //                NlJournalHeader nlJournalHeader = new NlJournalHeader();

        //                nlJournalHeader.NlJrnlDesc = "unallocatedpayment";
        //                nlJournalHeader.TranDate = DateTime.Now;
        //                nlJournalHeader.MEndDate = DateTime.Now;
        //                nlJournalHeader.TranYear = DateTime.Now.Year;
        //                nlJournalHeader.TranPeriod = DateTime.Now.Month;
        //                nlJournalHeader.TranType = "NT";
        //                nlJournalHeader.TranFrom = "SL-PY";
        //                nlJournalHeader.ModuleId = null;
        //                nlJournalHeader.SlJrnlNo = 0;


        //                long insertedId = 0L;
        //                string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
        //                "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";


        //                var cmd1 = new NpgsqlCommand(insertQuery1, cnn, trans);

        //                 nlid = int.Parse(cmd1.ExecuteScalar().ToString());


        //                /// credit payment that is not allocated
        //                creditentry1 = "INSERT INTO \"NLJournalDetails\" " +
        //              "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
        //              "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
        //              "VALUES('" + nlid + "','" + nlService.GetNLAccountAccountCodeUsingName(paymentmethod.ToUpper()).NlaccCode + "','0', '" + unallocatedbalance + "', '" + (unallocatedbalance) + "', " +
        //              "'Received payment without allocation','','false','','false','false','" + 0 + "');";


        //            }





        //            var cmd = new NpgsqlCommand(null, cnn, trans);



        //            if (unallocatedbalance < 0)
        //            {


        //                    cmd.CommandText = drnldetails.ToString() + crnldetails.ToString() + update_customer_credit;
        //            }


        //                string unallocatedstatus = "NOT ALLOCATED";
        //                string unallocatedbalancequery = "INSERT INTO \"SLReceipts\" (\"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\",\"journal_id\" ) " +
        //                      "VALUES( " + receiptref + ", '" + DateTime.Now + "'," + 0 + ", " + 0 + ", " + unallocatedbalance + "," + 0 + ",'" + paymentmethod + "', '" + chequenumber + "', '" + receivedfrom + "','" + details + "', '" + DateTime.Now + "'," + userId + "," + staffbranch + ",'" + custname + "','" + unallocatedstatus + "'," + customerid + "," + nlid + "); ";





        //            cmd.CommandText = drnldetails.ToString() + crnldetails.ToString() + unallocatedbalancequery + update_customer_credit +  creditentry1;
        //            cmd.ExecuteNonQuery();
        //            trans.Commit();
        //            returnvalue = 1;
        //            ////GO TO UPDATE THE NL DETAILS
        //            return returnvalue;

        //        }

        //        catch (Exception ex)
        //        {
        //            trans.Rollback();


        //           return returnvalue = 0;
        //            throw ex;

        //        }





        //    }
        //    }
        //    catch (Exception e) {


        //        throw e;
        //        return returnvalue = 0;

        //    }
        //    cnn.CloseAsync();

        //    return returnvalue;
        //}


        public async Task<MyResponse> allocatedPurchaseWithoutAllocation(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            string drnldetails = "";
            string crnldetails = "";
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            StringBuilder nljournalHeaderQuery1 = new StringBuilder();
            string insertQuery = "";

            ///GET RECEIPT DETAILS
            int lastref = 0;
            string query = "";
            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;

            //get last pyid
            int last_pyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();


            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];

            }
            cnn.Close();


            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    int period_month = Int32.Parse(receivePaymentRequest.Period.Split("/")[0]);
                    int period_year = Int32.Parse(receivePaymentRequest.Period.Split("/")[1]);

                    string reference;

                    insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\",  \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\",\"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"rate\",\"supplier_id\",\"allocation_remainder\" )" +
                       " VALUES(" + (last_pyid + 1) + ", " + (last_pyid + 1) + ", '" +receivePaymentRequest.PaymentDate + "', " +
                       " " + 0 + ", " + receivePaymentRequest.Amount + "," + receivePaymentRequest.Amount + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "'," +
                       " '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + "," + receivePaymentRequest.rate + " ," + receivePaymentRequest.CustId + ","+ receivePaymentRequest.Amount + "); ";

                    id++;
                    nlJournalHeader.NlJrnlNo = id;
                    nlJournalHeader.NlJrnlDesc = "allocating purchase payment";
                    nlJournalHeader.TranDate = DateTime.Now;
                    nlJournalHeader.MEndDate = DateTime.Now;
                    nlJournalHeader.TranYear = period_year;
                    nlJournalHeader.TranPeriod = period_month;
                    nlJournalHeader.TranType = "NT";
                    nlJournalHeader.TranFrom = "PL-PY";
                    nlJournalHeader.ModuleId = null;
                    nlJournalHeader.PlJrnlNo = last_pyid + 1;

                    nljournalHeaderQuery1.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                        "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','PL-PY','NT','" + 0 + "'," + nlJournalHeader.PlJrnlNo + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;");

                    if (receivePaymentRequest.PaymentMethod == "CASH")
                    {
                        crnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                     "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', " +
                     "'payment by cash','','false','','false','false','" + 0 + "');";
                    }
                    else
                    {
                        crnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', " +
                         "'payment by cheque','','false','','false','false','" + 0 + "');";
                    }
                    drnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                    "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', '0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                    "'reduce creditors account','','false','','false','false','" + 0 + "');";
                    var remainingBalance = paymentAmount;
                    query = nljournalHeaderQuery.ToString() + nljournalHeaderQuery1 + insertQuery + updateinvoicequery.ToString() + receiptsquery.ToString() + drnldetails + crnldetails;
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    myResponse.Id = (last_pyid + 1);
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return null;
        }
        public async Task<MyResponse> allocatedWithNoInvoices(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();

            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;

            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();
            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";
            string status = "Unallocated";

            string query = "";


            ///GET RECEIPT DETAILS
            int lastref = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyID\"), 0) as pyID From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastref = (int)sdra["pyID"];

            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];

            }
            cnn.Close();


            decimal cust_running_balance = 0;

            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;

            }
            cnn.Close();

            ////OPTIMIZE CODE

            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);

                    //id = int.Parse(cmd.ExecuteScalar().ToString());

                    int period_year = Int32.Parse(receivePaymentRequest.Period.Split("/")[1]);
                    int period_month = Int32.Parse(receivePaymentRequest.Period.Split("/")[0]);

                    id++;

                    nlJournalHeader.NlJrnlNo = id;
                    nlJournalHeader.NlJrnlDesc = " payment with no allocation";
                    nlJournalHeader.TranDate = DateTime.Now;
                    nlJournalHeader.MEndDate = DateTime.Now;
                    nlJournalHeader.TranYear = period_year;
                    nlJournalHeader.TranPeriod = period_month;
                    nlJournalHeader.TranType = "NT";
                    nlJournalHeader.TranFrom = "SL-PY";
                    nlJournalHeader.ModuleId = null;
                    nlJournalHeader.SlJrnlNo = lastref + 1;

                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                    "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";

                    nljournalHeaderQuery.Append(insertQuery1);

                    //query2
                    receiptsquery.Append("INSERT INTO \"SLReceipts\" ( \"pyID\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\" ,\"rate\",\"journal_id\",\"allocation_remainder\") " +
                               "VALUES(" + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + 0 + ", " +0 + ", " + receivePaymentRequest.Amount + "," + 0+ ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "', '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + " ,"+receivePaymentRequest.rate+","+id+"," + receivePaymentRequest.Amount + "); ");



                    if (receivePaymentRequest.PaymentMethod == "CASH")
                    {
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                     "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + (receivePaymentRequest.Amount  * receivePaymentRequest.rate) + "', '0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                     "'payment by cash','','false','','false','false','" + 0 + "');");
                    }
                    else
                    {
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', '0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                         "'payment by cheque','','false','','false','false','" + 0 + "');");
                    }
                    /// credit debtors
                    crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                  "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                  "'debt reduced','','false','','false','false','" + 0 + "');");
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    cust_running_balance = addCust.CustomerDept - receivePaymentRequest.Amount;
                    //query3
                    string update_customer_balance = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + cust_running_balance + "' WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "'; ";
                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString() + update_customer_balance + drnldetails.ToString() + crnldetails.ToString() + updateinvoicequery.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Debug.WriteLine(Environment.StackTrace);
                    Debug.WriteLine(ex.Message);
                    myResponse.Message = ex.Message;
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return myResponse;
        }
        public async Task<MyResponse> allocatedInvoicesFromPurchaseReceipt(ReceivePaymentRequest receivePaymentRequest, int userId, int receipt_id)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();

            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;
            string query = "";

            ////OPTIMIZE CODE

            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    //id = int.Parse(cmd.ExecuteScalar().ToString());
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x => x.Balance).ToList())
                    {
                        string insertQuery1 = "INSERT INTO \"PLReceiptsDetails\" (\"invoice_id\",\"allocated_amount\",\"receipt_id\",\"allocated_by\",\"allocated_on\",\"invoice_balance\") " +
                        "VALUES('" + invoice.InvoiceRef + "','" + invoice.AR + "','" + receipt_id + "','" + userId + "', '" + DateTime.Now.ToShortDateString() + "','"+invoice.Balance+"' )  ;";
                        nljournalHeaderQuery.Append(insertQuery1);
                        ///query1
                        updateinvoicequery.Append("UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  \"Balance\"  - " + invoice.AR + "   WHERE \"PLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        //query2
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    ////// update customer balance
                    ///USE CASE 1 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID 10,000 THEN REMAINING BALANCE IS 5000 
                    /// SO SINCE CUSTOMER RUNNING BALANCE IN THIS CASE IS 15000 WE DEDUCT 10000 MAKING CURRENT BALANCE IS 5000
                    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// USE CASE 2    /////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID IS 16,000 THEN REMAINING BALANCE IS -1000
                    /// SO SINCE THE CUSTOMER OVER PAID MAKING CURRENT BALANCE BE 15000-16000 = -1000
                    /// 
                    ////GET SUM AR AMOUNT TO MINUS THE CURRENT AMOUNT

                    decimal sum = receivePaymentRequest.AllocatedInvoices.Sum(A => A.AR);
                    //update receipt_remiander
                    string status = "allocated";
                    updateinvoicequery.Append("UPDATE \"PLReceipts\" SET    \"allocation_remainder\" =  \"allocation_remainder\"  - " + sum + "  , \"pyBalance\" =  \"pyBalance\" - " + sum+"   WHERE \"pyID\" = " + receipt_id + "; ");
                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString() + updateinvoicequery.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later debug";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return myResponse;
        }        
        public async Task<MyResponse> allocatedInvoicesFromReceipt(ReceivePaymentRequest receivePaymentRequest, int userId, int receipt_id)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();
            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            MyResponse myResponse = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NlService nlService = new NlService(OrganizationId);
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;
            string query = "";
            ////OPTIMIZE CODE
            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    //id = int.Parse(cmd.ExecuteScalar().ToString());
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x => x.Balance).ToList())
                    {
                        string insertQuery1 = "INSERT INTO \"SLReceiptsDetails\" (\"inv_id\",\"amount_allocated\",\"receipt_id\",\"allocated_by\",\"allocated_on\") VALUES('" + invoice.InvoiceRef + "','" + invoice.AR+ "','" + receipt_id + "','" + userId + "', '" + DateTime.Now.ToShortDateString() + "' )  ;";
                        nljournalHeaderQuery.Append(insertQuery1);
                        ///query1
                        updateinvoicequery.Append("UPDATE \"SLInvoiceHeader\" SET \"TotalBalance\" =  \"TotalBalance\"  - " + invoice.AR + "  WHERE \"SLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        //query2
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    ////// update customer balance
                    ///USE CASE 1 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID 10,000 THEN REMAINING BALANCE IS 5000 
                    /// SO SINCE CUSTOMER RUNNING BALANCE IN THIS CASE IS 15000 WE DEDUCT 10000 MAKING CURRENT BALANCE IS 5000
                    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// USE CASE 2    /////////////////////////////////////
                    /// IF TOTAL INVOICE AMOUNT IS 15,000 THEN PAID IS 16,000 THEN REMAINING BALANCE IS -1000
                    /// SO SINCE THE CUSTOMER OVER PAID MAKING CURRENT BALANCE BE 15000-16000 = -1000
                    /// 
                    ////GET SUM AR AMOUNT TO MINUS THE CURRENT AMOUNT
                    decimal sum = receivePaymentRequest.AllocatedInvoices.Sum(A => A.AR);
                    //update receipt_remiander
                    string status = "allocated";
                    updateinvoicequery.Append("UPDATE \"SLReceipts\" SET   \"status\" = '"+status+ "' ,  \"allocation_remainder\" =  \"allocation_remainder\"  - " + sum + "  WHERE \"pyID\" = " +receipt_id + "; ");
                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString()  + updateinvoicequery.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return myResponse;
        }
        public async Task<MyResponse> allocatedInvoicesV_3(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();

            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;




            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();
            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + receivePaymentRequest.CustId + "' ";
            string status = "allocated";

            string query = "";


            ///GET RECEIPT DETAILS
            int lastref = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyID\"), 0) as pyID From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastref = (int)sdra["pyID"];

            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];

            }
            cnn.Close();


            decimal cust_running_balance = 0;

            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;

            }
            cnn.Close();

            ////OPTIMIZE CODE

            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    //id = int.Parse(cmd.ExecuteScalar().ToString());

                    int period_year = Int32.Parse(receivePaymentRequest.Period.Split("/")[1]);
                    int period_month = Int32.Parse(receivePaymentRequest.Period.Split("/")[0]);

                    int journal_id = id + 1;
                    nlJournalHeader.NlJrnlNo = journal_id;
                    nlJournalHeader.NlJrnlDesc = "allocating payment";
                    nlJournalHeader.TranDate = DateTime.Now;
                    nlJournalHeader.MEndDate = DateTime.Now;
                    nlJournalHeader.TranYear = period_year;
                    nlJournalHeader.TranPeriod = period_month;
                    nlJournalHeader.TranType = "NT";
                    nlJournalHeader.TranFrom = "SL-PY";
                    nlJournalHeader.ModuleId = null;
                    nlJournalHeader.SlJrnlNo = (lastref + 1);

                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                    "VALUES('" +journal_id + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','SL-PY','NT','" + nlJournalHeader.SlJrnlNo + "'," + 0 + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";
                    nljournalHeaderQuery.Append(insertQuery1);
                    var ar = receivePaymentRequest.AllocatedInvoices.Sum(item => item.AR);
                    receiptsquery.Append("INSERT INTO \"SLReceipts\" ( \"pyID\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"currentCustName\",\"status\",\"cust_id\",   " +
                        "\"rate\",\"journal_id\",\"allocation_remainder\") " +
                                 "VALUES(" + (lastref + 1) + ", '" + receivePaymentRequest.PaymentDate + "'," + 0 + ", " + receivePaymentRequest.Amount+ ", " + receivePaymentRequest.Amount  + "," + (receivePaymentRequest.Amount - ar) + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "',   " +
                                 "'" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + ",'" + receivePaymentRequest.CurrenctCustomerName + "','" + status + "'," + receivePaymentRequest.CustId + " ," + receivePaymentRequest.rate + ",    " +
                                 "" + id + ","+ (receivePaymentRequest.Amount - ar)+ "); ");
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices.OrderBy(x => x.Balance).ToList())
                    {
                        ///query1
                        updateinvoicequery.Append("UPDATE \"SLInvoiceHeader\" SET \"TotalBalance\" =  \"TotalBalance\"  - " + invoice.AR + "  WHERE \"SLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        //query2
                        string sldetails = "INSERT INTO \"SLReceiptsDetails\" (\"inv_id\",\"amount_allocated\",\"receipt_id\",\"allocated_by\",\"allocated_on\") VALUES('" + invoice.InvoiceRef + "','" + invoice.AR + "','" + nlJournalHeader.SlJrnlNo + "','" + userId + "', '" + DateTime.Now.ToShortDateString() + "' )  ;";
                        updateinvoicequery.Append(sldetails);
                        /// if payment amount is negative break the code and this means the customer owes us money
                        if (paymentAmount <= 0) break;
                    }
                    if (receivePaymentRequest.PaymentMethod == "CASH")
                    {
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")   " +
                     "VALUES('" + journal_id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', '0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                     "'payment by cash','','false','','false','false','" + 0 + "');");
                    }
                    else
                    {
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + journal_id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', '0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                         "'payment by cheque','','false','','false','false','" + 0 + "');");
                    }
                    /// credit debtors
                    crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                  "VALUES('" + journal_id + "','" + nlService.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', '" + (receivePaymentRequest.rate) + "', " +
                  "'debt reduced','','false','','false','false','" + 0 + "');");
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    decimal sum = receivePaymentRequest.AllocatedInvoices.Sum(A => A.AR);
                    decimal total_diff = receivePaymentRequest.Amount  - sum;
                    cust_running_balance = addCust.CustomerDept - total_diff;
                    query = nljournalHeaderQuery.ToString() + receiptsquery.ToString()  + drnldetails.ToString() + crnldetails.ToString() + updateinvoicequery.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return myResponse;
        }
        public async Task<RemitanceReport> getReceiptDetailAllocation(int receipt_id)
        {

            string query = " SELECT \"PLReceiptsDetails\".*,\"invoice\".\"DocRef\" as invoiceref  FROM \"PLReceiptsDetails\" " +
                "   LEFT JOIN \"PLInvoiceHeader\" invoice  ON  \"invoice\".\"PLJrnlNo\" = \"PLReceiptsDetails\".\"invoice_id\"     " +
                "     WHERE \"receipt_id\" ='" + receipt_id+"' ";
            string query2 = "SELECT \"PLReceipts\".*,\"supplier\".\"CustName\",\"supplier\".\"PhysicalAddress\",\"supplier\".\"VATNo\" FROM \"PLReceipts\"   " +
                " LEFT JOIN \"PLCustomer\" supplier  ON \"supplier\".\"CustID\" = \"PLReceipts\".\"supplier_id\" " +
                "   WHERE  \"pyID\" = '" + receipt_id + "'  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            RemitanceReport remitanceReport = new RemitanceReport();

            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand(query, cnn).ExecuteReader();
            List<RemitanceData> remitanceDatas = new List<RemitanceData>();
            while (sdra.Read())
            {
                RemitanceData remitanceData = new RemitanceData();
                remitanceData.InvoiceBalance = sdra["invoice_balance"] != DBNull.Value ? (decimal)sdra["invoice_balance"] : 0;
                remitanceData.AllocatedAmount = sdra["allocated_amount"] != DBNull.Value ? (decimal)sdra["allocated_amount"] : 0;
                remitanceData.InvoiceRef = sdra["invoiceref"] != DBNull.Value ? (string)sdra["invoiceref"] : "";
                remitanceData.AllocatedOn = sdra["allocated_on"] != DBNull.Value ? (DateTime)sdra["allocated_on"] : DateTime.Now;
                remitanceDatas.Add(remitanceData);

            }
            cnn.Close();
            cnn.Open();

            NpgsqlDataReader sdrb = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (sdrb.Read())
            {
                remitanceReport.ChequeNumber  = sdrb["pyChequeNumber"] != DBNull.Value ? (string)sdrb["pyChequeNumber"] : "";
                remitanceReport.PaymentMode = sdrb["pyMode"] != DBNull.Value ? (string)sdrb["pyMode"] : "";
                remitanceReport.ReceiptBalance = sdrb["pyBalance"] != DBNull.Value ? (float)sdrb["pyBalance"] : 0;
                remitanceReport.ReceiptAmount  = sdrb["pyPaid"] != DBNull.Value ? (float)sdrb["pyPaid"] : 0;
                remitanceReport.PyId = sdrb["pyID"] != DBNull.Value ? (int)sdrb["pyID"] : 0;
                remitanceReport.CustName = sdrb["CustName"] != DBNull.Value ? (string)sdrb["CustName"] : "";
                remitanceReport.Pin = sdrb["VATNo"] != DBNull.Value ? (string)sdrb["VATNo"] : "";
                remitanceReport.PostalAddress = sdrb["PhysicalAddress"] != DBNull.Value ? (string)sdrb["PhysicalAddress"] : "";
                remitanceReport.ReceiptDate = sdrb["pyDate"] != DBNull.Value ? (DateTime)sdrb["pyDate"] : DateTime.Now;

            }
            cnn.Close();
            remitanceReport.RemitanceData = remitanceDatas;


            return remitanceReport;
        }
        public async Task<MyResponse> allocatedPurchaseInvoicesV_2(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            StringBuilder drnldetails = new StringBuilder();
            StringBuilder crnldetails = new StringBuilder();
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            string insertQuery = "";

            ///GET RECEIPT DETAILS
            int lastref = 0;
            string query = "";
            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;



            //get last pyid
            int last_pyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();


            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];

            }
            cnn.Close();

            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices)
                    {
                        id++;
                        nlJournalHeader.NlJrnlNo = id;
                        nlJournalHeader.NlJrnlDesc = "allocating purchase payment";
                        nlJournalHeader.TranDate = DateTime.Now;
                        nlJournalHeader.MEndDate = DateTime.Now;
                        nlJournalHeader.TranYear = DateTime.Now.Year;
                        nlJournalHeader.TranPeriod = DateTime.Now.Month;
                        nlJournalHeader.TranType = "NT";
                        nlJournalHeader.TranFrom = "PY-PY";
                        nlJournalHeader.ModuleId = null;
                        nlJournalHeader.PlJrnlNo = invoice.InvoiceRef;

                        nljournalHeaderQuery.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlNo\",\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                        "VALUES('" + nlJournalHeader.NlJrnlNo + "','" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','PL-PY','NT','" + 0 + "'," + nlJournalHeader.PlJrnlNo + ",'" + 0 + "' )     " +
                        "RETURNING \"NlJrnlNo\" ;");
                        decimal amounttopay = Math.Min(invoice.Balance, paymentAmount);
                        decimal invoiceremainder = invoice.Balance - amounttopay;
                        string status = "";

                        ///query1
                        updateinvoicequery.Append("UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  " + invoiceremainder + "  WHERE \"PLJrnlNo\" = " + invoice.InvoiceRef + "; ");

                        insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\" )" +
                               " VALUES(" + (last_pyid + 1) + ", " + (last_pyid + 1) + ", '" + DateTime.Now + "', " + invoice.InvoiceRef + ", " +
                               "" + invoice.Balance + ", " + amounttopay + "," + invoiceremainder + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "'," +
                               " '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + " ); ";

                        if (receivePaymentRequest.PaymentMethod == "CASH")
                        {
                            crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount + "', '" + receivePaymentRequest.Amount + "', " +
                         "'payment by cash','','false','','false','false','" + 0 + "');");
                        }
                        else
                        {
                            crnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount + "', '" + receivePaymentRequest.Amount + "', " +
                             "'payment by cheque','','false','','false','false','" + 0 + "');");
                        }
                        drnldetails.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                        "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + receivePaymentRequest.Amount + "', '0', '" + (receivePaymentRequest.Amount) + "', " +
                        "'payment by cash','','false','','false','false','" + 0 + "');");
                        paymentAmount -= amounttopay;
                        /// if payment amount is negative break the code and this means the customer owes us money
                        if (paymentAmount <= 0) break;
                    }
                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    query = nljournalHeaderQuery.ToString() + insertQuery + updateinvoicequery.ToString() + receiptsquery.ToString() + drnldetails.ToString() + crnldetails.ToString();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return null;
        }
        public async Task<MyResponse> UpdateSupplierBroughtForward(BalanceBroughtForwardUpdate balanceBroughtForwardUpdate)
        {
            StringBuilder receiptsquery = new StringBuilder();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            string myQuery = "SELECT \"PLCustomer\".* FROM \"PLCustomer\"   WHERE \"CustName\" = '" + balanceBroughtForwardUpdate.CustomerId + "' ";
            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();

            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["CustID"];
                addCust.CustCode = sdr4["PLCustCode"].ToString();
                // addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;
                addCust.OpeningBalance = sdr4["OpeningBalance"] != DBNull.Value ? (decimal)sdr4["OpeningBalance"] : 0;

            }
            cnn.Close();


            MyResponse myResponse = new MyResponse();
            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    string status = "allocated";
                    string supplier_payment_trail = "INSERT INTO \"supplier_brought_forward_trail\" (\"supplier_id\",\"current_brought_forward_amount\",\"amount_allocated\",\"created_on\",\"receipt_id\") " +
                     "VALUES('" + addCust.SLCustomerSerial + "','" + addCust.OpeningBalance + "','" + balanceBroughtForwardUpdate.AllocatedAmount + "', '" + DateTime.Now.ToShortDateString() + "','"+balanceBroughtForwardUpdate.ReceiptId+"' )  ;";
                    receiptsquery.Append(supplier_payment_trail);
                    receiptsquery.Append("UPDATE \"PLReceipts\" SET    \"pyBalance\" =  \"pyBalance\"  - " + balanceBroughtForwardUpdate.AllocatedAmount +   ",\"allocation_remainder\" =  \"allocation_remainder\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + "  WHERE \"pyID\" = " + balanceBroughtForwardUpdate.ReceiptId + "; ");
                    if (balanceBroughtForwardUpdate.AllocatedAmount ==  addCust.OpeningBalance)
                    {
                        bool hasopeningbalance = false;
                        receiptsquery.Append("UPDATE \"PLCustomer\" SET   \"HasOpeningBalance\" =  false,  \"OpeningBalance\" =  \"OpeningBalance\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + "  WHERE \"CustName\" = '" + balanceBroughtForwardUpdate.CustomerId + "'; ");
                    }
                    else
                    {
                        receiptsquery.Append("UPDATE \"PLCustomer\" SET    \"OpeningBalance\" =  \"OpeningBalance\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + "  WHERE \"CustName\" = '" + balanceBroughtForwardUpdate.CustomerId.ToString() + "'; ");
                    }
                    cmd.CommandText = receiptsquery.ToString();
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
        }
        public async Task<MyResponse> UpdateDebtorsBroughtForward(BalanceBroughtForwardUpdate balanceBroughtForwardUpdate)
        {
            StringBuilder receiptsquery = new StringBuilder();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"   WHERE \"SLCustomerSerial\" = '" + balanceBroughtForwardUpdate.CustId + "' ";
            ///UPDATE CUSTOMER RUNNIG BALANCE
            AddCustomer addCust = new AddCustomer();

            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                // addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;
                addCust.OpeningBalance = sdr4["OpeningBalance"] != DBNull.Value ? (decimal)sdr4["OpeningBalance"] : 0;
            }
            cnn.Close();
            MyResponse myResponse = new MyResponse();
            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    string status = "allocated";
                    string debtor_payment_trail = "INSERT INTO \"debtors_brought_forward_trail\" (\"customer_id\",\"current_brought_forward_amount\",\"amount_allocated\",\"created_on\",\"receipt_id\") " +
               "VALUES('" + addCust.SLCustomerSerial + "','" + addCust.OpeningBalance + "','" + balanceBroughtForwardUpdate.AllocatedAmount + "', '" + DateTime.Now.ToShortDateString() + "','" + balanceBroughtForwardUpdate.ReceiptId + "' )  ;";
                    receiptsquery.Append(debtor_payment_trail);
                    receiptsquery.Append("UPDATE \"SLReceipts\" SET    \"pyBalance\" =  \"pyBalance\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + ",\"allocation_remainder\" =  \"allocation_remainder\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + "  WHERE \"pyID\" = " + balanceBroughtForwardUpdate.ReceiptId + "; ");

                    if (balanceBroughtForwardUpdate.AllocatedAmount == addCust.OpeningBalance)
                    {
                        bool hasopeningbalance = false;
                        receiptsquery.Append("UPDATE \"SLCustomer\" SET   \"OpeningBalance\" =  \"OpeningBalance\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + "  WHERE \"SLCustomerSerial\" = '" + balanceBroughtForwardUpdate.CustId + "'; ");
                    }
                    else
                    {
                        receiptsquery.Append("UPDATE \"SLCustomer\" SET    \"OpeningBalance\" =  \"OpeningBalance\"  - " + balanceBroughtForwardUpdate.AllocatedAmount + "  WHERE \"SLCustomerSerial\" = '" + balanceBroughtForwardUpdate.CustId + "'; ");
                    }
                    cmd.CommandText = receiptsquery.ToString();
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
        }
        public async Task<MyResponse> allocatedPurchaseInvoicesV_3(ReceivePaymentRequest receivePaymentRequest, int userId, int staff_branch)
        {
            decimal paymentAmount = receivePaymentRequest.Amount;
            StringBuilder updateinvoicequery = new StringBuilder();
            StringBuilder receiptsquery = new StringBuilder();

            string drnldetails = "";
            string crnldetails = "";
            StringBuilder nljournalHeaderQuery = new StringBuilder();
            StringBuilder nljournalHeaderQuery1 = new StringBuilder();
            string insertQuery = "";

            ///GET RECEIPT DETAILS
            int lastref = 0;
            string query = "";
            MyResponse myResponse = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            NlService nlService = new NlService(OrganizationId);

            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            int id = 0;



            //get last pyid
            int last_pyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();


            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"NlJrnlNo\"), 0) as ref From \"NlJournalHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                id = (int)sdrb["ref"];

            }
            cnn.Close();


            cnn.Open();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    var cmd = new NpgsqlCommand("", cnn, trans);
                    int period_month = Int32.Parse(receivePaymentRequest.Period.Split("/")[0]);
                    int period_year = Int32.Parse(receivePaymentRequest.Period.Split("/")[1]);
                    decimal total_amount_received = receivePaymentRequest.AllocatedInvoices.Sum(x => x.AR);

                    string reference;
                    insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\",  \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\"," +
               " \"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\",\"rate\",\"supplier_id\",\"allocation_remainder\" )" +
               " VALUES(" + (last_pyid + 1) + ", " + (last_pyid + 1) + ", '" + receivePaymentRequest.PaymentDate + "', " +
               " " + receivePaymentRequest.Amount + ", " + receivePaymentRequest.Amount + "," + (receivePaymentRequest.Amount - total_amount_received) + ",'" + receivePaymentRequest.PaymentMethod + "', '" + receivePaymentRequest.ChequeNumber + "'," +
               " '" + receivePaymentRequest.ReceivedFrom + "','" + receivePaymentRequest.AdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + "," + receivePaymentRequest.rate + " ,"+receivePaymentRequest.CustId+","+ (receivePaymentRequest.Amount - total_amount_received) + "); ";

                    foreach (var invoice in receivePaymentRequest.AllocatedInvoices)
                    {
                        if (invoice.AR > 0)
                        {
                            id++;
                            nlJournalHeader.NlJrnlNo = id;
                            nlJournalHeader.NlJrnlDesc = "allocating purchase payment";
                            nlJournalHeader.TranDate = DateTime.Now;
                            nlJournalHeader.MEndDate = DateTime.Now;
                            nlJournalHeader.TranYear = period_year;
                            nlJournalHeader.TranPeriod = period_month;
                            nlJournalHeader.TranType = "NT";
                            nlJournalHeader.TranFrom = "PL-PY";
                            nlJournalHeader.ModuleId = null;
                            nlJournalHeader.PlJrnlNo = last_pyid + 1;

                            nljournalHeaderQuery.Append("INSERT INTO \"PLReceiptsDetails\" (\"invoice_id\",\"allocated_amount\",\"allocated_by\",\"allocated_on\",\"receipt_id\",\"invoice_balance\") VALUES" +
                                "('"+invoice.InvoiceRef+"','"+invoice.AR+"','"+ userId + "','"+DateTime.Now+"','"+ (last_pyid + 1) + "','"+invoice.Balance+"'); ");
                            nljournalHeaderQuery1.Append("INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                            "VALUES('" + nlJournalHeader.NlJrnlDesc + "','" + nlJournalHeader.TranDate + "','" + nlJournalHeader.MEndDate + "', '" + nlJournalHeader.TranPeriod + "', '" + nlJournalHeader.TranYear + "','PL-PY','NT','" + 0 + "'," + nlJournalHeader.PlJrnlNo + ",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;");
                            //  decimal amounttopay = Math.Min(invoice.Balance, paymentAmount);
                            decimal invoiceremainder = invoice.Balance - invoice.AR;
                            string status = "";
                            ///query1
                            //    string status = "allocated";
                            updateinvoicequery.Append("UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  " + invoiceremainder + "    WHERE \"PLJrnlNo\" = " + invoice.InvoiceRef + "; ");
                        }
                    }

                    if (receivePaymentRequest.PaymentMethod == "CASH")
                    {
                        crnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                     "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', " +
                     "'payment by cash','','false','','false','false','" + 0 + "');";
                    }
                    else
                    {
                        crnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName(receivePaymentRequest.Nominal).NlaccCode + "','0', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', '" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', " +
                         "'payment by cheque','','false','','false','false','" + 0 + "');";
                    }
                    drnldetails = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                    "VALUES('" + id + "','" + nlService.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + receivePaymentRequest.Amount * receivePaymentRequest.rate + "', '0', '" + (receivePaymentRequest.Amount * receivePaymentRequest.rate) + "', " +
                    "'reduce creditors account','','false','','false','false','" + 0 + "');";

                    // Allocate any remaining balance to customer running balance
                    var remainingBalance = paymentAmount;
                    query = nljournalHeaderQuery.ToString() + nljournalHeaderQuery1 + insertQuery + updateinvoicequery.ToString() + receiptsquery.ToString() + drnldetails + crnldetails;

                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    myResponse.Message = "Success";
                    myResponse.Httpcode = 200;
                    myResponse.Id = (last_pyid + 1);
                    cnn.Close();
                    return myResponse;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Environment.StackTrace);
                    Console.WriteLine(ex.Message);
                    myResponse.Message = "Error,please try again later";
                    myResponse.Httpcode = 400;
                    trans.Rollback();
                    trans.Dispose();
                    return myResponse;
                }
            }
            return null;
        }
    }
}
