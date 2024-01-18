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
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Service.NlServices;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlAnalysisController : ControllerBase
    {

        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;
        readonly ILogger<SlAnalysisController> _log;

        public SlAnalysisController(IConfiguration config, ILogger<SlAnalysisController> logger)
        {
            _configuration = config;
            _log = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_SlanalysisCode()
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            _log.LogInformation("Fetching Analysis Codes");


            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");

            if (tokenData != "")
            {
                //assign company Ref from header
                companyRes = tokenData;
            }
            string db = companyRes;

            NlService nlService = new NlService(db);


            var sLAnalysisCodes = nlService.GetSlanalysisCodes();
         
            return Ok(new { data = sLAnalysisCodes, Code = 200 });

        }




        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult Add_SlanalysisCode(SLAnalysisCodes sLAnalysisCodes)
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _log.LogInformation($"Creating Analysis code {sLAnalysisCodes.AnalCode}");

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            if (String.IsNullOrEmpty(sLAnalysisCodes.NLAccCode))
            {
                return BadRequest(new { message = "Account code is required." });
            }
            if (String.IsNullOrEmpty(sLAnalysisCodes.AnalType))
            {
                return BadRequest(new { message = "Analysis Type is required" });
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
            var result = nlService.addSlanalysis(sLAnalysisCodes);
            if(result.Httpcode == 400)
            {
                _log.LogError(result.Message);
                return BadRequest(new { message = result.Message });
            }

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"sl analysis code created successfully at {DateTime.Now.ToString("dd-MM-yyyy")}";
            auditTrail.module = "SL Analysis";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            _log.LogInformation($"sl analysis code created successfully  at {DateTime.Now.ToString("dd-MM-yyyy")}");
            return Ok(new { Code = 200, Message = "sl analysis code created successfully" });

        }


        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult Update_SlanalysisCode(SLAnalysisCodes sLAnalysisCodes)
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];


            if (String.IsNullOrEmpty(sLAnalysisCodes.NLAccCode))
            {
                return BadRequest(new { message = "Account code is required." });
            }
            if (String.IsNullOrEmpty(sLAnalysisCodes.AnalType))
            {
                return BadRequest(new { message = "Analysis Type is required" });
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
            var result = nlService.updateSlanalysis(sLAnalysisCodes);
            if (result.Httpcode == 400)
            {
                return BadRequest(new { message = result.Message.ToString()});
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"sl analysis code update successfully at {DateTime.Now.ToString("dd-MM-yyyy")}";
            auditTrail.module = "SL Analysis";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);




            return Ok(new {  Code = 200 ,Message = " analysis code updated successfully"});

        }









    }
}
