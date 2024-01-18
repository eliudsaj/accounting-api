using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using pyme_finance_api.Common;
using pyme_finance_api.Controllers.NlController;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.CustomerSalesLedger;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.NL.BalanceSheet;
using pyme_finance_api.Models.NL.NLAccount;
using pyme_finance_api.Models.NL.NlAccountGroup;
using pyme_finance_api.Models.NL.NLjournal;
using pyme_finance_api.Models.NL.TrialBalance;
using pyme_finance_api.Models.Purchases.Invoices;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Service.PlService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.NlServices
{

    public interface InService
    {
        public List<Nlaccount> GetNlaccounts();
        public List<TrialBalanceReport> GetTrialBalanceReports();
        public BalanceSheet GetBalanceSheetReport();
        public List<NlaccountGroup> GetNlaccountGroupByGroupLevel(int level);
        public List<NLJournalDetails> GetNLJournalDetails(int journalId);
        public Nlaccount getNlAccountsByCode(string code);
        public NlaccountGroup getNlAccountGroupByCode(string code);
        public MyResponse createNlaccount(Nlaccount recvData);
        public Nlaccount GetNLAccountAccountCodeUsingName(string accountName);
        public MyResponse updateAccountsOnCreditNoteCreation(Invoice inv, NlJournalHeader recvData, int lastinsertedcrnid, string reason, DateTime crnDate, decimal crnVat, int periodMonth, int periodYear, string vatPercent);
        public MyResponse addSlanalysis(SLAnalysisCodes sLAnalysisCodes);
        public SLAnalysisCodes findSlAnalysisCodeById(int Id);
        public MyResponse updateSlanalysis(SLAnalysisCodes sLAnalysisCodes);
        public MyResponse saveSalesReceiptsAccount(NlJournalHeader recvData, string transtype, float pyPaid, AddCustomer addCustomer);
        public MyResponse createNlJournalHeader(NlJournalHeader recvData, string transtype, List<Inventorylist> invoiceListDetailsData, decimal exchangerate);
        public MyResponse createNlJournalHeaderpl(NlJournalHeader recvData, string transtype, List<InvoiceListDetailsData> InvoiceDetailsList);
        public MyResponse updateNlaccount(Nlaccount recvData);
        public MyResponse updateNlJournalDetail(NLJournalDetails recvData);
        public List<NlaccountGroup> GetNlaccountGroups();
        public List<SLAnalysisCodes> GetSlanalysisCodes();
        public AddCustomer getCustomerById(int id);
        public List<CustomerSalesLedger> getCustomerLedgerCard(string Ref, DateTime From, DateTime To, int cust_id);
        List<SingleNlreport> NLReportDetails(string code);
        List<SingleNlreport> AccountGroupReportDetailsByDate(string code, DateTime from, DateTime to);
        List<SingleNlreport> AccountGroupReportDetailsByPeriod(string code, string periodfrom, string periodto);
        List<BankReconcilationReport> BankReconciliationReports(string code);
        List<BankHistoryRecord> GetbankHistory(string code, DateTime from, DateTime to);
        List<BankReconcilationReport> GettingBankDetailsPerDate(string code, DateTime from, DateTime to);
        List<BankReconcilationReport> GettingBankDetailsPerPeriod(string code, string periodFrom, string periodTo);
        List<BankHistoryRecord> GetBankHistoryByPeriod(string code, string periodFrom, string periodTo);
        List<CashBookHeader> GetBankBalance(string code);
        List<BankReconcilationReport> GettingClearedReconciliation(string code);
        List<BankReconcilationReport> GettingUnclearedReconciliationRecord(string code);
        List<BankReconcilationReport> GetCashBookBalancePerDate(string code, DateTime statementDate);
        List<NLAccount> GettingAllBankList();
        License GetCompanyDetail();
        List<NlJournalHeader> GettingNlJournal();
        List<NLJournalDetails> GetNLJournalByJournalNo(int journalNo);
        CustomerSalesLedger GetInvoiceBalanceBroughtForward(int custId, DateTime from);
        CustomerSalesLedger GetPaymentBalance(int custId, DateTime from);
        CustomerSalesLedger GetCRNBalance(int custId, DateTime from);
        CustomerSalesLedger GetReversalBalance(int custId, DateTime from);
        MyResponse BankStatementAdding(List<CashBookDetails> cashBook, int userId, DateTime createdon, decimal bankBalance, decimal unclearedbalance, decimal abbalance, decimal cashbookbalance, decimal cdbalance, string periodFrom, string periodTo, DateTime dateFrom, DateTime dateTo);
    }
    public class NlService : InService
    {
        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }
        public NlService(string organizationId)
        {
            OrganizationId = organizationId;
        }
        public List<NlaccountGroup> GetNlaccountGroups()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //get nlaccountsGroups
            List<NlaccountGroup> nlaccountGroups = new List<NlaccountGroup>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLAccountGroup\"", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                NlaccountGroup nlaccountGroup = new NlaccountGroup();
                nlaccountGroup.ModifiedOn = sdr0["ModifiedOn"] != DBNull.Value ? (DateTime)sdr0["ModifiedOn"] : DateTime.Today;
                nlaccountGroup.GroupName = sdr0["GroupName"] != DBNull.Value ? (string)sdr0["GroupName"] : null;
                nlaccountGroup.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccountGroup.PriGroupName = sdr0["PriGroupCode"] != DBNull.Value ? (string)sdr0["PriGroupCode"] : null;
                nlaccountGroup.GroupType = sdr0["GroupType"] != DBNull.Value ? (string)sdr0["GroupType"] : null;
                nlaccountGroup.GroupSubType = sdr0["GroupSubType"] != DBNull.Value ? (string)sdr0["GroupSubType"] : null;
                nlaccountGroup.GroupLevel = sdr0["GroupLevel"] != DBNull.Value ? (int?)sdr0["GroupLevel"] : null;
                nlaccountGroup.UserId = sdr0["UserId"] != DBNull.Value ? (int?)sdr0["UserId"] : null;
                nlaccountGroup.UserName = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                nlaccountGroup.SwverNo = sdr0["SwVerNo"] != DBNull.Value ? (string)sdr0["SwVerNo"] : null;
                nlaccountGroup.DefaultGroup = sdr0["DefaultGroup"] != DBNull.Value ? (int?)sdr0["DefaultGroup"] : null;
                nlaccountGroups.Add(nlaccountGroup);
            }
            cnn.Close();
            return nlaccountGroups;
        }
        public List<Nlaccount> GetNlaccounts()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //geT nlaccounts
            List<Nlaccount> nlaccounts = new List<Nlaccount>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLAccount\"", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                Nlaccount nlaccount = new Nlaccount();
                nlaccount.LastStatDate = sdr0["LastStatDate"] != DBNull.Value ? (DateTime)sdr0["LastStatDate"] : DateTime.Today;
                nlaccount.StatBalance = sdr0["StatBalance"] != DBNull.Value ? (decimal)sdr0["StatBalance"] : 0;
                nlaccount.GroupCode = sdr0["AGroupCode"] != DBNull.Value ? (string)sdr0["AGroupCode"] : null;
                nlaccount.IsMirrorAcc = (bool)sdr0["isMirrorAcc"];
                nlaccount.CurCode = sdr0["CurCode"] != DBNull.Value ? (string)sdr0["CurCode"] : null;
                nlaccount.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccount.NlaccName = sdr0["NLAccName"] != DBNull.Value ? (string)sdr0["NLAccName"] : null;
                nlaccount.NlaccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
                nlaccounts.Add(nlaccount);
            }
            cnn.Close();
            return nlaccounts;
        }
        public Nlaccount getNlAccountsByCode(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLAccount\" where  \"NlAccCode\"= '" + code + "'", cnn).ExecuteReader();
            Nlaccount nlaccount = new Nlaccount();
            while (sdr0.Read())
            {
                nlaccount.LastStatDate = sdr0["LastStatDate"] != DBNull.Value ? (DateTime)sdr0["LastStatDate"] : DateTime.Today;
                nlaccount.StatBalance = sdr0["StatBalance"] != DBNull.Value ? (decimal)sdr0["StatBalance"] : 0;
                nlaccount.GroupCode = sdr0["AGroupCode"] != DBNull.Value ? (string)sdr0["AGroupCode"] : null;
                nlaccount.IsMirrorAcc = (bool)sdr0["isMirrorAcc"];
                nlaccount.CurCode = sdr0["CurCode"] != DBNull.Value ? (string)sdr0["CurCode"] : null;
                nlaccount.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccount.NlaccName = sdr0["NLAccName"] != DBNull.Value ? (string)sdr0["NLAccName"] : null;
                nlaccount.NlaccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
            }
            cnn.Close();
            return nlaccount;
        }
        public MyResponse createNlaccount(Nlaccount recvData)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            int groupnamecount = 0;
            ////check if group name exists
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"NLAccount\" where  \"NlAccName\"= '" + recvData.NlaccName + "'", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                groupnamecount++;
            }
            if (groupnamecount >= 1)
            {
                response.Httpcode = 400;
                response.Message = "This  Account Name already exists";
                cnn.Close();
                return response;
            }
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            int count = 0;
            cnn1.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT MAX(\"NlAccCode\") as sj FROM \"NLAccount\"", cnn1).ExecuteReader();
            /// 
            while (sdr0.Read())
            {
                //  string max = (string)sdr0["sj"];
                string max = sdr0["sj"] != DBNull.Value ? (string)sdr0["sj"] : "000";
                string removedpadding = max.PadLeft(4);
                count = Int32.Parse(removedpadding) + 1;
                //  count = max + 1;
            }
            string stringcount = count.ToString();
            string code = stringcount.PadLeft(4, '0');

            string insertQuery1 = "INSERT INTO \"NLAccount\" (\"NlAccCode\",\"NlAccName\",\"GroupCode\",\"CurCode\",\"IsMirrorAcc\",\"MAccCode\",\"AGroupCode\",\"StatBalance\",\"LastStatDate\") " +
             "VALUES('" + code + "','" + recvData.NlaccName + "','" + recvData.GroupCode + "', '" + recvData.CurCode + "', '" + recvData.IsMirrorAcc + "', '" + recvData.MaccCode.Trim() + "','" + recvData.AgroupCode.Trim() + "','" + recvData.StatBalance + "','" + recvData.LastStatDate + "' );";

            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, OrganizationId);

            cnn1.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            return response;
        }
        public MyResponse updateNlaccount(Nlaccount recvData)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn1.Open();
            string updtQ = "UPDATE \"NLAccount\" SET \"GroupCode\" = '" + recvData.GroupCode + "',\"NlAccName\"='" + recvData.NlaccName + "',\"CurCode\"='" + recvData.CurCode + "',\"StatBalance\" = '"
                + recvData.StatBalance + "',\"IsMirrorAcc\"='" + recvData.IsMirrorAcc + "',\"MAccCode\"='" + recvData.MaccCode + "',\"AGroupCode\"='" + recvData.AgroupCode + "', \"LastStatDate\" = '" + recvData.LastStatDate + "' WHERE \"NlAccCode\"= '" + recvData.NlaccCode + "' ";
            bool myReq2 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);
            cnn1.Close();

            if (myReq2 == false)
            {
                //failed
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            response.Httpcode = 200;
            response.Message = "An occured while trying to save details.";

            return response;

        }
        public NlaccountGroup getNlAccountGroupByCode(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLAccountGroup\" where  \"GroupCode\"= '" + code + "'", cnn).ExecuteReader();
            NlaccountGroup nlaccountGroup = new NlaccountGroup();
            while (sdr0.Read())
            {
                nlaccountGroup.ModifiedOn = sdr0["ModifiedOn"] != DBNull.Value ? (DateTime)sdr0["ModifiedOn"] : DateTime.Today;
                nlaccountGroup.GroupName = sdr0["GroupName"] != DBNull.Value ? (string)sdr0["GroupName"] : null;
                nlaccountGroup.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccountGroup.PriGroupName = sdr0["PriGroupCode"] != DBNull.Value ? (string)sdr0["PriGroupCode"] : null;
                nlaccountGroup.GroupType = sdr0["GroupType"] != DBNull.Value ? (string)sdr0["GroupType"] : null;
                nlaccountGroup.GroupSubType = sdr0["GroupSubType"] != DBNull.Value ? (string)sdr0["GroupSubType"] : null;
                nlaccountGroup.GroupLevel = sdr0["GroupLevel"] != DBNull.Value ? (int?)sdr0["GroupLevel"] : null;
                nlaccountGroup.UserId = sdr0["UserId"] != DBNull.Value ? (int?)sdr0["UserId"] : null;
                nlaccountGroup.UserName = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                nlaccountGroup.SwverNo = sdr0["SwVerNo"] != DBNull.Value ? (string)sdr0["SwVerNo"] : null;
                nlaccountGroup.DefaultGroup = sdr0["DefaultGroup"] != DBNull.Value ? (int?)sdr0["DefaultGroup"] : null;
            }
            cnn.Close();
            return nlaccountGroup;
        }
        /// <summary>
        /// save customer brought forward
        /// </summary>
        /// <param name="recvData">Journal Header details</param>
        /// <param name="transtype">This dictates on how we handle the entry if its purchase,sales etc</param>
        /// <returns></returns>
        public MyResponse createCustomerBroughtForward(AccountsReceivableBroughtForward recvData)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id = recvData.AccountId;
            string REF = recvData.JournalRef;
            string Account = recvData.AccountName;
            string Description = $"{Account} ";

            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    string LpoUpdate = "UPDATE \"SLCustomer\" SET \"OpeningBalance\" = " + recvData.Amount + " , \"HasOpeningBalance\" = " + true + " ,\"OpeningBalanceDate\" = '" + recvData.TransactionDate + "'  WHERE \"SLCustomerSerial\" = " + id + " ;";

                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"UserJournalRef\") " +
                    "VALUES('" + Description + "','" + recvData.TransactionDate + "','" + DateTime.Now + "', '" + DateTime.Now.Month + "', '" + DateTime.Now.Year + "','SL-BF','NT','" + 0 + "'," + 0 + "," + 0 + ",'" + REF + "' ) RETURNING \"NlJrnlNo\" ;";

                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    decimal vatAmount = recvData.Amount * (recvData.Vat / 100);
                    decimal salesAmount = recvData.Amount - vatAmount;
                    //total salesAmount+vatAmount
                    decimal debtors = recvData.Amount;
                    /// we do sales in the journal details for now
                    ///Debit entries (debtors)
                    debtors = vatAmount + salesAmount;
                    ///Credit entries(vat,sales)
                    string creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("VAT").NlaccCode + "','', '" + vatAmount + "', '" + (vatAmount) + "',' VAT on B/F narration','','" + false + "','','" + false + "','" + false + "','" + vatAmount + "');";

                    string creditentry2 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("SALES").NlaccCode + "','', '" + salesAmount + "', '" + (salesAmount) + "', ' BALANCE B/F WITH VAT EXCLUDED','','" + false + "','','" + false + "','" + false + "','" + 0 + "');";

                    string debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','" + debtors + "', '', '" + (debtors) + "', 'BALANCE B/F WITH VAT INCLUDED','','" + false + "','','" + false + "','" + false + "','" + 0 + "');";

                    cmd.CommandText = creditentry1 + creditentry2 + debitentry1 + LpoUpdate;
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    Console.WriteLine(e.Message);
                    response.Message = "An error occoured , please try again later";
                }
            }
            cnn.Close();
            return response;

        }
        /// <summary>
        /// save customer brought forward
        /// </summary>
        /// <param name="recvData">Journal Header details</param>
        /// <param name="transtype">This dictates on how we handle the entry if its purchase,sales etc</param>
        /// <returns></returns>
        public MyResponse createSupplierBroughtForward(AccountsReceivableBroughtForward recvData)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id = recvData.AccountId;
            string REF = recvData.JournalRef;
            string Account = recvData.AccountName;
            string Description = $"{Account} ";

            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    string LpoUpdate = "UPDATE \"PLCustomer\" SET \"OpeningBalance\" = " + recvData.Amount + " , \"HasOpeningBalance\" = " + true + " ,\"OpeningBalanceDate\"  = '" + recvData.TransactionDate + "'  WHERE \"CustID\" = " + id + " ;";

                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"UserJournalRef\",\"CreatedOn\") " +
                        "VALUES('" + Description + "','" + recvData.TransactionDate + "','" + DateTime.Now + "', '" + DateTime.Now.Month + "', '" + DateTime.Now.Year + "','PL-BF','NT','" + 0 + "'," + 0 + ",'" + 0 + "','" + REF + "','" + DateTime.Now + "' ) RETURNING \"NlJrnlNo\" ; ";

                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);

                    id = int.Parse(cmd.ExecuteScalar().ToString());

                    decimal vatAmount = recvData.Amount * (recvData.Vat / 100);
                    decimal purchaseAmount = recvData.Amount - vatAmount;
                    //total salesAmount+vatAmount
                    decimal creditors = recvData.Amount;

                    /// we do sales in the journal details for now
                    ///Debit entries (debtors)

                    creditors = vatAmount + purchaseAmount;

                    string debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\", \"NlAccCode\", \"Dr\", \"Cr\", \"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("VAT").NlaccCode + "','" + vatAmount + "', '', '" + (vatAmount) + "', 'Balance B/F TAX ','narration','false','','false','false','" + vatAmount + "');";

                    string debitentry2 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("PURCHASES").NlaccCode + "','" + purchaseAmount + "', '', '" + (purchaseAmount) + "', 'Balance B/F with tax excluded ','','false','','false','false','" + 0 + "'); ";


                    string creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\", \"NlAccCode\", \"Dr\", \"Cr\", \"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','', '" + creditors + "', '" + (creditors) + "', 'Balance B/F with tax included','','false','','false','false','" + 0 + "'); ";

                    cmd.CommandText = creditentry1 + debitentry2 + debitentry1 + LpoUpdate;
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;

                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    Console.WriteLine(e.Message);
                    response.Message = e.Message;
                }
            }
            cnn.Close();
            return response;

        }
        /// <summary>
        /// we create a journal here 
        /// </summary>
        /// <param name="recvData">Journal Header details</param>
        /// <param name="transtype">This dictates on how we handle the entry if its purchase,sales etc</param>
        /// <returns></returns>
        public MyResponse createNlJournalHeader(NlJournalHeader recvData, string transtype, List<Inventorylist> invoiceListDetailsData, decimal exchangerate)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id;
            using (var trans = cnn.BeginTransaction())
            {

                try
                {
                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"CreatedOn\") " +
                    "VALUES('" + recvData.NlJrnlDesc + "','" + recvData.TranDate + "','" + recvData.MEndDate + "', '" + recvData.TranPeriod + "', '" + recvData.TranYear + "','SL','NT','" + recvData.SlJrnlNo + "'," + 0 + ",'" + 0 + "','" + DateTime.Now + "' ) RETURNING \"NlJrnlNo\" ;  ";

                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);

                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    decimal vatAmount = 0;
                    decimal salesAmount = 0;
                    //total salesAmount+vatAmount
                    decimal debtors = 0;
                    decimal discount_amount = 0;
                    StringBuilder creditentry2 = new StringBuilder();
                    StringBuilder debitdiscount = new StringBuilder();

                    /// we do sales in the journal details for now
                    ///Debit entries (debtors)
                    foreach (var draccount in invoiceListDetailsData)
                    {
                        vatAmount = vatAmount + draccount.VatAmt;
                        salesAmount = salesAmount + (draccount.Price * draccount.Quantity);
                        discount_amount = discount_amount + draccount.DiscountAmt;

                        creditentry2.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\", \"NlAccCode\", \"Dr\", \"Cr\", \"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                                "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName(draccount.SlProductGroup).NlaccCode + "','', '" + (draccount.Price * draccount.Quantity * exchangerate) + "', '" + (draccount.Price * draccount.Quantity * exchangerate) + "', 'total sales','','false','','false','false','" + 0 + "');");

                        if (discount_amount > 0)
                        {
                            debitdiscount.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                                "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName(draccount.SlProductGroup).NlaccCode + "','" + (draccount.DiscountAmt * exchangerate) + "', '', '" + (draccount.DiscountAmt * exchangerate) + "', 'discount allowed','','false','','false','false','" + 0 + "');");
                        }
                    }
                    debtors = vatAmount + salesAmount;
                    ///Credit entries(vat,sales)
                    string creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\", \"NlAccCode\", \"Dr\", \"Cr\", \"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("VAT").NlaccCode + "','', '" + (vatAmount * exchangerate) + "', '" + (vatAmount * exchangerate) + "', ' VAT on sales','narration','false','','false','false','" + (vatAmount * exchangerate) + "');";

                    string creditdiscount = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','', '" + (discount_amount * exchangerate) + "', '" + (discount_amount * exchangerate) + "', 'total discount allowed','','false','','false','false','" + 0 + "');  ";

                    string debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','" + (debtors * exchangerate) + "', '', '" + (debtors * exchangerate) + "', 'amount owed','','false','','false','false','" + 0 + "');   ";

                    if (discount_amount > 0)
                    {
                        cmd.CommandText = creditentry1 + creditentry2.ToString() + debitentry1 + creditdiscount + debitdiscount.ToString();
                    }
                    else
                    {
                        cmd.CommandText = creditentry1 + creditentry2.ToString() + debitentry1;
                    }
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";
                }
            }
            cnn.Close();
            return response;

        }
        public List<NlaccountGroup> GetNlaccountGroupByGroupLevel(int level)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            //get nlaccountsGroups
            List<NlaccountGroup> nlaccountGroups = new List<NlaccountGroup>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLAccountGroup\" where \"GroupLevel\" = '" + level + "'", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                NlaccountGroup nlaccountGroup = new NlaccountGroup();
                nlaccountGroup.ModifiedOn = sdr0["ModifiedOn"] != DBNull.Value ? (DateTime)sdr0["ModifiedOn"] : DateTime.Today;
                nlaccountGroup.GroupName = sdr0["GroupName"] != DBNull.Value ? (string)sdr0["GroupName"] : null;
                nlaccountGroup.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccountGroup.PriGroupName = sdr0["PriGroupCode"] != DBNull.Value ? (string)sdr0["PriGroupCode"] : null;
                nlaccountGroup.GroupType = sdr0["GroupType"] != DBNull.Value ? (string)sdr0["GroupType"] : null;
                nlaccountGroup.GroupSubType = sdr0["GroupSubType"] != DBNull.Value ? (string)sdr0["GroupSubType"] : null;
                nlaccountGroup.GroupLevel = sdr0["GroupLevel"] != DBNull.Value ? (int?)sdr0["GroupLevel"] : null;
                nlaccountGroup.UserId = sdr0["UserId"] != DBNull.Value ? (int?)sdr0["UserId"] : null;
                nlaccountGroup.UserName = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                nlaccountGroup.SwverNo = sdr0["SwVerNo"] != DBNull.Value ? (string)sdr0["SwVerNo"] : null;
                nlaccountGroup.DefaultGroup = sdr0["DefaultGroup"] != DBNull.Value ? (int?)sdr0["DefaultGroup"] : null;
                nlaccountGroups.Add(nlaccountGroup);
            }
            cnn.Close();

            return nlaccountGroups;
        }
        public List<SingleNlreport> NLReportDetails(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT  c.\"NlAccName\",b.\"JrnlSlNo\",\"NlJrnlDesc\",\"TranDate\",\"NlJrnlDesc\",\"TranFrom\",b.\"Dr\",b.\"Cr\",a.\"SlJrnlNo\",a.\"PlJrnlNo\"   " +
                "FROM  \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" WHERE b.\"NlAccCode\"='" + code + "';  ";
            List<SingleNlreport> singleNlreports = new List<SingleNlreport>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                SingleNlreport singleNlreport = new SingleNlreport();
                singleNlreport.Dr = (decimal)sdr0["Dr"];
                singleNlreport.Cr = (decimal)sdr0["Cr"];
                singleNlreport.Journal = (int)sdr0["JrnlSlNo"];
                singleNlreport.Description = (string)sdr0["NlJrnlDesc"];
                singleNlreport.AccName = (string)sdr0["NlAccName"];
                singleNlreport.TranDate = (DateTime)sdr0["TranDate"];
                singleNlreports.Add(singleNlreport);
            }
            cnn.Close();
            return singleNlreports;
        }
        public List<SingleNlreport> AccountGroupReportDetailsByDate(string code, DateTime from, DateTime to)
        {
            NpgsqlConnection conn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            conn.Open();
            //string query = "SELECT  c.\"NlAccCode\", c.\"NlAccName\", b.\"JrnlSlNo\", a.\"TranDate\", a.\"TranFrom\", b.\"Dr\", b.\"Cr\", concat(a.\"TranPeriod\",'/',a.\"TranYear\") as period, " +
            //    "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN d.\"currentCustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" WHEN a.\"TranFrom\" = 'PL-PY' then f.\"CustName\" END AS accDescription, " +
            //    "CASE when a.\"TranFrom\" = 'SL-PY' THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'PL-PY' THEN e.\"pyChequeNumber\"WHEN a.\"TranFrom\" = 'J-E' then b.\"FolioNo\" END as reference " +
            //    "FROM  \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"SLReceipts\" d on d.\"pyID\" = a.\"SlJrnlNo\" " +
            //    "LEFT JOIN \"PLReceipts\" e on e.\"pyID\" = a.\"PlJrnlNo\" LEFT JOIN \"PLCustomer\" f on f.\"CustID\" = e.supplier_id WHERE b.\"NlAccCode\"='"+code+"' and a.\"TranDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'   " +
            //    "group by c.\"NlAccCode\", c.\"NlAccName\", b.\"JrnlSlNo\", a.\"TranDate\", a.\"TranFrom\", b.\"Dr\", b.\"Cr\", a.\"TranPeriod\",a.\"TranYear\", d.\"currentCustName\", b.\"Narration\", f.\"CustName\", d.\"pyChequeNumber\", b.\"FolioNo\", e.\"pyChequeNumber\" ";
            //UPDATED QUERY FOR ACCOUNT DETAIL BY DATE
            string query = "SELECT  c.\"NlAccCode\", c.\"NlAccName\", b.\"JrnlSlNo\", a.\"TranDate\", a.\"TranFrom\", b.\"Dr\"::numeric, b.\"Cr\"::numeric, concat(a.\"TranPeriod\",'/',a.\"TranYear\") as period,      " +
                "CASE WHEN a.\"TranFrom\" in ('SL-PY','SL-PY-REVERSAL') THEN d.\"currentCustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" WHEN a.\"TranFrom\" in ('PL-PY','PL-PY-REVERSAL')  then f.\"CustName\"            " +
                " when a.\"TranFrom\" in ('SL', 'SL-CRN', 'SL-REVERSAL') then i.\"CustCompany\" when a.\"TranFrom\" in ('PL', 'PL-CRN', 'PL-REVERSAL') then j.\"CustName\" END AS accDescription,         " +
                "CASE when a.\"TranFrom\" in ('SL-PY','SL-PY-REVERSAL') THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" in ('PL-PY', 'PL-PY-REVERSAL') THEN e.\"pyChequeNumber\"WHEN a.\"TranFrom\" = 'J-E' then b.\"FolioNo\"             " +
                "when A.\"TranFrom\" IN ('SL', 'SL-CRN', 'SL-REVERSAL') THEN concat('',g.\"SLJrnlNo\") WHEN A.\"TranFrom\" IN ('PL', 'PL-CRN', 'PL-REVERSAL') THEN concat('',h.\"PLJrnlNo\") END as reference       " +
                "FROM  \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"SLReceipts\" d on d.journal_id = a.\"NlJrnlNo\"       " +
                "LEFT JOIN \"PLReceipts\" e on e.\"pyID\" = a.\"NlJrnlNo\" LEFT JOIN \"PLCustomer\" f on f.\"CustID\" = e.supplier_id left join \"SLInvoiceHeader\" g on g.\"SLJrnlNo\" = a.\"SlJrnlNo\"  left join \"PLInvoiceHeader\" h on h.\"PLJrnlNo\" = a.\"PlJrnlNo\"      " +
                "left join \"SLCustomer\" i on i.\"SLCustomerSerial\" = g.\"CustId\"  left join \"PLCustomer\" j on j.\"CustID\" = h.\"PLCustID\" WHERE b.\"NlAccCode\"='"+code+"' and a.\"TranDate\" BETWEEN '"+from.ToString("yyyy-MM-dd")+"' and '"+to.ToString("yyyy-MM-dd")+"'   ";

            List<SingleNlreport> singleNlreports = new List<SingleNlreport>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, conn).ExecuteReader();
            while (sdr0.Read())
            {
                SingleNlreport singleNlreport = new SingleNlreport();
                singleNlreport.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                singleNlreport.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                singleNlreport.Journal = sdr0["JrnlSlNo"] != DBNull.Value ? (int)sdr0["JrnlSlNo"] : 0;
                singleNlreport.Description = sdr0["accDescription"] != DBNull.Value ? (string)sdr0["accDescription"] : "";
                singleNlreport.AccName = sdr0["NlAccName"] != DBNull.Value ? (string)sdr0["NlAccName"] : "";
                singleNlreport.Period = sdr0["period"] != DBNull.Value ? (string)sdr0["period"] : "";
                singleNlreport.Reference = sdr0["reference"] != DBNull.Value ? (string)sdr0["reference"] : "";
                singleNlreport.AccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                singleNlreport.TranDate = (DateTime)sdr0["TranDate"];
                singleNlreports.Add(singleNlreport);
            }
            conn.Close();

            //balance Brought Forward
            List<SingleNlreport> balanceAccount = new List<SingleNlreport>();
            conn.Open();
            string query1 = "SELECT c.\"NlAccName\", SUM(CASE WHEN a.\"TranDate\" < '"+from.ToString("yyyy-MM-dd")+"' THEN b.\"Dr\":: numeric ELSE 0 END) - SUM(CASE WHEN a.\"TranDate\" < '"+from.ToString("yyyy-MM-dd")+"' THEN b.\"Cr\"::numeric ELSE 0 END) AS balance_brought_forward   " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" WHERE b.\"NlAccCode\" = '"+code+"' GROUP BY c.\"NlAccName\"    ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, conn).ExecuteReader();
            while (sdr1.Read())
            {
                SingleNlreport account = new SingleNlreport();
                account.AccName = sdr1["NlAccName"] != DBNull.Value ? (string)sdr1["NlAccName"] : "";
                account.BalanceDetails = "Balance Brought Forward";
                account.BalanceBroughtForward = sdr1["balance_brought_forward"] != DBNull.Value ? (decimal)sdr1["balance_brought_forward"] : 0;
                balanceAccount.Add(account);
            }
            conn.Close();
            singleNlreports.AddRange(balanceAccount);
            singleNlreports.Sort((x, y) => x.TranDate.CompareTo(y.TranDate));
            return singleNlreports;
        }
        public List<SingleNlreport> AccountGroupReportDetailsByPeriod(string code, string periodfrom, string periodto)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //string query = "SELECT c.\"NlAccCode\", c.\"NlAccName\", b.\"JrnlSlNo\", a.\"TranDate\", a.\"TranFrom\", b.\"Dr\", b.\"Cr\", concat(a.\"TranPeriod\",'/', a.\"TranYear\") as period, " +
            //    "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN d.\"currentCustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" WHEN a.\"TranFrom\" = 'PL-PY' then f.\"CustName\" END AS accDescription, " +
            //    "CASE when a.\"TranFrom\" = 'SL-PY' THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'PL-PY' THEN e.\"pyChequeNumber\"WHEN a.\"TranFrom\" = 'J-E' then b.\"FolioNo\" END as reference " +
            //    "FROM  \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" " +
            //    "LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"SLReceipts\" d on d.\"pyID\" = a.\"SlJrnlNo\" \r\nLEFT JOIN \"PLReceipts\" e on e.\"pyID\" = a.\"PlJrnlNo\" " +
            //    "LEFT JOIN \"PLCustomer\" f on f.\"CustID\" = e.supplier_id " +
            //    "WHERE b.\"NlAccCode\"='"+code+"' and a.\"TranPeriod\" BETWEEN '" + periodfrom.Split("/")[0] +"' and '" + periodto.Split("/")[0] +"' and a.\"TranYear\" BETWEEN '" + periodfrom.Split("/")[1] +"' and '" + periodto.Split("/")[1] +"' " +
            //    "group by c.\"NlAccCode\", c.\"NlAccName\", b.\"JrnlSlNo\", a.\"TranDate\", a.\"TranFrom\", b.\"Dr\", b.\"Cr\", a.\"TranPeriod\", a.\"TranYear\", d.\"currentCustName\", b.\"Narration\", f.\"CustName\", d.\"pyChequeNumber\", e.\"pyChequeNumber\", b.\"FolioNo\"; ";
            //UPDATED QUERY FOR GETTING ACCOUNT DETAILS BY PERIODS 
            string query = "SELECT  c.\"NlAccCode\", c.\"NlAccName\", b.\"JrnlSlNo\", a.\"TranDate\", a.\"TranFrom\", b.\"Dr\"::numeric, b.\"Cr\"::numeric, concat(a.\"TranPeriod\",'/',a.\"TranYear\") as period,       " +
                "CASE WHEN a.\"TranFrom\" in ('SL-PY','SL-PY-REVERSAL') THEN d.\"currentCustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" WHEN a.\"TranFrom\" in ('PL-PY','PL-PY-REVERSAL')  then f.\"CustName\"           " +
                "when a.\"TranFrom\" in ('SL', 'SL-CRN', 'SL-REVERSAL') then i.\"CustCompany\" when a.\"TranFrom\" in ('PL', 'PL-CRN', 'PL-REVERSAL') then j.\"CustName\" END AS accDescription,      " +
                "CASE when a.\"TranFrom\" in ('SL-PY','SL-PY-REVERSAL') THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" in ('PL-PY', 'PL-PY-REVERSAL') THEN e.\"pyChequeNumber\"WHEN a.\"TranFrom\" = 'J-E' then b.\"FolioNo\"           " +
                "when A.\"TranFrom\" IN ('SL', 'SL-CRN', 'SL-REVERSAL') THEN concat('',g.\"SLJrnlNo\") WHEN A.\"TranFrom\" IN ('PL', 'PL-CRN', 'PL-REVERSAL') THEN concat('',h.\"PLJrnlNo\") END as reference      " +
                "FROM  \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"SLReceipts\" d on d.journal_id = a.\"NlJrnlNo\" LEFT JOIN \"PLReceipts\" e on e.\"pyID\" = a.\"NlJrnlNo\"    " +
                "LEFT JOIN \"PLCustomer\" f on f.\"CustID\" = e.supplier_id left join \"SLInvoiceHeader\" g on g.\"SLJrnlNo\" = a.\"SlJrnlNo\"  left join \"PLInvoiceHeader\" h on h.\"PLJrnlNo\" = a.\"PlJrnlNo\" left join \"SLCustomer\" i on i.\"SLCustomerSerial\" = g.\"CustId\"   " +
                "left join \"PLCustomer\" j on j.\"CustID\" = h.\"PLCustID\" WHERE b.\"NlAccCode\"='"+code+"' and a.\"TranPeriod\" between '"+periodfrom.Split("/")[0]+"' and '"+periodto.Split("/")[0]+"' and a.\"TranYear\" between '"+periodfrom.Split("/")[1]+"' and '"+periodto.Split("/")[1]+"'  ";

            List<SingleNlreport> singleNlreports = new List<SingleNlreport> ();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                SingleNlreport singleNlreport = new SingleNlreport();
                singleNlreport.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                singleNlreport.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                singleNlreport.Journal = sdr0["JrnlSlNo"] != DBNull.Value ? (int)sdr0["JrnlSlNo"] : 0;
                singleNlreport.Description = sdr0["accDescription"] != DBNull.Value ? (string)sdr0["accDescription"] : "";
                singleNlreport.AccName = sdr0["NlAccName"] != DBNull.Value ? (string)sdr0["NlAccName"] : "";
                singleNlreport.Period = sdr0["period"] != DBNull.Value ? (string)sdr0["period"] : "";
                singleNlreport.Reference = sdr0["reference"] != DBNull.Value ? (string)sdr0["reference"] : "";
                singleNlreport.AccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                singleNlreport.TranDate = (DateTime)sdr0["TranDate"];
                singleNlreports.Add(singleNlreport);
            }
            cnn.Close();
            
            //getting balance brought forward
            List<SingleNlreport> accountBalance = new List<SingleNlreport> ();
            cnn.Open();
            string[] periodFromComponents = periodfrom.Split('/');
            string periodFromMonth = periodFromComponents[0];
            string periodFromYear = periodFromComponents[1];
            string query1 = "SELECT c.\"NlAccName\",  " +
                "SUM(CASE WHEN (a.\"TranYear\" < '"+ periodFromYear + "' OR (a.\"TranYear\" = '"+ periodFromYear + "' AND a.\"TranPeriod\" < '"+ periodFromMonth + "')) THEN b.\"Dr\":: numeric ELSE 0 END) - " +
                "SUM(CASE WHEN (a.\"TranYear\" < '"+ periodFromYear + "' OR (a.\"TranYear\" = '"+ periodFromYear + "' AND a.\"TranPeriod\" < '"+ periodFromMonth + "')) THEN b.\"Cr\":: numeric ELSE 0 END) AS balance_brought_forward" +
                "  FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" WHERE b.\"NlAccCode\" = '"+ code +"' " +
                "GROUP BY c.\"NlAccName\"  ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                SingleNlreport balance = new SingleNlreport();
                balance.AccName = sdr1["NlAccName"] != DBNull.Value ? (string)sdr1["NlAccName"] : "";
                balance.BalanceDetails = "Balance Brought Forward";
                balance.BalanceBroughtForward = sdr1["balance_brought_forward"] != DBNull.Value ? (decimal)sdr1["balance_brought_forward"] : 0;
                accountBalance.Add(balance);
            }
            cnn.Close();
            singleNlreports.AddRange(accountBalance);
            singleNlreports.Sort((x, y) => x.TranDate.CompareTo(y.TranDate));
            return singleNlreports;
        }
        public List<NLJournalDetails> GetNLJournalDetails(int journalId)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<NLJournalDetails> nljournaldetails = new List<NLJournalDetails>();
            cnn.Open();
            string Cr = "Cr";
            string Dr = "Dr";
            string query = "SELECT a.\"JrnlSlNo\",b.\"NlAccName\" as  NlAccCode ,SUM(a.\"Dr\") as "+Dr+" ,SUM(a.\"Cr\") as "+Cr+", concat(c.\"TranPeriod\",'/',c.\"TranYear\") as period     " +
                "FROM \"NLJournalDetails\" a LEFT JOIN \"NLAccount\" b on b.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"NlJournalHeader\" c on c.\"NlJrnlNo\" = a.\"JrnlSlNo\"  where a.\"JrnlSlNo\" = '"+journalId+"' Group By b.\"NlAccName\" ,a.\"JrnlSlNo\", c.\"TranPeriod\", c.\"TranYear\";    ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                NLJournalDetails detail = new NLJournalDetails();
                detail.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                detail.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                detail.JrnlSlNo = (int)sdr0["JrnlSlNo"];
                detail.NlAccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : null;
                detail.Period = sdr0["period"] != DBNull.Value ? (string)sdr0["period"] : "";
                nljournaldetails.Add(detail);
            }
            cnn.Close();
            return nljournaldetails;
        }
        public MyResponse updateNlJournalDetail(NLJournalDetails recvData)
        {
            throw new NotImplementedException();
        }
        public List<SLAnalysisCodes> GetSlanalysisCodes()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<SLAnalysisCodes> sLAnalysisCodes = new List<SLAnalysisCodes>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"SLAnalysisCodes\"  ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                SLAnalysisCodes sLAnalysisCode = new SLAnalysisCodes();
                sLAnalysisCode.AnalCode = sdr0["AnalCode"] != DBNull.Value ? (string)sdr0["AnalCode"] : null;
                sLAnalysisCode.AnalDesc = sdr0["AnalDesc"] != DBNull.Value ? (string)sdr0["AnalDesc"] : null;
                sLAnalysisCode.AnalType = sdr0["AnalType"] != DBNull.Value ? (string)sdr0["AnalType"] : null;
                sLAnalysisCode.NLAccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
                sLAnalysisCode.Id = (int)sdr0["id"]; ;
                sLAnalysisCodes.Add(sLAnalysisCode);
            }
            cnn.Close();
            return sLAnalysisCodes;
        }
        public MyResponse addSlanalysis(SLAnalysisCodes sLAnalysisCodes)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn1.Open();
            ///check if that analysiscode exists
            int groupnamecount = 0;
            ////check if group name exists
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"SLAnalysisCodes\" where  \"AnalCode\"= '" + sLAnalysisCodes.AnalCode + "' ", cnn1).ExecuteReader();
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
            string insertQuery1 = "INSERT INTO \"SLAnalysisCodes\" (\"NLAccCode\",\"AnalType\",\"AnalDesc\",\"AnalCode\") VALUES('" + sLAnalysisCodes.NLAccCode + "','" + sLAnalysisCodes.AnalType + "','" + sLAnalysisCodes.AnalDesc + "', '" + sLAnalysisCodes.AnalCode + "' );   ";
            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, OrganizationId);
            cnn1.Close();
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            return response;
        }
        public MyResponse updateSlanalysis(SLAnalysisCodes sLAnalysisCodes)
        {
            MyResponse response = new MyResponse();
            string updtQ = "UPDATE \"SLAnalysisCodes\" SET \"NLAccCode\" = '" + sLAnalysisCodes.NLAccCode + "',\"AnalType\"='" + sLAnalysisCodes.AnalType + "',\"AnalDesc\"='" + sLAnalysisCodes.AnalDesc + "',\"AnalCode\" = '"
                 + sLAnalysisCodes.AnalCode.ToUpper() + "',\"ModifiedOn\"='" + DateTime.Now + "' WHERE \"id\" = '" + sLAnalysisCodes.Id + "' ";
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            return response;
        }
        public SLAnalysisCodes findSlAnalysisCodeById(int Id)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //geT nljournal
            SLAnalysisCodes sLAnalysisCodes = new SLAnalysisCodes();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"SLAnalysisCodes\" where  id ='" + Id + "' ", cnn).ExecuteReader();
            SLAnalysisCodes sLAnalysisCode = new SLAnalysisCodes();
            while (sdr0.Read())
            {
                sLAnalysisCode.AnalCode = sdr0["AnalCode"] != DBNull.Value ? (string)sdr0["AnalCode"] : null;
                sLAnalysisCode.AnalDesc = sdr0["AnalDesc"] != DBNull.Value ? (string)sdr0["AnalDesc"] : null;
                sLAnalysisCode.AnalType = sdr0["AnalType"] != DBNull.Value ? (string)sdr0["AnalType"] : null;
                sLAnalysisCode.NLAccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
                sLAnalysisCode.Id = (int)sdr0["id"];
            }
            cnn.Close();
            return sLAnalysisCode;
        }
        public Nlaccount GetNLAccountAccountCodeUsingId(int id)
        {
            string myid = id.ToString().PadLeft(4, '0');
            string cust_check = "SELECT * FROM \"NLAccount\" WHERE \"NlAccCode\"  ='" + myid + "' ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(cust_check, cnn).ExecuteReader();
            Nlaccount nlaccount = new Nlaccount();
            while (sdr0.Read())
            {
                nlaccount.LastStatDate = sdr0["LastStatDate"] != DBNull.Value ? (DateTime)sdr0["LastStatDate"] : DateTime.Today;
                nlaccount.StatBalance = sdr0["StatBalance"] != DBNull.Value ? (decimal)sdr0["StatBalance"] : 0;
                nlaccount.GroupCode = sdr0["AGroupCode"] != DBNull.Value ? (string)sdr0["AGroupCode"] : null;
                nlaccount.IsMirrorAcc = (bool)sdr0["isMirrorAcc"];
                nlaccount.CurCode = sdr0["CurCode"] != DBNull.Value ? (string)sdr0["CurCode"] : null;
                nlaccount.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccount.NlaccName = sdr0["NLAccName"] != DBNull.Value ? (string)sdr0["NLAccName"] : null;
                nlaccount.NlaccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
            }
            cnn.Close();
            return nlaccount;
        }
        public Nlaccount GetNLAccountAccountCodeUsingName(string accountName)
        {
            string cust_check = "SELECT * FROM \"NLAccount\" WHERE \"NlAccName\"  ='" + accountName + "' ";
            int cust_check_res = myDbconnection.CheckRowExists(cust_check, OrganizationId);
            if (cust_check_res == 0)
            {
                return null;
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            NLAccount nLAccount = new NLAccount();
            cnn.Open();
            //UPDATE THE QUERY FROM ~* TO =
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLAccount\" where  \"NlAccName\" = '" + accountName + "'", cnn).ExecuteReader();
            Nlaccount nlaccount = new Nlaccount();
            while (sdr0.Read())
            {
                nlaccount.LastStatDate = sdr0["LastStatDate"] != DBNull.Value ? (DateTime)sdr0["LastStatDate"] : DateTime.Today;
                nlaccount.StatBalance = sdr0["StatBalance"] != DBNull.Value ? (decimal)sdr0["StatBalance"] : 0;
                nlaccount.GroupCode = sdr0["AGroupCode"] != DBNull.Value ? (string)sdr0["AGroupCode"] : null;
                nlaccount.IsMirrorAcc = (bool)sdr0["isMirrorAcc"];
                nlaccount.CurCode = sdr0["CurCode"] != DBNull.Value ? (string)sdr0["CurCode"] : null;
                nlaccount.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : null;
                nlaccount.NlaccName = sdr0["NLAccName"] != DBNull.Value ? (string)sdr0["NLAccName"] : null;
                nlaccount.NlaccCode = sdr0["NLAccCode"] != DBNull.Value ? (string)sdr0["NLAccCode"] : null;
            }
            cnn.Close();
            return nlaccount;
        }
        public MyResponse createNlJournalHeaderpl(NlJournalHeader recvData, string transtype, List<InvoiceListDetailsData> InvoiceDetailsList)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"PlJrnlNo\",\"SlJrnlNo\",\"ModuleId\",\"CreatedOn\") " +
                    "VALUES('" + recvData.NlJrnlDesc + "','" + recvData.TranDate + "','" + recvData.MEndDate + "', '" + recvData.TranPeriod + "', '" + recvData.TranYear + "','PL','NT','" + recvData.PlJrnlNo + "'," + 0 + ",'" + 0 + "','" + DateTime.Now + "' ) RETURNING \"NlJrnlNo\" ;";

                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    decimal vatAmount = 0;
                    decimal purchaseAmount = 0;
                    //total salesAmount+vatAmount
                    decimal creditors = 0;
                    StringBuilder purchasedebitentry = new StringBuilder();
                    StringBuilder vatdebitentry = new StringBuilder();
                    string debitentry2 = "";
                    foreach (var draccount in InvoiceDetailsList)
                    {
                        vatAmount = vatAmount + draccount.VatAmt;
                        purchaseAmount = purchaseAmount + (draccount.ItemUnitPrice * draccount.ItemQty);
                        creditors = vatAmount + purchaseAmount;
                        string description = "Purchase of " + draccount.Description;
                        purchasedebitentry.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")  " +
                        "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName(draccount.account).NlaccCode + "','" + (draccount.ItemUnitPrice * draccount.ItemQty) + "', '', '" + (draccount.ItemUnitPrice * draccount.ItemQty) + "', '" + description + "','','false','','false','false','" + 0 + "');");

                        vatdebitentry.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\",\"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")  " +
                          "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("VAT").NlaccCode + "','" + draccount.VatAmt + "', '', '" + (draccount.VatAmt) + "', ' VAT on purchases','narration','false','','false','false','" + draccount.VatAmt + "');");

                        debitentry2 = purchasedebitentry.ToString() + vatdebitentry.ToString();
                    }
                    string creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")  " +
                      "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','', '" + creditors + "', '" + (creditors) + "', 'amount to pay','','false','','false','false','" + 0 + "');";
                    cmd.CommandText = creditentry1 + debitentry2;
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";
                }
            }
            cnn.Close();
            return response;
        }
        public AddCustomer getCustomerById(int id)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //Get all customers
            string myQuery = "SELECT * FROM  \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + id + " LIMIT 1 ; ";
            cnn.Open();
            AddCustomer addCust = new AddCustomer();
            NpgsqlDataReader sdr = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr.Read())
            {
                addCust.SLCustomerSerial = (int)sdr["SLCustomerSerial"];
                addCust.CustCode = sdr["CustCode"] != DBNull.Value ? (string)sdr["CustCode"] : "";
                addCust.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                addCust.Address = sdr["Address"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.CurCode = (int)sdr["CurCode"];
                addCust.CustEmail = sdr["CustEmail"].ToString();
                addCust.CustContact = sdr["CustContact"].ToString();
                addCust.SLCTypeID = (int)sdr["SLCTypeID"];
                addCust.CustLastName = sdr["CustLastName"] != DBNull.Value ? (string)sdr["CustLastName"] : null;
                addCust.CustType = sdr["CustType"].ToString();
                addCust.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                addCust.VATNo = sdr["VATNo"] != DBNull.Value ? (string)sdr["VATNo"] : null;
                addCust.CustCreditLimit = (float)sdr["CustCreditLimit"];
                addCust.VATpin = sdr["VATpin"].ToString();
                addCust.CreditTerms = (int)sdr["CreditTerms"];
                addCust.OpeningBalance = sdr["OpeningBalance"] != DBNull.Value ? (decimal)sdr["OpeningBalance"] : 0;
                addCust.OpeningBalanceDate = sdr["OpeningBalanceDate"] != DBNull.Value ? (DateTime)sdr["OpeningBalanceDate"] : DateTime.Now;
                //addCust.CurrCode = sdr["CrCode"].ToString();
                //addCust.CustBranch = sdr["CustBranch"] != DBNull.Value ? (int)sdr["CustBranch"] : 0;
                //addCust.CustRef = sdr["CustRef"] != DBNull.Value ? (string)sdr["CustRef"] : null;
                //addCust.CustomerDept = sdr["CustomerDept"] != DBNull.Value ? (decimal)sdr["CustomerDept"] : 0;
            }
            cnn.Close();
            return addCust;
        }
        public AddCustomer getCustomerByCode(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //Get all customers
            string myQuery = "SELECT * FROM  \"SLCustomer\" WHERE \"CustCode\" = " + code + " LIMIT 1 ; ";
            cnn.Open();
            AddCustomer addCust = new AddCustomer();
            NpgsqlDataReader sdr = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr.Read())
            {
                addCust.SLCustomerSerial = (int)sdr["SLCustomerSerial"];
                addCust.CustCode = sdr["CustCode"].ToString();
                addCust.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                addCust.Address = sdr["Address"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.CurCode = (int)sdr["CurCode"];
                addCust.CustEmail = sdr["CustEmail"].ToString();
                addCust.CustContact = sdr["CustContact"].ToString();
                addCust.SLCTypeID = (int)sdr["SLCTypeID"];
                addCust.CustLastName = sdr["CustLastName"] != DBNull.Value ? (string)sdr["CustLastName"] : null;
                addCust.CustType = sdr["CustType"].ToString();
                addCust.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                addCust.VATNo = sdr["VATNo"] != DBNull.Value ? (string)sdr["VATNo"] : null;
                addCust.CustCreditLimit = (float)sdr["CustCreditLimit"];
                addCust.VATpin = sdr["VATpin"].ToString();
                addCust.CreditTerms = (int)sdr["CreditTerms"];
                //addCust.CurrCode = sdr["CrCode"].ToString();
                //addCust.CustBranch = sdr["CustBranch"] != DBNull.Value ? (int)sdr["CustBranch"] : 0;
                //addCust.CustRef = sdr["CustRef"] != DBNull.Value ? (string)sdr["CustRef"] : null;
                //addCust.CustomerDept = sdr["CustomerDept"] != DBNull.Value ? (decimal)sdr["CustomerDept"] : 0;
            }
            cnn.Close();
            return addCust;
        }
        /// <summary>
        ///  the first journal entry made is a debit to a Sales Return account
        ///  The credit is made to the buyer’s account or to accounts receivable.
        ///  sales reduces
        ///  inventory changes when applicable
        ///  update customer credit
        /// </summary>
        /// <param name="accountName"></param>
        ///  /// <param name="inv"></param>
        ///  /// <param name="custid"></param>
        public MyResponse updateAccountsOnCreditNoteCreation(Invoice inv, NlJournalHeader recvData, int lastinsertedcrnid, string reason, DateTime crnDate, decimal crnVat, int periodMonth, int periodYear, string vatPercent)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"CreatedOn\") " +
                        "VALUES('" + reason + "','" + recvData.TranDate + "','" + recvData.MEndDate + "', '" + periodMonth + "', '" + periodYear + "','SL-CRN','NT','" + inv.SLJrnlNo + "'," + 0 + ",'" + 0 + "' ,'" + DateTime.Now + "') RETURNING \"NlJrnlNo\" ; ";
                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    int CRN = lastinsertedcrnid + 1;
                    //Mark as dispute add reason update lastcrn invoicetypeRef and update reasons
                    string myQuery = "UPDATE \"SLInvoiceHeader\" SET \"CRNReason\" = '" + reason + "', \"Dispute\" = 't' ,\"INVTypeRef\" = 'CRN' , \"HasCreditNote\" = 't'  ,\"DocRef\" = '" + CRN + "' ,\"TotalBalance\" = \"TotalBalance\" - " + inv.CreditNoteAmount + ",   " +
                        "\"CreditNoteAmount\" ='" + inv.CreditNoteAmount + "', \"CRNDate\" = '"+ crnDate +"', \"CRNVat\" = '"+ crnVat +"', \"CrnVatPercent\" = '"+vatPercent+"' WHERE \"SLJrnlNo\" = " + inv.SLJrnlNo + " ; ";

                    ///SALES RETURNS RETURN DEBIT
                    string debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("SALES").NlaccCode + "','" + inv.CreditNoteAmount + "', '', '" + (inv.CreditNoteAmount) + "', 'sales return','narration','false','','false','false','" + 0 + "'); ";
                    /// CREDIT DEBTORS (Accounts Receivable)
                    string creditentry2 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                        "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','', '" + inv.CreditNoteAmount + "', '" + (inv.CreditNoteAmount) + "', 'total debtors','','false','','false','false','" + 0 + "'); ";

                    ///U[DATE CUSTOMER CREDIT 
                    string up_cust = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = \"CustomerDept\" - " + inv.CreditNoteAmount + " WHERE \"CustCode\" = '" + inv.CustCode + "' ; ";
                    cmd.CommandText = up_cust + creditentry2 + debitentry1 + myQuery;
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";
                }
            }
            cnn.Close();
            return response;
        }
        public List<CustomerSalesLedger> getCustomerLedgerCardUnAllocatedpayments(int Ref, DateTime From, DateTime To)
        {
            string query = "SELECT  c.\"NlJrnlNo\", a.\"Cr\",a.\"Dr\", receipts1.\"pyInvRef\", c.\"TranDate\", receipts1.\"pyDate\", c.\"TranFrom\", receipts1.\"pyChequeNumber\" as docref, receipts1.allocation_remainder , receipts1.\"pyAdditionalDetails\"   " +
                "FROM \"NLJournalDetails\" a LEFT JOIN \"NLAccount\" b ON b.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"NlJournalHeader\" c ON c.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLReceipts\" receipts1 ON receipts1.\"journal_id\" = c.\"NlJrnlNo\"   " +
                "WHERE receipts1.\"cust_id\" = '"+Ref+"' AND b.\"NlAccName\" ~*'debtors'  and receipts1.\"pyDate\" BETWEEN '"+From.ToString("yyyy-MM-dd")+"'  AND '"+To.ToString("yyyy-MM-dd")+"' and  c.\"TranFrom\" = 'SL-PY' and receipts1.allocation_remainder > '0'  " +
                "ORDER BY c.\"NlJrnlNo\" DESC ; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CustomerSalesLedger> salesLedgers = new List<CustomerSalesLedger>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                CustomerSalesLedger unallocatedsalesledger = new CustomerSalesLedger();
                unallocatedsalesledger.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                unallocatedsalesledger.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                unallocatedsalesledger.DocRef = sdr0["docref"] != DBNull.Value ? (string)sdr0["docref"] : null;
                unallocatedsalesledger.Description = sdr0["pyAdditionalDetails"] != DBNull.Value ? (string)sdr0["pyAdditionalDetails"] : null;
                unallocatedsalesledger.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                unallocatedsalesledger.TransactionType = (string)sdr0["TranFrom"];
                unallocatedsalesledger.SLJrnlNo = 0;
                unallocatedsalesledger.TransactionDate = (DateTime)sdr0["pyDate"];
                salesLedgers.Add(unallocatedsalesledger);
            }
            cnn.Close();
            return salesLedgers;
        }
        public List<CustomerSalesLedger> GetCustomerLedgerCardAllocatedPayments(int Ref, DateTime From, DateTime To)
        {
            string query = "SELECT  c.\"NlJrnlNo\", a.\"Cr\", a.\"Dr\", receipts1.\"pyInvRef\", c.\"TranDate\", receipts1.\"pyDate\", c.\"TranFrom\", receipts1.\"pyChequeNumber\" as docref, receipts1.allocation_remainder,  receipts1.\"pyAdditionalDetails\"  " +
                "FROM \"NLJournalDetails\" a LEFT JOIN \"NLAccount\" b ON b.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"NlJournalHeader\" c ON c.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLReceipts\" receipts1 ON receipts1.\"journal_id\" = c.\"NlJrnlNo\"   " +
                "WHERE receipts1.\"cust_id\" = '"+Ref+"' AND b.\"NlAccName\" ~*'debtors'  and receipts1.\"pyDate\" BETWEEN '"+From.ToString("yyyy-MM-dd")+"'  AND '"+To.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL-PY' and (receipts1.allocation_remainder is NULL OR receipts1.allocation_remainder <= '0')  " +
                "ORDER BY c.\"NlJrnlNo\" DESC ; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CustomerSalesLedger> salesLedgers = new List<CustomerSalesLedger>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                CustomerSalesLedger unallocatedsalesledger = new CustomerSalesLedger();
                unallocatedsalesledger.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                unallocatedsalesledger.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                unallocatedsalesledger.DocRef = sdr0["docref"] != DBNull.Value ? (string)sdr0["docref"] : null;
                unallocatedsalesledger.Description = sdr0["pyAdditionalDetails"] != DBNull.Value ? (string)sdr0["pyAdditionalDetails"] : null;
                unallocatedsalesledger.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                unallocatedsalesledger.TransactionType = (string)sdr0["TranFrom"];
                unallocatedsalesledger.SLJrnlNo = 0;
                unallocatedsalesledger.TransactionDate = (DateTime)sdr0["pyDate"];
                salesLedgers.Add(unallocatedsalesledger);
            }
            cnn.Close();
            return salesLedgers;
        }
        public List<CustomerSalesLedger> GetCustomerLedgerReversal(string Ref, DateTime From, DateTime To)
        {
            string query = "SELECT  c.\"NlJrnlNo\", a.\"Cr\",a.\"Dr\", inv.\"SLJrnlNo\", c.\"TranDate\", inv.\"INVDate\", c.\"TranFrom\" , inv.\"origin_status\", inv.\"post_number\", inv.\"CustCode\", inv.\"StatementDescription\"         " +
                "FROM \"NLJournalDetails\" a   LEFT JOIN \"NLAccount\" b ON b.\"NlAccCode\" = a.\"NlAccCode\"  LEFT JOIN \"NlJournalHeader\" c ON c.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" inv ON inv.\"SLJrnlNo\" = c.\"SlJrnlNo\"     " +
                "WHERE b.\"NlAccName\" ~*'debtors'  AND inv.\"CustCode\" = '"+Ref+"' and inv.\"INVDate\" BETWEEN '"+From.ToString("yyyy-MM-dd")+"' AND '"+To.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL-REVERSAL'     " +
                "ORDER BY  c.\"NlJrnlNo\" DESC; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CustomerSalesLedger> salesLedgers = new List<CustomerSalesLedger>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                CustomerSalesLedger salesledger = new CustomerSalesLedger();
                salesledger.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                salesledger.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                salesledger.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                salesledger.TransactionType = (string)sdr0["TranFrom"];
                salesledger.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                salesledger.TransactionDate = (DateTime)sdr0["INVDate"];
                salesledger.origin_status = (string)sdr0["origin_status"];
                salesledger.invoicenumber = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                salesledger.Description = sdr0["StatementDescription"] != DBNull.Value ? (string)sdr0["StatementDescription"] : "";
                salesLedgers.Add(salesledger);
            }
            cnn.Close();
            return salesLedgers;
        }
        public List<CustomerSalesLedger> GetCustomerLedgerCreditNote(string Ref, DateTime From, DateTime To)
        {
            string query = "SELECT  c.\"NlJrnlNo\", a.\"Cr\", a.\"Dr\", inv.\"SLJrnlNo\", c.\"TranDate\", inv.\"CRNDate\", c.\"TranFrom\" , inv.\"origin_status\", inv.\"post_number\", inv.\"CustCode\" , inv.\"CRNReason\"        " +
                "FROM \"NLJournalDetails\" a   LEFT JOIN \"NLAccount\" b ON b.\"NlAccCode\" = a.\"NlAccCode\"  LEFT JOIN \"NlJournalHeader\" c ON c.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" inv ON inv.\"SLJrnlNo\" = c.\"SlJrnlNo\"     " +
                "WHERE b.\"NlAccName\" ~*'debtors'  AND inv.\"CustCode\" = '"+Ref+"' and inv.\"CRNDate\" BETWEEN '"+From.ToString("yyyy-MM-dd")+"' AND '"+To.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL-CRN'     " +
                "ORDER BY  c.\"NlJrnlNo\" DESC; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CustomerSalesLedger> salesLedgers = new List<CustomerSalesLedger>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                CustomerSalesLedger salesledger = new CustomerSalesLedger();
                salesledger.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                salesledger.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                salesledger.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                salesledger.TransactionType = (string)sdr0["TranFrom"];
                salesledger.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                salesledger.TransactionDate = (DateTime)sdr0["CRNDate"];
                salesledger.origin_status = (string)sdr0["origin_status"];
                salesledger.invoicenumber = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                salesledger.Description = sdr0["CRNReason"] != DBNull.Value ? (string)sdr0["CRNReason"] : "";
                salesLedgers.Add(salesledger);
            }
            cnn.Close();
            return salesLedgers;
        }
        public CustomerSalesLedger GetInvoiceBalanceBroughtForward(int custId, DateTime from)
        {
            string invoiceQuery = "select a.\"CustId\", b.\"CustCompany\", b.\"OpeningBalance\", sum(case when a.\"INVDate\" < '"+from.ToString("yyyy-MM-dd")+"' then d.\"Dr\"::numeric else 0 end) - sum(case when a.\"INVDate\" < '"+from.ToString("yyyy-MM-dd")+"' then d.\"Cr\":: numeric else 0 end) as invBalance    " +
                "from \"SLInvoiceHeader\" a left join \"SLCustomer\" b on b.\"SLCustomerSerial\" = a.\"CustId\" left join \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\" left join \"NLJournalDetails\" d on d.\"JrnlSlNo\" = c.\"NlJrnlNo\" left join \"NLAccount\" e on e.\"NlAccCode\" = d.\"NlAccCode\"    " +
                "where  a.\"CustId\" = '"+custId+"' and e.\"NlAccName\" ~* 'debtors' and c.\"TranFrom\" = 'SL' group by a.\"CustId\", b.\"CustCompany\", b.\"OpeningBalance\"; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            CustomerSalesLedger salesLedger = new CustomerSalesLedger();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(invoiceQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                salesLedger.CustId = reader["CustId"] != DBNull.Value ? (int)reader["CustId"] : 0;
                salesLedger.InvoiceBalance = reader["invBalance"] != DBNull.Value ? (decimal)reader["invBalance"] : 0;
            }
            cnn.Close();
            return salesLedger;
        }
        public CustomerSalesLedger GetPaymentBalance(int custId, DateTime from)
        {
            string payQuery = "select a.cust_id, a.\"currentCustName\", e.\"OpeningBalance\", sum(case when a.\"pyDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Dr\"::numeric else 0 end) - sum(case when a.\"pyDate\" < '"+from.ToString("yyyy-MM-dd")+"' then c.\"Cr\"::numeric else 0 end) as payBalance     " +
                "from \"SLReceipts\" a left join \"NlJournalHeader\" b on b.\"NlJrnlNo\" = a.journal_id left join \"NLJournalDetails\" c on c.\"JrnlSlNo\" = b.\"NlJrnlNo\" left join \"NLAccount\" d on d.\"NlAccCode\" = c.\"NlAccCode\" left join \"SLCustomer\" e on e.\"SLCustomerSerial\" = a.cust_id    " +
                "where a.cust_id = '"+custId+"' and d.\"NlAccName\" ~* 'debtors' and b.\"TranFrom\" = 'SL-PY' group by a.cust_id, a.\"currentCustName\", e.\"OpeningBalance\";   ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn (OrganizationId));
            CustomerSalesLedger customerSalesLedger = new CustomerSalesLedger();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(payQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                customerSalesLedger.CustId = reader["cust_id"] != DBNull.Value ? (int)reader["cust_id"] : 0;
                customerSalesLedger.PaymentBalance = reader["payBalance"] != DBNull.Value ? (decimal)reader["payBalance"] : 0;
            }
            cnn.Close();
            return customerSalesLedger;
        }
        public CustomerSalesLedger GetCRNBalance(int custId, DateTime from)
        {
            string CrnQuery = "select a.\"CustId\", b.\"CustCompany\", b.\"OpeningBalance\", sum(case when a.\"CRNDate\" < '"+from.ToString("yyyy-MM-dd")+"' then d.\"Dr\"::numeric else 0 end) - sum(case when a.\"CRNDate\" < '"+from.ToString("yyyy-MM-dd")+"' then d.\"Cr\"::numeric else 0 end) as CRNBal    " +
                "from \"SLInvoiceHeader\" a left join \"SLCustomer\" b on b.\"SLCustomerSerial\" = a.\"CustId\" left join \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\" left join \"NLJournalDetails\" d on d.\"JrnlSlNo\" = c.\"NlJrnlNo\" left join \"NLAccount\" e on e.\"NlAccCode\" = d.\"NlAccCode\"   " +
                "where a.\"CustId\" = '"+custId+"' and e.\"NlAccName\" ~* 'debtors' and c.\"TranFrom\" = 'SL-CRN' group by a.\"CustId\", b.\"CustCompany\", b.\"OpeningBalance\";  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            CustomerSalesLedger customerSales = new CustomerSalesLedger();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(CrnQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                customerSales.CustId = reader["CustId"] != DBNull.Value ? (int)reader["CustId"] : 0;
                customerSales.CrnBalance = reader["CRNBal"] != DBNull.Value ? (decimal)reader["CRNBal"] : 0;
            }
            cnn.Close();
            return customerSales;
        }
        public CustomerSalesLedger GetReversalBalance(int custId, DateTime from)
        {
            string reversalBalQuery = "select a.\"CustId\", b.\"CustCompany\", b.\"OpeningBalance\", sum(case when a.\"INVDate\" < '"+from.ToString("yyyy-MM-dd")+"' then d.\"Dr\"::numeric else 0 end) - sum(case when a.\"INVDate\" < '"+from.ToString("yyyy-MM-dd")+"' then d.\"Cr\"::numeric else 0 end) as ReversalBal " +
                "from \"SLInvoiceHeader\" a left join \"SLCustomer\" b on b.\"SLCustomerSerial\" = a.\"CustId\" left join \"NlJournalHeader\" c on c.\"SlJrnlNo\" = a.\"SLJrnlNo\" left join \"NLJournalDetails\" d on d.\"JrnlSlNo\" = c.\"NlJrnlNo\" left join \"NLAccount\" e on e.\"NlAccCode\" = d.\"NlAccCode\"   " +
                "where a.\"CustId\" = '"+custId+"' and e.\"NlAccName\" ~* 'debtors' and c.\"TranFrom\" = 'SL-REVERSAL' group by a.\"CustId\", b.\"CustCompany\", b.\"OpeningBalance\"; ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            CustomerSalesLedger customerSales = new CustomerSalesLedger();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(reversalBalQuery, cnn).ExecuteReader();
            while (reader.Read())
            {
                customerSales.CustId = reader["CustId"] != DBNull.Value ? (int)reader["CustId"] : 0;
                customerSales.ReversalBalance = reader["ReversalBal"] != DBNull.Value ? (decimal)reader["ReversalBal"] : 0;
            }
            cnn.Close();
            return customerSales;
        }
        public List<CustomerSalesLedger> getCustomerLedgerCard(string Ref, DateTime From, DateTime To, int cust_id)
        {
            string query3 = "SELECT  c.\"NlJrnlNo\", a.\"Cr\",a.\"Dr\", inv.\"SLJrnlNo\", c.\"TranDate\", inv.\"INVDate\", c.\"TranFrom\" , inv.\"origin_status\", inv.\"post_number\" , inv.\"StatementDescription\" , inv.\"SLDescription\"      " +
                "FROM \"NLJournalDetails\" a LEFT JOIN \"NLAccount\" b ON b.\"NlAccCode\" = a.\"NlAccCode\" LEFT JOIN \"NlJournalHeader\" c ON c.\"NlJrnlNo\" = a.\"JrnlSlNo\" LEFT JOIN \"SLInvoiceHeader\" inv ON inv.\"SLJrnlNo\" = c.\"SlJrnlNo\"    " +
                "WHERE b.\"NlAccName\" ~*'debtors'  AND inv.\"CustCode\" = '"+Ref+"' and inv.\"INVDate\" BETWEEN '"+From.ToString("yyyy-MM-dd")+"' AND '"+To.ToString("yyyy-MM-dd")+"' and c.\"TranFrom\" = 'SL'     " +
                "GROUP BY a.\"Cr\", a.\"Dr\", b.\"NlAccName\", inv.\"INVTypeRef\" , c.\"TranDate\" , inv.\"SLJrnlNo\", inv.\"origin_status\", inv.\"post_number\",  c.\"NlJrnlNo\", c.\"TranFrom\" , inv.\"SLDescription\", inv.\"StatementDescription\"  " +
                "ORDER BY  c.\"NlJrnlNo\" DESC; ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CustomerSalesLedger> salesLedgers = new List<CustomerSalesLedger>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query3, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                CustomerSalesLedger salesledger = new CustomerSalesLedger();
                salesledger.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                salesledger.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                //  salesledger.DocRef = sdr0["docref"] != DBNull.Value ? (string)sdr0["docref"] : null;
                salesledger.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                //salesledger.DocumentRef = (int)sdr0["documentref"];
                //salesledger.HasCreditNote = (bool)sdr0["HasCreditNote"];
                salesledger.TransactionType = (string)sdr0["TranFrom"];
                salesledger.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                salesledger.TransactionDate = (DateTime)sdr0["INVDate"];
                salesledger.origin_status = (string)sdr0["origin_status"];
                salesledger.invoicenumber = sdr0["post_number"] != DBNull.Value ? (string)sdr0["post_number"] : "";
                salesledger.Description = sdr0["StatementDescription"] != DBNull.Value ? (string)sdr0["StatementDescription"] : (string)sdr0["SLDescription"];
                //salesledger.Date =  (DateTime)sdr0["INVDate"] ;
                //salesledger.DueDate=  (DateTime)sdr0["DueDate"] ;
                //salesledger.Type = sdr0["INVTypeRef"] != DBNull.Value ? (string)sdr0["INVTypeRef"] : null;
                //salesledger.Pymode =sdr0["pyMode"] != DBNull.Value ? (string)sdr0["pyMode"] : null;
                //salesledger.PyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : null;
                salesLedgers.Add(salesledger);
            }
            cnn.Close();
            var dataunallocatedpayment = getCustomerLedgerCardUnAllocatedpayments(cust_id, From, To);
            var allocatedPay = GetCustomerLedgerCardAllocatedPayments(cust_id, From, To);
            var reserval = GetCustomerLedgerReversal(Ref, From, To);
            var creditNote = GetCustomerLedgerCreditNote(Ref, From, To);
            salesLedgers.AddRange(dataunallocatedpayment);
            salesLedgers.AddRange(allocatedPay);
            salesLedgers.AddRange(reserval);
            salesLedgers.AddRange(creditNote);
            salesLedgers.Sort((a, b) => a.JournalId.CompareTo(b.JournalId));
            return salesLedgers;
        }
        public MyResponse saveSalesReceiptsAccount(NlJournalHeader recvData, string transtype, float pyPaid, AddCustomer addCustomer)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"CreatedOn\") " +
                        "VALUES('" + recvData.NlJrnlDesc + "','" + recvData.TranDate + "','" + recvData.MEndDate + "', '" + recvData.TranPeriod + "', '" + recvData.TranYear + "','SL-PY','NT','" + recvData.SlJrnlNo + "'," + 0 + ",'" + 0 + "','" + DateTime.Now + "' ) RETURNING \"NlJrnlNo\" ;";
                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    float debtors = pyPaid;
                    string debitentry1;
                    /// credit debtors
                    string creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("DEBTORS").NlaccCode + "','0', '" + debtors + "', '" + (debtors) + "', 'debt reduced','','false','','false','false','" + 0 + "'); ";
                    /// debit asset(cash or bank )
                    if (transtype == "CASH")
                    {
                        debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("CASH").NlaccCode + "','" + debtors + "', '0', '" + (debtors) + "', 'payment by cash','','false','','false','false','" + 0 + "'); ";
                    }
                    else
                    {
                        debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("BANK").NlaccCode + "','" + debtors + "', '0', '" + (debtors) + "', 'payment by cheque','','false','','false','false','" + 0 + "'); ";
                    }
                    // reduce customer credit 
                    decimal new_credit_limit = addCustomer.CustomerDept - (decimal)debtors;
                    string update_customer_credit = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = '" + new_credit_limit + "' WHERE \"SLCustomerSerial\" = '" + addCustomer.SLCustomerSerial + "'; ";
                    cmd.CommandText = creditentry1 + debitentry1 + update_customer_credit;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";
                }
            }
            cnn.Close();
            return response;
        }
        /// <summary>
        /// Accounts Affected are purchase and creditors nl accounts
        /// </summary>
        /// <param name="recvData"></param>
        /// <param name="transtype"></param>
        /// <param name="pyPaid"></param>
        /// <param name="plInv"></param>
        /// <returns></returns>
        public MyResponse savePurchaseCreditNoteJournal(NlJournalHeader recvData, string transtype, float pyPaid, PLInvoice plInv)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            string debitentry1;
            string creditentry1;
            cnn.Open();
            int id;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"CreatedOn\") " +
                        "VALUES('" + recvData.NlJrnlDesc + "','" + recvData.TranDate + "','" + recvData.MEndDate + "', '" + recvData.TranPeriod + "', '" + recvData.TranYear + "','PL-CRN','NT'," + 0 + ",'" + recvData.PlJrnlNo + "','" + 0 + "' ,'" + DateTime.Now + "') RETURNING \"NlJrnlNo\" ;";
                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    float credit = pyPaid;
                    creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                         "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("PURCHASES").NlaccCode + "','0', '" + (decimal)credit + "', '" + (decimal)credit + "', 'payment by cheque','','false','','false','false','" + 0 + "');";

                    debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                        "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + (decimal)credit + "', '0', '" + (decimal)credit + "', 'amount settled','','false','','false','false','" + 0 + "');";
                    cmd.CommandText = creditentry1 + debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";
                }
            }
            cnn.Close();
            return response;
        }
        public List<AccountGroupReport> nljournalreport()
        {
            string query = "SELECT * FROM (SELECT d.\"NlAccName\" as account ,SUM(c.\"Dr\")::numeric::int  as dr ,SUM(c.\"Cr\")::numeric::int  as cr   " +
                " FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" c on c.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = c.\"NlAccCode\" " +
                "WHERE ( \"TranFrom\" = 'SL' OR  \"TranFrom\" = 'PL' OR \"TranFrom\" = 'SL-PY' OR \"TranFrom\" = 'PL-PY' OR \"TranFrom\" = 'PL-CRN' OR  \"TranFrom\" = 'SL-CRN' ) GROUP BY d.\"NlAccName\") AS LEDGER GROUP BY account,dr,cr";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query, cnn).ExecuteReader();
            List<AccountGroupReport> list = new List<AccountGroupReport>();
            while (sdr1.Read())
            {
                AccountGroupReport accountGroupReport = new AccountGroupReport();
                accountGroupReport.account = sdr1["account"] != DBNull.Value ? (string)sdr1["account"] : null;
                accountGroupReport.cr = sdr1["cr"] != DBNull.Value ? (int)sdr1["cr"] : 0;
                accountGroupReport.dr = sdr1["dr"] != DBNull.Value ? (int)sdr1["dr"] : 0;
                list.Add(accountGroupReport);
            }
            return list;
        }
        public List<NlJournalHeader> GettingNlJournal()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //geT nljournal
            List<NlJournalHeader> nljournals = new List<NlJournalHeader>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NlJournalHeader\"", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                NlJournalHeader nljournalHeader = new NlJournalHeader();
                nljournalHeader.NlJrnlNo = (int)sdr0["NlJrnlNo"];
                nljournalHeader.NlJrnlDesc = sdr0["NlJrnlDesc"] != DBNull.Value ? (string)sdr0["NlJrnlDesc"] : null;
                nljournalHeader.TranDate = (DateTime)sdr0["TranDate"];
                nljournalHeader.MEndDate = (DateTime)sdr0["MEndDate"];
                nljournalHeader.TranPeriod = sdr0["TranPeriod"] != DBNull.Value ? (decimal)sdr0["TranPeriod"] : 0;
                nljournalHeader.TranYear = sdr0["TranYear"] != DBNull.Value ? (decimal)sdr0["TranYear"] : 0;
                nljournalHeader.TranFrom = sdr0["TranFrom"] != DBNull.Value ? (string)sdr0["TranFrom"] : null;
                nljournalHeader.TranType = sdr0["TranType"] != DBNull.Value ? (string)sdr0["TranType"] : null;
                var sljrnlo = sdr0["SlJrnlNo"];
                nljournalHeader.SlJrnlNo = (long)sljrnlo;
                // nljournalHeader.SlJrnlNo = (long)sdr0["SlJrnlNo"] != DBNull.Value ? (string)sdr0["SlJrnlNo"] : null;
                nljournalHeader.ModuleId = sdr0["ModuleId"] != DBNull.Value ? (int?)sdr0["ModuleId"] : null;
                nljournals.Add(nljournalHeader);
            }
            cnn.Close();
            return nljournals;
        }
        public List<NLJournalDetails> GetNLJournalByJournalNo(int journalNo)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //geT nljournal
            List<NLJournalDetails> nljournaldetails = new List<NLJournalDetails>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"NLJournalDetails\" where \"NlJrnlNo\" = '" + journalNo + "'", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                NLJournalDetails nljournalHeader = new NLJournalDetails();
                nljournalHeader.SlJrnlNo = (int)sdr0["SlJrnlNo"];
                nljournalHeader.ModuleId = sdr0["ModuleId"] != DBNull.Value ? (int)sdr0["ModuleId"] : 0;
                nljournalHeader.ModifiedOn = (DateTime)sdr0["ModifiedOn"];
                nljournalHeader.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int?)sdr0["NlJrnlNo"] : null;
                nljournalHeader.JrnlSlNo = (int)sdr0["JrnlSlNo"];
                nljournalHeader.NlAccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : null;
                nljournalHeader.Dr = sdr0["Dr"] != DBNull.Value ? (Decimal)sdr0["Dr"] : 0;
                nljournalHeader.Cr = sdr0["Cr"] != DBNull.Value ? (Decimal)sdr0["Cr"] : 0;
                nljournalHeader.Amount = sdr0["Amount"] != DBNull.Value ? (Decimal)sdr0["Amount"] : 0;
                nljournalHeader.Narration = sdr0["Narration"] != DBNull.Value ? (string)sdr0["Narration"] : null;
                nljournalHeader.SLNarration = sdr0["SLNarration"] != DBNull.Value ? (string)sdr0["SLNarration"] : null;
                nljournalHeader.IsForex = (bool)sdr0["IsForex"];
                nljournalHeader.FolioNo = sdr0["FolioNo"] != DBNull.Value ? (string)sdr0["FolioNo"] : null;
                nljournalHeader.IsCleard = (bool)sdr0["IsCleard"];
                nljournalHeader.ClearDate = (DateTime)sdr0["ClearDate"];
                nljournalHeader.FCCleared = (bool)sdr0["FCCleared"];
                nljournalHeader.FCClearDate = (DateTime)sdr0["FCClearDate"];
                nljournalHeader.VatAmount = sdr0["VatAmount"] != DBNull.Value ? (Decimal)sdr0["VatAmount"] : 0;
                nljournaldetails.Add(nljournalHeader);
            }
            cnn.Close();
            return nljournaldetails;
        }
        public License GetCompanyDetail()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            License lic = new License();
            cnn.Open();
            string query = "Select * From \"Licence\"  ";
            NpgsqlDataReader reader = new NpgsqlCommand(@query, cnn).ExecuteReader();
            while (reader.Read())
            {
                lic.CompanyName = (string)reader["CompanyName"];
                lic.CompanySlogan = (string)reader["CompanySlogan"];
                lic.CompanyPostal = (string)reader["CompanyPostal"];
                lic.CompanyContact = (string)reader["CompanyContact"];
                lic.CompanyVAT = (string)reader["CompanyVAT"];
                lic.PhysicalAddress = (string)reader["PhysicalAddress"];
                lic.CompanyLogo = (string)reader["CompanyLogo"];
            }
            cnn.Close();
            return lic;
        }
        public List<NLAccount> GettingAllBankList()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<NLAccount> nljournals = new List<NLAccount>();
            //to change account_name per the db.
            string query = " SELECT a.\"NlAccCode\", a.\"NlAccName\", a.\"GroupCode\", a.\"CurCode\" FROM \"NLAccount\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"NlAccCode\" = a.\"NlAccCode\" " +
                "LEFT JOIN \"NlJournalHeader\" c on c.\"NlJrnlNo\" = b.\"JrnlSlNo\" WHERE a.\"NlAccName\" LIKE '%BANK%' GROUP BY a.\"NlAccCode\", a.\"NlAccName\", a.\"GroupCode\", a.\"CurCode\" ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                NLAccount nLAccount = new NLAccount();
                nLAccount.NLAccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                nLAccount.NLAccName = sdr0["NlAccName"] != DBNull.Value ? (string)sdr0["NlAccName"] : "";
                nLAccount.GroupCode = sdr0["GroupCode"] != DBNull.Value ? (string)sdr0["GroupCode"] : "";
                nLAccount.CurCode = sdr0["CurCode"] != DBNull.Value ? (string)sdr0["CurCode"] : "";
                nljournals.Add(nLAccount);
            }
            cnn.Close();
            return nljournals;
        }
        public List<BankReconcilationReport> GettingClearedReconciliation(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<BankReconcilationReport> bank = new List<BankReconcilationReport>();

            string query = "SELECT a.\"TranDate\", concat(a.\"TranPeriod\", '/', a.\"TranYear\") as period, b.\"Dr\", b.\"Cr\", b.\"Amount\", b.\"NlJrnlNo\", f.\"NlAccName\", " +
                "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN c.\"currentCustName\" WHEN a.\"TranFrom\" = 'PL-PY' THEN e.\"CustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" END AS accountDescription,  " +
                "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN c.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'PL-PY' THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"FolioNo\" END AS accountReference   " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"SLReceipts\" c on c.\"pyID\" = a.\"SlJrnlNo\" LEFT JOIN \"PLReceipts\" d on d.\"pyID\" = a.\"PlJrnlNo\" " +
                "LEFT JOIN \"PLCustomer\" e on e.\"CustID\" = d.supplier_id LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN cash_book_details g on g.journal_id = b.\"NlJrnlNo\" " +
                "LEFT JOIN cash_book_header h on h.\"id\" = g.cash_book_id WHERE a.\"TranFrom\" IN ('SL-PY', 'PL-PY', 'J-E') and b.\"NlAccCode\" = '"+code+"' and g.status = 'true';   ";

            NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (reader.Read())
            {
                BankReconcilationReport report = new BankReconcilationReport();
                report.pyDate = reader["TranDate"] != DBNull.Value ? (DateTime)reader["TranDate"] : DateTime.Now;
                report.CurrentCustomer = reader["accountDescription"] != DBNull.Value ? (string)reader["accountDescription"] : "";
                report.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                report.Cr = reader["Cr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                report.pyChequeNumber = reader["accountReference"] != DBNull.Value ? (string)reader["accountReference"] : "";
                report.Amount = reader["Amount"] != DBNull.Value ? (decimal)reader["Amount"] : 0;
                report.JournalId = reader["NlJrnlNo"] != DBNull.Value ? (int)(long)reader["NlJrnlNo"] : 0;
                report.Period = reader["period"] != DBNull.Value ? (string)reader["period"] : "";
                report.AccountName = reader["NlAccName"] != DBNull.Value ? (string)reader["NlAccName"] : "";
                bank.Add(report);
            }
            cnn.Close();
            return bank;
        }
        public List<BankReconcilationReport> GettingUnclearedReconciliationRecord(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<BankReconcilationReport> bankReconcilation = new List<BankReconcilationReport>();

            string query = "SELECT a.\"TranDate\", concat(a.\"TranPeriod\", '/', a.\"TranYear\") as period, b.\"Dr\", b.\"Cr\", b.\"Amount\", b.\"NlJrnlNo\", f.\"NlAccName\", f.\"CurCode\",     " +
                "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN c.\"currentCustName\" WHEN a.\"TranFrom\" = 'PL-PY' THEN e.\"CustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" END AS accountDescription,  " +
                "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN c.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'PL-PY' THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"FolioNo\" END AS accountReference  " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"SLReceipts\" c on c.\"pyID\" = a.\"SlJrnlNo\" LEFT JOIN \"PLReceipts\" d on d.\"pyID\" = a.\"PlJrnlNo\" " +
                "LEFT JOIN \"PLCustomer\" e on e.\"CustID\" = d.supplier_id LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN cash_book_details g on g.journal_id = b.\"NlJrnlNo\"  " +
                "LEFT JOIN cash_book_header h on h.\"id\" = g.cash_book_id  " +
                "WHERE a.\"TranFrom\" IN ('SL-PY', 'PL-PY', 'J-E') and b.\"NlAccCode\" = '"+code+"' AND NOT EXISTS (SELECT 1 FROM cash_book_details cbd WHERE cbd.journal_id = b.\"NlJrnlNo\" AND cbd.status = TRUE);  ";

            NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (reader.Read())
            {
                BankReconcilationReport bank = new BankReconcilationReport();
                bank.pyDate = reader["TranDate"] != DBNull.Value ? (DateTime)reader["TranDate"] : DateTime.Now;
                bank.CurrentCustomer = reader["accountDescription"] != DBNull.Value ? (string)reader["accountDescription"] : "";
                bank.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                bank.Cr = reader["Cr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                bank.pyChequeNumber = reader["accountReference"] != DBNull.Value ? (string)reader["accountReference"] : "";
                bank.Amount = reader["Amount"] != DBNull.Value ? (decimal)reader["Amount"] : 0;
                bank.JournalId = reader["NlJrnlNo"] != DBNull.Value ? (int)(long)reader["NlJrnlNo"] : 0;
                bank.Period = reader["period"] != DBNull.Value ? (string)reader["period"] : "";
                bank.AccountName = reader["NlAccName"] != DBNull.Value ? (string)reader["NlAccName"] : "";
                bank.CurrencyCode = reader["CurCode"] != DBNull.Value ? (string)reader["CurCode"] : "";
                bankReconcilation.Add(bank);
            }
            cnn.Close();
            return bankReconcilation;
        }
        public List<CashBookHeader> GetBankBalance(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<CashBookHeader> cashBooks = new List<CashBookHeader>();
            string query2 = "select a.\"id\", a.bank_balance, a.created_on from cash_book_header a LEFT JOIN cash_book_details b on b.cash_book_id = a.\"id\" LEFT JOIN \"NLJournalDetails\" c on c.\"NlJrnlNo\" = b.journal_id  " +
                "where c.\"NlAccCode\" = '"+code+"' GROUP BY a.\"id\" ORDER BY a.\"id\" DESC";
            NpgsqlDataReader sdr2 = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (sdr2.Read())
            {
                CashBookHeader balance = new CashBookHeader();
                balance.Id = sdr2["id"] != DBNull.Value ? (int)sdr2["id"] : 0;
                balance.bankBalance = sdr2["bank_balance"] != DBNull.Value ? (decimal)sdr2["bank_balance"] : 0;
                balance.CreatedOn = sdr2["created_on"] != DBNull.Value ? (DateTime)sdr2["created_on"] : DateTime.Now;
                cashBooks.Add(balance);
            }
            cnn.Close();
            return cashBooks;
        }
        public List<BankReconcilationReport> BankReconciliationReports(string code)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<BankReconcilationReport> bankReconcilations = new List<BankReconcilationReport>();

            string query = "SELECT a.\"TranDate\", concat(a.\"TranPeriod\", '/', a.\"TranYear\") as period, b.\"Dr\", b.\"Cr\", b.\"Amount\", b.\"NlJrnlNo\", " +
                "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN c.\"currentCustName\" WHEN a.\"TranFrom\" = 'PL-PY' THEN e.\"CustName\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"Narration\" END AS accountDescription, " +
                "CASE WHEN a.\"TranFrom\" = 'SL-PY' THEN c.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'PL-PY' THEN d.\"pyChequeNumber\" WHEN a.\"TranFrom\" = 'J-E' THEN b.\"FolioNo\" END AS accountReference " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"SLReceipts\" c on c.\"pyID\" = a.\"SlJrnlNo\" LEFT JOIN \"PLReceipts\" d on d.\"pyID\" = a.\"PlJrnlNo\" " +
                "LEFT JOIN \"PLCustomer\" e on e.\"CustID\" = d.supplier_id LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN cash_book_details g on g.journal_id = b.\"NlJrnlNo\" " +
                "LEFT JOIN cash_book_header h on h.\"id\" = g.cash_book_id " +
                "WHERE a.\"TranFrom\" IN ('SL-PY', 'PL-PY', 'J-E') and b.\"NlAccCode\" = '"+code+"' AND NOT EXISTS (SELECT 1 FROM cash_book_details cbd WHERE cbd.journal_id = b.\"NlJrnlNo\" AND cbd.status = TRUE); ";

            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                BankReconcilationReport nljournalHeader = new BankReconcilationReport();
                nljournalHeader.pyDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                nljournalHeader.CurrentCustomer = sdr0["accountDescription"] != DBNull.Value ? (string)sdr0["accountDescription"] : "";
                nljournalHeader.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                nljournalHeader.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                nljournalHeader.Amount = sdr0["Amount"] != DBNull.Value ? (decimal)sdr0["Amount"] : 0;
                nljournalHeader.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)(long)sdr0["NlJrnlNo"] : 0;
                nljournalHeader.pyChequeNumber = sdr0["accountReference"] != DBNull.Value ? (string)sdr0["accountReference"] : "";
                nljournalHeader.Period = sdr0["period"] != DBNull.Value ? (string)sdr0["period"] : "";
                bankReconcilations.Add(nljournalHeader);
            }
            cnn.Close();

            //balance brought forward
            List<BankReconcilationReport> reconcilationReports = new List<BankReconcilationReport>();
            cnn.Open();
            string query2 = "SELECT b.\"NlAccCode\", sum(b.\"Dr\":: numeric)-sum(b.\"Cr\"::numeric) as Account_balance, " +
                "sum(case when a.\"TranDate\" < e.\"created_on\" THEN b.\"Dr\" :: numeric else 0 end) - sum(case when a.\"TranDate\" < e.\"created_on\" then b.\"Cr\" :: numeric else 0 end) as balance_brought_forward   " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c on c.\"NlAccCode\" = b.\"NlAccCode\" " +
                "LEFT JOIN \"cash_book_details\" d on d.\"journal_id\" = b.\"NlJrnlNo\" LEFT JOIN \"cash_book_header\" e on e.\"id\" = d.\"cash_book_id\" " +
                " WHERE b.\"NlAccCode\" = '"+code+"' GROUP BY b.\"NlAccCode\"  ";
            NpgsqlDataReader reader = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (reader.Read())
            {
                BankReconcilationReport bankReconcilation = new BankReconcilationReport();
                bankReconcilation.NlAccCode = reader["NlAccCode"] != DBNull.Value ? (string)reader["NlAccCode"] : "NlAccCode";
                bankReconcilation.BalancebroughtForward = reader["balance_brought_forward"] != DBNull.Value ? (decimal)reader["balance_brought_forward"] : 0;
                bankReconcilation.BankDescriptions = "Balance Brought Forward";
                bankReconcilation.AccountBalance = reader["Account_balance"] != DBNull.Value ? (decimal)reader["Account_balance"] : 0;
                reconcilationReports.Add(bankReconcilation);
            }
            cnn.Close();

            bankReconcilations.AddRange(reconcilationReports);
            return bankReconcilations;
        }
        public List<BankReconcilationReport> GetCashBookBalancePerDate(string code, DateTime statementDate)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<BankReconcilationReport> reconcilationReports = new List<BankReconcilationReport>();
            cnn.Open();
            string query2 = "SELECT b.\"NlAccCode\",  "+
                "sum(case when a.\"TranDate\" <= '"+ statementDate.ToString("yyyy-MM-dd") +"' THEN b.\"Dr\" :: numeric else 0 end) - sum(case when a.\"TranDate\" <= '"+ statementDate.ToString("yyyy-MM-dd") + "' then b.\"Cr\" :: numeric else 0 end) as balance_brought_forward   "+
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c on c.\"NlAccCode\" = b.\"NlAccCode\"   "+
                "LEFT JOIN \"cash_book_details\" d on d.\"journal_id\" = b.\"NlJrnlNo\" LEFT JOIN \"cash_book_header\" e on e.\"id\" = d.\"cash_book_id\"  "+
                " WHERE b.\"NlAccCode\" = '" + code + "' GROUP BY b.\"NlAccCode\"  ";
            NpgsqlDataReader reader = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (reader.Read())
            {
                BankReconcilationReport bankReconcilation = new BankReconcilationReport();
                bankReconcilation.NlAccCode = reader["NlAccCode"] != DBNull.Value ? (string)reader["NlAccCode"] : "NlAccCode";
                bankReconcilation.BalancebroughtForward = reader["balance_brought_forward"] != DBNull.Value ? (decimal)reader["balance_brought_forward"] : 0;
                bankReconcilation.BankDescriptions = "Balance Brought Forward";
                //bankReconcilation.AccountBalance = reader["Account_balance"] != DBNull.Value ? (decimal)reader["Account_balance"] : 0;
                reconcilationReports.Add(bankReconcilation);
            }
            cnn.Close();
            return reconcilationReports;
        }
        public List<BankHistoryRecord> GetbankHistory(string code, DateTime from, DateTime to)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<BankHistoryRecord> records = new List<BankHistoryRecord>();
            string query = "SELECT e.\"currentCustName\", f.\"NlAccName\", e.\"pyDate\", c.\"NlAccCode\", a.\"bank_balance\", a.\"uncleared_balance\", a.\"ab_balance\", a.\"cashbook_balance\", a.\"cd_balance\", " +
                "a.\"created_on\", a.\"date_from\", a.\"date_to\", b.\"journal_id\", b.\"status\", c.\"Dr\", c.\"Cr\", c.\"Amount\", e.\"pyChequeNumber\" " +
                "FROM cash_book_header a LEFT JOIN cash_book_details b on b.cash_book_id = a.\"id\" LEFT JOIN \"NLJournalDetails\" c on c.\"NlJrnlNo\" = b.journal_id LEFT JOIN \"NlJournalHeader\" d on d.\"NlJrnlNo\" = c.\"JrnlSlNo\" " +
                "LEFT JOIN \"SLReceipts\" e on e.\"pyID\" = d.\"SlJrnlNo\" LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = c.\"NlAccCode\" " +
                "WHERE d.\"TranFrom\" = 'SL-PY' and c.\"NlAccCode\" = '" + code + "' and a.\"date_from\" = '" + from + "' and a.\"date_to\" = '" + to + "'; ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                BankHistoryRecord bankHistory = new BankHistoryRecord();
                bankHistory.created_on = (DateTime)sdr0["created_on"];
                bankHistory.bank_balance = sdr0["bank_balance"] != DBNull.Value ? (decimal)sdr0["bank_balance"] : 0;
                bankHistory.uncleared_balance = sdr0["uncleared_balance"] != DBNull.Value ? (decimal)sdr0["uncleared_balance"] : 0;
                bankHistory.ab_balance = sdr0["ab_balance"] != DBNull.Value ? (decimal)sdr0["ab_balance"] : 0;
                bankHistory.cash_book_balance = sdr0["cashbook_balance"] != DBNull.Value ? (decimal)sdr0["cashbook_balance"] : 0;
                bankHistory.cd_balance = sdr0["cd_balance"] != DBNull.Value ? (decimal)sdr0["cd_balance"] : 0;
                bankHistory.dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                bankHistory.cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                bankHistory.amount = sdr0["Amount"] != DBNull.Value ? (decimal)sdr0["Amount"] : 0;
                bankHistory.account_holder = sdr0["currentCustName"] != DBNull.Value ? (string)sdr0["currentCustName"] : "";
                bankHistory.account_name = sdr0["NlAccName"] != DBNull.Value ? (string)sdr0["NlAccName"] : "";
                bankHistory.account_code = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                bankHistory.pay_date = (DateTime)sdr0["pyDate"];
                bankHistory.date_from = (DateTime)sdr0["date_from"];
                bankHistory.date_to = (DateTime)sdr0["date_to"];
                bankHistory.status = (bool)sdr0["status"];
                bankHistory.journal_id = sdr0["journal_id"] != DBNull.Value ? (int)sdr0["journal_id"] : 0;
                bankHistory.pyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : "";
                records.Add(bankHistory);
            }
            cnn.Close();

            List<BankHistoryRecord> bankHistories = new List<BankHistoryRecord>();
            cnn.Open();
            string query1 = "SELECT g.\"CustName\", f.\"NlAccName\", e.\"pyDate\", c.\"NlAccCode\", a.\"bank_balance\", a.\"uncleared_balance\", a.\"ab_balance\", a.\"cashbook_balance\", a.\"cd_balance\", a.\"created_on\",  " +
                "a.\"date_from\", a.\"date_to\", b.\"journal_id\", b.\"status\", c.\"Dr\", c.\"Cr\", c.\"Amount\", e.\"pyChequeNumber\"  " +
                "FROM cash_book_header a LEFT JOIN cash_book_details b on b.cash_book_id = a.\"id\" LEFT JOIN \"NLJournalDetails\" c on c.\"NlJrnlNo\" = b.journal_id " +
                "LEFT JOIN \"NlJournalHeader\" d on d.\"NlJrnlNo\" = c.\"JrnlSlNo\" LEFT JOIN \"PLReceipts\" e on e.\"pyID\" = d.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = c.\"NlAccCode\" LEFT JOIN \"PLCustomer\" g on g.\"CustID\" = e.\"supplier_id\" " +
                "WHERE d.\"TranFrom\" = 'PL-PY' and c.\"NlAccCode\" = '" + code + "' and a.\"date_from\" = '" + from + "' and a.\"date_to\" = '" + to + "'; ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                BankHistoryRecord historyRecord = new BankHistoryRecord();
                historyRecord.created_on = (DateTime)sdr1["created_on"];
                historyRecord.bank_balance = sdr1["bank_balance"] != DBNull.Value ? (decimal)sdr1["bank_balance"] : 0;
                historyRecord.uncleared_balance = sdr1["uncleared_balance"] != DBNull.Value ? (decimal)sdr1["uncleared_balance"] : 0;
                historyRecord.ab_balance = sdr1["ab_balance"] != DBNull.Value ? (decimal)sdr1["ab_balance"] : 0;
                historyRecord.cash_book_balance = sdr1["cashbook_balance"] != DBNull.Value ? (decimal)sdr1["cashbook_balance"] : 0;
                historyRecord.cd_balance = sdr1["cd_balance"] != DBNull.Value ? (decimal)sdr1["cd_balance"] : 0;
                historyRecord.dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                historyRecord.cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                historyRecord.amount = sdr1["Amount"] != DBNull.Value ? (decimal)sdr1["Amount"] : 0;
                historyRecord.account_holder = sdr1["CustName"] != DBNull.Value ? (string)sdr1["CustName"] : "";
                historyRecord.account_name = sdr1["NlAccName"] != DBNull.Value ? (string)sdr1["NlAccName"] : "";
                historyRecord.account_code = sdr1["NlAccCode"] != DBNull.Value ? (string)sdr1["NlAccCode"] : "";
                historyRecord.pay_date = (DateTime)sdr1["pyDate"];
                historyRecord.date_from = (DateTime)sdr1["date_from"];
                historyRecord.date_to = (DateTime)sdr1["date_to"];
                historyRecord.status = (bool)sdr1["status"];
                historyRecord.journal_id = sdr1["journal_id"] != DBNull.Value ? (int)sdr1["journal_id"] : 0;
                historyRecord.pyChequeNumber = sdr1["pyChequeNumber"] != DBNull.Value ? (string)sdr1["pyChequeNumber"] : "";
                bankHistories.Add(historyRecord);
            }
            cnn.Close();
            records.AddRange(bankHistories);
            return records;
        }
        public List<BankHistoryRecord> GetBankHistoryByPeriod(string code, string periodFrom, string periodTo)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<BankHistoryRecord> records = new List<BankHistoryRecord>();
            string query = "SELECT e.\"currentCustName\", f.\"NlAccName\", e.\"pyDate\", c.\"NlAccCode\", a.\"bank_balance\", a.\"uncleared_balance\", a.\"ab_balance\", a.\"cashbook_balance\", a.\"cd_balance\", a.\"created_on\", " +
                "a.\"period_from\", a.\"period_to\", b.\"journal_id\", b.\"status\", c.\"Dr\", c.\"Cr\", c.\"Amount\", e.\"pyChequeNumber\" " +
                "FROM cash_book_header a LEFT JOIN cash_book_details b on b.cash_book_id = a.\"id\" LEFT JOIN \"NLJournalDetails\" c on c.\"NlJrnlNo\" = b.journal_id LEFT JOIN \"NlJournalHeader\" d on d.\"NlJrnlNo\" = c.\"JrnlSlNo\" LEFT JOIN \"SLReceipts\" e on e.\"pyID\" = d.\"SlJrnlNo\"   " +
                "LEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = c.\"NlAccCode\" WHERE d.\"TranFrom\" = 'SL-PY' and c.\"NlAccCode\" = '"+code+ "' and a.\"period_from\" = '" + periodFrom+ "' and a.\"period_to\" = '" + periodTo+"';   ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                BankHistoryRecord bankHistory = new BankHistoryRecord();
                bankHistory.created_on = (DateTime)sdr0["created_on"];
                bankHistory.bank_balance = sdr0["bank_balance"] != DBNull.Value ? (decimal)sdr0["bank_balance"] : 0;
                bankHistory.uncleared_balance = sdr0["uncleared_balance"] != DBNull.Value ? (decimal)sdr0["uncleared_balance"] : 0;
                bankHistory.ab_balance = sdr0["ab_balance"] != DBNull.Value ? (decimal)sdr0["ab_balance"] : 0;
                bankHistory.cash_book_balance = sdr0["cashbook_balance"] != DBNull.Value ? (decimal)sdr0["cashbook_balance"] : 0;
                bankHistory.cd_balance = sdr0["cd_balance"] != DBNull.Value ? (decimal)sdr0["cd_balance"] : 0;
                bankHistory.dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                bankHistory.cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                bankHistory.amount = sdr0["Amount"] != DBNull.Value ? (decimal)sdr0["Amount"] : 0;
                bankHistory.account_holder = sdr0["currentCustName"] != DBNull.Value ? (string)sdr0["currentCustName"] : "";
                bankHistory.account_name = sdr0["NlAccName"] != DBNull.Value ? (string)sdr0["NlAccName"] : "";
                bankHistory.account_code = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                bankHistory.period_from = sdr0["period_from"] != DBNull.Value ? (string)sdr0["period_from"] : "";
                bankHistory.period_to = sdr0["period_to"] != DBNull.Value ? (string)sdr0["period_to"] : "";
                bankHistory.pay_date = (DateTime)sdr0["pyDate"];
                bankHistory.status = (bool)sdr0["status"];
                bankHistory.journal_id = sdr0["journal_id"] != DBNull.Value ? (int)sdr0["journal_id"] : 0;
                bankHistory.pyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : "";
                records.Add(bankHistory);
            }
            cnn.Close();

            List<BankHistoryRecord> bankHistories = new List<BankHistoryRecord>();
            cnn.Open();
            string query1 = "SELECT g.\"CustName\", f.\"NlAccName\", e.\"pyDate\", c.\"NlAccCode\", a.\"bank_balance\", a.\"uncleared_balance\", a.\"ab_balance\", a.\"cashbook_balance\", a.\"cd_balance\", a.\"created_on\", a.\"period_from\", a.\"period_to\", b.\"journal_id\", b.\"status\", c.\"Dr\", c.\"Cr\", c.\"Amount\",    " +
                "e.\"pyChequeNumber\"   " +
                "FROM cash_book_header a LEFT JOIN cash_book_details b on b.cash_book_id = a.\"id\" LEFT JOIN \"NLJournalDetails\" c on c.\"NlJrnlNo\" = b.journal_id LEFT JOIN \"NlJournalHeader\" d on d.\"NlJrnlNo\" = c.\"JrnlSlNo\"   " +
                "LEFT JOIN \"PLReceipts\" e on e.\"pyID\" = d.\"PlJrnlNo\"\r\nLEFT JOIN \"NLAccount\" f on f.\"NlAccCode\" = c.\"NlAccCode\" LEFT JOIN \"PLCustomer\" g on g.\"CustID\" = e.\"supplier_id\"  " +
                "WHERE d.\"TranFrom\" = 'PL-PY' and c.\"NlAccCode\" = '"+code+ "' and a.\"period_from\" = '" + periodFrom+ "' and a.\"period_to\" = '" + periodTo+"';    ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                BankHistoryRecord historyRecord = new BankHistoryRecord();
                historyRecord.created_on = (DateTime)sdr1["created_on"];
                historyRecord.bank_balance = sdr1["bank_balance"] != DBNull.Value ? (decimal)sdr1["bank_balance"] : 0;
                historyRecord.uncleared_balance = sdr1["uncleared_balance"] != DBNull.Value ? (decimal)sdr1["uncleared_balance"] : 0;
                historyRecord.ab_balance = sdr1["ab_balance"] != DBNull.Value ? (decimal)sdr1["ab_balance"] : 0;
                historyRecord.cash_book_balance = sdr1["cashbook_balance"] != DBNull.Value ? (decimal)sdr1["cashbook_balance"] : 0;
                historyRecord.cd_balance = sdr1["cd_balance"] != DBNull.Value ? (decimal)sdr1["cd_balance"] : 0;
                historyRecord.dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                historyRecord.cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                historyRecord.amount = sdr1["Amount"] != DBNull.Value ? (decimal)sdr1["Amount"] : 0;
                historyRecord.account_holder = sdr1["CustName"] != DBNull.Value ? (string)sdr1["CustName"] : "";
                historyRecord.account_name = sdr1["NlAccName"] != DBNull.Value ? (string)sdr1["NlAccName"] : "";
                historyRecord.account_code = sdr1["NlAccCode"] != DBNull.Value ? (string)sdr1["NlAccCode"] : "";
                historyRecord.pay_date = (DateTime)sdr1["pyDate"];
                historyRecord.period_from = sdr1["period_from"] != DBNull.Value ? (string)sdr1["period_from"] : "";
                historyRecord.period_to = sdr1["period_to"] != DBNull.Value ? (string)sdr1["period_to"] : "";
                historyRecord.status = (bool)sdr1["status"];
                historyRecord.journal_id = sdr1["journal_id"] != DBNull.Value ? (int)sdr1["journal_id"] : 0;
                historyRecord.pyChequeNumber = sdr1["pyChequeNumber"] != DBNull.Value ? (string)sdr1["pyChequeNumber"] : "";
                bankHistories.Add(historyRecord);
            }
            cnn.Close();
            records.AddRange(bankHistories);
            return records;
        }
        public List<BankReconcilationReport> GettingBankDetailsPerDate(string code, DateTime from, DateTime to)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //geT nljournal
            //getting Cr and Dr for Sales.
            List<BankReconcilationReport> nljournals = new List<BankReconcilationReport>();
            //to change account_name per the db.
            string query = "select c.\"pyDate\", c.\"currentCustName\",b.\"NlJrnlNo\", b.\"Dr\", b.\"Cr\", b.\"NlAccCode\", b.\"Amount\", c.\"pyChequeNumber\"  " +
                "from \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"SLReceipts\" c on c.\"pyID\" = a.\"SlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\"    " +
                " where a.\"TranFrom\" = 'SL-PY' and b.\"NlAccCode\" = '" + code + "' and a.\"TranDate\" BETWEEN '" + from + "' and '" + to + "'   ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                BankReconcilationReport nljournalHeader = new BankReconcilationReport();
                nljournalHeader.pyDate = sdr0["pyDate"] != DBNull.Value ? (DateTime)sdr0["pyDate"] : DateTime.Now;
                nljournalHeader.NlAccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                nljournalHeader.CurrentCustomer = sdr0["currentCustName"] != DBNull.Value ? (string)sdr0["currentCustName"] : "";
                nljournalHeader.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                nljournalHeader.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                nljournalHeader.Amount = sdr0["Amount"] != DBNull.Value ? (decimal)sdr0["Amount"] : 0;
                nljournalHeader.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)(long)sdr0["NlJrnlNo"] : 0;
                nljournalHeader.pyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : "";
                nljournals.Add(nljournalHeader);
            }
            cnn.Close();

            //Getting Dr and Cr for Purchase.
            List<BankReconcilationReport> report = new List<BankReconcilationReport>();
            cnn.Open();
            string query1 = " select c.\"pyDate\", e.\"CustName\",b.\"NlJrnlNo\", b.\"Dr\", b.\"Cr\",b.\"NlAccCode\", b.\"Amount\", c.\"pyChequeNumber\"   " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"PLReceipts\" c on c.\"pyID\" = a.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"PLCustomer\" e on e.\"CustID\" = c.supplier_id   " +
                "WHERE a.\"TranFrom\" = 'PL-PY' and b.\"NlAccCode\" = '" + code + "' and a.\"TranDate\" BETWEEN '" + from + "' and '" + to + "'  ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                BankReconcilationReport headerResult = new BankReconcilationReport();
                headerResult.pyDate = sdr1["pyDate"] != DBNull.Value ? (DateTime)sdr1["pyDate"] : DateTime.Now;
                headerResult.NlAccCode = sdr1["NlAccCode"] != DBNull.Value ? (string)sdr1["NlAccCode"] : "";
                headerResult.CurrentCustomer = sdr1["CustName"] != DBNull.Value ? (string)sdr1["CustName"] : "";
                headerResult.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                headerResult.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                headerResult.Amount = sdr1["Amount"] != DBNull.Value ? (decimal)sdr1["Amount"] : 0;
                headerResult.JournalId = sdr1["NlJrnlNo"] != DBNull.Value ? (int)(long)sdr1["NlJrnlNo"] : 0;
                headerResult.pyChequeNumber = sdr1["pyChequeNumber"] != DBNull.Value ? (string)sdr1["pyChequeNumber"] : "";
                report.Add(headerResult);
            }
            cnn.Close();

            //getting the Balance brought Forward.
            List<BankReconcilationReport> bankAccount = new List<BankReconcilationReport>();
            cnn.Open();
            string query2 = "SELECT c.\"NlAccCode\", SUM(CASE WHEN a.\"TranDate\" < '" + from + "' THEN b.\"Dr\":: numeric ELSE 0 END) - SUM(CASE WHEN a.\"TranDate\" < '" + from + "' THEN b.\"Cr\"::numeric ELSE 0 END) AS balance_brought_forward  " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" WHERE b.\"NlAccCode\" = '" + code + "' GROUP BY c.\"NlAccCode\"  ";
            NpgsqlDataReader sdr2 = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (sdr2.Read())
            {
                BankReconcilationReport balance = new BankReconcilationReport();
                balance.NlAccCode = sdr2["NlAccCode"] != DBNull.Value ? (string)sdr2["NlAccCode"] : "";
                balance.BankDescriptions = "Balance Brought Forward";
                balance.BalancebroughtForward = sdr2["balance_brought_forward"] != DBNull.Value ? (decimal)sdr2["balance_brought_forward"] : 0;
                bankAccount.Add(balance);
            }
            cnn.Close();

            nljournals.AddRange(report);
            nljournals.AddRange(bankAccount);
            return nljournals;
        }
        public List<BankReconcilationReport> GettingBankDetailsPerPeriod(string code, string periodFrom, string periodTo)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            //geT nljournal
            List<BankReconcilationReport> nljournals = new List<BankReconcilationReport>();
            //to change account_name per the db.
            //sales Cr and Dr.
            string query = "select c.\"pyDate\", c.\"currentCustName\", b.\"NlJrnlNo\", b.\"Dr\", b.\"Cr\", b.\"NlAccCode\", b.\"Amount\", c.\"pyChequeNumber\"   " +
                "from \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"SLReceipts\" c on c.\"pyID\" = a.\"SlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\"   " +
                "where a.\"TranFrom\" = 'SL-PY' and b.\"NlAccCode\" = '" + code + "' and a.\"TranPeriod\" BETWEEN " + periodFrom.Split("/")[0] + " and " + periodTo.Split("/")[0] + " and a.\"TranYear\" BETWEEN " + periodFrom.Split("/")[1] + " and " + periodTo.Split("/")[1] + "   ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                BankReconcilationReport nljournalHeader = new BankReconcilationReport();
                nljournalHeader.pyDate = sdr0["pyDate"] != DBNull.Value ? (DateTime)sdr0["pyDate"] : DateTime.Now;
                nljournalHeader.CurrentCustomer = sdr0["currentCustName"] != DBNull.Value ? (string)sdr0["currentCustName"] : "";
                nljournalHeader.NlAccCode = sdr0["NlAccCode"] != DBNull.Value ? (string)sdr0["NlAccCode"] : "";
                nljournalHeader.Dr = sdr0["Dr"] != DBNull.Value ? (decimal)sdr0["Dr"] : 0;
                nljournalHeader.Cr = sdr0["Cr"] != DBNull.Value ? (decimal)sdr0["Cr"] : 0;
                nljournalHeader.Amount = sdr0["Amount"] != DBNull.Value ? (decimal)sdr0["Amount"] : 0;
                nljournalHeader.JournalId = sdr0["NlJrnlNo"] != DBNull.Value ? (int)(long)sdr0["NlJrnlNo"] : 0;
                nljournalHeader.pyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : "";
                nljournals.Add(nljournalHeader);
            }
            cnn.Close();
            //purchase Cr and Dr.
            List<BankReconcilationReport> bankReconcilationReports = new List<BankReconcilationReport>();
            cnn.Open();
            string query1 = "select c.\"pyDate\", e.\"CustName\",b.\"NlJrnlNo\", b.\"Dr\", b.\"Cr\", b.\"NlAccCode\", b.\"Amount\", c.\"pyChequeNumber\"   " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b on b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"PLReceipts\" c on c.\"pyID\" = a.\"PlJrnlNo\" LEFT JOIN \"NLAccount\" d on d.\"NlAccCode\" = b.\"NlAccCode\" LEFT JOIN \"PLCustomer\" e on e.\"CustID\" = c.supplier_id   " +
                "WHERE a.\"TranFrom\" = 'PL-PY' and b.\"NlAccCode\" = '" + code + "' and a.\"TranPeriod\" BETWEEN " + periodFrom.Split("/")[0] + " and " + periodTo.Split("/")[0] + " and  a.\"TranYear\" BETWEEN " + periodFrom.Split("/")[1] + " and " + periodTo.Split("/")[1] + "    ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                BankReconcilationReport bankReport = new BankReconcilationReport();
                bankReport.pyDate = sdr1["pyDate"] != DBNull.Value ? (DateTime)sdr1["pyDate"] : DateTime.Now;
                bankReport.CurrentCustomer = sdr1["CustName"] != DBNull.Value ? (string)sdr1["CustName"] : "";
                bankReport.NlAccCode = sdr1["NlAccCode"] != DBNull.Value ? (string)sdr1["NlAccCode"] : "";
                bankReport.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                bankReport.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                bankReport.Amount = sdr1["Amount"] != DBNull.Value ? (decimal)sdr1["Amount"] : 0;
                bankReport.JournalId = sdr1["NlJrnlNo"] != DBNull.Value ? (int)(long)sdr1["NlJrnlNo"] : 0;
                bankReport.pyChequeNumber = sdr1["pyChequeNumber"] != DBNull.Value ? (string)sdr1["pyChequeNumber"] : "";
                bankReconcilationReports.Add(bankReport);
            }
            cnn.Close();

            //balance brought forward.
            List<BankReconcilationReport> account_balance = new List<BankReconcilationReport>();
            cnn.Open();
            string[] perioding_From = periodFrom.Split("/");
            string periodMonth = perioding_From[0];
            string periodYear = perioding_From[1];
            string query2 = "SELECT c.\"NlAccCode\", SUM(CASE WHEN (a.\"TranYear\" < '" + periodYear + "' OR (a.\"TranYear\" = '" + periodYear + "' AND a.\"TranPeriod\" < '" + periodMonth + "')) THEN b.\"Dr\":: numeric ELSE 0 END) - " +
                "SUM(CASE WHEN (a.\"TranYear\" < '" + periodYear + "' OR (a.\"TranYear\" = '" + periodYear + "' AND a.\"TranPeriod\" < '" + periodMonth + "')) THEN b.\"Cr\":: numeric ELSE 0 END) AS balance_brought_forward  " +
                "FROM \"NlJournalHeader\" a LEFT JOIN \"NLJournalDetails\" b ON b.\"JrnlSlNo\" = a.\"NlJrnlNo\" LEFT JOIN \"NLAccount\" c ON c.\"NlAccCode\" = b.\"NlAccCode\" WHERE b.\"NlAccCode\" = '" + code + "' GROUP BY c.\"NlAccCode\" ";
            NpgsqlDataReader sdr2 = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (sdr2.Read())
            {
                BankReconcilationReport balance = new BankReconcilationReport();
                balance.NlAccCode = sdr2["NlAccCode"] != DBNull.Value ? (string)sdr2["NlAccCode"] : "";
                balance.BankDescriptions = "Balance Brought Forward";
                balance.BalancebroughtForward = sdr2["balance_brought_forward"] != DBNull.Value ? (decimal)sdr2["balance_brought_forward"] : 0;
                account_balance.Add(balance);
            }
            cnn.Close();

            nljournals.AddRange(bankReconcilationReports);
            nljournals.AddRange(account_balance);
            return nljournals;
        }
        public MyResponse BankStatementAdding(List<CashBookDetails> cashBook, int userId, DateTime createdon, decimal bankBalance, decimal unclearedbalance, decimal abbalance, decimal cashbookbalance, decimal cdbalance, string periodFrom, string periodTo, DateTime dateFrom, DateTime dateTo)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            StringBuilder bankDetails = new StringBuilder();
            int id = 0;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    string query = null;
                    if(periodFrom != null && periodTo != null)
                    {
                        query = "INSERT INTO \"cash_book_header\"(\"created_on\", \"created_by\", \"bank_balance\", \"uncleared_balance\", \"ab_balance\", \"cashbook_balance\", \"cd_balance\", \"period_from\", \"period_to\" ) " +
                        "VALUES('" + createdon + "','" + userId + "', '" + bankBalance + "', '" + unclearedbalance + "', '" + abbalance + "', '" + cashbookbalance + "', '" + cdbalance + "', '"+periodFrom+"','"+periodTo+"') returning \"id\"; ";
                    }
                    else
                    {
                        query = "INSERT INTO \"cash_book_header\"(\"created_on\", \"created_by\", \"bank_balance\", \"uncleared_balance\", \"ab_balance\", \"cashbook_balance\", \"cd_balance\", \"date_from\", \"date_to\") " +
                        "VALUES('" + createdon + "','" + userId + "', '" + bankBalance + "', '" + unclearedbalance + "', '" + abbalance + "', '" + cashbookbalance + "', '" + cdbalance + "', '"+dateFrom+"','"+dateTo+"') returning \"id\"; ";
                    }
                    var cmd = new NpgsqlCommand(query, cnn, trans);
                    //cashbook id 
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    foreach (var bank in cashBook)
                    {
                        if(bank.Status == true)
                        {
                            bankDetails.Append("INSERT INTO \"cash_book_details\"(\"cash_book_id\", \"journal_id\", \"status\") VALUES('" + id + "', '" + bank.JournalId + "', '" + true + "'); ");
                        }
                    }
                    cmd.CommandText = bankDetails.ToString();
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "Successfully Posted";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occurred while trying to save details.";
                }
                
            }
            cnn.Close();
            return response;
        }
        public MyResponse customJournalEntry(List<JournalInputRequest> journalInputRequests, DateTime transactionDate, int period_month, int period_year)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();

            StringBuilder debitentry = new StringBuilder();
            StringBuilder creditentry = new StringBuilder();

            int id = 0;
            using (var trans = cnn.BeginTransaction())
            {
                decimal creditors = 0;
                try
                {
                    //J-E STRANDS FOR JOURNAL ENTRY
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\",\"CreatedOn\",\"PeriodYear\") " +
                    "VALUES('','" + transactionDate + "','" + DateTime.Now + "', '" + period_month + "', '" + period_year + "','J-E','NT'," + 0 + ",'" + 0 + "','" + 0 + "','" + DateTime.Now + "','" + period_year + "' ) RETURNING \"NlJrnlNo\" ;";
                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    foreach (var data in journalInputRequests)
                    {
                        if (data.action == "Dr")
                        {
                            debitentry.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                            "VALUES('" + id + "','" + data.accountcode + "','" + (decimal)data.amount + "', '0', '" + (decimal)data.amount + "', '" + data.description + "','"+data.description+"','false', '"+data.FolioNo+"', 'false','false','" + 0 + "');");
                        }
                        else
                        {
                            creditentry.Append("INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\") " +
                             " VALUES('" + id + "','" + data.accountcode + "','0', '" + (decimal)data.amount + "', '" + (decimal)data.amount + "', '" + data.description + "','"+data.description+"','false', '"+data.FolioNo+"', 'false','false','" + 0 + "'); ");
                        }
                    }
                    cmd.CommandText = creditentry.ToString() + debitentry.ToString();
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "Successfully Created Journal.";
                }
                catch (Exception E)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An Occurred while trying to save details.";
                }
            }
            cnn.Close();
            return response;
        }
        public MyResponse savePurchaseReceiptsAccount(NlJournalHeader recvData, string transtype, float pyPaid, PLInvoice plInv)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            string debitentry1;
            string creditentry1;
            cnn.Open();
            int id;
            using (var trans = cnn.BeginTransaction())
            {
                decimal creditors = 0;
                try
                {
                    //var journalSavePoint = cnn.SavePoint("");
                    long insertedId = 0L;
                    string insertQuery1 = "INSERT INTO \"NlJournalHeader\" (\"NlJrnlDesc\",\"TranDate\",\"MEndDate\",\"TranPeriod\",\"TranYear\",\"TranFrom\",\"TranType\",\"SlJrnlNo\",\"PlJrnlNo\",\"ModuleId\") " +
                        "VALUES('" + recvData.NlJrnlDesc + "','" + recvData.TranDate + "','" + recvData.MEndDate + "', '" + recvData.TranPeriod + "', '" + recvData.TranYear + "','PL-PY','NT'," + 0 + ",'" + recvData.PlJrnlNo + "','" + 0 + "' ) RETURNING \"NlJrnlNo\" ;  ";

                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    float credit = pyPaid;
                    /// credit asset(cash or bank ) since its reducing

                    if (transtype == "cash")
                    {
                        creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")  " +
                         "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("CASH").NlaccCode + "','0', '" + (decimal)credit + "', '" + (decimal)credit + "', 'payment by cash','','false','','false','false','" + 0 + "');  ";
                    }
                    else
                    {
                        creditentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")    " +
                         "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("BANK").NlaccCode + "','0', '" + (decimal)credit + "', '" + (decimal)credit + "', 'payment by cheque','','false','','false','false','" + 0 + "');  ";
                    }

                    //debit creditors since its reducing
                    debitentry1 = "INSERT INTO \"NLJournalDetails\" (\"JrnlSlNo\",\"NlAccCode\",\"Dr\",\"Cr\",\"Amount\", \"Narration\",\"SLNarration\",\"IsForex\",\"FolioNo\",\"IsCleard\",\"FCCleared\",\"VatAmount\")    " +
                        "VALUES('" + id + "','" + this.GetNLAccountAccountCodeUsingName("CREDITORS").NlaccCode + "','" + (decimal)credit + "', '0', '" + (decimal)credit + "', 'amount settled','','false','','false','false','" + 0 + "');  ";

                    decimal balance = plInv.Balance - (decimal)credit;
                    string plheader_upd = "UPDATE \"PLInvoiceHeader\" SET \"Balance\" =  " + balance + " WHERE \"PLJrnlNo\" = " + recvData.PlJrnlNo + "   ";

                    cmd.CommandText = creditentry1 + debitentry1 + plheader_upd;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";
                }
            }
            cnn.Close();
            return response;
        }
        public List<TrialBalanceReport> GetTrialBalanceReports()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //Get all customers
            string myQuery = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\"\r\n,\"NLAccount\".\"GroupCode\",\r\n\tCASE\r\n\t\t\tWHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') " +
                "THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)\r\n\t\t\tWHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') " +
                "THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric)\r\n\t\t\t else \r\n     " +
                "   0\r\n\t\t  end as run_status,\r\n\t\t\r\nsum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"\r\nFROM \"NLAccountGroup\" " +
                " \r\nLeft JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\"\r\nLEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"\r\n" +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"\r\nwhere \"NLAccount\" IS NOT NULL \r\n" +
                "AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now())    GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"\r\n";
            //QUERY 2 IS USED SINCE ITS NOT AFFECTED BY THE FINACIAL  YEAR 
            string myQuery3 = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\"\r\n,\"NLAccount\".\"GroupCode\",\r\n\tCASE\r\n\t\t\tWHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') " +
             "THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)\r\n\t\t\tWHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') " +
             "THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric)\r\n\t\t\t else \r\n     " +
             "   0\r\n\t\t  end as run_status,\r\n\t\t\r\nsum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"\r\nFROM \"NLAccountGroup\" " +
             " \r\nLeft JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\"\r\nLEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"\r\n" +
             "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"\r\nwhere \"NLAccount\" IS NOT NULL \r\n" +
             "   GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"\r\n";
            //GETTING CR AND DR FOR A AND E ACCOUNTS
            string queryDr = "select b.\"NlAccName\", b.\"GroupCode\", a.\"GroupType\", sum(c.\"Dr\") as Dr, sum(c.\"Cr\") as Cr, sum(c.\"Dr\"::numeric) - sum(c.\"Cr\"::numeric) as run_status     " +
                "from \"NLAccountGroup\" a left join \"NLAccount\" b on b.\"GroupCode\" = a.\"GroupCode\" left join \"NLJournalDetails\" c on c.\"NlAccCode\" = b.\"NlAccCode\" left join \"NlJournalHeader\" d on d.\"NlJrnlNo\" = c.\"JrnlSlNo\"   " +
                "where a.\"GroupType\" in ('A','E') and d.\"TranFrom\" in ('J-E', 'PL', 'SL', 'SL-PY','PL-PY','SL-CRN', 'PL-CRN', 'SL-REVERSAL', 'PL-REVERSAL', 'SL-PY-REVERSAL', 'PL-PY-REVERSAL') and b is not null group by b.\"NlAccName\", b.\"GroupCode\", a.\"GroupType\";   ";
            //GETTING CR AND DR OF I AND L ACCOUNTS
            string queryCr = "select b.\"NlAccName\", b.\"GroupCode\", a.\"GroupType\", sum(c.\"Cr\"::numeric) as Cr, sum(c.\"Dr\"::numeric) as Dr, sum(c.\"Cr\"::numeric) - sum(c.\"Dr\"::numeric) as run_status    " +
                "from \"NLAccountGroup\" a left join \"NLAccount\" b on b.\"GroupCode\" = a.\"GroupCode\" left join \"NLJournalDetails\" c on c.\"NlAccCode\" = b.\"NlAccCode\" left join \"NlJournalHeader\" d on d.\"NlJrnlNo\" = c.\"JrnlSlNo\"    " +
                "where a.\"GroupType\" in ('I','L') and d.\"TranFrom\" in ('J-E', 'PL', 'SL', 'SL-PY','PL-PY','SL-CRN', 'PL-CRN', 'SL-REVERSAL', 'PL-REVERSAL', 'SL-PY-REVERSAL', 'PL-PY-REVERSAL') and b is not null group by b.\"NlAccName\", b.\"GroupCode\", a.\"GroupType\";    ";

            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(queryDr, cnn).ExecuteReader();
            List<TrialBalanceReport> list = new List<TrialBalanceReport>();

            while (sdr1.Read())
            {
                TrialBalanceReport trialBalanceReport = new TrialBalanceReport();
                trialBalanceReport.NlAccName = sdr1["NlAccName"] != DBNull.Value ? (string)sdr1["NlAccName"] : "";
                trialBalanceReport.GroupType = sdr1["GroupType"] != DBNull.Value ? (string)sdr1["GroupType"] : "";
                trialBalanceReport.RunStatus = sdr1["run_status"] != DBNull.Value ? (decimal)sdr1["run_status"] : 0;
                trialBalanceReport.Cr = sdr1["Cr"] != DBNull.Value ? (decimal)sdr1["Cr"] : 0;
                trialBalanceReport.Dr = sdr1["Dr"] != DBNull.Value ? (decimal)sdr1["Dr"] : 0;
                list.Add(trialBalanceReport);
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(queryCr, cnn).ExecuteReader();
            while(reader.Read())
            {
                TrialBalanceReport BalanceReport = new TrialBalanceReport();
                BalanceReport.NlAccName = reader["NlAccName"] != DBNull.Value ? (string)reader["NlAccName"] : "";
                BalanceReport.GroupType = reader["GroupType"] != DBNull.Value ? (string)reader["GroupType"] : "";
                BalanceReport.RunStatus = reader["run_status"] != DBNull.Value ? (decimal)reader["run_status"] : 0;
                BalanceReport.Cr = reader["Cr"] != DBNull.Value ? (decimal)reader["Cr"] : 0;
                BalanceReport.Dr = reader["Dr"] != DBNull.Value ? (decimal)reader["Dr"] : 0;
                list.Add(BalanceReport);
            }
            cnn.Close();
            string myQuery2 = "SELECT  \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",CASE WHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)  " +
                "WHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric) else 0 end as run_status   " +
                "FROM \"NLAccountGroup\" Left JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\" LEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"   " +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"\r\nwhere \"NLAccount\" IS NOT NULL AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now() - interval '1 year')    " +
                "GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"  ";
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand(myQuery2, cnn).ExecuteReader();
            List<OpeningTrialBalanceData> list2 = new List<OpeningTrialBalanceData>();

            while (sdr2.Read())
            {
                OpeningTrialBalanceData trialBalanceReport = new OpeningTrialBalanceData();
                trialBalanceReport.NlAccName = sdr2["NlAccName"] != DBNull.Value ? (string)sdr2["NlAccName"] : "";
                trialBalanceReport.RunStatus = sdr2["run_status"] != DBNull.Value ? (decimal)sdr2["run_status"] : 0;
                trialBalanceReport.GroupType = sdr2["GroupType"] != DBNull.Value ? (string)sdr2["GroupType"] : "";
                list2.Add(trialBalanceReport);
            }
            cnn.Close();

            foreach (var trial in list2)
            {
                //["BANK","CASH","PURCHASE","CREDITORS","SALES","VAT"] -- LIST2
                // ENTITY IS TRIAL
                int count = list.Where(x => x.NlAccName.Equals(trial.NlAccName)).Count();
                // IF WE HAVE A MATCH
                if (count > 0)
                {
                    //WE  GIVE THAT MATCH AN OPENING BALANCE
                    var trial_balance_entity = list.Where(x => x.NlAccName.Equals(trial.NlAccName)).FirstOrDefault();
                    trial_balance_entity.OpeningBalance = trial.RunStatus;
                }
                else
                {
                    /// create new entity i
                    TrialBalanceReport trialBalanceReport2 = new TrialBalanceReport();
                    trialBalanceReport2.NlAccName = trial.NlAccName;
                    trialBalanceReport2.Cr = 0;
                    trialBalanceReport2.Dr = 0;
                    trialBalanceReport2.OpeningBalance = trial.RunStatus;
                    trialBalanceReport2.GroupType = trial.GroupType;
                    list.Add(trialBalanceReport2);
                }
            }
            //foreach (var trial in list)
            //{
            //    int count = list2.Where(x => x.NlAccName.Equals(trial.NlAccName)).Count();
            //    if (count >= 1)
            //    {
            //        var openingbalance = list2.Where(x => x.NlAccName.Equals(trial.NlAccName)).FirstOrDefault().RunStatus;
            //        trial.OpeningBalance = openingbalance;
            //    }
            //    else
            //    {
            //        trial.OpeningBalance = 0;
            //    }
            //}

            return list;
        }
        public BalanceSheet GetBalanceSheetReport()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            
            string current_assests_query = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\", CASE WHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)  " +
                "WHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric) else 0  end as run_status, sum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"    " +
                "FROM \"NLAccountGroup\" Left JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\" LEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"    " +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"    " +
                "where \"NLAccount\" IS NOT NULL AND \"NLAccount\".\"NlAccName\" IN('BANK','CASH') AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now()) " +
                "GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"   ";
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(current_assests_query, cnn).ExecuteReader();
            BalanceSheet balanceSheet = new BalanceSheet();
            List<BalanceSheetDetails> list = new List<BalanceSheetDetails>();
            //------------------------------------------------------------------------current_assests_query ------------------------------------------------------------------------------------//
            while (sdr1.Read())
            {
                BalanceSheetDetails currentAssets = new BalanceSheetDetails();
                currentAssets.AccountName = sdr1["NlAccName"] != DBNull.Value ? (string)sdr1["NlAccName"] : "";
                currentAssets.Ammount = sdr1["run_status"] != DBNull.Value ? (decimal)sdr1["run_status"] : 0;
                list.Add(currentAssets);
            }
            cnn.Close();
            //------------------------------------------------------------------------current_assests_query ---------------------------------------------------------------------------------------//
            string non_current_assests_query = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\", CASE WHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)    " +
                "WHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric)  else 0 end as run_status, sum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"    " +
                "FROM \"NLAccountGroup\" Left JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\" LEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"        " +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"    " +
                "where \"NLAccount\" IS NOT NULL AND \"NLAccount\".\"NlAccName\" IN('DEBTORS','PURCHASES') AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now())    " +
                "GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"    ";
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand(non_current_assests_query, cnn).ExecuteReader();
            List<BalanceSheetDetails> noncurrentlist = new List<BalanceSheetDetails>();
            //------------------------------------------------------------------------non_current_assests_query ------------------------------------------------------------------------------------//
            while (sdr2.Read())
            {
                BalanceSheetDetails noncurrentAssets = new BalanceSheetDetails();
                noncurrentAssets.AccountName = sdr2["NlAccName"] != DBNull.Value ? (string)sdr2["NlAccName"] : "";
                noncurrentAssets.Ammount = sdr2["run_status"] != DBNull.Value ? (decimal)sdr2["run_status"] : 0;
                noncurrentlist.Add(noncurrentAssets);
            }
            cnn.Close();
            //------------------------------------------------------------------------non_current_assests_query ---------------------------------------------------------------------------------------//
            cnn.Open();
            string current_liability_query = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\", CASE WHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)   " +
                "WHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric) else 0 end as run_status, sum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"    " +
                "FROM \"NLAccountGroup\" Left JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\" LEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"     " +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\" where \"NLAccount\" IS NOT NULL AND \"NLAccount\".\"NlAccName\" IN('CREDITORS') AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now())    " +
                "GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"";
            NpgsqlDataReader sdr3 = new NpgsqlCommand(current_liability_query, cnn).ExecuteReader();
            List<BalanceSheetDetails> currentliabilitylist = new List<BalanceSheetDetails>();
            //------------------------------------------------------------------------current_liability_query ------------------------------------------------------------------------------------//
            while (sdr3.Read())
            {
                BalanceSheetDetails currentLiability = new BalanceSheetDetails();
                currentLiability.AccountName = sdr3["NlAccName"] != DBNull.Value ? (string)sdr3["NlAccName"] : "";
                currentLiability.Ammount = sdr3["run_status"] != DBNull.Value ? (decimal)sdr3["run_status"] : 0;
                currentliabilitylist.Add(currentLiability);
            }
            cnn.Close();
            //------------------------------------------------------------------------current_liability_query ---------------------------------------------------------------------------------------//
            cnn.Open();
            string noncurrent_liability_query = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\", CASE WHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)     " +
                "WHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric) else 0 end as run_status, sum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"     " +
                "FROM \"NLAccountGroup\"  Left JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\" LEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"    " +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"  where \"NLAccount\" IS NOT NULL AND \"NLAccount\".\"NlAccName\" IN('VAT') AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now())    " +
                "GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"  ";
            NpgsqlDataReader sdr4 = new NpgsqlCommand(noncurrent_liability_query, cnn).ExecuteReader();
            List<BalanceSheetDetails> noncurrentliabilitylist = new List<BalanceSheetDetails>();
            //------------------------------------------------------------------------current_liability_query ------------------------------------------------------------------------------------//
            while (sdr4.Read())
            {
                BalanceSheetDetails noncurrentLiability = new BalanceSheetDetails();
                noncurrentLiability.AccountName = sdr4["NlAccName"] != DBNull.Value ? (string)sdr4["NlAccName"] : "";
                noncurrentLiability.Ammount = sdr4["run_status"] != DBNull.Value ? (decimal)sdr4["run_status"] : 0;
                noncurrentliabilitylist.Add(noncurrentLiability);
            }
            cnn.Close();
            //------------------------------------------------------------------------current_liability_query ---------------------------------------------------------------------------------------//
            cnn.Open();
            string equity_query = "SELECT \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\", CASE WHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)     " +
                "WHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric) else 0 end as run_status, sum(\"NLJournalDetails\".\"Cr\")as \"Cr\",sum(\"NLJournalDetails\".\"Dr\")as \"Dr\"      " +
                "FROM \"NLAccountGroup\" Left JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\" LEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"       " +
                "LEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\" where \"NLAccount\" IS NOT NULL AND \"NLAccount\".\"NlAccName\" IN('SALES') AND \"NlJournalHeader\".\"TranYear\" = date_part('year', now())     " +
                "GROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"";
            NpgsqlDataReader sdr5 = new NpgsqlCommand(equity_query, cnn).ExecuteReader();
            List<BalanceSheetDetails> equitylist = new List<BalanceSheetDetails>();
            //------------------------------------------------------------------------current_liability_query ------------------------------------------------------------------------------------//
            while (sdr5.Read())
            {
                BalanceSheetDetails equity = new BalanceSheetDetails();
                equity.AccountName = sdr5["NlAccName"] != DBNull.Value ? (string)sdr5["NlAccName"] : "";
                equity.Ammount = sdr5["run_status"] != DBNull.Value ? (decimal)sdr5["run_status"] : 0;
                equitylist.Add(equity);
            }
            cnn.Close();
            //------------------------------------------------------------------------current_liability_query ---------------------------------------------------------------------------------------//
            balanceSheet.CurrentAssets = list;
            balanceSheet.Equity = equitylist;
            balanceSheet.NoncurrentAssets = noncurrentlist;
            balanceSheet.CurrentLiabilities = currentliabilitylist;
            balanceSheet.NoncurrentLiabilities = noncurrentliabilitylist;
            return balanceSheet;
        }
        //public List<TrialBalanceReport> getOpeningBalance()
        //{
        //    NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
        //    string myQuery2 = "SELECT  \"NLAccount\".\"NlAccName\",CASE\r\n\t\t\tWHEN \"NLAccountGroup\".\"GroupType\" IN('A','E') THEN sum(\"NLJournalDetails\".\"Dr\"::numeric) -sum(\"NLJournalDetails\".\"Cr\" ::numeric)\r\n\t\t\tWHEN \"NLAccountGroup\".\"GroupType\" IN('I','L') THEN sum(\"NLJournalDetails\".\"Cr\" ::numeric) - sum(\"NLJournalDetails\".\"Dr\" ::numeric)\r\n\t\t\t else \r\n        0\r\n\t\t  end as run_status FROM \"NLAccountGroup\"  \r\nLeft JOIN \"NLAccount\" on \"NLAccount\".\"GroupCode\" = \"NLAccountGroup\".\"GroupCode\"\r\nLEFT JOIN \"NLJournalDetails\" on \"NLJournalDetails\".\"NlAccCode\" = \"NLAccount\".\"NlAccCode\"\r\nLEFT JOIN \"NlJournalHeader\" on \"NlJournalHeader\".\"NlJrnlNo\" = \"NLJournalDetails\".\"JrnlSlNo\"\r\nwhere \"NLAccount\" IS NOT NULL AND \"NLAccount\".\"NlAccName\"  NOT IN ('DEBTORS','CREDITORS')\r\nAND \"NlJournalHeader\".\"TranYear\" = date_part('year', now() - interval '1 year')\r\nGROUP BY \"NLAccount\".\"NlAccName\",\"NLAccountGroup\".\"GroupType\",\"NLAccount\".\"GroupCode\"";

        //    NpgsqlDataReader sdr2 = new NpgsqlCommand(myQuery2, cnn).ExecuteReader();
        //    List<OpeningTrialBalanceData> list2 = new List<OpeningTrialBalanceData>();
        //    while (sdr2.Read())
        //    {
        //        OpeningTrialBalanceData trialBalanceReport = new OpeningTrialBalanceData();
        //        trialBalanceReport.NlAccName = sdr2["NlAccName"] != DBNull.Value ? (string)sdr2["NlAccName"] : "";
        //        trialBalanceReport.RunStatus = sdr2["run_status"] != DBNull.Value ? (decimal)sdr2["Cr"] : 0;
        //        list2.Add(trialBalanceReport);
        //    }
        //    cnn.Close();
        //    foreach (var trial in list)
        //    {
        //        var openingbalance = list2.Where(x => x.NlAccName == trial.NlAccName).FirstOrDefault().RunStatus;
        //        trial.OpeningBalance = openingbalance;
        //    }
        //}
    }
}
