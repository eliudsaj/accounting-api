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
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Service.MeasureofUnitService;

namespace pyme_finance_api.Controllers.UnitofMeasure
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitofMeasureController : ControllerBase
    {

        private IConfiguration _configuration;
        private readonly ILogger<UnitofMeasureController> _logger;


        public UnitofMeasureController(IConfiguration config, ILogger<UnitofMeasureController> logger)
        {
            _configuration = config;
            _logger = logger;

        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult ListCurrencies()
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

            _logger.LogInformation($"Fetching list of unit of measure");

            string db = companyRes;
            MeasureofUnitService measureofUnitService = new MeasureofUnitService(db);



            return Ok(measureofUnitService.listofUnitofMeasure());

        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddUnitofMeasureAsync(Models.UnitofMeasure.UnitofMeasure unitofMeasure)
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
            unitofMeasure.CreatedBy = userId;

            _logger.LogInformation($"Creating   unit of measure  { unitofMeasure.Name}");
            string db = tokenData;
            MeasureofUnitService  measureofUnitService = new MeasureofUnitService(db);
          var response =   measureofUnitService.saveUnitofMeasure(unitofMeasure);


            if(response.Httpcode == 400)
            {
                _logger.LogError($"An error {response.Message} occured");
                return BadRequest(new { message = response.Message });
            }

               unitofMeasure.Id =int.Parse(response.Message);

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"Created new unit of measurement { unitofMeasure.Name} ";
            auditTrail.module = "Unit of Measure";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            return Ok(new { data = unitofMeasure });



        }


        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult UpdateUnitofMeasure(Models.UnitofMeasure.UnitofMeasure unitofMeasure)
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
            unitofMeasure.CreatedBy = userId;

            string db = companyRes;
            MeasureofUnitService measureofUnitService = new MeasureofUnitService(db);
          var response =   measureofUnitService.updateUnitofMeasure(unitofMeasure);
            if (response.Httpcode == 400)
            {
                _logger.LogInformation($"Error  { response.Message} occured while updating unit of measure { unitofMeasure.Name }");
                return BadRequest(new { message = response.Message });
            }
            _logger.LogInformation($"Updating  unit of measure  { unitofMeasure.Name} is successful");
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"Updated new unit of measurement { unitofMeasure.Name} ";
            auditTrail.module = "Unit of Measure";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            return Ok(new { message = response.Message });

        }
    }
}
