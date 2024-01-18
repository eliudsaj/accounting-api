using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Purchases;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.PlService;

namespace pyme_finance_api.Controllers.Purchases
{
    [Route("api/[controller]")]
    [ApiController]
    public class PLAnalysisCodesController : ControllerBase
    {
        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;
        public PLAnalysisCodesController(IConfiguration config)
        {
            _configuration = config;
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_PlanalysisCode()
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
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            string db = companyRes;
            PlService PlService = new PlService(db);
            NlService nlService = new NlService(db);
            var pLAnalysisCodes = PlService.GetPlanalysisCodes();
            var nlAccounts = nlService.GetNlaccounts();
            return Ok(new { PLAnalysisCodes = pLAnalysisCodes, Code = 200,NlAccounts = nlAccounts });
        }
        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult AddGRNType(GoodReturnNoteType returnNote)
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
            var response = plService.CreateGRNType(returnNote);
            if (response.Httpcode.Equals(400))
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = "Good Return Note Type '"+ returnNote.GRNType +"' has Successfully Registered.", Code = 200 });
        }
        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult Add_PlanalysisCode(PLAnalysisCodes PLAnalysisCodes)
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
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            string db = companyRes;
            PlService nlService = new PlService(db);
            var response = nlService.addPlanalysis(PLAnalysisCodes);
            if (response.Httpcode.Equals(400))
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = "Request has been successfully saved", Code = 200 });
        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult Update_PlanalysisCode(PLAnalysisCodes PLAnalysisCodes)
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
            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            string db = companyRes;
            PlService nlService = new PlService(db);
            var response = nlService.updatePlanalysis(PLAnalysisCodes);
            if (response.Httpcode.Equals(400))
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = "Request has been successfully saved", Code = 200 });
        }
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult UpdateGRNType([FromBody]GoodReturnNoteType goodReturn, int key)
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
            goodReturn.GRNId = key;
            var response = plService.UpdateGRNType(goodReturn);
            if (response.Httpcode.Equals(400))
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = "Good Return Note Type "+ goodReturn.GRNType +" has Successfully Updated.", Code = 200 });
        }        
        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult DeleteGRNType(int key)
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
            var response = plService.DeleteGRNType(key);
            if (response.Httpcode.Equals(400))
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = "Good Return Note Type Successfully Deleted.", Code = 200 });
        }

    }
}
