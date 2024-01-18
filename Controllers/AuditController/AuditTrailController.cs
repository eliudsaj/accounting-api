using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Service.AuditTrailService;

namespace pyme_finance_api.Controllers.AuditController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditTrailController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();

        public AuditTrailController(IConfiguration config)
        {
            _configuration = config;

        }


        [Route("ListAuditTrail")]
        [HttpGet]
        [Authorize]
        public ActionResult ListAuditTrail()
        {

            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
          var  userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            AuditTrailService auditTrailService = new AuditTrailService(tokenData);


            return Ok(new { AuditTrailData = auditTrailService.GetAllAuditTrails() });
        }


        [Route("UserAuditTrail")]
        [HttpGet]
        [Authorize]
        public ActionResult ListAuditTrail(int id)
        {

            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            var userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);

            return Ok(new { AuditTrailData = auditTrailService.GetUserAuditTrails(id) });


        }




    }
}
