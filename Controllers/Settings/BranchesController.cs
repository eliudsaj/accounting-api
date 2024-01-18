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
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Settings;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;

namespace pyme_finance_api.Controllers.Settings
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private readonly ILogger<BranchesController> _logger;

        public BranchesController(IConfiguration config, ILogger<BranchesController> logger)
        {
            _configuration = config;
            _logger = logger;

        }

        [Route("ListBranches")]
        [HttpGet]
        [Authorize]
        public ActionResult ListBranches()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _logger.LogInformation("fetching company branches");

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

            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

           
            //get database name
            string db = companyRes;

            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //Get all Branches
            List<Branches> BranchesList = new List<Branches>();
            cnn.Open();
            NpgsqlDataReader sdr_br = new NpgsqlCommand("Select \"Branches\".*, \"Users\".\"UFirstName\", \"Users\".\"ULastName\" From \"Branches\" Left Join \"Users\" On \"Users\".\"UId\" = \"Branches\".\"ContactStaff\"  ", cnn).ExecuteReader();
            while (sdr_br.Read())
            {
                Branches br = new Branches();

                br.BrId = sdr_br["BrId"] != DBNull.Value ? (int)sdr_br["BrId"] : 0;
                br.BrName = sdr_br["BrName"] != DBNull.Value ? (string)sdr_br["BrName"] : null;
                br.BrLocation = sdr_br["BrLocation"] != DBNull.Value ? (string)sdr_br["BrLocation"] : null;
                br.BrCode = sdr_br["BrCode"] != DBNull.Value ? (string)sdr_br["BrCode"] : null;
                br.ContactStaff = sdr_br["ContactStaff"] != DBNull.Value ? (int)sdr_br["ContactStaff"] : 0;
                br.BrActive = sdr_br["BrActive"] != DBNull.Value ? (bool)sdr_br["BrActive"] : false;

                br.UFirstName = sdr_br["UFirstName"] != DBNull.Value ? (string)sdr_br["UFirstName"] : null;
                br.ULastName = sdr_br["ULastName"] != DBNull.Value ? (string)sdr_br["ULastName"] : null;

                BranchesList.Add(br);
            }
            cnn.Close();


            //get active staff users
            List<Users> activeStaff = new List<Users>();
            cnn.Open();
            NpgsqlDataReader sdr_st = new NpgsqlCommand("Select * From \"Users\" ", cnn).ExecuteReader();
            while (sdr_st.Read())
            {
                Users usr = new Users();

                usr.UFirstName = sdr_st["UFirstName"] != DBNull.Value ? (string)sdr_st["UFirstName"] : null;
                usr.ULastName = sdr_st["ULastName"] != DBNull.Value ? (string)sdr_st["ULastName"] : null;
                usr.UEmail = sdr_st["UEmail"] != DBNull.Value ? (string)sdr_st["UEmail"] : null;
                usr.UId = sdr_st["UId"] != DBNull.Value ? (int)sdr_st["UId"] : 0;

                activeStaff.Add(usr);
            }
            cnn.Close();

            
            return Ok(new { BranchesData = BranchesList, StaffData = activeStaff });


        }

        [Route("CreateBranch")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateBranch(Branches branchData)
        {
            //List<Users> clUsers;
            if (string.IsNullOrEmpty(branchData.BrName))
            {
                return BadRequest(new { message = "Missing branch name" });
            }
            else if (string.IsNullOrEmpty(branchData.BrCode))
            {
                return BadRequest(new { message = "Missing branch code" });
            }
            else if (string.IsNullOrEmpty(branchData.BrLocation))
            {
                return BadRequest(new { message = "Missing branch location" });
            }
            else if (string.IsNullOrEmpty(branchData.ContactStaff.ToString()))
            {
                return BadRequest(new { message = "Missing branch code" });
            }


            //set Date
            DateTime today = DateTime.Today;

            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);

            //set Defaults
            // branchData.VtModifyDate = DateTime.Now;
            //vatData.VtSetDate = DateTime.Now;
            branchData.BrId = uniqueId;
            branchData.BrActive = true;


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
            _logger.LogInformation($"Creating branch {branchData.BrName}");

            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            _logger.LogInformation($"Checking if branch {branchData.BrName} exists");
            //check branch name and code
            string br_check = "SELECT * FROM \"Branches\" WHERE \"BrName\" = '" + branchData.BrName +
                              "' OR \"BrCode\" = '" + branchData.BrCode + "' ";
            int br_check_res = myDbconnection.CheckRowExists(br_check, db);
            if (br_check_res > 0)
            {
                return BadRequest(new { message = "The branch name or branch code is already registered" });
            }
            _logger.LogInformation($"Saving branch {branchData.BrName} details");

            //create new branch
            string br_insrt = "INSERT INTO \"Branches\" (\"BrId\",\"BrName\",\"BrLocation\",\"BrCode\",\"ContactStaff\",\"BrActive\") VALUES ("+branchData.BrId+", '"+branchData.BrName+"', '"+branchData.BrLocation+"','"+branchData.BrCode+"',"+userId+", 't') ";
            bool br_insrt_res = myDbconnection.UpdateDelInsert(br_insrt, db);
            if (br_insrt_res == false)
            {
                return BadRequest(new { message = "An error occurred trying to create a new branch. Please contact support for more details." });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"Created branch {branchData.BrName} at {DateTime.Now.ToString("dd-MM-yyyy")} ";
            auditTrail.module = "Branch";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _logger.LogInformation($"Branch {branchData.BrName}  has been saved successfully");

            return Ok(new
            {
                message = "Branch " + branchData.BrName + "(" + branchData.BrCode + ")" + " has been successfully created"
            });


        }

        [Route("UpdateBranch")]
        [HttpPut]
        [Authorize]
        public IActionResult UpdateBranch(Branches branchData)
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
                if (string.IsNullOrEmpty(branchData.BrId.ToString()) || branchData.BrId == 0)
                {
                    return BadRequest(new { message = "Request Error! Missing branch reference." });
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


                //get database name
                string db = companyRes;

                //create connection
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //check if branch ID exists
                string check_brid = "SELECT * FROM \"Branches\" WHERE \"BrId\" = " + branchData.BrId + " ";
                int check_brid_count = myDbconnection.CheckRowExists(check_brid, db);
                if (check_brid_count == 0)
                {
                    return BadRequest(new { message = "request failed. No matching reference was found" });
                }

                //get branches data
                Branches storageData = new Branches();
                cnn.Open();
                NpgsqlDataReader sdr_b = new NpgsqlCommand("SELECT * FROM \"Branches\" WHERE \"BrId\" = " + branchData.BrId + " ", cnn).ExecuteReader();
                while (sdr_b.Read())
                {
                    storageData.BrId = sdr_b["BrId"] != DBNull.Value ? (int)sdr_b["BrId"] : 0;
                    storageData.BrName = sdr_b["BrName"] != DBNull.Value ? (string)sdr_b["BrName"] : null;
                    storageData.BrLocation = sdr_b["BrLocation"] != DBNull.Value ? (string)sdr_b["BrLocation"] : null;
                    storageData.BrCode = sdr_b["BrCode"] != DBNull.Value ? (string)sdr_b["BrCode"] : null;
                    storageData.ContactStaff = sdr_b["ContactStaff"] != DBNull.Value ? (int)sdr_b["ContactStaff"] : 0;
                    storageData.BrActive = sdr_b["BrActive"] != DBNull.Value ? (bool)sdr_b["BrActive"] : false;
                   
                }
                cnn.Close();

                //suspend or Activate
                if (reqDescription == "activate_deactivate")
                {

                    //check permission
                    bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                    if (perStatus == false)
                    {
                        return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                    }

                    var currState = false;

                    if (storageData.BrActive == true)
                    {
                        currState = false;
                    }
                    else
                    {
                        currState = true;
                    }

                    storageData.BrActive = currState;
                    //update query
                    string upd_q = "UPDATE \"Branches\" SET \"BrActive\" = '"+currState+ "' WHERE \"BrId\" = " + branchData.BrId + " ";
                    bool upd_q_res = myDbconnection.UpdateDelInsert(upd_q, db);
                    if (upd_q_res == false)
                    {
                        return BadRequest(new { message = "Sorry! No changes were made trying to Activate/Deactivate" });
                    }
                    AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                    AuditTrail auditTrail = new AuditTrail();
                    auditTrail.action = $" branch {branchData.BrName} status has been chaged to {currState} at {DateTime.Now.ToString("dd-MM-yyyy")} ";
                    auditTrail.module = "Branch";
                    auditTrail.userId = userId;
                    auditTrailService.createAuditTrail(auditTrail);

                    _logger.LogInformation(auditTrail.action);


                    return Ok(new
                    {
                        message = "Request has been successfully processed"
                    });
                }

                else if (reqDescription == "updatebranch")
                {

                    //check permission
                    bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
                    if (perStatus == false)
                    {
                        return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
                    }

                    storageData.BrName = branchData.BrName;
                    storageData.BrCode = branchData.BrCode;
                    storageData.BrLocation = branchData.BrLocation;
                    storageData.ContactStaff = branchData.ContactStaff;

                    //update data
                    string upd_q = "UPDATE \"Branches\" SET \"BrName\" = '" + storageData.BrName + "', \"BrLocation\" = '" + storageData.BrLocation + "',\"BrCode\" = '" + storageData.BrCode + "',\"ContactStaff\" = '" + storageData.ContactStaff + "' WHERE \"BrId\" = " + branchData.BrId + " ";
                    bool upd_q_res = myDbconnection.UpdateDelInsert(upd_q, db);
                    if (upd_q_res == false)
                    {
                        return BadRequest(new { message = "Sorry! No changes were made trying to modify branch data" });
                    }


                    AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                    AuditTrail auditTrail = new AuditTrail();
                    auditTrail.action = $"Updated branch {branchData.BrName} at {DateTime.Now.ToString("dd-MM-yyyy")} ";
                    auditTrail.module = "Branch";
                    auditTrail.userId = userId;
                    auditTrailService.createAuditTrail(auditTrail);

                    _logger.LogInformation($"Branch {branchData.BrName}  has been updated successfully");

                    return Ok(new
                    {
                        message = "Your data has been successfully updated"
                        
                    });

                }

                else
                {
                    _logger.LogError("Undefined procedure. Request Failed. Contact support for more information");
                    return BadRequest(new { message = "Undefined procedure. Request Failed. Contact support for more information" });
                }

            }



        }









        [Route("ActivateBranch")]
        [HttpPut]
        [Authorize]
        public IActionResult ActivateBranch(int branchId)
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

           

                //get database name
                string db = companyRes;

                //create connection
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

                //check if branch ID exists
                string check_brid = "SELECT * FROM \"Branches\" WHERE \"BrId\" = " + branchId + " ";
                int check_brid_count = myDbconnection.CheckRowExists(check_brid, db);
                if (check_brid_count == 0)
                {
                    return Ok(new { message = "request failed. No matching reference was found",status = 400 });
                }
            //get branches data
                    Branches storageData = new Branches();
                    cnn.Open();
                    NpgsqlDataReader sdr_b = new NpgsqlCommand("SELECT * FROM \"Branches\" WHERE \"BrId\" = " + branchId + " ", cnn).ExecuteReader();
                    while (sdr_b.Read())
                    {
                        storageData.BrId = sdr_b["BrId"] != DBNull.Value ? (int)sdr_b["BrId"] : 0;
                        storageData.BrName = sdr_b["BrName"] != DBNull.Value ? (string)sdr_b["BrName"] : null;
                        storageData.BrLocation = sdr_b["BrLocation"] != DBNull.Value ? (string)sdr_b["BrLocation"] : null;
                        storageData.BrCode = sdr_b["BrCode"] != DBNull.Value ? (string)sdr_b["BrCode"] : null;
                        storageData.ContactStaff = sdr_b["ContactStaff"] != DBNull.Value ? (int)sdr_b["ContactStaff"] : 0;
                        storageData.BrActive = sdr_b["BrActive"] != DBNull.Value ? (bool)sdr_b["BrActive"] : false;

                    }
                    cnn.Close();

                    string upd_qr = "UPDATE \"Branches\" SET \"BrActive\" = " + true + " WHERE \"BrId\" = '" + branchId + "' ";

                    // update warehouse
                    bool myReq2 = myDbconnection.UpdateDelInsert(upd_qr, companyRes);
                    if (myReq2 == false)
                    {
                        //failed
                        return Ok(new { message = "An occurred while trying to process request." ,status = 400});
                    }




                     AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                    AuditTrail auditTrail = new AuditTrail();
                    auditTrail.action = $"Activate branch {storageData.BrName} at {DateTime.Now.ToString("dd-MM-yyyy")} ";
                    auditTrail.module = "Branch";
                    auditTrail.userId = userId;
                    auditTrailService.createAuditTrail(auditTrail);

                    _logger.LogInformation($"Branch {storageData.BrName}  has been activated successfully");

                    return Ok(new
                    {
                        message = "Your branch has been successfully activated",
                        status = 200

                    });





         }
        

             
            



        }

    

}
