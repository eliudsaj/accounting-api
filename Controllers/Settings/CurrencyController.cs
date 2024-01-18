using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Service.CurrencyService;

namespace pyme_finance_api.Controllers.Settings
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();

        public CurrencyController(IConfiguration config)
        {
            _configuration = config;

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
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

           
            //get database name
      

            string db = companyRes;

            CurrencyService currencyService = new CurrencyService(db);

            var data = currencyService.GetCurrency();

            var count = data.Count();
            return Ok(new { CurrencyCount = count, CurrencyData = data });


        }


        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult CreateCurrency(Currencies currencyData)
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

            CurrencyService currencyService = new CurrencyService(db);

            var data = currencyService.AddCurrency(currencyData);


            return Ok(new
            {
                message = "Currency has been added successfully ."
            });



        }




        // PUT: api/Products/5
        [Route("UpdateCurr")]
        [HttpPut]
        [Authorize]
        public IActionResult UpdateCurr(Currencies currencyData)
        {
            //check req desc
            string reqDescription = Request.Headers["requestDesc"];
            if (string.IsNullOrEmpty(reqDescription))
            {
                return BadRequest(new { message = "Undefined request process mode." });
            }
            else
            {
                //check id
                if (string.IsNullOrEmpty(currencyData.CrId.ToString()) || currencyData.CrId == 0)
                {
                    return BadRequest(new { message = "Request Error! Missing currency reference." });
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

                //create connection
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //check if currency ID exists
                string check_crid = "SELECT * FROM \"Currencies\" WHERE \"CrId\" = " + currencyData.CrId + " ";
                int check_crid_count = myDbconnection.CheckRowExists(check_crid, db);
                if (check_crid_count == 0)
                {
                    return BadRequest(new { message = "request failed. No matching reference was found" });
                }

                //get currencies data
                Currencies storageData = new Currencies();
                cnn.Open();
                NpgsqlDataReader sdr_b = new NpgsqlCommand("SELECT * FROM \"Currencies\" WHERE \"CrId\" = " + currencyData.CrId + " ", cnn).ExecuteReader();
                while (sdr_b.Read())
                {
                    storageData.CrId = sdr_b["CrId"] != DBNull.Value ? (int)sdr_b["CrId"] : 0;
                    storageData.CrName = sdr_b["CrName"] != DBNull.Value ? (string)sdr_b["CrName"] : null;
                    storageData.CrCode = sdr_b["CrCode"] != DBNull.Value ? (string)sdr_b["CrCode"] : null;
                    storageData.CrCountry = sdr_b["CrCountry"] != DBNull.Value ? (string)sdr_b["CrCountry"] : null;
                    storageData.CrStatus = sdr_b["CrStatus"] != DBNull.Value ? (string)sdr_b["CrStatus"] : null;
                    storageData.CrCreatedDate = sdr_b["CrCreatedDate"] != DBNull.Value ? (DateTime)sdr_b["CrCreatedDate"] : DateTime.Now;
                    storageData.CrModifiedDate = sdr_b["CrModifiedDate"] != DBNull.Value ? (DateTime)sdr_b["CrModifiedDate"] : DateTime.Now;
                }
                cnn.Close();

                //suspend or Activate
                if (reqDescription == "activate_deactivate")
                {
                    var currState = "";

                    if (storageData.CrStatus == "Active")
                    {
                        currState = "Inactive";
                    }
                    else
                    {
                        currState = "Active";
                    }

                    //update query
                    string upd_q = "UPDATE \"Currencies\" SET \"CrStatus\" = '" + currState + "' WHERE \"CrId\" = " + currencyData.CrId + " ";
                    bool upd_q_res = myDbconnection.UpdateDelInsert(upd_q, db);
                    if (upd_q_res == false)
                    {
                        return BadRequest(new { message = "Sorry! No changes were made trying to Activate/Deactivate" });
                    }

                    storageData.CrStatus = currState;
                    
                    return Ok(new
                    {
                        message = "Request has been successfully processed"
                       
                    });
                }

                else if (reqDescription == "editCurrency")
                {
                    storageData.CrName = currencyData.CrName;
                    storageData.CrCode = currencyData.CrCode;
                    storageData.CrCountry = currencyData.CrCountry;

                    //update Data
                    string upd_q = "UPDATE \"Currencies\" SET \"CrName\" = '" + storageData.CrName + "', \"CrCode\" = '" + storageData.CrCode + "',\"CrCountry\" = '" + storageData.CrCountry + "',\"CrStatus\" = '" + storageData.CrStatus + "', \"CrModifiedDate\" = '" + DateTime.Today + "' WHERE \"CrId\" = " + currencyData.CrId + " ";
                    bool upd_q_res = myDbconnection.UpdateDelInsert(upd_q, db);
                    if (upd_q_res == false)
                    {
                        return BadRequest(new { message = "Sorry! No changes were made trying to modify data" });
                    }

                    return Ok(new
                    {
                        message = "Your data has been successfully updated"
                    });

                }

                else if (reqDescription == "deleteCurrency")
                {
                    //Delete Data
                    string del_q = "DELETE * FROM \"Currencies\" WHERE \"CrId\" = " + currencyData.CrId + " ";
                    bool del_status = myDbconnection.UpdateDelInsert(del_q, db);
                    if (del_status == false)
                    {
                        return BadRequest(new { message = "Sorry! No changes were made trying to delete data" });
                    }
                   
                    return Ok(new
                    {
                        message = "Currency has been successfully deleted."
                    });



                }
                else
                {
                    return BadRequest(new { message = "Undefined procedure. Request Failed. Contact support for more information" });
                }

            }


        }

    }
}
