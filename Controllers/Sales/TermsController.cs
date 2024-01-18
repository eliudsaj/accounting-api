using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Sales.Terms;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();

        public TermsController(IConfiguration config)
        {
            _configuration = config;

        }

        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult UpdateInvTerms(Allterms termsData)
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

            string htmlEncoded = WebUtility.HtmlEncode(termsData.terms);
            string doctype = termsData.tosType;

            // Encode the content for storing in Sql server.
            //string htmlEncoded = WebUtility.HtmlEncode(text);

            // Decode the content for showing on Web page.
            //string original = WebUtility.HtmlDecode(htmlEncoded);

            //update query
            string myQuery = "UPDATE \"AllSystemTerms\" SET \"terms\" = '" + htmlEncoded + "' WHERE \"tosType\" = '"+doctype+"'  ";
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


    }
}
