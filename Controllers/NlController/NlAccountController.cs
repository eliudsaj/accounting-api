using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pyme_finance_api.Models.JWT;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using pyme_finance_api.Models.DBConn;
using Npgsql;
using pyme_finance_api.Models.NL.NLAccount;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.CurrencyService;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Models.Authentication;

namespace pyme_finance_api.Controllers.NlController
{
    [Route("api/[controller]")]
    [ApiController]
    public class NlAccountController : ControllerBase
    {


        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;
        readonly ILogger<NlAccountController> _log;

        public NlAccountController(IConfiguration config, ILogger<NlAccountController> logger)
        {
            _configuration = config;
            _log = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_NlAccounts ()
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _log.LogInformation("fetching nominal accounts");

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
            else {
                return BadRequest(new { message = "Missing company Id" });
            }

            string db = companyRes;
            NlService nlService = new NlService(db);
            CurrencyService currencyService = new CurrencyService(db);

            var data = currencyService.GetCurrency();

        

            var nlaccounts = nlService.GetNlaccounts();
            var nlaccountgroups = nlService.GetNlaccountGroups();

            return Ok(new {  nlaccountlist = nlaccounts, Nlaccountgrouplist = nlaccountgroups ,CurrencyList = data });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetBalanceSheet()
        {
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
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
            License ls = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr1.HasRows == false)
            {
                //no license row found
                return BadRequest(new { message = "No license was found in your System.Please contact PYME FINANCE support" });
            }
            while (sdr1.Read())
            {
                ls.LsId = sdr1["LsId"] != DBNull.Value ? (int)sdr1["LsId"] : 0;
                ls.CompanyName = sdr1["CompanyName"] != DBNull.Value ? (string)sdr1["CompanyName"] : null;
                ls.LsType = sdr1["LsType"] != DBNull.Value ? (string)sdr1["LsType"] : null;
                ls.LsCode = sdr1["LsCode"] != DBNull.Value ? (string)sdr1["LsCode"] : null;
                ls.LsIssueDate = sdr1["LsIssueDate"] != DBNull.Value ? (DateTime)sdr1["LsIssueDate"] : DateTime.Now;
                ls.LsExpireDate = sdr1["LsExpireDate"] != DBNull.Value ? (DateTime)sdr1["LsExpireDate"] : DateTime.Now;
            }
            cnn.Close();
            NlService nlService = new NlService(db);
            var data = nlService.GetBalanceSheetReport();
            return Ok(data);
        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetTrialBalance()
        {
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
            string db = companyRes;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
            License ls = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr1.HasRows == false)
            {
                //no license row found
                return BadRequest(new { message = "No license was found in your System.Please contact PYME FINANCE support" });
            }
            while (sdr1.Read())
            {
                ls.LsId = sdr1["LsId"] != DBNull.Value ? (int)sdr1["LsId"] : 0;
                ls.CompanyName = sdr1["CompanyName"] != DBNull.Value ? (string)sdr1["CompanyName"] : null;
                ls.LsType = sdr1["LsType"] != DBNull.Value ? (string)sdr1["LsType"] : null;
                ls.LsCode = sdr1["LsCode"] != DBNull.Value ? (string)sdr1["LsCode"] : null;
                ls.LsIssueDate = sdr1["LsIssueDate"] != DBNull.Value ? (DateTime)sdr1["LsIssueDate"] : DateTime.Now;
                ls.LsExpireDate = sdr1["LsExpireDate"] != DBNull.Value ? (DateTime)sdr1["LsExpireDate"] : DateTime.Now;
            }
            cnn.Close();
            NlService nlService = new NlService(db);
            var data = nlService.GetTrialBalanceReports();
            data.First().CompanyName = ls.CompanyName;                 
          return Ok(data);
        }
        //[Route("[action]")]
        [HttpGet]
        [Authorize]
        [Route("Get_Nlaccount_bycode")]
        public ActionResult Get_NlAccountsByCode( string  code)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = " code is required" });
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
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nlaccount = nlService.getNlAccountsByCode(code);
            return Ok(new { nlaccount = nlaccount });
        }
        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult AddAccount(Nlaccount recvData)
        {
            //check data
            if (string.IsNullOrEmpty(recvData.NlaccName))
            {
                return BadRequest(new { message = "Missing acc name " });
            }
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request  
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            // string permissionName = Request.Headers["PermName"];
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
            //get database name
            string db = companyRes;
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
            NlService nlService = new NlService(db);
            var response = nlService.createNlaccount(recvData);
            if (response.Httpcode.Equals(400))
            {
                _log.LogError(response.Message);
                return BadRequest(new { message = response.Message });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"  Created NLAccount of name {recvData.NlaccName} at {DateTime.Now.ToString("dd-MM-yyyy")}";
            auditTrail.module = "NL Account";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            _log.LogInformation(auditTrail.action);
            //success
            return Ok(new{message = "Request has been successfully saved"});
        }
        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateNlAccount(Nlaccount recvData)
        {
            //check data
            if (string.IsNullOrEmpty(recvData.NlaccCode))
            {
                return BadRequest(new { message = "Missing account code " });
            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;
            //check if cookie exists in Request  
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
           // string permissionName = Request.Headers["PermName"];
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
            NlService nlService = new NlService(db);
            var response = nlService.updateNlaccount(recvData);
            if (response.Httpcode.Equals(400))
            {
                _log.LogError(response.Message);
                return BadRequest(new { message = response.Message });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"Updated NLAccount of name {recvData.NlaccName} at {DateTime.Now.ToString("dd-MM-yyyy")}";
            auditTrail.module = "NL Account";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _log.LogInformation(auditTrail.action);
            //success
            return Ok(new{ message = "Request has been successfully processed"});
        }
    }


}

