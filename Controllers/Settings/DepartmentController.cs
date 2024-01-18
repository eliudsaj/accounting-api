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
    public class DepartmentController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(IConfiguration config, ILogger<DepartmentController> logger)
        {
            _configuration = config;
            _logger = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult department_new_defaults()
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
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get list of users
            List<Users> usersList = new List<Users>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Users\" WHERE \"UBranch\" = " + staff_branch + "  ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                Users usr = new Users
                {
                    UFirstName = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null,
                    ULastName = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null,
                    UEmail = sdr0["UEmail"] != DBNull.Value ? (string)sdr0["UEmail"] : null,
                    UType = sdr0["UType"] != DBNull.Value ? (string)sdr0["UType"] : null,
                    UProfile = sdr0["UProfile"] != DBNull.Value ? (string)sdr0["UProfile"] : null,
                    UId = sdr0["UId"] != DBNull.Value ? (int)sdr0["UId"] : 0
                };

                usersList.Add(usr);
            }
            cnn.Close();

            //get departments data in your branch
            List<Departments> departmentsList = new List<Departments>();
            cnn.Open();
            NpgsqlDataReader sdrInv = new NpgsqlCommand("Select \"Departments\".*, \"Branches\".\"BrName\", \"Branches\".\"BrId\", \"Users\".\"UFirstName\", \"Users\".\"ULastName\", \"Users\".\"UEmail\" From \"Departments\" LEFT Join \"Branches\" On \"Branches\".\"BrId\" = \"Departments\".\"DpBranch\" LEFT Join \"Users\" On \"Users\".\"UId\" = \"Departments\".\"DpHead\" WHERE \"DpBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdrInv.Read())
            {
                Departments dp = new Departments
                {
                    DpId = sdrInv["DpId"] != DBNull.Value ? (int)sdrInv["DpId"] : 0,
                    DpBranch = sdrInv["DpBranch"] != DBNull.Value ? (int)sdrInv["DpBranch"] : 0,
                    DpName = sdrInv["DpName"] != DBNull.Value ? (string)sdrInv["DpName"] : null,
                    DpHead = sdrInv["DpHead"] != DBNull.Value ? (int)sdrInv["DpHead"] : 0,
                    DpRef = sdrInv["DpRef"] != DBNull.Value ? (string)sdrInv["DpRef"] : null,

                    BranchId = sdrInv["BrId"] != DBNull.Value ? (int)sdrInv["BrId"] : 0,
                    BranchName = sdrInv["BrName"] != DBNull.Value ? (string)sdrInv["BrName"] : null,

                    DepartmentHead_firstname = sdrInv["UFirstName"] != DBNull.Value ? (string)sdrInv["UFirstName"] : null,
                    DepartmentHead_lastname = sdrInv["ULastName"] != DBNull.Value ? (string)sdrInv["ULastName"] : null
                };

                departmentsList.Add(dp);
            }
            cnn.Close();


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




            return Ok(new
            {
                DepartmentsList = departmentsList,
                StaffList = usersList,
                BranchesData = BranchesList
            });

        }

        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult department_create_new(Departments recvData)
        {
            _logger.LogInformation($"Creating deparment {recvData.DpName}");
            //check data
            if (string.IsNullOrEmpty(recvData.DpName))
            {
                return BadRequest(new { message = "Missing department name " });

            }
            else if (recvData.DpHead == 0)
            {
                return BadRequest(new { message = "Missing department head" });

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

            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occured while trying to get your branch details. Request failed" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            _logger.LogInformation($"Cheking if deparment {recvData.DpName} under a branch");

            //check if department already exists in same branch
            cnn.Open();
            NpgsqlDataReader sdr_dp = new NpgsqlCommand("Select * From \"Departments\" WHERE \"DpBranch\" = " + user_branch + " AND \"DpName\" = '" + recvData.DpName + "'  ", cnn).ExecuteReader();
            if (sdr_dp.HasRows == true)
            {
                cnn.Close();
                return BadRequest(new { message = "The department " + recvData.DpName + " already exists in your branch." });
            }
            cnn.Close();

            //get last id
            int last_id = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"DpId\") as sl From \"Departments\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_id = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //get sth_ref guid
            string my_unique_Reference = System.Guid.NewGuid().ToString("D");

            //set header details
            _logger.LogInformation($"Saving deparment {recvData.DpName} details");
            cnn.Open();
            string insertQ = "INSERT INTO \"Departments\" (\"DpId\", \"DpBranch\", \"DpName\", \"DpHead\", \"DpRef\" ) VALUES(" + (last_id + 1) + ", " + user_branch + ", '" + recvData.DpName + "', " + recvData.DpHead + ",'" + my_unique_Reference + "');";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                _logger.LogError($"An error in saving {recvData.DpName} details");
                return BadRequest(new { message = "An occurred while trying to save details." });
            }

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Created Department " + recvData.DpName + "  at " + DateTime.Now.ToString("dd/MM/yyyy") ;
            auditTrail.module = "Department";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _logger.LogInformation(auditTrail.action);
            //success
            return Ok(new
            {
                message = "Request has been successfully saved"

            });


        }







        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult update_department(Departments recvData)
        {
            _logger.LogInformation($"updating department {recvData.DpName} ");
            //check data
            if (string.IsNullOrEmpty(recvData.DpName))
            {
                return BadRequest(new { message = "Missing department name " });

            }
            else if (recvData.DpHead == 0)
            {
                return BadRequest(new { message = "Missing department head" });

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

            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occured while trying to get your branch details. Request failed" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            ////check if department already exists in same branch
            //cnn.Open();
            //NpgsqlDataReader sdr_dp = new NpgsqlCommand("Select * From \"Departments\" WHERE \"DpBranch\" = " + user_branch + " AND \"DpName\" = '" + recvData.DpName + "'  ", cnn).ExecuteReader();
            //if (sdr_dp.HasRows == true)
            //{
            //    cnn.Close();
            //    return BadRequest(new { message = "The department " + recvData.DpName + " already exists in your branch." });
            //}
            //cnn.Close();

            //get last id
            //int last_id = 0;
            //cnn.Open();
            //NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"DpId\") as sl From \"Departments\" LIMIT 1 ", cnn).ExecuteReader();
            //while (sdra.Read())
            //{
            //    last_id = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            //}
            //cnn.Close();

            //get sth_ref guid
            //string my_unique_Reference = System.Guid.NewGuid().ToString("D");

            //set header details
            cnn.Open();
            //string insertQ = "INSERT INTO \"Departments\" (\"DpId\", \"DpBranch\", \"DpName\", \"DpHead\", \"DpRef\" ) VALUES(" + (last_id + 1) + ", " + user_branch + ", '" + recvData.DpName + "', " + recvData.DpHead + ",'" + my_unique_Reference + "');";
            string upd_q = "UPDATE \"Departments\" SET \"DpName\" = '" + recvData.DpName + "',\"DpHead\" = '"+recvData.DpHead+ "',\"DpBranch\" = '"+recvData.BranchId+"'  WHERE \"DpId\" = " + recvData.DpId+ " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(upd_q, db);
            cnn.Close();
            if (myReq2 == false)
            {
                _logger.LogError($"An occurred while trying to save details of department ${recvData.DpName}");
                //failed
                return BadRequest(new { message = "An occurred while trying to save details." });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Updated Department " + recvData.DpName + "  at " + DateTime.Now.ToString("dd/MM/yyyy");
            auditTrail.module = "Department";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _logger.LogInformation(auditTrail.action);
            //success
            return Ok(new
            {
                message = "Request has been successfully saved"

            });


        }
















    }
}
