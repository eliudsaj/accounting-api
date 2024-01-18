using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Controllers.NlController;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.NL.NLAccount;
using pyme_finance_api.Models.Purchases;
using pyme_finance_api.Models.Purchases.Customers;
using pyme_finance_api.Models.Purchases.Invoices;
using pyme_finance_api.Models.Purchases.LPO;
using pyme_finance_api.Models.Purchases.PurchaseReport;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Models.Vat;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.VatService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.PlService
{
    public interface IPlnService
    {
        public List<PLAnalysisCodes> GetPlanalysisCodes();
        public MyResponse addPlanalysis(PLAnalysisCodes PLAnalysisCodes);
        public MyResponse updatePlanalysis(PLAnalysisCodes PLAnalysisCodes);
        public LpoHeader GetLpoHeaderById(int id);
        public PLCustomer GetCustomerById(int id);
        public string getPlAccountGroupNominal(string code);
        public List<PlCustomerStatement> GetCustomerLedger(int id, DateTime from, DateTime to);
        public List<PlCustomerStatement> GetCustomerStatement(int cust_id);
        public MyResponse convertlpotoInvoice(int lpoId, NlJournalHeader recvData, List<InvoiceListDetailsData> InvoiceDetailsList, int userid, int staff_branch);
        List<Vat3BReport> GettingSupplierActivity(int custId, DateTime from, DateTime to);
        License GettingCompanyDetails();
        MyResponse CreateGRNType(GoodReturnNoteType goodReturn);
        MyResponse UpdateGRNType(GoodReturnNoteType goodReturnNote);
        MyResponse DeleteGRNType(int key);
        MyResponse UpdateHeaderSettings(PurchaseHeaderSettings header);
        PLCustomer GettingOpeningBalance(int custId);
        List<PlCustomerStatement> GetSupplierActivity(int custId, DateTime from, DateTime to);
        PlCustomerStatement GetPaymentBalanceForward(int custId, DateTime from);
        PlCustomerStatement GetCrnBalanceForward(int custId, DateTime from);
        PlCustomerStatement GetInvoiceBalance(int custId, DateTime from);
    }
    public class PlService : IPlnService
    {
        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }
        public PlService(string organizationId)
        {
            OrganizationId = organizationId;
        }
        public List<PLAnalysisCodes> GetPlanalysisCodes()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //geT nljournal
            List<PLAnalysisCodes> pLAnalysisCodes = new List<PLAnalysisCodes>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"PLAnalysisCodes\"", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                PLAnalysisCodes pLAnalysisCode = new PLAnalysisCodes();
                pLAnalysisCode.AnalCode = sdr0["AnalCode"] != DBNull.Value ? (string)sdr0["AnalCode"] : null;
                pLAnalysisCode.AnalDesc = sdr0["AnalDesc"] != DBNull.Value ? (string)sdr0["AnalDesc"] : null;
                pLAnalysisCode.AnalType = sdr0["AnalType"] != DBNull.Value ? (string)sdr0["AnalType"] : null;
                pLAnalysisCode.NLAccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
                pLAnalysisCode.Id = (int)sdr0["id"]; ;
                pLAnalysisCodes.Add(pLAnalysisCode);
            }
            cnn.Close();
            return pLAnalysisCodes;
        }
        public MyResponse addPlanalysis(PLAnalysisCodes PLAnalysisCodes)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn1.Open();
            ///check if that analysiscode exists
            int groupnamecount = 0;
            ////check if group name exists
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"PLAnalysisCodes\" where  \"AnalCode\"= '" + PLAnalysisCodes.AnalCode + "'", cnn1).ExecuteReader();
            while (sdr1.Read())
            {
                groupnamecount++;
            }
            if (groupnamecount > 1)
            {
                response.Httpcode = 400;
                response.Message = "This  Analysis Code already exists";
                cnn1.Close();
                return response;
            }
            string insertQuery1 = "INSERT INTO \"PLAnalysisCodes\" (\"NLAccCode\",\"AnalType\",\"AnalDesc\",\"AnalCode\") " +
             "VALUES('" + PLAnalysisCodes.NLAccCode + "','" + PLAnalysisCodes.AnalType + "','" + PLAnalysisCodes.AnalDesc + "', '" + PLAnalysisCodes.AnalCode + "' );";
            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, OrganizationId);
            cnn1.Close();
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            return response;  
        }
        public MyResponse updatePlanalysis(PLAnalysisCodes PLAnalysisCodes)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn1.Open();
            string updtQ = "UPDATE \"PLAnalysisCodes\" SET \"NLAccCode\" = '" + PLAnalysisCodes.NLAccCode + "',\"AnalType\"='" + PLAnalysisCodes.AnalType + "',\"AnalDesc\"='" + PLAnalysisCodes.AnalDesc + "',\"AnalCode\" = '"
                 + PLAnalysisCodes.AnalCode.ToUpper() + "',\"ModifiedOn\"='" + DateTime.Now + "' WHERE \"id\" = '" + PLAnalysisCodes.Id + "' ";
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);
            cnn1.Close();
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }     
            return response;         
        }     
        public MyResponse convertlpotoInvoice(int lpoId, NlJournalHeader recvData, List<InvoiceListDetailsData> InvoiceDetailsList,int userid,int staff_branch)
        {
            string query = "Select  * FROM  \"LPOHeader\"  WHERE  \"LID\"  = '" + lpoId + "' ";
            int count = myDbconnection.CheckRowExists(query, OrganizationId);
            MyResponse response = new MyResponse();
            if (count == 0)
            {
                response.Httpcode = 400;
                response.Message = "This lpo does not exist";
                return response;
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NlService nlServices = new NlService(OrganizationId);
            cnn.OpenAsync();
            int journalid;
            int lastPLjrnlNo = 0;
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"PLJrnlNo\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastPLjrnlNo = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            // get last DOC REF
            int lastDOCREF = 0;
            cnn.Open();
            NpgsqlDataReader sdrF = new NpgsqlCommand("Select MAX(\"DocRef\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrF.Read())
            {
                lastDOCREF = sdrF["sl"] != DBNull.Value ? (int)sdrF["sl"] : 0;
            }
            cnn.Close();
            //Get financial period Settings -- create a separate function
            FinancialPeriod finPrd = new FinancialPeriod();
            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand("Select *  FROM financial_periods WHERE fp_branch = " + staff_branch + " AND fp_active = 't' ", cnn)
                .ExecuteReader();
            while (sdr11.Read())
            {
                finPrd.fp_id = sdr11["fp_id"] != DBNull.Value ? (int)sdr11["fp_id"] : 0;
                finPrd.fp_name = sdr11["fp_name"] != DBNull.Value ? (string)sdr11["fp_name"] : null;
                finPrd.fp_ref = sdr11["fp_ref"] != DBNull.Value ? (string)sdr11["fp_ref"] : null;
                finPrd.fp_trans_date = sdr11["fp_trans_date"] != DBNull.Value ? (DateTime)sdr11["fp_trans_date"] : DateTime.Today;
                finPrd.fp_openingdate = sdr11["fp_openingdate"] != DBNull.Value ? (DateTime)sdr11["fp_openingdate"] : DateTime.Today;
                finPrd.fp_closingdate = sdr11["fp_closingdate"] != DBNull.Value ? (DateTime)sdr11["fp_closingdate"] : DateTime.Today;
                finPrd.fp_active = sdr11["fp_active"] != DBNull.Value ? (bool)sdr11["fp_active"] : false;
                finPrd.fp_date_mode = sdr11["fp_date_mode"] != DBNull.Value ? (string)sdr11["fp_date_mode"] : null;
            }
            cnn.Close();
            //get last registered INV number
            int lastInvNumber = 0;
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                lastInvNumber = (int)sdr3["st"];
            }
            cnn.Close();
            //Get Invoice Settings -- create a separate function
            InvoiceSettings invsettings = new InvoiceSettings();
            cnn.Open();
            NpgsqlDataReader sdrInv = new NpgsqlCommand("Select *  FROM \"PLInvoiceSettings\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrInv.Read())
            {
                invsettings.InvPrefix = sdrInv["InvPrefix"].ToString();
                invsettings.InvStartNumber = (int)sdrInv["InvStartNumber"];
                invsettings.LastNumber = lastInvNumber;
                invsettings.InvNumberingType = sdrInv["InvNumberingType"].ToString();
            }
            cnn.Close();
            var lpo = this.GetLpoHeaderById(lpoId);
            cnn.OpenAsync();
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    /// start with the nljournal 
                    int invoicenumber = lastInvNumber + 1;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"PlJrnlNo\",\"SlJrnlNo\",\"ModuleId\") " +
                    "VALUES('" + recvData.NlJrnlDesc + "','" + recvData.TranDate + "','" + lpo.TransDate + "', '" + recvData.TranPeriod + "', '" + recvData.TranYear + "','PL','NT','" + invoicenumber + "',"+0+",'" + 0 + "' ) RETURNING \"NlJrnlNo\" ;";


                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    journalid = int.Parse(cmd.ExecuteScalar().ToString());
                    decimal vatAmount = 0;
                    decimal purchaseAmount = 0;
                    //total salesAmount+vatAmount
                    decimal creditors = 0;
                    /// update in plinvoice details
                    foreach (var draccount in InvoiceDetailsList)
                    {
                        vatAmount = vatAmount + draccount.VatAmt;
                        purchaseAmount = purchaseAmount + (draccount.ItemUnitPrice * draccount.ItemQty);
                    }
                    creditors = vatAmount + purchaseAmount;
                    string debitentry1 = "INSERT INTO \"NLJournalDetails\" " +
                        "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                        "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                 "VALUES('" + journalid + "','" + nlServices.GetNLAccountAccountCodeUsingName("VAT").NlaccCode + "','" + vatAmount + "', '', '" + (vatAmount) + "', ' VAT on purchases','narration','false','','false','false','" + vatAmount + "');";

                    string debitentry2 = "INSERT INTO \"NLJournalDetails\" " +
                      "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                      "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
               "VALUES('" + journalid + "','" + nlServices.GetNLAccountAccountCodeUsingName("PURCHASES").NlaccCode + "','" + purchaseAmount + "', '', '" + (purchaseAmount) + "', 'total purchase for this transaction','','false','','false','false','" + 0 + "');";


                    string creditentry1 = "INSERT INTO \"NLJournalDetails\" " +
                  "(\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\"," +
                  "\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
           "VALUES('" + journalid + "','" + nlServices.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','', '" + creditors + "', '" + (creditors) + "', 'amount to pay','','false','','false','false','" + 0 + "');";

                    var invoiceheaderref = lastPLjrnlNo + 1;
                    string plinvoiceHeader = "INSERT INTO \"PLInvoiceHeader\" (\"PLJrnlNo\", \"NlJrnlNo\", \"PLCustID\", \"TranDate\", \"Period\", \"DocRef\", \"InvDate\", \"CurrencyId\", \"PLDescription\",\"StaffId\",\"DocPrefix\",\"HasCreditNote\",\"DueDate\",\"Totals\",\"Balance\",\"PLBranch\",\"TotalDiscount\" ) VALUES(" + invoiceheaderref + ", " + journalid + ", " + lpo.LPOCustID + ", '" + DateTime.Today + "' ,'" + finPrd.fp_ref + "'," + (lastDOCREF + 1) + ",'" + DateTime.Now + "'," + lpo.CurrencyID + ",'" + lpo.LDescription  + "  invoice', " + userid + ", '" + invsettings.InvPrefix + "','f','" + lpo.TransDate+ "'," + creditors + "," + creditors + "," + staff_branch + "," + 0 + " ); ";
                                       
                    var inventorylist = GetInventoryItemsfromlpo(InvoiceDetailsList);
                    System.Text.StringBuilder invoicedetails = new System.Text.StringBuilder();
                    foreach (var invoiceitem in InvoiceDetailsList)
                    {

                        invoicedetails.Append("INSERT INTO \"PLInvoiceDetail\" (\"PLJrnlNo\", \"JrnlPLNo\", \"UnitPrice\", \"VatPerc\", \"VatAmt\", \"ProdGroupCode\", \"NLAccCode\", \"StkDesc\"," +
                            " \"UserID\", \"ProdQty\",\"DiscountAmt\",\"Total\",\"ProdId\" ) VALUES(" + invoiceheaderref + ", " + journalid + ", " + invoiceitem.ItemUnitPrice + ", '" + invoiceitem.VatCode + "', " + invoiceitem.VatAmt + ", '" + invoiceitem.ItemCode + "','" + null + "','" + invoiceitem.StkDesc + "'," + userid + ", " + invoiceitem.ItemQty + "," + 0 + ", " + invoiceitem.Total + ", " + invoiceitem.ItemId + " ); ");

                        // dont update inventory when creating an invoice
                        //var item = inventorylist.Where(x => x.InvtId == invoiceitem.ItemId).FirstOrDefault();
                        ////update inventory if goods
                        //if (item.InvtType == "GOODS")
                        //{
                        //    string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" + " + invoiceitem.ItemQty +
                        //                    " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + invoiceitem.ItemId + " ;";
                        //    invoicedetails.Append(up_inv);
                        //} 
                    }
                    ////  TO INVOICED WITH AN INVOICE NUMBER REFRENCE - THEN MODULARIZE THE FUNCTIONS ABOVE
                    ///
                    string LpoUpdate = "UPDATE \"LPOHeader\" SET \"Invoiced\" = "+true+",\"invoiceid\" = " + invoiceheaderref + " WHERE \"LID\" = " + lpoId + " ;";
                    cmd.CommandText = creditentry1 + debitentry2 + debitentry1+ plinvoiceHeader + invoicedetails+LpoUpdate;
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";

                }
            }
            return response;
        }
        public LpoHeader GetLpoHeaderById(int id)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            LpoHeader plInv = new LpoHeader();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"LPOHeader\".*, \"CrCode\", \"PLCustCode\", \"CustName\" FROM \"LPOHeader\" LEFT JOIN \"Currencies\"  ON \"CrId\" = \"CurrencyID\" LEFT JOIN  \"PLCustomer\" ON \"CustID\" = \"LPOCustID\" WHERE \"LID\" = " + id + "  ", cnn).ExecuteReader();
            while (sdr0.Read())
            {

                         plInv.LID = sdr0["LID"] != DBNull.Value ? (int)sdr0["LID"] : 0;
                        plInv.LPOCustID = sdr0["LPOCustID"] != DBNull.Value ? (int)sdr0["LPOCustID"] : 0;
                       plInv.LPODate = sdr0["LPODate"] != DBNull.Value ? (DateTime)sdr0["LPODate"] : DateTime.Today;
                         plInv.TransDate = sdr0["TransDate"] != DBNull.Value ? (DateTime)sdr0["TransDate"] : DateTime.Today;
                        plInv.Prefix = sdr0["Prefix"] != DBNull.Value ? (string)sdr0["Prefix"] : null;
                        plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (int)sdr0["DocRef"] : 0;
                       plInv.CurrencyID = sdr0["CurrencyID"] != DBNull.Value ? (int)sdr0["CurrencyID"] : 0;
                       plInv.LDescription = sdr0["LDescription"] != DBNull.Value ? (string)sdr0["LDescription"] : null;
                        plInv.StaffID = sdr0["StaffID"] != DBNull.Value ? (int)sdr0["StaffID"] : 0;
                        plInv.Totals = sdr0["LID"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                          plInv.Invoiced = (bool) sdr0["Invoiced"];

                          plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;

                        plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                        plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;

                

               
            }
            cnn.Close();

            return plInv;
        }
        public List<InvoiceListDetailsData> GetLpoHeaderDetails(int headerid)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            cnn.Open();
            List<InvoiceListDetailsData> myLPODetails = new List<InvoiceListDetailsData>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT a.\"VatPerc\",a.\"VatAmt\",a.\"StkDesc\",a.\"ProdQty\",a.\"Total\",a.\"UnitPrice\",b.\"InvtId\"  FROM \"LPODetails\" a  LEFT JOIN \"Inventory\" b ON b.\"InvtName\" = a.\"StkDesc\"  WHERE \"PldRef\" = " + headerid + "  ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                InvoiceListDetailsData lpdet = new InvoiceListDetailsData();
                lpdet.ItemId = sdr1["InvtId"] != DBNull.Value ? (int)sdr1["InvtId"] : 0;

                string percentage = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : "";
                lpdet.VatCode = Int32.Parse(percentage);
                lpdet.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                lpdet.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : null;
                lpdet.ItemQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                lpdet.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;
                lpdet.ItemUnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;

                myLPODetails.Add(lpdet);
            }
            cnn.Close();

            return myLPODetails;
        }
        public List<Inventory> GetInventoryItemsfromlpo(List<InvoiceListDetailsData> InvoiceDetailsList)
        {
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<Inventory> lpoInventory = new List<Inventory>();
            //get inventory item id
            try
            {              
                for (int i = 0; i < InvoiceDetailsList.Count; i++)
                {
                    cnn1.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr_inv = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + InvoiceDetailsList[i].ItemId + "  ", cnn1).ExecuteReader();
                    while (sdr_inv.Read())
                    {
                        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                        inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                        inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                        inv.InvtId = sdr_inv["InvtId"] != DBNull.Value ? (int)sdr_inv["InvtId"] :0;
                        lpoInventory.Add(inv);
                    }
                    cnn1.Close();
                }     
                return lpoInventory;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return lpoInventory;
        }
        public List<PlCustomerStatement> GetCustomerStatement(int cust_id)
        {
            //INVOICE QUERY
            string query = "select a.\"NlJrnlNo\", b.\"Dr\", b.\"Cr\", c.\"Totals\", c.\"Balance\" as balance , a.\"TranFrom\", d.\"CustName\", c.\"InvDate\", concat(c.\"DocPrefix\", c.\"DocRef\") as docref, c.\"PLCustID\",  " +
                " c.\"PLDescription\", c.\"DocPrefix\", f.\"NlAccName\", a.\"TranPeriod\", a.\"TranYear\"   " +
                "from \"NlJournalHeader\" a  INNER JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" INNER JOIN \"PLInvoiceHeader\" c on c.\"PLJrnlNo\" = a.\"PlJrnlNo\" INNER JOIN \"PLCustomer\" d on d.\"CustID\" = c.\"PLCustID\"  INNER JOIN \"NLAccount\" f on f.\"NlAccCode\" = b.\"NlAccCode\"     " +
                "where c.\"PLCustID\" = '"+cust_id+"' and a.\"TranFrom\" = 'PL'  AND f.\"NlAccName\" = 'CREDITORS' AND C.\"Balance\"::NUMERIC > 0     " +
                "ORDER BY A.\"NlJrnlNo\" DESC; ";
            //PAYMENT QUERY
            string payQuery = "SELECT a.\"pyID\", a.\"pyDate\", a.allocation_remainder, d.\"Dr\", d.\"Cr\", a.\"pyBalance\", c.\"CustName\", a.\"pyChequeNumber\", a.\"pyAdditionalDetails\", concat(b.\"TranPeriod\", '/', b.\"TranYear\") as period    " +
                "FROM \"PLReceipts\" a  LEFT JOIN \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"pyID\" LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.supplier_id LEFT JOIN \"NLJournalDetails\" d on d.\"JrnlSlNo\" = b.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" e on e.\"NlAccCode\" = d.\"NlAccCode\"    " +
                "where a.supplier_id = '"+cust_id+"' and b.\"TranFrom\" = 'PL-PY' and e.\"NlAccName\" ~*'CREDITORS' and a.\"pyBalance\"::numeric > 0; ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand( query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PlCustomerStatement plCustomerStatement = new PlCustomerStatement();
                plCustomerStatement.documentRef = sdr1["docref"] != DBNull.Value ? (string)sdr1["docref"] : null;
                plCustomerStatement.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                //plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                plCustomerStatement.invDate = (DateTime)sdr1["InvDate"];
                plCustomerStatement.description = sdr1["PLDescription"] != DBNull.Value ? (string)sdr1["PLDescription"] : "";
                plCustomerStatement.Balance = sdr1["balance"] != DBNull.Value ? (decimal)sdr1["balance"] : 0;
                if(plCustomerStatement.Balance > 0)
                {
                    plCustomerStatement.Cr = sdr1["balance"] != DBNull.Value ? (decimal)sdr1["balance"] : 0;
                }
                else
                {
                    plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                }
                customerStatement.Add(plCustomerStatement);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand( payQuery, cnn ).ExecuteReader();
            while (sdr2.Read())
            {
                PlCustomerStatement statement = new PlCustomerStatement();
                statement.documentRef = sdr2["pyChequeNumber"] != DBNull.Value ? (string)sdr2["pyChequeNumber"] : "";
                statement.description = sdr2["pyAdditionalDetails"] != DBNull.Value ? (string)sdr2["pyAdditionalDetails"] : "";
                statement.invDate = (DateTime)sdr2["pyDate"];
                statement.Balance = sdr2["pyBalance"] != DBNull.Value ? (decimal)(float)sdr2["pyBalance"] : 0;
                //statement.Dr = sdr2["Dr"] != DBNull.Value ? (decimal)sdr2["Dr"] : 0;
                statement.Cr = sdr2["Cr"] != DBNull.Value ? (decimal)sdr2["Cr"] : 0;
                if(statement.Balance > 0)
                {
                    statement.Dr = sdr2["allocation_remainder"] != DBNull.Value ? (decimal)sdr2["allocation_remainder"] : (decimal)(float)sdr2["pyBalance"];
                }
                else
                {
                    statement.Dr = sdr2["Dr"] != DBNull.Value ? (decimal)sdr2["Dr"] : 0;
                }
                customerStatement.Add(statement);
            }
            cnn.Close();
            return customerStatement;
        }
        public PLCustomer GetCustomerById(int id)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"PLCustomer\" WHERE \"CustID\" = " + id + "  ", cnn).ExecuteReader();
            PLCustomer plCust = new PLCustomer();
            while (sdr0.Read())
            {     
                plCust.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                plCust.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                plCust.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                plCust.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                plCust.CurrID = sdr0["CurrID"] != DBNull.Value ? (int)sdr0["CurrID"] : 0;
                plCust.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                plCust.CustID = sdr0["CustID"] != DBNull.Value ? (int)sdr0["CustID"] : 0;
                plCust.CustID = sdr0["CustID"] != DBNull.Value ? (int)sdr0["CustID"] : 0;
                plCust.RegisterDate = sdr0["RegisterDate"] != DBNull.Value ? (DateTime)sdr0["RegisterDate"] : DateTime.Now;
                plCust.StaffID = sdr0["StaffID"] != DBNull.Value ? (int)sdr0["StaffID"] : 0;
                plCust.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                plCust.OpeningBalance = sdr0["OpeningBalance"] != DBNull.Value ? (decimal)sdr0["OpeningBalance"] : 0;
                plCust.OpeningBalanceDate = sdr0["OpeningBalanceDate"] != DBNull.Value ? (DateTime)sdr0["OpeningBalanceDate"] : DateTime.Now;
                //plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;               
            }
            cnn.Close();
            return plCust;
        }
        public PlCustomerStatement GetInvoiceBalance(int custId, DateTime from)
        {
            string invoiceBalance = "select a.\"PLCustID\", e.\"OpeningBalance\", sum(case when a.\"InvDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Cr\"::numeric else 0 end ) - sum(case when a.\"InvDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Dr\"::numeric else 0 end ) as InvoiceBalance      " +
                "from \"PLInvoiceHeader\" a left join \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" left join \"NLJournalDetails\" c on c.\"JrnlSlNo\" = b.\"NlJrnlNo\" left join \"NLAccount\" d on d.\"NlAccCode\" = c.\"NlAccCode\" left join \"PLCustomer\" e on e.\"CustID\" = a.\"PLCustID\"     " +
                "where a.\"PLCustID\" = '"+custId+"' and b.\"TranFrom\" = 'PL' and d.\"NlAccName\" ~* 'CREDITORS' group by a.\"PLCustID\", e.\"OpeningBalance\"; ";            

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            PlCustomerStatement customerStatement = new PlCustomerStatement();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoiceBalance, cnn).ExecuteReader();
            while (reader.Read())
            {
                customerStatement.InvoiceBalance = reader["InvoiceBalance"] != DBNull.Value ? (decimal)reader["InvoiceBalance"] : 0;
            }
            cnn.Close();
            return customerStatement;
        }
        public PlCustomerStatement GetCrnBalanceForward(int custId, DateTime from)
        {
            string crnQuery = "select a.\"PLCustID\", e.\"OpeningBalance\", sum(case when a.\"CRNDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Cr\"::numeric else 0 end) - sum(case when a.\"CRNDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Dr\"::numeric else 0 end) as crnBalance    " +
                "from \"PLInvoiceHeader\" a left join \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"PLJrnlNo\" left join \"NLJournalDetails\" c on c.\"JrnlSlNo\" = b.\"NlJrnlNo\" left join \"NLAccount\" d on d.\"NlAccCode\" = c.\"NlAccCode\" left join \"PLCustomer\" e on e.\"CustID\" = a.\"PLCustID\"   " +
                "where a.\"PLCustID\" = '"+custId+"' and b.\"TranFrom\" = 'PL-CRN' and d.\"NlAccName\" ~* 'CREDITORS' group by a.\"PLCustID\", e.\"OpeningBalance\";   ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            PlCustomerStatement state = new PlCustomerStatement();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(crnQuery, cnn).ExecuteReader();
            while(reader.Read())
            {
                state.CrnBalance = reader["crnBalance"] != DBNull.Value ? (decimal)reader["crnBalance"] : 0;
            }
            cnn.Close();
            return state;
        }
        public PlCustomerStatement GetPaymentBalanceForward(int custId, DateTime from)
        {
            string paymentBalance = "select a.supplier_id, e.\"OpeningBalance\", sum(case when a.\"pyDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Cr\"::numeric else 0 end) - sum(case when a.\"pyDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Dr\"::numeric else 0 end) as payBalance     " +
                "from \"PLReceipts\" a left join \"NlJournalHeader\" b on b.\"PlJrnlNo\" = a.\"pyID\" left join \"NLJournalDetails\" c on c.\"JrnlSlNo\" = b.\"NlJrnlNo\" left join \"NLAccount\" d on d.\"NlAccCode\" = c.\"NlAccCode\" left join \"PLCustomer\" e on e.\"CustID\" = a.supplier_id     " +
                "where a.supplier_id = '"+custId+"' and b.\"TranFrom\" = 'PL-PY' and d.\"NlAccName\" ~* 'CREDITORS'\r\ngroup by a.supplier_id, e.\"OpeningBalance\";    ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            PlCustomerStatement pay = new PlCustomerStatement();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(paymentBalance, cnn).ExecuteReader();
            while (reader.Read())
            {
                pay.PayBalance = reader["payBalance"] != DBNull.Value ? (decimal)reader["payBalance"] : 0;
            }
            cnn.Close();
            return pay;
        }
        public List<PlCustomerStatement> GetPurchaseLedgerUnAllocatedPayment(int id, DateTime from, DateTime to)
        {
            string query = "select a.\"NlJrnlNo\", a.\"TranDate\", d.\"pyDate\", b.\"Dr\", b.\"Cr\", d.supplier_id, a.\"TranFrom\", d.\"pyChequeNumber\", d.\"pyAdditionalDetails\"    " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c on c.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"PLReceipts\" d on d.\"pyID\" = a.\"PlJrnlNo\"     " +
                "WHERE a.\"TranFrom\" = 'PL-PY' and d.supplier_id = '"+id+"' and d.allocation_remainder > '0' and c.\"NlAccName\" ~*'CREDITORS' and d.\"pyDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"' " +
                "ORDER BY a.\"NlJrnlNo\" DESC ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PlCustomerStatement plCustomerStatement = new PlCustomerStatement();
                plCustomerStatement.documentRef = sdr1["pyChequeNumber"] != DBNull.Value ? (string)sdr1["pyChequeNumber"] : null;
                plCustomerStatement.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                plCustomerStatement.transtype = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : null;
                plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                plCustomerStatement.description = sdr1["pyAdditionalDetails"] != DBNull.Value ? (string)sdr1["pyAdditionalDetails"] : null;
                plCustomerStatement.invDate = (DateTime)sdr1["pyDate"];
                customerStatement.Add(plCustomerStatement);
            }
            cnn.Close();
            return customerStatement;
        }
        public List<PlCustomerStatement> GetPurchaseLedgerAllocatedPayment(int id, DateTime from, DateTime to)
        {
            string query = "select a.\"NlJrnlNo\", a.\"TranDate\", d.\"pyDate\", b.\"Dr\", b.\"Cr\", d.supplier_id, a.\"TranFrom\", d.\"pyChequeNumber\", d.\"pyAdditionalDetails\"    " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c on c.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"PLReceipts\" d on d.\"pyID\" = a.\"PlJrnlNo\"     " +
                "WHERE a.\"TranFrom\" = 'PL-PY' and d.supplier_id = '"+id+"' and (d.allocation_remainder is null OR d.allocation_remainder <= '0') and c.\"NlAccName\" ~*'CREDITORS' and d.\"pyDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"' " +
                "ORDER BY a.\"NlJrnlNo\" DESC ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PlCustomerStatement plCustomerStatement = new PlCustomerStatement();
                plCustomerStatement.documentRef = sdr1["pyChequeNumber"] != DBNull.Value ? (string)sdr1["pyChequeNumber"] : null;
                plCustomerStatement.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                plCustomerStatement.transtype = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : null;
                plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                plCustomerStatement.description = sdr1["pyAdditionalDetails"] != DBNull.Value ? (string)sdr1["pyAdditionalDetails"] : null;
                plCustomerStatement.invDate = (DateTime)sdr1["pyDate"];
                customerStatement.Add(plCustomerStatement);
            }
            cnn.Close();
            return customerStatement;
        }
        public List<PlCustomerStatement> GetPurchaseLedgerReversal(int id, DateTime from, DateTime to)
        {
            string query = "select a.\"NlJrnlNo\", a.\"TranDate\", c.\"InvDate\", b.\"Dr\", b.\"Cr\", c.\"PLCustID\", a.\"TranFrom\", concat(C.\"DocPrefix\",'-', C.\"DocRef\") as docref, c.\"PLDescription\"  " +
                "from \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"PLInvoiceHeader\" c on c.\"PLJrnlNo\" = a.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\"  " +
                "WHERE a.\"TranFrom\" = 'PL-REVERSAL'  AND c.\"PLCustID\" = '"+id+"' and c.\"InvDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'    " +
                "ORDER BY a.\"NlJrnlNo\" DESC;  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PlCustomerStatement plCustomerStatement = new PlCustomerStatement();
                plCustomerStatement.documentRef = sdr1["docref"] != DBNull.Value ? (string)sdr1["docref"] : null;
                plCustomerStatement.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                plCustomerStatement.transtype = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : null;
                plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                plCustomerStatement.description = sdr1["PLDescription"] != DBNull.Value ? (string)sdr1["PLDescription"] : null;
                plCustomerStatement.invDate = (DateTime)sdr1["InvDate"];
                customerStatement.Add(plCustomerStatement);
            }
            cnn.Close();
            return customerStatement;
        }
        public List<PlCustomerStatement> GetPurchaseLedgerCreditLimit(int id, DateTime from, DateTime to)
        {
            //string query = "select a.\"NlJrnlNo\", a.\"TranDate\", c.\"InvDate\", c.\"CRNDate\", b.\"Dr\", b.\"Cr\", c.\"PLCustID\", a.\"TranFrom\", c.\"CRNReference\" as docref, c.\"Additionals\", concat('CRN', c.\"PLJrnlNo\") as inv     " +
            //    "from \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\"    " +
            //    "LEFT JOIN \"PLInvoiceHeader\" c on c.\"PLJrnlNo\" = a.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\"     " +
            //    "WHERE a.\"TranFrom\" = 'PL-CRN' and d.\"NlAccName\" ~* 'CREDITORS'  AND c.\"PLCustID\" = '"+id+"' and c.\"CRNDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'   " +
            //    "ORDER BY a.\"NlJrnlNo\" DESC;  ";
            string query = "select a.\"NlJrnlNo\", a.\"TranDate\", c.\"CRNTotal\", c.\"InvDate\", c.\"CRNDate\", b.\"Dr\", b.\"Cr\", c.\"PLCustID\", a.\"TranFrom\", c.\"CRNReference\" as docref, c.\"Additionals\", concat('CRN', c.\"PLJrnlNo\") as inv   " +
                "from \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\"    LEFT JOIN \"PLInvoiceHeader\" c on c.\"PLJrnlNo\" = a.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\"    " +
                "WHERE a.\"TranFrom\" = 'PL-CRN' and d.\"NlAccName\" ~* 'CREDITORS'  AND c.\"PLCustID\" = '"+id+"' and c.\"CRNDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'   " +
                "group by a.\"NlJrnlNo\", a.\"TranDate\", c.\"CRNTotal\", c.\"InvDate\", c.\"CRNDate\", b.\"Dr\", b.\"Cr\", c.\"PLCustID\", a.\"TranFrom\", c.\"CRNReference\", c.\"Additionals\", inv   ORDER BY a.\"NlJrnlNo\" DESC;  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PlCustomerStatement plCustomerStatement = new PlCustomerStatement();
                plCustomerStatement.documentRef = sdr1["docref"] != DBNull.Value ? (string)sdr1["docref"] : (string)sdr1["inv"];
                plCustomerStatement.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                plCustomerStatement.transtype = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : null;
                plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                plCustomerStatement.description = sdr1["Additionals"] != DBNull.Value ? (string)sdr1["Additionals"] : null;
                plCustomerStatement.invDate = (DateTime)sdr1["CRNDate"];
                customerStatement.Add(plCustomerStatement);
            }
            cnn.Close();
            return customerStatement;
        }
        public List<PlCustomerStatement> GetCustomerLedger(int id, DateTime from, DateTime to)
        {
            string query = " SELECT DISTINCT ON(A.\"NlJrnlNo\")A.\"NlJrnlNo\" ,A.\"TranDate\",C.\"InvDate\",B.\"Dr\",E.\"pyMode\",B.\"Cr\",concat(C.\"DocPrefix\", C.\"DocRef\") as docref, A.\"TranFrom\",D.\"NlAccName\"   " +
                " FROM \"NlJournalHeader\" A LEFT JOIN \"NLJournalDetails\" B ON B.\"JrnlSlNo\" = A.\"NlJrnlNo\" LEFT JOIN \"PLInvoiceHeader\" C ON C.\"PLJrnlNo\" = A.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" D ON D.\"NlAccCode\" = B.\"NlAccCode\" LEFT JOIN \"PLReceipts\" E ON E.\"pyInvRef\" = A.\"PlJrnlNo\" " +
                "WHERE A.\"TranFrom\" IN('PL', 'PL-PY', 'PL-CRN') AND D.\"NlAccName\" = 'CREDITORS'     AND C.\"PLCustID\" = " + id + " ORDER BY  A.\"NlJrnlNo\",A.\"TranDate\" DESC; ";

            string query1 = "select a.\"NlJrnlNo\", a.\"TranDate\", c.\"InvDate\", b.\"Dr\", b.\"Cr\", c.\"PLCustID\", a.\"TranFrom\", concat(C.\"DocPrefix\",'-', C.\"DocRef\") as docref, c.\"PLDescription\"     " +
                "from \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"PLInvoiceHeader\" c on c.\"PLJrnlNo\" = a.\"PlJrnlNo\"  LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\"     " +
                "WHERE a.\"TranFrom\" = 'PL' and d.\"NlAccName\" ~*'CREDITORS'  AND c.\"PLCustID\" = '"+id+"' and c.\"InvDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'      " +
                "ORDER BY a.\"NlJrnlNo\" DESC; ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PlCustomerStatement plCustomerStatement = new PlCustomerStatement();
                plCustomerStatement.documentRef = sdr1["docref"] != DBNull.Value ? (string)sdr1["docref"] : null;
                plCustomerStatement.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                plCustomerStatement.transtype = sdr1["TranFrom"] != DBNull.Value ? (string)sdr1["TranFrom"] : null;
                plCustomerStatement.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                plCustomerStatement.description = sdr1["PLDescription"] != DBNull.Value ? (string)sdr1["PLDescription"] :null;
                plCustomerStatement.invDate = (DateTime)sdr1["InvDate"];
                customerStatement.Add(plCustomerStatement);
            }
            cnn.Close();
            var unAllocatedPay = GetPurchaseLedgerUnAllocatedPayment(id, from, to);
            var allocatedPay = GetPurchaseLedgerAllocatedPayment(id, from, to);
            var reversal = GetPurchaseLedgerReversal(id, from, to);
            var creditLimit = GetPurchaseLedgerCreditLimit(id, from, to);
            customerStatement.AddRange(unAllocatedPay);
            customerStatement.AddRange(allocatedPay);
            customerStatement.AddRange(reversal);
            customerStatement.AddRange(creditLimit);
            customerStatement.Sort((a,b) => a.invDate.CompareTo(b.invDate));
            return customerStatement;
        }
        public List<PlCustomerStatement> GetSupplierActivity(int custId, DateTime from, DateTime to)
        {
            string invoiceQuery = "SELECT a.\"Totals\", sum(b.\"VatAmt\") as vatAmount, a.\"TranDate\", concat('INV', a.\"DocRef\") as inv,  concat(e.\"TranPeriod\", '/', e.\"TranYear\") as period, a.\"PLJrnlNo\" , a.\"InvDate\", f.\"Dr\"::numeric, f.\"Cr\"::numeric, a.\"PLDescription\", e.\"TranFrom\", a.\"Balance\"     " +
                "FROM \"PLInvoiceHeader\" a  LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NlJournalHeader\" e on e.\"PlJrnlNo\" = a.\"PLJrnlNo\"     LEFT JOIN \"NLJournalDetails\" f on f.\"JrnlSlNo\" = e.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" g on g.\"NlAccCode\" = f.\"NlAccCode\"   " +
                "WHERE a.\"PLCustID\" = '"+custId+"' and a.\"InvDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and e.\"TranFrom\" = 'PL' and  g.\"NlAccName\" ~* 'CREDITORS'   " +
                "GROUP BY a.\"Totals\",  a.\"TranDate\", inv, e.\"TranPeriod\", e.\"TranYear\", a.\"PLJrnlNo\", a.\"InvDate\", f.\"Dr\", f.\"Cr\", a.\"PLDescription\", e.\"TranFrom\", a.\"Balance\"   ;   ";

            string payQuery = "select a.\"pyPaid\"::numeric, a.\"pyDate\", a.\"pyID\", concat('', a.\"pyChequeNumber\") as inv, concat(c.\"TranPeriod\", '/', c.\"TranYear\") as period, 0.00 as vatAmount, d.\"Dr\"::numeric, d.\"Cr\"::numeric, a.\"pyAdditionalDetails\", c.\"TranFrom\", a.\"pyBalance\"     " +
                "FROM \"PLReceipts\" a LEFT JOIN \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"pyID\" LEFT JOIN \"NLJournalDetails\" d on d.\"JrnlSlNo\" = c.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" e on e.\"NlAccCode\" = d.\"NlAccCode\"       " +
                "WHERE a.\"supplier_id\" = '"+custId+"' and a.\"pyDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'PL-PY' and e.\"NlAccName\" ~* 'CREDITORS'     " +
                "GROUP BY a.\"pyPaid\", a.\"pyDate\", a.\"pyID\", inv, c.\"TranPeriod\", c.\"TranYear\", vatAmount, d.\"Dr\", d.\"Cr\", a.\"pyAdditionalDetails\", c.\"TranFrom\", a.\"pyBalance\";    ";

            string crnQuery = "SELECT a.\"CRNTotal\", a.\"Balance\", a.\"CRNVat\" as vatAmount, a.\"TranDate\", a.\"CRNDate\", concat('CRN', a.\"PLJrnlNo\") as inv, a.\"CRNReference\",concat(d.\"TranPeriod\", '/', d.\"TranYear\") as period, a.\"PLJrnlNo\", a.\"InvDate\", e.\"Dr\"::numeric, e.\"Cr\"::numeric, a.\"Additionals\", d.\"TranFrom\"   " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\"  LEFT JOIN \"NlJournalHeader\" d on d.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NLJournalDetails\" e on e.\"JrnlSlNo\" = d.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = e.\"NlAccCode\"    " +
                "where a.\"PLCustID\" = '"+custId+"' and a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and d.\"TranFrom\" = 'PL-CRN' and f.\"NlAccName\" ~* 'CREDITORS'     " +
                "GROUP BY a.\"CRNTotal\", a.\"Balance\", a.\"CRNVat\",  a.\"TranDate\", inv, d.\"TranPeriod\", d.\"TranYear\", a.\"CRNDate\", a.\"PLJrnlNo\", a.\"InvDate\", e.\"Dr\", e.\"Cr\", a.\"Additionals\", a.\"CRNReference\", d.\"TranFrom\"; ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<PlCustomerStatement> customerStatement = new List<PlCustomerStatement>();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoiceQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                PlCustomerStatement statement = new PlCustomerStatement();
                statement.documentRef = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                statement.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                statement.Cr = reader["Dr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                statement.transtype = reader["TranFrom"] != DBNull.Value ? (string)reader["TranFrom"] : "";
                statement.description = reader["PLDescription"] != DBNull.Value ? (string)reader["PLDescription"] : "";
                statement.invDate = (DateTime)reader["InvDate"];
                statement.Balance = reader["Balance"] != DBNull.Value ? (decimal)reader["Balance"] : 0;
                customerStatement.Add(statement);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand(payQuery, cnn).ExecuteReader();
            while (reader1.Read())
            {
                PlCustomerStatement plCustomer = new PlCustomerStatement();
                plCustomer.documentRef = reader1["inv"] != DBNull.Value ? (string)reader1["inv"] : "";
                plCustomer.Dr = reader1["Dr"] != DBNull.Value ? (decimal)reader1["Dr"] : 0;
                plCustomer.Cr = reader1["Cr"] != DBNull.Value ? (decimal)reader1["Cr"] : 0;
                plCustomer.transtype = reader1["TranFrom"] != DBNull.Value ? (string)reader1["TranFrom"] : "";
                plCustomer.description = reader1["pyAdditionalDetails"] != DBNull.Value ? (string)reader1["pyAdditionalDetails"] : "";
                plCustomer.invDate = (DateTime)reader1["pyDate"];
                plCustomer.Balance = reader1["pyBalance"] != DBNull.Value ? Convert.ToDecimal(reader1["pyBalance"]) : 0;
                customerStatement.Add(plCustomer);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader2 = new NpgsqlCommand(crnQuery, cnn).ExecuteReader();
            while (reader2.Read())
            {
                PlCustomerStatement pl = new PlCustomerStatement();
                pl.documentRef = reader2["CRNReference"] != DBNull.Value ? (string)reader2["CRNReference"] : (string)reader2["inv"];
                pl.Dr = reader2["Dr"] != DBNull.Value ? (decimal)reader2["Dr"] : 0;
                pl.Cr = reader2["Cr"] != DBNull.Value ? (decimal)reader2["Cr"] : 0;
                pl.transtype = reader2["TranFrom"] != DBNull.Value ? (string)reader2["TranFrom"] : "";
                pl.description = reader2["Additionals"] != DBNull.Value ? (string)reader2["Additionals"] : "";
                pl.invDate = (DateTime)reader2["CRNDate"];

                customerStatement.Add(pl);
            }
            cnn.Close();
            customerStatement.Sort((x, y) => x.invDate.CompareTo(y.invDate));
            return customerStatement;
        }
        public List<AccountsPayableAgeingReport> getAccountsPayableAgeingReport()
        {
            //string query = "select b.\"CustName\" AS customer,a.\"PLCustID\",\t\t\t\t\r\n \t\t\t\t\t( SELECT SUM(\"Balance\" ::numeric)\t\r\n\t\t  from \"PLInvoiceHeader\"   \t\r\n        Left JOIN \"PLCustomer\" ON \"PLCustomer\".\"CustID\"  = \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t  WHERE  CURRENT_DATE - \"DueDate\"::date > 91 AND  \"PLCustomer\".\"CustID\" = a.\"PLCustID\"\r\n\t\t\t\t\t\tGROUP BY \"PLInvoiceHeader\".\"PLCustID\"\t\t\t\t\t\r\n\t\t\t   )\r\n\t\t\t  as \"Over90Days\",\t\r\n \t\t\t\t ( SELECT SUM(\"Balance\" ::numeric)\r\n\t\t\t  from \"PLInvoiceHeader\" \r\n \t\t\t\t\t    Left JOIN \"PLCustomer\" ON \"PLCustomer\".\"CustID\"  =  \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t\t  WHERE  ( CURRENT_DATE - \"DueDate\"::date) BETWEEN 61 AND 90 AND  \"PLCustomer\".\"CustID\" = a.\"PLCustID\"\r\n\t\t\t\t \t\tGROUP BY \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t ) as \"61-90 days\" ,\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\r\n\t\t\t\t ( SELECT  SUM(\"Balance\"::numeric)\r\n\t\t\t\t  from \"PLInvoiceHeader\" \r\n\t\t\t\t\t  Left JOIN \"PLCustomer\" ON \"PLCustomer\".\"CustID\"  =  \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t  WHERE  ( CURRENT_DATE - \"DueDate\"::date) BETWEEN 31 AND 60 AND \"PLCustomer\".\"CustID\" = a.\"PLCustID\"\r\n\t\t\t\t \tGROUP BY \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t ) as \"31-60 days\" ,\t\t\t\r\n\t\t\t\t\t( SELECT SUM(\"Balance\"::numeric)\r\n\t\t\t\t\t  from \"PLInvoiceHeader\" \r\n\t\t\t\t\t \t   Left JOIN \"PLCustomer\" ON \"PLCustomer\".\"CustID\"  =  \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t\t  WHERE  ( CURRENT_DATE - \"DueDate\"::date) BETWEEN 1 AND 30 AND \"PLCustomer\".\"CustID\" = a.\"PLCustID\"\r\n\t\t\t\t\t\tGROUP BY \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t\t)   as \"1-30days\" ,\r\n \t\t\t\t\t ( SELECT  SUM(\"Balance\"::numeric)\r\n\t\t\t\t  from \"PLInvoiceHeader\" \r\n\t\t\t\t   Left JOIN \"PLCustomer\" ON \"PLCustomer\".\"CustID\"  = \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t  WHERE  CURRENT_DATE  = \"DueDate\"::date AND \"PLCustomer\".\"CustID\" = a.\"PLCustID\"\r\n\t\t\t\t\t \tGROUP BY \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t\t ) as \"Current\" \t   \t\t\t\t\t\t\t\t\r\nfrom \"PLInvoiceHeader\" a \r\nLEFT join \"PLCustomer\" b  on b.\"CustID\" = a.\"PLCustID\"\r\nWHERE a.\"HasCreditNote\" = false\r\nGROUP by  customer,a.\"PLCustID\";";
            string query2 = "SELECT main.\"CustName\" as customer,SUM(main.\"61-90 days\") as \"61-90 days\" , SUM(main.\"31-60 days\") as \"31-60 days\" ,SUM(main.current) as Current, SUM(main.\"Over90Days\") as \"Over90Days\"\r\nFROM(SELECT \"PLInvoiceHeader\".\"PLCustID\",\"PLCustomer\".\"CustName\",\r\n\t CASE WHEN extract(day from CURRENT_DATE - \"InvDate\"::timestamp)  \r\n\t BETWEEN 1 and 30 THEN  SUM(\"Balance\"::numeric) ELSE  0 END AS current,\r\n\t \t CASE WHEN extract(day from CURRENT_DATE - \"InvDate\"::timestamp)\r\n\t\t BETWEEN 31 and 60 THEN  SUM(\"Balance\"::numeric) ELSE  0 END AS \"31-60 days\",\r\n\t\t \t CASE WHEN extract(day from CURRENT_DATE - \"InvDate\"::timestamp)\r\n\t\t\t BETWEEN 61 and 90 THEN  SUM(\"Balance\"::numeric) ELSE  0 END AS \"61-90 days\",\r\n\t\t\t  CASE WHEN extract(day from CURRENT_DATE - \"InvDate\"::timestamp) > 90 \r\n\t\t\t  THEN  SUM(\"Balance\"::numeric) ELSE  0 END AS \"Over90Days\"\r\nFROM \"PLInvoiceHeader\" \r\nLEFT JOIN \"PLCustomer\" on \"PLCustomer\".\"CustID\" = \"PLInvoiceHeader\".\"PLCustID\"\r\nGROUP by  \"PLInvoiceHeader\".\"PLCustID\",\"InvDate\",\"PLCustomer\".\"CustName\") as main\r\nGROUP BY customer\r\n";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<AccountsPayableAgeingReport> accountsPayableAgeingReports = new List<AccountsPayableAgeingReport>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                AccountsPayableAgeingReport account= new AccountsPayableAgeingReport();
                account.customer = sdr1["customer"] != DBNull.Value ? (string)sdr1["customer"] : null;
               account.Current = sdr1["Current"] != DBNull.Value ? (decimal)sdr1["Current"] : 0;
                //account.oneto30days = sdr1["1-30days"] != DBNull.Value ? (decimal)sdr1["1-30days"] : 0;
                account.thirtyoneto60days = sdr1["31-60 days"] != DBNull.Value ? (decimal)sdr1["31-60 days"] : 0;
                account.sixtoneto90days = sdr1["61-90 days"] != DBNull.Value ? (decimal)sdr1["61-90 days"] : 0;
                account.Over90Days = sdr1["Over90Days"] != DBNull.Value ? (decimal)sdr1["Over90Days"] : 0;
                accountsPayableAgeingReports.Add(account);
            }
            cnn.Close();
            return accountsPayableAgeingReports;
        }
        public List<DetailedAccountsReceivableAgeingReport> getSupplierDetailAgeingReport(string custid)
        {
            List<DetailedAccountsReceivableAgeingReport> detailedAccountsReceivableAgeingReports = new List<DetailedAccountsReceivableAgeingReport>();
            string query3 = " SELECT * FROM (SELECT \"PLInvoiceHeader\".\"TranDate\",\"PLInvoiceHeader\".\"PLCustID\",\"PLInvoiceHeader\".\"DueDate\",\r\n\t\t\t   \"PLInvoiceHeader\".\"Balance\",\"PLInvoiceHeader\".\"Totals\",\r\n-- CASE WHEN (\"SLInvoiceHeader\".\"CreditNoteAmount\" IS NULL) THEN '0' ELSE \"SLInvoiceHeader\".\"CreditNoteAmount\" END as cramount ,\r\n\t\t\t   concat('INV',\"PLInvoiceHeader\".\"PLJrnlNo\") as invoice,CASE\r\nWHEN extract(day from CURRENT_DATE - \"TranDate\"::timestamp)  \r\n\t BETWEEN 0 and 30 THEN  'current' \r\n\t WHEN extract(day from CURRENT_DATE - \"TranDate\"::timestamp)  \r\n\t BETWEEN 31 and 60 THEN  '31-60 days' \r\n\t \t WHEN extract(day from CURRENT_DATE - \"TranDate\"::timestamp)  \r\n\t BETWEEN 61 and 90 THEN  '61-90 days' \r\n\t \t WHEN extract(day from CURRENT_DATE - \"TranDate\"::timestamp)  \r\n\t>90 THEN  'Over90Days'  \r\n\t ELSE  '' END AS timeframe FROM \"PLInvoiceHeader\" \r\n\t\t\t\t\tLEFT JOIN \"PLCustomer\"  on \"PLCustomer\".\"CustID\" = \"PLInvoiceHeader\".\"PLCustID\"\r\n\t\t\t\t\t\r\n\tWHERE \"PLCustomer\".\"CustName\" = '"+custid+"'   \r\n\t) as main;";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand(query3, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                DetailedAccountsReceivableAgeingReport ageingreport = new DetailedAccountsReceivableAgeingReport();
                ageingreport.DueDate = (DateTime)sdr0["DueDate"];
                ageingreport.TimeFrame = (string)sdr0["timeframe"];
                ageingreport.Balance = (decimal)sdr0["Balance"];
                ageingreport.Invoice = (string)sdr0["invoice"];
                detailedAccountsReceivableAgeingReports.Add(ageingreport);
            }
            cnn.Close();

            return detailedAccountsReceivableAgeingReports;

        }
        public  PLinvoiceResponse suppliersAgeAnalysis()
        {
            Dictionary<string, List<PLinvoiceAnalysis>> supplierinvoiceanalysis = new Dictionary<string, List<PLinvoiceAnalysis>>();
            PLinvoiceResponse pLinvoiceResponse = new PLinvoiceResponse();
            string query = "SELECT b.\"PLCustCode\",b.\"CustName\",SUM(A.\"Balance\"),to_char(DATE(A.\"InvDate\"), 'Mon-YY') AS MONTH,A.\"DueDate\",A.\"Balance\" " +
                "FROM \"PLInvoiceHeader\" A LEFT JOIN \"PLCustomer\" b ON b.\"CustID\" = A.\"PLCustID\" WHERE \"HasCreditNote\" = FALSE " +
                "GROUP BY b.\"PLCustCode\", A.\"Balance\" ,b.\"CustName\",MONTH,A.\"DueDate\" ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr1.Read())
            {
              //  string key = (string)sdr1["CustName"];
              string key = sdr1["CustName"] != DBNull.Value ? (string)sdr1["CustName"] : null;
                if(key != null)
                {
                    if (supplierinvoiceanalysis.ContainsKey(key))
                    {
                        PLinvoiceAnalysis pLinvoiceAnalysis = new PLinvoiceAnalysis();
                        // pLinvoiceAnalysis.balance = (float)sdr1["Balance"];
                        pLinvoiceAnalysis.date = (string)sdr1["month"];
                        //pLinvoiceAnalysis.dueDate = sdr1["CustName"];
                        var currentdata = supplierinvoiceanalysis[key];
                        currentdata.Add(pLinvoiceAnalysis);
                    }
                    else
                    {
                        List<PLinvoiceAnalysis> list = new List<PLinvoiceAnalysis>();
                        PLinvoiceAnalysis pLinvoiceAnalysis = new PLinvoiceAnalysis();
                        // pLinvoiceAnalysis.balance = (int)sdr1["Balance"];
                        pLinvoiceAnalysis.date = (string)sdr1["month"];
                        //pLinvoiceAnalysis.dueDate = sdr1["CustName"];
                        list.Add(pLinvoiceAnalysis);

                        supplierinvoiceanalysis.Add(key, list);
                    }
                }              

            }
            pLinvoiceResponse.list = supplierinvoiceanalysis;
            return pLinvoiceResponse;
        }
        public string getPlAccountGroupNominal(string code)
        {
            string query = " SELECT  * FROM  \"PLAnalysisCodes\" WHERE  \"id\" = '"+ int.Parse(code) +"' ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            PLAnalysisCodes pLAnalysisCodes = new PLAnalysisCodes();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {               
                pLAnalysisCodes.NLAccCode = (string)sdr0["NLAccCode"];
            }    
            cnn.Close();
            string query2 = "SELECT * FROM \"NLAccount\" WHERE \"NlAccCode\" = '" + pLAnalysisCodes.NLAccCode + "'  ";
            Nlaccount nlaccount = new Nlaccount();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query2, cnn).ExecuteReader();

            while (sdr1.Read())
            {
                nlaccount.NlaccName = (string)sdr1["NlAccName"];
            }
            cnn.Dispose();
            cnn.Close();
            return nlaccount.NlaccName;
        }
        public List<Vat3BReport> GettingSupplierActivity(int custId, DateTime from,  DateTime to)
        {
            //invoice query
            string InvoiceQuery = "SELECT a.\"Totals\", sum(b.\"VatAmt\") as vatAmount, a.\"TranDate\", concat('INV-', a.\"DocRef\") as inv,  concat(e.\"TranPeriod\", '/', e.\"TranYear\") as period, c.\"CustName\", c.\"PLCustCode\", c.\"PhysicalAddress\", c.\"PostalAddress\",      " +
                " c.\"VATNo\", a.\"PLJrnlNo\" , a.\"InvDate\", f.\"Dr\"::numeric, f.\"Cr\"::numeric, a.\"PLDescription\"        " +
                "FROM \"PLInvoiceHeader\" a  LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\"  LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\" LEFT JOIN financial_periods d on d.fp_ref = a.\"Period\"  LEFT JOIN \"NlJournalHeader\" e on e.\"PlJrnlNo\" = a.\"PLJrnlNo\"     " +
                "LEFT JOIN \"NLJournalDetails\" f on f.\"JrnlSlNo\" = e.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" g on g.\"NlAccCode\" = f.\"NlAccCode\"     " +
                "WHERE a.\"PLCustID\" = '"+custId+"' and a.\"DocPrefix\" = 'INVP' and a.\"InvDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and e.\"TranFrom\" = 'PL' and  g.\"NlAccName\" ~* 'CREDITORS'   " +
                "GROUP BY a.\"Totals\",  a.\"TranDate\", inv, e.\"TranPeriod\", e.\"TranYear\", c.\"CustName\",  c.\"PLCustCode\", c.\"PhysicalAddress\", c.\"PostalAddress\", c.\"VATNo\", a.\"PLJrnlNo\", a.\"InvDate\", f.\"Dr\", f.\"Cr\", a.\"PLDescription\" ;   ";
            //PAYMENT QUERY
            string PaymentQuery = "select a.\"pyPaid\"::numeric, a.\"pyDate\", a.\"pyID\", a.\"pyChequeNumber\" as inv, b.\"CustName\", b.\"VATNo\", b.\"PLCustCode\", b.\"PhysicalAddress\", b.\"PostalAddress\", concat(c.\"TranPeriod\", '/', c.\"TranYear\") as period, 0.00 as vatAmount,   " +
                " d.\"Dr\"::numeric, d.\"Cr\"::numeric, a.\"pyAdditionalDetails\"     " +
                "FROM \"PLReceipts\" a LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\" LEFT JOIN \"NlJournalHeader\" c on c.\"PlJrnlNo\" = a.\"pyID\" LEFT JOIN \"NLJournalDetails\" d on d.\"JrnlSlNo\" = c.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" e on e.\"NlAccCode\" = d.\"NlAccCode\"     " +
                "WHERE a.\"supplier_id\" = '"+custId+"' and a.\"pyDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'PL-PY' and e.\"NlAccName\" ~* 'CREDITORS'      " +
                "GROUP BY a.\"pyPaid\", a.\"pyDate\", a.\"pyID\", inv, b.\"CustName\", b.\"VATNo\", b.\"PLCustCode\", b.\"PhysicalAddress\", b.\"PostalAddress\", c.\"TranPeriod\", c.\"TranYear\", vatAmount, d.\"Dr\", d.\"Cr\", a.\"pyAdditionalDetails\";    ";
            //CREDIT NOTE QUERY
            string CreditNoteQuery = "SELECT a.\"CRNTotal\", a.\"CRNVat\" as vatAmount, a.\"TranDate\", a.\"CRNDate\", c.\"CustName\", c.\"PLCustCode\", c.\"PhysicalAddress\", c.\"PostalAddress\", c.\"VATNo\", concat('CRN', a.\"PLJrnlNo\") as inv, a.\"CRNReference\",    " +
                " concat(d.\"TranPeriod\", '/', d.\"TranYear\") as period, a.\"PLJrnlNo\", a.\"InvDate\", e.\"Dr\"::numeric, e.\"Cr\"::numeric, a.\"Additionals\"      " +
                "FROM \"PLInvoiceHeader\" a LEFT JOIN \"PLInvoiceDetail\" b on b.\"PLJrnlNo\" = a.\"PLJrnlNo\"  LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"PLCustID\"  LEFT JOIN \"NlJournalHeader\" d on d.\"PlJrnlNo\" = a.\"PLJrnlNo\" LEFT JOIN \"NLJournalDetails\" e on e.\"JrnlSlNo\" = d.\"NlJrnlNo\"    " +
                "LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = e.\"NlAccCode\"    " +
                "where a.\"PLCustID\" = '"+custId+"' and a.\"CRNDate\" BETWEEN  '"+from.ToString("yyyy-MM-dd")+"' AND '"+to.ToString("yyyy-MM-dd")+"' and d.\"TranFrom\" = 'PL-CRN' and f.\"NlAccName\" ~* 'CREDITORS'   " +
                "GROUP BY a.\"CRNTotal\", a.\"CRNVat\",  a.\"TranDate\", inv, d.\"TranPeriod\", d.\"TranYear\", a.\"CRNDate\", c.\"CustName\",  c.\"PLCustCode\", c.\"PhysicalAddress\", c.\"PostalAddress\",    c.\"VATNo\",  a.\"PLJrnlNo\", a.\"InvDate\", e.\"Dr\", e.\"Cr\", a.\"Additionals\", a.\"CRNReference\";   ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<Vat3BReport> vatAnalysis = new List<Vat3BReport>();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(InvoiceQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                Vat3BReport vat = new Vat3BReport();
                vat.TotalAmount = reader["Totals"] != DBNull.Value ? (decimal)reader["Totals"] : 0;
                vat.VatAmount = reader["vatAmount"] != DBNull.Value ? (decimal)reader["vatAmount"] : 0;
                vat.TransDate = (DateTime)reader["InvDate"];
                vat.inv = reader["inv"] != DBNull.Value ? (string)reader["inv"] : "";
                vat.period = reader["period"] != DBNull.Value ? (string)reader["period"] : "";
                vat.custname = reader["CustName"] != DBNull.Value ? (string)reader["CustName"] : "";
                vat.PhysicalAddress = reader["PhysicalAddress"] != DBNull.Value ? (string)reader["PhysicalAddress"] : "";
                vat.CustomerCode = reader["PLCustCode"] != DBNull.Value ? (string)reader["PLCustCode"] : "";
                vat.PostAddress = reader["PostalAddress"] != DBNull.Value ? (string)reader["PostalAddress"] : "";
                vat.vatCode = reader["VATNo"] != DBNull.Value ? (string)reader["VATNo"] : "";
                vat.PLJrnlNo = reader["PLJrnlNo"] != DBNull.Value ? (int)reader["PLJrnlNo"] : 0;
                vat.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                vat.Cr = reader["Cr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                vat.Description = reader["PLDescription"] != DBNull.Value ? (string)reader["PLDescription"] : "";
                vat.DocType = "INV";
                vatAnalysis.Add(vat);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader2 = new NpgsqlCommand(PaymentQuery, cnn).ExecuteReader();
            List<Vat3BReport> paymentReport = new List<Vat3BReport>();
            while (reader2.Read())
            {
                Vat3BReport vatReport = new Vat3BReport();
                vatReport.TotalAmount = reader2["pyPaid"] != DBNull.Value ? (decimal)reader2["pyPaid"] : 0;
                vatReport.VatAmount = reader2["vatAmount"] != DBNull.Value ? (decimal)reader2["vatAmount"] : 0;
                vatReport.TransDate = (DateTime)reader2["pyDate"];
                vatReport.inv = reader2["inv"] != DBNull.Value ? (string)reader2["inv"] : "";
                vatReport.period = reader2["period"] != DBNull.Value ? (string)reader2["period"] : "";
                vatReport.custname = reader2["CustName"] != DBNull.Value ? (string)reader2["CustName"] : "";
                vatReport.PhysicalAddress = reader2["PhysicalAddress"] != DBNull.Value ? (string)reader2["PhysicalAddress"] : "";
                vatReport.CustomerCode = reader2["PLCustCode"] != DBNull.Value ? (string)reader2["PLCustCode"] : "";
                vatReport.PostAddress = reader2["PostalAddress"] != DBNull.Value ? (string)reader2["PostalAddress"] : "";
                vatReport.vatCode = reader2["VATNo"] != DBNull.Value ? (string)reader2["VATNo"] : "";
                vatReport.PLJrnlNo = reader2["pyID"] != DBNull.Value ? (int)reader2["pyID"] : 0;
                vatReport.Dr = reader2["Dr"] != DBNull.Value ? (decimal)reader2["Dr"] : 0;
                vatReport.Cr = reader2["Cr"] != DBNull.Value ? (decimal)reader2["Cr"] : 0;
                vatReport.Description = reader2["pyAdditionalDetails"] != DBNull.Value ? (string)reader2["pyAdditionalDetails"] : "";
                vatReport.DocType = "PYT";
                paymentReport.Add(vatReport);
            }
            cnn.Close();

            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand(CreditNoteQuery, cnn).ExecuteReader();
            List<Vat3BReport> vatReports = new List<Vat3BReport>();
            while (reader1.Read())
            {
                Vat3BReport CreditVat = new Vat3BReport();
                CreditVat.TotalAmount = reader1["CRNTotal"] != DBNull.Value ? (decimal)reader1["CRNTotal"] : 0;
                CreditVat.VatAmount = reader1["vatAmount"] != DBNull.Value ? (decimal)reader1["vatAmount"] : 0;
                CreditVat.TransDate = (DateTime)reader1["CRNDate"];
                CreditVat.inv = reader1["CRNReference"] != DBNull.Value ? (string)reader1["CRNReference"] : (string)reader1["inv"];
                CreditVat.period = reader1["period"] != DBNull.Value ? (string)reader1["period"] : "";
                CreditVat.custname = reader1["CustName"] != DBNull.Value ? (string)reader1["CustName"] : "";
                CreditVat.PhysicalAddress = reader1["PhysicalAddress"] != DBNull.Value ? (string)reader1["PhysicalAddress"] : "";
                CreditVat.CustomerCode = reader1["PLCustCode"] != DBNull.Value ? (string)reader1["PLCustCode"] : "";
                CreditVat.PostAddress = reader1["PostalAddress"] != DBNull.Value ? (string)reader1["PostalAddress"] : "";
                CreditVat.vatCode = reader1["VATNo"] != DBNull.Value ? (string)reader1["VATNo"] : "";
                CreditVat.PLJrnlNo = reader1["PLJrnlNo"] != DBNull.Value ? (int)reader1["PLJrnlNo"] : 0;
                CreditVat.Dr = reader1["Dr"] != DBNull.Value ? (decimal)reader1["Dr"] : 0;
                CreditVat.Cr = reader1["Cr"] != DBNull.Value ? (decimal)reader1["Cr"] : 0;
                CreditVat.Description = reader1["Additionals"] != DBNull.Value ? (string)reader1["Additionals"] : "";
                CreditVat.DocType = "CRN";
                vatReports.Add(CreditVat);
            }
            cnn.Close();

            vatAnalysis.Sort((x,y) => y.TransDate.CompareTo(x.TransDate));
            vatAnalysis.AddRange(paymentReport);
            vatAnalysis.AddRange(vatReports);
            return vatAnalysis;
        }
        public PLCustomer GettingOpeningBalance(int custId)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            PLCustomer customer = new PLCustomer();
            string balancequery = "select a.\"OpeningBalance\", a.\"OpeningBalanceDate\"  from \"PLCustomer\" a where a.\"CustID\" = '"+custId+"'; ";
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(balancequery, cnn).ExecuteReader();
            while (reader.Read())
            {
                customer.OpeningBalanceDate = (DateTime)reader["OpeningBalanceDate"];
                customer.OpeningBalance = reader["OpeningBalance"] != DBNull.Value ? (decimal)reader["OpeningBalance"] : 0;
            }
            cnn.Close();
            return customer;
        }
        public License GettingCompanyDetails()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            License lic = new License();
            cnn.Open();
            string query = " Select * From \"Licence\" ";
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
        public MyResponse CreateGRNType(GoodReturnNoteType goodReturn)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            int gnt_last = 0;
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand("SELECT COALESCE(max(\"GRNId\"), 0) as st FROM \"GoodReturnNoteType\"; ", cnn).ExecuteReader();
            while (reader.Read())
            {
                gnt_last = (int)reader["st"];
            }
            cnn.Close();
            string check_gnt_exit = "SELECT * FROM \"GoodReturnNoteType\" WHERE \"GRNType\" = '"+ goodReturn.GRNType +"' ";
            int check_result = myDbconnection.CheckRowExists(check_gnt_exit, OrganizationId);
            if(check_result > 0)
            {
                response.Httpcode = 400;
                response.Message = "Sorry! The Good Return Note type you are trying to register already exists.";
                return response;
            }
            gnt_last = gnt_last + 1;
            string insert_gnt = "INSERT INTO \"GoodReturnNoteType\" (\"GRNId\", \"GRNType\", \"GRNComment\") VALUES ('"+ gnt_last +"', '"+ goodReturn.GRNType +"', '"+ goodReturn.GRNComment +"') ";
            bool insert_result = myDbconnection.UpdateDelInsert(insert_gnt, OrganizationId);
            if(insert_result == false)
            {
                response.Httpcode = 400;
                response.Message = "Sorry! An error occurred while trying to create a new Good Return Note Entity.";
            }
            return response;
        }
        public MyResponse UpdateHeaderSettings(PurchaseHeaderSettings header)
        {
            MyResponse response = new MyResponse();
            string check_header = "SELECT * FROM \"Document_header\" WHERE \"id\" = '"+ header.Id +"' ";
            int header_result = myDbconnection.CheckRowExists(check_header, OrganizationId);
            if(header_result == 0)
            {
                response.Httpcode = 400;
                response.Message = "Error! The Purchase Document Header reference parsed was NOT found.";
                return response;
            }            
            string update_header_settings = "UPDATE \"Document_header\" SET \"Status\" = '"+header.Status+"' WHERE \"id\" = '"+ header.Id +"'; ";
            bool result = myDbconnection.UpdateDelInsert(update_header_settings, OrganizationId);
            if (result == false)
            {
                response.Httpcode = 400;
                response.Message = "No changes were made in your update request.";
            }
            return response;
        }
        public MyResponse UpdateGRNType(GoodReturnNoteType goodReturnNote)
        {
            MyResponse response = new MyResponse();
            string grn_check = "SELECT * FROM \"GoodReturnNoteType\" WHERE \"GRNId\" = '"+ goodReturnNote.GRNId +"' ";
            int grn_result = myDbconnection.CheckRowExists(grn_check, OrganizationId);
            if(grn_result == 0)
            {
                response.Httpcode = 400;
                response.Message = "Error! The Good Return Note type reference parsed was NOT found.";
                return response;
            }
            string update_grn = "UPDATE \"GoodReturnNoteType\" SET \"GRNType\" = '"+ goodReturnNote.GRNType +"', \"GRNComment\" = '"+ goodReturnNote.GRNComment +"' WHERE \"GRNId\" = '"+ goodReturnNote.GRNId +"' ";
            bool update_grn_result = myDbconnection.UpdateDelInsert(update_grn, OrganizationId);
            if(update_grn_result == false)
            {
                response.Httpcode = 400;
                response.Message = "No changes were made in your update request.";
            }
            return response;
        }
        public MyResponse DeleteGRNType(int key)
        {
            MyResponse response = new MyResponse();
            string grn_check = "SELECT * FROM \"GoodReturnNoteType\" WHERE \"GRNId\" = '"+ key +"' ";
            int grn_check_result = myDbconnection.CheckRowExists(grn_check, OrganizationId);
            if (grn_check_result == 0)
            {
                response.Httpcode = 400;
                response.Message = "Error! The Good Return Note type reference parsed was NOT found.";
                return response;
            }
            string delete_grn = "DELETE FROM \"GoodReturnNoteType\" WHERE \"GRNId\" = '"+ key +"' ";
            bool delete_grn_check = myDbconnection.UpdateDelInsert(delete_grn, OrganizationId);
            if(delete_grn_check == false)
            {
                response.Httpcode = 400;
                response.Message = "An error occurred while trying to remove this Good Return Note from database.";
            }
            return response;
        }
    }
}
