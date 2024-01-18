using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MimeKit;
using MimeKit.Text;
using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Purchases.Customers;
using pyme_finance_api.Models.Purchases.Invoices;
using pyme_finance_api.Models.Purchases.LPO;
using pyme_finance_api.Models.Purchases.PurchaseReceipt;
using pyme_finance_api.Models.Purchases.PurchaseRequest;
using pyme_finance_api.Models.Purchases.PurchaseReturn;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Models.Sales.Terms;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Models.UserProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using pyme_finance_api.Models.Email;
using pyme_finance_api.Models.ReusableCodes;
using pyme_finance_api.Service.MailService;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Controllers.NlController;
using pyme_finance_api.Service.PlService;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;
using static System.Net.WebRequestMethods;
using System.Text.RegularExpressions;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Dashboard;
using System.Text;

namespace pyme_finance_api.Controllers.Purchases
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private IWebHostEnvironment _hostingEnvironment;
        readonly ILogger<PurchasesController> _log;

        public PurchasesController(IWebHostEnvironment environment, IConfiguration config, ILogger<PurchasesController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _log = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult allinvoices()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];


            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }



            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }
            _log.LogInformation($"fetching all purchases invoices");

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get purchases Invoices headers
            List<PLInvoice> plinvoicelist = new List<PLInvoice>();
            cnn.Open();

            string myq = "Select \"PLInvoiceHeader\".*, \"Currencies\".\"CrCode\", \"PLCustomer\".\"PLCustCode\", \"PLCustomer\".\"CustName\", fp_name,\"NlJournalHeader\".\"NlJrnlNo\" AS journalref " +
                "From \"PLInvoiceHeader\" " +
                "Left Join \"Currencies\" On \"Currencies\".\"CrId\" = \"PLInvoiceHeader\".\"CurrencyId\"" +
                " Left Join \"PLCustomer\" On \"PLCustomer\".\"CustID\" = \"PLInvoiceHeader\".\"PLCustID\" " +
                "  LEFT JOIN \"NlJournalHeader\" ON \"NlJournalHeader\".\"PlJrnlNo\" = \"PLInvoiceHeader\".\"PLJrnlNo\" " +
                "Left Join financial_periods On fp_ref = \"Period\" WHERE \"PLBranch\" = " + staff_branch + "AND \"NlJournalHeader\".\"TranFrom\" = 'PL' ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(myq, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                PLInvoice plInv = new PLInvoice();

                plInv.PLJrnNo = sdr0["PLJrnlNo"] != DBNull.Value ? (int)sdr0["PLJrnlNo"] : 0;
                plInv.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                plInv.PLCustID = sdr0["PLCustID"] != DBNull.Value ? (int)sdr0["PLCustID"] : 0;
                plInv.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                plInv.Period = sdr0["Period"] != DBNull.Value ? (string)sdr0["Period"] : null;
                plInv.journalRef = sdr0["journalref"] != DBNull.Value ? (int)sdr0["journalref"] : 0;


                plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (string)sdr0["DocRef"] : "";
                plInv.InvDate = sdr0["InvDate"] != DBNull.Value ? (DateTime)sdr0["InvDate"] : DateTime.Now;
                plInv.CurrencyId = sdr0["CurrencyId"] != DBNull.Value ? (int)sdr0["CurrencyId"] : 0;
                plInv.PLDescription = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : null;
                plInv.StaffId = sdr0["StaffId"] != DBNull.Value ? (int)sdr0["StaffId"] : 0;
                plInv.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                plInv.HasCreditNote = sdr0["HasCreditNote"] != DBNull.Value ? (bool)sdr0["HasCreditNote"] : false;
                plInv.DueDate = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                plInv.Totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                plInv.Balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;
                plInv.Additionals = sdr0["Additionals"] != DBNull.Value ? (string)sdr0["Additionals"] : null;
                /// fix this issue/
                plInv.Returned = sdr0["InvReturned"] != DBNull.Value ? (bool)sdr0["InvReturned"] : false;
                plInv.ttl_discount = sdr0["TotalDiscount"] != DBNull.Value ? (decimal)sdr0["TotalDiscount"] : 0;

                plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;

                plInv.fp_name = sdr0["fp_name"] != DBNull.Value ? (string)sdr0["fp_name"] : null;

                plinvoicelist.Add(plInv);
            }
            cnn.Close();


            return Ok(new { PLInvoices = plinvoicelist });

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult InvoiceDefaults()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get currencies
            List<Currencies> currencyList = new List<Currencies>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"Currencies\" ", cnn).ExecuteReader();

            while (sdr1.Read())
            {
                Currencies cr = new Currencies();

                cr.CrId = sdr1["CrId"] != DBNull.Value ? (int)sdr1["CrId"] : 0;
                cr.CrName = sdr1["CrName"] != DBNull.Value ? (string)sdr1["CrName"] : null;
                cr.CrCode = sdr1["CrCode"] != DBNull.Value ? (string)sdr1["CrCode"] : null;

                currencyList.Add(cr);
            }
            cnn.Close();

            //get PLcstomers
            List<PLCustomer> plcustomerlist = new List<PLCustomer>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*, \"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" ", cnn).ExecuteReader();

            while (sdr0.Read())
            {
                PLCustomer plCust = new PLCustomer();

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
                plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;

                plcustomerlist.Add(plCust);
            }
            cnn.Close();

            //get last PL invoice number
            int lastInvNumber = 0;

            //get last registered number
            //cnn.Open();
            //NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            //while (sdr3.Read())
            //{
            //    lastInvNumber = (int)sdr3["st"];
            //}
            //cnn.Close();

            //Get Invoice Settings
            InvoiceSettings invsettings = new InvoiceSettings();
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("Select *  FROM \"PLInvoiceSettings\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdr2.Read())
            {
                invsettings.InvPrefix = sdr2["InvPrefix"].ToString();
                invsettings.InvStartNumber = (int)sdr2["InvStartNumber"];
                invsettings.LastNumber = lastInvNumber;
                invsettings.InvNumberingType = sdr2["InvNumberingType"].ToString();
            }
            cnn.Close();

            //get inventory
            List<Inventory> inventory_list = new List<Inventory>();
            cnn.Open();

            NpgsqlDataReader sdr10 = new NpgsqlCommand("SELECT * FROM \"Inventory\" ", cnn).ExecuteReader();

            while (sdr10.Read())
            {
                Inventory inv = new Inventory();

                inv.InvtId = sdr10["InvtId"] != DBNull.Value ? (int)sdr10["InvtId"] : 0;
                inv.InvtType = sdr10["InvtId"] != DBNull.Value ? (string)sdr10["InvtType"] : null;
                inv.InvtName = sdr10["InvtName"] != DBNull.Value ? (string)sdr10["InvtName"] : null;
                inv.InvtProdCode = sdr10["InvtProdCode"] != DBNull.Value ? (string)sdr10["InvtProdCode"] : null;
                inv.InvtQty = sdr10["InvtQty"] != DBNull.Value ? (int)sdr10["InvtQty"] : 0;
                inv.InvtSP = sdr10["InvtSP"] != DBNull.Value ? (decimal)sdr10["InvtSP"] : 0;
                inv.InvtBP = sdr10["InvtBP"] != DBNull.Value ? (decimal)sdr10["InvtBP"] : 0;
                inv.InventoryItem = sdr10["InventoryItem"] != DBNull.Value ? (string)sdr10["InventoryItem"] : null;


                inventory_list.Add(inv);
            }
            cnn.Close();

            //Get financial period Settings
            FinancialPeriod finPrd = new FinancialPeriod();
            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand("Select *  FROM financial_periods WHERE fp_branch = " + staff_branch + " AND fp_active = 't' ", cnn)
                .ExecuteReader();
            while (sdr11.Read())
            {
                finPrd.fp_id = sdr11["fp_id"] != DBNull.Value ? (int)sdr11["fp_id"] : 0;
                finPrd.fp_name = sdr11["fp_name"] != DBNull.Value ? (string)sdr11["fp_name"] : null;
                finPrd.fp_ref = sdr11["fp_ref"] != DBNull.Value ? (string)sdr11["fp_ref"] : null;
                finPrd.fp_trans_date = sdr11["fp_trans_date"] != DBNull.Value
                    ? (DateTime)sdr11["fp_trans_date"]
                    : DateTime.Today;
                finPrd.fp_openingdate = sdr11["fp_openingdate"] != DBNull.Value
                    ? (DateTime)sdr11["fp_openingdate"]
                    : DateTime.Today;
                finPrd.fp_closingdate = sdr11["fp_closingdate"] != DBNull.Value
                    ? (DateTime)sdr11["fp_closingdate"]
                    : DateTime.Today;
                finPrd.fp_active = sdr11["fp_active"] != DBNull.Value ? (bool)sdr11["fp_active"] : false;
                finPrd.fp_date_mode = sdr11["fp_date_mode"] != DBNull.Value ? (string)sdr11["fp_date_mode"] : null;
            }
            cnn.Close();

            NlService nlService = new NlService(db);
            var nlaccounts = nlService.GetNlaccounts();

            return Ok(new { CurrencyData = currencyList, CustomerData = plcustomerlist, InvSettings = invsettings, financialPeriod_Data = finPrd, InventData = inventory_list, NLAccounts = nlaccounts });

        }



        //[Route("CreateCustomerInvoiceV2")]
        //[Authorize]
        //[HttpPost]
        //public ActionResult CreateCustomerInvoiceV2(PLInvoiceV2 invoiceData)
        //{

        //    _log.LogInformation($"Creating Purchasing Invoice");


        //    //check data
        //    if (string.IsNullOrEmpty(invoiceData.DocPrefix))
        //    {
        //        return BadRequest(new { message = "Missing document prefix" });

        //    }
        //    else if (invoiceData.DocRef == 0)
        //    {
        //        return BadRequest(new { message = "Missing document reference" });
        //    }
        //    else if (invoiceData.PLCustID == 0)
        //    {
        //        return BadRequest(new { message = "Missing invoice customer" });
        //    }
        //    else if (string.IsNullOrEmpty(invoiceData.Period))
        //    {
        //        return BadRequest(new { message = "Missing document financial period" });
        //    }
        //    else if (string.IsNullOrEmpty(invoiceData.PLDescription))
        //    {
        //        return BadRequest(new { message = "Missing document description" });
        //    }
        //    else if (invoiceData.InvoiceDetailsList.Count == 0)
        //    {
        //        return BadRequest(new { message = "No attached invoice details were found" });
        //    }

        //    //check due date
        //    int value = DateTime.Compare(invoiceData.DueDate, invoiceData.InvDate);
        //    if (value < 0)
        //    {
        //        //Console.Write("date1 is earlier than date2. ");
        //        return BadRequest(new { message = "The due date cannot be earlier than the invoice date" });
        //    }

        //    //check if company code exists
        //    var companyRes = "";
        //    int userId = 0;

        //    //check if cookie exists in Request
        //    string authHeader = Request.Headers[HeaderNames.Authorization];
        //    //permission name
        //    string permissionName = Request.Headers["PermName"];

        //    if (string.IsNullOrEmpty(permissionName))
        //    {
        //        return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
        //    }

        //    string jwtHeader = authHeader.Split(' ')[1];

        //    //get token data
        //    jwt_token jwt = new jwt_token(_configuration);

        //    var tokenData = jwt.GetClaim(jwtHeader, "DB");
        //    userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
        //    if (tokenData != "")
        //    {
        //        //assign company Ref from header
        //        companyRes = tokenData;
        //    }

        //    //check companyRes
        //    if (string.IsNullOrEmpty(companyRes))
        //    {
        //        return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
        //    }

        //    //check user id
        //    if (userId == 0)
        //    {
        //        return BadRequest(new { message = "Missing user reference. Page verification failed" });
        //    }

        //    //check permission
        //    bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
        //    if (perStatus == false)
        //    {
        //        return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
        //    }


        //    //get database name
        //    string db = companyRes;

        //    //get staff branch
        //    int staff_branch = myDbconnection.GetStaffBranch(userId, db);
        //    if (staff_branch == 0)
        //    {
        //        return BadRequest(new
        //        {
        //            message =
        //                "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
        //        });
        //    }

        //    NlService nlService = new NlService(db);

        //    for (int i = 0; i < invoiceData.InvoiceDetailsList.Count; i++)
        //    {
        //        var data = nlService.GetNLAccountAccountCodeUsingId(invoiceData.InvoiceDetailsList[i].ItemId);
        //        if (data.NlaccName == null)
        //        {
        //            _log.LogError($" account missing can't complete this transaction");
        //            return BadRequest(new
        //            {
        //                message =
        //                    $" An error  occoured while posting this invoice please try again later "
        //            });

        //        }
        //        else
        //        {
        //            invoiceData.InvoiceDetailsList[i].AccountName = data.NlaccName;
        //        }

        //    }



        //        if (nlService.GetNLAccountAccountCodeUsingName("CREDITORS") == null)
        //    {
        //        _log.LogError($"CREDITORS account missing can't complete this transaction");
        //        return BadRequest(new
        //        {
        //            message =
        //                "Missing CREDITORS account is your system setup please add it in the nl account module"
        //        });

        //    }
        //    //if (nlService.GetNLAccountAccountCodeUsingName("PURCHASES") == null)
        //    //{
        //    //    _log.LogError($"PURCHASES account missing can't complete this transaction");
        //    //    return BadRequest(new
        //    //    {
        //    //        message =
        //    //            "Missing PURCHASES account is your system setup please add it in the nl account module"
        //    //    });

        //    //}
        //    //if (nlService.GetNLAccountAccountCodeUsingName("VAT") == null)
        //    //{
        //    //    _log.LogError($"VAT account missing can't complete this transaction");
        //    //    return BadRequest(new
        //    //    {
        //    //        message =
        //    //            "Missing VAT account is your system setup please add it in the nl account module"
        //    //    });

        //    //}


        //    NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

        //    //get last PLJrnNo
        //    int lastPLjrnlNo = 0;
        //    cnn.Open();
        //    NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"PLJrnlNo\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
        //    while (sdra.Read())
        //    {
        //        lastPLjrnlNo = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
        //    }
        //    cnn.Close();

        //    //get last NLJrnNO
        //    int lastNLJRN = 0;
        //    cnn.Open();
        //    NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
        //    while (sdrb.Read())
        //    {
        //        lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;

        //    }
        //    cnn.Close();

        //    //get last DOC REF
        //    int lastDOCREF = 0;
        //    cnn.Open();
        //    NpgsqlDataReader sdrF = new NpgsqlCommand("Select MAX(\"DocRef\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
        //    while (sdrF.Read())
        //    {
        //        lastDOCREF = sdrF["sl"] != DBNull.Value ? (int)sdrF["sl"] : 0;
        //    }
        //    cnn.Close();

        //    cnn.Open();
        //    //get customer data currency
        //    PLCustomer plCust = new PLCustomer();
        //    NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*,\"Currencies\".\"CrId\",\"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" WHERE \"CustID\" = " + invoiceData.PLCustID + " ", cnn).ExecuteReader();
        //    if (sdr0.HasRows == false)
        //    {
        //        return BadRequest(new { message = "An occurred while trying to save invoice details. plcustomer doesnt exists" });
        //    }
        //    if (sdr0.Read())
        //    {
        //        plCust.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
        //        plCust.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
        //        plCust.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
        //        plCust.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
        //        plCust.CurrID = sdr0["CurrID"] != DBNull.Value ? (int)sdr0["CurrID"] : 0;
        //        plCust.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
        //        plCust.CustID = sdr0["CustID"] != DBNull.Value ? (int)sdr0["CustID"] : 0;
        //        plCust.RegisterDate = sdr0["RegisterDate"] != DBNull.Value ? (DateTime)sdr0["RegisterDate"] : DateTime.Now;
        //        plCust.StaffID = sdr0["StaffID"] != DBNull.Value ? (int)sdr0["StaffID"] : 0;
        //        plCust.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
        //        plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
        //        plCust.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
        //    }
        //    cnn.Close();

        //    int plDLlast = (lastPLjrnlNo + 1);
        //    int plDNlast = (lastNLJRN + 1);
        //    decimal CalcTotals = 0;
        //    _log.LogInformation($"Saving purchases details");
        //    //insert invoice details
        //    if (invoiceData.InvoiceDetailsList.Count > 0)
        //    {
        //        for (int i = 0; i < invoiceData.InvoiceDetailsList.Count; i++)
        //        {
        //           NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));

        //        //    //get inventory item id
        //        //    cnn1.Open();
        //        //    Inventory inv = new Inventory();
        //        //    NpgsqlDataReader sdr_inv =
        //        //        new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + invoiceData.InvoiceDetailsList[i].ItemId + "  ", cnn1)
        //        //            .ExecuteReader();
        //        //    if (sdr_inv.HasRows == false)
        //        //    {
        //        //        return BadRequest(new { message = "The Inventory Item reference could NOT be found." });
        //        //    }
        //        //    while (sdr_inv.Read())
        //        //    {
        //        //        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
        //        //        inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
        //        //        inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
        //        //    }
        //        //    cnn1.Close();


        //           decimal item_total = (invoiceData.InvoiceDetailsList[i].ItemUnitPrice * invoiceData.InvoiceDetailsList[i].ItemQty) + (decimal)invoiceData.InvoiceDetailsList[i].VatAmt;

        //        //    string item_details = null;
        //        //    if (string.IsNullOrEmpty(invoiceData.InvoiceDetailsList[i].StkDesc))
        //        //    {
        //        //        item_details = inv.InvtName;
        //        //    }
        //        //    else
        //        //    {
        //        //        item_details = inv.InvtName + " (" + invoiceData.InvoiceDetailsList[i].StkDesc + ")";
        //        //    }

        //        //    //CalcTotals += invoiceData.InvoiceDetailsList[i].Total;
        //            CalcTotals += (decimal)item_total;

        //        //    //save details
        //            cnn1.Open();
        //           string insertQuery1 = "INSERT INTO \"PLInvoiceDetail\" (\"PLJrnlNo\", \"JrnlPLNo\", \"UnitPrice\", \"VatPerc\", \"VatAmt\", \"ProdGroupCode\", \"NLAccCode\", \"StkDesc\", \"UserID\", \"ProdQty\",\"DiscountAmt\",\"Total\",\"ProdId\" ) VALUES(" + (plDLlast) + ", " + (plDNlast) + ", " + invoiceData.InvoiceDetailsList[i].ItemUnitPrice + ", '" + invoiceData.InvoiceDetailsList[i].VatCode + "', " + invoiceData.InvoiceDetailsList[i].VatAmt + ", '','" + invoiceData.InvoiceDetailsList[i].AccountName+ "','" + invoiceData.InvoiceDetailsList[i].AccountName+ "'," + userId + ", " + invoiceData.InvoiceDetailsList[i].ItemQty + "," + 0 + ", " + item_total + ", " + 0 + " ); ";
        //           bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
        //           cnn1.Close();
        //            if (myReq1 == false)
        //            {
        //               //failed
        //                return BadRequest(new { message = "An occurred while trying to save invoice details." });
        //           }



        //        }
        //    }



        //    //get last registered INV number
        //    int lastInvNumber = 0;
        //    cnn.Open();
        //    NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
        //    while (sdr3.Read())
        //    {
        //        lastInvNumber = (int)sdr3["st"];
        //    }
        //    cnn.Close();

        //    //Get Invoice Settings
        //    InvoiceSettings invsettings = new InvoiceSettings();
        //    cnn.Open();
        //    NpgsqlDataReader sdrInv = new NpgsqlCommand("Select *  FROM \"PLInvoiceSettings\" LIMIT 1 ", cnn).ExecuteReader();
        //    while (sdrInv.Read())
        //    {
        //        invsettings.InvPrefix = sdrInv["InvPrefix"].ToString();
        //        invsettings.InvStartNumber = (int)sdrInv["InvStartNumber"];
        //        invsettings.LastNumber = lastInvNumber;
        //        invsettings.InvNumberingType = sdrInv["InvNumberingType"].ToString();
        //    }
        //    cnn.Close();

        //    _log.LogInformation($"Fetching financial periods");

        //    //Get financial period Settings
        //    FinancialPeriod finPrd = new FinancialPeriod();
        //    cnn.Open();
        //    NpgsqlDataReader sdr11 = new NpgsqlCommand("Select *  FROM financial_periods WHERE fp_branch = " + staff_branch + " AND fp_active = 't' ", cnn)
        //        .ExecuteReader();
        //    while (sdr11.Read())
        //    {
        //        finPrd.fp_id = sdr11["fp_id"] != DBNull.Value ? (int)sdr11["fp_id"] : 0;
        //        finPrd.fp_name = sdr11["fp_name"] != DBNull.Value ? (string)sdr11["fp_name"] : null;
        //        finPrd.fp_ref = sdr11["fp_ref"] != DBNull.Value ? (string)sdr11["fp_ref"] : null;
        //        finPrd.fp_trans_date = sdr11["fp_trans_date"] != DBNull.Value
        //            ? (DateTime)sdr11["fp_trans_date"]
        //            : DateTime.Today;
        //        finPrd.fp_openingdate = sdr11["fp_openingdate"] != DBNull.Value
        //            ? (DateTime)sdr11["fp_openingdate"]
        //            : DateTime.Today;
        //        finPrd.fp_closingdate = sdr11["fp_closingdate"] != DBNull.Value
        //            ? (DateTime)sdr11["fp_closingdate"]
        //            : DateTime.Today;
        //        finPrd.fp_active = sdr11["fp_active"] != DBNull.Value ? (bool)sdr11["fp_active"] : false;
        //        finPrd.fp_date_mode = sdr11["fp_date_mode"] != DBNull.Value ? (string)sdr11["fp_date_mode"] : null;
        //    }
        //    cnn.Close();


        //    //set invoice header details

        //    _log.LogInformation($"Saving purchases invoice headers");
        //    cnn.Open();
        //    string insertQ = "INSERT INTO \"PLInvoiceHeader\" (\"PLJrnlNo\", \"NlJrnlNo\", \"PLCustID\", \"TranDate\", \"Period\", \"DocRef\", \"InvDate\", \"CurrencyId\", \"PLDescription\",\"StaffId\"," +
        //        "\"DocPrefix\",\"HasCreditNote\",\"DueDate\",\"Totals\",\"Balance\",\"PLBranch\",\"TotalDiscount\",\"LpoNumber\",\"InvReturned\" )" +
        //        " VALUES(" + (lastPLjrnlNo + 1) + ", " + (lastNLJRN + 1) + ", " + invoiceData.PLCustID + ", '" + DateTime.Now + "' ,'" + finPrd.fp_ref + "'," + (lastDOCREF + 1) + ",'" + DateTime.Now + "'," + plCust.CrId + ",'" + invoiceData.PLDescription + "', " + userId + ", '" + invsettings.InvPrefix + "','f','" + invoiceData.DueDate + "'," + (CalcTotals - invoiceData.ttl_discount) + "," + (CalcTotals - invoiceData.ttl_discount) + "," + staff_branch + "," + invoiceData.ttl_discount + " ,'" + invoiceData.LpoNumber + "'," + false + "); ";

        //    bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);

        //    cnn.Close();

        //    if (myReq2 == false)
        //    {
        //        //failed
        //        return BadRequest(new { message = "An occurred while trying to save invoice details." });
        //    }

        //    NlJournalHeader nlJournalHeader = new NlJournalHeader();

        //    nlJournalHeader.NlJrnlDesc = "INVP" + lastPLjrnlNo + 1;
        //    nlJournalHeader.TranDate = DateTime.Now;
        //    nlJournalHeader.MEndDate = invoiceData.DueDate;
        //    nlJournalHeader.TranYear = DateTime.Now.Year;
        //    nlJournalHeader.TranPeriod = DateTime.Now.Month;
        //    nlJournalHeader.TranType = "PL";
        //    nlJournalHeader.TranFrom = "";
        //    nlJournalHeader.ModuleId = null;
        //    nlJournalHeader.PlJrnlNo = lastPLjrnlNo + 1;

        //    _log.LogInformation($"Updating NLjournals ");

        //    var response = nlService.createNlJournalHeaderpl(nlJournalHeader, "PL", invoiceData.InvoiceDetailsList);

        //    if (response.Httpcode == 400)
        //    {
        //        _log.LogError(response.Message);
        //        return BadRequest(new
        //        {
        //            message =
        //                 "Sorry! We have problems saving the purchase invoice . Request failed. Please contact your system administrator"
        //        });
        //    }



        //    AuditTrailService auditTrailService = new AuditTrailService(tokenData);
        //    AuditTrail auditTrail = new AuditTrail();
        //    auditTrail.action = "Created PLInvoice  Ref PL" + (lastPLjrnlNo + 1) + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", (CalcTotals - invoiceData.ttl_discount));
        //    auditTrail.module = "PURCHASES";
        //    auditTrail.userId = userId;
        //    auditTrailService.createAuditTrail(auditTrail);
        //    _log.LogInformation(auditTrail.action);


        //    //success
        //    return Ok(new
        //    {
        //        message = "Invoice has been successfully posted"

        //    });


        //}










        [Route("CreateCustomerInvoicev1")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCustomerInvoicev1(PLInvoice invoiceData)
        {
            _log.LogInformation($"Creating Purchasing Invoice");


            //check data
            if (string.IsNullOrEmpty(invoiceData.DocPrefix))
            {
                return BadRequest(new { message = "Missing document prefix" });

            }
            else if (String.IsNullOrEmpty(invoiceData.CustomerRef))
            {
                return BadRequest(new { message = "Missing document reference" });
            }
            else if (invoiceData.PLCustID == 0)
            {
                return BadRequest(new { message = "Missing invoice customer" });
            }
            else if (string.IsNullOrEmpty(invoiceData.Period))
            {
                return BadRequest(new { message = "Missing document financial period" });
            }
            else if (string.IsNullOrEmpty(invoiceData.PLDescription))
            {
                return BadRequest(new { message = "Missing document description" });
            }
            else if (invoiceData.InvoiceDetailsList.Count == 0)
            {
                return BadRequest(new { message = "No attached invoice details were found" });
            }

            //check due date
            int value = DateTime.Compare(invoiceData.DueDate, invoiceData.InvDate);
            if (value < 0)
            {
                //Console.Write("date1 is earlier than date2. ");
                return BadRequest(new { message = "The due date cannot be earlier than the invoice date" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];

            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            string jwtHeader = authHeader.Split(' ')[1];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }


            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }

            NlService nlService = new NlService(db);


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get last PLJrnNo
            int lastPLjrnlNo = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"PLJrnlNo\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastPLjrnlNo = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //get last NLJrnNO
            int lastNLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;

            }
            cnn.Close();

            //get last DOC REF
            int lastDOCREF = 0;
            cnn.Open();
            NpgsqlDataReader sdrF = new NpgsqlCommand("Select MAX(\"DocRef\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrF.Read())
            {
                lastDOCREF = sdrF["sl"] != DBNull.Value ? (int)sdrF["sl"] : 0;
            }
            cnn.Close();

            cnn.Open();
            //get customer data currency
            PLCustomer plCust = new PLCustomer();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*,\"Currencies\".\"CrId\",\"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" WHERE \"CustID\" = " + invoiceData.PLCustID + " ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                return BadRequest(new { message = "An occurred while trying to save invoice details. plcustomer doesnt exists" });
            }
            if (sdr0.Read())
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
                plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plCust.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
            }
            cnn.Close();

            int plDLlast = (lastPLjrnlNo + 1);
            int plDNlast = (lastNLJRN + 1);
            decimal CalcTotals = 0;
            _log.LogInformation($"Saving purchases details");
            StringBuilder inventory_stringBuilder = new StringBuilder();

            if (invoiceData.InvoiceDetailsList.Count > 0)
            {
                for (int i = 0; i < invoiceData.InvoiceDetailsList.Count; i++)
                {
                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));

                    //get inventory item id
                    cnn1.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr_inv =
                        new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + invoiceData.InvoiceDetailsList[i].ItemId + "  ", cnn1)
                            .ExecuteReader();
                    if (sdr_inv.HasRows == false)
                    {
                        return BadRequest(new { message = "The Inventory Item reference could NOT be found." });
                    }
                    while (sdr_inv.Read())
                    {
                        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                        inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                        inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                        inv.PLProdGrpCode = sdr_inv["PLProdGrpCode"] != DBNull.Value ? (string)sdr_inv["PLProdGrpCode"] : null;
                    }
                    cnn1.Close();


                    decimal item_total = (invoiceData.InvoiceDetailsList[i].ItemUnitPrice * invoiceData.InvoiceDetailsList[i].ItemQty) + (decimal)invoiceData.InvoiceDetailsList[i].VatAmt;

                    string item_details = null;
                    if (string.IsNullOrEmpty(invoiceData.InvoiceDetailsList[i].StkDesc))
                    {
                        item_details = inv.InvtName;
                    }
                    else
                    {
                        item_details = inv.InvtName + " (" + invoiceData.InvoiceDetailsList[i].StkDesc + ")";
                    }

                    //CalcTotals += invoiceData.InvoiceDetailsList[i].Total;
                    CalcTotals += (decimal)item_total;


                    //save details
                    cnn1.Open();
                    string insertQuery1 = "INSERT INTO \"PLInvoiceDetail\" (\"PLJrnlNo\", \"JrnlPLNo\", \"UnitPrice\", \"VatPerc\", \"VatAmt\", \"ProdGroupCode\", \"NLAccCode\", \"StkDesc\", \"UserID\", \"ProdQty\",\"DiscountAmt\",\"Total\",\"ProdId\" ) VALUES(" + (plDLlast) + ", " + (plDNlast) + ", " + invoiceData.InvoiceDetailsList[i].ItemUnitPrice + ", '" + invoiceData.InvoiceDetailsList[i].VatCode + "', " + invoiceData.InvoiceDetailsList[i].VatAmt + ", '" + invoiceData.InvoiceDetailsList[i].ItemCode + "','" + null + "','" + item_details + "'," + userId + ", " + invoiceData.InvoiceDetailsList[i].ItemQty + "," + 0 + ", " + item_total + ", " + invoiceData.InvoiceDetailsList[i].ItemId + " ); ";
                    inventory_stringBuilder.Append(insertQuery1);

                }
            }

            //get last registered INV number
            int lastInvNumber = 0;
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                lastInvNumber = (int)sdr3["st"];
            }
            cnn.Close();

            //Get Invoice Settings
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

            _log.LogInformation($"Fetching financial periods");

            //Get financial period Settings
            FinancialPeriod finPrd = new FinancialPeriod();
            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand("Select *  FROM financial_periods WHERE fp_branch = " + staff_branch + " AND fp_active = 't' ", cnn)
                .ExecuteReader();
            while (sdr11.Read())
            {
                finPrd.fp_id = sdr11["fp_id"] != DBNull.Value ? (int)sdr11["fp_id"] : 0;
                finPrd.fp_name = sdr11["fp_name"] != DBNull.Value ? (string)sdr11["fp_name"] : null;
                finPrd.fp_ref = sdr11["fp_ref"] != DBNull.Value ? (string)sdr11["fp_ref"] : null;
                finPrd.fp_trans_date = sdr11["fp_trans_date"] != DBNull.Value
                    ? (DateTime)sdr11["fp_trans_date"]
                    : DateTime.Today;
                finPrd.fp_openingdate = sdr11["fp_openingdate"] != DBNull.Value
                    ? (DateTime)sdr11["fp_openingdate"]
                    : DateTime.Today;
                finPrd.fp_closingdate = sdr11["fp_closingdate"] != DBNull.Value
                    ? (DateTime)sdr11["fp_closingdate"]
                    : DateTime.Today;
                finPrd.fp_active = sdr11["fp_active"] != DBNull.Value ? (bool)sdr11["fp_active"] : false;
                finPrd.fp_date_mode = sdr11["fp_date_mode"] != DBNull.Value ? (string)sdr11["fp_date_mode"] : null;
            }
            cnn.Close();


            //set invoice header details

            _log.LogInformation($"Saving purchases invoice headers");
            cnn.Open();
            int custref = Int32.Parse(invoiceData.CustomerRef);
            string insertQ = "INSERT INTO \"PLInvoiceHeader\" (\"PLJrnlNo\", \"NlJrnlNo\", \"PLCustID\", \"TranDate\", \"Period\", \"DocRef\", \"InvDate\", \"CurrencyId\", \"PLDescription\",\"StaffId\"," +
                "\"DocPrefix\",\"HasCreditNote\",\"DueDate\",\"Totals\",\"Balance\",\"PLBranch\",\"TotalDiscount\",\"LpoNumber\",\"InvReturned\" )" +
                " VALUES(" + (lastPLjrnlNo + 1) + ", " + (lastNLJRN + 1) + ", " + invoiceData.PLCustID + ", '" + DateTime.Now + "' ,'" + finPrd.fp_ref + "'," + custref + ",'" + DateTime.Now + "'," + plCust.CrId + ",'" + invoiceData.PLDescription + "', " + userId + ", '" + invsettings.InvPrefix + "','f','" + invoiceData.DueDate + "'," + (CalcTotals - invoiceData.ttl_discount) + "," + (CalcTotals - invoiceData.ttl_discount) + "," + staff_branch + "," + invoiceData.ttl_discount + " ,'" + invoiceData.LpoNumber + "'," + false + "); ";










            using (var trans = cnn.BeginTransaction())
            {
                try
                {





                }
                catch (Exception e)
                {
                    trans.Rollback();



                }




            }






            return null;

        }


















        [Route("CreateCustomerInvoice")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateCustomerInvoice(PLInvoice invoiceData)
        {

            _log.LogInformation($"Creating Purchasing Invoice");


            //check data
            if (string.IsNullOrEmpty(invoiceData.DocPrefix))
            {
                return BadRequest(new { message = "Missing document prefix" });

            }
            else if (String.IsNullOrEmpty(invoiceData.CustomerRef))
            {
                return BadRequest(new { message = "Missing document reference" });
            }
            else if (invoiceData.PLCustID == 0)
            {
                return BadRequest(new { message = "Missing invoice customer" });
            }
            else if (string.IsNullOrEmpty(invoiceData.Period))
            {
                return BadRequest(new { message = "Missing document financial period" });
            }
            else if (string.IsNullOrEmpty(invoiceData.PLDescription))
            {
                return BadRequest(new { message = "Missing document description" });
            }
            else if (invoiceData.InvoiceDetailsList.Count == 0)
            {
                return BadRequest(new { message = "No attached invoice details were found" });
            }

            //check due date
            int value = DateTime.Compare(invoiceData.DueDate, invoiceData.InvDate);
            if (value < 0)
            {
                //Console.Write("date1 is earlier than date2. ");
                return BadRequest(new { message = "The due date cannot be earlier than the invoice date" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];

            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            string jwtHeader = authHeader.Split(' ')[1];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }


            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }

            NlService nlService = new NlService(db);
            //if (nlService.GetNLAccountAccountCodeUsingName("CREDITORS") == null)
            //{
            //    _log.LogError($"CREDITORS account missing can't complete this transaction");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing CREDITORS account is your system setup please add it in the nl account module"
            //    });

            //}
            //if (nlService.GetNLAccountAccountCodeUsingName("PURCHASES") == null)
            //{
            //    _log.LogError($"PURCHASES account missing can't complete this transaction");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing PURCHASES account is your system setup please add it in the nl account module"
            //    });

            //}
            //if (nlService.GetNLAccountAccountCodeUsingName("VAT") == null)
            //{
            //    _log.LogError($"VAT account missing can't complete this transaction");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing VAT account is your system setup please add it in the nl account module"
            //    });

            //}


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get last PLJrnNo
            int lastPLjrnlNo = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"PLJrnlNo\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastPLjrnlNo = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //get last NLJrnNO
            int lastNLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;

            }
            cnn.Close();

            //get last DOC REF
            int lastDOCREF = 0;
            //cnn.Open();
            //NpgsqlDataReader sdrF = new NpgsqlCommand("Select MAX(\"DocRef\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            //while (sdrF.Read())
            //{
            //    lastDOCREF = sdrF["sl"] != DBNull.Value ? (int)sdrF["sl"] : 0;
            //}
            //cnn.Close();

            cnn.Open();
            //get customer data currency
            PLCustomer plCust = new PLCustomer();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*,\"Currencies\".\"CrId\",\"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" WHERE \"CustID\" = " + invoiceData.PLCustID + " ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                return BadRequest(new { message = "An occurred while trying to save invoice details. plcustomer doesnt exists" });
            }
            if (sdr0.Read())
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
                plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plCust.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
            }
            cnn.Close();

            int plDLlast = (lastPLjrnlNo + 1);
            int plDNlast = (lastNLJRN + 1);
            decimal CalcTotals = 0;
            _log.LogInformation($"Saving purchases details");
            //insert invoice details
            if (invoiceData.InvoiceDetailsList.Count > 0)
            {
                for (int i = 0; i < invoiceData.InvoiceDetailsList.Count; i++)
                {
                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));

                    //get inventory item id
                    cnn1.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr_inv =
                        new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + invoiceData.InvoiceDetailsList[i].ItemId + "  ", cnn1)
                            .ExecuteReader();
                    if (sdr_inv.HasRows == false)
                    {
                        return BadRequest(new { message = "The Inventory Item reference could NOT be found." });
                    }
                    while (sdr_inv.Read())
                    {
                        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                        inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                        inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                        inv.PLProdGrpCode = sdr_inv["PLProdGrpCode"] != DBNull.Value ? (string)sdr_inv["PLProdGrpCode"] : null;
                    }
                    cnn1.Close();


                    decimal item_total = (invoiceData.InvoiceDetailsList[i].ItemUnitPrice * invoiceData.InvoiceDetailsList[i].ItemQty) + (decimal)invoiceData.InvoiceDetailsList[i].VatAmt;

                    string item_details = null;
                    if (string.IsNullOrEmpty(invoiceData.InvoiceDetailsList[i].StkDesc))
                    {
                        item_details = inv.InvtName;
                    }
                    else
                    {
                        item_details = inv.InvtName + " (" + invoiceData.InvoiceDetailsList[i].StkDesc + ")";
                    }

                    //CalcTotals += invoiceData.InvoiceDetailsList[i].Total;
                    CalcTotals += (decimal)item_total;


                    //save details
                    cnn1.Open();
                    string insertQuery1 = "INSERT INTO \"PLInvoiceDetail\" (\"PLJrnlNo\", \"JrnlPLNo\", \"UnitPrice\", \"VatPerc\", \"VatAmt\", \"ProdGroupCode\", \"NLAccCode\", \"StkDesc\", \"UserID\", \"ProdQty\",\"DiscountAmt\",\"Total\",\"ProdId\" ) VALUES(" + (plDLlast) + ", " + (plDNlast) + ", " + invoiceData.InvoiceDetailsList[i].ItemUnitPrice + ", '" + invoiceData.InvoiceDetailsList[i].VatCode + "', " + invoiceData.InvoiceDetailsList[i].VatAmt + ", '" + invoiceData.InvoiceDetailsList[i].ItemCode + "','" + null + "','" + item_details + "'," + userId + ", " + invoiceData.InvoiceDetailsList[i].ItemQty + "," + 0 + ", " + item_total + ", " + invoiceData.InvoiceDetailsList[i].ItemId + " ); ";
                    bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                    cnn1.Close();
                    if (myReq1 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occurred while trying to save invoice details." });
                    }
                    //// check if item is connected to nominal account
                    //if (!inv.PLProdGrpCode.Equals(null) || inv.PLProdGrpCode != "0")
                    //{
                    // PlService plService = new PlService(tokenData);
                    //    invoiceData.InvoiceDetailsList[i].AccountName =    plService.getPlAccountGroupNominal(inv.PLProdGrpCode);


                    //}






                    ///dont update inventory when creating inventory

                    ////update inventory if goods
                    //if (inv.InvtType == "GOODS")
                    //{
                    //    string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" + " + invoiceData.InvoiceDetailsList[i].ItemQty +
                    //                    " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + invoiceData.InvoiceDetailsList[i].ItemId + " ";
                    //    bool myReq24 = myDbconnection.UpdateDelInsert(up_inv, db);
                    //    if (myReq24 == false)
                    //    {
                    //        //failed
                    //        return BadRequest(new
                    //            { message = "An occurred while trying to process Inventory update requests." });
                    //    }
                    //}

                    //insert into warehouse summary
                    //ManageWarehouseSummary whs = new ManageWarehouseSummary();
                    //bool wh_req = whs.warehouse_summary_sl_pl(db, invoiceData.InvoiceDetailsList[i].ItemId, invoiceData.InvoiceDetailsList[i].ItemQty, userId, "Purchase");
                    //if (wh_req == false)
                    //{
                    //    //failed
                    //    return BadRequest(new
                    //        { message = "An occurred while trying to register request to warehouse summary." });
                    //}


                }
            }


















            //get last registered INV number
            int lastInvNumber = 0;
            //cnn.Open();
            //NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            //while (sdr3.Read())
            //{
            //    lastInvNumber = (int)sdr3["st"];
            //}
            //cnn.Close();

            //Get Invoice Settings
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

            _log.LogInformation($"Fetching financial periods");

            //Get financial period Settings
            FinancialPeriod finPrd = new FinancialPeriod();
            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand("Select *  FROM financial_periods WHERE fp_branch = " + staff_branch + " AND fp_active = 't' ", cnn)
                .ExecuteReader();
            while (sdr11.Read())
            {
                finPrd.fp_id = sdr11["fp_id"] != DBNull.Value ? (int)sdr11["fp_id"] : 0;
                finPrd.fp_name = sdr11["fp_name"] != DBNull.Value ? (string)sdr11["fp_name"] : null;
                finPrd.fp_ref = sdr11["fp_ref"] != DBNull.Value ? (string)sdr11["fp_ref"] : null;
                finPrd.fp_trans_date = sdr11["fp_trans_date"] != DBNull.Value
                    ? (DateTime)sdr11["fp_trans_date"]
                    : DateTime.Today;
                finPrd.fp_openingdate = sdr11["fp_openingdate"] != DBNull.Value
                    ? (DateTime)sdr11["fp_openingdate"]
                    : DateTime.Today;
                finPrd.fp_closingdate = sdr11["fp_closingdate"] != DBNull.Value
                    ? (DateTime)sdr11["fp_closingdate"]
                    : DateTime.Today;
                finPrd.fp_active = sdr11["fp_active"] != DBNull.Value ? (bool)sdr11["fp_active"] : false;
                finPrd.fp_date_mode = sdr11["fp_date_mode"] != DBNull.Value ? (string)sdr11["fp_date_mode"] : null;
            }
            cnn.Close();


            //set invoice header details

            _log.LogInformation($"Saving purchases invoice headers");
            cnn.Open();
            //int custref = Int32.Parse(invoiceData.CustomerRef);
            string insertQ = "INSERT INTO \"PLInvoiceHeader\" (\"PLJrnlNo\", \"NlJrnlNo\", \"PLCustID\", \"TranDate\", \"Period\", \"DocRef\", \"InvDate\", \"CurrencyId\", \"PLDescription\",\"StaffId\"," +
                "\"DocPrefix\",\"HasCreditNote\",\"DueDate\",\"Totals\",\"Balance\",\"PLBranch\",\"TotalDiscount\",\"LpoNumber\",\"InvReturned\" )" +
                " VALUES(" + (lastPLjrnlNo + 1) + ", " + (lastNLJRN + 1) + ", " + invoiceData.PLCustID + ", '" + DateTime.Now + "' ,'" + finPrd.fp_ref + "','" + invoiceData.CustomerRef + "','" + DateTime.Now + "'," + plCust.CrId + ",'" + invoiceData.PLDescription + "', " + userId + ", '" + invsettings.InvPrefix + "','f','" + invoiceData.DueDate + "'," + (CalcTotals - invoiceData.ttl_discount) + "," + (CalcTotals - invoiceData.ttl_discount) + "," + staff_branch + "," + invoiceData.ttl_discount + " ,'" + invoiceData.LpoNumber + "'," + false + "); ";

            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);

            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to save invoice details." });
            }



            NlJournalHeader nlJournalHeader = new NlJournalHeader();

            nlJournalHeader.NlJrnlDesc = "INVP" + lastPLjrnlNo + 1;
            nlJournalHeader.TranDate = DateTime.Now;
            nlJournalHeader.MEndDate = invoiceData.DueDate;
            nlJournalHeader.TranYear = invoiceData.PeriodYear;
            nlJournalHeader.TranPeriod = invoiceData.PeriodMonth;
            nlJournalHeader.TranType = "PL";
            nlJournalHeader.TranFrom = "";
            nlJournalHeader.ModuleId = null;
            nlJournalHeader.PlJrnlNo = lastPLjrnlNo + 1;

            _log.LogInformation($"Updating NLjournals ");

            var response = nlService.createNlJournalHeaderpl(nlJournalHeader, "PL", invoiceData.InvoiceDetailsList);

            if (response.Httpcode == 400)
            {
                _log.LogError(response.Message);
                return BadRequest(new
                {
                    message =
                         "Sorry! We have problems saving the purchase invoice . Request failed. Please contact your system administrator"
                });
            }


            ////get last vat transaction
            //int last_vat_transaction = 0;
            //cnn.Open();
            //NpgsqlDataReader sdr_vt_ = new NpgsqlCommand("SELECT COALESCE(MAX(\"v_id\"),0) as st FROM \"vat_transactions\" LIMIT 1 ", cnn).ExecuteReader();
            //while (sdr_vt_.Read())
            //{
            //    last_vat_transaction = (int)sdr_vt_["st"];
            //}
            //cnn.Close();

            ////Get VAT Balance
            //TaxSetup vt = new TaxSetup();
            //cnn.Open();
            //NpgsqlDataReader sdr_tx = new NpgsqlCommand("Select *  FROM \"NLAccount\" WHERE \"GroupCode\" = 'VRH20' AND \"AccBranch\" = " + staff_branch + " ", cnn)
            //    .ExecuteReader();
            //while (sdr_tx.Read())
            //{
            //    vt.VAT_Balance = sdr_tx["AccBalance"] != DBNull.Value ? (decimal)sdr_tx["AccBalance"] : 0;

            //}
            //cnn.Close();

            //string unique_trans = System.Guid.NewGuid().ToString("D");

            ////Add VAT transaction
            //string transaction_description = "Purchase PLJrnlNo " + (lastPLjrnlNo + 1);
            //cnn.Open();
            //string insrt_vat = "INSERT INTO vat_transactions (v_id, v_date, v_ref, v_type, v_transaction,v_trans_ref,v_amount, v_balance ) VALUES(" + (last_vat_transaction + 1) + ", '" + DateTime.Now + "', '"+unique_trans+"' ,'PURCHASE', '"+ transaction_description + "','" + (lastDOCREF + 1) + "'," + -(CalcTotals - invoiceData.ttl_discount) + "," + (vt.VAT_Balance - (CalcTotals - invoiceData.ttl_discount)) + " ); ";

            //bool myReq_vat = myDbconnection.UpdateDelInsert(insrt_vat, db);
            //cnn.Close();
            //if (myReq_vat == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occurred while trying to save VAT details." });
            //}

            ////update VAT account
            //cnn.Open();
            //string up_vat = "UPDATE \"NLAccount\" SET \"AccBalance\" = \"AccBalance\" - " + (CalcTotals - invoiceData.ttl_discount) + " WHERE \"GroupCode\" = 'VRH20' AND \"AccBranch\" = " + staff_branch + " ";
            //bool myReq21 = myDbconnection.UpdateDelInsert(up_vat, db);
            //cnn.Close();
            //if (myReq21 == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occurred while trying to process NL VAT update requests." });
            //}

            ////update debtors
            //cnn.Open();
            //string up_crd = "UPDATE \"NLAccount\" SET \"AccBalance\" = \"AccBalance\" +" +
            //                (CalcTotals - invoiceData.ttl_discount) + " WHERE \"GroupCode\" = 'DRH20' AND \"AccBranch\" = " + staff_branch + " ";
            //bool myReq22 = myDbconnection.UpdateDelInsert(up_crd, db);
            //cnn.Close();
            //if (myReq22 == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occurred while trying to process NL Creditors update requests." });
            //}

            ////update PL Customers
            //cnn.Open();
            //string up_cust = "UPDATE \"PLCustomer\" SET \"AmtOwed\" = \"AmtOwed\" + " + (CalcTotals - invoiceData.ttl_discount) + " WHERE \"PLCustCode\" = '" + plCust.PLCustCode + "' ";
            //bool myReq23 = myDbconnection.UpdateDelInsert(up_cust, db);
            //cnn.Close();

            //if (myReq23 == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occurred while trying to process PL Customer update requests." });
            //}

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Created PLInvoice  Ref PL" + (lastPLjrnlNo + 1) + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", (CalcTotals - invoiceData.ttl_discount));
            auditTrail.module = "PURCHASES";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            _log.LogInformation(auditTrail.action);


            //success
            return Ok(new
            {
                message = "Invoice has been successfully posted"

            });


        }





        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult ConvertLpotoInvoice(int lpoid)
        {
            _log.LogInformation("Converting LPO to invoice ");
            if (lpoid == 0)
            {
                _log.LogError("Request could not be done lpo id is missing ");
                return BadRequest(new { message = "Cannot find the required reference." });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            //string permissionName = Request.Headers["PermName"];


            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}


            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NlJournalHeader nlJournalHeader = new NlJournalHeader();

            nlJournalHeader.NlJrnlDesc = "";
            nlJournalHeader.TranDate = DateTime.Now;

            nlJournalHeader.TranYear = DateTime.Now.Year;
            nlJournalHeader.TranPeriod = DateTime.Now.Month;
            nlJournalHeader.TranType = "";
            nlJournalHeader.TranFrom = "PL";
            nlJournalHeader.ModuleId = null;


            _log.LogInformation("Checking if purchasing invoice exists ");

            PlService plService = new PlService(companyRes);
            var invoicedetaillist = plService.GetLpoHeaderDetails(lpoid);
            var invoiceheader = plService.GetLpoHeaderById(lpoid);
            if (invoiceheader.Invoiced)
            {
                return BadRequest(new { message = "This LPO has already been invoiced" });
            }

            _log.LogInformation("Creating LPO invoice ");

            var response = plService.convertlpotoInvoice(lpoid, nlJournalHeader, invoicedetaillist, userId, staff_branch);

            if (response.Httpcode == 400)
            {
                _log.LogError(response.Message);
                return BadRequest(new { message = response.Message });
            }

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"PLInvoice created from LPO  {DateTime.Now.ToString("dd/MM/yyyy")} ";
            auditTrail.module = "PURCHASES";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);


            return Ok(new
            {
                message = "Invoice has been successfully created from this lpo"

            });
        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PLINVReport(int pljrnNo)
        {
            try
            {
                if (pljrnNo == 0)
                {
                    return BadRequest(new { message = "Cannot find the required reference." });
                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                string jwtHeader = authHeader.Split(' ')[1];

                //permission name
                string permissionName = Request.Headers["PermName"];


                //if (string.IsNullOrEmpty(permissionName))
                //{
                //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                //}


                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                //if (perStatus == false)
                //{
                //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                //}

                //get database name
                string db = companyRes;

                //get user branch
                int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
                if (staff_branch == 0)
                {
                    return BadRequest(new { message = "We have problems identifying your branch" });
                }

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //get purchases Invoices header
                PLInvoice plInv = new PLInvoice();
                cnn.Open();

                string query = "SELECT \"PLInvoiceHeader\".*, \"CrCode\",\"PLCustCode\",\"CustName\",\"PhysicalAddress\",\"PostalAddress\",\"VATNo\",\"UFirstName\",\"ULastName\" FROM \"PLInvoiceHeader\" LEFT JOIN  \"Currencies\" ON \"CrId\" = \"CurrencyId\" " +
                    "LEFT JOIN  \"PLCustomer\" ON \"CustID\" = \"PLCustID\" " +
                    "LEFT JOIN \"Users\" ON \"UId\" = \"StaffId\" " +
                    " WHERE \"PLJrnlNo\" = " + pljrnNo + " AND \"PLBranch\" = " + staff_branch + " ";

                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                if (sdr0.HasRows == false)
                {
                    return BadRequest(new { message = "Could NOT find invoice.H details" });
                }

                if (sdr0.Read())
                {
                    plInv.PLJrnNo = sdr0["PLJrnlNo"] != DBNull.Value ? (int)sdr0["PLJrnlNo"] : 0;
                    plInv.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                    plInv.PLCustID = sdr0["PLCustID"] != DBNull.Value ? (int)sdr0["PLCustID"] : 0;
                    plInv.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                    plInv.Period = sdr0["Period"] != DBNull.Value ? (string)sdr0["Period"] : null;
                    plInv.LpoNumber = sdr0["LpoNumber"] != DBNull.Value ? (string)sdr0["LpoNumber"] : "";
                    plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (string)sdr0["DocRef"] : "";
                    plInv.InvDate = sdr0["InvDate"] != DBNull.Value ? (DateTime)sdr0["InvDate"] : DateTime.Now;
                    plInv.CurrencyId = sdr0["CurrencyId"] != DBNull.Value ? (int)sdr0["CurrencyId"] : 0;
                    plInv.PLDescription = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : null;
                    plInv.StaffId = sdr0["StaffId"] != DBNull.Value ? (int)sdr0["StaffId"] : 0;
                    plInv.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                    plInv.HasCreditNote = sdr0["HasCreditNote"] != DBNull.Value ? (bool)sdr0["HasCreditNote"] : false;
                    plInv.DueDate = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                    plInv.Totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    plInv.Balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;
                    plInv.Additionals = sdr0["Additionals"] != DBNull.Value ? (string)sdr0["Additionals"] : null;
                    //   plInv.HasReturned = sdr0["HasReturned"] != DBNull.Value ? (BOOL)sdr0["HasReturned"] : null;
                    plInv.ttl_discount = sdr0["TotalDiscount"] != DBNull.Value ? (decimal)sdr0["TotalDiscount"] : 0;
                    plInv.Returned = sdr0["InvReturned"] != DBNull.Value ? (bool)sdr0["InvReturned"] : false;
                    //check credit note date
                    //   plInv.CRNDate = sdr0["CRNDate"] != DBNull.Value ? (DateTime)sdr0["CRNDate"] : DateTime.Now;
                    plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                    plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                    plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                    plInv.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                    plInv.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                    plInv.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;

                    plInv.UFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                    plInv.ULastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;
                    plInv.CRNVat = sdr0["CRNVat"] != DBNull.Value ? (decimal)sdr0["CRNVat"] : 0;
                    plInv.CRNTotal = sdr0["CRNTotal"] != DBNull.Value ? (decimal)sdr0["CRNTotal"] : 0;
                    plInv.CRNDate = sdr0["CRNDate"] != DBNull.Value ? (DateTime)sdr0["CRNDate"] : (DateTime)sdr0["InvDate"];
                }
                cnn.Close();

                cnn.Open();
                List<PLInvoiceDetails> pldetailsList = new List<PLInvoiceDetails>();
                NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"PLInvoiceDetail\"  WHERE \"PLJrnlNo\" = " + pljrnNo + "  ", cnn).ExecuteReader();
                if (sdr1.HasRows == false)
                {
                    return BadRequest(new { message = "Could NOT find invoice.B details" });
                }

                while (sdr1.Read())
                {
                    PLInvoiceDetails plInvdetails = new PLInvoiceDetails();

                    plInvdetails.PLJrnlNo = sdr1["PLJrnlNo"] != DBNull.Value ? (int)sdr1["PLJrnlNo"] : 0;
                    plInvdetails.JrnlPLNo = sdr1["JrnlPLNo"] != DBNull.Value ? (int)sdr1["JrnlPLNo"] : 0;
                    plInvdetails.UnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;
                    plInvdetails.VatPerc = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : null;
                    plInvdetails.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                    plInvdetails.ProdGroupCode = sdr1["ProdGroupCode"] != DBNull.Value ? (string)sdr1["ProdGroupCode"] : null;
                    plInvdetails.NLAccCode = sdr1["NLAccCode"] != DBNull.Value ? (string)sdr1["NLAccCode"] : null;
                    plInvdetails.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : null;
                    plInvdetails.UserID = sdr1["UserID"] != DBNull.Value ? (int)sdr1["UserID"] : 0;
                    plInvdetails.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                    plInvdetails.DiscountAmt = sdr1["DiscountAmt"] != DBNull.Value ? (decimal)sdr1["DiscountAmt"] : 0;
                    plInvdetails.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;

                    pldetailsList.Add(plInvdetails);
                }
                cnn.Close();

                //get company data
                License lic = new License();
                cnn.Open();
                NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
                if (sdr3.Read())
                {
                    lic.CompanyName = (string)sdr3["CompanyName"];
                    lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                    lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                    lic.CompanyContact = (string)sdr3["CompanyContact"];
                    lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                    lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                    lic.CompanyLogo = (string)sdr3["CompanyLogo"];
                }
                cnn.Close();
                string img_path = "";
                bool url_is_base64 = IsBase64String(lic.CompanyLogo);
                if (String.IsNullOrEmpty(lic.CompanyLogo))
                {
                    //upload image
                    //using ngenx.jpg for test purpose
                    lic.CompanyLogo = "invoice_default.jpg";
                    //  lic.CompanyLogo = "ngenx.jpg";
                    img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                    string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                    byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                    lic.CompanyLogo = Convert.ToBase64String(imageArray); ;
                }
                else
                {
                    img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                    string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                    byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                    lic.CompanyLogo = Convert.ToBase64String(imageArray);
                }

                Allterms invterms = new Allterms();
                cnn.Open();
                NpgsqlDataReader sdr4 = new NpgsqlCommand( "SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'pl_inv_terms' AND branch = " + staff_branch +" ", cnn).ExecuteReader();
                while (sdr4.Read())
                {
                    invterms.tosID = sdr4["tosID"] != DBNull.Value ? (int)sdr4["tosID"] : 0;
                    invterms.tosType = sdr4["tosType"] != DBNull.Value ? (string)sdr4["tosType"] : null;
                    invterms.terms = sdr4["terms"] != DBNull.Value ? sdr4["terms"].ToString() : null;
                }
                cnn.Close();

                List<PurchaseHeaderSettings> headSetting = new List<PurchaseHeaderSettings>();
                cnn.Open();
                NpgsqlDataReader reader1 = new NpgsqlCommand("SELECT * FROM \"Document_header\" WHERE \"Category\" = 'PURCHASE' ORDER BY id ASC; ", cnn).ExecuteReader();
                while (reader1.Read())
                {
                    PurchaseHeaderSettings head = new PurchaseHeaderSettings();
                    head.Id = reader1["id"] != DBNull.Value ? (int)reader1["id"] : 0;
                    head.Category = reader1["Category"] != DBNull.Value ? (string)reader1["Category"] : null;
                    head.DocumentName = reader1["DocumentName"] != DBNull.Value ? (string)reader1["DocumentName"] : null;
                    head.Status = reader1["Status"] != DBNull.Value ? (bool)reader1["Status"] : false;
                    headSetting.Add(head);
                }
                cnn.Close();

                return Ok(new { PLHeader = plInv, PLDetails = pldetailsList, myCompany = lic, LPOTerms = invterms, headerSettings = headSetting });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PL_Receipt_Defaults(int pljrn_receipt)
        {
            try
            {
                if (pljrn_receipt == 0)
                {
                    return BadRequest(new { message = "Cannot find the required reference." });
                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                string jwtHeader = authHeader.Split(' ')[1];

                //permission name
                string permissionName = Request.Headers["PermName"];


                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }


                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                //if (perStatus == false)
                //{
                //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                //}

                //get database name
                string db = companyRes;

                //get user branch
                int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
                if (staff_branch == 0)
                {
                    return BadRequest(new { message = "We have problems identifying your branch" });
                }

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //get purchases Invoices header
                PLInvoice plInv = new PLInvoice();
                cnn.Open();

                NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLInvoiceHeader\".*, \"CrCode\",\"PLCustCode\",\"CustName\",\"PhysicalAddress\",\"PostalAddress\",\"VATNo\",\"UFirstName\",\"ULastName\",fp_name FROM \"PLInvoiceHeader\"" +
                    " LEFT JOIN  \"Currencies\" ON \"CrId\" = \"CurrencyId\" " +
                    "LEFT JOIN  \"PLCustomer\" ON \"CustID\" = \"PLCustID\"" +
                    " LEFT JOIN \"Users\" ON \"UId\" = \"StaffId\"" +
                    " LEFT JOIN  \"financial_periods\" ON \"Period\" = \"fp_ref\" " +
                    " WHERE \"PLJrnlNo\" = " + pljrn_receipt + " AND \"PLBranch\" = " + staff_branch + "  ", cnn).ExecuteReader();
                if (sdr0.HasRows == false)
                {
                    return BadRequest(new { message = "Could NOT find invoice.H details" });
                }

                if (sdr0.Read())
                {

                    plInv.PLJrnNo = sdr0["PLJrnlNo"] != DBNull.Value ? (int)sdr0["PLJrnlNo"] : 0;
                    plInv.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                    plInv.PLCustID = sdr0["PLCustID"] != DBNull.Value ? (int)sdr0["PLCustID"] : 0;
                    plInv.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                    plInv.Period = sdr0["Period"] != DBNull.Value ? (string)sdr0["Period"] : null;

                    plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (string)sdr0["DocRef"] : "";
                    plInv.InvDate = sdr0["InvDate"] != DBNull.Value ? (DateTime)sdr0["InvDate"] : DateTime.Now;
                    plInv.CurrencyId = sdr0["CurrencyId"] != DBNull.Value ? (int)sdr0["CurrencyId"] : 0;
                    plInv.PLDescription = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : null;
                    plInv.StaffId = sdr0["StaffId"] != DBNull.Value ? (int)sdr0["StaffId"] : 0;
                    plInv.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                    plInv.HasCreditNote = sdr0["HasCreditNote"] != DBNull.Value ? (bool)sdr0["HasCreditNote"] : false;
                    plInv.DueDate = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                    plInv.Totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    plInv.Balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;
                    plInv.Additionals = sdr0["Additionals"] != DBNull.Value ? (string)sdr0["Additionals"] : null;
                    //   plInv.HasReturned = sdr0["HasReturned"] != DBNull.Value ? (string)sdr0["HasReturned"] : null;
                    plInv.ttl_discount = sdr0["TotalDiscount"] != DBNull.Value ? (decimal)sdr0["TotalDiscount"] : 0;
                    plInv.Returned = sdr0["InvReturned"] != DBNull.Value ? (bool)sdr0["InvReturned"] : false;


                    //CREDIT NOTE DATE FIX
                    // plInv.CRNDate = sdr0["CRNDate"] != DBNull.Value ? (DateTime)sdr0["CRNDate"] : DateTime.Now;

                    plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                    plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                    plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                    plInv.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                    plInv.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                    plInv.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                    // plInv.AmtOwed = sdr0["AmtOwed"] != DBNull.Value ? (decimal)sdr0["AmtOwed"] : 0;


                    plInv.UFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                    plInv.ULastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;

                    plInv.fp_name = sdr0["fp_name"] != DBNull.Value ? (string)sdr0["fp_name"] : null;

                }
                cnn.Close();

                cnn.Open();
                List<PLInvoiceDetails> pldetailsList = new List<PLInvoiceDetails>();
                NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"PLInvoiceDetail\"  WHERE \"PLJrnlNo\" = " + pljrn_receipt + "  ", cnn).ExecuteReader();
                if (sdr1.HasRows == false)
                {
                    return BadRequest(new { message = "Could NOT find invoice.B details" });
                }

                while (sdr1.Read())
                {
                    PLInvoiceDetails plInvdetails = new PLInvoiceDetails();

                    plInvdetails.PLJrnlNo = sdr1["PLJrnlNo"] != DBNull.Value ? (int)sdr1["PLJrnlNo"] : 0;
                    plInvdetails.JrnlPLNo = sdr1["JrnlPLNo"] != DBNull.Value ? (int)sdr1["JrnlPLNo"] : 0;
                    plInvdetails.UnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;
                    plInvdetails.VatPerc = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : null;
                    plInvdetails.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                    plInvdetails.ProdGroupCode = sdr1["ProdGroupCode"] != DBNull.Value ? (string)sdr1["ProdGroupCode"] : null;
                    plInvdetails.NLAccCode = sdr1["NLAccCode"] != DBNull.Value ? (string)sdr1["NLAccCode"] : null;
                    plInvdetails.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : null;
                    plInvdetails.UserID = sdr1["UserID"] != DBNull.Value ? (int)sdr1["UserID"] : 0;
                    plInvdetails.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                    plInvdetails.DiscountAmt = sdr1["DiscountAmt"] != DBNull.Value ? (decimal)sdr1["DiscountAmt"] : 0;
                    plInvdetails.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;

                    pldetailsList.Add(plInvdetails);
                }
                cnn.Close();

                //get company data
                License lic = new License();
                cnn.Open();
                NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
                if (sdr3.Read())
                {
                    lic.CompanyName = (string)sdr3["CompanyName"];
                    lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                    lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                    lic.CompanyContact = (string)sdr3["CompanyContact"];
                    lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                    lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                    lic.CompanyLogo = (string)sdr3["CompanyLogo"];
                }
                cnn.Close();

                //get all receipts attached to that invoice
                List<PLReceipts> pl_receiptsList = new List<PLReceipts>();




                string query = "SELECT a.*, b.\"UFirstName\" as firstname,b.\"ULastName\" as lastname,b.\"UId\" FROM \"PLReceipts\" a " +
                    "LEFT JOIN \"Users\" b ON(a.\"pyUser\" = b.\"UId\") WHERE \"pyInvRef\" = " + pljrn_receipt + ";";

                cnn.Open();
                NpgsqlDataReader sdr3_p = new NpgsqlCommand(query, cnn).ExecuteReader();

                while (sdr3_p.Read())
                {
                    PLReceipts pr = new PLReceipts();

                    pr.pyID = sdr3_p["pyID"] != DBNull.Value ? (int)sdr3_p["pyID"] : 0;
                    pr.pyRef = sdr3_p["pyRef"] != DBNull.Value ? (int)sdr3_p["pyRef"] : 0;
                    pr.pyDate = sdr3_p["pyDate"] != DBNull.Value ? (DateTime)sdr3_p["pyDate"] : DateTime.Now;
                    pr.pyInvRef = sdr3_p["pyInvRef"] != DBNull.Value ? (int)sdr3_p["pyInvRef"] : 0;
                    pr.pyPayable = sdr3_p["pyPayable"] != DBNull.Value ? (float)sdr3_p["pyPayable"] : 0;
                    pr.pyPaid = sdr3_p["pyPaid"] != DBNull.Value ? (float)sdr3_p["pyPaid"] : 0;
                    pr.pyBalance = sdr3_p["pyBalance"] != DBNull.Value ? (float)sdr3_p["pyBalance"] : 0;
                    pr.pyMode = sdr3_p["pyMode"] != DBNull.Value ? (string)sdr3_p["pyMode"] : null;
                    pr.pyChequeNumber = sdr3_p["pyChequeNumber"] != DBNull.Value ? (string)sdr3_p["pyChequeNumber"] : null;
                    pr.pyReceivedBy = sdr3_p["pyReceivedBy"] != DBNull.Value ? (string)sdr3_p["pyReceivedBy"] : null;
                    pr.pyAdditionalDetails = sdr3_p["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3_p["pyAdditionalDetails"] : null;
                    pr.pyProcessDate = sdr3_p["pyProcessDate"] != DBNull.Value ? (DateTime)sdr3_p["pyProcessDate"] : DateTime.Now;
                    pr.pyUser = sdr3_p["pyUser"] != DBNull.Value ? (int)sdr3_p["pyUser"] : 0;
                    //pr.pyCancelled = sdr3_p["pyCancelled"] != DBNull.Value ? (bool)sdr3_p["pyCancelled"] : false;
                    // pr.pyCancelReason = sdr3_p["pyCancelReason"] != DBNull.Value ? (string)sdr3_p["pyCancelReason"] : null;
                    pr.pyBranch = sdr3_p["pyBranch"] != DBNull.Value ? (int)sdr3_p["pyBranch"] : 0;

                    pr.UFirstName = sdr3_p["firstname"] != DBNull.Value ? (string)sdr3_p["firstname"] : null;
                    pr.ULastName = sdr3_p["lastname"] != DBNull.Value ? (string)sdr3_p["lastname"] : null;


                    pl_receiptsList.Add(pr);

                }
                cnn.Close();

                return Ok(new { PLHeader = plInv, PLDetails = pldetailsList, myCompany = lic, ReceiptHistory = pl_receiptsList });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

        }
        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult PL_Receipt_CreateNew(PLReceipts rcp_data)
        {

            try
            {
                if (string.IsNullOrEmpty(rcp_data.pyDate.ToString()))
                {
                    return BadRequest(new { message = "Missing receipt payment date." });

                }
                else if (string.IsNullOrEmpty(rcp_data.pyInvRef.ToString()) || rcp_data.pyInvRef == 0)
                {
                    return BadRequest(new { message = "Missing or invalid receipt invoice reference." });
                }
                else if (string.IsNullOrEmpty(rcp_data.pyMode))
                {
                    return BadRequest(new { message = "Missing receipt payment mode." });
                }
                else if (string.IsNullOrEmpty(rcp_data.pyChequeNumber) && rcp_data.pyMode == "cheque")
                {
                    return BadRequest(new { message = "Missing cheque number for cheque payment" });
                }
                else if (string.IsNullOrEmpty(rcp_data.pyReceivedBy))
                {
                    return BadRequest(new { message = "Missing receipt received from details." });
                }
                else if (!string.IsNullOrEmpty(rcp_data.Attachment_Image))
                {

                    //remove prefix

                    string strImage = rcp_data.Attachment_Image.Substring(rcp_data.Attachment_Image.IndexOf(',') + 1);
                    rcp_data.Attachment_Image = strImage;

                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                //permission name
                string permissionName = Request.Headers["PermName"];

                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }

                string jwtHeader = authHeader.Split(' ')[1];

                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }


                string db = companyRes;

                //get staff branch
                int staff_branch = myDbconnection.GetStaffBranch(userId, db);
                if (staff_branch == 0)
                {
                    return BadRequest(new
                    {
                        message =
                            "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                    });
                }

                //start
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //get company data
                License lic = new License();
                cnn.Open();
                NpgsqlDataReader sdrLc = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
                if (sdrLc.Read())
                {
                    lic.CompanyName = (string)sdrLc["CompanyName"];
                    //lic.CompanySlogan = (string)sdrLc["CompanySlogan"];
                    lic.CompanyPostal = (string)sdrLc["CompanyPostal"];
                    lic.CompanyContact = (string)sdrLc["CompanyContact"];
                    lic.CompanyVAT = (string)sdrLc["CompanyVAT"];
                    lic.PhysicalAddress = (string)sdrLc["PhysicalAddress"];
                    //lic.CompanyLogo = (string)sdrLc["CompanyLogo"];
                }
                cnn.Close();


                //get purchases Invoices header
                PLInvoice plInv = new PLInvoice();
                cnn.Open();

                NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLInvoiceHeader\".*, \"CrCode\",\"PLCustCode\",\"CustName\",\"PhysicalAddress\",\"PostalAddress\"," +
                    " \"VATNo\",\"UFirstName\",\"ULastName\" FROM \"PLInvoiceHeader\" " +
                    " LEFT JOIN  \"Currencies\" ON \"CrId\" = \"CurrencyId\" LEFT JOIN  " +
                    " \"PLCustomer\" ON \"CustID\" = \"PLCustID\" " +
                     " LEFT JOIN \"Users\" ON \"UId\" = \"StaffId\" " +
                    " WHERE \"PLJrnlNo\" = " + rcp_data.pyInvRef + " AND \"PLBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
                if (sdr0.HasRows == false)
                {
                    return BadRequest(new { message = "Could NOT find invoice.H details" });
                }

                if (sdr0.Read())
                {

                    plInv.PLJrnNo = sdr0["PLJrnlNo"] != DBNull.Value ? (int)sdr0["PLJrnlNo"] : 0;
                    plInv.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                    plInv.PLCustID = sdr0["PLCustID"] != DBNull.Value ? (int)sdr0["PLCustID"] : 0;
                    plInv.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                    plInv.Period = sdr0["Period"] != DBNull.Value ? (string)sdr0["Period"] : null;

                    plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (string)sdr0["DocRef"] : "";
                    plInv.InvDate = sdr0["InvDate"] != DBNull.Value ? (DateTime)sdr0["InvDate"] : DateTime.Now;
                    plInv.CurrencyId = sdr0["CurrencyId"] != DBNull.Value ? (int)sdr0["CurrencyId"] : 0;
                    plInv.PLDescription = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : null;
                    plInv.StaffId = sdr0["StaffId"] != DBNull.Value ? (int)sdr0["StaffId"] : 0;
                    plInv.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                    plInv.HasCreditNote = sdr0["HasCreditNote"] != DBNull.Value ? (bool)sdr0["HasCreditNote"] : false;
                    plInv.DueDate = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                    plInv.Totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                    plInv.Balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;
                    plInv.Additionals = sdr0["Additionals"] != DBNull.Value ? (string)sdr0["Additionals"] : null;
                    plInv.HasReturned = (bool)sdr0["InvReturned"];
                    plInv.ttl_discount = sdr0["TotalDiscount"] != DBNull.Value ? (decimal)sdr0["TotalDiscount"] : 0;

                    //plInv.CRNDate = sdr0["CRNDate"] != DBNull.Value ? (DateTime)sdr0["CRNDate"] : DateTime.Now;


                    plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                    plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                    plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                    plInv.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                    plInv.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                    plInv.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;


                    plInv.UFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                    plInv.ULastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;

                }
                cnn.Close();


                //get all receipts attached to that invoice
                cnn.Open();
                List<PLInvoiceDetails> pldetailsList = new List<PLInvoiceDetails>();
                NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"PLInvoiceDetail\"  WHERE \"PLJrnlNo\" = " + plInv.PLJrnNo + "  ", cnn).ExecuteReader();
                if (sdr1.HasRows == false)
                {
                    return BadRequest(new { message = "Could NOT find invoice.B details" });
                }

                while (sdr1.Read())
                {
                    PLInvoiceDetails plInvdetails = new PLInvoiceDetails();

                    plInvdetails.PLJrnlNo = sdr1["PLJrnlNo"] != DBNull.Value ? (int)sdr1["PLJrnlNo"] : 0;
                    plInvdetails.JrnlPLNo = sdr1["JrnlPLNo"] != DBNull.Value ? (int)sdr1["JrnlPLNo"] : 0;
                    plInvdetails.UnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;
                    plInvdetails.VatPerc = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : null;
                    plInvdetails.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                    plInvdetails.ProdGroupCode = sdr1["ProdGroupCode"] != DBNull.Value ? (string)sdr1["ProdGroupCode"] : null;
                    plInvdetails.NLAccCode = sdr1["NLAccCode"] != DBNull.Value ? (string)sdr1["NLAccCode"] : null;
                    plInvdetails.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : null;
                    plInvdetails.UserID = sdr1["UserID"] != DBNull.Value ? (int)sdr1["UserID"] : 0;
                    plInvdetails.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                    plInvdetails.DiscountAmt = sdr1["DiscountAmt"] != DBNull.Value ? (decimal)sdr1["DiscountAmt"] : 0;
                    plInvdetails.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;

                    pldetailsList.Add(plInvdetails);
                }
                cnn.Close();



                decimal pl_balance = plInv.Balance - (decimal)rcp_data.pyPaid;

                //create transaction

                //   get last transid
                int last_transid = 0;
                //cnn.Open();
                //NpgsqlDataReader sdr_tr = new NpgsqlCommand("Select MAX(\"t_id\") as sl From \"transactions\" LIMIT 1 ", cnn).ExecuteReader();
                //while (sdr_tr.Read())
                //{
                //    last_transid = sdr_tr["sl"] != DBNull.Value ? (int)sdr_tr["sl"] : 0;
                //}
                //cnn.Close();

                string unique_id = System.Guid.NewGuid().ToString("D");

                string trans_narration = "Payment Receipt if purchase Invoice " + plInv.DocPrefix +
                                         plInv.CustomerRef + " paid through " + rcp_data.pyMode +
                                         " and paid to " + rcp_data.pyReceivedBy;

                //string insert_trans_q = "INSERT INTO \"transactions\" (\"t_id\", \"t_date\", \"t_ref\", \"t_type\", \"t_opening_bal\", \"t_amount\", \"t_closing_bal\", \"t_user\", \"t_branch\", \"t_attachment_ref\", \"t_narration\", \"t_img_ref\", \"t_additional_details\" ) VALUES(" + (last_transid + 1) + ", '" + DateTime.Now + "', '" + unique_id + "', 'PURCHASE', " + plInv.Balance + ", " + rcp_data.pyPaid + "," + pl_balance + "," + userId + ", " + staff_branch + ", '" + rcp_data.pyInvRef + "','" + trans_narration + "', '" + null + "','" + rcp_data.pyAdditionalDetails + "' ); ";

                //bool trans_req = myDbconnection.UpdateDelInsert(insert_trans_q, db);
                //if (trans_req == false)
                //{
                //    //failed
                //    return BadRequest(new { message = "An occurred while trying to create your transaction receipt." });
                //}


                //update PL customer
                //string up_plcust = "UPDATE \"PLCustomer\" SET \"AmtOwed\" = \"AmtOwed\" - " + rcp_data.pyPaid +
                //                " WHERE \"CustID\" = " + plInv.PLCustID + " ";
                //bool plcust_req = myDbconnection.UpdateDelInsert(up_plcust, db);
                //if (plcust_req == false)
                //{
                //    //failed
                //    return BadRequest(new
                //    { message = "An occurred while trying to process Inventory update requests." });
                //}



                //update PLHeader
                string plheader_upd = "UPDATE \"PLInvoiceHeader\" SET \"Balance\" = '" + pl_balance + "' " +
                                     " WHERE \"PLJrnlNo\" = " + rcp_data.pyInvRef + " ";
                bool plheadr_req = myDbconnection.UpdateDelInsert(plheader_upd, db);
                if (plheadr_req == false)
                {
                    //failed
                    return BadRequest(new
                    { message = "An occurred while trying to Update PL balance" });
                }

                //update cash account
                //******************************************

                //get last pyid
                int last_pyid = 0;
                cnn.Open();
                NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"pyID\") as sl From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdra.Read())
                {
                    last_pyid = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
                }
                cnn.Close();

                //get lat pyref
                int lastpyRef = 0;
                cnn.Open();
                NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"pyRef\"), 0) as ref From \"PLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdrb.Read())
                {
                    lastpyRef = (int)sdrb["ref"];

                }
                cnn.Close();

                _log.LogInformation("Saving PLReceipts into database ");


                //save PL receipt details
                string insertQuery = "INSERT INTO \"PLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\"," +
                    " \"pyReceivedBy\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\" )" +
                    " VALUES(" + (last_pyid + 1) + ", " + (lastpyRef + 1) + ", '" + rcp_data.pyDate + "', " + rcp_data.pyInvRef + ", " +
                    "" + plInv.Balance + ", " + rcp_data.pyPaid + "," + pl_balance + ",'" + rcp_data.pyMode + "', '" + rcp_data.pyChequeNumber + "'," +
                    " '" + rcp_data.pyReceivedBy + "','" + rcp_data.pyAdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + " ); ";

                bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery, db);
                if (myReq1 == false)
                {
                    return BadRequest(new { message = "An occurred while trying to create your payment receipt." });
                }


                NlService nlService = new NlService(db);
                NlJournalHeader nlJournalHeader = new NlJournalHeader();
                nlJournalHeader.NlJrnlDesc = "";
                nlJournalHeader.TranDate = DateTime.Now;
                nlJournalHeader.MEndDate = plInv.DueDate;
                nlJournalHeader.TranYear = DateTime.Now.Year;
                nlJournalHeader.TranPeriod = DateTime.Now.Month;
                nlJournalHeader.TranType = "";
                nlJournalHeader.TranFrom = "";
                nlJournalHeader.ModuleId = null;
                nlJournalHeader.PlJrnlNo = plInv.PLJrnNo;

                _log.LogInformation("Updating NLJOURNALS records ");

                var response = nlService.savePurchaseReceiptsAccount(nlJournalHeader, rcp_data.pyMode, (float)rcp_data.pyPaid, plInv);

                if (response.Httpcode == 400)
                {
                    return BadRequest(new { message = response.Message });
                }
                AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                AuditTrail auditTrail = new AuditTrail();
                auditTrail.action = "Created Purchase Receipt " + (last_pyid + 1) + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", rcp_data.pyPaid);
                auditTrail.module = "Purchase Receipt";
                auditTrail.userId = userId;
                auditTrailService.createAuditTrail(auditTrail);





                //end

                ////Email
                //var path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplates", "PurchaseReceiptPayment.html");

                //var builder = new BodyBuilder();
                //using (StreamReader SourceReader = System.IO.File.OpenText(path))
                //{
                //    builder.HtmlBody = SourceReader.ReadToEnd();
                //}
                ////{0} : Subject
                ////{1} : DateTime
                ////{2} : Email
                ////{3} : Password
                ////{4} : Message
                ////{5} : callbackURL

                //string receiptRef = "PYT" + (lastpyRef + 1).ToString("D4");
                //string desc = "";
                //if (plInv.Totals < (decimal)rcp_data.pyPaid)
                //{
                //    desc = "partial payment of " + plInv.DocPrefix + plInv.DocRef.ToString("D4");
                //}
                //else
                //{
                //    desc = "payment of " + plInv.DocPrefix + plInv.DocRef.ToString("D4");
                //}


                ////create list of previous receipts
                //decimal ttl_VAT = 0;
                //decimal subtottal = 0;
                //string inv_details = null;
                //foreach (var _item in pldetailsList)
                //{
                //    decimal sub_ = _item.Total - _item.VatAmt;
                //    subtottal = subtottal + sub_;

                //    ttl_VAT = _item.VatAmt;

                //    string row_data =
                //        "<tr style='width: 100%; font-size: 12px; font-family: Segoe-UI; padding: 10px;'>" +
                //        "<td style ='font-weight: 500; border: solid 1px #505050;'>" + _item.StkDesc + "</td>" +
                //        "<td style ='font-weight: 500; border: solid 1px #505050;'>" + _item.ProdQty + "</td>" +
                //        "<td style ='font-weight: 500; border: solid 1px #505050;'>" + plInv.CrCode + " " +
                //        _item.UnitPrice.ToString("N") + "</td>" +
                //        "<td style ='font-weight: 500; border: solid 1px #505050;'>" + _item.VatPerc + "%" + "</td>" +
                //        "<td style ='font-weight: 500; border: solid 1px #505050;'>" + plInv.CrCode + " " +
                //        _item.VatAmt.ToString("N") + "</td>" +
                //        "<td style ='font-weight: 500; border: solid 1px #505050;'>" + plInv.CrCode + " " +
                //        _item.Total.ToString("N") + "</td>" +
                //        "</tr>";
                //    inv_details = inv_details + row_data;
                //}

                //string messageBody = string.Format(builder.HtmlBody,
                //    receiptRef,
                //    String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),

                //    plInv.DocPrefix + plInv.DocRef.ToString("D4"),

                //    lic.CompanyName,// from company name
                //    plInv.CustName, //to company
                //    plInv.CrCode + " " + plInv.Balance.ToString("N"), //payable totals
                //    plInv.CrCode + " " + rcp_data.pyPaid.ToString("N"), //paid
                //    plInv.CrCode + " " + pl_balance.ToString("N"), //balance

                //    lic.CompanyName,//from
                //    receiptRef,//receipt
                //    lic.PhysicalAddress,//payer address
                //    plInv.CrCode + " " + plInv.Balance.ToString("N"), //payable
                //    desc,
                //    plInv.CustName,//cust name
                //    rcp_data.pyMode,//payment mode
                //    rcp_data.pyChequeNumber, //cheque number

                //    "<span style='color: #007E33;'>Successful</span>",
                //    plInv.CrCode + " " + rcp_data.pyPaid.ToString("N"), //paid
                //    plInv.CrCode + " " + pl_balance.ToString("N"), //balance
                //    rcp_data.pyReceivedBy,

                //    plInv.CustName,//cust name
                //    plInv.PLCustCode, //cust code
                //    plInv.PhysicalAddress, //address
                //    plInv.VATNo, //VAT number

                //    lic.CompanyName, //company name
                //    lic.CompanySlogan, //slogan
                //    lic.CompanyPostal, //po box
                //    lic.PhysicalAddress, //address
                //    lic.CompanyContact, //contact

                //    inv_details, // PL invoice details
                //    plInv.CrCode + " " + subtottal.ToString("N"), //total subtotal
                //    plInv.CrCode + " " + ttl_VAT.ToString("N"), //total VAT
                //    plInv.CrCode + " " + plInv.ttl_discount.ToString("N"), //total discount
                //    plInv.CrCode + " " + plInv.Totals.ToString("N") //totals

                //);

                //check attachment and link it
                //if receipt has image attached


                //sent email to customer confirming payment
                //EmailFunctions eml_functions = new EmailFunctions(_configuration);

                //EmailProp eml_prop = new EmailProp(new string[] { "mwangijustus12@gmail.com","fabianmwangi77@hotmail.com" }, "Purchase Receipt Payment "+ plInv.DocPrefix + plInv.DocRef.ToString("D4"),
                //   messageBody, rcp_data.Attachment_Image);

                //eml_functions.SendEmail(eml_prop);

                return Ok(new { message = "Receipt has been successfully created" });


            }
            catch (Exception e)
            {
                e.StackTrace.TrimEnd();
                _log.LogError(e.Message);
                return BadRequest(new { message = e.Message });
            }


        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetPL_Settings()
        {
            try
            {
                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                string jwtHeader = authHeader.Split(' ')[1];

                //permission name
                string permissionName = Request.Headers["PermName"];


                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }

                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                if (perStatus == false)
                {
                    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                }

                //get database name
                string db = companyRes;


                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //Get invoice terms and conditions
                Allterms invterms = new Allterms();
                cnn.Open();

                NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'pl_inv_terms' ", cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    invterms.tosID = sdr1["tosID"] != DBNull.Value ? (int)sdr1["tosID"] : 0;
                    invterms.tosType = sdr1["tosType"] != DBNull.Value ? (string)sdr1["tosType"] : null;
                    invterms.terms = sdr1["terms"] != DBNull.Value ? sdr1["terms"].ToString() : null;
                }
                cnn.Close();



                //Get PL Invoice Settings
                InvoiceSettings invsettings = new InvoiceSettings();
                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand("Select *  FROM \"PLInvoiceSettings\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdr2.Read())
                {
                    invsettings.InvPrefix = sdr2["InvPrefix"].ToString();
                    invsettings.InvStartNumber = (int)sdr2["InvStartNumber"];
                    invsettings.InvNumberingType = sdr2["InvNumberingType"].ToString();
                }
                cnn.Close();

                // Decode the content for showing on Web page.
                invterms.terms = WebUtility.HtmlDecode(invterms.terms);

                //Get LPO Settings
                LPOSettings lposettings = new LPOSettings();
                cnn.Open();
                NpgsqlDataReader sdr21 = new NpgsqlCommand("Select *  FROM \"LPOSettings\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdr21.Read())
                {
                    lposettings.LPO_SID = sdr21["LPO_SID"] != DBNull.Value ? (int)sdr21["LPO_SID"] : 0;
                    lposettings.LPO_SPrefix = sdr21["LPO_SPrefix"] != DBNull.Value ? (string)sdr21["LPO_SPrefix"] : null;
                    lposettings.LPO_StartNO = sdr21["LPO_StartNO"] != DBNull.Value ? (int)sdr21["LPO_StartNO"] : 0;
                    lposettings.LPO_NumberingType = sdr21["LPO_NumberingType"] != DBNull.Value ? (string)sdr21["LPO_NumberingType"] : null;
                }
                cnn.Close();

                return Ok(new { PLInvTerms = invterms, PLInvSettings = invsettings, LPOConfigs = lposettings });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });

            }

        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult CreateUpdateInvTerms(Allterms termsData)
        {

            if (string.IsNullOrEmpty(termsData.terms))
            {
                return BadRequest(new { message = "Missing required terms details" });
            }



            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];

            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            string jwtHeader = authHeader.Split(' ')[1];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }



            //get database name
            string db = companyRes;

            string htmlEncoded = WebUtility.HtmlEncode(termsData.terms);

            // Encode the content for storing in Sql server.
            //string htmlEncoded = WebUtility.HtmlEncode(text);

            // Decode the content for showing on Web page.
            //string original = WebUtility.HtmlDecode(htmlEncoded);

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //check if row exists
            bool PLtermsExist = false;
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'pl_inv_terms' ", cnn).ExecuteReader();
            if (sdr0.HasRows == true)
            {
                PLtermsExist = true;
            }
            else
            {
                PLtermsExist = false;
            }
            cnn.Close();

            if (PLtermsExist == true)
            {
                //update
                string myQuery = "UPDATE \"AllSystemTerms\" SET \"terms\" = '" + htmlEncoded + "' WHERE \"tosType\" = 'pl_inv_terms'  ";
                bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);

                if (reqStatus == true)
                {
                    return Ok(new { message = "Your invoice terms have been successfully updated" });
                }
                else
                {
                    return BadRequest(new { message = "An error occurred while trying to process your request" });
                }
            }
            else
            {
                //get previous value
                int Last_alID = 0;
                cnn.Open();
                NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"tosID\") as sj From \"AllSystemTerms\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdrb.Read())
                {
                    Last_alID = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;

                }
                cnn.Close();

                //insert
                string myQuery = "INSERT INTO \"AllSystemTerms\" (\"tosID\",\"tosType\",\"terms\") VALUES (" + (Last_alID + 1) + ",'pl_inv_terms','" + termsData.terms + "'   ) ";
                bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);

                if (reqStatus == true)
                {
                    return Ok(new { message = "Your invoice terms have been successfully created" });
                }
                else
                {
                    return BadRequest(new { message = "An error occurred while trying to process your request" });
                }
            }




        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult SavePL_INV_Settings(InvoiceSettings invdata)
        {

            try
            {
                if (string.IsNullOrEmpty(invdata.InvNumberingType))
                {
                    return BadRequest(new { message = "Missing required PL numbering type" });

                }
                else if (string.IsNullOrEmpty(invdata.InvPrefix))
                {
                    return BadRequest(new { message = "Missing required PL invoice prefix" });
                }
                else if (string.IsNullOrEmpty(invdata.InvStartNumber.ToString()))
                {
                    return BadRequest(new { message = "Missing required PL invoice start number" });
                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                //permission name
                string permissionName = Request.Headers["PermName"];

                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }

                string jwtHeader = authHeader.Split(' ')[1];

                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                if (perStatus == false)
                {
                    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                }

                //get database name
                string db = companyRes;

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //update
                cnn.Open();
                string myQuery = "UPDATE \"PLInvoiceSettings\" SET \"InvPrefix\" = '" + invdata.InvPrefix + "', \"InvStartNumber\" = " + invdata.InvStartNumber + ", \"InvNumberingType\" = '" + invdata.InvNumberingType + "'  ";
                bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
                cnn.Close();
                if (reqStatus == true)
                {
                    return Ok(new { message = "Details have been successfully saved" });
                }
                else
                {
                    return BadRequest(new { message = "An error occurred while trying to process your request" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }


        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult SavePL_LPO_Settings(LPOSettings recvdata)
        {

            try
            {
                if (string.IsNullOrEmpty(recvdata.LPO_SPrefix))
                {
                    return BadRequest(new { message = "Missing required L.P.O prefix" });

                }
                else if (string.IsNullOrEmpty(recvdata.LPO_StartNO.ToString()))
                {
                    return BadRequest(new { message = "Missing required L.P.O strat number" });
                }
                else if (string.IsNullOrEmpty(recvdata.LPO_NumberingType))
                {
                    return BadRequest(new { message = "Missing required L.P.O numbering type " });
                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                //permission name
                string permissionName = Request.Headers["PermName"];

                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }

                string jwtHeader = authHeader.Split(' ')[1];

                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                if (perStatus == false)
                {
                    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                }

                //get database name
                string db = companyRes;

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //check if table has row
                bool hasData = false;
                cnn.Open();
                NpgsqlDataReader sdrL = new NpgsqlCommand("SELECT * FROM \"LPOSettings\" ", cnn).ExecuteReader();
                if (sdrL.HasRows == true)
                {
                    hasData = true;
                }
                cnn.Close();

                if (hasData == false)
                {
                    //create new
                    cnn.Open();
                    string myQuery = "INSERT INTO \"LPOSettings\" (\"LPO_SID\",\"LPO_SPrefix\",\"LPO_StartNO\",\"LPO_NumberingType\") VALUES ( 1, '" + recvdata.LPO_SPrefix + "', " + recvdata.LPO_StartNO + ", '" + recvdata.LPO_NumberingType + "' ) ";
                    bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
                    cnn.Close();
                    if (reqStatus == false)
                    {
                        return BadRequest(new { message = "An error occurred while trying to process your request" });
                    }

                }
                else
                {
                    //update
                    cnn.Open();
                    string myQuery = "UPDATE \"LPOSettings\" SET \"LPO_SPrefix\" = '" + recvdata.LPO_SPrefix + "', \"LPO_StartNO\" = " + recvdata.LPO_StartNO + ", \"LPO_NumberingType\" = '" + recvdata.LPO_NumberingType + "'  ";
                    bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
                    cnn.Close();
                    if (reqStatus == false)
                    {
                        return BadRequest(new { message = "An error occurred while trying to process your request" });

                    }

                }

                return Ok(new { message = "Details have been successfully saved" });


            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }


        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult AddINV_PL_CreditNote(int received_INVSJN, [FromBody] PLInvoice sendData)
        {
            try
            {
                if (received_INVSJN == 0)
                {
                    return BadRequest(new { message = "Missing or undefined required reference details" });
                }
                else if (string.IsNullOrEmpty(sendData.Additionals))
                {
                    return BadRequest(new { message = "Missing required CRN description" });
                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;


                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                //permission name
                string permissionName = Request.Headers["PermName"];

                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }

                string jwtHeader = authHeader.Split(' ')[1];

                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }
                NlService nlService = new NlService(tokenData);

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                if (perStatus == false)
                {
                    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                }

                //get database name
                string db = companyRes;

                //get staff branch
                int staff_branch = myDbconnection.GetStaffBranch(userId, db);
                if (staff_branch == 0)
                {
                    return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
                }

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //get last PLJrnNo
                int lastPLjrnlNo = 0;
                cnn.Open();
                NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"PLJrnlNo\") as sl From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdra.Read())
                {
                    lastPLjrnlNo = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
                }
                cnn.Close();

                //get last NLJrnNO
                int lastNLJRN = 0;
                cnn.Open();
                NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"PLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
                while (sdrb.Read())
                {
                    lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;

                }
                cnn.Close();

                //check if invoice has any receipt
                cnn.Open();
                NpgsqlDataReader sdr_ch_r = new NpgsqlCommand("Select * From \"PLReceipts\" WHERE \"pyInvRef\" = " + received_INVSJN + " ", cnn).ExecuteReader();
                if (sdr_ch_r.HasRows == true)
                {
                    return BadRequest(new { message = "A PL receipt already exists preventing a CreditNote to be issued." });
                }
                cnn.Close();



                cnn.Open();
                //get PL invoice details
                List<PLInvoiceDetails> pl_inv_details = new List<PLInvoiceDetails>();
                NpgsqlDataReader sdr_pld = new NpgsqlCommand("SELECT *  FROM \"PLInvoiceDetail\" WHERE \"PLJrnlNo\" = " + received_INVSJN + " ", cnn).ExecuteReader();
                if (sdr_pld.HasRows == false)
                {
                    return BadRequest(new { message = "An occurred while trying to validate details reference." });
                }
                while (sdr_pld.Read())
                {
                    PLInvoiceDetails pl = new PLInvoiceDetails();

                    pl.PLJrnlNo = sdr_pld["PLJrnlNo"] != DBNull.Value ? (int)sdr_pld["PLJrnlNo"] : 0;
                    pl.JrnlPLNo = sdr_pld["JrnlPLNo"] != DBNull.Value ? (int)sdr_pld["JrnlPLNo"] : 0;
                    pl.UnitPrice = sdr_pld["UnitPrice"] != DBNull.Value ? (decimal)sdr_pld["UnitPrice"] : 0;
                    pl.VatPerc = sdr_pld["VatPerc"] != DBNull.Value ? (string)sdr_pld["VatPerc"] : null;
                    pl.VatAmt = sdr_pld["VatAmt"] != DBNull.Value ? (decimal)sdr_pld["VatAmt"] : 0;
                    pl.ProdGroupCode = sdr_pld["ProdGroupCode"] != DBNull.Value ? (string)sdr_pld["ProdGroupCode"] : null;
                    pl.NLAccCode = sdr_pld["NLAccCode"] != DBNull.Value ? (string)sdr_pld["NLAccCode"] : null;
                    pl.StkDesc = sdr_pld["StkDesc"] != DBNull.Value ? (string)sdr_pld["StkDesc"] : null;
                    pl.UserID = sdr_pld["UserID"] != DBNull.Value ? (int)sdr_pld["UserID"] : 0;
                    pl.ProdQty = sdr_pld["ProdQty"] != DBNull.Value ? (int)sdr_pld["ProdQty"] : 0;
                    pl.DiscountAmt = sdr_pld["DiscountAmt"] != DBNull.Value ? (decimal)sdr_pld["DiscountAmt"] : 0;
                    pl.Total = sdr_pld["Total"] != DBNull.Value ? (decimal)sdr_pld["Total"] : 0;
                    pl.ProdId = sdr_pld["ProdId"] != DBNull.Value ? (int)sdr_pld["ProdId"] : 0;


                    pl_inv_details.Add(pl);

                }
                cnn.Close();

                int plDLlast = (lastPLjrnlNo + 1);
                int plDNlast = (lastNLJRN + 1);
                decimal CalcTotals = 0;

                //insert invoice negative details
                //if (pl_inv_details.Count > 0)
                //{
                //    for (int i = 0; i < pl_inv_details.Count; i++)
                //    {
                //        NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //        //get inventory item id
                //        cnn1.Open();
                //        Inventory inv = new Inventory();
                //        NpgsqlDataReader sdr_inv =
                //            new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + pl_inv_details[i].ProdId + "  ", cnn1)
                //                .ExecuteReader();
                //        if (sdr_inv.HasRows == false)
                //        {
                //            return BadRequest(new { message = "The Inventory Item reference could NOT be found." });
                //        }
                //        while (sdr_inv.Read())
                //        {
                //            inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                //            inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                //            inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                //        }
                //        cnn1.Close();


                //        //save details
                //        cnn1.Open();
                //        string insertQuery1 = "INSERT INTO \"PLInvoiceDetail\" (\"PLJrnlNo\", \"JrnlPLNo\", \"UnitPrice\", \"VatPerc\", \"VatAmt\", \"ProdGroupCode\", " +
                //            " \"NLAccCode\", \"StkDesc\", \"UserID\", \"ProdQty\",\"DiscountAmt\",\"Total\",\"ProdId\" ) VALUES(" + (plDLlast) + ", " + (plDNlast) + ", " + pl_inv_details[i].UnitPrice + ", " +
                //            " '" + pl_inv_details[i].VatPerc + "', " + pl_inv_details[i].VatAmt + ", '" + pl_inv_details[i].ProdGroupCode + "','" + pl_inv_details[i].NLAccCode + "','" + pl_inv_details[i].StkDesc + "', " +
                //            " " + userId + ", " + pl_inv_details[i].ProdQty + "," + pl_inv_details[i].DiscountAmt + ", " + pl_inv_details[i].Total + "," + pl_inv_details[i].ProdId + " ); ";
                //        bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                //        cnn1.Close();
                //        if (myReq1 == false)
                //        {
                //            //failed
                //            return BadRequest(new { message = "An occurred while trying to save invoice details." });
                //        }

                //        ///ask for more info
                //        //update inventory if goods
                //        //if (inv.InvtType == "GOODS")
                //        //{
                //        //    string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " + pl_inv_details[i].ProdQty + " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + pl_inv_details[i].ProdId + " ";
                //        //    bool myReq24 = myDbconnection.UpdateDelInsert(up_inv, db);
                //        //    if (myReq24 == false)
                //        //    {
                //        //        //failed
                //        //        return BadRequest(new
                //        //            { message = "An occurred while trying to process Inventory update requests." });
                //        //    }
                //        //}


                //        //Remove into warehouse summary
                //        ManageWarehouseSummary whs = new ManageWarehouseSummary();
                //        bool wh_req = whs.warehouse_summary_sl_pl(db, pl_inv_details[i].ProdId, pl_inv_details[i].ProdQty, userId, "Reversal");
                //        if (wh_req == false)
                //        {
                //            //failed
                //            return BadRequest(new
                //            { message = "An occurred while trying to register request to warehouse summary." });

                //        }


                //    }
                //}


                cnn.Open();
                decimal bal = sendData.Totals - sendData.CRNTotal;
                string myQuery = "UPDATE \"PLInvoiceHeader\" SET \"HasCreditNote\" = 'true', \"Additionals\" = '" + sendData.Additionals + "', \"CRNTotal\" = '"+ sendData.CRNTotal +"', \"CrnVatPercent\" = '"+ sendData.CrnVatPercent +"', \"CRNVat\" = '"+ sendData.CRNVat +"',\"Balance\" = \"Balance\" - '"+sendData.CRNTotal+"', \"CRNDate\" = '" + sendData.CRNDate + "', \"CRNReference\" = '"+ sendData.CRNReference +"' WHERE \"PLJrnlNo\" = " + received_INVSJN + "  ";
                bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
                cnn.Close();
                if (reqStatus == true)
                {
                    //string[] period = sendData.Period.Split('/');
                    //string month = period[0];
                    //string year = period[1];
                    NlJournalHeader nlJournalHeader = new NlJournalHeader();
                    nlJournalHeader.NlJrnlDesc = "CREDIT NOTE FOR INVP" + received_INVSJN;
                    nlJournalHeader.TranDate = DateTime.Now;
                    nlJournalHeader.MEndDate = DateTime.Now;
                    nlJournalHeader.TranYear = sendData.PeriodYear;
                    nlJournalHeader.TranPeriod = sendData.PeriodMonth;
                    nlJournalHeader.TranType = "";
                    nlJournalHeader.TranFrom = "";
                    nlJournalHeader.ModuleId = null;
                    nlJournalHeader.PlJrnlNo = received_INVSJN;
                    decimal inv_total = pl_inv_details.Sum(x => x.Total);
                    var response = nlService.savePurchaseCreditNoteJournal(nlJournalHeader, "PL-CRN", (float)sendData.CRNTotal, null);
                    if (response.Httpcode == 400)
                    {
                        return BadRequest(new { message = "An error occurred while trying to process your request" });
                    }
                    return Ok(new { message = "A credit note has been successfully created" });
                }
                else
                {
                    return BadRequest(new { message = "An error occurred while trying to process your request" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
                // Console.WriteLine(e);
                // throw;
            }
        }
        [Route("[action]")]
        [HttpGet]
        public ActionResult all_PL_creditnotes()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }


            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get purchases Invoices headers
            List<PLInvoice> plinvoicelist = new List<PLInvoice>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select \"PLInvoiceHeader\".*, \"Currencies\".\"CrCode\", \"PLCustomer\".\"PLCustCode\",\"CustName\" " +
                " From \"PLInvoiceHeader\" LEFT Join \"Currencies\" On \"Currencies\".\"CrId\" = \"PLInvoiceHeader\".\"CurrencyId\" " +
                "LEFT Join \"PLCustomer\" On \"PLCustomer\".\"CustID\" = \"PLInvoiceHeader\".\"PLCustID\" " +
                "WHERE \"HasCreditNote\" = 't' AND \"PLBranch\" = " + staff_branch + " ORDER BY \"PLInvoiceHeader\".\"CRNDate\" DESC ", cnn).ExecuteReader();

            while (sdr0.Read())
            {
                PLInvoice plInv = new PLInvoice();

                plInv.PLJrnNo = sdr0["PLJrnlNo"] != DBNull.Value ? (int)sdr0["PLJrnlNo"] : 0;
                plInv.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                plInv.PLCustID = sdr0["PLCustID"] != DBNull.Value ? (int)sdr0["PLCustID"] : 0;
                plInv.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                plInv.Period = sdr0["Period"] != DBNull.Value ? (string)sdr0["Period"] : null;
                plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (string)sdr0["DocRef"] : "";
                plInv.InvDate = sdr0["CRNDate"] != DBNull.Value ? (DateTime)sdr0["CRNDate"] : DateTime.Now;
                plInv.CurrencyId = sdr0["CurrencyId"] != DBNull.Value ? (int)sdr0["CurrencyId"] : 0;
                plInv.PLDescription = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : null;
                plInv.StaffId = sdr0["StaffId"] != DBNull.Value ? (int)sdr0["StaffId"] : 0;
                plInv.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                plInv.HasCreditNote = sdr0["HasCreditNote"] != DBNull.Value ? (bool)sdr0["HasCreditNote"] : false;
                plInv.DueDate = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                plInv.Totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                plInv.Balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;
                plInv.Additionals = sdr0["Additionals"] != DBNull.Value ? (string)sdr0["Additionals"] : null;
                plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                plInv.CRNTotal = sdr0["CRNTotal"] != DBNull.Value ? (decimal)sdr0["CRNTotal"] : 0;
                plInv.CRNReference = sdr0["CRNReference"] != DBNull.Value ? (string)sdr0["CRNReference"] : "";
                plinvoicelist.Add(plInv);
            }
            cnn.Close();
            return Ok(new { PLInvoices = plinvoicelist });
        }
        [Route("[action]")]
        [HttpGet]
        public ActionResult LPO_ListAll()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }



            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get lpo headers
            List<LpoHeader> plinvoicelist = new List<LpoHeader>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"LPOHeader\".*, \"CrCode\", \"PLCustCode\", \"CustName\" FROM \"LPOHeader\" " +
                "LEFT JOIN \"Currencies\"  ON \"CrId\" = \"CurrencyID\" " +
                "LEFT JOIN  \"PLCustomer\" ON \"CustID\" = \"LPOCustID\" " +
                " WHERE \"LPOBranch\" = " + staff_branch + "  ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                LpoHeader plInv = new LpoHeader
                {
                    LID = sdr0["LID"] != DBNull.Value ? (int)sdr0["LID"] : 0,
                    LPOCustID = sdr0["LPOCustID"] != DBNull.Value ? (int)sdr0["LPOCustID"] : 0,
                    LPODate = sdr0["LPODate"] != DBNull.Value ? (DateTime)sdr0["LPODate"] : DateTime.Today,
                    TransDate = sdr0["TransDate"] != DBNull.Value ? (DateTime)sdr0["TransDate"] : DateTime.Today,
                    Prefix = sdr0["Prefix"] != DBNull.Value ? (string)sdr0["Prefix"] : null,
                    DocRef = sdr0["DocRef"] != DBNull.Value ? (int)sdr0["DocRef"] : 0,
                    CurrencyID = sdr0["CurrencyID"] != DBNull.Value ? (int)sdr0["CurrencyID"] : 0,
                    LDescription = sdr0["LDescription"] != DBNull.Value ? (string)sdr0["LDescription"] : null,
                    StaffID = sdr0["StaffID"] != DBNull.Value ? (int)sdr0["StaffID"] : 0,
                    Totals = sdr0["LID"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0,
                    Invoiced = (bool)sdr0["Invoiced"],

                    CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null,

                    PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null,
                    CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null

                };

                plinvoicelist.Add(plInv);
            }
            cnn.Close();


            return Ok(new { LPOheaderDetails = plinvoicelist });

        }
        [Route("[action]")]
        [HttpGet]
        public ActionResult LPO_New_Get_Default()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //get inventory data
            List<Inventory> invList = new List<Inventory>();
            cnn.Open();
            NpgsqlDataReader sdrInv = new NpgsqlCommand("SELECT \"Inventory\".*,\"VtRef\",\"VtPerc\" FROM \"Inventory\" LEFT JOIN \"VATs\" ON (\"VtId\" = \"InvtVATId\") ", cnn).ExecuteReader();
            while (sdrInv.Read())
            {
                Inventory inv = new Inventory();

                inv.InvtId = sdrInv["InvtId"] != DBNull.Value ? (int)sdrInv["InvtId"] : 0;
                inv.InvtType = sdrInv["InvtType"] != DBNull.Value ? (string)sdrInv["InvtType"] : null;
                inv.InvtName = sdrInv["InvtName"] != DBNull.Value ? (string)sdrInv["InvtName"] : null;
                inv.InvtQty = sdrInv["InvtQty"] != DBNull.Value ? (int)sdrInv["InvtQty"] : 0;
                inv.InvtBP = sdrInv["InvtBP"] != DBNull.Value ? (decimal)sdrInv["InvtBP"] : 0;
                inv.InvtSP = sdrInv["InvtSP"] != DBNull.Value ? (decimal)sdrInv["InvtSP"] : 0;
                inv.InvtReorderLevel = sdrInv["InvtReorderLevel"] != DBNull.Value ? (int)sdrInv["InvtReorderLevel"] : 0;
                inv.InvtVATId = sdrInv["InvtVATId"] != DBNull.Value ? (int)sdrInv["InvtVATId"] : 0;

                inv.VATPerc = sdrInv["VtPerc"] != DBNull.Value ? (float)sdrInv["VtPerc"] : 0;
                inv.VATRef = sdrInv["VtRef"] != DBNull.Value ? (string)sdrInv["VtRef"] : null;

                invList.Add(inv);
            }
            cnn.Close();

            //get currencies
            List<Currencies> currencyList = new List<Currencies>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"Currencies\" ", cnn).ExecuteReader();

            while (sdr1.Read())
            {
                Currencies cr = new Currencies();

                cr.CrId = sdr1["CrId"] != DBNull.Value ? (int)sdr1["CrId"] : 0;
                cr.CrName = sdr1["CrName"] != DBNull.Value ? (string)sdr1["CrName"] : null;
                cr.CrCode = sdr1["CrCode"] != DBNull.Value ? (string)sdr1["CrCode"] : null;

                currencyList.Add(cr);
            }
            cnn.Close();


            //get PLcstomers
            List<PLCustomer> plcustomerlist = new List<PLCustomer>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*, \"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" ", cnn).ExecuteReader();

            while (sdr0.Read())
            {
                PLCustomer plCust = new PLCustomer();

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
                plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;

                plcustomerlist.Add(plCust);
            }
            cnn.Close();

            //get last LPO number
            int lastLPONumber = 0;
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT COALESCE(MAX(\"LID\"),0) as st FROM \"LPOHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                lastLPONumber = (int)sdr3["st"];
            }
            cnn.Close();

            //Get LPO Settings
            LPOSettings lposettings = new LPOSettings();
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("Select *  FROM \"LPOSettings\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdr2.Read())
            {
                lposettings.LPO_SID = sdr2["LPO_SID"] != DBNull.Value ? (int)sdr2["LPO_SID"] : 0;
                lposettings.LPO_SPrefix = sdr2["LPO_SPrefix"] != DBNull.Value ? (string)sdr2["LPO_SPrefix"] : null;
                lposettings.LPO_StartNO = sdr2["LPO_StartNO"] != DBNull.Value ? (int)sdr2["LPO_StartNO"] : 0;
                lposettings.LPO_NumberingType = sdr2["LPO_NumberingType"] != DBNull.Value ? (string)sdr2["LPO_NumberingType"] : null;
            }
            cnn.Close();

            //check if the LPO start numbe ris greater than the current count
            if (lposettings.LPO_StartNO <= lastLPONumber)
            {
                lposettings.LPO_StartNO = lastLPONumber;
            }

            return Ok(new { LPOConfigs = lposettings, InventData = invList, CurrencyData = currencyList, PLCustomerData = plcustomerlist });

        }
        [Route("[action]")]
        [HttpPost]
        public ActionResult LPO_AddNew(LpoHeader lpo_Data)
        {
            //check data
            if (string.IsNullOrEmpty(lpo_Data.LPOCustID.ToString()) || lpo_Data.LPOCustID == 0)
            {
                return BadRequest(new { message = "Missing or undefined L.P.O customer" });

            }
            else if (string.IsNullOrEmpty(lpo_Data.LPODate.ToString()))
            {
                return BadRequest(new { message = "Missing LPO date" });

            }
            else if (string.IsNullOrEmpty(lpo_Data.Prefix))
            {
                return BadRequest(new { message = "Missing LPO prefix" });

            }
            else if (string.IsNullOrEmpty(lpo_Data.DocRef.ToString()))
            {
                return BadRequest(new { message = "Missing LPO Document Reference" });

            }
            else if (lpo_Data.lpo_Details.Length == 0)
            {
                return BadRequest(new { message = "No details have been attached to the LPO. Please add details to save" });

            }

            //set Date
            DateTime today = DateTime.Today;


            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];

            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            string jwtHeader = authHeader.Split(' ')[1];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }


            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //get last LPOID
            int last_LPO_ID = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"LID\") as sl From \"LPOHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_LPO_ID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            int currentId = last_LPO_ID + 1;



            //get customer data currency
            cnn.Open();
            PLCustomer plCust = new PLCustomer();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*,\"Currencies\".\"CrId\",\"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" WHERE \"CustID\" = " + lpo_Data.LPOCustID + " ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                return BadRequest(new { message = "An occured while trying to save invoice details. plcustomer doesnt exists" });
            }
            if (sdr0.Read())
            {
                /// GET PL CUSTOMER DATA
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
                plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plCust.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
            }
            cnn.Close();


            decimal CalcTotals = 0;

            //insert invoice details
            if (lpo_Data.lpo_Details.Length > 0)
            {
                for (int i = 0; i < lpo_Data.lpo_Details.Length; i++)
                {
                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));

                    cnn1.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtId\" = " + lpo_Data.lpo_Details[i].StkDesc + " ", cnn1).ExecuteReader();
                    if (sdr.HasRows == false)
                    {
                        return BadRequest(new { message = "Sorry! No item was found matching passed reference." });
                    }
                    while (sdr.Read())
                    {
                        inv.InvtName = (string)sdr["InvtName"];
                    }
                    cnn.Close();

                    CalcTotals += lpo_Data.lpo_Details[i].Total;
                    //justo
                    //   string insertQuery1 = "INSERT INTO \"LPODetails\" (\"PldID\", \"PldRef\", \"VatPerc\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ProdQty\", \"Total\", \"UnitPrice\", \"PldDate\") VALUES(" + (last_LPO_ID + 1) + ", " + (lpo_Data.DocRef+1) + ", '" + lpo_Data.lpo_Details[i].VatPerc + "', " + lpo_Data.lpo_Details[i].VatAmt + ", '" + inv.InvtName + "', " + userId + "," + lpo_Data.lpo_Details[i].ProdQty + "," + lpo_Data.lpo_Details[i].Total + "," + lpo_Data.lpo_Details[i].UnitPrice + ", '" + lpo_Data.LPODate + "' ); ";

                    string insertQuery1 = "INSERT INTO \"LPODetails\" (\"PldID\", \"PldRef\", \"VatPerc\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ProdQty\", \"Total\", \"UnitPrice\", \"PldDate\",\"Details\") VALUES(" + (last_LPO_ID + 1) + ", " + (currentId) + ", '" + lpo_Data.lpo_Details[i].VatPerc + "', " + lpo_Data.lpo_Details[i].VatAmt + ", '" + inv.InvtName + "', " + userId + "," + lpo_Data.lpo_Details[i].ProdQty + "," + lpo_Data.lpo_Details[i].Total + "," + lpo_Data.lpo_Details[i].UnitPrice + ", '" + lpo_Data.LPODate + "','" + lpo_Data.lpo_Details[i].StkInfo + "' ); ";
                    bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);

                    cnn1.Close();

                    if (myReq1 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occured while trying to save invoice details." });
                    }

                }
            }
            //set LPO header details
            cnn.Open();
            string insertQ = "INSERT INTO \"LPOHeader\" (\"LID\", \"LPOCustID\", \"LPODate\", \"TransDate\", \"Prefix\", \"DocRef\", \"CurrencyID\", \"LDescription\", \"StaffID\", \"Totals\",\"Invoiced\", \"LPOBranch\")  " +
                "VALUES(" + (last_LPO_ID + 1) + ", " + lpo_Data.LPOCustID + ", '" + lpo_Data.LPODate + "', '" + DateTime.Today + "' ,'" + lpo_Data.Prefix + "'," + (lpo_Data.DocRef + 1) + "," + plCust.CrId + ",'" + lpo_Data.LDescription + "'," + userId + "," + CalcTotals + ", 'f', " + staff_branch + " ); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to save invoice details." });
            }
            //success
            return Ok(new { message = "L.P.O Request has been successfully processed" });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult LPO_View_Report(int lpoRef)
        {
            if (lpoRef == 0)
            {
                return BadRequest(new { message = "Cannot find the required reference." });
            }
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get LPO Header
            LpoHeader plInv = new LpoHeader();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"LPOHeader\".*, \"PLCustomer\".*, \"CrCode\", \"PLCustCode\", \"CustName\",\"UFirstName\",\"ULastName\" FROM \"LPOHeader\" LEFT JOIN \"Currencies\"  ON \"CrId\" = \"CurrencyID\" LEFT JOIN  \"PLCustomer\" ON \"CustID\" = \"LPOCustID\" LEFT JOIN  \"Users\" ON \"UId\" = \"LPOHeader\".\"StaffID\"  WHERE \"LPOHeader\".\"LID\" = " + lpoRef + " ", cnn).ExecuteReader();
            if (sdr0.Read())
            {

                plInv.LID = sdr0["LID"] != DBNull.Value ? (int)sdr0["LID"] : 0;
                plInv.LPOCustID = sdr0["LPOCustID"] != DBNull.Value ? (int)sdr0["LPOCustID"] : 0;
                plInv.LPODate = sdr0["LPODate"] != DBNull.Value ? (DateTime)sdr0["LPODate"] : DateTime.Today;
                plInv.TransDate = sdr0["TransDate"] != DBNull.Value ? (DateTime)sdr0["TransDate"] : DateTime.Today;
                plInv.Prefix = sdr0["Prefix"] != DBNull.Value ? (string)sdr0["Prefix"] : null;
                plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (int)sdr0["DocRef"] : 0;
                plInv.CurrencyID = sdr0["CurrencyID"] != DBNull.Value ? (int)sdr0["CurrencyID"] : 0;
                plInv.LDescription = sdr0["LDescription"] != DBNull.Value ? (string)sdr0["LDescription"] : null;
                plInv.Totals = sdr0["LID"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                plInv.Custtelephone = sdr0["ContactPhone"] != DBNull.Value ? (string)sdr0["ContactPhone"] : "";
                plInv.custaddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : "";
                plInv.custpostal = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : "";
                plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                plInv.UFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                plInv.ULastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;
            }
            cnn.Close();
            cnn.Open();
            List<LpoDetails> myLPODetails = new List<LpoDetails>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"LPODetails\" WHERE \"PldRef\" = " + lpoRef + "  ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                LpoDetails lpdet = new LpoDetails();
                lpdet.PldID = sdr1["PldID"] != DBNull.Value ? (int)sdr1["PldID"] : 0;
                lpdet.PldRef = sdr1["PldRef"] != DBNull.Value ? (int)sdr1["PldRef"] : 0;
                lpdet.VatPerc = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : null;
                lpdet.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                lpdet.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : null;
                lpdet.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                lpdet.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;
                lpdet.UnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;
                lpdet.StkInfo = sdr1["Details"] != DBNull.Value ? (string)sdr1["Details"] : "";
                myLPODetails.Add(lpdet);
            }
            cnn.Close();
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();
            string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
            string full_imgPath = "";
            if (String.IsNullOrEmpty(lic.CompanyLogo))
            {
                full_imgPath = Path.Combine(img_path, "invoice_default.jpg");
            }
            else
            {
                full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
            }

            byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            lic.CompanyLogo = base64ImageRepresentation;

            //get LPO terms
            Allterms invterms = new Allterms();
            cnn.Open();
            NpgsqlDataReader sdr12 = new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'pl_inv_terms' ", cnn).ExecuteReader();
            while (sdr12.Read())
            {
                invterms.tosID = sdr12["tosID"] != DBNull.Value ? (int)sdr12["tosID"] : 0;
                invterms.tosType = sdr12["tosType"] != DBNull.Value ? (string)sdr12["tosType"] : null;
                invterms.terms = sdr12["terms"] != DBNull.Value ? sdr12["terms"].ToString() : null;
            }
            cnn.Close();
            // Decode the content for showing on Web page.
            invterms.terms = WebUtility.HtmlDecode(invterms.terms);

            List<PurchaseHeaderSettings> list = new List<PurchaseHeaderSettings>();
            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand("SELECT * FROM \"Document_header\" WHERE \"Category\" = 'PURCHASE' ORDER BY id ASC; ", cnn).ExecuteReader();
            while (reader1.Read())
            {
                PurchaseHeaderSettings head = new PurchaseHeaderSettings();
                head.Id = reader1["id"] != DBNull.Value ? (int)reader1["id"] : 0;
                head.Category = reader1["Category"] != DBNull.Value ? (string)reader1["Category"] : null;
                head.DocumentName = reader1["DocumentName"] != DBNull.Value ? (string)reader1["DocumentName"] : null;
                head.Status = reader1["Status"] != DBNull.Value ? (bool)reader1["Status"] : false;
                list.Add(head);
            }
            cnn.Close();

            return Ok(new { LPOReportHeader = plInv, LPOBodyDetails = myLPODetails, myCompany = lic, LPOTerms = invterms, headerSettings = list });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseReceipt_All()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get PR headers
            List<PLReceipts> PR_list = new List<PLReceipts>();
            cnn.Open();
            string query = "  SELECT \"a\".\"pr_id\",\"d\".\"CrCode\",\"a\".\"pr_total\", \"a\".\"pr_prefix\",\"a\".pr_date,\"b\".\"UFirstName\",\"b\".\"ULastName\",d.\"CrCode\",\"c\".\"CustName\",\"a\".\"pr_additional\" " +
                "from \"purchase_receipt_header\" a " +
                "LEFT JOIN \"Users\" b on \"b\".\"UId\" = \"a\".\"pr_user\" " +
                "LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = a.\"pr_customer\" " +
                "LEFT JOIN \"Currencies\" d on \"d\".\"CrId\" = \"c\".CurrID";
            string query2 = " SELECT a.\"pyAdditionalDetails\",a.\"pyPayable\",a.\"pyID\",a.\"pyPayable\",a.\"pyDate\" ,b.\"CustName\" , e.\"CrCode\"  FROM \"PLReceipts\" a  " +
                "LEFT JOIN \"PLInvoiceHeader\" d ON(d.\"PLJrnlNo\" = a.\"pyInvRef\")" +
                " LEFT JOIN \"PLCustomer\" b ON(b.\"CustID\" = d.\"PLCustID\")" +
                "LEFT JOIN \"Currencies\" e on e.\"CrId\" = b.\"CurrID\"   ";

            string query3 = " Select \"PLReceipts\".*, \"PLInvoiceHeader\".\"DocRef\", \"PLInvoiceHeader\".\"PLJrnlNo\", \"PLInvoiceHeader\".\"DocPrefix\", \"Currencies\".\"CrCode\", \"PLCustomer\".\"CustName\", " +
                "\"PLCustomer\".\"PhysicalAddress\"" +
                " From \"PLReceipts\" Inner Join \"PLInvoiceHeader\" On \"PLInvoiceHeader\".\"PLJrnlNo\" = \"PLReceipts\".\"pyInvRef\" Inner Join \"PLCustomer\" On \"PLInvoiceHeader\".\"PLCustID\" = \"PLCustomer\".\"CustID\" " +
                "Inner Join \"Currencies\" On \"Currencies\".\"CrId\" = \"PLInvoiceHeader\".\"CurrencyId\" ";

            string query4 = "Select \"PLReceipts\".*, \"PLInvoiceHeader\".\"DocRef\", \"PLInvoiceHeader\".\"PLJrnlNo\", \"PLInvoiceHeader\".\"DocPrefix\", \"Currencies\".\"CrCode\", \"PLCustomer\".\"CustName\", \"PLCustomer\".\"PhysicalAddress\", \"PLCustomer\".\"OpeningBalance\"  " +
                " From \"PLReceipts\" LEFT JOIN \"PLReceiptsDetails\" on \"PLReceiptsDetails\".receipt_id = \"PLReceipts\".\"pyID\" LEFT Join \"PLInvoiceHeader\" On \"PLInvoiceHeader\".\"PLJrnlNo\" = \"PLReceiptsDetails\".invoice_id " +
                " LEFT Join \"PLCustomer\" On \"PLCustomer\".\"CustID\"  = \"PLReceipts\".supplier_id LEFT Join \"Currencies\" On \"Currencies\".\"CrId\" = \"PLInvoiceHeader\".\"CurrencyId\" ";

            NpgsqlDataReader sdr0 = new NpgsqlCommand(query4, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                PLReceipts PR = new PLReceipts();
                PR.pyID = sdr0["pyID"] != DBNull.Value ? (int)sdr0["pyID"] : 0;
                PR.pyAdditionalDetails = (string)sdr0["pyAdditionalDetails"];
                PR.pyDate = (DateTime)sdr0["pyDate"];
                PR.pyPaid = sdr0["pyPaid"] != DBNull.Value ? (float)sdr0["pyPaid"] : 0;
                PR.pyPayable = sdr0["pyPayable"] != DBNull.Value ? (float)sdr0["pyPayable"] : 0;
                PR.pyRef = sdr0["pyRef"] != DBNull.Value ? (int)sdr0["pyRef"] : 0;
                PR.pyBalance = sdr0["pyBalance"] != DBNull.Value ? (float)sdr0["pyBalance"] : 0;
                PR.pyMode = sdr0["pyMode"] != DBNull.Value ? (string)sdr0["pyMode"] : "";
                PR.PLCustomer = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : "";
                PR.pyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : "";
                PR.pyPrefix = "PYT";
                PR.pyCurr = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : "";
                PR.SupplierID = sdr0["supplier_id"] != DBNull.Value ? (int)sdr0["supplier_id"] : 0;
                PR.rate = sdr0["rate"] != DBNull.Value ? (decimal)sdr0["rate"] : 0;
                PR.AllocationRemainder = sdr0["allocation_remainder"] != DBNull.Value ? (decimal)sdr0["allocation_remainder"] : 0;
                PR.OpeningBalance = sdr0["OpeningBalance"] != DBNull.Value ? (decimal)sdr0["OpeningBalance"] : 0;
                PR_list.Add(PR);
            }
            cnn.Close();
            return Ok(new { Receipts = PR_list });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseReceiptPerId(int parseRef)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            PLReceipts rcpDetail = new PLReceipts();
            cnn.Open();
            string query = "Select \"PLReceipts\".*, \"PLInvoiceHeader\".\"DocRef\", \"PLInvoiceHeader\".\"PLJrnlNo\", \"PLInvoiceHeader\".\"DocPrefix\", \"Currencies\".\"CrCode\", \"PLCustomer\".\"CustName\", \"PLCustomer\".\"PhysicalAddress\", \"Users\".\"UFirstName\" As UFirstName " +
                "From \"PLReceipts\" LEFT JOIN \"PLReceiptsDetails\" on \"PLReceiptsDetails\".\"receipt_id\" = \"PLReceipts\".\"pyID\" LEFT Join \"PLInvoiceHeader\" On \"PLInvoiceHeader\".\"PLJrnlNo\" = \"PLReceiptsDetails\".\"invoice_id\" " +
                "LEFT Join \"PLCustomer\" On \"PLInvoiceHeader\".\"PLCustID\" = \"PLCustomer\".\"CustID\" LEFT Join \"Currencies\" On \"Currencies\".\"CrId\" = \"PLInvoiceHeader\".\"CurrencyId\" LEFT Join \"Users\" On \"Users\".\"UId\" = \"PLInvoiceHeader\".\"StaffId\" " +
                "WHERE \"PLReceipts\".\"pyID\"= "+ parseRef +"  ";

            NpgsqlDataReader sdr3 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr3.Read())
            {
                rcpDetail.pyID = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyRef = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyDate = sdr3["pyDate"] != DBNull.Value ? (DateTime)sdr3["pyDate"] : DateTime.Now;
                rcpDetail.pyInvRef = sdr3["pyInvRef"] != DBNull.Value ? (int)sdr3["pyInvRef"] : 0;
                rcpDetail.pyPayable = sdr3["pyPayable"] != DBNull.Value ? (float)sdr3["pyPayable"] : 0;
                rcpDetail.pyPaid = sdr3["pyPaid"] != DBNull.Value ? (float)sdr3["pyPaid"] : 0;
                rcpDetail.pyBalance = sdr3["pyBalance"] != DBNull.Value ? (float)sdr3["pyBalance"] : 0;
                rcpDetail.pyMode = sdr3["pyMode"] != DBNull.Value ? (string)sdr3["pyMode"] : null;
                rcpDetail.pyChequeNumber = sdr3["pyChequeNumber"] != DBNull.Value ? (string)sdr3["pyChequeNumber"] : null;
                rcpDetail.pyAdditionalDetails = sdr3["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3["pyAdditionalDetails"] : null;
                rcpDetail.pyProcessDate = sdr3["pyProcessDate"] != DBNull.Value ? (DateTime)sdr3["pyProcessDate"] : DateTime.Now;
                rcpDetail.pyUser = sdr3["pyUser"] != DBNull.Value ? (int)sdr3["pyUser"] : 0;
                rcpDetail.DocRef = sdr3["DocRef"] != DBNull.Value ? (string)sdr3["DocRef"] : null;
                rcpDetail.PLJrnlNo = sdr3["PLJrnlNo"] != DBNull.Value ? (int)sdr3["PLJrnlNo"] : 0;
                rcpDetail.DocPrefix = sdr3["DocPrefix"] != DBNull.Value ? (string)sdr3["DocPrefix"] : null;
                rcpDetail.CrCode = sdr3["CrCode"] != DBNull.Value ? (string)sdr3["CrCode"] : null;
                rcpDetail.CustName = sdr3["CustName"] != DBNull.Value ? (string)sdr3["CustName"] : null;
                rcpDetail.PhysicalAddress = sdr3["PhysicalAddress"] != DBNull.Value ? (string)sdr3["PhysicalAddress"] : null;
                rcpDetail.UFirstName = sdr3["UFirstName"] != DBNull.Value ? (string)sdr3["UFirstName"] : null;
                rcpDetail.SupplierID = sdr3["supplier_id"] != DBNull.Value ? (int)sdr3["supplier_id"] : 0;
            }
            cnn.Close();
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr1.Read())
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
            return Ok(new { PRHeader = rcpDetail, MyCompany = lic, Code = 200});
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseAllocationPerId(int prcRef)
        {
            if (prcRef == 0)
            {
                return BadRequest(new { message = "Cannot find parsed required details." });
            }
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];

            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            string query = "select a.*, b.\"InvDate\", b.\"Totals\", b.\"DueDate\", b.\"DocRef\", b.\"DocPrefix\", b.\"PLJrnlNo\", c.\"CustName\", c.\"PostalAddress\", c.\"PhysicalAddress\", d.\"CrCode\", e.\"UFirstName\"" +
                " from \"PLReceipts\" a LEFT JOIN \"PLInvoiceHeader\" b on b.\"PLJrnlNo\" = a.\"pyInvRef\"" +
                " LEFT JOIN \"PLCustomer\" c on c.\"CustID\" = b.\"PLCustID\"" +
                " LEFT JOIN \"Currencies\" d on d.\"CrId\" = c.\"CurrID\"" +
                " LEFT JOIN \"Users\" e on e.\"UId\" = b.\"StaffId\"" +
                " WHERE a.\"pyRef\" = '"+ prcRef + "' ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            PLReceipts pr = new PLReceipts();
            if (sdr0.Read())
            {
                pr.pyID = sdr0["pyID"] != DBNull.Value ? (int)sdr0["pyID"] : 0;
                //pr.cust_id = sdr0["cust_id"] != DBNull.Value ? (int)sdr0["cust_id"] : 0;
                pr.pyRef = sdr0["pyRef"] != DBNull.Value ? (int)sdr0["pyID"] : 0;
                pr.pyDate = sdr0["pyDate"] != DBNull.Value ? (DateTime)sdr0["pyDate"] : DateTime.Now;
                pr.pyInvRef = sdr0["pyInvRef"] != DBNull.Value ? (int)sdr0["pyInvRef"] : 0;
                pr.pyPayable = sdr0["pyPayable"] != DBNull.Value ? (float)sdr0["pyPayable"] : 0;
                pr.pyPaid = sdr0["pyPaid"] != DBNull.Value ? (float)sdr0["pyPaid"] : 0;
                pr.pyBalance = sdr0["pyBalance"] != DBNull.Value ? (float)sdr0["pyBalance"] : 0;
                pr.pyMode = sdr0["pyMode"] != DBNull.Value ? (string)sdr0["pyMode"] : null;
                pr.pyChequeNumber = sdr0["pyChequeNumber"] != DBNull.Value ? (string)sdr0["pyChequeNumber"] : null;
                //pr.pyReceivedFrom = sdr0["pyReceivedFrom"] != DBNull.Value ? (string)sdr0["pyReceivedFrom"] : null;
                pr.pyAdditionalDetails = sdr0["pyAdditionalDetails"] != DBNull.Value ? (string)sdr0["pyAdditionalDetails"] : null;
                pr.pyProcessDate = sdr0["pyProcessDate"] != DBNull.Value ? (DateTime)sdr0["pyProcessDate"] : DateTime.Now;
                pr.pyUser = sdr0["pyUser"] != DBNull.Value ? (int)sdr0["pyUser"] : 0;
                //customer details
                //pr.CustFirstName = sdr0["CustFirstName"] != DBNull.Value ? (string)sdr0["CustFirstName"] : null;
                //pr.CustLastName = sdr0["CustLastName"] != DBNull.Value ? (string)sdr0["CustLastName"] : null;
                //pr.CustEmail = sdr0["CustEmail"] != DBNull.Value ? (string)sdr0["CustEmail"] : null;
                //pr.CustCompany = sdr0["CustCompany"] != DBNull.Value ? (string)sdr0["CustCompany"] : null;
                //pr.CustType = sdr0["CustType"] != DBNull.Value ? (string)sdr0["CustType"] : null;
                //pr.CustAddress = sdr0["Address"] != DBNull.Value ? (string)sdr0["Address"] : null;
                //pr.currentCustName = sdr0["currentCustName"] != DBNull.Value ? (string)sdr0["currentCustName"] : null;
                ////currency
                //pr.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                ////invoice
                //pr.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                //pr.DocRef = sdr0["DocRef"] != DBNull.Value ? (int)sdr0["DocRef"] : 0;
                //pr.SLJrnlNo = (int)sdr0["SLJrnlNo"];
                //pr.invDate = (DateTime)sdr0["INVDate"];
                //pr.invAmount = sdr0["TotalAmount"] != DBNull.Value ? (decimal)sdr0["TotalAmount"] : 0;
                //pr.dueDate = (DateTime)sdr0["DueDate"];
                //pr.paymentdays = (int)sdr0["PaymentDays"];
                //pr.fname = (string)sdr0["Fname"];
            }
            cnn.Close();
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();
            return Ok(new { Receipts = pr, companydata = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseReturn_view(int parsed_pljrnl)
        {
            if (parsed_pljrnl == 0)
            {
                return BadRequest(new { message = "Cannot find parsed required details." });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }



            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get PR headers
            PurchaseReturnHeader PR = new PurchaseReturnHeader();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select purchase_return_header.*, u_rt.\"UFirstName\", u_rt.\"ULastName\", sn_rt.sign_data, sn_rt.sign_name, u_app.\"UFirstName\" As app_fname, u_app.\"ULastName\" As app_lname, sn_app.sign_data As app_sndata, sn_app.sign_name As app_snname, \"PLInvoiceHeader\".*, \"PLInvoiceHeader\".\"CurrencyId\" As curr_id, \"Currencies\".\"CrCode\", \"PLCustomer\".\"PLCustCode\", \"PLCustomer\".\"CustName\" From purchase_return_header Inner Join \"Users\" u_rt On u_rt.\"UId\" = purchase_return_header.returnedby Left Join signatures sn_rt On sn_rt.sign_id = purchase_return_header.returner_signature Left Join \"Users\" u_app On u_app.\"UId\" = purchase_return_header.approvedby Left Join signatures sn_app On sn_app.sign_id = purchase_return_header.approver_signature Left Join \"PLInvoiceHeader\" On \"PLInvoiceHeader\".\"PLJrnlNo\" = purchase_return_header.prh_pljrnl Left Join \"Currencies\" On \"Currencies\".\"CrId\" = \"PLInvoiceHeader\".\"CurrencyId\" Inner Join \"PLCustomer\" On \"PLCustomer\".\"CustID\" = \"PLInvoiceHeader\".\"PLCustID\" WHERE prh_pljrnl = " + parsed_pljrnl + " ", cnn).ExecuteReader();
            if (sdr0.Read())
            {
                PR.prh_ref = sdr0["prh_ref"] != DBNull.Value ? (string)sdr0["prh_ref"] : null;
                PR.prh_date = sdr0["prh_date"] != DBNull.Value ? (DateTime)sdr0["prh_date"] : DateTime.Today;
                PR.prh_pljrnl = sdr0["prh_pljrnl"] != DBNull.Value ? (int)sdr0["prh_pljrnl"] : 0;
                PR.returnedby = sdr0["returnedby"] != DBNull.Value ? (int)sdr0["returnedby"] : 0;
                PR.returner_signature = sdr0["returner_signature"] != DBNull.Value ? (int)sdr0["returner_signature"] : 0;
                PR.approvedby = sdr0["approvedby"] != DBNull.Value ? (int)sdr0["approvedby"] : 0;
                PR.approver_signature = sdr0["approver_signature"] != DBNull.Value ? (int)sdr0["approver_signature"] : 0;
                PR.status = sdr0["status"] != DBNull.Value ? (string)sdr0["status"] : null;
                PR.prh_staff = sdr0["prh_staff"] != DBNull.Value ? (int)sdr0["prh_staff"] : 0;

                PR.docref = sdr0["DocRef"] != DBNull.Value ? (int)sdr0["DocRef"] : 0;
                PR.docprefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                PR.InvDate = sdr0["InvDate"] != DBNull.Value ? (DateTime)sdr0["InvDate"] : DateTime.Today;
                PR.due_date = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                PR.totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                PR.balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;

                PR.Returner_firstname = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                PR.Returner_lastname = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;
                PR.Returner_signaturename = sdr0["sign_name"] != DBNull.Value ? (string)sdr0["sign_name"] : null;
                PR.Returner_signaturedata = sdr0["sign_data"] != DBNull.Value ? (string)sdr0["sign_data"] : null;

                PR.Approver_firstname = sdr0["app_fname"] != DBNull.Value ? (string)sdr0["app_fname"] : null;
                PR.Approver_lastname = sdr0["app_lname"] != DBNull.Value ? (string)sdr0["app_lname"] : null;
                PR.Approver_signaturename = sdr0["app_snname"] != DBNull.Value ? (string)sdr0["app_snname"] : null;
                PR.Approver_signaturedata = sdr0["app_sndata"] != DBNull.Value ? (string)sdr0["app_sndata"] : null;

                PR.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;

                PR.CustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                PR.InvCustomerName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;

            }
            cnn.Close();

            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();

            //get return details
            List<PurchaseReturnDetails> PRDedatils_list = new List<PurchaseReturnDetails>();
            cnn.Open();
            NpgsqlDataReader sdr31 = new NpgsqlCommand("Select * From purchase_return_details WHERE pr_pl_invref = " + parsed_pljrnl + " ", cnn).ExecuteReader();
            while (sdr31.Read())
            {
                PurchaseReturnDetails prt_det = new PurchaseReturnDetails();

                prt_det.pr_ref = sdr31["pr_ref"] != DBNull.Value ? (string)sdr31["pr_ref"] : null;
                prt_det.pr_pl_invref = sdr31["pr_pl_invref"] != DBNull.Value ? (int)sdr31["pr_pl_invref"] : 0;
                prt_det.pr_item_name = sdr31["pr_item_name"] != DBNull.Value ? (string)sdr31["pr_item_name"] : null;
                prt_det.pr_pl_invref = sdr31["pr_pl_invref"] != DBNull.Value ? (int)sdr31["pr_pl_invref"] : 0;
                prt_det.pr_item_qty = sdr31["pr_item_qty"] != DBNull.Value ? (int)sdr31["pr_item_qty"] : 0;
                prt_det.pr_reason = sdr31["pr_reason"] != DBNull.Value ? (string)sdr31["pr_reason"] : null;

                PRDedatils_list.Add(prt_det);

            }
            cnn.Close();

            //get signatures
            List<usersignature> users_Signatures = new List<usersignature>();
            cnn.Open();

            NpgsqlDataReader sdr10 = new NpgsqlCommand("SELECT * FROM signatures WHERE sign_user = " + userId + " ", cnn).ExecuteReader();
            while (sdr10.Read())
            {
                usersignature sgn = new usersignature();

                sgn.sign_id = sdr10["sign_id"] != DBNull.Value ? (int)sdr10["sign_id"] : 0;
                sgn.sign_date = sdr10["sign_date"] != DBNull.Value ? (DateTime)sdr10["sign_date"] : DateTime.Today;
                sgn.sign_user = sdr10["sign_user"] != DBNull.Value ? (int)sdr10["sign_user"] : 0;
                sgn.sign_data = sdr10["sign_data"] != DBNull.Value ? (string)sdr10["sign_data"] : null;
                sgn.sign_name = sdr10["sign_name"] != DBNull.Value ? (string)sdr10["sign_name"] : null;

                users_Signatures.Add(sgn);
            }
            cnn.Close();

            return Ok(new
            {
                PurchaseRHeader = PR,
                PurchaseRDetails = PRDedatils_list,
                MyCompany = lic,
                CurrUserSignatures = users_Signatures

            });

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseReceipt_New_Get_Default()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //get inventory data
            List<Inventory> invList = new List<Inventory>();
            cnn.Open();
            NpgsqlDataReader sdrInv = new NpgsqlCommand("SELECT \"Inventory\".*,\"VtRef\",\"VtPerc\" FROM \"Inventory\" LEFT JOIN \"VATs\" ON (\"VtId\" = \"InvtVATId\") ", cnn).ExecuteReader();
            while (sdrInv.Read())
            {
                Inventory inv = new Inventory();

                inv.InvtId = sdrInv["InvtId"] != DBNull.Value ? (int)sdrInv["InvtId"] : 0;
                inv.InvtType = sdrInv["InvtType"] != DBNull.Value ? (string)sdrInv["InvtType"] : null;
                inv.InvtName = sdrInv["InvtName"] != DBNull.Value ? (string)sdrInv["InvtName"] : null;
                inv.InvtQty = sdrInv["InvtQty"] != DBNull.Value ? (int)sdrInv["InvtQty"] : 0;
                inv.InvtBP = sdrInv["InvtBP"] != DBNull.Value ? (decimal)sdrInv["InvtBP"] : 0;
                inv.InvtSP = sdrInv["InvtSP"] != DBNull.Value ? (decimal)sdrInv["InvtSP"] : 0;
                inv.InvtReorderLevel = sdrInv["InvtReorderLevel"] != DBNull.Value ? (int)sdrInv["InvtReorderLevel"] : 0;
                inv.InvtVATId = sdrInv["InvtVATId"] != DBNull.Value ? (int)sdrInv["InvtVATId"] : 0;

                inv.VATPerc = sdrInv["VtPerc"] != DBNull.Value ? (float)sdrInv["VtPerc"] : 0;
                inv.VATRef = sdrInv["VtRef"] != DBNull.Value ? (string)sdrInv["VtRef"] : null;

                invList.Add(inv);
            }
            cnn.Close();


            //get PLcstomers
            List<PLCustomer> plcustomerlist = new List<PLCustomer>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*, \"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" ", cnn).ExecuteReader();

            while (sdr0.Read())
            {
                PLCustomer plCust = new PLCustomer();

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
                plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;

                plcustomerlist.Add(plCust);
            }
            cnn.Close();

            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
            }
            cnn.Close();
            return Ok(new { MyCompany = lic, InventData = invList, PLCustomerData = plcustomerlist });
        }

        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult PurchaseReceipt_Add_New(PurchaseReceipt recvdata)
        {

            try
            {
                if (recvdata.pr_customer == 0)
                {
                    return BadRequest(new { message = "Missing/Invalid required purchase receipt" });

                }
                else if (string.IsNullOrEmpty(recvdata.pr_date.ToString()))
                {
                    return BadRequest(new { message = "Missing required purchase receipt date" });
                }
                else if (recvdata.pr_Details.Length == 0)
                {
                    return BadRequest(new { message = "Missing purchase receipt list details" });
                }

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                //permission name
                string permissionName = Request.Headers["PermName"];

                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
                }

                string jwtHeader = authHeader.Split(' ')[1];

                //get token data
                jwt_token jwt = new jwt_token(_configuration);

                var tokenData = jwt.GetClaim(jwtHeader, "DB");
                userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
                if (tokenData != "")
                {
                    //assign company Ref from header
                    companyRes = tokenData;
                }

                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
                }

                //check user id
                if (userId == 0)
                {
                    return BadRequest(new { message = "Missing user reference. Page verification failed" });
                }

                //check permission
                bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                if (perStatus == false)
                {
                    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                }

                //get database name
                string db = companyRes;

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //get last LPOID
                int last_Receipt_ID = 0;
                cnn.Open();
                NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(pr_id) as sl From purchase_receipt_header LIMIT 1 ", cnn).ExecuteReader();
                while (sdra.Read())
                {
                    last_Receipt_ID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
                }
                cnn.Close();


                //get customer data currency
                cnn.Open();
                PLCustomer plCust = new PLCustomer();
                NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*,\"Currencies\".\"CrId\",\"Currencies\".\"CrCode\" FROM \"PLCustomer\" " +
                    "LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" WHERE \"CustID\" = " + recvdata.pr_customer + " ", cnn).ExecuteReader();
                if (sdr0.HasRows == false)
                {
                    return BadRequest(new { message = "An occured while trying to save invoice details." });
                }
                if (sdr0.Read())
                {
                    plCust.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                    plCust.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                    plCust.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                    plCust.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                    plCust.CurrID = sdr0["CurrID"] != DBNull.Value ? (int)sdr0["CurrID"] : 0;
                    plCust.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                    plCust.CustID = sdr0["CustID"] != DBNull.Value ? (int)sdr0["CustID"] : 0;

                    plCust.RegisterDate = sdr0["RegisterDate"] != DBNull.Value ? (DateTime)sdr0["RegisterDate"] : DateTime.Now;
                    plCust.StaffID = sdr0["StaffID"] != DBNull.Value ? (int)sdr0["StaffID"] : 0;
                    plCust.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                    plCust.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                    plCust.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
                }
                cnn.Close();


                decimal CalcTotals = 0;

                //insert invoice details
                string mainquery = "";

                if (recvdata.pr_Details.Length > 0)
                {
                    for (int i = 0; i < recvdata.pr_Details.Length; i++)
                    {
                        NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                        cnn1.Open();

                        CalcTotals += recvdata.pr_Details[i].pd_totals;

                        string insertQuery1 = "INSERT INTO purchase_receipt_details (pd_date, pd_ref, pd_item, pd_qty, pd_unitprice, pd_vat_perc, pd_vat_amt, pd_totals ) " +
                            "VALUES('" + DateTime.Today + "', " + (last_Receipt_ID + 1) + ", " + recvdata.pr_Details[i].pd_item + ", " + recvdata.pr_Details[i].pd_qty + ", " + recvdata.pr_Details[i].pd_unitprice + ",'" + recvdata.pr_Details[i].pd_vat_perc + "'," + recvdata.pr_Details[i].pd_vat_amt + "," + recvdata.pr_Details[i].pd_totals + " ); ";
                        mainquery.Concat(insertQuery1);
                        bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);

                        ///update stock
                        //ManageWarehouseSummary whs = new ManageWarehouseSummary();
                        //bool wh_req = whs.warehouse_summary_sl_pl(db, recvdata.pr_Details[i].pd_item, recvdata.pr_Details[i].pd_qty, userId, "Purchase");
                        //if (wh_req == false)
                        //{
                        //    _log.LogError("An occurred while trying to register request to warehouse summary.");
                        //    //failed
                        //    return BadRequest(new
                        //    { message = "An occurred while trying to register request to warehouse summary." });
                        //}
                        //bool inv_req = whs.updateinvoicefrompurchase(db, recvdata.pr_Details[i].pd_item, recvdata.pr_Details[i].pd_qty, "");
                        //if (inv_req == false)
                        //{
                        //    _log.LogError("An occurred while trying to update inventory data ");
                        //    //failed
                        //    return BadRequest(new
                        //    { message = "An occurred while trying to update inventory data ." });
                        //}

                        cnn1.Close();

                        if (myReq1 == false)
                        {
                            //failed
                            return BadRequest(new { message = "An occured while trying to save invoice details." });
                        }

                    }
                }


                //set LPO header details
                cnn.Open();
                string insertQ = "INSERT INTO purchase_receipt_header (pr_id, pr_date, pr_ref, pr_prefix, pr_customer, pr_user, pr_total, pr_currency, pr_additional, pr_invoiced, pr_transdate, pr_returned) VALUES(" + (last_Receipt_ID + 1) + ", '" + recvdata.pr_date + "', " + (last_Receipt_ID + 1) + ", 'PR' ," + recvdata.pr_customer + "," + userId + "," + CalcTotals + "," + plCust.CrId + ",'" + recvdata.pr_additional + "','f','" + DateTime.Today + "','f' ); ";

                bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);

                cnn.Close();

                if (myReq2 == false)
                {
                    //failed
                    return BadRequest(new { message = "An occured while trying to save invoice details." });
                }

                //success
                return Ok(new
                {
                    message = "Receipt Request has been successfully processed"

                });


            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }


        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult PurchaseReceipt_Delete(int rcpRef)
        {

            if (rcpRef == 0)
            {
                return BadRequest(new { message = "Cannot find the required receipt details" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            PurchaseReceipt PRCP = new PurchaseReceipt();

            //CHeck if receipt id exists
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM purchase_receipt_header WHERE pr_ref = " + rcpRef + " ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! Required details were NOT found. Operation failed." });
            }
            else
            {
                if (sdr0.Read())
                {
                    PRCP.pr_prefix = sdr0["pr_prefix"] != DBNull.Value ? (string)sdr0["pr_prefix"] : null;
                    PRCP.pr_ref = sdr0["pr_ref"] != DBNull.Value ? (int)sdr0["pr_ref"] : 0;
                    PRCP.pr_invoiced = sdr0["pr_invoiced"] != DBNull.Value ? (bool)sdr0["pr_invoiced"] : true;

                }
            }
            cnn.Close();

            //check if receip is invoiced
            if (PRCP.pr_invoiced == true)
            {
                return BadRequest(new { message = "Sorry! The purchase receipt " + PRCP.pr_prefix + PRCP.pr_ref.ToString("D4") + " already exists" });
            }

            //delete from header
            string delHeader = "DELETE FROM purchase_receipt_header WHERE pr_ref = " + rcpRef + " ";
            bool myReq1 = myDbconnection.UpdateDelInsert(delHeader, db);
            if (myReq1 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process receipt header request." });
            }

            //delete from details
            string delDetails = "DELETE FROM purchase_receipt_details WHERE pd_ref = " + rcpRef + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(delDetails, db);
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process receipt details request." });
            }

            return Ok(new { message = "Request has been successfully processed." });


        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseReceipt_GetReport(int rcpRef)
        {
            if (rcpRef == 0)
            {
                return BadRequest(new { message = "Cannot find the required receipt details" });
            }
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get receipt Header
            PurchaseReceipt PRInv = new PurchaseReceipt();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT purchase_receipt_header.*, \"CrCode\", \"PLCustCode\", \"CustName\", \"PhysicalAddress\", \"PostalAddress\",\"VATNo\",\"UFirstName\",\"ULastName\" " +
                "FROM purchase_receipt_header LEFT JOIN \"Currencies\"  ON \"CrId\" = pr_currency" +
                " LEFT JOIN  \"PLCustomer\" ON \"CustID\" = pr_customer " +
                "LEFT JOIN  \"Users\" ON \"UId\" = pr_user WHERE pr_id = " + rcpRef + " ", cnn).ExecuteReader();
            if (sdr0.Read())
            {
                PRInv.pr_id = sdr0["pr_id"] != DBNull.Value ? (int)sdr0["pr_id"] : 0;
                PRInv.pr_date = sdr0["pr_date"] != DBNull.Value ? (DateTime)sdr0["pr_date"] : DateTime.Now;
                PRInv.pr_ref = sdr0["pr_ref"] != DBNull.Value ? (int)sdr0["pr_ref"] : 0;
                PRInv.pr_prefix = sdr0["pr_prefix"] != DBNull.Value ? (string)sdr0["pr_prefix"] : null;
                PRInv.pr_customer = sdr0["pr_customer"] != DBNull.Value ? (int)sdr0["pr_customer"] : 0;
                PRInv.pr_user = sdr0["pr_user"] != DBNull.Value ? (int)sdr0["pr_user"] : 0;
                PRInv.pr_total = sdr0["pr_total"] != DBNull.Value ? (decimal)sdr0["pr_total"] : 0;
                PRInv.pr_currency = sdr0["pr_currency"] != DBNull.Value ? (int)sdr0["pr_currency"] : 0;
                PRInv.pr_additional = sdr0["pr_additional"] != DBNull.Value ? (string)sdr0["pr_additional"] : null;
                PRInv.pr_invoiced = sdr0["pr_invoiced"] != DBNull.Value ? (bool)sdr0["pr_invoiced"] : false;
                PRInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                PRInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                PRInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;
                PRInv.PhysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                PRInv.PostalAddress = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                PRInv.VATNo = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                PRInv.UFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                PRInv.ULastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;
            }
            cnn.Close();

            cnn.Open();
            List<PurchaseReceiptDetails> PRDetails = new List<PurchaseReceiptDetails>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT purchase_receipt_details.*,\"InvtName\" FROM purchase_receipt_details LEFT JOIN \"Inventory\" ON (\"InvtId\" = pd_item ) WHERE pd_ref = " + rcpRef + "  ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PurchaseReceiptDetails pr_det = new PurchaseReceiptDetails();
                pr_det.pd_date = sdr1["pd_date"] != DBNull.Value ? (DateTime)sdr1["pd_date"] : DateTime.Now;
                pr_det.pd_ref = sdr1["pd_ref"] != DBNull.Value ? (int)sdr1["pd_ref"] : 0;
                pr_det.pd_item = sdr1["pd_item"] != DBNull.Value ? (int)sdr1["pd_item"] : 0;
                pr_det.pd_qty = sdr1["pd_qty"] != DBNull.Value ? (int)sdr1["pd_qty"] : 0;
                pr_det.pd_unitprice = sdr1["pd_unitprice"] != DBNull.Value ? (decimal)sdr1["pd_unitprice"] : 0;
                pr_det.pd_vat_perc = sdr1["pd_vat_perc"] != DBNull.Value ? (string)sdr1["pd_vat_perc"] : null;
                pr_det.pd_totals = sdr1["pd_totals"] != DBNull.Value ? (decimal)sdr1["pd_totals"] : 0;
                pr_det.pd_vat_amt = sdr1["pd_vat_amt"] != DBNull.Value ? (decimal)sdr1["pd_vat_amt"] : 0;
                pr_det.StkDesc = sdr1["InvtName"] != DBNull.Value ? (string)sdr1["InvtName"] : null;
                PRDetails.Add(pr_det);
            }
            cnn.Close();
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
            }
            cnn.Close();
            return Ok(new
            {
                MyCompany = lic,
                PRHeader = PRInv,
                PRDetails = PRDetails
            });
        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult PurchaseReceipt_Return(int rcpRef)
        {

            if (rcpRef == 0)
            {
                return BadRequest(new { message = "Cannot find the required receipt details" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            PurchaseReceipt PRCP = new PurchaseReceipt();

            //CHeck if receipt id exists
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM purchase_receipt_header WHERE pr_ref = " + rcpRef + " ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! Required details were NOT found. Operation failed." });
            }
            else
            {
                if (sdr0.Read())
                {
                    PRCP.pr_prefix = sdr0["pr_prefix"] != DBNull.Value ? (string)sdr0["pr_prefix"] : null;
                    PRCP.pr_ref = sdr0["pr_ref"] != DBNull.Value ? (int)sdr0["pr_ref"] : 0;
                    PRCP.pr_invoiced = sdr0["pr_invoiced"] != DBNull.Value ? (bool)sdr0["pr_invoiced"] : true;
                }
            }
            cnn.Close();

            //check if receip is invoiced
            if (PRCP.pr_invoiced == true)
            {
                return BadRequest(new { message = "Sorry! The purchase receipt " + PRCP.pr_prefix + PRCP.pr_ref.ToString("D4") + " already exists" });
            }
            //Set returned true
            string delHeader = "UPDATE purchase_receipt_header SET pr_returned = 't' WHERE pr_ref = " + rcpRef + " ";
            bool myReq1 = myDbconnection.UpdateDelInsert(delHeader, db);
            if (myReq1 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process receipt header request." });
            }
            return Ok(new { message = "Request has been successfully processed." });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseReceipt_All_Returned()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get PR headers
            List<PurchaseReceipt> PRlist = new List<PurchaseReceipt>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT purchase_receipt_header.*, \"CrCode\", \"PLCustCode\", \"CustName\" FROM purchase_Receipt_header LEFT JOIN \"Currencies\"  ON \"CrId\" = pr_currency LEFT JOIN  \"PLCustomer\" ON \"CustID\" = pr_customer WHERE pr_returned = 't' ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                PurchaseReceipt PR = new PurchaseReceipt
                {
                    pr_id = sdr0["pr_id"] != DBNull.Value ? (int)sdr0["pr_id"] : 0,
                    pr_date = sdr0["pr_date"] != DBNull.Value ? (DateTime)sdr0["pr_date"] : DateTime.Today,
                    pr_ref = sdr0["pr_ref"] != DBNull.Value ? (int)sdr0["pr_ref"] : 0,
                    pr_prefix = sdr0["pr_prefix"] != DBNull.Value ? (string)sdr0["pr_prefix"] : null,
                    pr_customer = sdr0["pr_customer"] != DBNull.Value ? (int)sdr0["pr_customer"] : 0,
                    pr_user = sdr0["pr_user"] != DBNull.Value ? (int)sdr0["pr_user"] : 0,
                    pr_total = sdr0["pr_total"] != DBNull.Value ? (decimal)sdr0["pr_total"] : 0,
                    pr_currency = sdr0["pr_currency"] != DBNull.Value ? (int)sdr0["pr_currency"] : 0,
                    pr_additional = sdr0["pr_additional"] != DBNull.Value ? (string)sdr0["pr_additional"] : null,
                    pr_invoiced = sdr0["pr_invoiced"] != DBNull.Value ? (bool)sdr0["pr_invoiced"] : false,
                    pr_transdate = sdr0["pr_transdate"] != DBNull.Value ? (DateTime)sdr0["pr_transdate"] : DateTime.Today,
                    pr_returned = sdr0["pr_returned"] != DBNull.Value ? (bool)sdr0["pr_returned"] : false,

                    CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null,

                    PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null,
                    CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null

                };

                PRlist.Add(PR);
            }
            cnn.Close();


            return Ok(new { Receipts = PRlist });

        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult purchase_request_create_new(PRHeader PRDetails)
        {

            if (PRDetails.po_description.Length == 0)
            {
                return BadRequest(new { message = "Cannot find the purchase request details" });
            }
            else if (PRDetails.po_sender_signature == 0)
            {
                return BadRequest(new { message = "Cannot find the sender attached signature" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //check if a department name procurement exists
            cnn.Open();
            NpgsqlDataReader sdrDE = new NpgsqlCommand("SELECT * FROM \"Departments\" WHERE \"DpName\" = 'PROCUREMENT' ", cnn).ExecuteReader();
            if (sdrDE.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! NO procurement department was found. Please ensure that the department 'procurement' exists." });
            }
            cnn.Close();

            //check if any user is allowed to receive requests
            cnn.Open();
            NpgsqlDataReader sdrCH = new NpgsqlCommand("SELECT * FROM \"UserPermissions\" WHERE \"ReceivePurchaseRequest\" = 't' ", cnn).ExecuteReader();
            if (sdrCH.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! NO users has been granted permissions to receive the request you want to send. Please ensure that a user is allowed to receive purchase request' exists." });
            }
            cnn.Close();

            //get last purchase request
            int last_purchaseRequest_id = 0;
            cnn.Open();
            NpgsqlDataReader sdr_PRI = new NpgsqlCommand("Select MAX(po_id) as sl From purchase_order_header LIMIT 1 ", cnn).ExecuteReader();
            while (sdr_PRI.Read())
            {
                last_purchaseRequest_id = sdr_PRI["sl"] != DBNull.Value ? (int)sdr_PRI["sl"] : 0;
            }
            cnn.Close();


            //send email to all those allowed to receive requests
            //check permissions and loop through all users allowed to receive purchase request receipts
            cnn.Open();
            NpgsqlDataReader sdr5 = new NpgsqlCommand("Select * From \"UserPermissions\" WHERE \"ReceivePurchaseRequest\" = 't' ", cnn).ExecuteReader();
            while (sdr5.Read())
            {
                NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //get user data
                Users userdata = new Users();
                cnn2.Open();
                NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Users\" WHERE \"UId\" = " + sdr5["PUser"] + " ", cnn2).ExecuteReader();
                if (sdr3.Read())
                {
                    userdata.UFirstName = (string)sdr3["UFirstName"];
                    userdata.ULastName = (string)sdr3["ULastName"];
                    userdata.UEmail = (string)sdr3["UEmail"];
                }
                cnn2.Close();

                ////send email
                //var path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplates", "PurchaseRequest.html");
                //var imgsnipp_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "email_images", "purchaseRequest_snipp.png");

                //var builder = new BodyBuilder();
                //using (StreamReader SourceReader = System.IO.File.OpenText(path))
                //{
                //    builder.HtmlBody = SourceReader.ReadToEnd();
                //}
                ////{0} : Subject
                ////{1} : DateTime
                ////{2} : Email
                ////{3} : Password
                ////{4} : Message
                ////{5} : callbackURL

                string receiptRef = "PRQ" + (last_purchaseRequest_id + 1).ToString("D4");


                //string messageBody = string.Format(builder.HtmlBody,
                // String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                // userdata.UFirstName + " " + userdata.ULastName,
                // receiptRef
                //);

                ////prepare email
                //var email = new MimeMessage();
                //email.From.Add(MailboxAddress.Parse("pymetest@ngenx.io"));
                //email.To.Add(MailboxAddress.Parse(userdata.UEmail));
                //email.Subject = "Purchase Request " + receiptRef;

                //email.Body = new TextPart(TextFormat.Html)
                //{
                //    Text =
                //   messageBody
                //};



                //// send email
                //using var smtp = new SmtpClient();
                //smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                //smtp.Connect("smtp.munshiram.com", 587, SecureSocketOptions.Auto);
                //smtp.Authenticate("pymetest@ngenx.io", "S-85d9v7");
                //smtp.Send(email);
                //smtp.Disconnect(true);
                MailService mailService = new MailService();

                mailService.SendPurchaseRequest(_hostingEnvironment.WebRootPath, userdata, receiptRef);

            }
            cnn.Close();

            //save details
            decimal purchase_order_total = 0;

            if (PRDetails.po_description.Length > 0)
            {
                for (int i = 0; i < PRDetails.po_description.Length; i++)
                {
                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                    cnn1.Open();

                    purchase_order_total += PRDetails.po_description[i].pod_total;

                    //get item name from item id
                    Inventory inv = new Inventory();
                    cnn.Open();
                    NpgsqlDataReader sdr_inv = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + PRDetails.po_description[i].pod_itemid + " ", cnn).ExecuteReader();

                    if (sdr_inv.Read())
                    {
                        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                    }
                    cnn.Close();


                    string insertQuery1 = "INSERT INTO purchase_order_details (pod_ref, pod_itemname, pod_qty, pod_unitprice, pod_total, pod_vat_perc, pod_vat_amt, pod_itemid ) VALUES(" + (last_purchaseRequest_id + 1) + ", '" + inv.InvtName + "', " + PRDetails.po_description[i].pod_qty + ", " + PRDetails.po_description[i].pod_unitprice + "," + PRDetails.po_description[i].pod_total + "," + PRDetails.po_description[i].pod_vat_perc + "," + PRDetails.po_description[i].pod_vat_amt + "," + PRDetails.po_description[i].pod_itemid + " ); ";

                    bool insertQ0 = myDbconnection.UpdateDelInsert(insertQuery1, db);

                    cnn1.Close();

                    if (insertQ0 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occured while trying to save details." });
                    }

                }
            }




            //set header details
            cnn.Open();
            string insertQ = "INSERT INTO purchase_order_header (po_id, po_date, po_prefix, po_ref, po_user, po_total, po_status, po_sender_signature, po_transdate,po_has_lpo) VALUES(" + (last_purchaseRequest_id + 1) + ", '" + DateTime.Today + "', 'PRQ', " + (last_purchaseRequest_id + 1) + " ," + userId + "," + purchase_order_total + ",'pending review'," + PRDetails.po_sender_signature + ",'" + DateTime.Today + "','f'); ";

            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);

            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to save details." });
            }


            return Ok(new { message = "Request has been successfully processed." });


        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult reject_purchase_request(int order_id)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //get database name
            string db = companyRes;
            string status = "rejected";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();

            string updtQ = "UPDATE \"purchase_order_header\" SET \"po_status\" = '" + status + "' WHERE \"po_id\" = '" + order_id + "' ";
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, db);

            cnn.Close();
            MyResponse response = new MyResponse();
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Group has been deactivated successfuly";
            }

            return Ok(new { message = "purchase order has been rejected" });

        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult approve_purchase_request(int order_id)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //get database name
            string db = companyRes;
            string status = "approved";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();

            string updtQ = "UPDATE \"purchase_order_header\" SET \"po_status\" = '" + status + "' , \"po_approvedby\" = '" + userId + "' WHERE \"po_id\" = '" + order_id + "' ";
            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, db);

            cnn.Close();
            MyResponse response = new MyResponse();
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Group has been deactivated successfuly";
            }

            return Ok(new { message = "purchase order has been approved" });

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult purchase_request_get_all()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get purchase order Header
            List<PRHeader> pHeader = new List<PRHeader>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT purchase_order_header.*, s.\"UFirstName\", s.\"ULastName\", r.\"UFirstName\" AS rfn, r.\"ULastName\" AS rln, sgs.sign_data, sgs.sign_name, sgr.sign_data AS sgrdata, sgr.sign_name AS sgrname " +
                "FROM purchase_order_header" +
                " LEFT JOIN \"Users\" s ON s.\"UId\" = purchase_order_header.po_user " +
                "LEFT JOIN \"Users\" r ON r.\"UId\" = purchase_order_header.po_approvedby" +
                " LEFT JOIN signatures sgs ON sgs.sign_id = purchase_order_header.po_sender_signature" +
                " LEFT JOIN signatures sgr ON sgr.sign_id = purchase_order_header.po_approval_signature ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                //get received signature details

                PRHeader pr = new PRHeader();
                pr.po_id = sdr0["po_id"] != DBNull.Value ? (int)sdr0["po_id"] : 0;
                pr.po_date = sdr0["po_date"] != DBNull.Value ? (DateTime)sdr0["po_date"] : DateTime.Today;
                pr.po_prefix = sdr0["po_prefix"] != DBNull.Value ? (string)sdr0["po_prefix"] : null;
                pr.po_ref = sdr0["po_ref"] != DBNull.Value ? (int)sdr0["po_ref"] : 0;
                pr.po_user = sdr0["po_user"] != DBNull.Value ? (int)sdr0["po_user"] : 0;
                pr.po_total = sdr0["po_total"] != DBNull.Value ? (decimal)sdr0["po_total"] : 0;
                pr.po_status = sdr0["po_status"] != DBNull.Value ? (string)sdr0["po_status"] : null;
                pr.po_approvedby = sdr0["po_approvedby"] != DBNull.Value ? (int)sdr0["po_approvedby"] : 0;
                pr.po_sender_signature = sdr0["po_sender_signature"] != DBNull.Value ? (int)sdr0["po_sender_signature"] : 0;
                pr.po_approval_signature = sdr0["po_approval_signature"] != DBNull.Value ? (int)sdr0["po_approval_signature"] : 0;
                pr.po_transdate = sdr0["po_transdate"] != DBNull.Value ? (DateTime)sdr0["po_transdate"] : DateTime.Today;
                pr.po_has_lpo = sdr0["po_has_lpo"] != DBNull.Value ? (bool)sdr0["po_has_lpo"] : false;

                //sender details
                pr.SenderFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                pr.SenderLastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;

                //approver details
                pr.ApprovalFirstName = sdr0["rfn"] != DBNull.Value ? (string)sdr0["rfn"] : null;
                pr.ApprovalLastName = sdr0["rln"] != DBNull.Value ? (string)sdr0["rln"] : null;

                //sender signature details
                pr.SendSignatureData = sdr0["sign_data"] != DBNull.Value ? (string)sdr0["sign_data"] : null;
                pr.SendSignaturename = sdr0["sign_name"] != DBNull.Value ? (string)sdr0["sign_name"] : null;

                pr.ApproveSignatureData = sdr0["sgrdata"] != DBNull.Value ? (string)sdr0["sgrdata"] : null;
                pr.ApproveSignatureName = sdr0["sgrname"] != DBNull.Value ? (string)sdr0["sgrname"] : null;


                pHeader.Add(pr);

            }
            cnn.Close();
            //////////////////////////////e
            ///


            List<PLCustomer> plCustlist = new List<PLCustomer>();
            cnn.Open();
            NpgsqlDataReader sdr10 = new NpgsqlCommand("SELECT \"PLCustomer\".* FROM \"PLCustomer\" ", cnn).ExecuteReader();

            while (sdr10.Read())
            {
                PLCustomer plCust = new PLCustomer();
                plCust.PLCustCode = sdr10["PLCustCode"] != DBNull.Value ? (string)sdr10["PLCustCode"] : null;
                plCust.CustName = sdr10["CustName"] != DBNull.Value ? (string)sdr10["CustName"] : null;
                plCust.PhysicalAddress = sdr10["PhysicalAddress"] != DBNull.Value ? (string)sdr10["PhysicalAddress"] : null;
                plCust.PostalAddress = sdr10["PostalAddress"] != DBNull.Value ? (string)sdr10["PostalAddress"] : null;
                plCust.CurrID = sdr10["CurrID"] != DBNull.Value ? (int)sdr10["CurrID"] : 0;
                plCust.VATNo = sdr10["VATNo"] != DBNull.Value ? (string)sdr10["VATNo"] : null;

                plCust.CustID = sdr10["CustID"] != DBNull.Value ? (int)sdr10["CustID"] : 0;
                plCust.RegisterDate = sdr10["RegisterDate"] != DBNull.Value ? (DateTime)sdr10["RegisterDate"] : DateTime.Now;
                plCust.StaffID = sdr10["StaffID"] != DBNull.Value ? (int)sdr10["StaffID"] : 0;
                plCust.VATNo = sdr10["VATNo"] != DBNull.Value ? (string)sdr10["VATNo"] : null;

                plCustlist.Add(plCust);
            }
            cnn.Close();

            return Ok(new
            {
                PRHeaderList = pHeader,
                supplierlist = plCustlist

            });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult purchase_request_report_view(int rcp)
        {

            if (rcp == 0)
            {
                return BadRequest(new { message = "Cannot find the required receipt details" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //CHeck if receipt id exists
            cnn.Open();
            NpgsqlDataReader sdr01 = new NpgsqlCommand("SELECT * FROM purchase_order_header WHERE po_ref = " + rcp + " ", cnn).ExecuteReader();
            if (sdr01.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! Required details were NOT found. Operation failed." });
            }
            cnn.Close();

            //set header
            cnn.Open();

            PRHeader pr = new PRHeader();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT purchase_order_header.*, s.\"UFirstName\", s.\"ULastName\", r.\"UFirstName\" AS rfn, r.\"ULastName\" AS rln, sgs.sign_data, sgs.sign_name, sgr.sign_data AS sgrdata, sgr.sign_name AS sgrname FROM purchase_order_header " +
                "LEFT JOIN \"Users\" s ON s.\"UId\" = purchase_order_header.po_user" +
                " LEFT JOIN \"Users\" r ON r.\"UId\" = purchase_order_header.po_approvedby" +
                " LEFT JOIN signatures sgs ON sgs.sign_id = purchase_order_header.po_sender_signature " +
                "LEFT JOIN signatures sgr ON sgr.sign_id = purchase_order_header.po_approval_signature WHERE po_ref = " + rcp + " ", cnn).ExecuteReader();
            while (sdr0.Read())
            {

                pr.po_id = sdr0["po_id"] != DBNull.Value ? (int)sdr0["po_id"] : 0;
                pr.po_date = sdr0["po_date"] != DBNull.Value ? (DateTime)sdr0["po_date"] : DateTime.Today;
                pr.po_prefix = sdr0["po_prefix"] != DBNull.Value ? (string)sdr0["po_prefix"] : null;
                pr.po_ref = sdr0["po_ref"] != DBNull.Value ? (int)sdr0["po_ref"] : 0;
                pr.po_user = sdr0["po_user"] != DBNull.Value ? (int)sdr0["po_user"] : 0;
                pr.po_total = sdr0["po_total"] != DBNull.Value ? (decimal)sdr0["po_total"] : 0;
                pr.po_status = sdr0["po_status"] != DBNull.Value ? (string)sdr0["po_status"] : null;
                pr.po_approvedby = sdr0["po_approvedby"] != DBNull.Value ? (int)sdr0["po_approvedby"] : 0;
                pr.po_sender_signature = sdr0["po_sender_signature"] != DBNull.Value ? (int)sdr0["po_sender_signature"] : 0;
                pr.po_approval_signature = sdr0["po_approval_signature"] != DBNull.Value ? (int)sdr0["po_approval_signature"] : 0;
                pr.po_transdate = sdr0["po_transdate"] != DBNull.Value ? (DateTime)sdr0["po_transdate"] : DateTime.Today;
                pr.po_has_lpo = sdr0["po_has_lpo"] != DBNull.Value ? (bool)sdr0["po_has_lpo"] : false;

                //sender details
                pr.SenderFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                pr.SenderLastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;

                //approver details
                pr.ApprovalFirstName = sdr0["rfn"] != DBNull.Value ? (string)sdr0["rfn"] : null;
                pr.ApprovalLastName = sdr0["rln"] != DBNull.Value ? (string)sdr0["rln"] : null;

                //sender signature details
                pr.SendSignatureData = sdr0["sign_data"] != DBNull.Value ? (string)sdr0["sign_data"] : null;
                pr.SendSignaturename = sdr0["sign_name"] != DBNull.Value ? (string)sdr0["sign_name"] : null;

                pr.ApproveSignatureData = sdr0["sgrdata"] != DBNull.Value ? (string)sdr0["sgrdata"] : null;
                pr.ApproveSignatureName = sdr0["sgrname"] != DBNull.Value ? (string)sdr0["sgrname"] : null;

            }
            cnn.Close();

            //assign details
            List<PRDetails> pDetails = new List<PRDetails>();
            cnn.Open();
            NpgsqlDataReader sdrDet = new NpgsqlCommand("SELECT * FROM purchase_order_details WHERE pod_ref = " + rcp + " ", cnn).ExecuteReader();
            while (sdrDet.Read())
            {
                //get received signature details

                PRDetails prd = new PRDetails();

                prd.pod_ref = sdrDet["pod_ref"] != DBNull.Value ? (int)sdrDet["pod_ref"] : 0;
                prd.pod_itemname = sdrDet["pod_itemname"] != DBNull.Value ? (string)sdrDet["pod_itemname"] : null;
                prd.pod_qty = sdrDet["pod_qty"] != DBNull.Value ? (int)sdrDet["pod_qty"] : 0;
                prd.pod_unitprice = sdrDet["pod_unitprice"] != DBNull.Value ? (decimal)sdrDet["pod_unitprice"] : 0;
                prd.pod_total = sdrDet["pod_total"] != DBNull.Value ? (decimal)sdrDet["pod_total"] : 0;
                prd.pod_vat_perc = sdrDet["pod_vat_perc"] != DBNull.Value ? (decimal)sdrDet["pod_vat_perc"] : 0;
                prd.pod_vat_amt = sdrDet["pod_vat_amt"] != DBNull.Value ? (decimal)sdrDet["pod_vat_amt"] : 0;
                prd.pod_itemid = sdrDet["pod_itemid"] != DBNull.Value ? (int)sdrDet["pod_itemid"] : 0;

                pDetails.Add(prd);

            }
            cnn.Close();

            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();

            return Ok(new
            {
                MyCompany = lic,
                PRHeaderReport = pr,
                PRDetailsReport = pDetails

            });


        }
        private static bool IsBase64String(string base64)
        {
            try
            {
                string result = Regex.Replace(base64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                byte[] data = Convert.FromBase64String(result);
                return true;
            }
            catch
            {
                return false;
            }
        }
        [Route("PaymentReverseReceipt")]
        [HttpPost]
        [Authorize]
        public IActionResult PaymentReverseReceipt(paymentReceiptReversal reversal)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            string insertQuery1 = "INSERT INTO \"PlReceiptReversal\" ( \"CreatedBy\", \"ReceiptNumber\",\"Reason\",\"CreatedOn\", \"Status\",\"IsReversed\") VALUES(" +  userId + ", '" + reversal.ReceiptNumber + "', '" + reversal.Reason + "', '" + reversal.CreatedOn + "', 'Pending', 'f' ); ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            bool inst_res = myDbconnection.UpdateDelInsert(insertQuery1, db);
            if (inst_res == false)
            {
                return BadRequest(new { message = $"Sorry! An error occurred while trying to send Payment Receipt Reversal request. Please contact support for more details." });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"Payment Receipt Reversal of receipt {reversal.ReceiptNumber} created successfully  ";
            auditTrail.module = "Payment Receipt Reversal ";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            return Ok(new { message = "Payment Receipt Reverse Request Has Been Made" });
        }
        [Route("GettingPaymentReversalList")]
        [HttpGet]
        [Authorize]
        public ActionResult<List<paymentReceiptReversalList>> GettingPaymentReversalList()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" });
            }
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            List<paymentReceiptReversalList> receiptList = new List<paymentReceiptReversalList>();
            string query = "SELECT a.*, b.\"pyID\", concat(c.\"UFirstName\" ,' ', c.\"ULastName\") as name FROM \"PlReceiptReversal\" a LEFT JOIN \"PLReceipts\" b on b.\"pyID\" = a.\"ReceiptNumber\" LEFT JOIN \"Users\" c on c.\"UId\" = a.\"CreatedBy\" ORDER BY a.\"CreatedOn\"   ";
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (reader.Read())
            {
                paymentReceiptReversalList list = new paymentReceiptReversalList();
                list.CreatedBy = reader["name"] != DBNull.Value ? (string)reader["name"] : "";
                list.ReceiptNumber = reader["ReceiptNumber"] != DBNull.Value ? (int)reader["ReceiptNumber"] : 0;
                list.Reason = reader[""] != DBNull.Value ? (string)reader[""] : "";
                list.Status = reader[""] != DBNull.Value ? (string)reader[""] : "";
                list.CreatedOn = (DateTime)reader[""];
                list.ReversedOn = reader[""] != DBNull.Value ? (DateTime)reader[""] : DateTime.MinValue;
                list.IsReversed = reader[""] != DBNull.Value ? (bool)reader[""] : false;
                if(list.IsReversed == true)
                {
                    list.ReversedBy = reader["name"] != DBNull.Value ? (string)reader["name"] : "";
                }
                receiptList.Add(list);
            }
            cnn.Close();
            return Ok(receiptList);
        }
    }
}
