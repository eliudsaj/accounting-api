using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Sales;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();

        public DiscountController(IConfiguration config)
        {
            _configuration = config;
        }

        private static bool IsNumeric(decimal mynumber)
        {

            var Result = decimal.TryParse("123", out mynumber);
            return Result;
        }



        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult CreateDiscount(Discounts discData)
        {
            if (string.IsNullOrEmpty(discData.DRef))
            {
                return BadRequest(new { message = "Missing discount reference name" });
            }

            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");

            if (!regexItem.IsMatch(discData.DRef))
            {
                return BadRequest(new { message = "Please ensure you have no special characters in your discount name reference " });
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

            //check last added number
            int lastDiscNumber = 0;

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("SELECT MAX(\"DId\") as sn From \"Discounts\" LIMIT 1 ", cnn).ExecuteReader();

            while (sdr.Read())
            {

                lastDiscNumber = sdr["sn"] != DBNull.Value ? (int)sdr["sn"] : 0;

            }

            cnn.Close();


            //insert  
            string myQuery = "INSERT INTO \"Discounts\" (\"DId\",\"DRef\",\"DPerc\",\"DSetDate\",\"DEndDate\") vALUES (" + (lastDiscNumber + 1) + ",'" + discData.DRef + "', " + discData.DPerc + ", '" + DateTime.Now + "','" + discData.DEndDate + "' ) ";
            cnn.Open();
            bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
            cnn.Close();
            if (reqStatus == true)
            {
                return Ok(new { message = "Discount has been successfully created" });
            }
            else
            {
                return BadRequest(new { message = "An error occurred while trying to create new discount item" });
            }




        }

        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult UpdateDiscount(int key, [FromBody] Discounts discData)
        {

            if (string.IsNullOrEmpty(discData.DRef))
            {
                return BadRequest(new { message = "Missing required discount reference name" });
            }
            else if (string.IsNullOrEmpty(discData.DId.ToString()))
            {
                return BadRequest(new { message = "Missing required discount Ref id" });
            }
            else if (string.IsNullOrEmpty(discData.DPerc.ToString()))
            {
                return BadRequest(new { message = "Missing required discount percentage" });
            }
            else if (string.IsNullOrEmpty(discData.DEndDate.ToString()))
            {
                return BadRequest(new { message = "Missing required Expirely date" });
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



            //get database name
            string db = companyRes;

            //update query

            string myQuery = "UPDATE \"Discounts\" SET \"DRef\" = '" + discData.DRef + "', \"DPerc\" = " + discData.DPerc + ", \"DEndDate\" = '" + discData.DEndDate + "' WHERE \"DId\" = " + key + "  ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
            cnn.Close();
            if (reqStatus == true)
            {
                return Ok(new { message = "Discount data has been successfully modified" });
            }
            else
            {
                return BadRequest(new { message = "An error occurred while trying to modify discount item" });
            }



        }


        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult DeleteDiscount(int key)
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

            string myQuery = "DELETE FROM \"Discounts\" WHERE \"DId\" = " + key + " ";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            bool reqStatus = myDbconnection.UpdateDelInsert(myQuery, db);
            cnn.Close();
            if (reqStatus == true)
            {
                return Ok(new { message = "Discount data has been successfully deleted" });
            }
            else
            {
                return BadRequest(new { message = "An error occurred while trying to modify discount item" });
            }


        }

    }
}
