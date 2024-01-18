using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Models.Sales.Terms;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.VatService;
using pyme_finance_api.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using pyme_finance_api.Service.PlService;

namespace pyme_finance_api.Controllers.Settings
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxSetupController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();

        public TaxSetupController(IConfiguration config)
        {
            _configuration = config;
        }

        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult create_financial_period(FinancialPeriod recvData)
        {
            //check customer type
            if (string.IsNullOrEmpty(recvData.fp_name))
            {
                return BadRequest(new { message = "Missing financial period name" });
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

            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            //check if financial period name exists
            cnn.Open();
            NpgsqlDataReader sdr78 = new NpgsqlCommand("Select * From financial_periods WHERE fp_name = '" + recvData.fp_name + "' ", cnn).ExecuteReader();
            if (sdr78.HasRows == true)
            {
                return BadRequest(new { message = "The Financial period name is already in use." });
            }
            cnn.Close();

            //check fp ref
            cnn.Open();
            NpgsqlDataReader sdr79 = new NpgsqlCommand("Select * From financial_periods WHERE fp_ref = '" + recvData.fp_ref + "' ", cnn).ExecuteReader();
            if (sdr79.HasRows == true)
            {
                return BadRequest(new { message = "The Financial Period reference already exists." });
            }
            cnn.Close();

            //get last fp id
            int last_fp_id = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(fp_id) as sl From fp_ref LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_fp_id = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //get guid
            string int_Ref = System.Guid.NewGuid().ToString("D");

            // create inventory
            cnn.Open();
            string insertQ = "INSERT INTO financial_periods (fp_id, fp_ref, fp_name, fp_trans_date, fp_opening_date, fp_closing_date, fp_active, fp_createdby, fp_closedby, fp_authorisedby, fp_trans_option, fp_createdon, fp_branch)   " +
                "VALUES(" + (last_fp_id + 1) + ", '" + int_Ref + "', '" + recvData.fp_name + "', '" + recvData.fp_trans_date + "', '" + recvData.fp_openingdate + "' ,'" + recvData.fp_closingdate + "','f'," + userId + "," + 0 + ", " + 0 + ",'" + recvData.fp_trans_option + "', '" + DateTime.Today + ", " + staff_branch + " ); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, companyRes);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request." });
            }
            //success
            return Ok(new { message = "Request has been successfully processed" });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseTransactionListing(string transaction_type, DateTime from, DateTime to, string no)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.PurchaseTransactionListing(transaction_type, from, to, no);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseTransactionListingPeriod(string transaction_type, string period_from, string period_to, string no)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.PurchaseTransactionListingPeriod(transaction_type, period_from, period_to, no);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult SalesTransactionListing(string transaction_type, DateTime from, DateTime to, string no)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.SalesTransactionListing(transaction_type, from, to, no);

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult SalesTransactionListingPeriod(string transaction_type, string period_from, string period_to, string no)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.SalesTransactionListingPeriod(transaction_type, period_from, period_to, no);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult SalesAuditTrailPeriod(string transaction_type, string period_from, string period_to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.SaleAuditTrailByPeriod(transaction_type, period_from, period_to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult SalesAuditTrail(string transaction_type, DateTime from, DateTime to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.SalesAuditTrail(transaction_type, from, to);
            var dnote = vatAnalysisService.DeliveryNoteAuditByDate(transaction_type, from, to);

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, DnoteReport = dnote, companylicense = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseAuditTrailPeriod(string transaction_type, string period_from, string period_to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.PurchaseAuditTrailPeriod(transaction_type, period_from, period_to);

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

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
            return Ok(new { invdata = data, companylicense = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PurchaseAuditTrail(string transaction_type, DateTime from, DateTime to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.PurchaseAuditTrail(transaction_type, from, to);

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

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
            return Ok(new { invdata = data, companylicense = lic });
        }
        [HttpGet]
        [Route("[action]")]
        [Authorize]
        public ActionResult PurchaseVat3bByPeriod(string period_from, string period_to)
        {
            var companyRes = "";
            int userId = 0;
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string permissionName = Request.Headers["PermName"];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                companyRes = tokenData;
            }
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.Vat3BReportByPeriod(period_from, period_to);
            //licence
            NpgsqlConnection conn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
            License lic = new License();
            conn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" ", conn).ExecuteReader();
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
            conn.Close();
            return Ok(new { invdata = data, companylicense = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Vat3a(DateTime from, DateTime to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.VAT3AReport(from, to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Vat3b(DateTime from, DateTime to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.VAT3BReport(from, to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Vat3bByperiod(string from_period, string to_period)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string permissionName = Request.Headers["PermName"];
            //check if cookie exists in Request
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(companyRes);
            var data = vatAnalysisService.VAT3BbyPeriod(from_period, to_period);

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));
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
            return Ok(new { invdata = data, companylicense = lic });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_vat_details(int id)
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
            //get database name
            string db = companyRes;
            VatAnalysisService vatAnalysisService = new VatAnalysisService(db);
            var data = vatAnalysisService.getVatById(id);
            return Ok(new { result = data });
        }
        [Route("ListVATs")]
        [HttpGet]
        [Authorize]
        public ActionResult ListVATs()
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
            //    return Unauthorized(new { message = "Sorry! You are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;
            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            //tax list
            List<TaxSetup> taxList = new List<TaxSetup>();
            cnn.Open();
            NpgsqlDataReader sdr_vt = new NpgsqlCommand("SELECT * FROM \"VATs\" ", cnn).ExecuteReader();
            while (sdr_vt.Read())
            {
                TaxSetup tx = new TaxSetup();
                tx.VtId = sdr_vt["VtId"] != DBNull.Value ? (int)sdr_vt["VtId"] : 0;
                tx.VtRef = sdr_vt["VtRef"] != DBNull.Value ? (string)sdr_vt["VtRef"] : null;
                tx.VtPerc = sdr_vt["VtPerc"] != DBNull.Value ? (float)sdr_vt["VtPerc"] : 0;
                tx.VtSetDate = sdr_vt["VtSetDate"] != DBNull.Value ? (DateTime)sdr_vt["VtSetDate"] : DateTime.Now;
                tx.VtModifyDate = sdr_vt["VtModifyDate"] != DBNull.Value ? (DateTime)sdr_vt["VtModifyDate"] : DateTime.Now;
                tx.VtActive = sdr_vt["VtActive"] != DBNull.Value ? (bool)sdr_vt["VtActive"] : false;
                tx.VtBranch = sdr_vt["VtBranch"] != DBNull.Value ? (int)sdr_vt["VtBranch"] : 0;
                taxList.Add(tx);
            }
            cnn.Close();
            //invoice types
            List<InvoiceTypes> invTypes = new List<InvoiceTypes>();
            cnn.Open();
            NpgsqlDataReader sdr_it = new NpgsqlCommand("SELECT * FROM \"SLInvoiceTypes\" ", cnn).ExecuteReader();
            while (sdr_it.Read())
            {
                InvoiceTypes it = new InvoiceTypes();
                it.INVypeID = sdr_it["INVypeID"] != DBNull.Value ? (int)sdr_it["INVypeID"] : 0;
                it.INVType = sdr_it["INVType"] != DBNull.Value ? (string)sdr_it["INVType"] : null;
                it.INVComment = sdr_it["INVComment"] != DBNull.Value ? (string)sdr_it["INVComment"] : null;
                invTypes.Add(it);
            }
            cnn.Close();
            //invoice types
            List<InvoiceTypes> dinvTypes = new List<InvoiceTypes>();
            cnn.Open();
            NpgsqlDataReader sdr_it1 = new NpgsqlCommand("SELECT * FROM \"DeliveryNoteTypes\" ", cnn).ExecuteReader();
            while (sdr_it1.Read())
            {
                InvoiceTypes dit = new InvoiceTypes();
                dit.INVypeID = sdr_it1["Id"] != DBNull.Value ? (int)sdr_it1["Id"] : 0;
                dit.INVType = sdr_it1["Type"] != DBNull.Value ? (string)sdr_it1["Type"] : null;
                dit.INVComment = sdr_it1["Comment"] != DBNull.Value ? (string)sdr_it1["Comment"] : null;
                dinvTypes.Add(dit);
            }
            cnn.Close();

            //Delivery Note 
            List<CreditNoteType> crnType = new List<CreditNoteType>();
            cnn.Open();
            NpgsqlDataReader sdr_crn = new NpgsqlCommand("SELECT * FROM \"CreditNoteTypes\" ", cnn).ExecuteReader();
            while (sdr_crn.Read())
            {
                CreditNoteType crn = new CreditNoteType();
                crn.CRNId = sdr_crn["CRNId"] != DBNull.Value ? (int)sdr_crn["CRNId"] : 0;
                crn.CRNType = sdr_crn["CRNType"] != DBNull.Value ? (string)sdr_crn["CRNType"] : null;
                crn.CRNComment = sdr_crn["CRNComment"] != DBNull.Value ? (string)sdr_crn["CRNComment"] : null;
                crnType.Add(crn);
            }
            cnn.Close();
            //get internet defaults
            InvoiceSettings invDefaults = new InvoiceSettings();
            cnn.Open();
            NpgsqlDataReader sdr_is = new NpgsqlCommand("SELECT * FROM \"SLInvoiceSettings\" WHERE \"InvBranch\" = " + staff_branch + "  LIMIT 1 ", cnn).ExecuteReader();
            while (sdr_is.Read())
            {
                invDefaults.InvSettingId = sdr_is["InvSettingId"] != DBNull.Value ? (int)sdr_is["InvSettingId"] : 0;
                invDefaults.InvPrefix = sdr_is["InvPrefix"] != DBNull.Value ? (string)sdr_is["InvPrefix"] : null;
                invDefaults.InvStartNumber = sdr_is["InvStartNumber"] != DBNull.Value ? (int)sdr_is["InvStartNumber"] : 0;
                invDefaults.InvNumberingType = sdr_is["InvNumberingType"] != DBNull.Value ? (string)sdr_is["InvNumberingType"] : null;
                invDefaults.InvDeliveryNotes = sdr_is["InvDeliveryNotes"] != DBNull.Value ? (int)sdr_is["InvDeliveryNotes"] : 0;
                invDefaults.InvTypesCount = invTypes.Count;
            }
            cnn.Close();
            //Get all discounts
            List<Discounts> discList = new List<Discounts>();
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select * From \"Discounts\" ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Discounts disc = new Discounts();
                disc.DId = (int)sdr["DId"];
                disc.DRef = sdr["DRef"].ToString();
                disc.DPerc = (float)sdr["DPerc"];
                disc.DSetDate = (DateTime)sdr["DSetDate"];
                disc.DEndDate = (DateTime)sdr["DEndDate"];
                discList.Add(disc);
            }
            cnn.Close();
            //Get invoice terms and conditions
            Allterms invterms = new Allterms();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'inv_terms' ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                invterms.tosID = sdr1["tosID"] != DBNull.Value ? (int)sdr1["tosID"] : 0;
                invterms.tosType = sdr1["tosType"] != DBNull.Value ? (string)sdr1["tosType"] : null;
                invterms.terms = sdr1["terms"] != DBNull.Value ? sdr1["terms"].ToString() : null;
            }
            cnn.Close();
            // Decode the content for showing on Web page.
            invterms.terms = WebUtility.HtmlDecode(invterms.terms);
            NlService nlService = new NlService(db);
            var sLAnalysisCodes = nlService.GetSlanalysisCodes();
            var availablenlaccounts = nlService.GetNlaccounts();
            return Ok(new { VATs = taxList, InvoiceTypesData = invTypes, DeliveryNoteTypesData = dinvTypes, InvoiceConfigs = invDefaults, DiscountData = discList, InvTerms = invterms, SLAnalysisCodes = sLAnalysisCodes, Nlaccounts = availablenlaccounts, CreditNoteDataList = crnType });
        }
        [Route("ListPURCHASEVATs")]
        [HttpGet]
        [Authorize]
        public ActionResult ListPURCHASEVATs()
        {
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
            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return Unauthorized(new { message = "Sorry! You are denied access to this action or information. Please contact your system administrator for more information." });
            //}

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
            // Decode the content for showing on Web page.
            invterms.terms = WebUtility.HtmlDecode(invterms.terms);

            List<GoodReturnNoteType> good = new List<GoodReturnNoteType>();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand("SELECT * FROM \"GoodReturnNoteType\" ORDER BY \"GRNId\" ASC ", cnn).ExecuteReader();
            while (reader.Read())
            {
                GoodReturnNoteType goodReturn = new GoodReturnNoteType();
                goodReturn.GRNId = reader["GRNId"] != DBNull.Value ? (int)reader["GRNId"] : 0;
                goodReturn.GRNType = reader["GRNType"] != DBNull.Value ? (string)reader["GRNType"] : null;
                goodReturn.GRNComment = reader["GRNComment"] != DBNull.Value ? (string)reader["GRNComment"] : null;
                good.Add(goodReturn);
            }
            cnn.Close();

            List<PurchaseHeaderSettings> pHeader = new List<PurchaseHeaderSettings>();
            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand("SELECT * FROM \"Document_header\" WHERE \"Category\" = 'PURCHASE' ORDER BY \"id\" ASC; ", cnn).ExecuteReader();
            while (reader1.Read())
            {
                PurchaseHeaderSettings header = new PurchaseHeaderSettings();
                header.Id = reader1["id"] != DBNull.Value ? (int)reader1["id"] : 0;
                header.Category = reader1["Category"] != DBNull.Value ? (string)reader1["Category"] : null;
                header.DocumentName = reader1["DocumentName"] != DBNull.Value ? (string)reader1["DocumentName"] : null;
                header.Status = reader1["Status"] != DBNull.Value ? (bool)reader1["Status"] : false;
                pHeader.Add(header);
            }
            cnn.Close();

            return Ok(new { InvTerms = invterms, GRNDataList = good, HeaderSettingDataList = pHeader });
        }
        [Route("AddVAT")]
        [Authorize]
        [HttpPost]
        public ActionResult AddVAT(TaxSetup vatData)
        {
            //List<Users> clUsers;
            if (string.IsNullOrEmpty(vatData.VtRef))
            {
                return BadRequest(new { message = "Missing VAT reference name" });
            }
            else if (string.IsNullOrEmpty(vatData.VtPerc.ToString()))
            {
                return BadRequest(new { message = "Missing VAT percentage(%) amount" });
            }
            //set Date
            DateTime today = DateTime.Today;
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);
            //set Defaults
            vatData.VtModifyDate = DateTime.Now;
            vatData.VtSetDate = DateTime.Now;
            vatData.VtId = uniqueId;
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
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            //}
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            //check if there is any data stored in VAT
            string check_q = "SELECT * FROM \"VATs\" WHERE \"VtBranch\" = " + staff_branch + " ";
            int storedVATcount = myDbconnection.CheckRowExists(check_q, db);
            if (storedVATcount == 0)
            {
                vatData.VtActive = true;
            }
            else
            {
                vatData.VtActive = false;
            }
            //VAt Data
            string check_ref_q = "SELECT * FROM \"VATs\" WHERE \"VtRef\" = '" + vatData.VtRef + "' ";
            int check_ref_res = myDbconnection.CheckRowExists(check_ref_q, db);
            if (check_ref_res > 0)
            {
                return BadRequest(new { message = "VAT reference '<span class='text-warning'>" + vatData.VtRef + "</span>' is already registered with another VAT value" });
            }
            //insert vAT data
            string insrt_vat = "INSERT INTO \"VATs\"(\"VtRef\",\"VtPerc\",\"VtSetDate\",\"VtModifyDate\",\"VtActive\",\"VtBranch\") VALUES ( '" + vatData.VtRef + "','" + vatData.VtPerc + "' ,'" + vatData.VtSetDate + "', '" + vatData.VtModifyDate + "', '" + vatData.VtActive + "'," + staff_branch + ") ";
            bool insrt_res = myDbconnection.UpdateDelInsert(insrt_vat, db);
            if (insrt_res == false)
            {
                return BadRequest(new { message = "Sorry! An error occurred while trying to create a new VAT account." });
            }
            return Ok(new { message = "VAT Reference " + vatData.VtRef + "(" + vatData.VtPerc + "%)" + " has been successfully registered." });
        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult DeactivateVatItem(int id)
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
            //get database name
            string db = companyRes;
            VatAnalysisService vatAnalysisService = new VatAnalysisService(db);
            var data = vatAnalysisService.deactivateVATItem(id);
            if (data.Httpcode == 400)
            {
                return BadRequest(new { message = data.Message });
            }
            return Ok( new { message = data.Message });
        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult ActivateVatItem(int id)
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
            //get database name
            string db = companyRes;
            VatAnalysisService vatAnalysisService = new VatAnalysisService(db);
            var data = vatAnalysisService.activateVATItem(id);
            if (data.Httpcode == 400)
            {
                return BadRequest(new { message = data.Message });
            }
            return Ok( new { message = data.Message });
        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateVatItem(TaxSetup vatData)
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
            //get database name
            string db = companyRes;
            VatAnalysisService vatAnalysisService = new VatAnalysisService(db);
            var data = vatAnalysisService.editVatItem(vatData);
            if (data.Httpcode == 400)
            {
                return BadRequest(new { message = data.Message });
            }
            return Ok(new { message = data.Message });
        }
        [Route("UpdateCRNType")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateCRNType(int key, [FromBody] CreditNoteType crn)
        {
            var companyRes = "";
            int userId = 0;
            string autheHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = autheHeader.Split(' ')[1];
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
            string crn_check = "SELECT * FROM \"CreditNoteTypes\" WHERE \"CRNId\" = '"+ key +"' ";
            int crn_check_result = myDbconnection.CheckRowExists(crn_check, db);
            if(crn_check_result == 0)
            {
                return BadRequest(new { message = "Error! The Credit Note type reference parsed was NOT found." });
            }
            CreditNoteType creditNoteType = new CreditNoteType();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand("SELECT * FROM \"CreditNoteTypes\" WHERE \"CRNId\" = '"+ key +"' ", cnn).ExecuteReader();
            while (reader.Read())
            {
                creditNoteType.CRNId = reader["CRNId"] != DBNull.Value ? (int)reader["CRNId"] : 0;
                creditNoteType.CRNType = reader["CRNType"] != DBNull.Value ? (string)reader["CRNType"] : null;
                creditNoteType.CRNComment = reader["CRNComment"] != DBNull.Value ? (string)reader["CRNComment"] : null;
            }
            cnn.Close();
            if (!string.IsNullOrEmpty(crn.CRNType))
            {
                creditNoteType.CRNType = crn.CRNType;
            }else if (!string.IsNullOrEmpty(crn.CRNComment))
            {
                creditNoteType.CRNComment = crn.CRNComment;
            }
            string update_query = "UPDATE \"CreditNoteTypes\" SET \"CRNType\" = '"+ creditNoteType.CRNType +"', \"CRNComment\" = '"+ creditNoteType.CRNComment +"' WHERE \"CRNId\" = '"+ key +"' ";
            bool upd_qr_res = myDbconnection.UpdateDelInsert(update_query, db);
            if (upd_qr_res == false)
            {
                return BadRequest(new { message = "No changes were made in your update request." });
            }
            return Ok( new { message = "Data has been successfully updated" });
        }
        [Route("UpdateDNInvoiceType")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateDNInvoiceType(int key, [FromBody] InvoiceTypes invData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            //string permissionName = Request.Headers["PermName"];

            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}
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
            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //check if there is any data stored in VAT
            string sl_qr = "SELECT * FROM \"DeliveryNoteTypes\" WHERE \"Id\" = " + key + " ";
            int sl_qr_res = myDbconnection.CheckRowExists(sl_qr, db);
            if (sl_qr_res == 0)
            {
                return BadRequest(new { message = "Error! The invoice type reference parsed was NOT found." });
            }
            //select invoice types
            InvoiceTypes invTypesStoredData = new InvoiceTypes();
            cnn.Open();
            NpgsqlDataReader sdr_invt = new NpgsqlCommand("SELECT * FROM \"DeliveryNoteTypes\" WHERE \"Id\" = " + key + " ", cnn).ExecuteReader();
            while (sdr_invt.Read())
            {
                invTypesStoredData.INVypeID = sdr_invt["Id"] != DBNull.Value ? (int)sdr_invt["Id"] : 0;
                invTypesStoredData.INVType = sdr_invt["Type"] != DBNull.Value ? (string)sdr_invt["Type"] : null;
                invTypesStoredData.INVComment = sdr_invt["Comment"] != DBNull.Value ? (string)sdr_invt["Comment"] : null;
            }
            cnn.Close();
            if (!string.IsNullOrEmpty(invData.INVType))
            {
                invTypesStoredData.INVType = invData.INVType;
            }
            else if (!string.IsNullOrEmpty(invData.INVComment))
            {
                invTypesStoredData.INVComment = invData.INVComment;
            }
            //update invoice type
            string upd_qr = "UPDATE \"DeliveryNoteTypes\" SET \"Type\" = '" + invTypesStoredData.INVType + "',\"Comment\" = '" + invTypesStoredData.INVComment + "' WHERE \"Id\" = " + key + "  ";
            bool upd_qr_res = myDbconnection.UpdateDelInsert(upd_qr, db);
            if (upd_qr_res == false)
            {
                return BadRequest(new { message = "No changes were made in your update request." });
            }
            return Ok(new { message = "Data has been successfully updated" });
        }
        [Route("UpdateInvoiceType")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateInvoiceType(int key, [FromBody] InvoiceTypes invData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            //string permissionName = Request.Headers["PermName"];

            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}
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
            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //check if there is any data stored in VAT
            string sl_qr = "SELECT * FROM \"SLInvoiceTypes\" WHERE \"INVypeID\" = " + key + " ";
            int sl_qr_res = myDbconnection.CheckRowExists(sl_qr, db);
            if (sl_qr_res == 0)
            {
                return BadRequest(new { message = "Error! The invoice type reference parsed was NOT found." });
            }
            //select invoice types
            InvoiceTypes invTypesStoredData = new InvoiceTypes();
            cnn.Open();
            NpgsqlDataReader sdr_invt = new NpgsqlCommand("SELECT * FROM \"SLInvoiceTypes\" WHERE \"INVypeID\" = " + key + " ", cnn).ExecuteReader();
            while (sdr_invt.Read())
            {
                invTypesStoredData.INVypeID = sdr_invt["INVypeID"] != DBNull.Value ? (int)sdr_invt["INVypeID"] : 0;
                invTypesStoredData.INVType = sdr_invt["INVType"] != DBNull.Value ? (string)sdr_invt["INVType"] : null;
                invTypesStoredData.INVComment = sdr_invt["INVComment"] != DBNull.Value ? (string)sdr_invt["INVComment"] : null;

            }
            cnn.Close();
            if (!string.IsNullOrEmpty(invData.INVType))
            {
                invTypesStoredData.INVType = invData.INVType;
            }
            else if (!string.IsNullOrEmpty(invData.INVComment))
            {
                invTypesStoredData.INVComment = invData.INVComment;
            }
            //update invoice type
            string upd_qr = "UPDATE \"SLInvoiceTypes\" SET \"INVType\" = '" + invTypesStoredData.INVType + "', \"INVComment\" = '" + invTypesStoredData.INVComment + "' WHERE \"INVypeID\" = " + key + "  ";
            bool upd_qr_res = myDbconnection.UpdateDelInsert(upd_qr, db);
            if (upd_qr_res == false)
            {
                return BadRequest(new { message = "No changes were made in your update request." });
            }
            return Ok(new { message = "Data has been successfully updated" });
        }
        [Route("UpdateInvoiceNumberingType")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateInvoiceNumberingType(string NuminvType)
        {
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
            cnn.Open();
            string myQuery = "UPDATE \"SLInvoiceSettings\" SET \"InvNumberingType\" = '" + NuminvType + "'  ";
            bool myReq = myDbconnection.UpdateDelInsert(myQuery, db);
            cnn.Close();
            if (myReq == true)
            {
                //success
                return Ok(new { message = "Invoice numbering type data has been successfully saved" });
            }
            else
            {
                //failed
                return BadRequest(new { message = "An occured while trying to save invoice numbering type data." });
            }
        }
        [Route("VatAnalysis")]
        [Authorize]
        [HttpGet]
        public ActionResult vatAnalysis(DateTime from, DateTime to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(tokenData);
            var data = vatAnalysisService.vatanalysis(from, to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
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
            return Ok(new { vatanalysis = data, companyinfo = lic });
        }
        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult vatAnalysisPeriod(string period_from, string period_to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(tokenData);
            var data = vatAnalysisService.vatanalysisPeriod(period_from, period_to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
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
            return Ok(new { vatanalysis = data, companyinfo = lic });
        }
        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult vatPurchaseAnalysisPeriod(string period_from, string period_to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            string jwtHeader = authHeader.Split(' ')[1];
            //get token data
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "") { companyRes = tokenData; }
            //check companyRes
            if (string.IsNullOrEmpty(companyRes)) { return BadRequest(new { message = "Cannot find your client connection route. Page verification failed" }); }
            //check user id
            if (userId == 0) { return BadRequest(new { message = "Missing user reference. Page verification failed" }); }
            VatAnalysisService vatAnalysisService = new VatAnalysisService(tokenData);
            var data = vatAnalysisService.vatPurchaseanalysisByPeriod(period_from, period_to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
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
            return Ok(new { vatanalysis = data, companyinfo = lic });
        }
        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult vatPurchaseAnalysis(DateTime from, DateTime to)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
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
            VatAnalysisService vatAnalysisService = new VatAnalysisService(tokenData);
            var data = vatAnalysisService.vatPurchaseanalysis(from, to);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
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
            return Ok(new { vatanalysis = data, companyinfo = lic });
        }
        [Route("DeleteCRNType")]
        [Authorize]
        [HttpPut]
        public ActionResult DeleteCRNType(int key)
        {
            var companyRes = "";
            int userId = 0;
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                companyRes = tokenData;
            }
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
            string sl_qr = "SELECT * FROM \"CreditNoteTypes\" WHERE \"CRNId\" = " + key + " ";
            int sl_qr_res = myDbconnection.CheckRowExists(sl_qr, db);
            if (sl_qr_res == 0)
            {
                return BadRequest(new { message = "Error! The Credit Note type reference parsed was NOT found." });
            }

            string del_qr = "DELETE  FROM \"CreditNoteTypes\" WHERE \"CRNId\" = " + key + " ";
            bool del_qr_res = myDbconnection.UpdateDelInsert(del_qr, db);
            if (del_qr_res == false)
            {
                return BadRequest(new { message = "An error occurred while trying to remove item from database." });
            }

            return Ok(new { message = "Credit Note Type has been successfully deleted" });
        }
        [Route("DeleteDNInvoiceType")]
        [Authorize]
        [HttpPut]
        public ActionResult DeleteDNInvoiceType(int key)
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];

            //permission name
            //string permissionName = Request.Headers["PermName"];

            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}

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
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}


            //get database name
            string db = companyRes;

            string sl_qr = "SELECT * FROM \"DeliveryNoteTypes\" WHERE \"Id\" = " + key + " ";
            int sl_qr_res = myDbconnection.CheckRowExists(sl_qr, db);
            if (sl_qr_res == 0)
            {
                return BadRequest(new { message = "Error! The invoice type reference parsed was NOT found." });
            }

            string del_qr = "DELETE  FROM \"DeliveryNoteTypes\" WHERE \"Id\" = " + key + " ";
            bool del_qr_res = myDbconnection.UpdateDelInsert(del_qr, db);
            if (del_qr_res == false)
            {
                return BadRequest(new { message = "An error occurred while trying to remove item from database." });
            }

            return Ok(new
            {
                message = "Data has been successfully deleted"

            });
        }

        [Route("DeleteInvoiceType")]
        [Authorize]
        [HttpPut]
        public ActionResult DeleteInvoiceType(int key)
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];

            //permission name
            //string permissionName = Request.Headers["PermName"];

            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}

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
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}


            //get database name
            string db = companyRes;

            string sl_qr = "SELECT * FROM \"SLInvoiceTypes\" WHERE \"INVypeID\" = " + key + " ";
            int sl_qr_res = myDbconnection.CheckRowExists(sl_qr, db);
            if (sl_qr_res == 0)
            {
                return BadRequest(new { message = "Error! The invoice type reference parsed was NOT found." });
            }

            string del_qr = "DELETE  FROM \"SLInvoiceTypes\" WHERE \"INVypeID\" = " + key + " ";
            bool del_qr_res = myDbconnection.UpdateDelInsert(del_qr, db);
            if (del_qr_res == false)
            {
                return BadRequest(new { message = "An error occurred while trying to remove item from database." });
            }

            return Ok(new
            {
                message = "Data has been successfully deleted"

            });


        }
        [Route("AddCRNType")]
        [Authorize]
        [HttpPost]
        public ActionResult AddCRNType(CreditNoteType crnType)
        {
            if (string.IsNullOrEmpty(crnType.CRNType))
            {
                return BadRequest(new { message = "Missing required Credit Note Type!!" });
            }else if (string.IsNullOrEmpty(crnType.CRNComment))
            {
                return BadRequest(new { meassage = "Missing required Credit Note Comment!!" });
            }
            var companyRes = "";
            int userId = 0;
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string permissionName = Request.Headers["PermName"];
            if (string.IsNullOrEmpty(permissionName))
            {
                return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            }
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
            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            int last_crnType = 0;
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT COALESCE(MAX(\"CRNId\"),0) as st FROM \"CreditNoteTypes\"; ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                last_crnType = (int)sdr0["st"];
            }
            cnn.Close();
            string check_crnType_exit = "SELECT * FROM \"CreditNoteTypes\" WHERE \"CRNType\" = '"+ crnType.CRNType +"' ";
            int check_res = myDbconnection.CheckRowExists(check_crnType_exit, db);
            if(check_res > 0)
            {
                return BadRequest(new { message = "Sorry! The Credit Note type you are trying to register already exists." });
            }
            last_crnType = last_crnType + 1;
            string insert_crnType = "INSERT INTO \"CreditNoteTypes\" (\"CRNId\", \"CRNType\", \"CRNComment\") VALUES ('"+ last_crnType +"', '"+ crnType.CRNType +"', '"+ crnType.CRNComment +"') ";
            bool insert_result = myDbconnection.UpdateDelInsert(insert_crnType, db);
            if(insert_result == false)
            {
                return BadRequest(new { message = "Sorry! An error occurred while trying to create a new Credit Note entity." });
            }
            return Ok(new { message = "Credit Note type '" + crnType.CRNType + "' has been successfully registered." });
        }
        [Route("AddDNType")]
        [Authorize]
        [HttpPost]
        public ActionResult AddDNType(InvoiceTypes invtypeData)
        {
            if (string.IsNullOrEmpty(invtypeData.INVType))
            {
                return BadRequest(new { message = "Missing requuired invoice type" });
            }
            else if (string.IsNullOrEmpty(invtypeData.INVComment))
            {
                return BadRequest(new { message = "Missing required invoice comment" });
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
                return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            // create connection string
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get last invtype ID
            int last_invtype = 0;
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("SELECT COALESCE(MAX(\"Id\"),0) as st FROM \"DeliveryNoteTypes\" ", cnn).ExecuteReader();
            while (sdr2.Read())
            {
                last_invtype = (int)sdr2["st"];
            }
            cnn.Close();
            //check invoice type if exists
            string check_inv_type_qr = "SELECT * FROM \"DeliveryNoteTypes\" WHERE \"Type\" = '" + invtypeData.INVType + "' ";
            int check_res = myDbconnection.CheckRowExists(check_inv_type_qr, db);
            if (check_res > 0)
            {
                return BadRequest(new { message = "Sorry! The invoice type you are trying to register already exists." });
            }
            last_invtype = last_invtype + 1;
            //Insert data
            string insrt_type =
                "INSERT INTO \"DeliveryNoteTypes\" (\"Id\"  ,\"Type\", \"Comment\" ) VALUES ('" + last_invtype + "', '" + invtypeData.INVType + "', '" + invtypeData.INVComment + "' ) ";
            bool insrt_res = myDbconnection.UpdateDelInsert(insrt_type, db);
            if (insrt_res == false)
            {
                return BadRequest(new { message = "Sorry! An error occurred while trying to create a new entity." });
            }
            return Ok(new { message = "Delivery Note type '" + invtypeData.INVType + "' has been successfully registered."});
        }
        [Route("AddInvoiceType")]
        [Authorize]
        [HttpPost]
        public ActionResult AddInvoiceType(InvoiceTypes invtypeData)
        {

            if (string.IsNullOrEmpty(invtypeData.INVType))
            {
                return BadRequest(new { message = "Missing requuired invoice type" });
            }
            else if (string.IsNullOrEmpty(invtypeData.INVComment))
            {
                return BadRequest(new { message = "Missing required invoice comment" });
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
                return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            //get database name
            string db = companyRes;

            // create connection string
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get last invtype ID
            int last_invtype = 0;

            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("SELECT COALESCE(MAX(\"INVypeID\"),0) as st FROM \"SLInvoiceTypes\" ", cnn).ExecuteReader();

            while (sdr2.Read())
            {
                last_invtype = (int)sdr2["st"];
            }
            cnn.Close();

            //check invoice type if exists
            string check_inv_type_qr = "SELECT * FROM \"SLInvoiceTypes\" WHERE \"INVType\" = '" + invtypeData.INVType + "' ";
            int check_res = myDbconnection.CheckRowExists(check_inv_type_qr, db);
            if (check_res > 0)
            {
                return BadRequest(new { message = "Sorry! The invoice type you are trying to register already exists." });
            }

            //Insert data
            string insrt_type =
                "INSERT INTO \"SLInvoiceTypes\" ( \"INVypeID\", \"INVType\", \"INVComment\" ) VALUES (" +
                (last_invtype + 1) + ", '" + invtypeData.INVType + "', '" + invtypeData.INVComment + "' ) ";
            bool insrt_res = myDbconnection.UpdateDelInsert(insrt_type, db);
            if (insrt_res == false)
            {
                return BadRequest(new { message = "Sorry! An error occurred while trying to create a new entity." });
            }


            return Ok(new
            {
                message = "Invoice type '" + invtypeData.INVType + "' has been successfully registered."

            });


        }
        [Route("UpdateInvSettings")]
        [Authorize]
        [HttpPost]
        public ActionResult UpdateInvSettings(string query, [FromBody] InvoiceSettings invsettingsData)
        {
            if (query == "")
            {
                return BadRequest(new { message = "Required request update preference" });
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
                return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            }


            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }

            //check if inv setting exists
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            string sl_invsettings = "SELECT * FROM \"SLInvoiceSettings\" WHERE \"InvBranch\" = " + staff_branch + "  ";
            int sl_res = myDbconnection.CheckRowExists(sl_invsettings, db);
            if (sl_res == 0)
            {
                return BadRequest(new { message = "Sorry! Default configurations are missing. Please contact support for help." });
            }

            //InvSettings Data
            InvoiceSettings storedInvSettings = new InvoiceSettings();

            cnn.Open();
            NpgsqlDataReader sdr_is = new NpgsqlCommand(sl_invsettings, cnn).ExecuteReader();
            while (sdr_is.Read())
            {
                storedInvSettings.InvSettingId = sdr_is["InvSettingId"] != DBNull.Value ? (int)sdr_is["InvSettingId"] : 0;
                storedInvSettings.InvPrefix = sdr_is["InvPrefix"] != DBNull.Value ? (string)sdr_is["InvPrefix"] : null;
                storedInvSettings.InvStartNumber = sdr_is["InvStartNumber"] != DBNull.Value ? (int)sdr_is["InvStartNumber"] : 0;
                storedInvSettings.InvNumberingType = sdr_is["InvNumberingType"] != DBNull.Value ? (string)sdr_is["InvNumberingType"] : null;
                storedInvSettings.InvDeliveryNotes = sdr_is["InvDeliveryNotes"] != DBNull.Value ? (int)sdr_is["InvDeliveryNotes"] : 0;
            }
            cnn.Close();

            if (query == "prefix")
            {

                if (string.IsNullOrEmpty(invsettingsData.InvPrefix))
                {
                    return BadRequest(new { message = "Missing required invoice prefix" });
                }
                else
                {
                    storedInvSettings.InvPrefix = invsettingsData.InvPrefix;
                }

            }
            else if (query == "startno")
            {
                if (invsettingsData.InvStartNumber == 0)
                {
                    return BadRequest(new { message = "Missing or undefined required invoice start number" });
                }
                else
                {
                    storedInvSettings.InvStartNumber = invsettingsData.InvStartNumber;
                }
            }
            //update changes
            string upd_qr = "UPDATE \"SLInvoiceSettings\" SET \"InvPrefix\" = '" + storedInvSettings.InvPrefix + "', \"InvStartNumber\" = " + storedInvSettings.InvStartNumber + ", \"InvNumberingType\" = '" + storedInvSettings.InvNumberingType + "', \"InvDeliveryNotes\" = " + storedInvSettings.InvDeliveryNotes + " WHERE \"InvBranch\" = " + staff_branch + "  ";
            bool upd_res = myDbconnection.UpdateDelInsert(upd_qr, db);
            if (upd_res == false)
            {
                return BadRequest(new { message = "No changes were made while trying to modify Invoice settings." });
            }

            return Ok(new
            {
                message = "Invoice settings have been successfully saved! "

            });


        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult UpdatePurchaseHeaderSettings([FromBody] PurchaseHeaderSettings headerSettings, int key)
        {
            var companyRes = "";
            int userId = 0;
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string permissionName = Request.Headers["PermName"];
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
            //check permission
            bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            if (perStatus == false)
            {
                return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            }
            //get database name
            string db = companyRes;
            PlService plService = new PlService(db);
            headerSettings.Id = key;
            var response = plService.UpdateHeaderSettings(headerSettings);
            if (response.Httpcode.Equals(400))
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = "Good Return Note Type "+ headerSettings.DocumentName +" has Successfully Updated.", Code = 200 });
        }
    }
}
