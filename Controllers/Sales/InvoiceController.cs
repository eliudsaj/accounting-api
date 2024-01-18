using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using pyme_finance_api.Models.DBConn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.ReportPages.Sales;
using pyme_finance_api.Models.ReusableCodes;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Models.Sales.Terms;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Controllers.NlController;
using pyme_finance_api.Service.NlServices;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;
using Org.BouncyCastle.Ocsp;
using System.Text.RegularExpressions;
using pyme_finance_api.Service.CurrencyService;
using Newtonsoft.Json;
using pyme_finance_api.Models.Dashboard;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private IWebHostEnvironment _hostingEnvironment;
        readonly ILogger<InvoiceController> _log;

        public InvoiceController(IConfiguration config, IWebHostEnvironment environment, ILogger<InvoiceController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _log = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult ListDefaults(string customer_ref)
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
            else if (string.IsNullOrEmpty(customer_ref))
            {
                return BadRequest(new { message = "Cannot find required customer reference. Request failed" });
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
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //check customer id exists
            string check_query = "SELECT * FROM \"SLCustomer\" WHERE \"CustRef\" = '" + customer_ref + "' ";
            int check_res = myDbconnection.CheckRowExists(check_query, db);
            if (check_res == 0)
            {
                return BadRequest(new { message = "No customer found with the parsed reference was found. Request cancelled" });
            }
            //Get all currencies
            List<Currencies> currencyList = new List<Currencies>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Currencies\" ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                Currencies curr = new Currencies();
                curr.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
                curr.CrName = sdr0["CrName"] != DBNull.Value ? (string)sdr0["CrName"] : null;
                curr.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                curr.CrCountry = sdr0["CrCountry"] != DBNull.Value ? (string)sdr0["CrCountry"] : null;
                curr.CrStatus = sdr0["CrStatus"] != DBNull.Value ? (string)sdr0["CrStatus"] : null;
                curr.CrCreatedDate = sdr0["CrCreatedDate"] != DBNull.Value ? (DateTime)sdr0["CrCreatedDate"] : DateTime.Now;
                curr.CrModifiedDate = sdr0["CrModifiedDate"] != DBNull.Value ? (DateTime)sdr0["CrModifiedDate"] : DateTime.Now;
                currencyList.Add(curr);
            }
            cnn.Close();
            //Get all VAT
            List<TaxSetup> vatList = new List<TaxSetup>();
            cnn.Open();
            NpgsqlDataReader sdr_1 = new NpgsqlCommand("SELECT * FROM \"VATs\" ", cnn).ExecuteReader();
            while (sdr_1.Read())
            {
                TaxSetup tx = new TaxSetup();
                tx.VtId = sdr_1["VtId"] != DBNull.Value ? (int)sdr_1["VtId"] : 0;
                tx.VtRef = sdr_1["VtRef"] != DBNull.Value ? (string)sdr_1["VtRef"] : null;
                tx.VtPerc = sdr_1["VtPerc"] != DBNull.Value ? (float)sdr_1["VtPerc"] : 0;
                vatList.Add(tx);
            }
            cnn.Close();
            //get all Invoices
            List<Inventory> inventList = new List<Inventory>();
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select \"Inventory\".*, \"VtId\", \"VtRef\", \"VtPerc\" From \"Inventory\" LEFT JOIN \"VATs\" ON ( \"InvtVATId\" =  \"VtId\" ) WHERE \"InvtBranch\" = " + staff_branch + "  ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Inventory inv = new Inventory();
                inv.InvtId = (int)sdr["InvtId"];
                inv.InvtType = (string)sdr["InvtType"];
                inv.InvtQty = (int)sdr["InvtQty"];
                inv.InvtName = (string)sdr["InvtName"];
                inv.InvtSP = (decimal)sdr["InvtSP"];
                inv.InvtVATId = (int)sdr["InvtVATId"];
                inv.VATPerc = (float)sdr["VtPerc"];
                inv.VATRef = (string)sdr["VtRef"];
                inv.InvtProdCode = (string)sdr["InvtProdCode"];
                inv.InventoryItem = sdr["InventoryItem"] != DBNull.Value ? (string)sdr["InventoryItem"] : null;
                inv.SLProdGrpCode = sdr["SLProdGrpCode"] != DBNull.Value ? (string)sdr["SLProdGrpCode"] : null;
                inventList.Add(inv);
            }
            cnn.Close();
            //Get Last
            int lastInvNumber = 0;
            //get last registered number
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"SLInvoiceHeader\" WHERE  \"INVTypeRef\" = 'INV' LIMIT 1 ", cnn).ExecuteReader();
            while (sdr2.Read())
            {
                lastInvNumber = (int)sdr2["st"];
            }
            cnn.Close();
            //count invoice types
            int count_inv_types = 0;
            cnn.Open();
            NpgsqlDataReader sdr_1C = new NpgsqlCommand("SELECT * FROM \"SLInvoiceTypes\" ", cnn).ExecuteReader();
            while (sdr_1C.Read())
            {
                count_inv_types++;
            }
            cnn.Close();
            //Get Invoice Settings
            InvoiceSettings invsettings = new InvoiceSettings();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select *  FROM \"SLInvoiceSettings\" WHERE \"InvBranch\" = " + staff_branch + " LIMIT 1 ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                invsettings.InvPrefix = sdr1["InvPrefix"].ToString();
                invsettings.InvStartNumber = (int)sdr1["InvStartNumber"];
                invsettings.LastNumber = lastInvNumber;
                invsettings.InvNumberingType = sdr1["InvNumberingType"].ToString();
                invsettings.InvDeliveryNotes = sdr1["InvDeliveryNotes"] != DBNull.Value ? (int)sdr1["InvDeliveryNotes"] : 0;
                invsettings.InvTypesCount = count_inv_types;
            }

            cnn.Close();
            //Get financial period Settings
            FinancialPeriod finPrd = new FinancialPeriod();
            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand("Select *  FROM financial_periods WHERE fp_branch = " + staff_branch + " AND fp_active = 't' ", cnn).ExecuteReader();
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
            //check if selected financial period is active
            if (finPrd.fp_active == false)
            {
                return BadRequest(new { message = "Sorry! the financial period found is NOT active. Please configure of set your financial period to proceed" });
            }
            //Get all discounts not expired
            List<Discounts> discList = new List<Discounts>();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Discounts\" WHERE \"DEndDate\" >= '" + DateTime.Now + "'  ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                Discounts disc = new Discounts();
                disc.DId = (int)sdr3["DId"];
                disc.DRef = (string)sdr3["DRef"];
                disc.DPerc = (float)sdr3["DPerc"];
                discList.Add(disc);
            }
            cnn.Close();
            //selected customer
            AddCustomer selected_customer = new AddCustomer();
            cnn.Open();
            NpgsqlDataReader sdr_cust = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"CustRef\" = '" + customer_ref + "'  ", cnn).ExecuteReader();
            if (sdr_cust.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! the customer reference was NOT found." });
            }
            while (sdr_cust.Read())
            {
                selected_customer.SLCustomerSerial = (int)sdr_cust["SLCustomerSerial"];
                selected_customer.CustCode = sdr_cust["CustCode"].ToString();
                selected_customer.CustFirstName = sdr_cust["CustFirstName"] != DBNull.Value ? (string)sdr_cust["CustFirstName"] : null;
                selected_customer.CustLastName = sdr_cust["CustLastName"] != DBNull.Value ? (string)sdr_cust["CustLastName"] : null;
                selected_customer.CustCompany = sdr_cust["CustCompany"] != DBNull.Value ? (string)sdr_cust["CustCompany"] : null;
                selected_customer.CustType = sdr_cust["CustType"] != DBNull.Value ? (string)sdr_cust["CustType"] : null;
                selected_customer.CurCode = sdr_cust["CurCode"] != DBNull.Value ? (int)sdr_cust["CurCode"] : 0;
                selected_customer.CreditTerms = sdr_cust["CreditTerms"] != DBNull.Value ? (int)sdr_cust["CreditTerms"] : 0;
                selected_customer.Status = sdr_cust["Status"] != DBNull.Value ? (string)sdr_cust["Status"] : null;
                selected_customer.CustEmail = sdr_cust["CustEmail"] != DBNull.Value ? (string)sdr_cust["CustEmail"] : null;
                selected_customer.Address = sdr_cust["Address"] != DBNull.Value ? (string)sdr_cust["Address"] : null;
            }
            cnn.Close();
            //Get All customers
            List<AddCustomer> customersList = new List<AddCustomer>();
            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"CustBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr4.Read())
            {
                AddCustomer addCust = new AddCustomer();
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustFirstName = sdr4["CustFirstName"] != DBNull.Value ? (string)sdr4["CustFirstName"] : null;
                addCust.CustLastName = sdr4["CustLastName"] != DBNull.Value ? (string)sdr4["CustLastName"] : null;
                addCust.CustCompany = sdr4["CustCompany"] != DBNull.Value ? (string)sdr4["CustCompany"] : null;
                addCust.CustType = sdr4["CustType"].ToString();
                addCust.CurrCode = sdr4["CurCode"].ToString();
                customersList.Add(addCust);
            }

            cnn.Close();
            NlService nlService = new NlService(db);
            var nominallist = nlService.GetNlaccounts();
            return Ok(new
            {
                SelectedCustomerData = customersList,
                CustomerData = customersList,
                VATData = vatList,
                CurrencyData = currencyList,
                NominalData = nominallist,
                InventoryData = inventList,
                InvSettings = invsettings,
                CustCurrId = selected_customer.CurCode,
                DiscData = discList,
                PaymentDays = selected_customer.CreditTerms,
                SelectedCustomer = selected_customer,
                financialPeriod_Data = finPrd
            });
        }
        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> PostCustomerInvoiceAsync([FromForm] Invoice invoiceData)
        {
            //check data
            if (string.IsNullOrEmpty(invoiceData.Period))
            {
                return BadRequest(new { message = "Missing Invoice period" });
            }
            _log.LogInformation($"Creating invoice for customer {invoiceData.CustCode} ");
            invoiceData.InvoiceDetailsList = JsonConvert.DeserializeObject<List<Inventorylist>>(invoiceData.test);
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                _log.LogError($"Cannot find required permission parameters. Request terminated.Security verification failed.");
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
                _log.LogError($"Cannot find your client connection route. Page verification failed");
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
                return BadRequest(new
                {
                    message =
                        "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
                });
            }
            //get database name
            string db = companyRes;
            /// ensure system has accounts of  VAT,SALES,DEBTORS
            NlService nlService = new NlService(db);
            //if(nlService.GetNLAccountAccountCodeUsingName("SALES") == null)
            //{
            //    _log.LogError($"Missing SALES account is your system setup please add it in the nl account module");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing SALES account is your system setup please add it in the nl account module"
            //    });

            //}
            //if (nlService.GetNLAccountAccountCodeUsingName("DEBTORS") == null)
            //{
            //    _log.LogError($"Missing DEBTORS account is your system setup please add it in the nl account module");
            //    return BadRequest(new
            //    {

            //        message =
            //            "Missing DEBTORS account is your system setup please add it in the nl account module"
            //    });

            //}
            //if (nlService.GetNLAccountAccountCodeUsingName("VAT") == null)
            //{
            //    _log.LogError($"Missing VAT account is your system setup please add it in the nl account module");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing VAT account is your system setup please add it in the nl account module"
            //    });
            //}
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                _log.LogError($"Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator");
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            //check if customer exists
            string cust_check = "SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ";
            int cust_check_res = myDbconnection.CheckRowExists(cust_check, db);
            if (cust_check_res == 0)
            {
                _log.LogError($"Customer of Id {invoiceData.CustId} was not found");
                return BadRequest(new { message = "No customer found with the parsed data was found. Request cancelled" });
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get selected customer
            AddCustomer selectedCustomer = new AddCustomer();
            cnn.Open();
            NpgsqlDataReader sdr_sc = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ", cnn).ExecuteReader();
            while (sdr_sc.Read())
            {
                selectedCustomer.SLCustomerSerial = (int)sdr_sc["SLCustomerSerial"];
                selectedCustomer.CustCode = sdr_sc["CustCode"].ToString();
                selectedCustomer.CustFirstName = sdr_sc["CustFirstName"] != DBNull.Value ? (string)sdr_sc["CustFirstName"] : null;
                selectedCustomer.Address = sdr_sc["Address"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.CurCode = (int)sdr_sc["CurCode"];
                selectedCustomer.CustEmail = sdr_sc["CustEmail"].ToString();
                selectedCustomer.CustContact = sdr_sc["CustContact"].ToString();
                selectedCustomer.SLCTypeID = (int)sdr_sc["SLCTypeID"];
                selectedCustomer.CustLastName = sdr_sc["CustLastName"] != DBNull.Value ? (string)sdr_sc["CustLastName"] : null;
                selectedCustomer.CustType = sdr_sc["CustType"].ToString();
                selectedCustomer.CustCompany = sdr_sc["CustCompany"] != DBNull.Value ? (string)sdr_sc["CustCompany"] : null;
                selectedCustomer.VATNo = sdr_sc["VATNo"] != DBNull.Value ? (string)sdr_sc["VATNo"] : null;
                selectedCustomer.CustCreditLimit = (float)sdr_sc["CustCreditLimit"];
                selectedCustomer.VATpin = sdr_sc["VATpin"].ToString();
                selectedCustomer.CreditTerms = (int)sdr_sc["CreditTerms"];
                selectedCustomer.CurrCode = sdr_sc["CurCode"].ToString();
                selectedCustomer.CustBranch = sdr_sc["CustBranch"] != DBNull.Value ? (int)sdr_sc["CustBranch"] : 0;
                selectedCustomer.CustRef = sdr_sc["CustRef"] != DBNull.Value ? (string)sdr_sc["CustRef"] : null;
                selectedCustomer.CustomerDept = sdr_sc["CustomerDept"] != DBNull.Value ? (decimal)sdr_sc["CustomerDept"] : 0;
            }
            cnn.Close();
            //check if customer dept is greater
            decimal customerDept = selectedCustomer.CustomerDept;
            //check customer credit limit
            decimal customer_credit_limit = (decimal)selectedCustomer.CustCreditLimit;
            decimal total_invoice_amount = 0;
            foreach (var t in invoiceData.InvoiceDetailsList)
            {
                ///sales
                total_invoice_amount += (t.Total - t.DiscountAmt);
                //Debug.WriteLine(total_invoice_amount);
                //check inventory items quantity if can be deducted
                NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn2.Open();
                Inventory inv = new Inventory();
                NpgsqlDataReader sdr_c = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + t.ItemId + " ", cnn2).ExecuteReader();
                if (sdr_c.Read())
                {
                    inv.InvtName = sdr_c["InvtName"] != DBNull.Value ? (string)sdr_c["InvtName"] : null;
                    inv.InvtQty = sdr_c["InvtQty"] != DBNull.Value ? (int)sdr_c["InvtQty"] : 0;
                    inv.InvtType = sdr_c["InvtType"] != DBNull.Value ? (string)sdr_c["InvtType"] : null;
                    if (inv.InvtType == "GOODS")
                    {
                        if (t.Quantity > inv.InvtQty)
                        {
                            return BadRequest(new { message = "Cannot remove " + t.Quantity + " from your inventory. You only have " + inv.InvtQty + " items left." });
                        }
                    }
                }
                cnn2.Close();
            }
            /// total debt of the customer + total invoice amount = what the customer owes
            decimal ttl_customer_dept = total_invoice_amount + customerDept;
            if (ttl_customer_dept > customer_credit_limit)
            {
                _log.LogError($"Customer code {invoiceData.CustCode}  has exceded thier credit limit ");
                return BadRequest(new { message = "The Invoice Amount " + string.Format("{0:#,0.00}", total_invoice_amount) + " exceeds the customer credit limit of " + string.Format("{0:#,0.00}", customer_credit_limit) + " " });
            }
            //check if invoice settings exists
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("Select * From \"SLInvoiceSettings\" LIMIT 1 ", cnn).ExecuteReader();
            if (sdr2.HasRows == false)
            {
                return BadRequest(new { message = "No data was found with the invoice settings. Please set the invoice  configurations to continue" });
            }
            cnn.Close();
            //get last SLJrnNO
            int lastSLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"SLJrnlNo\") as sl From \"SLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastSLJRN = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            //get last NLJrnNO
            int lastNLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"SLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;
            }
            cnn.Close();
            //   var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
            decimal fullPrice = 0;
            decimal vatAmount = 0;
            //insert invoice details
            if (invoiceData.InvoiceDetailsList.Count > 0)
            {
                foreach (var t in invoiceData.InvoiceDetailsList)
                {
                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                    //get inventory item id
                    cnn1.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr_inv = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + t.ItemId + "  ", cnn1).ExecuteReader();
                    while (sdr_inv.Read())
                    {
                        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                        inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                        inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                        inv.SLProdGrpCode = sdr_inv["SLProdGrpCode"] != DBNull.Value ? (string)sdr_inv["SLProdGrpCode"] : null;
                        inv.WarehouseRef = sdr_inv["WarehouseRef"] != DBNull.Value ? (string)sdr_inv["WarehouseRef"] : null;
                    }
                    cnn1.Close();
                    //get discount data
                    Discounts disc = new Discounts();
                    cnn1.Open();
                    NpgsqlDataReader sdr_disc = new NpgsqlCommand("SELECT * FROM \"Discounts\" WHERE \"DId\" = " + t.Discount + "  ", cnn1).ExecuteReader();
                    if (sdr_disc.Read())
                    {
                        disc.DPerc = sdr_disc["DPerc"] != DBNull.Value ? (float)sdr_disc["DPerc"] : 0;
                    }
                    cnn1.Close();
                    //calculate proce and Qty
                    fullPrice = t.Price * t.Quantity;
                    decimal beforeperrate = t.VatCode * fullPrice;
                    decimal afterperrate = beforeperrate / 100;
                    //calculate VAT
                    vatAmount = afterperrate;
                    cnn1.Open();
                    string unique_id = System.Guid.NewGuid().ToString("D");
                    //check if inventory id exists
                    //string insertQuery1 =
                    //    "INSERT INTO \"SLInvoiceDetail\" (\"SLJrnlNo\", \"JrnlSLNo\", \"VatCode\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ItemSerial\", \"ItemQty\", \"ItemTotals\", \"ItemUnitPrice\", \"DiscountPerc\",\"DiscountAmt\", \"AdditionalDetails\",\"ItemId\",\"ItemCode\",\"ItemName\" ) VALUES(" +
                    //    (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + t.VAT + "', " + vatAmount + ",'" +
                    //    t.Description + "'," + userId + ",'" + unique_id + "', " + t.Quantity + ", " + t.Total + ", " +
                    //    t.Price + ", " + disc.DPerc + ", " + t.DiscountAmt + ", '" + t.additionalDetails + "'," +
                    //    t.ItemId + ",'" + t.ItemCode + "','" + inv.InvtName + "' ); ";
                    //bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                    int inventoryId = inv.SLProdGrpCode == "NA" ? 0 : Int32.Parse(inv.SLProdGrpCode);
                    var productgroupcode = inventoryId > 0 ? nlService.findSlAnalysisCodeById(inventoryId).Id : 0;
                    string insertQuery1 = "INSERT INTO \"SLInvoiceDetail\" (\"SLJrnlNo\", \"JrnlSLNo\", \"VatCode\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ItemSerial\", \"ItemQty\", \"ItemTotals\", " +
                        "\"ItemUnitPrice\", \"DiscountPerc\",\"DiscountAmt\", \"AdditionalDetails\",\"ItemId\",\"ItemCode\",\"ItemName\",\"ProdGroupCode\" ) VALUES(" +
                        (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + t.VatCode + "', " + vatAmount + ",'" +
                        t.StkDesc.Replace("'", "\\") + "'," + userId + ",'" + unique_id + "', " + t.Quantity + ", " + t.Total + ", " +
                        t.Price + ", " + disc.DPerc + ", " + t.DiscountAmt + ", '" + t.StkDesc.Replace("'", "\\") + "'," +
                        t.ItemId + ",'" + t.ItemCode + "','" + inv.InvtName + "','" + productgroupcode + "' ); ";
                    bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                    cnn1.Close();
                    if (myReq1 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occurred while trying to save invoice details." });
                    }
                    //update inventory item quantity if goods
                    if (inv.InvtType == "GOODS")
                    {
                        string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " + t.Quantity +
                                        " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + t.ItemId + " ";
                        bool myReq24 = myDbconnection.UpdateDelInsert(up_inv, db);
                        if (myReq24 == false)
                        {
                            //failed
                            return BadRequest(new { message = "An occurred while trying to process Inventory update requests." });
                        }
                    }

                    //insert into warehouse summary
                    if (inv.WarehouseRef != "")
                    {
                        ManageWarehouseSummary whs = new ManageWarehouseSummary();
                        bool wh_req = whs.warehouse_summary_sl_pl(db, t.ItemId, t.Quantity, userId, "Sale");
                        if (wh_req == false)
                        {
                            Console.WriteLine("Errot in updating warehouse summary");
                        }
                    }
                }
            }
            string descrip = String.IsNullOrEmpty(invoiceData.SLDescription) == true ? "" : invoiceData.SLDescription.Replace("'", "\\");
            string statement_descrip = String.IsNullOrEmpty(invoiceData.StatementDescription) == true ? "" : invoiceData.StatementDescription.Replace("'", "\\");
            string filename = null;
            if (invoiceData.lpoFile != null)
            {
                if (invoiceData.lpoFile.Length > 0)
                {
                    string uploads = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "lpos", db);
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    filename = invoiceData.lpoFile.Length > 0 ? invoiceData.lpoFile.FileName : null;
                    if (invoiceData.lpoFile.Length > 0)
                    {
                        string filePath = Path.Combine(uploads, invoiceData.lpoFile.FileName);
                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await invoiceData.lpoFile.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            string status = "Posted";
            //insert invoice header
            cnn.Open();
            string ins_q = "INSERT INTO \"SLInvoiceHeader\" (\"SLJrnlNo\",\"NlJrnlNo\",\"CustCode\",\"TransDate\",\"Period\",\"DocRef\",\"TotalAmount\",\"INVTypeRef\",\"Dispute\",\"DeliveryCust\",\"DeliveryAddress\", " +
                "\"DeliveryDue\",\"INVDate\",\"CustId\",\"PaymentDays\",\"CustomRef\",\"CurrencyId\",\"DueDate\",\"SLDescription\",\"StaffID\",\"DocPrefix\",\"CRNReason\",\"HasCreditNote\",\"Branch\", " +
                "\"InvPrinted\",\"TotalBalance\",\"LpoFile\",\"origin_status\",\"post_number\",\"StatementDescription\") VALUES(" +
                (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + selectedCustomer.CustCode + "', '" + DateTime.Today +
                "', '" + invoiceData.Period + "', '" + invoiceData.DocRef + "'," + total_invoice_amount +",'INV','f'," + invoiceData.DeliveryCust + ",'" + invoiceData.DeliveryAddress + "', '" +
                invoiceData.DeliveryDue + "','" + invoiceData.INVDate + "'," + invoiceData.CustId + ",'" + invoiceData.PaymentDays + "','" + invoiceData.CustomRef + "', " + invoiceData.CurrencyId + ",'" +
                invoiceData.DueDate + "','" + descrip + "'," + userId + ",'" + invoiceData.DocPrefix + "','" + null + "','f'," + staff_branch + ",'f'," + total_invoice_amount + ",'" + filename + "','" + status + "','" + invoiceData.InvoiceNumber + "', '"+ statement_descrip + "' ); ";

            bool myReq2 = myDbconnection.UpdateDelInsert(ins_q, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to save invoice header details." });
            }
            //update customers credits
            cnn.Open();
            string up_cust = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = \"CustomerDept\" + " + total_invoice_amount +
                             " WHERE \"SLCustomerSerial\" = " + selectedCustomer.SLCustomerSerial + " ";
            bool myReq23 = myDbconnection.UpdateDelInsert(up_cust, db);
            cnn.Close();
            if (myReq23 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process Customer update requests." });
            }
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            nlJournalHeader.NlJrnlDesc = "INV" + lastNLJRN + 1;
            nlJournalHeader.TranDate = DateTime.Now;
            nlJournalHeader.MEndDate = invoiceData.DueDate;
            nlJournalHeader.TranYear = invoiceData.PeriodYear;
            nlJournalHeader.TranPeriod = invoiceData.PeriodNumber;
            nlJournalHeader.TranType = "";
            nlJournalHeader.TranFrom = "";
            nlJournalHeader.ModuleId = null;
            nlJournalHeader.SlJrnlNo = lastSLJRN + 1;
            var response = nlService.createNlJournalHeader(nlJournalHeader, "", invoiceData.InvoiceDetailsList, invoiceData.ExchangeRate);
            if (response.Httpcode == 400)
            {
                _log.LogError(response.Message);
                return BadRequest(new { message = response.Message });
            }
            _log.LogInformation($"Invoice of customer {selectedCustomer.CustFirstName} has been created at {DateTime.Now.ToString("dd-MM-yyyy")} of value {String.Format("{0:n}", total_invoice_amount)} ");
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Created Invoice of customer " + selectedCustomer.CustFirstName + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", total_invoice_amount);
            auditTrail.module = "Invoice";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            //success
            return Ok(new { message = "Invoice has been successfully created" });
        }


        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateCustomerInvoiceAsync([FromForm] Invoice invoiceData)
        {
            //check data
            if (string.IsNullOrEmpty(invoiceData.Period))
            {
                return BadRequest(new { message = "Missing Invoice period" });
            }
            _log.LogInformation($"Creating invoice for customer {invoiceData.CustCode} ");
            invoiceData.InvoiceDetailsList = JsonConvert.DeserializeObject<List<Inventorylist>>(invoiceData.test);
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                _log.LogError($"Cannot find required permission parameters. Request terminated.Security verification failed.");
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
                _log.LogError($"Cannot find your client connection route. Page verification failed");
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
            /// ensure system has accounts of  VAT,SALES,DEBTORS
            NlService nlService = new NlService(db);
            //if(nlService.GetNLAccountAccountCodeUsingName("SALES") == null)
            //{
            //    _log.LogError($"Missing SALES account is your system setup please add it in the nl account module");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing SALES account is your system setup please add it in the nl account module"
            //    });
            //}
            //if (nlService.GetNLAccountAccountCodeUsingName("DEBTORS") == null)
            //{
            //    _log.LogError($"Missing DEBTORS account is your system setup please add it in the nl account module");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing DEBTORS account is your system setup please add it in the nl account module"
            //    });
            //}
            //if (nlService.GetNLAccountAccountCodeUsingName("VAT") == null)
            //{
            //    _log.LogError($"Missing VAT account is your system setup please add it in the nl account module");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Missing VAT account is your system setup please add it in the nl account module"
            //    });
            //}
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                _log.LogError($"Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator");
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            //check if customer exists
            string cust_check = "SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ";
            int cust_check_res = myDbconnection.CheckRowExists(cust_check, db);
            if (cust_check_res == 0)
            {
                _log.LogError($"Customer of Id {invoiceData.CustId} was not found");
                return BadRequest(new { message = "No customer found with the parsed data was found. Request cancelled" });
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get selected customer
            AddCustomer selectedCustomer = new AddCustomer();
            cnn.Open();
            NpgsqlDataReader sdr_sc = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ", cnn).ExecuteReader();
            while (sdr_sc.Read())
            {
                selectedCustomer.SLCustomerSerial = (int)sdr_sc["SLCustomerSerial"];
                selectedCustomer.CustCode = sdr_sc["CustCode"].ToString();
                selectedCustomer.CustFirstName = sdr_sc["CustFirstName"] != DBNull.Value ? (string)sdr_sc["CustFirstName"] : null;
                selectedCustomer.Address = sdr_sc["Address"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.CurCode = (int)sdr_sc["CurCode"];
                selectedCustomer.CustEmail = sdr_sc["CustEmail"].ToString();
                selectedCustomer.CustContact = sdr_sc["CustContact"].ToString();
                selectedCustomer.SLCTypeID = (int)sdr_sc["SLCTypeID"];
                selectedCustomer.CustLastName = sdr_sc["CustLastName"] != DBNull.Value ? (string)sdr_sc["CustLastName"] : null;
                selectedCustomer.CustType = sdr_sc["CustType"].ToString();
                selectedCustomer.CustCompany = sdr_sc["CustCompany"] != DBNull.Value ? (string)sdr_sc["CustCompany"] : null;
                selectedCustomer.VATNo = sdr_sc["VATNo"] != DBNull.Value ? (string)sdr_sc["VATNo"] : null;
                selectedCustomer.CustCreditLimit = (float)sdr_sc["CustCreditLimit"];
                selectedCustomer.VATpin = sdr_sc["VATpin"].ToString();
                selectedCustomer.CreditTerms = (int)sdr_sc["CreditTerms"];
                selectedCustomer.CurrCode = sdr_sc["CurCode"].ToString();
                selectedCustomer.CustBranch = sdr_sc["CustBranch"] != DBNull.Value ? (int)sdr_sc["CustBranch"] : 0;
                selectedCustomer.CustRef = sdr_sc["CustRef"] != DBNull.Value ? (string)sdr_sc["CustRef"] : null;
                selectedCustomer.CustomerDept = sdr_sc["CustomerDept"] != DBNull.Value ? (decimal)sdr_sc["CustomerDept"] : 0;
            }
            cnn.Close();
            //check if customer dept is greater
            decimal customerDept = selectedCustomer.CustomerDept;
            //check customer credit limit
            decimal customer_credit_limit = (decimal)selectedCustomer.CustCreditLimit;
            decimal total_invoice_amount = 0;
            foreach (var t in invoiceData.InvoiceDetailsList)
            {
                ///sales
                total_invoice_amount += (t.Total - t.DiscountAmt);
                //Debug.WriteLine(total_invoice_amount);
                //check inventory items quantity if can be deducted
                NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn2.Open();
                Inventory inv = new Inventory();
                NpgsqlDataReader sdr_c = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + t.ItemId + " ", cnn2).ExecuteReader();
                if (sdr_c.Read())
                {
                    inv.InvtName = sdr_c["InvtName"] != DBNull.Value ? (string)sdr_c["InvtName"] : null;
                    inv.InvtQty = sdr_c["InvtQty"] != DBNull.Value ? (int)sdr_c["InvtQty"] : 0;
                    inv.InvtType = sdr_c["InvtType"] != DBNull.Value ? (string)sdr_c["InvtType"] : null;
                    if (inv.InvtType == "GOODS")
                    {
                        if (t.Quantity > inv.InvtQty)
                        {
                            return BadRequest(new { message = "Cannot remove " + t.Quantity + " from your inventory. You only have " + inv.InvtQty + " items left." });
                        }
                    }
                }
                cnn2.Close();
            }
            /// total debt of the customer + total invoice amount = what the customer owes
            decimal ttl_customer_dept = total_invoice_amount + customerDept;
            if (ttl_customer_dept > customer_credit_limit)
            {
                _log.LogError($"Customer code {invoiceData.CustCode}  has exceded thier credit limit ");
                return BadRequest(new { message = "The Invoice Amount " + string.Format("{0:#,0.00}", total_invoice_amount) + " exceeds the customer credit limit of " + string.Format("{0:#,0.00}", customer_credit_limit) + " " });
            }
            //check if invoice settings exists
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("Select * From \"SLInvoiceSettings\" LIMIT 1 ", cnn).ExecuteReader();
            if (sdr2.HasRows == false)
            {
                return BadRequest(new { message = "No data was found with the invoice settings. Please set the invoice  configurations to continue" });
            }
            cnn.Close();
            //get last SLJrnNO
            int lastSLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"SLJrnlNo\") as sl From \"SLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastSLJRN = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            //get last NLJrnNO
            int lastNLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"SLInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;
            }
            cnn.Close();
            //   var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);
            decimal fullPrice = 0;
            decimal vatAmount = 0;
            //insert invoice details
            if (invoiceData.InvoiceDetailsList.Count > 0)
            {
                foreach (var t in invoiceData.InvoiceDetailsList)
                {
                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                    //get inventory item id
                    cnn1.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr_inv = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + t.ItemId + "  ", cnn1).ExecuteReader();
                    while (sdr_inv.Read())
                    {
                        inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                        inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                        inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                        inv.SLProdGrpCode = sdr_inv["SLProdGrpCode"] != DBNull.Value ? (string)sdr_inv["SLProdGrpCode"] : null;
                        inv.WarehouseRef = sdr_inv["WarehouseRef"] != DBNull.Value ? (string)sdr_inv["WarehouseRef"] : null;
                    }
                    cnn1.Close();
                    //get discount data
                    Discounts disc = new Discounts();
                    cnn1.Open();
                    NpgsqlDataReader sdr_disc = new NpgsqlCommand("SELECT * FROM \"Discounts\" WHERE \"DId\" = " + t.Discount + "  ", cnn1).ExecuteReader();
                    if (sdr_disc.Read())
                    {
                        disc.DPerc = sdr_disc["DPerc"] != DBNull.Value ? (float)sdr_disc["DPerc"] : 0;
                    }
                    cnn1.Close();
                    //calculate proce and Qty
                    fullPrice = t.Price * t.Quantity;
                    decimal beforeperrate = t.VatCode * fullPrice;
                    decimal afterperrate = beforeperrate / 100;
                    //calculate VAT
                    vatAmount = afterperrate;
                    cnn1.Open();
                    string unique_id = System.Guid.NewGuid().ToString("D");
                    //check if inventory id exists
                    //string insertQuery1 =
                    //    "INSERT INTO \"SLInvoiceDetail\" (\"SLJrnlNo\", \"JrnlSLNo\", \"VatCode\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ItemSerial\", \"ItemQty\", \"ItemTotals\", \"ItemUnitPrice\", \"DiscountPerc\",\"DiscountAmt\", \"AdditionalDetails\",\"ItemId\",\"ItemCode\",\"ItemName\" ) VALUES(" +
                    //    (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + t.VAT + "', " + vatAmount + ",'" +
                    //    t.Description + "'," + userId + ",'" + unique_id + "', " + t.Quantity + ", " + t.Total + ", " +
                    //    t.Price + ", " + disc.DPerc + ", " + t.DiscountAmt + ", '" + t.additionalDetails + "'," +
                    //    t.ItemId + ",'" + t.ItemCode + "','" + inv.InvtName + "' ); ";
                    //bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                    int inventoryId = inv.SLProdGrpCode == "NA" ? 0 : Int32.Parse(inv.SLProdGrpCode);
                    var productgroupcode = inventoryId > 0 ? nlService.findSlAnalysisCodeById(inventoryId).Id : 0;
                    string insertQuery1 = "INSERT INTO \"SLInvoiceDetail\" (\"SLJrnlNo\", \"JrnlSLNo\", \"VatCode\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ItemSerial\", \"ItemQty\", \"ItemTotals\", \"ItemUnitPrice\"," +
                        " \"DiscountPerc\",\"DiscountAmt\", \"AdditionalDetails\",\"ItemId\",\"ItemCode\",\"ItemName\",\"ProdGroupCode\" ) VALUES(" +
                        (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + t.VatCode + "', " + vatAmount + ",'" +
                        t.StkDesc.Replace("'", "\\") + "'," + userId + ",'" + unique_id + "', " + t.Quantity + ", " + t.Total + ", " +
                        t.Price + ", " + disc.DPerc + ", " + t.DiscountAmt + ", '" + t.StkDesc.Replace("'", "\\") + "'," +
                        t.ItemId + ",'" + t.ItemCode + "','" + inv.InvtName + "','" + productgroupcode + "' ); ";
                    bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                    cnn1.Close();
                    if (myReq1 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occurred while trying to save invoice details." });
                    }
                    if (inv.InvtType == "GOODS")
                    {
                        string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " + t.Quantity +
                                        " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + t.ItemId + " ";
                        bool myReq24 = myDbconnection.UpdateDelInsert(up_inv, db);
                        if (myReq24 == false)
                        {
                            //failed
                            return BadRequest(new { message = "An occurred while trying to process Inventory update requests." });
                        }
                    }
                    //insert into warehouse summary
                    if (inv.WarehouseRef != "")
                    {
                        ManageWarehouseSummary whs = new ManageWarehouseSummary();
                        bool wh_req = whs.warehouse_summary_sl_pl(db, t.ItemId, t.Quantity, userId, "Sale");
                        if (wh_req == false)
                        {
                            //failed
                            //return BadRequest(new
                            //    {message = "An occurred while trying to register request to warehouse summary."});
                            Console.WriteLine("Errot in updating warehouse summary");
                        }
                    }
                }
            }
            string descrip = String.IsNullOrEmpty(invoiceData.SLDescription) == true ? "" : invoiceData.SLDescription.Replace("'", "\\");
            string statement_descrip = String.IsNullOrEmpty(invoiceData.StatementDescription) == true ? "" : invoiceData.StatementDescription.Replace("'", "\\");
            string filename = null;
            if (invoiceData.lpoFile != null)
            {
                if (invoiceData.lpoFile.Length > 0)
                {
                    string uploads = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "lpos", db);
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    filename = invoiceData.lpoFile.Length > 0 ? invoiceData.lpoFile.FileName : null;
                    if (invoiceData.lpoFile.Length > 0)
                    {
                        string filePath = Path.Combine(uploads, invoiceData.lpoFile.FileName);
                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await invoiceData.lpoFile.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            //insert invoice header
            cnn.Open();
            string ins_q = "INSERT INTO \"SLInvoiceHeader\" (\"SLJrnlNo\",\"NlJrnlNo\",\"CustCode\",\"TransDate\",\"Period\",\"DocRef\",\"TotalAmount\",\"INVTypeRef\",\"Dispute\",\"DeliveryCust\",\"DeliveryAddress\"," +
                "\"DeliveryDue\",\"INVDate\",\"CustId\",\"PaymentDays\",\"CustomRef\",\"CurrencyId\",\"DueDate\",\"SLDescription\",\"StaffID\",\"DocPrefix\",\"CRNReason\",\"HasCreditNote\",\"Branch\",\"InvPrinted\",\"TotalBalance\",\"LpoFile\",\"StatementDescription\") VALUES(" +
                (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + selectedCustomer.CustCode + "', '" + DateTime.Today +
                "', '" + invoiceData.Period + "', '" + invoiceData.DocRef + "'," + total_invoice_amount +
                ",'INV','f'," + invoiceData.DeliveryCust + ",'" + invoiceData.DeliveryAddress + "', '" +
                invoiceData.DeliveryDue + "','" + invoiceData.INVDate + "'," + invoiceData.CustId + ",'" +
                invoiceData.PaymentDays + "','" + invoiceData.CustomRef + "', " + invoiceData.CurrencyId + ",'" +
                invoiceData.DueDate + "','" + descrip + "'," + userId + ",'" + invoiceData.DocPrefix +
                "','" + null + "','f'," + staff_branch + ",'f'," + total_invoice_amount + ",'" + filename + "', '"+ statement_descrip +"' ); ";

            bool myReq2 = myDbconnection.UpdateDelInsert(ins_q, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to save invoice header details." });
            }
            //update customers credits
            cnn.Open();
            string up_cust = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = \"CustomerDept\" + " + total_invoice_amount +
                             " WHERE \"SLCustomerSerial\" = " + selectedCustomer.SLCustomerSerial + " ";
            bool myReq23 = myDbconnection.UpdateDelInsert(up_cust, db);
            cnn.Close();
            if (myReq23 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process Customer update requests." });
            }
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            nlJournalHeader.NlJrnlDesc = "INV" + lastNLJRN + 1;
            nlJournalHeader.TranDate = DateTime.Now;
            nlJournalHeader.MEndDate = invoiceData.DueDate;
            nlJournalHeader.TranYear = invoiceData.PeriodYear;
            nlJournalHeader.TranPeriod = invoiceData.PeriodNumber;
            nlJournalHeader.TranType = "";
            nlJournalHeader.TranFrom = "";
            nlJournalHeader.ModuleId = null;
            nlJournalHeader.SlJrnlNo = lastSLJRN + 1;
            var response = nlService.createNlJournalHeader(nlJournalHeader, "", invoiceData.InvoiceDetailsList, invoiceData.ExchangeRate);
            if (response.Httpcode == 400)
            {
                _log.LogError(response.Message);
                return BadRequest(new { message = response.Message });
            }
            _log.LogInformation($"Invoice of customer {selectedCustomer.CustFirstName} has been created at {DateTime.Now.ToString("dd-MM-yyyy")} of value {String.Format("{0:n}", total_invoice_amount)} ");
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Created Invoice of customer " + selectedCustomer.CustFirstName + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", total_invoice_amount);
            auditTrail.module = "Invoice";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            //success
            return Ok(new { message = "Invoice has been successfully created" });
        }

        [Route("GetAll")]
        [HttpGet]
        [Authorize]
        public ActionResult<Invoice> GetAll()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _log.LogInformation($"Fetching all invoices");
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new
            //    {
            //        message =
            //            "Cannot find required permission parameters. Request terminated.Security verification failed."
            //    });
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
                _log.LogError($"DatabaseReference could not be found");
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
            //    _log.LogError($"User {userId} doesnt have rights to view this data");
            //    return BadRequest(new
            //    {
            //        message =
            //            "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
            //    });
            //}
            //get database name
            string db = companyRes;
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                _log.LogError($"Staff branch could not be found");
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            _log.LogInformation($"Fetching all invoices");
            //get all Invoices
            List<Invoice> invList = new List<Invoice>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();

            //NpgsqlDataReader sdr = new NpgsqlCommand(
            //    "Select \"SLInvoiceHeader\".*, \"SLCustomer\".\"CustCode\" As sl_custcode, \"SLCustomer\".\"CustFirstName\", \"SLCustomer\".\"CustLastName\", \"SLCustomer\".\"CustType\"," +
            //    "\"SLCustomer\".\"CustEmail\", \"SLCustomer\".\"CustCompany\", \"Currencies\".\"CrName\", \"Currencies\".\"CrCode\", " +
            //    "\"Users\".\"UFirstName\", \"Users\".\"ULastName\", \"Users\".\"UEmail\", financial_periods.fp_name " +
            //    "From \"SLInvoiceHeader\" Left Join \"SLCustomer\" On \"SLCustomer\".\"SLCustomerSerial\" = \"SLInvoiceHeader\".\"CustId\" " +
            //    "Left Join \"Currencies\" On \"Currencies\".\"CrId\" = \"SLInvoiceHeader\".\"CurrencyId\"" +
            //    " Left Join \"Users\" On \"Users\".\"UId\" = \"SLInvoiceHeader\".\"StaffID\" " +
            //    "Left Join financial_periods On financial_periods.fp_ref = \"SLInvoiceHeader\".\"Period\" WHERE \"INVTypeRef\" = 'INV' AND \"Branch\" = " +staff_branch + "  ", cnn).ExecuteReader();
            NpgsqlDataReader sdr = new NpgsqlCommand(
                "Select \"SLInvoiceHeader\".*, \"SLCustomer\".\"CustCode\" As sl_custcode, \"SLCustomer\".\"CustFirstName\", \"SLCustomer\".\"CustLastName\", \"SLCustomer\".\"CustType\", \"NlJournalHeader\".\"NlJrnlNo\" AS journalref, " +
                "\"SLCustomer\".\"CustEmail\", \"SLCustomer\".\"CustCompany\", \"Currencies\".\"CrName\", \"Currencies\".\"CrCode\", \"Users\".\"UFirstName\", \"Users\".\"ULastName\", \"Users\".\"UEmail\", financial_periods.fp_name " +
                "From \"SLInvoiceHeader\" Left Join \"SLCustomer\" On \"SLCustomer\".\"SLCustomerSerial\" = \"SLInvoiceHeader\".\"CustId\" Left Join \"Currencies\" On \"Currencies\".\"CrId\" = \"SLInvoiceHeader\".\"CurrencyId\"" +
                "Left Join \"Users\" On \"Users\".\"UId\" = \"SLInvoiceHeader\".\"StaffID\" LEFT JOIN \"NlJournalHeader\" ON \"NlJournalHeader\".\"SlJrnlNo\" = \"SLInvoiceHeader\".\"SLJrnlNo\"  " +
                "Left Join financial_periods On financial_periods.fp_ref = \"SLInvoiceHeader\".\"Period\" WHERE  \"Branch\" = " + staff_branch + "  AND \"NlJournalHeader\".\"TranFrom\" = 'SL'  ", cnn).ExecuteReader();

            while (sdr.Read())
            {
                Invoice inv = new Invoice();
                inv.SLJrnlNo = sdr["SLJrnlNo"] != DBNull.Value ? (int)sdr["SLJrnlNo"] : 0;
                inv.NlJrnlNo = sdr["NlJrnlNo"] != DBNull.Value ? (int)sdr["NlJrnlNo"] : 0;
                inv.CustCode = sdr["CustCode"] != DBNull.Value ? (string)sdr["CustCode"] : null;
                inv.TransDate = sdr["TransDate"] != DBNull.Value ? (DateTime)sdr["TransDate"] : DateTime.Today;
                inv.Period = sdr["Period"] != DBNull.Value ? (string)sdr["Period"] : null;
                inv.DocRef = sdr["DocRef"] != DBNull.Value ? (int)sdr["DocRef"] : 0;
                inv.TotalAmount = sdr["TotalAmount"] != DBNull.Value ? (decimal)sdr["TotalAmount"] : 0;
                inv.INVTypeRef = sdr["INVTypeRef"] != DBNull.Value ? (string)sdr["INVTypeRef"] : null;
                inv.Dispute = sdr["Dispute"] != DBNull.Value ? (bool)sdr["Dispute"] : false;
                inv.DeliveryCust = sdr["DeliveryCust"] != DBNull.Value ? (int)sdr["DeliveryCust"] : 0;
                inv.DeliveryAddress = sdr["DeliveryAddress"] != DBNull.Value ? (string)sdr["DeliveryAddress"] : null;
                inv.DeliveryDue = sdr["DeliveryDue"] != DBNull.Value ? (DateTime)sdr["DeliveryDue"] : DateTime.Today;
                inv.INVDate = sdr["INVDate"] != DBNull.Value ? (DateTime)sdr["INVDate"] : DateTime.Today;
                inv.CustId = sdr["CustId"] != DBNull.Value ? (int)sdr["CustId"] : 0;
                inv.PaymentDays = sdr["PaymentDays"] != DBNull.Value ? (int)sdr["PaymentDays"] : 0;
                inv.CustomRef = sdr["CustomRef"] != DBNull.Value ? (string)sdr["CustomRef"] : null;
                inv.CurrencyId = sdr["CurrencyId"] != DBNull.Value ? (int)sdr["CurrencyId"] : 0;
                inv.DueDate = sdr["DueDate"] != DBNull.Value ? (DateTime)sdr["DueDate"] : DateTime.Today;
                inv.SLDescription = sdr["SLDescription"] != DBNull.Value ? (string)sdr["SLDescription"] : null;
                inv.StaffID = sdr["StaffID"] != DBNull.Value ? (int)sdr["StaffID"] : 0;
                inv.DocPrefix = sdr["DocPrefix"] != DBNull.Value ? (string)sdr["DocPrefix"] : null;
                inv.CRNReason = sdr["CRNReason"] != DBNull.Value ? (string)sdr["CRNReason"] : null;
                inv.HasCreditNote = sdr["HasCreditNote"] != DBNull.Value ? (bool)sdr["HasCreditNote"] : false;
                inv.Branch = sdr["Branch"] != DBNull.Value ? (int)sdr["Branch"] : 0;
                inv.InvPrinted = sdr["InvPrinted"] != DBNull.Value ? (bool)sdr["InvPrinted"] : false;
                inv.TotalBalance = sdr["TotalBalance"] != DBNull.Value ? (decimal)sdr["TotalBalance"] : 0;
                inv.CreditNoteAmount = sdr["CreditNoteAmount"] != DBNull.Value ? (decimal)sdr["CreditNoteAmount"] : 0;
                inv.journalRef = sdr["journalref"] != DBNull.Value ? (int)sdr["journalref"] : 0;
                inv.status = sdr["origin_status"] != DBNull.Value ? (string)sdr["origin_status"] : "";
                inv.InvoiceNumber = sdr["post_number"] != DBNull.Value ? (string)sdr["post_number"] : "";
                inv.StatementDescription = sdr["StatementDescription"] != DBNull.Value ? (string)sdr["StatementDescription"] : "";
                inv.IsReversed = sdr["IsReversed"] != DBNull.Value ? (bool)sdr["IsReversed"] : false;
                inv.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                inv.CustLastname = sdr["CustLastname"] != DBNull.Value ? (string)sdr["CustLastname"] : null;
                inv.CustEmail = sdr["CustEmail"] != DBNull.Value ? (string)sdr["CustEmail"] : null;
                inv.CustType = sdr["CustType"] != DBNull.Value ? (string)sdr["CustType"] : null;
                inv.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                inv.CrName = sdr["CrName"] != DBNull.Value ? (string)sdr["CrName"] : null;
                inv.CrCode = sdr["CrCode"] != DBNull.Value ? (string)sdr["CrCode"] : null;
                inv.UFirstName = sdr["UFirstName"] != DBNull.Value ? (string)sdr["UFirstName"] : null;
                inv.ULastName = sdr["ULastName"] != DBNull.Value ? (string)sdr["ULastName"] : null;
                inv.UEmail = sdr["UEmail"] != DBNull.Value ? (string)sdr["UEmail"] : null;
                inv.fp_name = sdr["fp_name"] != DBNull.Value ? (string)sdr["fp_name"] : null;
                inv.formatedduedate = DateTime.ParseExact(inv.DueDate.ToString("dd-MM-yyyy"), "dd-MM-yyyy", null);
                inv.NewInvoiceNumber = sdr["invoicenumber"] != DBNull.Value ? (int)sdr["invoicenumber"] : 0;
                invList.Add(inv);
            }
            cnn.Close();
            var filtered_data = invList.Where(x => x.TotalAmount != x.CreditNoteAmount).ToList();
            return Ok(new { InvoiceData = filtered_data });
        }
        [Route("GetInvoicePageReport")]
        [HttpGet]
        [Authorize]
        public ActionResult<Invoice> GetInvoicePageReport(string INVJRN)
        {
            try
            {
                if (string.IsNullOrEmpty(INVJRN) || INVJRN == "0")
                {
                    return BadRequest(new { message = "Cannot find required invoice journal reference " });
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
                //    return BadRequest(new
                //    {
                //        message =
                //            "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
                //    });
                //}
                //get database name
                string db = companyRes;
                //get staff branch
                int staff_branch = myDbconnection.GetStaffBranch(userId, db);
                if (staff_branch == 0)
                {
                    return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
                }
                //get all Invoices
                Invoice inv = new Invoice();
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn.Open();
                string query1 = "Select \"SLInvoiceHeader\".*, \"SLCustomer\".\"CustCode\" As sl_custcode, \"SLCustomer\".\"CustFirstName\", \"SLCustomer\".\"CustLastName\", \"SLCustomer\".\"CustType\",\"SLCustomer\".\"CustEmail\"," +
                    " \"SLCustomer\".\"CustCompany\", \"Currencies\".\"CrName\", \"Currencies\".\"CrCode\", \"Users\".\"UFirstName\", \"Users\".\"ULastName\", \"Users\".\"UEmail\", financial_periods.fp_name " +
                    "From \"SLInvoiceHeader\" Left Join \"SLCustomer\" On \"SLCustomer\".\"SLCustomerSerial\" = \"SLInvoiceHeader\".\"CustId\"  " +
                    "Left Join \"Currencies\" On \"Currencies\".\"CrId\" = \"SLInvoiceHeader\".\"CurrencyId\"" +
                    " Left Join \"Users\" On \"Users\".\"UId\" = \"SLInvoiceHeader\".\"StaffID\" " +
                    "Left Join financial_periods On financial_periods.fp_ref = \"SLInvoiceHeader\".\"Period\"" +
                    " WHERE \"SLJrnlNo\" = '" + INVJRN + "' AND \"Branch\" = " + staff_branch + "";

                NpgsqlDataReader sdr = new NpgsqlCommand(query1, cnn).ExecuteReader();
                if (sdr.Read())
                {
                    inv.SLJrnlNo = sdr["SLJrnlNo"] != DBNull.Value ? (int)sdr["SLJrnlNo"] : 0;
                    inv.NlJrnlNo = sdr["NlJrnlNo"] != DBNull.Value ? (int)sdr["NlJrnlNo"] : 0;
                    inv.CustCode = sdr["CustCode"] != DBNull.Value ? (string)sdr["CustCode"] : null;
                    inv.TransDate = sdr["TransDate"] != DBNull.Value ? (DateTime)sdr["TransDate"] : DateTime.Today;
                    inv.Period = sdr["Period"] != DBNull.Value ? (string)sdr["Period"] : null;
                    inv.DocRef = sdr["DocRef"] != DBNull.Value ? (int)sdr["DocRef"] : 0;
                    inv.SLDescription = sdr["SLDescription"] != DBNull.Value ? (string)sdr["SLDescription"] : "";
                    inv.TotalAmount = sdr["TotalAmount"] != DBNull.Value ? (decimal)sdr["TotalAmount"] : 0;
                    inv.INVTypeRef = sdr["INVTypeRef"] != DBNull.Value ? (string)sdr["INVTypeRef"] : null;
                    inv.Dispute = sdr["Dispute"] != DBNull.Value ? (bool)sdr["Dispute"] : false;
                    inv.DeliveryCust = sdr["DeliveryCust"] != DBNull.Value ? (int)sdr["DeliveryCust"] : 0;
                    inv.DeliveryAddress = sdr["DeliveryAddress"] != DBNull.Value ? (string)sdr["DeliveryAddress"] : null;
                    inv.DeliveryDue = sdr["DeliveryDue"] != DBNull.Value ? (DateTime)sdr["DeliveryDue"] : DateTime.Today;
                    inv.INVDate = sdr["INVDate"] != DBNull.Value ? (DateTime)sdr["INVDate"] : DateTime.Today;
                    inv.CustId = sdr["CustId"] != DBNull.Value ? (int)sdr["CustId"] : 0;
                    inv.PaymentDays = sdr["PaymentDays"] != DBNull.Value ? (int)sdr["PaymentDays"] : 0;
                    inv.CustomRef = sdr["CustomRef"] != DBNull.Value ? (string)sdr["CustomRef"] : null;
                    inv.CurrencyId = sdr["CurrencyId"] != DBNull.Value ? (int)sdr["CurrencyId"] : 0;
                    inv.DueDate = sdr["DueDate"] != DBNull.Value ? (DateTime)sdr["DueDate"] : DateTime.Today;
                    inv.SLDescription = sdr["SLDescription"] != DBNull.Value ? (string)sdr["SLDescription"] : null;
                    inv.StatementDescription = sdr["StatementDescription"] != DBNull.Value ? (string)sdr["StatementDescription"] : null;
                    inv.StaffID = sdr["StaffID"] != DBNull.Value ? (int)sdr["StaffID"] : 0;
                    inv.DocPrefix = sdr["DocPrefix"] != DBNull.Value ? (string)sdr["DocPrefix"] : null;
                    inv.CRNReason = sdr["CRNReason"] != DBNull.Value ? (string)sdr["CRNReason"] : null;
                    inv.HasCreditNote = sdr["HasCreditNote"] != DBNull.Value ? (bool)sdr["HasCreditNote"] : false;
                    inv.Branch = sdr["Branch"] != DBNull.Value ? (int)sdr["Branch"] : 0;
                    inv.InvPrinted = sdr["InvPrinted"] != DBNull.Value ? (bool)sdr["InvPrinted"] : false;
                    inv.TotalBalance = sdr["TotalBalance"] != DBNull.Value ? (decimal)sdr["TotalBalance"] : 0;
                    inv.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                    inv.CustLastname = sdr["CustLastname"] != DBNull.Value ? (string)sdr["CustLastname"] : null;
                    inv.CustEmail = sdr["CustEmail"] != DBNull.Value ? (string)sdr["CustEmail"] : null;
                    inv.CustType = sdr["CustType"] != DBNull.Value ? (string)sdr["CustType"] : null;
                    inv.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                    inv.CrName = sdr["CrName"] != DBNull.Value ? (string)sdr["CrName"] : null;
                    inv.CrCode = sdr["CrCode"] != DBNull.Value ? (string)sdr["CrCode"] : null;
                    inv.UFirstName = sdr["UFirstName"] != DBNull.Value ? (string)sdr["UFirstName"] : null;
                    inv.ULastName = sdr["ULastName"] != DBNull.Value ? (string)sdr["ULastName"] : null;
                    inv.UEmail = sdr["UEmail"] != DBNull.Value ? (string)sdr["UEmail"] : null;
                    inv.fp_name = sdr["fp_name"] != DBNull.Value ? (string)sdr["fp_name"] : null;
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
                DeliverTo deliverTo = new DeliverTo();
                cnn.Open();
                NpgsqlDataReader sdr41 = new NpgsqlCommand("Select * From \"SLCustomer\" WHERE \"SLCustomer\".\"SLCustomerSerial\" = '" + inv.DeliveryCust + "'   ", cnn).ExecuteReader();
                if (sdr41.Read())
                {
                    deliverTo.CustFirstName = sdr41["CustFirstName"] != DBNull.Value ? (string)sdr41["CustFirstName"] : "";
                    deliverTo.CustLastname = sdr41["CustLastname"] != DBNull.Value ? (string)sdr41["CustLastname"] : "";
                    deliverTo.CustType = sdr41["CustType"] != DBNull.Value ? (string)sdr41["CustType"] : "";
                    deliverTo.CustCompany = sdr41["CustCompany"] != DBNull.Value ? (string)sdr41["CustCompany"] : "";
                    deliverTo.Address = inv.DeliveryAddress;
                }
                cnn.Close();
                inv.DeliverToData = deliverTo;
                string img_path = "";
                //upload image if base64
                bool url_is_base64 = IsBase64String(lic.CompanyLogo);
                if (String.IsNullOrEmpty(lic.CompanyLogo))
                {
                    //upload image
                    //using ngenx.jpg for test purpose
                    lic.CompanyLogo = "invoice_default.jpg";
                    //  lic.CompanyLogo = "ngenx.jpg";
                    img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                    //Check if directory exist
                    //if (!System.IO.Directory.Exists(img_path))
                    //{
                    //    return BadRequest(new { message = "The path to upload account profile does NOT exist" });
                    //}
                    //     string rand_imageName = System.Guid.NewGuid().ToString("D") + ".jpg";
                    //set the image path
                    string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                    //write file
                    //       System.IO.File.WriteAllBytes(full_imgPath, Convert.FromBase64String(lic.CompanyLogo));
                    //      img_path = rand_imageName;
                    byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                    lic.CompanyLogo = Convert.ToBase64String(imageArray); ;
                }
                else
                {
                    //remove prefix
                    //recvData.ProdImage = recvData.ProdImage.Substring(lic.CompanyLogo.LastIndexOf(',') + 1);
                    //upload image
                    //    img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "profiles");
                    img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                    //Check if directory exist
                    //if (!System.IO.Directory.Exists(img_path))
                    //{
                    //    return BadRequest(new { message = "The path to upload account profile does NOT exist" });
                    //}
                    //string rand_imageName = System.Guid.NewGuid().ToString("D") + ".jpg";
                    //set the image path
                    string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                    //write file
                    //System.IO.File.WriteAllBytes(full_imgPath, Convert.FromBase64String(lic.CompanyLogo));
                    byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                    lic.CompanyLogo = Convert.ToBase64String(imageArray);
                    //img_path = rand_imageName;
                    //lic.CompanyLogo = img_path;
                }
                //get Delivery customer details
                DeliveryCustomer dc = new DeliveryCustomer();
                string query = "Select \"SLCustomer\".*,\"Currencies\".\"CrCode\" From \"SLCustomer\" LEFT JOIN \"Currencies\"  on \"Currencies\".\"CrId\" = \"SLCustomer\".\"CurCode\" " +
                    " WHERE \"SLCustomerSerial\" = " + inv.DeliveryCust + " ";
                cnn.Open();
                NpgsqlDataReader sdr2 = new NpgsqlCommand(query, cnn).ExecuteReader();
                if (sdr2.Read())
                {
                    dc.CustFirstName = sdr2["CustFirstName"].ToString();
                    dc.CustLastName = sdr2["CustLastName"].ToString();
                    dc.CustEmail = sdr2["CustEmail"].ToString();
                    dc.CustCompany = sdr2["CustCompany"].ToString();
                    dc.CustType = sdr2["CustType"].ToString();
                    dc.Postal = sdr2["PostalAddress"].ToString();
                    dc.Address = sdr2["Address"].ToString();
                    dc.Tel = sdr2["CustContact"].ToString();
                    dc.Currency = sdr2["CrCode"].ToString();
                }
                cnn.Close();
                ///IMAGE
                //string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "profiles");
                //string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                //byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                //lic.CompanyLogo = base64ImageRepresentation;
                //get invoice list details
                List<InvoiceDetails> invDetailsList = new List<InvoiceDetails>();
                cnn.Open();
                string query2 = "Select * From \"SLInvoiceDetail\" WHERE \"SLJrnlNo\" = " + INVJRN + " ";
                NpgsqlDataReader sdr3 = new NpgsqlCommand(query2, cnn).ExecuteReader();
                while (sdr3.Read())
                {
                    InvoiceDetails invDetail = new InvoiceDetails();
                    invDetail.SLJrnlNo = (int)sdr3["SLJrnlNo"];
                    invDetail.JrnlSLNo = (int)sdr3["JrnlSLNo"];
                    invDetail.VatCode = (string)sdr3["VatCode"];
                    invDetail.VatAmt = (float)(decimal)sdr3["VatAmt"];
                    invDetail.ItemQty = (int)sdr3["ItemQty"];
                    invDetail.ItemTotals = (float)(decimal)sdr3["ItemTotals"];
                    invDetail.ItemUnitPrice = (float)(decimal)sdr3["ItemUnitPrice"];
                    invDetail.Currency = inv.CrCode;
                    invDetail.DiscountPerc = sdr3["DiscountPerc"] != DBNull.Value ? (float)(decimal)sdr3["DiscountPerc"] : 0;
                    invDetail.DiscountAmt = sdr3["DiscountAmt"] != DBNull.Value ? (float)(decimal)sdr3["DiscountAmt"] : 0;
                    invDetail.AdditionalDetails = sdr3["AdditionalDetails"] != DBNull.Value ? (string)sdr3["AdditionalDetails"] : null;
                    invDetail.StkDesc = sdr3["ItemName"] != DBNull.Value ? (string)sdr3["ItemName"] : null;
                    invDetail.ProdGroupCode = sdr3["ItemCode"] != DBNull.Value ? (string)sdr3["ItemCode"] : null;
                    invDetailsList.Add(invDetail);
                }
                cnn.Close();
                //   Get invoice terms and conditions
                Allterms invterms = new Allterms();
                cnn.Open();
                NpgsqlDataReader sdr4 = new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'inv_terms' AND branch = " + staff_branch + " ", cnn).ExecuteReader();
                if (sdr4.HasRows == false)
                {
                    return BadRequest(new { message = "Sorry! the sales invoice terms have NOT been set or are NOT available in your branch. Please set the terms of sale to continue" });
                }
                while (sdr4.Read())
                {
                    invterms.tosID = sdr4["tosID"] != DBNull.Value ? (int)sdr4["tosID"] : 0;
                    invterms.tosType = sdr4["tosType"] != DBNull.Value ? (string)sdr4["tosType"] : null;
                    invterms.terms = sdr4["terms"] != DBNull.Value ? sdr4["terms"].ToString() : null;
                }
                cnn.Close();
                // Decode the content for showing on Web page.
                invterms.terms = WebUtility.HtmlDecode(invterms.terms);
                //get invoice settings
                InvoiceSettings inv_settings = new InvoiceSettings();
                cnn.Open();
                NpgsqlDataReader sdr4_1 = new NpgsqlCommand("SELECT * FROM \"SLInvoiceSettings\" WHERE \"InvType\" = 'INV' AND \"InvBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
                if (sdr4_1.HasRows == false)
                {
                    return BadRequest(new { message = "Sorry! NO invoice settings were found in your branch. Request terminated prematurely " });
                }
                while (sdr4_1.Read())
                {
                    inv_settings.InvSettingId = sdr4_1["InvSettingId"] != DBNull.Value ? (int)sdr4_1["InvSettingId"] : 0;
                    inv_settings.InvPrefix = sdr4_1["InvPrefix"] != DBNull.Value ? (string)sdr4_1["InvPrefix"] : null;
                    inv_settings.InvStartNumber = sdr4_1["InvStartNumber"] != DBNull.Value ? (int)sdr4_1["InvStartNumber"] : 0;
                    inv_settings.InvNumberingType = sdr4_1["InvNumberingType"] != DBNull.Value ? (string)sdr4_1["InvNumberingType"] : null;
                    inv_settings.InvDeliveryNotes = sdr4_1["InvDeliveryNotes"] != DBNull.Value ? (int)sdr4_1["InvDeliveryNotes"] : 0;
                }
                cnn.Close();
                //get invoices count
                List<InvoiceTypes> inv_types_list = new List<InvoiceTypes>();
                cnn.Open();
                NpgsqlDataReader sdr4_2 = new NpgsqlCommand("SELECT * FROM \"SLInvoiceTypes\" ", cnn).ExecuteReader();
                if (sdr4_2.HasRows == false)
                {
                    return BadRequest(new { message = "Sorry! NO invoice types were found in your branch. Request terminated prematurely " });
                }
                while (sdr4_2.Read())
                {
                    InvoiceTypes invtype = new InvoiceTypes();
                    invtype.INVypeID = sdr4_2["INVypeID"] != DBNull.Value ? (int)sdr4_2["INVypeID"] : 0;
                    invtype.INVType = sdr4_2["INVType"] != DBNull.Value ? (string)sdr4_2["INVType"] : null;
                    invtype.INVComment = sdr4_2["INVComment"] != DBNull.Value ? (string)sdr4_2["INVComment"] : null;
                    inv_types_list.Add(invtype);
                }
                cnn.Close();
                //get invoices count
                List<InvoiceTypes> dn_types_list = new List<InvoiceTypes>();
                cnn.Open();
                NpgsqlDataReader sdr4_3 = new NpgsqlCommand("SELECT * FROM \"DeliveryNoteTypes\" ", cnn).ExecuteReader();
                if (sdr4_3.HasRows == false)
                {
                    return BadRequest(new { message = "Sorry! NO invoice types were found in your branch. Request terminated prematurely " });
                }
                while (sdr4_3.Read())
                {
                    InvoiceTypes invtype = new InvoiceTypes();
                    invtype.INVypeID = sdr4_3["Id"] != DBNull.Value ? (int)sdr4_3["Id"] : 0;
                    invtype.INVType = sdr4_3["Type"] != DBNull.Value ? (string)sdr4_3["Type"] : null;
                    invtype.INVComment = sdr4_3["Comment"] != DBNull.Value ? (string)sdr4_3["Comment"] : null;
                    dn_types_list.Add(invtype);
                }
                cnn.Close();
                return Ok(new
                {
                    InvoiceHeader = inv,
                    CompanyData = lic,
                    DeliveryCust = dc,
                    InvoiceDetails = invDetailsList,
                    InvTerms = invterms,
                    DnTypeList = dn_types_list,
                    InvSettings = inv_settings,
                    InvTypeList = inv_types_list
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }
        private static bool IsBase64String(string base64)
        {
            try
            {
                //remove header from base 64
                string result = Regex.Replace(base64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(result);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                // return (base64.Replace(" ", "").Length % 4 == 0);
                return true;
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetAddress(int selectID)
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
            //Get customer selected data
            AddCustomer selectCust = new AddCustomer();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //check if invoice settings exists
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + selectID + " ", cnn).ExecuteReader();
            while (sdr2.Read())
            {
                selectCust.Address = sdr2["Address"] != DBNull.Value ? (string)sdr2["Address"] : null;
            }
            cnn.Close();
            return Ok(new { deliveryCust = selectCust });
        }
        [Route("MarkDispute")]
        [HttpPut]
        [Authorize]
        public ActionResult<Invoice> MarkDispute(int INVJRN)
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
            //Update request
            string myQuery = "UPDATE \"SLInvoiceHeader\" SET \"Dispute\" = 't' WHERE \"SLJrnlNo\" = " + INVJRN + "  ";
            bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
            if (reqStatus == true)
            {
                return Ok(new { message = "Invoice has been successfully marked as dispute" });
            }
            else
            {
                return BadRequest(new { message = "An error occurred while marking invoice as dispute" });
            }
        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult<Invoice> AddCreditNote(int INVSJN, [FromBody] Invoice sendData)
        {
            if (string.IsNullOrEmpty(sendData.CRNReason))
            {
                return BadRequest(new { message = "Cannot find the reason why you are parsing a credit note." });
            }
            if (sendData.CreditNoteAmount == 0)
            {
                return BadRequest(new { message = "Please set credit note amount" });
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
            NlService nlService = new NlService(db);
            //if(nlService.GetNLAccountAccountCodeUsingName("SALES RETURN") == null)
            //{
            //    _log.LogError($"Account SALES RETURN was not found");
            //    return BadRequest(new
            //    {
            //        message =
            //         "Sorry!Can't complete this transaction .Create Account  (SALES RETURN) in the Nominal Module"
            //    });
            //}
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"});
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get invoice details
            Invoice inv = new Invoice();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * From \"SLInvoiceHeader\" WHERE \"SLJrnlNo\" = " + INVSJN + " ", cnn).ExecuteReader();
            if (sdr1.HasRows == false)
            {
                _log.LogError($"Refrence {INVSJN} for the credit note was not found");
                //empty
                cnn.Close();
                return BadRequest(new { message = "The Sales journal reference was NOT found." });
            }
            if (sdr1.Read())
            {
                inv.SLJrnlNo = sdr1["SLJrnlNo"] != DBNull.Value ? (int)sdr1["SLJrnlNo"] : 0;
                inv.NlJrnlNo = sdr1["NlJrnlNo"] != DBNull.Value ? (int)sdr1["NlJrnlNo"] : 0;
                inv.CustCode = sdr1["CustCode"] != DBNull.Value ? (string)sdr1["CustCode"] : null;
                inv.TransDate = sdr1["TransDate"] != DBNull.Value ? (DateTime)sdr1["TransDate"] : DateTime.Today;
                inv.Period = sdr1["Period"] != DBNull.Value ? (string)sdr1["Period"] : null;
                inv.DocRef = sdr1["DocRef"] != DBNull.Value ? (int)sdr1["DocRef"] : 0;
                inv.TotalAmount = sdr1["TotalAmount"] != DBNull.Value ? (decimal)sdr1["TotalAmount"] : 0;
                inv.INVTypeRef = sdr1["INVTypeRef"] != DBNull.Value ? (string)sdr1["INVTypeRef"] : null;
                inv.Dispute = (bool)sdr1["Dispute"];
                inv.DeliveryCust = sdr1["DeliveryCust"] != DBNull.Value ? (int)sdr1["DeliveryCust"] : 0;
                inv.DeliveryAddress = sdr1["DeliveryAddress"] != DBNull.Value ? (string)sdr1["DeliveryAddress"] : null;
                inv.DeliveryDue = sdr1["DeliveryDue"] != DBNull.Value ? (DateTime)sdr1["DeliveryDue"] : DateTime.Today;
                inv.INVDate = sdr1["INVDate"] != DBNull.Value ? (DateTime)sdr1["INVDate"] : DateTime.Today;
                inv.CustId = sdr1["CustId"] != DBNull.Value ? (int)sdr1["CustId"] : 0;
                inv.PaymentDays = sdr1["PaymentDays"] != DBNull.Value ? (int)sdr1["PaymentDays"] : 0;
                inv.CustomRef = sdr1["CustomRef"] != DBNull.Value ? (string)sdr1["CustomRef"] : null;
                inv.CurrencyId = (int)sdr1["CurrencyId"];
                inv.DueDate = (DateTime)sdr1["DueDate"];
                inv.SLDescription = (string)sdr1["SLDescription"];
                inv.StatementDescription = sdr1["StatementDescription"] != DBNull.Value ? (string)sdr1["StatementDescription"] : null;
                inv.StaffID = sdr1["StaffID"] != DBNull.Value ? (int)sdr1["StaffID"] : 0;
                inv.DocPrefix = sdr1["DocPrefix"] != DBNull.Value ? (string)sdr1["DocPrefix"] : null;
                inv.CRNReason = sdr1["CRNReason"] != DBNull.Value ? (string)sdr1["CRNReason"] : null;
                inv.HasCreditNote = sdr1["HasCreditNote"] != DBNull.Value ? (bool)sdr1["HasCreditNote"] : false;
                inv.Branch = sdr1["Branch"] != DBNull.Value ? (int)sdr1["Branch"] : 0;
                inv.InvPrinted = sdr1["InvPrinted"] != DBNull.Value ? (bool)sdr1["InvPrinted"] : false;
                inv.TotalBalance = sdr1["TotalBalance"] != DBNull.Value ? (decimal)sdr1["TotalBalance"] : 0;
            }

            cnn.Close();
            inv.CreditNoteAmount = sendData.CreditNoteAmount;
            //check if has credit note
            if (inv.HasCreditNote == true)
            {
                _log.LogError($"This invoice already has a credit note");
                return BadRequest(new { message = "The invoice ref " + inv.DocPrefix + inv.SLJrnlNo.ToString("D5") +  " already indicated that it has a creditnote attached."});
            }
            //if (inv.Dispute == false)
            //{
            //    return BadRequest(new
            //    {
            //        message = "The invoice ref " + inv.DocPrefix + inv.SLJrnlNo.ToString("D5") +
            //                  " needs to be disputed before creating a creditnote."
            //    });
            //}
            //get last NLJrnNO
            int lastNLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"SLInvoiceHeader\" WHERE \"INVTypeRef\" = 'INV'  LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;
            }
            cnn.Close();
            //get last CRNNO
            int lastCRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrc = new NpgsqlCommand("Select COALESCE(MAX(\"DocRef\"), 0) as ref From \"SLInvoiceHeader\" WHERE \"INVTypeRef\" = 'CRN'  LIMIT 1 ", cnn).ExecuteReader();
            while (sdrc.Read())
            {
                lastCRN = (int)sdrc["ref"];
            }
            cnn.Close();
            NlJournalHeader nlJournalHeader = new NlJournalHeader();
            nlJournalHeader.NlJrnlDesc = "";
            nlJournalHeader.TranDate = DateTime.Now;
            nlJournalHeader.MEndDate = inv.DueDate;
            nlJournalHeader.TranYear = DateTime.Now.Year;
            nlJournalHeader.TranPeriod = DateTime.Now.Month;
            nlJournalHeader.TranType = "";
            nlJournalHeader.TranFrom = "";
            nlJournalHeader.ModuleId = null;
            var response = nlService.updateAccountsOnCreditNoteCreation(inv, nlJournalHeader, lastCRN, sendData.CRNReason, sendData.CRNDate, sendData.CRNvat, sendData.PeriodNumber, sendData.PeriodYear, sendData.CrnPercent);
            if (response.Httpcode == 400)
            {
                _log.LogError(response.Message);
                return BadRequest(new { message = response.Message });
            }
            else
            {
                _log.LogInformation("Created CreditNote  from invoice ref INV" + inv.SLJrnlNo + "  at " + DateTime.Now.ToString("dd/MM/yyyy"));
                AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                AuditTrail auditTrail = new AuditTrail();
                auditTrail.action = "Created CreditNote from invoice  ref INV" + inv.SLJrnlNo + "  at " + DateTime.Now.ToString("dd-MM-yyyy");
                auditTrail.module = "Invoice";
                auditTrail.userId = userId;
                auditTrailService.createAuditTrail(auditTrail);
                return Ok(new { message = "Credit Not has been created successfully" });
            }
            //Mark as dispute add reason update lastcrn invoicetypeRef and update reasons
            //string myQuery = "UPDATE \"SLInvoiceHeader\" SET \"CRNReason\" = '" + sendData.CRNReason +
            //                 "', \"Dispute\" = 't' ,\"INVTypeRef\" = 'CRN'  ,\"DocRef\" = '"+lastCRN+1+"'        WHERE \"SLJrnlNo\" = " + INVSJN + "  ";
            //bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
            //if (reqStatus == true)
            //{
            //    return Ok(new
            //        {message = "Request has been processed successfully by needs to be confirmed to complete"});
            //}
            //else
            //{
            //    return BadRequest(new {message = "An error occurred while processing request(Mark as dispute)"});
            //}
            //// Start issuing credit note
            ////get last SLJrnNO
            //int lastSLJRN = 0;
            //cnn.Open();
            //NpgsqlDataReader sdra =
            //    new NpgsqlCommand(
            //        "Select MAX(\"SLJrnlNo\") as sl From \"SLInvoiceHeader\" WHERE \"INVTypeRef\" = 'INV' LIMIT 1 ",
            //        cnn).ExecuteReader();
            //while (sdra.Read())
            //{
            //    lastSLJRN = sdra["sl"] != DBNull.Value ? (int) sdra["sl"] : 0;
            //}
            //cnn.Close();
            ////get last NLJrnNO
            //int lastNLJRN = 0;
            //cnn.Open();
            //NpgsqlDataReader sdrb =
            //    new NpgsqlCommand(
            //        "Select MAX(\"NlJrnlNo\") as sj From \"SLInvoiceHeader\" WHERE \"INVTypeRef\" = 'INV'  LIMIT 1 ",
            //        cnn).ExecuteReader();
            //while (sdrb.Read())
            //{
            //    lastNLJRN = sdrb["sj"] != DBNull.Value ? (int) sdrb["sj"] : 0;
            //}
            //cnn.Close();
            ////get last CRNNO
            //int lastCRN = 0;
            //cnn.Open();
            //NpgsqlDataReader sdrc =
            //    new NpgsqlCommand(
            //        "Select COALESCE(MAX(\"DocRef\"), 0) as ref From \"SLInvoiceHeader\" WHERE \"INVTypeRef\" = 'CRN'  LIMIT 1 ",
            //        cnn).ExecuteReader();
            //while (sdrc.Read())
            //{
            //    lastCRN = (int) sdrc["ref"];
            //}
            //cnn.Close();
            ////check financial period
            //FinancialPeriod fpData = new FinancialPeriod();
            //cnn.Open();
            //NpgsqlDataReader sdr_fp =
            //    new NpgsqlCommand("SELECT * From financial_periods WHERE fp_active = 't' ", cnn).ExecuteReader();
            //if (sdr_fp.HasRows == false)
            //{
            //    cnn.Close();
            //    return BadRequest(new
            //    {
            //        message =
            //            "No financial period was found active. Please check your financial periods configurations and try again."
            //    });
            //}
            //else
            //{
            //    if (sdr_fp.Read())
            //    {
            //        fpData.fp_id = sdr_fp["fp_id"] != DBNull.Value ? (int) sdr_fp["fp_id"] : 0;
            //        fpData.fp_ref = sdr_fp["fp_ref"] != DBNull.Value ? (string) sdr_fp["fp_ref"] : null;
            //    }
            //}
            //cnn.Close();
            ////get invoice details
            //Invoice inv = new Invoice();
            //cnn.Open();
            //NpgsqlDataReader sdr1 =
            //    new NpgsqlCommand("SELECT * From \"SLInvoiceHeader\" WHERE \"SLJrnlNo\" = " + INVSJN + " ", cnn)
            //        .ExecuteReader();
            //if (sdr1.HasRows == false)
            //{
            //    //empty
            //    cnn.Close();
            //    return BadRequest(new {message = "The Sales journal reference was NOT found."});
            //}
            //if (sdr1.Read())
            //{
            //    inv.SLJrnlNo = sdr1["SLJrnlNo"] != DBNull.Value ? (int) sdr1["SLJrnlNo"] : 0;
            //    inv.NlJrnlNo = sdr1["NlJrnlNo"] != DBNull.Value ? (int) sdr1["NlJrnlNo"] : 0;
            //    inv.CustCode = sdr1["CustCode"] != DBNull.Value ? (string) sdr1["CustCode"] : null;
            //    inv.TransDate = sdr1["TransDate"] != DBNull.Value ? (DateTime) sdr1["TransDate"] : DateTime.Today;
            //    inv.Period = sdr1["Period"] != DBNull.Value ? (string) sdr1["Period"] : null;
            //    inv.DocRef = sdr1["DocRef"] != DBNull.Value ? (int) sdr1["DocRef"] : 0;
            //    inv.TotalAmount = sdr1["TotalAmount"] != DBNull.Value ? (decimal) sdr1["TotalAmount"] : 0;
            //    inv.INVTypeRef = sdr1["INVTypeRef"] != DBNull.Value ? (string) sdr1["INVTypeRef"] : null;
            //    inv.Dispute = sdr1["Dispute"] != DBNull.Value ? (bool) sdr1["Dispute"] : false;
            //    inv.DeliveryCust = sdr1["DeliveryCust"] != DBNull.Value ? (int) sdr1["DeliveryCust"] : 0;
            //    inv.DeliveryAddress = sdr1["DeliveryAddress"] != DBNull.Value
            //        ? (string) sdr1["DeliveryAddress"]
            //        : null;
            //    inv.DeliveryDue = sdr1["DeliveryDue"] != DBNull.Value
            //        ? (DateTime) sdr1["DeliveryDue"]
            //        : DateTime.Today;
            //    inv.INVDate = sdr1["INVDate"] != DBNull.Value ? (DateTime) sdr1["INVDate"] : DateTime.Today;
            //    inv.CustId = sdr1["CustId"] != DBNull.Value ? (int) sdr1["CustId"] : 0;
            //    inv.PaymentDays = sdr1["PaymentDays"] != DBNull.Value ? (int) sdr1["PaymentDays"] : 0;
            //    inv.CustomRef = sdr1["CustomRef"] != DBNull.Value ? (string) sdr1["CustomRef"] : null;
            //    inv.CurrencyId = (int) sdr1["CurrencyId"];
            //    inv.DueDate = (DateTime) sdr1["DueDate"];
            //    inv.SLDescription = (string) sdr1["SLDescription"];
            //    inv.StaffID = sdr1["StaffID"] != DBNull.Value ? (int) sdr1["StaffID"] : 0;
            //    inv.DocPrefix = sdr1["DocPrefix"] != DBNull.Value ? (string) sdr1["DocPrefix"] : null;
            //    inv.CRNReason = sdr1["CRNReason"] != DBNull.Value ? (string) sdr1["CRNReason"] : null;
            //    inv.HasCreditNote = sdr1["HasCreditNote"] != DBNull.Value ? (bool) sdr1["HasCreditNote"] : false;
            //    inv.Branch = sdr1["Branch"] != DBNull.Value ? (int) sdr1["Branch"] : 0;
            //    inv.InvPrinted = sdr1["InvPrinted"] != DBNull.Value ? (bool) sdr1["InvPrinted"] : false;
            //    inv.TotalBalance = sdr1["TotalBalance"] != DBNull.Value ? (decimal) sdr1["TotalBalance"] : 0;

            //}

            //cnn.Close();

            ////check if invoice is disputed
            //if (inv.Dispute == false)
            //{
            //    return BadRequest(new
            //    {
            //        message = "The invoice ref " + inv.DocPrefix + inv.DocRef +
            //                  " needs to be disputed before creating a creditnote."
            //    });
            //}

            ////check if has credit note
            //if (inv.HasCreditNote == true)
            //{
            //    return BadRequest(new
            //    {
            //        message = "The invoice ref " + inv.DocPrefix + inv.DocRef.ToString("D5") +
            //                  " already indicated that it has a creditnote attached."
            //    });
            //}

            ////check if credit note exists in DB for the invoice
            //string checkquery = "Select * From \"SLInvoiceHeader\" WHERE \"SLJrnlNo\" = " + INVSJN +
            //                    " AND  \"HasCreditNote\" = 't' ";
            //int queryCheck = myDbconnection.CheckRowExists(checkquery, db);
            //if (queryCheck > 0)
            //{
            //    // exist
            //    return BadRequest(new
            //    {
            //        message = "A credit note is already issued to the invoice " + inv.DocPrefix + inv.DocRef +
            //                  ". This request has been declined."
            //    });
            //}


            ////create new credit note
            //string insertQuery =
            //    "INSERT INTO \"SLInvoiceHeader\" (\"SLJrnlNo\", \"NlJrnlNo\", \"CustCode\", \"TransDate\", \"Period\", \"DocRef\", \"TotalAmount\", \"INVTypeRef\", \"Dispute\", \"DeliveryCust\", \"DeliveryAddress\", \"DeliveryDue\", \"INVDate\",\"CustId\", \"PaymentDays\", \"CustomRef\", \"CurrencyId\", \"DueDate\", \"SLDescription\", \"StaffID\",\"DocPrefix\",\"CRNReason\", \"HasCreditNote\", \"Branch\", \"InvPrinted\",\"TotalBalance\" ) VALUES(" +
            //    (lastSLJRN + 1) + "," + inv.NlJrnlNo + ",'" + inv.CustCode + "', '" + DateTime.Today + "', '" +
            //    fpData.fp_ref + "','" + (lastCRN + 1) + "', " + -(inv.TotalAmount) + ",'CRN','t', " +
            //    inv.DeliveryCust + ", '" + inv.DeliveryAddress + "','" + inv.DeliveryDue + "', '" + inv.INVDate +
            //    "', " + inv.CustId + ", " + inv.PaymentDays + ", '" + inv.SLJrnlNo + "', " + inv.CurrencyId +
            //    ", '" + inv.DueDate + "', '" + inv.SLDescription + "', " + userId + ",'CRN','" +
            //    sendData.CRNReason + "','t'," + staff_branch + ",'" + inv.InvPrinted + "', " + inv.TotalBalance +
            //    " ); ";
            //bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery, db);
            //if (myReq1 == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occured while trying to save Credit note details." });
            //}

            ////get total VAT billed
            //decimal total_VAT_billed = 0;

            ////get SL invoice details
            //cnn.Open();
            //NpgsqlDataReader sdr_invd =
            //    new NpgsqlCommand("SELECT * From \"SLInvoiceDetail\" WHERE \"SLJrnlNo\" = " + inv.SLJrnlNo + " ",
            //        cnn).ExecuteReader();
            //if (sdr_invd.HasRows == false)
            //{
            //    return BadRequest(new
            //    {
            //        message = "Sorry! No invoice details were found. Please ensure that the invoice " +
            //                  inv.DocPrefix + "-" + inv.DocRef.ToString("D4") + " has invoice details."
            //    });
            //}

            //while (sdr_invd.Read())
            //{
            //    //Get VAT
            //    total_VAT_billed += (decimal)sdr_invd["VatAmt"];

            //    //return items to inventory
            //    NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //    cnn2.Open();
            //    string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " +
            //                    (int)sdr_invd["ItemQty"] + " WHERE \"InvtId\" = " + (int)sdr_invd["ItemId"] + " ";
            //    bool myReq24 = myDbconnection.UpdateDelInsert(up_inv, db);
            //    cnn2.Close();
            //    if (myReq24 == false)
            //    {
            //        //failed
            //        return BadRequest(new
            //        { message = "An occured while trying to process Inventory update requests." });
            //    }


            //}

            //cnn.Close();

            ////deduct VAT amount from VAT Account
            //cnn.Open();
            //string up_vat = "UPDATE \"NLAccount\" SET \"AccBalance\" = \"AccBalance\" - " + total_VAT_billed +
            //                " WHERE \"GroupCode\" = 'VRH20' AND \"AccBranch\" = " + staff_branch + "  ";
            //bool myReq21 = myDbconnection.UpdateDelInsert(up_vat, db);
            //cnn.Close();
            //if (myReq21 == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occured while trying to process NL VAT update requests." });
            //}

            ////deduct account from Creditors account
            //cnn.Open();
            //string up_crd = "UPDATE \"NLAccount\" SET \"AccBalance\" = \"AccBalance\" -" +
            //                (inv.TotalAmount - total_VAT_billed) +
            //                " WHERE \"GroupCode\" = 'CRH20' AND \"AccBranch\" = " + staff_branch + "  ";
            //bool myReq22 = myDbconnection.UpdateDelInsert(up_crd, db);
            //cnn.Close();
            //if (myReq22 == false)
            //{
            //    //failed
            //    return BadRequest(
            //        new { message = "An occured while trying to process NL Creditors update requests." });
            //}

            ////update customers debtors
            //cnn.Open();
            //string up_cust = "UPDATE \"SLCustomer\" SET \"CustomerDept\" = \"CustomerDept\" + " + inv.TotalAmount +
            //                 " WHERE \"SLCustomerSerial\" = " + inv.CustId + " ";
            //bool myReq23 = myDbconnection.UpdateDelInsert(up_cust, db);
            //cnn.Close();
            //if (myReq23 == false)
            //{
            //    //failed
            //    return BadRequest(new { message = "An occured while trying to process Customer update requests." });
            //}

            ////update invoice
            //string updQuery = "UPDATE \"SLInvoiceHeader\" SET  \"HasCreditNote\" = 't' WHERE \"SLJrnlNo\" = " +
            //                  INVSJN + " ";
            //bool sendR = myDbconnection.UpdateDelInsert(updQuery, db);
            //if (sendR == false)
            //{
            //    return BadRequest(new { message = "An occured while trying to update invoice details." });
            //}
            //else
            //{
            //    return Ok(new { message = "Credit note has been successfully created" });
            //}
        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult<Invoice> RevokeCreditNote(int INVSJN)
        {
            if (INVSJN == 0) { return BadRequest(new { message = "Missing required parameters" });}
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
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."});
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
            //check if request is marked as dispute
            Invoice inv_header = new Invoice();
            cnn.Open();
            NpgsqlDataReader sdr_dis = new NpgsqlCommand("Select * From \"SLInvoiceHeader\" WHERE \"SLJrnlNo\" = " + INVSJN + " ", cnn).ExecuteReader();
            while (sdr_dis.Read())
            {
                inv_header.Dispute = sdr_dis["Dispute"] != DBNull.Value ? (bool)sdr_dis["Dispute"] : true;
            }
            cnn.Close();
            //validate if invoice is disputed
            if (inv_header.Dispute == true)
            {
                //Mark as dispute and update reasons
                string myQuery = "UPDATE \"SLInvoiceHeader\" SET  \"Dispute\" = 'f' WHERE \"SLJrnlNo\" = " + INVSJN + "  ";
                bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
                if (reqStatus == true)
                {
                    return Ok(new { message = "Request has been processed successfully completed" });
                }
                else
                {
                    return BadRequest(new { message = "An error occurred while processing request" });
                }
            }
            else
            {
                return BadRequest(new { message = "This request is only available for disputed invoices" });
            }
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult ProformaInvDefaults()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _log.LogInformation("Fetching proforma invoices");
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //Customer ID reference
            string customerId = Request.Headers["Customer"];
            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed."});
            }
            else if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(new { message = "Cannot find required customer reference. Request failed" });
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
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."});
            }
            //get database name
            string db = companyRes;
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"});
            }
            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //check customer id exists
            string cust_check = "SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + Int64.Parse(customerId) +
                                " ";
            int cust_check_res = myDbconnection.CheckRowExists(cust_check, db);
            if (cust_check_res == 0)
            {
                _log.LogError("No customer found with the parsed reference was found. Request cancelled");
                return BadRequest(new { message = "No customer found with the parsed reference was found. Request cancelled" });
            }
            //get selected customer
            AddCustomer selectedCustomer = new AddCustomer();
            cnn.Open();
            NpgsqlDataReader sdr_sc = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + Int64.Parse(customerId) + " ", cnn).ExecuteReader();
            while (sdr_sc.Read())
            {
                selectedCustomer.SLCustomerSerial = (int)sdr_sc["SLCustomerSerial"];
                selectedCustomer.CustCode = sdr_sc["CustCode"].ToString();
                selectedCustomer.CustFirstName = sdr_sc["CustFirstName"] != DBNull.Value ? (string)sdr_sc["CustFirstName"] : null;
                selectedCustomer.Address = sdr_sc["Address"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.CurCode = (int)sdr_sc["CurCode"];
                selectedCustomer.CustEmail = sdr_sc["CustEmail"].ToString();
                selectedCustomer.CustContact = sdr_sc["CustContact"].ToString();
                selectedCustomer.SLCTypeID = (int)sdr_sc["SLCTypeID"];
                selectedCustomer.CustLastName = sdr_sc["CustLastName"] != DBNull.Value ? (string)sdr_sc["CustLastName"] : null;
                selectedCustomer.CustType = sdr_sc["CustType"].ToString();
                selectedCustomer.CustCompany = sdr_sc["CustCompany"] != DBNull.Value ? (string)sdr_sc["CustCompany"] : null;
                selectedCustomer.VATNo = sdr_sc["VATNo"] != DBNull.Value ? (string)sdr_sc["VATNo"] : null;
                selectedCustomer.CustCreditLimit = (float)sdr_sc["CustCreditLimit"];
                selectedCustomer.VATpin = sdr_sc["VATpin"].ToString();
                selectedCustomer.CreditTerms = (int)sdr_sc["CreditTerms"];
                selectedCustomer.CurrCode = sdr_sc["CrCode"].ToString();
                selectedCustomer.CustBranch = sdr_sc["CustBranch"] != DBNull.Value ? (int)sdr_sc["CustBranch"] : 0;
                selectedCustomer.CustRef = sdr_sc["CustRef"] != DBNull.Value ? (string)sdr_sc["CustRef"] : null;
                selectedCustomer.CustomerDept = sdr_sc["CustomerDept"] != DBNull.Value ? (decimal)sdr_sc["CustomerDept"] : 0;
            }
            cnn.Close();
            //Get all currencies
            List<Currencies> currencyList = new List<Currencies>();
            cnn.Open();
            NpgsqlDataReader sdr_cr = new NpgsqlCommand("SELECT * FROM \"Currencies\" ", cnn).ExecuteReader();
            while (sdr_cr.Read())
            {
                Currencies cr = new Currencies();
                cr.CrId = sdr_cr["CrId"] != DBNull.Value ? (int)sdr_cr["CrId"] : 0;
                cr.CrName = sdr_cr["CrName"] != DBNull.Value ? (string)sdr_cr["CrName"] : null;
                currencyList.Add(cr);
            }
            cnn.Close();
            //Get all VAT
            List<TaxSetup> vatList = new List<TaxSetup>();
            cnn.Open();
            NpgsqlDataReader sdr_vt = new NpgsqlCommand("SELECT * FROM \"VATs\" WHERE \"VtActive\" = 't' ", cnn).ExecuteReader();
            while (sdr_vt.Read())
            {
                TaxSetup tx = new TaxSetup();
                tx.VtId = sdr_vt["VtId"] != DBNull.Value ? (int)sdr_vt["VtId"] : 0;
                tx.VtRef = sdr_vt["VtRef"] != DBNull.Value ? (string)sdr_vt["VtRef"] : null;
                tx.VtPerc = sdr_vt["VtPerc"] != DBNull.Value ? (float)sdr_vt["VtPerc"] : 0;
                vatList.Add(tx);
            }
            cnn.Close();
            //get all Invoices
            List<Inventory> inventList = new List<Inventory>();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select \"InvtId\", \"InvtType\", \"InvtName\",\"InvtQty\", \"InvtBP\", \"InvtSP\", \"InvtReorderLevel\", \"InvtDateAdded\", \"InvtDateModified\", \"InvtAddedBy\"," +
                " \"InvtModifiedBy\" , \"InvtCurrency\", \"InvtVATId\", \"VtId\", \"VtRef\", \"VtPerc\" From \"Inventory\" LEFT JOIN \"VATs\" ON ( \"InvtVATId\" =  \"VtId\" ) ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Inventory inv = new Inventory();
                inv.InvtId = (int)sdr["InvtId"];
                inv.InvtType = (string)sdr["InvtType"];
                inv.InvtName = (string)sdr["InvtName"];
                //inv.InvtQty = (int)sdr["InvtQty"];
                //inv.InvtBP = (float)sdr["InvtBP"];
                inv.InvtSP = (decimal)sdr["InvtSP"];
                inv.InvtVATId = (int)sdr["InvtVATId"];
                inv.VATPerc = (float)sdr["VtPerc"];
                inventList.Add(inv);
            }
            cnn.Close();
            //Get Last
            int lastproformaInvNumber = 0;
            //get last registered number
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand( "SELECT COALESCE(MAX(\"DocRef\"),0) as st FROM \"SLInvoiceHeader\" WHERE  \"INVTypeRef\" = 4 LIMIT 1 ", cnn).ExecuteReader();
            while (sdr2.Read())
            {
                lastproformaInvNumber = (int)sdr2["st"];
            }
            cnn.Close();
            //Set Proforma Invoice Settings
            InvoiceSettings invsettings = new InvoiceSettings();
            invsettings.InvPrefix = "PRF";
            invsettings.InvStartNumber = 0;
            invsettings.LastNumber = lastproformaInvNumber;
            invsettings.InvNumberingType = "Auto";
            //Get all discounts not expired
            List<Discounts> discList = new List<Discounts>();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Discounts\" WHERE \"DEndDate\" >= '" + DateTime.Now + "'  ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                Discounts disc = new Discounts();
                disc.DId = (int)sdr3["DId"];
                disc.DRef = (string)sdr3["DRef"];
                disc.DPerc = (float)sdr3["DPerc"];
                discList.Add(disc);
            }
            cnn.Close();
            //Get All customers
            List<AddCustomer> customerList = new List<AddCustomer>();
            cnn.Open();
            NpgsqlDataReader sdr_cl = new NpgsqlCommand("SELECT \"SLCustomer\".*,\"CrId\",\"CrCode\" FROM \"SLCustomer\" LEFT JOIN \"Currencies\" ON ( \"CurCode\" = \"CrId\") WHERE \"CustBranch\" = " + staff_branch + "  ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                AddCustomer addCust = new AddCustomer();
                addCust.SLCustomerSerial = (int)sdr["SLCustomerSerial"];
                addCust.CustCode = sdr["CustCode"].ToString();
                addCust.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                addCust.CustLastName = sdr["CustLastName"] != DBNull.Value ? (string)sdr["CustLastName"] : null;
                addCust.CustType = sdr["CustType"].ToString();
                addCust.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                addCust.CurrCode = sdr["CrCode"].ToString();
                customerList.Add(addCust);
            }
            cnn.Close();
            return Ok(new
            {
                CustomerData = customerList,
                VATData = vatList,
                CurrencyData = currencyList,
                InventoryData = inventList,
                InvSettings = invsettings,
                CustCurrId = selectedCustomer.CurCode,
                DiscData = discList,
                PaymentDays = selectedCustomer.CreditTerms,
                SelectedCustomer = selectedCustomer
            });
        }
        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateCustomerProformaInvoice(Invoice invoiceData)
        {
            //check data
            if (string.IsNullOrEmpty(invoiceData.Period))
            {
                _log.LogError("Missing Invoice period");
                return BadRequest(new { message = "Missing Invoice period" });
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
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed."});
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
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."});
            }
            //get database name
            string db = companyRes;
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"});
            }
            //check if customer exists
            string cust_check = "SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ";
            int cust_check_res = myDbconnection.CheckRowExists(cust_check, db);
            if (cust_check_res == 0)
            {
                _log.LogError("No customer found with the parsed data was found. Request cancelled");
                return BadRequest(new { message = "No customer found with the parsed data was found. Request cancelled" });
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get selected customer
            AddCustomer selectedCustomer = new AddCustomer();
            cnn.Open();
            NpgsqlDataReader sdr_sc = new NpgsqlCommand("SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ", cnn).ExecuteReader();
            while (sdr_sc.Read())
            {
                selectedCustomer.SLCustomerSerial = (int)sdr_sc["SLCustomerSerial"];
                selectedCustomer.CustCode = sdr_sc["CustCode"].ToString();
                selectedCustomer.CustFirstName = sdr_sc["CustFirstName"] != DBNull.Value ? (string)sdr_sc["CustFirstName"] : null;
                selectedCustomer.Address = sdr_sc["Address"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                selectedCustomer.CurCode = (int)sdr_sc["CurCode"];
                selectedCustomer.CustEmail = sdr_sc["CustEmail"].ToString();
                selectedCustomer.CustContact = sdr_sc["CustContact"].ToString();
                selectedCustomer.SLCTypeID = (int)sdr_sc["SLCTypeID"];
                selectedCustomer.CustLastName = sdr_sc["CustLastName"] != DBNull.Value ? (string)sdr_sc["CustLastName"] : null;
                selectedCustomer.CustType = sdr_sc["CustType"].ToString();
                selectedCustomer.CustCompany = sdr_sc["CustCompany"] != DBNull.Value ? (string)sdr_sc["CustCompany"] : null;
                selectedCustomer.VATNo = sdr_sc["VATNo"] != DBNull.Value ? (string)sdr_sc["VATNo"] : null;
                selectedCustomer.CustCreditLimit = (float)sdr_sc["CustCreditLimit"];
                selectedCustomer.VATpin = sdr_sc["VATpin"].ToString();
                selectedCustomer.CreditTerms = (int)sdr_sc["CreditTerms"];
                selectedCustomer.CurrCode = sdr_sc["CurCode"].ToString();
                selectedCustomer.CustBranch = sdr_sc["CustBranch"] != DBNull.Value ? (int)sdr_sc["CustBranch"] : 0;
                selectedCustomer.CustRef = sdr_sc["CustRef"] != DBNull.Value ? (string)sdr_sc["CustRef"] : null;
                selectedCustomer.CustomerDept = sdr_sc["CustomerDept"] != DBNull.Value ? (decimal)sdr_sc["CustomerDept"] : 0;
            }
            cnn.Close();
            //check if customer debt id greater
            decimal customerDept = selectedCustomer.CustomerDept;
            //check customer credit limit
            decimal customer_credit_limit = (decimal)selectedCustomer.CustCreditLimit;
            decimal total_invoice_amount = 0;
            foreach (var t in invoiceData.InvoiceDetailsList)
            {
                total_invoice_amount += (t.Total - t.DiscountAmt);
                NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn2.Open();
                Inventory inv = new Inventory();
                NpgsqlDataReader sdr_c = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + t.ItemId + " ", cnn2).ExecuteReader();
                if (sdr_c.Read())
                {
                    inv.InvtName = sdr_c["InvtName"] != DBNull.Value ? (string)sdr_c["InvtName"] : null;
                    inv.InvtQty = sdr_c["InvtQty"] != DBNull.Value ? (int)sdr_c["InvtQty"] : 0;
                    inv.InvtType = sdr_c["InvtType"] != DBNull.Value ? (string)sdr_c["InvtType"] : null;
                    if (inv.InvtType == "GOODS")
                    {
                        if (t.Quantity > inv.InvtQty)
                        {
                            return BadRequest(new { message = "Cannot remove " + t.Quantity + " from your inventory. You only have " + inv.InvtQty + " items left."});
                        }
                    }
                }
                cnn2.Close();
            }

            //decimal ttl_customer_dept = total_invoice_amount + customerDept;
            //if (ttl_customer_dept > customer_credit_limit)
            //{
            //    _log.LogError("The Invoice Amount " + string.Format("{0:#,0.00}", total_invoice_amount) +
            //                  " exceeds the customer credit limit of");

            //    return BadRequest(new
            //    {
            //        message = "The Invoice Amount " + string.Format("{0:#,0.00}", total_invoice_amount) +
            //                  " exceeds the customer credit limit of " +
            //                  string.Format("{0:#,0.00}", customer_credit_limit) +
            //                  ". The Total debt of the customer inclusive the current invoice is sum upto " +
            //                  string.Format("{0:#,0.00}", ttl_customer_dept) +
            //                  " You can only add an invoice amount that does NOT exceed " +
            //                  string.Format("{0:#,0.00}", total_invoice_amount - customerDept)
            //    });
            //}

            //check if invoice settings exists
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("SELECT * From \"SLInvoiceSettings\" WHERE \"InvBranch\" = " + staff_branch + " LIMIT 1", cnn).ExecuteReader();
            if (sdr2.HasRows == false)
            {
                return BadRequest(new { message = "No data was found with the invoice settings. Please set the invoice  configurations to continue"});
            }
            cnn.Close();
            //get last SLJrnNO
            int lastSLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"SLJrnlNo\") as sl From \"SLProformaInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastSLJRN = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            //get last NLJrnNO
            int lastNLJRN = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"SLProformaInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;
            }
            cnn.Close();
            //get last DOC REF
            int lastDOCREF = 0;
            cnn.Open();
            NpgsqlDataReader sdrR = new NpgsqlCommand("Select MAX(\"DocRef\") as sj From \"SLProformaInvoiceHeader\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrR.Read())
            {
                lastDOCREF = sdrR["sj"] != DBNull.Value ? (int)sdrR["sj"] : 0;
            }
            cnn.Close();
            //set Data
            invoiceData.SLJrnlNo = lastSLJRN + 1;
            invoiceData.NlJrnlNo = lastNLJRN + 1;
            invoiceData.CustCode = selectedCustomer.CustCode;
            invoiceData.TransDate = DateTime.Now;
            invoiceData.INVTypeRef = "PRF";
            invoiceData.Dispute = false;
            //manage invoice details
            invoiceData.TotalAmount = 0;
            decimal totals_billed = 0;
            //insert invoice details
            try
            {
                if (invoiceData.InvoiceDetailsList.Count > 0)
                {
                    foreach (var t in invoiceData.InvoiceDetailsList)
                    {
                        totals_billed = (t.Price * t.Quantity) + t.VatAmt - t.DiscountAmt;
                        NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                        cnn2.Open();
                        Inventory inv = new Inventory();
                        NpgsqlDataReader sdr_c = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtId\" = " + t.ItemId + " ", cnn2).ExecuteReader();
                        if (sdr_c.Read())
                        {
                            inv.InvtName = sdr_c["InvtName"] != DBNull.Value ? (string)sdr_c["InvtName"] : null;
                            inv.InvtQty = sdr_c["InvtQty"] != DBNull.Value ? (int)sdr_c["InvtQty"] : 0;
                            inv.InvtType = sdr_c["InvtType"] != DBNull.Value ? (string)sdr_c["InvtType"] : null;
                        }
                        cnn2.Close();
                        NpgsqlConnection cnn3 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                        cnn3.Open();
                        Discounts ds = new Discounts();
                        NpgsqlDataReader sdr_d =  new NpgsqlCommand("SELECT * FROM \"Discounts\" WHERE \"DId\" = " + t.Discount + " ", cnn3).ExecuteReader();
                        while (sdr_d.Read())
                        {
                            ds.DRef = sdr_d["DRef"] != DBNull.Value ? (string)sdr_d["DRef"] : null;
                            ds.DPerc = sdr_d["DPerc"] != DBNull.Value ? (float)sdr_d["DPerc"] : 0;
                        }
                        cnn3.Close();
                        NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                        cnn1.Open();
                        Guid obj = Guid.NewGuid();
                        string insertQuery1 =
                            "INSERT INTO \"SLProformaInvoiceDetails\" (\"SLJrnlNo\", \"JrnlSLNo\", \"InvAmt\", \"VatCode\", \"VatAmt\", \"ProdGroupCode\", \"NLAccCode\", \"StkDesc\", \"UserID\", \"ItemSerial\", \"ItemQty\", \"ItemTotals\", \"ItemUnitPrice\", \"DiscountPerc\",\"DiscountAmt\", \"AdditionalDetails\" ) VALUES(" +
                            (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", " + totals_billed + ", '" + t.VatCode + "', " + t.VatAmt + ", '" + "N/A" + "','45','" + inv.InvtName + "'," + userId + ",'" + obj.ToString() +"', " + t.Quantity + ", " + (t.Total - t.DiscountAmt - t.VatAmt) + ", " + t.Price + ", " +
                            ds.DPerc + ", " + t.DiscountAmt + ", '" + t.StkDesc + "' ); ";

                        bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                        cnn1.Close();
                        if (myReq1 == false)
                        {
                            //failed
                            return BadRequest(new { message = "An occurred while trying to save invoice details." });
                        }
                    }
                }
                //insert into header
                string myquer =
                    "INSERT INTO \"SLProformaInvoiceHeader\" (\"SLJrnlNo\", \"NlJrnlNo\", \"CustCode\", \"TransDate\", \"Period\", \"DocRef\", \"TotalAmount\", \"INVTypeRef\", \"Dispute\", \"DeliveryCust\", \"DeliveryAddress\", \"DeliveryDue\", \"INVDate\",\"CustId\", \"PaymentDays\",\"CustomRef\",\"CurrencyId\",\"DueDate\"," +
                    "\"SLDescription\",\"StaffID\",\"DocPrefix\",\"HasInvoice\", \"BranchRef\" ,\"BankName\",\"BankBranch\",\"AccName\",\"AccNumber\",\"SwiftCode\") VALUES(" +
                    (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + invoiceData.CustCode + "', '" + DateTime.Now +"',  '" + invoiceData.Period + "', " + (lastDOCREF + 1) + "," + totals_billed + ",'4','f',  " +invoiceData.DeliveryCust + ",  '" + invoiceData.DeliveryAddress + "',  '" + invoiceData.DeliveryDue +
                    "',  '" + invoiceData.INVDate + "', " + invoiceData.CustId + ",  " + invoiceData.PaymentDays + ",  '" +invoiceData.CustomRef + "',  " + invoiceData.CurrencyId + ", '" + invoiceData.DueDate + "', '" +invoiceData.SLDescription + "', " + userId + ", 'PRF','f'," + staff_branch + " ,'" + invoiceData.BankName + "','" + invoiceData.BranchName + "'," +
                    "'" + invoiceData.AccName + "','" + invoiceData.AccNumber + "','" + invoiceData.SwiftCode + "'); ";

                bool sendq = myDbconnection.UpdateDelInsert(myquer, db);
                if (sendq == false)
                {
                    //failed
                    return BadRequest(new { message = "An occurred while trying to save proforma invoice details." });
                }
                else
                {
                    AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                    AuditTrail auditTrail = new AuditTrail();
                    auditTrail.action = "Created Proforma Invoice of customer " + selectedCustomer.CustFirstName + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", total_invoice_amount);
                    auditTrail.module = "Invoice";
                    auditTrail.userId = userId;
                    auditTrailService.createAuditTrail(auditTrail);
                    return Ok(new { message = "Proforma Invoice has been successfully created"});
                }
            }
            catch (Exception ex)
            {
                throw ex;
                return BadRequest(new { message = "An occurred while trying to save proforma invoice details." });
            }
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult<Invoice> GetAllProformaInvoices()
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
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed."});
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
                    message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"
                });
            }
            //get all Invoices
            List<Invoice> invList = new List<Invoice>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand(
                "SELECT \"SLProformaInvoiceHeader\".*, \"SLCustomerSerial\", \"CustFirstName\", \"CustLastName\",  \"CustCompany\", \"CustType\", \"CrId\", \"CrCode\", fp_name FROM  \"SLProformaInvoiceHeader\"" +
                " LEFT JOIN  \"SLCustomer\" ON ( \"CustId\" =  \"SLCustomerSerial\" ) LEFT JOIN  \"Currencies\" ON ( \"CurrencyId\" =  \"CrId\" ) LEFT JOIN  financial_periods ON ( fp_ref =  \"Period\" ) WHERE \"DocPrefix\" = 'PRF' AND \"BranchRef\" = " +
                staff_branch + "  ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Invoice inv = new Invoice();
                inv.SLJrnlNo = (int)sdr["SLJrnlNo"];
                inv.NlJrnlNo = (int)sdr["NlJrnlNo"];
                inv.CustCode = (string)sdr["CustCode"];
                inv.TransDate = (DateTime)sdr["TransDate"];
                inv.Period = (string)sdr["Period"];
                inv.DocPrefix = sdr["DocPrefix"].ToString();
                inv.DocRef = (int)sdr["DocRef"];
                inv.TotalAmount = (decimal)sdr["TotalAmount"];
                inv.INVTypeRef = (string)sdr["INVTypeRef"];
                inv.Dispute = (bool)sdr["Dispute"];
                inv.DeliveryCust = (int)sdr["DeliveryCust"];
                // inv.DeliveryAddress = (string)sdr["DeliveryAddress"];
                inv.DeliveryDue = (DateTime)sdr["DeliveryDue"];
                inv.INVDate = (DateTime)sdr["INVDate"];
                inv.CustId = (int)sdr["CustId"];
                inv.PaymentDays = (int)sdr["PaymentDays"];
                inv.CustomRef = sdr["CustomRef"].ToString();
                inv.CurrencyId = (int)sdr["CurrencyId"];
                inv.DueDate = (DateTime)sdr["DueDate"];
                inv.HasInvoice = (bool)sdr["HasInvoice"];
                inv.HasInvoice = (bool)sdr["HasInvoice"];
                inv.BranchRef = (int)sdr["BranchRef"];
                inv.CustFirstName = sdr["CustFirstName"].ToString();
                inv.CustLastname = sdr["CustLastname"].ToString();
                inv.CustCompany = sdr["CustCompany"].ToString();
                inv.CustType = sdr["CustType"].ToString();
                inv.CrCode = sdr["CrCode"].ToString();
                inv.fp_name = sdr["fp_name"].ToString();
                invList.Add(inv);
            }
            cnn.Close();
            return Ok(new { proformainvoiceData = invList });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult<Invoice> GetProformaInvoicePageReport(int DOC)
        {
            if (DOC == 0)
            {
                return BadRequest(new { message = "Sorry! Missing required details to process request." });
            }

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
                    return BadRequest(new
                    {
                        message =
                            "Cannot find required permission parameters. Request terminated.Security verification failed."
                    });
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
                    return BadRequest(new
                    { message = "Cannot find your client connection route. Page verification failed" });
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
                    return BadRequest(new
                    {
                        message =
                            "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
                    });
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

                //get all Invoices
                Invoice inv = new Invoice();
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn.Open();
                try
                {


                    NpgsqlDataReader sdr = new NpgsqlCommand(
                        "Select \"SLProformaInvoiceHeader\".* \"SLCustomerSerial\", \"CustFirstName\", \"CustLastName\",\"Address\",\"PostalAddress\",\"CustEmail\",\"CustContact\",\"CustCompany\", \"CustType\", \"CrId\", \"CrCode\" From \"SLProformaInvoiceHeader\" LEFT JOIN  \"SLCustomer\" ON ( \"CustId\" =  \"SLCustomerSerial\" ) LEFT JOIN  \"Currencies\" ON ( \"CurrencyId\" =  \"CrId\" ) WHERE \"DocRef\" = " +
                        DOC + " AND \"BranchRef\" = " + staff_branch + "  ", cnn).ExecuteReader();
                    if (sdr.Read())
                    {
                        inv.SLJrnlNo = (int)sdr["SLJrnlNo"];
                        inv.NlJrnlNo = (int)sdr["NlJrnlNo"];
                        inv.CustCode = (string)sdr["CustCode"];
                        inv.TransDate = (DateTime)sdr["TransDate"];
                        inv.Period = (string)sdr["Period"];
                        inv.DocPrefix = sdr["DocPrefix"].ToString();
                        inv.DocRef = (int)sdr["DocRef"];
                        inv.TotalAmount = (decimal)sdr["TotalAmount"];
                        inv.INVTypeRef = (string)sdr["INVTypeRef"];
                        inv.Dispute = (bool)sdr["Dispute"];
                        inv.DeliveryCust = (int)sdr["DeliveryCust"];
                        inv.DeliveryAddress = (string)sdr["DeliveryAddress"];
                        inv.DeliveryDue = (DateTime)sdr["DeliveryDue"];
                        inv.INVDate = (DateTime)sdr["INVDate"];
                        inv.CustId = (int)sdr["CustId"];
                        inv.PaymentDays = (int)sdr["PaymentDays"];
                        inv.CustomRef = sdr["CustomRef"].ToString();
                        inv.CurrencyId = (int)sdr["CurrencyId"];
                        inv.DueDate = (DateTime)sdr["DueDate"];
                        inv.CustFirstName = sdr["CustFirstName"].ToString();
                        inv.CustLastname = sdr["CustLastname"].ToString();
                        inv.CustCompany = sdr["CustCompany"].ToString();
                        inv.CustType = sdr["CustType"].ToString();
                        inv.CrCode = sdr["CrCode"].ToString();
                        inv.Address = sdr["Address"].ToString();
                        inv.PostalAddress = sdr["PostalAddress"].ToString();
                        inv.CustEmail = sdr["CustEmail"].ToString();
                        inv.CustContact = sdr["CustContact"].ToString();
                        inv.BankName = sdr["BankName"].ToString();
                        inv.AccNumber = sdr["AccNumber"].ToString();
                        inv.BranchName = sdr["BankBranch"].ToString();
                        inv.AccName = sdr["AccName"] != DBNull.Value ? (string)sdr["AccName"] : "";
                        inv.SwiftCode = sdr["SwiftCode"] != DBNull.Value ? (string)sdr["SwiftCode"] : "";

                        inv.DeliveryAddress = sdr["DeliveryAddress"].ToString();

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
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
                    lic.CompanyLogo = sdr1["CompanyLogo"] != DBNull.Value ? (string)sdr1["CompanyLogo"] : null;
                }

                cnn.Close();
                string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                string base64ImageRepresentation = "";
                if (!String.IsNullOrEmpty(lic.CompanyLogo))
                {
                    string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                    byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                    base64ImageRepresentation = Convert.ToBase64String(imageArray);

                }
                else
                {
                    string full_imgPath = Path.Combine(img_path, "invoice_default.jpg");
                    byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                    base64ImageRepresentation = Convert.ToBase64String(imageArray);
                }

                lic.CompanyLogo = base64ImageRepresentation;
                //get Delivery customer details

                DeliveryCustomer dc = new DeliveryCustomer();
                cnn.Open();
                NpgsqlDataReader sdr2 =
                    new NpgsqlCommand(
                            "Select * From \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + inv.DeliveryCust + " ", cnn)
                        .ExecuteReader();
                if (sdr2.Read())
                {
                    dc.CustFirstName = sdr2["CustFirstName"].ToString();
                    dc.CustLastName = sdr2["CustLastName"].ToString();
                    dc.CustEmail = sdr2["CustEmail"].ToString();
                    dc.CustCompany = sdr2["CustCompany"].ToString();
                    dc.CustType = sdr2["CustType"].ToString();
                }

                cnn.Close();


                //get invoice list details
                List<InvoiceDetails> invDetailsList = new List<InvoiceDetails>();
                cnn.Open();
                NpgsqlDataReader sdr3 =
                    new NpgsqlCommand(
                            "Select * From \"SLProformaInvoiceDetails\" WHERE \"JrnlSLNo\" = " + inv.NlJrnlNo + " ",
                            cnn)
                        .ExecuteReader();
                while (sdr3.Read())
                {
                    InvoiceDetails invDetail = new InvoiceDetails();

                    invDetail.SLJrnlNo = (int)sdr3["SLJrnlNo"];
                    invDetail.JrnlSLNo = (int)sdr3["JrnlSLNo"];
                    invDetail.InvAmt = (float)sdr3["InvAmt"];
                    invDetail.VatCode = (string)sdr3["VatCode"];
                    invDetail.VatAmt = (float)sdr3["VatAmt"];
                    invDetail.ProdGroupCode = (string)sdr3["ProdGroupCode"];
                    invDetail.NLAccCode = sdr3["NLAccCode"].ToString();
                    invDetail.StkDesc = (string)sdr3["StkDesc"];
                    invDetail.ItemQty = (int)sdr3["ItemQty"];
                    invDetail.ItemTotals = (float)sdr3["ItemTotals"];
                    invDetail.ItemUnitPrice = (float)sdr3["ItemUnitPrice"];
                    invDetail.Currency = inv.CrCode;
                    invDetail.DiscountPerc = sdr3["DiscountPerc"] != DBNull.Value ? (float)sdr3["DiscountPerc"] : 0;
                    invDetail.DiscountAmt = sdr3["DiscountAmt"] != DBNull.Value ? (float)sdr3["DiscountAmt"] : 0;
                    invDetail.AdditionalDetails = sdr3["AdditionalDetails"] != DBNull.Value ? (string)sdr3["AdditionalDetails"] : null;
                    invDetailsList.Add(invDetail);
                }

                cnn.Close();

                //Get invoice terms and conditions
                Allterms invterms = new Allterms();
                cnn.Open();

                NpgsqlDataReader sdr4 =
                    new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'inv_terms' ", cnn)
                        .ExecuteReader();
                while (sdr4.Read())
                {
                    invterms.tosID = sdr4["tosID"] != DBNull.Value ? (int)sdr4["tosID"] : 0;
                    invterms.tosType = sdr4["tosType"] != DBNull.Value ? (string)sdr4["tosType"] : null;
                    invterms.terms = sdr4["terms"] != DBNull.Value ? sdr4["terms"].ToString() : null;

                }

                cnn.Close();

                // Decode the content for showing on Web page.
                invterms.terms = WebUtility.HtmlDecode(invterms.terms);

                return Ok(new
                {
                    InvoiceHeader = inv,
                    CompanyData = lic,
                    DeliveryCust = dc,
                    InvoiceDetails = invDetailsList,
                    InvTerms = invterms
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
                //Console.WriteLine(e);
                //throw;
            }


        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult createInvoicefromCreditNote(int CRN)
        {
            try
            {
                if (CRN == 0)
                {
                    return BadRequest(new { message = "Missing required parameters to start operation" });
                }

                //set Date
                DateTime today = DateTime.Today;

                var now = DateTime.Now;
                var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second)
                    .AddMilliseconds(now.Millisecond);
                int uniqueId = (int)(zeroDate.Ticks / 10000);

                int uniqueId1 = (int)(zeroDate.Ticks / 10000);

                //check if company code exists
                var companyRes = "";
                int userId = 0;

                //check if cookie exists in Request
                string authHeader = Request.Headers[HeaderNames.Authorization];
                //permission name
                string permissionName = Request.Headers["PermName"];

                if (string.IsNullOrEmpty(permissionName))
                {
                    return BadRequest(new
                    {
                        message =
                            "Cannot find required permission parameters. Request terminated.Security verification failed."
                    });
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
                    return BadRequest(new
                    {
                        message =
                            "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
                    });
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

                //get proforma invoice Data
                Invoice invoiceData = new Invoice();

                cnn.Open();
                NpgsqlDataReader sdrInv =
                    new NpgsqlCommand(
                        "SELECT * FROM \"SLProformaInvoiceHeader\" WHERE  \"DocRef\" = " + CRN + " AND  \"BranchRef\" = " +
                        staff_branch + " ", cnn).ExecuteReader();
                while (sdrInv.Read())
                {
                    invoiceData.SLJrnlNo = sdrInv["SLJrnlNo"] != DBNull.Value ? (int)sdrInv["SLJrnlNo"] : 0;
                    invoiceData.NlJrnlNo = sdrInv["NlJrnlNo"] != DBNull.Value ? (int)sdrInv["NlJrnlNo"] : 0;
                    invoiceData.CustCode = sdrInv["CustCode"] != DBNull.Value ? (string)sdrInv["CustCode"] : null;
                    invoiceData.TransDate = DateTime.Today;
                    ;
                    invoiceData.Period = sdrInv["Period"] != DBNull.Value ? (string)sdrInv["Period"] : null;
                    invoiceData.DocRef = sdrInv["DocRef"] != DBNull.Value ? (int)sdrInv["DocRef"] : 0;
                    invoiceData.TotalAmount = sdrInv["TotalAmount"] != DBNull.Value ? (decimal)sdrInv["TotalAmount"] : 0;
                    invoiceData.INVTypeRef = sdrInv["INVTypeRef"] != DBNull.Value ? (string)sdrInv["INVTypeRef"] : null;
                    invoiceData.Dispute = sdrInv["Dispute"] != DBNull.Value ? (bool)sdrInv["Dispute"] : false;
                    invoiceData.DeliveryCust = sdrInv["DeliveryCust"] != DBNull.Value ? (int)sdrInv["DeliveryCust"] : 0;
                    invoiceData.DeliveryAddress = sdrInv["DeliveryAddress"] != DBNull.Value
                        ? (string)sdrInv["DeliveryAddress"]
                        : null;
                    invoiceData.DeliveryDue =
                        sdrInv["DeliveryDue"] != DBNull.Value ? (DateTime)sdrInv["DeliveryDue"] : today;
                    invoiceData.INVDate = sdrInv["INVDate"] != DBNull.Value ? (DateTime)sdrInv["INVDate"] : today;
                    invoiceData.CustId = sdrInv["CustId"] != DBNull.Value ? (int)sdrInv["CustId"] : 0;
                    invoiceData.PaymentDays = sdrInv["PaymentDays"] != DBNull.Value ? (int)sdrInv["PaymentDays"] : 0;
                    invoiceData.CustomRef = sdrInv["CustomRef"] != DBNull.Value ? (string)sdrInv["CustomRef"] : null;
                    invoiceData.CurrencyId = sdrInv["CurrencyId"] != DBNull.Value ? (int)sdrInv["CurrencyId"] : 0;
                    invoiceData.DueDate = sdrInv["DueDate"] != DBNull.Value ? (DateTime)sdrInv["DueDate"] : today;
                    invoiceData.SLDescription =
                        sdrInv["SLDescription"] != DBNull.Value ? (string)sdrInv["SLDescription"] : null;
                    invoiceData.StaffID = userId;
                    invoiceData.DocPrefix = sdrInv["DocPrefix"] != DBNull.Value ? (string)sdrInv["DocPrefix"] : null;
                    invoiceData.HasInvoice = sdrInv["HasInvoice"] != DBNull.Value ? (bool)sdrInv["HasInvoice"] : false;
                    invoiceData.HasCreditNote = false;

                }

                cnn.Close();


                //set invoice details
                List<InvoiceDetails> invDetailsList = new List<InvoiceDetails>();
                cnn.Open();
                NpgsqlDataReader sdr3 =
                    new NpgsqlCommand(
                        "Select * From \"SLProformaInvoiceDetails\" WHERE \"JrnlSLNo\" = " + invoiceData.NlJrnlNo + " ",
                        cnn).ExecuteReader();
                while (sdr3.Read())
                {
                    InvoiceDetails invDetail = new InvoiceDetails();

                    invDetail.SLJrnlNo = (int)sdr3["SLJrnlNo"];
                    invDetail.JrnlSLNo = (int)sdr3["JrnlSLNo"];
                    invDetail.InvAmt = (float)sdr3["InvAmt"];
                    invDetail.VatCode = (string)sdr3["VatCode"];
                    invDetail.VatAmt = (float)sdr3["VatAmt"];
                    invDetail.ProdGroupCode = (string)sdr3["ProdGroupCode"];
                    invDetail.NLAccCode = sdr3["NLAccCode"].ToString();
                    invDetail.StkDesc = (string)sdr3["StkDesc"];
                    invDetail.UserID = (int)sdr3["UserID"];
                    invDetail.ItemSerial = (string)sdr3["ItemSerial"];
                    invDetail.ItemQty = (int)sdr3["ItemQty"];
                    invDetail.ItemTotals = (float)sdr3["ItemTotals"];
                    invDetail.ItemUnitPrice = (float)sdr3["ItemUnitPrice"];
                    invDetail.DiscountPerc = sdr3["DiscountPerc"] != DBNull.Value ? (float)sdr3["DiscountPerc"] : 0;
                    invDetail.DiscountAmt = sdr3["DiscountAmt"] != DBNull.Value ? (float)sdr3["DiscountAmt"] : 0;
                    invDetail.AdditionalDetails = sdr3["AdditionalDetails"] != DBNull.Value
                        ? (string)sdr3["AdditionalDetails"]
                        : null;

                    invDetailsList.Add(invDetail);

                }

                cnn.Close();

                //start

                //check if customer exists
                string cust_check = "SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ";
                int cust_check_res = myDbconnection.CheckRowExists(cust_check, db);
                if (cust_check_res == 0)
                {
                    return BadRequest(new
                    { message = "No customer found with the parsed data was found. Request cancelled" });
                }

                //get selected customer
                AddCustomer selectedCustomer = new AddCustomer();
                cnn.Open();
                NpgsqlDataReader sdr_sc =
                    new NpgsqlCommand(
                            "SELECT * FROM \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + invoiceData.CustId + " ", cnn)
                        .ExecuteReader();
                while (sdr_sc.Read())
                {
                    selectedCustomer.SLCustomerSerial = (int)sdr_sc["SLCustomerSerial"];
                    selectedCustomer.CustCode = sdr_sc["CustCode"].ToString();
                    selectedCustomer.CustFirstName =
                        sdr_sc["CustFirstName"] != DBNull.Value ? (string)sdr_sc["CustFirstName"] : null;
                    selectedCustomer.Address = sdr_sc["Address"].ToString();
                    selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                    selectedCustomer.PostalAddress = sdr_sc["PostalAddress"].ToString();
                    selectedCustomer.CurCode = (int)sdr_sc["CurCode"];
                    selectedCustomer.CustEmail = sdr_sc["CustEmail"].ToString();
                    selectedCustomer.CustContact = sdr_sc["CustContact"].ToString();
                    selectedCustomer.SLCTypeID = (int)sdr_sc["SLCTypeID"];
                    selectedCustomer.CustLastName =
                        sdr_sc["CustLastName"] != DBNull.Value ? (string)sdr_sc["CustLastName"] : null;
                    selectedCustomer.CustType = sdr_sc["CustType"].ToString();
                    selectedCustomer.CustCompany =
                        sdr_sc["CustCompany"] != DBNull.Value ? (string)sdr_sc["CustCompany"] : null;
                    selectedCustomer.VATNo = sdr_sc["VATNo"] != DBNull.Value ? (string)sdr_sc["VATNo"] : null;
                    selectedCustomer.CustCreditLimit = (float)sdr_sc["CustCreditLimit"];
                    selectedCustomer.VATpin = sdr_sc["VATpin"].ToString();
                    selectedCustomer.CreditTerms = (int)sdr_sc["CreditTerms"];
                    selectedCustomer.CurrCode = sdr_sc["CurCode"].ToString();
                    selectedCustomer.CustBranch = sdr_sc["CustBranch"] != DBNull.Value ? (int)sdr_sc["CustBranch"] : 0;
                    selectedCustomer.CustRef = sdr_sc["CustRef"] != DBNull.Value ? (string)sdr_sc["CustRef"] : null;
                    selectedCustomer.CustomerDept =
                        sdr_sc["CustomerDept"] != DBNull.Value ? (decimal)sdr_sc["CustomerDept"] : 0;
                }

                cnn.Close();


                //check if customer dept id greater
                decimal customerDept = selectedCustomer.CustomerDept;

                //check customer credit limit
                decimal customer_credit_limit = (decimal)selectedCustomer.CustCreditLimit;

                decimal total_invoice_amount = 0;

                foreach (var t in invDetailsList)
                {
                    float computed_ttl = (t.ItemUnitPrice * t.ItemQty) + t.VatAmt - t.DiscountAmt;
                    total_invoice_amount += (decimal)computed_ttl;

                    //check inventory items quantity if can be deducted
                    NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(db));

                    cnn2.Open();
                    Inventory inv = new Inventory();
                    NpgsqlDataReader sdr_c =
                        new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtName\" = '" + t.StkDesc + "' ", cnn2)
                            .ExecuteReader();
                    if (sdr_c.Read())
                    {
                        inv.InvtName = sdr_c["InvtName"] != DBNull.Value ? (string)sdr_c["InvtName"] : null;
                        inv.InvtQty = sdr_c["InvtQty"] != DBNull.Value ? (int)sdr_c["InvtQty"] : 0;
                        inv.InvtType = sdr_c["InvtType"] != DBNull.Value ? (string)sdr_c["InvtType"] : null;

                        if (inv.InvtType == "GOODS")
                        {
                            if (t.ItemQty > inv.InvtQty)
                            {
                                return BadRequest(new
                                {
                                    message = "Cannot remove " + t.ItemQty + " from your inventory. You only have " +
                                              inv.InvtQty + " items left."
                                });
                            }
                        }

                    }

                    cnn2.Close();


                }

                decimal ttl_customer_dept = total_invoice_amount + customerDept;
                if (ttl_customer_dept > customer_credit_limit)
                {
                    return BadRequest(new
                    {
                        message = "The Invoice Amount " + string.Format("{0:#,0.00}", total_invoice_amount) +
                                  " exceeds the customer credit limit of " +
                                  string.Format("{0:#,0.00}", customer_credit_limit) +
                                  ". The Total debt of the customer inclusive the current invoice is sum upto " +
                                  string.Format("{0:#,0.00}", ttl_customer_dept) +
                                  " You can only add an invoice amount that does NOT exceed " +
                                  string.Format("{0:#,0.00}", total_invoice_amount - customerDept)
                    });
                }

                //check if invoice settings exists
                cnn.Open();
                NpgsqlDataReader sdr2 =
                    new NpgsqlCommand("Select * From \"SLInvoiceSettings\" WHERE \"InvBranch\" = " + staff_branch + "  LIMIT 1 ", cnn).ExecuteReader();
                if (sdr2.HasRows == false)
                {
                    return BadRequest(new
                    {
                        message =
                            "No data was found with the invoice settings. Please set the invoice  configurations to continue"
                    });
                }

                cnn.Close();

                //get last SLJrnNO
                int lastSLJRN = 0;
                cnn.Open();
                NpgsqlDataReader sdra =
                    new NpgsqlCommand("Select MAX(\"SLJrnlNo\") as sl From \"SLInvoiceHeader\" LIMIT 1 ", cnn)
                        .ExecuteReader();
                while (sdra.Read())
                {
                    lastSLJRN = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
                }

                cnn.Close();

                //get last NLJrnNO
                int lastNLJRN = 0;
                cnn.Open();
                NpgsqlDataReader sdrb =
                    new NpgsqlCommand("Select MAX(\"NlJrnlNo\") as sj From \"SLInvoiceHeader\" LIMIT 1 ", cnn)
                        .ExecuteReader();
                while (sdrb.Read())
                {
                    lastNLJRN = sdrb["sj"] != DBNull.Value ? (int)sdrb["sj"] : 0;
                }

                cnn.Close();


                decimal fullPrice = 0;
                decimal vatAmount = 0;

                //insert invoice details
                if (invDetailsList.Count > 0)
                {
                    foreach (var t in invDetailsList)
                    {
                        NpgsqlConnection cnn1_ = new NpgsqlConnection(new dbconnection().CheckConn(db));

                        //get inventory item id
                        cnn1_.Open();
                        Inventory inv = new Inventory();
                        NpgsqlDataReader sdr_inv =
                            new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtName\" = '" + t.StkDesc + "'  ",
                                cnn1_).ExecuteReader();
                        while (sdr_inv.Read())
                        {
                            inv.InvtId = sdr_inv["InvtId"] != DBNull.Value ? (int)sdr_inv["InvtId"] : 0;
                            inv.InvtName = sdr_inv["InvtName"] != DBNull.Value ? (string)sdr_inv["InvtName"] : null;
                            inv.InvtSP = sdr_inv["InvtSP"] != DBNull.Value ? (decimal)sdr_inv["InvtSP"] : 0;
                            inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
                            inv.InvtProdCode = sdr_inv["InvtProdCode"] != DBNull.Value
                                ? (string)sdr_inv["InvtProdCode"]
                                : null;
                        }

                        cnn1_.Close();

                        //get discount data
                        Discounts disc = new Discounts();

                        cnn1_.Open();
                        NpgsqlDataReader sdr_disc =
                            new NpgsqlCommand("SELECT * FROM \"Discounts\" WHERE \"DPerc\" = " + t.DiscountPerc + "  ",
                                cnn1_).ExecuteReader();
                        while (sdr_disc.Read())
                        {
                            disc.DPerc = sdr_disc["DPerc"] != DBNull.Value ? (float)sdr_disc["DPerc"] : 0;
                        }

                        cnn1_.Close();


                        //calculate proce and Qty
                        fullPrice = inv.InvtSP * t.ItemQty;

                        //calculate VAT
                        vatAmount = (decimal)t.VatAmt;

                        string unique_id = Guid.NewGuid().ToString("D");

                        cnn1_.Open();

                        //check if inventory id exists
                        string insertQuery1_ =
                            "INSERT INTO \"SLInvoiceDetail\" (\"SLJrnlNo\", \"JrnlSLNo\", \"VatCode\", \"VatAmt\", \"StkDesc\", \"UserID\", \"ItemSerial\", \"ItemQty\", \"ItemTotals\", \"ItemUnitPrice\", \"DiscountPerc\",\"DiscountAmt\", \"AdditionalDetails\",\"ItemId\",\"ItemCode\",\"ItemName\" ) VALUES(" +
                            (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + t.VatCode + "', " + vatAmount + ",'" +
                            t.StkDesc + "'," + userId + ",'" + unique_id + "', " + t.ItemQty + ", " + t.ItemTotals + ", " +
                            t.ItemUnitPrice + ", " + disc.DPerc + ", " + t.DiscountAmt + ", '" + t.AdditionalDetails +
                            "'," + inv.InvtId + ",'" + inv.InvtProdCode + "','" + inv.InvtName + "' ); ";
                        bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1_, db);
                        cnn1_.Close();
                        if (myReq1 == false)
                        {
                            //failed
                            return BadRequest(new { message = "An occurred while trying to save invoice details." });
                        }

                        //update inventory if goods
                        if (inv.InvtType == "GOODS")
                        {
                            string up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " + t.ItemQty +
                                            " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + inv.InvtId + " ";
                            bool myReq24 = myDbconnection.UpdateDelInsert(up_inv, db);
                            if (myReq24 == false)
                            {
                                //failed
                                return BadRequest(new
                                { message = "An occurred while trying to process Inventory update requests." });
                            }
                        }

                        //insert into warehouse summary
                        ManageWarehouseSummary whs = new ManageWarehouseSummary();
                        bool wh_req = whs.warehouse_summary_sl_pl(db, inv.InvtId, t.ItemQty, userId, "Sale");
                        if (wh_req == false)
                        {
                            //failed
                            return BadRequest(new
                            { message = "An occurred while trying to register request to warehouse summary." });
                        }

                        ///////////////////////////////////////////////////


                    }

                }

                //get last SLJrnNO
                int last_doc_ref = 0;
                cnn.Open();
                NpgsqlDataReader sdr_dc =
                    new NpgsqlCommand("Select MAX(\"DocRef\") as sl From \"SLInvoiceHeader\" LIMIT 1 ", cnn)
                        .ExecuteReader();
                while (sdr_dc.Read())
                {
                    last_doc_ref = sdr_dc["sl"] != DBNull.Value ? (int)sdr_dc["sl"] : 0;
                }

                cnn.Close();

                //insert into header
                string myquer =
                    "INSERT INTO \"SLInvoiceHeader\" (\"SLJrnlNo\", \"NlJrnlNo\", \"CustCode\", \"TransDate\", \"Period\", \"DocRef\", \"TotalAmount\", \"INVTypeRef\", \"Dispute\", \"DeliveryCust\", \"DeliveryAddress\", \"DeliveryDue\", \"INVDate\",\"CustId\", \"PaymentDays\",\"CustomRef\",\"CurrencyId\",\"DueDate\",\"SLDescription\",\"StaffID\",\"DocPrefix\",\"CRNReason\",\"HasCreditNote\", \"Branch\", \"InvPrinted\", \"TotalBalance\" ) VALUES(" +
                    (lastSLJRN + 1) + ", " + (lastNLJRN + 1) + ", '" + invoiceData.CustCode + "', '" + today +
                    "',  '" + invoiceData.Period + "', '" + (last_doc_ref + 1) + "'," + invoiceData.TotalAmount +
                    ",'INV','f',  " + invoiceData.DeliveryCust + ",  '" + invoiceData.DeliveryAddress + "',  '" +
                    invoiceData.DeliveryDue + "',  '" + invoiceData.INVDate + "', " + invoiceData.CustId + ",  " +
                    invoiceData.PaymentDays + ",  '" + invoiceData.CustomRef + "',  " + invoiceData.CurrencyId +
                    ", '" + invoiceData.DueDate + "', '" + invoiceData.SLDescription + "', " + userId + ", 'INV','','f'," + staff_branch + ",'f'," + invoiceData.TotalAmount + " ); ";

                bool sendq = myDbconnection.UpdateDelInsert(myquer, db);

                if (sendq == false)
                {
                    //failed
                    return BadRequest(new { message = "An occurred while trying to save proforma invoice details." });
                }

                //update proforma invoice status
                string updquer = "UPDATE \"SLProformaInvoiceHeader\" SET \"HasInvoice\" = 't' WHERE \"DocRef\" = " +
                                 CRN + " ";
                bool upd = myDbconnection.UpdateDelInsert(updquer, db);
                if (upd == false)
                {
                    //failed
                    return BadRequest(
                        new { message = "An occurred while trying to update proforma invoice details." });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Invoice has been successfully created"
                    });
                }
            }
            catch (Exception e)
            {
                return BadRequest(
                    new { message = e.Message });
            }

        }
    }
}
