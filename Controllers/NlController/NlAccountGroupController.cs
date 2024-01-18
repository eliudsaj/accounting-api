using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Org.BouncyCastle.Ocsp;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.NL.NlAccountGroup;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Service.NlServices;

namespace pyme_finance_api.Controllers.NlController
{
    [Route("api/[controller]")]
    [ApiController]
    public class NlAccountGroupController : ControllerBase
    {


        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;
        readonly ILogger<NlAccountGroupController> _log;





        public NlAccountGroupController(IConfiguration config, ILogger<NlAccountGroupController> logger)
        {
            _configuration = config;
            _log = logger;

        }



        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_NlAccountGroups()
        {

            _log.LogInformation("fetching NLAccount Groups");
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];


            //permission name
            //   string permissionName = Request.Headers["PermName"];


            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));

            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //get database name
            string db = companyRes;

            NlService nlService = new NlService(db);

            var data = nlService.GetNlaccountGroups();
            return Ok(new { nlaccoungrouptlist = data });
        }



        [HttpGet]
        [Authorize]
        [Route("Get_Nlaccountgroup_bycode")]
        public ActionResult Get_NlAccountsByCode(string code)
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _log.LogInformation($"fetching NLAccount of code {code}");

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

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //get database name
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nlaccountGroup = nlService.getNlAccountGroupByCode(code);

            return Ok(new { nlaccountGroup = nlaccountGroup });
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateCustomerBroughtForward")]
        public ActionResult UpdateCustomerBroughtForward(AccountsReceivableBroughtForward recvData)
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

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //get database name
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nlaccountGroup = nlService.createCustomerBroughtForward(recvData);

            return Ok(new { Code = nlaccountGroup.Httpcode, Message = nlaccountGroup.Message });
        }





        [HttpPost]
        [Authorize]
        [Route("UpdateSupplierBroughtForward")]
        public ActionResult UpdateSupplierBroughtForward(AccountsReceivableBroughtForward recvData)
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

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }

            //get database name
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nlaccountGroup = nlService.createSupplierBroughtForward(recvData);

            return Ok(new { Code = nlaccountGroup.Httpcode, Message = nlaccountGroup.Message });
        }


        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult AddAccountGroups(NlaccountGroup recvData)
        {

            //check data
            if (string.IsNullOrEmpty(recvData.GroupName))
            {
                return BadRequest(new { message = "Missing group name " });

            }
            _log.LogInformation($"Creating AccountGroup {recvData.GroupName}");

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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.OpenAsync();



            int groupnamecount = 0;

            ////check if group name exists
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"NLAccountGroup\" where  \"GroupName\"= '" + recvData.GroupName + "'", cnn).ExecuteReader();


            while (sdr1.Read())
            {
                groupnamecount++;
            }
            if (groupnamecount > 1)
            {
                _log.LogError($" AccountGroup of name {recvData.GroupName} already exists");
                return BadRequest(new { message = "This Group Name already exists" });
            }
            cnn.Close();






            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
            int count = 0;

            /// NOT BEST SOLUTION
            cnn1.Open();
        
            /// 

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT MAX(\"GroupCode\") as sj FROM \"NLAccountGroup\"", cnn1).ExecuteReader();
            /// 
            while (sdr0.Read())
            {
                string max = sdr0["sj"] != DBNull.Value ? (string)sdr0["sj"] : "000";


                //string max = (string)sdr0["sj"];
                string removedpadding = max.Remove(0,2);
                count = Int32.Parse(removedpadding) + 1;
                //  count = max + 1;
            }


            string stringcount = count.ToString();
            string strvalue = stringcount.PadLeft(4, '0');
            string groupCode = "RG" + strvalue;


            string insertQuery1 = "INSERT INTO \"NLAccountGroup\" (\"GroupCode\",\"GroupName\",\"PriGroupCode\",\"GroupType\",\"GroupSubType\",\"GroupLevel\",\"DefaultGroup\") " +
                "VALUES('" + groupCode + "','" + recvData.GroupName + "', '" + recvData.PriGroupName + "', '" + recvData.GroupType + "', '" + recvData.GroupSubType + "', '" + recvData.GroupLevel + "','" + recvData.DefaultGroup + "' );";

            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);

            cnn1.Close();

            if (myReq1 == false)
            {
                _log.LogError($"  AccountGroup of name {recvData.GroupName} had an error on creating ");
                //failed
                return BadRequest(new { message = "An occured while trying to save details." });
            }

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"  Created AccountGroup of name {recvData.GroupName} at {DateTime.Now.ToString("dd-MM-yyyy")}";
            auditTrail.module = "Account Group";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _log.LogInformation(auditTrail.action);
            //success
            return Ok(new
            {
                message = "Request has been successfully saved"

            });
        }




        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateAccountGroups(NlaccountGroup recvData)
        {
            //check data
            if (string.IsNullOrEmpty(recvData.GroupCode))
            {
                return BadRequest(new { message = "Missing group code is missing " });

            }

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request  
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            //  string permissionName = Request.Headers["PermName"];

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
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn1.Open();
            string updtQ = "UPDATE \"NLAccountGroup\" SET \"PriGroupCode\" = '" + recvData.PriGroupName + "',\"GroupType\" = '"
                + recvData.GroupType + "',\"GroupName\"='" + recvData.GroupName + "',\"GroupSubType\"='" + recvData.GroupSubType + "',\"DefaultGroup\"='" + recvData.DefaultGroup + "', \"ModifiedOn\" ='" + DateTime.Now + "' WHERE \"GroupCode\"= '" + recvData.GroupCode + "' ";
            bool myReq2 = myDbconnection.UpdateDelInsert(updtQ, db);
            cnn1.Close();

            if (myReq2 == false)
            {
                _log.LogError("An occured while trying to update details.");
                //failed
                return BadRequest(new { message = "An occured while trying to update details." });
            }



            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $" Updated AccountGroup  name {recvData.GroupName}  at {DateTime.Now.ToString("dd-MM-yyyy")}";
            auditTrail.module = "Account Group";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _log.LogInformation(auditTrail.action);
            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });
        }


        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult NlAccountGroupsReports()
        {
            

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request  
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //permission name
            //  string permissionName = Request.Headers["PermName"];

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

            //get database name
            string db = companyRes;
            NlService nlService = new NlService(db);
            var data = nlService.nljournalreport();


            return Ok(new
            {
                AccountGroupReport = data,

            });
        }






    }
}