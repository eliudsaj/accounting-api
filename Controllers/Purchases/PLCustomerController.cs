using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pyme_finance_api.Models.DBConn;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using pyme_finance_api.Models.JWT;
using Npgsql;
using pyme_finance_api.Models.Purchases.Customers;
using pyme_finance_api.Models.Settings;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Service.PlService;
using pyme_finance_api.Models.Sales;

namespace pyme_finance_api.Controllers.Purchases
{
    [Route("api/[controller]")]
    [ApiController]
    public class PLCustomerController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        readonly ILogger<PLCustomerController> _log;
        public PLCustomerController(IConfiguration config, ILogger<PLCustomerController> logger)
        {
            _configuration = config;
            _log = logger;
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult allcustomers()
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

            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;
            _log.LogInformation($"Fetching Purchase Customers");
            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get PLcstomers
            List<PLCustomer> plcustomerlist = new List<PLCustomer>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT \"PLCustomer\".*, \"Currencies\".\"CrCode\" FROM \"PLCustomer\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" WHERE \"CustBranch\" = "+staff_branch+"  ", cnn).ExecuteReader();

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
                plCust.PrimaryContact = sdr0["ContactPhone"] != DBNull.Value ? (string)sdr0["ContactPhone"] : null;
                plCust.ContactPerson = sdr0["ContactPerson"] != DBNull.Value ? (string)sdr0["ContactPerson"] : null;
                plCust.OpeningBalance = sdr0["OpeningBalance"] != DBNull.Value ? (decimal)sdr0["OpeningBalance"] : 0;
                plCust.HasOpeningBalance = (bool)sdr0["HasOpeningBalance"];
                plCust.MpesaNo = sdr0["MpesaNo"] != DBNull.Value ? (string)sdr0["MpesaNo"] : null;
                plCust.PayBillNo = sdr0["PayBillNo"] != DBNull.Value ? (string)sdr0["PayBillNo"] : null;
                plCust.BankName = sdr0["BankName"] != DBNull.Value ? (string)sdr0["BankName"] : null;
                plCust.Branch = sdr0["Branch"] != DBNull.Value ? (string)sdr0["Branch"] : null;
                plCust.AccountNumber = sdr0["AccountNumber"] != DBNull.Value ? (string)sdr0["AccountNumber"] : null;
                plCust.OpeningBalanceDate = (DateTime?)(sdr0["OpeningBalanceDate"] != DBNull.Value ? (DateTime?)sdr0["OpeningBalanceDate"] : null);
                plcustomerlist.Add(plCust);
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
            return Ok(new { PLCustomerList = plcustomerlist, CurrencyData = currencyList });
        }
        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult addCustomer(PLCustomer recvData)
        {          
            //validate data
            if (string.IsNullOrEmpty(recvData.CustName))
            {
                return BadRequest(new { message = "Missing customer name." });
            }
            else if (string.IsNullOrEmpty(recvData.PhysicalAddress))
            {
                return BadRequest(new { message = "Missing physical address." });
            }
            else if (string.IsNullOrEmpty(recvData.PostalAddress))
            {
                return BadRequest(new { message = "Missing postal address." });
            }
            else if (string.IsNullOrEmpty(recvData.CurrID.ToString()))
            {
                return BadRequest(new { message = "Missing currency." });
            }
            else if (string.IsNullOrEmpty(recvData.VATNo))
            {
                return BadRequest(new { message = "Missing VAT number." });
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
            _log.LogInformation($"User {userId}  created purchase customer {recvData.CustName}");
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

            ///TODO-check if a supplier has this KRA PIN
            /// update request model-Done
            /// update database -Done
            // string checkifKrapinExists = "SELECT * FROM \"PLCustomer\"  WHERE  \"VATNo\" = '"+recvData.VATNo+"';    ";
            string checkifSupplierNameExists = "SELECT * FROM \"PLCustomer\"  WHERE  \"CustName\" = '" + recvData.CustName+ "';    ";
           // int countkra = myDbconnection.CheckRowExists(checkifKrapinExists, db);
            int countcustname = myDbconnection.CheckRowExists(checkifSupplierNameExists, db);
            if (countcustname > 0)
            {
                return Ok(new { message = "Supplier with this name already exists", status = 400 });
            }
            //if (countkra > 0)
            //{
            //        return Ok(new { message = "Supplier with this KRA PIN already exists",status =400 });
            //}

            //get last curcode
            int lastcustID = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"CustID\"), 0) as ref From \"PLCustomer\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastcustID = (int)sdra["ref"];

            }
            cnn.Close();
           
            int lastcustomerID = (lastcustID + 1);
            string customercode = "CUST" + lastcustomerID.ToString("D4");

            //save customer           
            string insertQuery = "INSERT INTO \"PLCustomer\" (\"PLCustCode\", \"CustName\", \"PhysicalAddress\", \"PostalAddress\", \"CurrID\", \"VATNo\", \"CustID\", \"RegisterDate\", \"StaffID\", \"CustBranch\",\"ContactPerson\",\"ContactPhone\" ,\"BankName\",\"Branch\",\"AccountNumber\",\"MpesaNo\",\"PayBillNo\",\"Comment\")" +
                " VALUES('" + customercode + "', '" + recvData.CustName + "', '" + recvData.PhysicalAddress + "','" + recvData.PostalAddress + "', " + recvData.CurrID + ", '" + recvData.VATNo + "'," + lastcustomerID + ",'" + DateTime.Now + "'," +
                " " + userId + ","+staff_branch+",'"+recvData.ContactPerson.Trim().ToString()+"','"+recvData.PrimaryContact+"','"+recvData.BankName+"','"+recvData.Branch+"','"+recvData.AccountNumber+"','"+recvData.MpesaNo+"','"+recvData.PayBillNo+"','"+recvData.Comment+"' ); ";

            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery, db);
            if (myReq1 == true)
            {
                AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                AuditTrail auditTrail = new AuditTrail();
                auditTrail.action = "Customer " + recvData.CustName + " is added successfully  ";
                auditTrail.module = "PLCustomer";
                auditTrail.userId = userId;
                auditTrailService.createAuditTrail(auditTrail);

                return Ok(new { message = "Customer has been successfully created",status = 200 });
            }
            else
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to process request." ,status = 200});
            }
        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult updateCustomer(int cust, [FromBody] PLCustomer recvData)
        {
            //validate data
            if (string.IsNullOrEmpty(recvData.CustName))
            {
                return BadRequest(new { message = "Missing customer name." });

            }
            else if (string.IsNullOrEmpty(recvData.PhysicalAddress))
            {
                return BadRequest(new { message = "Missing physical address." });
            }
            else if (string.IsNullOrEmpty(recvData.PostalAddress))
            {
                return BadRequest(new { message = "Missing postal address." });
            }
            else if (string.IsNullOrEmpty(recvData.CurrID.ToString()))
            {
                return BadRequest(new { message = "Missing currency." });
            }
            else if (string.IsNullOrEmpty(recvData.VATNo))
            {
                return BadRequest(new { message = "Missing VAT number." });
            }
            else if (string.IsNullOrEmpty(cust.ToString()) || cust == 0)
            {
                return BadRequest(new { message = "Invalid customer reference" });
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
            _log.LogInformation($"User {userId}  updated purchase customer {recvData.CustName}");

            //save customer           
            string updateQuery = "UPDATE \"PLCustomer\" SET \"CustName\" = '" + recvData.CustName + "', \"PhysicalAddress\" = '" + recvData.PhysicalAddress + "', \"PostalAddress\" = '" + recvData.PostalAddress + "'," +
                " \"CurrID\" = " + recvData.CurrID + ", \"VATNo\" = '" + recvData.VATNo + "' " +
                ",\"ContactPerson\" = '"+recvData.ContactPerson+ "',\"ContactPhone\" = '"+recvData.PrimaryContact+ "'" +
                " ,\"BankName\" = '"+recvData.BankName+ "',\"Branch\" = '"+recvData.Branch+ "',\"AccountNumber\" = '"+recvData.AccountNumber+ "',\"MpesaNo\" = '"+recvData.MpesaNo+ "',\"PayBillNo\" = '"+recvData.PayBillNo+"'  WHERE \"CustID\" = " + cust + "  ";

            bool myReq1 = myDbconnection.UpdateDelInsert(updateQuery, db);
            if (myReq1 == true)
            {
                AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                AuditTrail auditTrail = new AuditTrail();
                auditTrail.action = "Customer " + recvData.CustName + " is updated successfully  ";
                auditTrail.module = "PLCustomer";
                auditTrail.userId = userId;
                auditTrailService.createAuditTrail(auditTrail);
                return Ok(new { message = "Customer has been successfully modified" });
            }
            else
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request." });
            }
        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult deleteCustomer(int cust)
        {
            //validate data
            if (string.IsNullOrEmpty(cust.ToString()) || cust == 0)
            {
                return BadRequest(new { message = "Invalid customer reference" });
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
            //save customer           
            string deleteQuery = "DELETE FROM \"PLCustomer\" WHERE \"CustID\" = " + cust + "  ";
            bool myReq1 = myDbconnection.UpdateDelInsert(deleteQuery, db);
            if (myReq1 == true)
            {
                AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                AuditTrail auditTrail = new AuditTrail();
                auditTrail.action = "Customer id " + cust  + " is deleted successfully  ";
                auditTrail.module = "PLCustomer";
                auditTrail.userId = userId;
                auditTrailService.createAuditTrail(auditTrail);
                return Ok(new { message = "Customer has been successfully removed" });
            }
            else
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request." });
            }
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult getCustomerStatement(int cust)
        {
            //validate data
            if (string.IsNullOrEmpty(cust.ToString()) || cust == 0)
            {
                return BadRequest(new { message = "Invalid customer reference" });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
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
            //get database name
            string db = companyRes;
            PlService plService = new PlService(tokenData);
            var data = plService.GetCustomerStatement(cust);
            var customer = plService.GetCustomerById(cust);
            var lic = plService.GettingCompanyDetails();
            return Ok(new { customerStatement = data, customerdata = customer, license = lic });
        }
        [Route("[action]")]
        [HttpGet]
        public ActionResult Get_Customers_purchaseAnalyis()
        {
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            PlService plService = new PlService(tokenData);
            // var result = plService.suppliersAgeAnalysis();
            var result = plService.getAccountsPayableAgeingReport();
            var lic = plService.GettingCompanyDetails();
            return Ok(new { accountspayableAgeingResponse = result, license = lic, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        public ActionResult Get_detailageing_report(string custid)
        {
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            PlService plService = new PlService(tokenData);
            // var result = plService.suppliersAgeAnalysis();
            var result = plService.getSupplierDetailAgeingReport(custid);
            var lic = plService.GettingCompanyDetails();
            return Ok(new { detailedaccountsreceivableageingreport = result, license = lic, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult getCustomerLedger(int cust, DateTime from, DateTime to)
        {
            //validate data
            if (string.IsNullOrEmpty(cust.ToString()) || cust == 0)
            {
                return BadRequest(new { message = "Invalid customer reference" });
            }
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
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
            //get database name
            string db = companyRes;
            PlService plService = new PlService(tokenData);
            var data = plService.GetCustomerLedger(cust, from, to);
            var customer = plService.GetCustomerById(cust);
            var lic = plService.GettingCompanyDetails();
            var invBalance = plService.GetInvoiceBalance(cust, from);
            var payBalance = plService.GetPaymentBalanceForward(cust, from);
            var crnBalance = plService.GetCrnBalanceForward(cust, from);
            return Ok(new { customerStatement = data, customerdata = customer, InvoiceBalance = invBalance, PaymentBalance = payBalance, CreditNoteBalance = crnBalance, license = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetSupplierActivities(int custId, DateTime from, DateTime to)
        {
            if(string.IsNullOrEmpty(custId.ToString()) || custId == 0)
            {
                return BadRequest(new { message = "Invalid Supplier Reference!!" });
            }
            var companyRes = "";
            int userId = 0;
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            jwt_token jwt = new jwt_token(_configuration);
            var tokendata = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if(tokendata != "")
            {
                companyRes = tokendata;
            }
            if(string.IsNullOrEmpty(companyRes))
            {
                return BadRequest(new { message = "Page Verification Failed, Cannot Find Your Client Connection Route!!" });
            }
            if(userId == 0)
            {
                return BadRequest(new { message = "User Could Not Be Found, Page Verification Failed!!" });
            }
            string db = companyRes;
            PlService plService = new PlService(db);
            var data = plService.GetSupplierActivity(custId, from, to);
            var supplier = plService.GetCustomerById(custId);
            var licence = plService.GettingCompanyDetails();
            var invBalance = plService.GetInvoiceBalance(custId, from);
            var payBalance = plService.GetPaymentBalanceForward(custId, from);
            var crnBalance = plService.GetCrnBalanceForward(custId, from);
            return Ok( new { customerStatement = data, customerdata = supplier, InvoiceBalance = invBalance, PaymentBalance = payBalance, CreditNoteBalance = crnBalance, license = licence});
        }
    }
}
