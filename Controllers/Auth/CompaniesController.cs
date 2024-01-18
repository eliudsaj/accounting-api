using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Org.BouncyCastle.Asn1.Ocsp;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;

namespace pyme_finance_api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {

        private IWebHostEnvironment _hostingEnvironment;
        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;

        public CompaniesController(IWebHostEnvironment environment, IConfiguration config)
        {
            _hostingEnvironment = environment;
            _configuration = config;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult allcompanies()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //instantiate jwt
            jwt_token jwt = new jwt_token(_configuration);

            //get token data
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

            string primaryDbConn = _configuration.GetConnectionString("LocalDatabase");
            NpgsqlConnection cnnPrimary = new NpgsqlConnection(primaryDbConn);
            NpgsqlConnection cnnCurr = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get user details from current database
            Users usrcurrent = new Users();
            cnnCurr.Open();
            NpgsqlDataReader sdrCurr = new NpgsqlCommand("SELECT * FROM \"Users\" WHERE \"UId\" = " + userId + "  ", cnnCurr).ExecuteReader();
            if (sdrCurr.HasRows == false)
            {
                cnnCurr.Close();
                return BadRequest(new { message = "Sorry, user account was NOT found in your dir" });
            }
            if (sdrCurr.Read())
            {
                usrcurrent.UFirstName = sdrCurr["UFirstName"] != DBNull.Value ? (string)sdrCurr["UFirstName"] : null;
                usrcurrent.ULastName = sdrCurr["ULastName"] != DBNull.Value ? (string)sdrCurr["ULastName"] : null;
                usrcurrent.UEmail = sdrCurr["UEmail"] != DBNull.Value ? (string)sdrCurr["UEmail"] : null;
            }
            cnnCurr.Close();

            //check primary database if user is administrator
            Users usrprimary = new Users();
            cnnPrimary.Open();
            NpgsqlDataReader sdrpr = new NpgsqlCommand("SELECT * FROM \"Users\" WHERE \"UEmail\" = '" + usrcurrent.UEmail + "'  ", cnnPrimary).ExecuteReader();
            if (sdrpr.HasRows == false)
            {
                cnnPrimary.Close();
                return BadRequest(new { message = "Sorry, user account was NOT found in the primary database" });
            }
            if (sdrpr.Read())
            {
                usrprimary.UFirstName = sdrpr["UFirstName"] != DBNull.Value ? (string)sdrpr["UFirstName"] : null;
                usrprimary.ULastName = sdrpr["ULastName"] != DBNull.Value ? (string)sdrpr["ULastName"] : null;
                usrprimary.UEmail = sdrpr["UEmail"] != DBNull.Value ? (string)sdrpr["UEmail"] : null;
                usrprimary.UType = sdrpr["UType"] != DBNull.Value ? (string)sdrpr["UType"] : null;
            }
            cnnPrimary.Close();

            //check if user is administrtor from primary db
            if (usrprimary.UType != "Administrator")
            {
                return BadRequest(new { message = "Sorry, you are required to be an Administrator at NGENX PYME FINANCE to access this information." });
            }



            //get all Invoices

            List<Company> companieslist = new List<Company>();
            cnnPrimary.Open();
            NpgsqlDataReader sdrCP = new NpgsqlCommand("SELECT * FROM \"Company\" ", cnnPrimary).ExecuteReader();
            while (sdrCP.Read())
            {
                Company cpny = new Company();

                cpny.CpName = sdrCP["CpName"] != DBNull.Value ? (string)sdrCP["CpName"] : null;
                cpny.CpAddress = sdrCP["CpAddress"] != DBNull.Value ? (string)sdrCP["CpAddress"] : null;
                cpny.CpAdminEmail = sdrCP["CpAdminEmail"] != DBNull.Value ? (string)sdrCP["CpAdminEmail"] : null;
                cpny.CpRegisterDate = sdrCP["CpRegisterDate"] != DBNull.Value ? (DateTime)sdrCP["CpRegisterDate"] : DateTime.Now;
                cpny.CpRef = sdrCP["CpRef"] != DBNull.Value ? (string)sdrCP["CpRef"] : null;
                cpny.CpConnString = sdrCP["CpConnString"] != DBNull.Value ? (string)sdrCP["CpConnString"] : null;
                cpny.CpStatus = sdrCP["CpStatus"] != DBNull.Value ? (string)sdrCP["CpStatus"] : null;
                cpny.CpAdminContact = sdrCP["CpAdminContact"] != DBNull.Value ? (string)sdrCP["CpAdminContact"] : null;
                cpny.CpAdminFirstname = sdrCP["CpAdminFirstname"] != DBNull.Value ? (string)sdrCP["CpAdminFirstname"] : null;
                cpny.CpAdminLastname = sdrCP["CpAdminLastname"] != DBNull.Value ? (string)sdrCP["CpAdminLastname"] : null;
                cpny.CpLicenseType = sdrCP["CpLicenseType"] != DBNull.Value ? (string)sdrCP["CpLicenseType"] : null;
                cpny.CpExpireDate = sdrCP["CpExpireDate"] != DBNull.Value ? (DateTime)sdrCP["CpExpireDate"] : DateTime.Now;
                cpny.CpLicense = sdrCP["CpLicense"] != DBNull.Value ? (string)sdrCP["CpLicense"] : null;
                cpny.CpId = sdrCP["CpId"] != DBNull.Value ? (int)sdrCP["CpId"] : 0;
                cpny.KRAPin = sdrCP["KRAPin"] != DBNull.Value ? (string)sdrCP["KRAPin"] : null;

                companieslist.Add(cpny);
            }
            cnnPrimary.Close();


            return Ok(new { CompanyDetails = companieslist });

        }


    }

}

