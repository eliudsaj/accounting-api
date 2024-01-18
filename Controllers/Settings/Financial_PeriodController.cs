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
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;

namespace pyme_finance_api.Controllers.Settings
{
    [Route("api/[controller]")]
    [ApiController]
    public class Financial_PeriodController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private readonly ILogger<Financial_PeriodController> _logger;

        public Financial_PeriodController(IConfiguration config, ILogger<Financial_PeriodController>logger)
        {
            _configuration = config;
            _logger = logger;
        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult financialperiod_addNew(FinancialPeriod recvData)
        {
            _logger.LogInformation($"Adding new finacial period  {recvData.fp_name}");

            if (string.IsNullOrEmpty(recvData.fp_name))
            {
                return BadRequest(new { message = "Missing financial period name" });
            }
            else if (string.IsNullOrEmpty(recvData.fp_date_mode))
            {
                return BadRequest(new { message = "Missing financial date mode" });
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


            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occurred while trying to get your branch details. Request failed" });
            }


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            _logger.LogInformation($"Checking if  {recvData.fp_name} exists");

            //check if fp name exists
            cnn.Open();
            NpgsqlDataReader sdr_dp = new NpgsqlCommand("Select * From financial_periods WHERE fp_name= '" + recvData.fp_name + "' ", cnn).ExecuteReader();
            if (sdr_dp.HasRows == true)
            {
                cnn.Close();
                return BadRequest(new { message = "The financial period `" + recvData.fp_name.ToUpper() + "` is already registered." });
            }
            cnn.Close();

            //get last financial period
            int last_fp_ID = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(fp_id) as sl From financial_periods LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_fp_ID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //get guid
            string uniqRef = System.Guid.NewGuid().ToString("D");

            // create category
            _logger.LogInformation($"Saving  {recvData.fp_name}  details");
            cnn.Open();
            string insertQ = "INSERT INTO financial_periods (fp_id,fp_ref,fp_name,fp_trans_date,fp_openingdate,fp_closingdate,fp_active,fp_createdby,fp_closedby,fp_authorisedby,fp_createdon,fp_branch,fp_date_mode ) VALUES(" + (last_fp_ID + 1) + ", '" + uniqRef + "', '" + recvData.fp_name + "', '" + recvData.fp_trans_date + "' ,'" + recvData.fp_openingdate + "','" + recvData.fp_closingdate + "','f'," + userId + "," + 0 + "," + 0 + ",'" + DateTime.Today + "'," + user_branch + ",'" + recvData.fp_date_mode + "' ); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();

            if (myReq2 == false)
            {
                _logger.LogError("An occurred while trying to process request.");
                //failed
                return BadRequest(new { message = "An occurred while trying to process request." });
            }

            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Created Finacial Period " + recvData.fp_name + "  at " + DateTime.Now.ToString("dd/MM/yyyy");
            auditTrail.module = "Finacial Period";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            _logger.LogInformation(auditTrail.action);

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult financialperiod_list_All()
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            _logger.LogInformation($"Fetching financial period");

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
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occured while trying to get your branch details. Request failed" });
            }


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get financial periods
            List<FinancialPeriod> fp_list = new List<FinancialPeriod>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select financial_periods.*, cr.\"UFirstName\", cr.\"ULastName\", cr.\"UEmail\", cl.\"UFirstName\" As cl_fname, cl.\"ULastName\" As cl_lname, cl.\"UEmail\" As cl_email, ap.\"UFirstName\" As app_fname, ap.\"ULastName\" As app_lname, ap.\"UEmail\" As app_email, \"Branches\".\"BrName\" From financial_periods LEFT Join \"Users\" cr On cr.\"UId\" = financial_periods.fp_createdby LEFT Join \"Users\" cl On cl.\"UId\" = financial_periods.fp_closedby LEFT Join \"Users\" ap On ap.\"UId\" = financial_periods.fp_authorisedby LEFT Join \"Branches\" On \"Branches\".\"BrId\" = financial_periods.fp_branch WHERE fp_branch = " + user_branch + " ORDER BY fp_id DESC ", cnn).ExecuteReader();

            while (sdr0.Read())
            {
                FinancialPeriod fp = new FinancialPeriod();

                fp.fp_id = sdr0["fp_id"] != DBNull.Value ? (int)sdr0["fp_id"] : 0;
                fp.fp_ref = sdr0["fp_ref"] != DBNull.Value ? (string)sdr0["fp_ref"] : null;
                fp.fp_name = sdr0["fp_name"] != DBNull.Value ? (string)sdr0["fp_name"] : null;
                fp.fp_trans_date = sdr0["fp_trans_date"] != DBNull.Value ? (DateTime)sdr0["fp_trans_date"] : DateTime.Today;
                fp.fp_openingdate = sdr0["fp_openingdate"] != DBNull.Value ? (DateTime)sdr0["fp_openingdate"] : DateTime.Today;
                fp.fp_closingdate = sdr0["fp_closingdate"] != DBNull.Value ? (DateTime)sdr0["fp_closingdate"] : DateTime.Today;
                fp.fp_active = sdr0["fp_active"] != DBNull.Value ? (bool)sdr0["fp_active"] : false;
                fp.fp_createdby = sdr0["fp_createdby"] != DBNull.Value ? (int)sdr0["fp_createdby"] : 0;
                fp.fp_closedby = sdr0["fp_closedby"] != DBNull.Value ? (int)sdr0["fp_closedby"] : 0;
                fp.fp_authorisedby = sdr0["fp_authorisedby"] != DBNull.Value ? (int)sdr0["fp_authorisedby"] : 0;
                fp.fp_createdon = sdr0["fp_createdon"] != DBNull.Value ? (DateTime)sdr0["fp_createdon"] : DateTime.Today;
                fp.fp_date_mode = sdr0["fp_date_mode"] != DBNull.Value ? (string)sdr0["fp_date_mode"] : null;
                fp.fp_branch = sdr0["fp_branch"] != DBNull.Value ? (int)sdr0["fp_branch"] : 0;

                fp.creator_fname = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                fp.creator_lname = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;
                fp.creator_email = sdr0["UEmail"] != DBNull.Value ? (string)sdr0["UEmail"] : null;

                fp.closer_fname = sdr0["cl_fname"] != DBNull.Value ? (string)sdr0["cl_fname"] : null;
                fp.closer_lname = sdr0["cl_lname"] != DBNull.Value ? (string)sdr0["cl_lname"] : null;
                fp.closer_lname = sdr0["cl_email"] != DBNull.Value ? (string)sdr0["cl_email"] : null;

                fp.approver_fname = sdr0["app_fname"] != DBNull.Value ? (string)sdr0["app_fname"] : null;
                fp.approver_lname = sdr0["app_lname"] != DBNull.Value ? (string)sdr0["app_lname"] : null;
                fp.approver_email = sdr0["app_email"] != DBNull.Value ? (string)sdr0["app_email"] : null;

                fp_list.Add(fp);
            }
            cnn.Close();

            //success
            return Ok(new
            {
                FPList = fp_list
            });


        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult financialperiod_Activate_Deactivate(int financial_id)
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
                return BadRequest(new { message = "An error occurred while trying to get your branch details. Request failed" });
            }


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            bool period_active = false;

            //check if fp name exists
            cnn.Open();
            NpgsqlDataReader sdr_dp = new NpgsqlCommand("Select * From financial_periods WHERE fp_id= " + financial_id + " ", cnn).ExecuteReader();
            if (sdr_dp.HasRows == false)
            {
                cnn.Close();
                return BadRequest(new { message = "The financial reference was NOT found " });
            }
            else
            {
                while (sdr_dp.Read())
                {
                    period_active = (bool)sdr_dp["fp_active"];
                }
            }
            cnn.Close();

            bool set_status = false;

            if (period_active == false)
            {
                set_status = true;
            }
            else
            {
                set_status = false;
            }

            // set all to inactive 
            cnn.Open();
            string updQ = "UPDATE financial_periods SET fp_active = 'f' ";
            bool myReq1 = myDbconnection.UpdateDelInsert(updQ, db);
            cnn.Close();
            if (myReq1 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request." });
            }

            // update
            cnn.Open();
            string insertQ = "UPDATE financial_periods SET fp_active = '" + set_status + "' WHERE fp_id = " + financial_id + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request." });
            }

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"
            });


        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult financialperiod_Specific(int financial_id)
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

            ////check permission
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occured while trying to get your branch details. Request failed" });
            }


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get financial periods
            FinancialPeriod fp = new FinancialPeriod();

            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select financial_periods.*, cr.\"UFirstName\", cr.\"ULastName\", cr.\"UEmail\", cl.\"UFirstName\" As cl_fname, cl.\"ULastName\" As cl_lname, cl.\"UEmail\" As cl_email, ap.\"UFirstName\" As app_fname, ap.\"ULastName\" As app_lname, ap.\"UEmail\" As app_email, \"Branches\".\"BrName\" From financial_periods LEFT Join \"Users\" cr On cr.\"UId\" = financial_periods.fp_createdby LEFT Join \"Users\" cl On cl.\"UId\" = financial_periods.fp_closedby LEFT Join \"Users\" ap On ap.\"UId\" = financial_periods.fp_authorisedby LEFT Join \"Branches\" On \"Branches\".\"BrId\" = financial_periods.fp_branch WHERE fp_branch = " + user_branch + " AND fp_id = " + financial_id + "  ", cnn).ExecuteReader();

            while (sdr0.Read())
            {


                fp.fp_id = sdr0["fp_id"] != DBNull.Value ? (int)sdr0["fp_id"] : 0;
                fp.fp_ref = sdr0["fp_ref"] != DBNull.Value ? (string)sdr0["fp_ref"] : null;
                fp.fp_name = sdr0["fp_name"] != DBNull.Value ? (string)sdr0["fp_name"] : null;
                fp.fp_trans_date = sdr0["fp_trans_date"] != DBNull.Value ? (DateTime)sdr0["fp_trans_date"] : DateTime.Today;
                fp.fp_openingdate = sdr0["fp_openingdate"] != DBNull.Value ? (DateTime)sdr0["fp_openingdate"] : DateTime.Today;
                fp.fp_closingdate = sdr0["fp_closingdate"] != DBNull.Value ? (DateTime)sdr0["fp_closingdate"] : DateTime.Today;
                fp.fp_active = sdr0["fp_active"] != DBNull.Value ? (bool)sdr0["fp_active"] : false;
                fp.fp_createdby = sdr0["fp_createdby"] != DBNull.Value ? (int)sdr0["fp_createdby"] : 0;
                fp.fp_closedby = sdr0["fp_closedby"] != DBNull.Value ? (int)sdr0["fp_closedby"] : 0;
                fp.fp_authorisedby = sdr0["fp_authorisedby"] != DBNull.Value ? (int)sdr0["fp_authorisedby"] : 0;
                fp.fp_createdon = sdr0["fp_createdon"] != DBNull.Value ? (DateTime)sdr0["fp_createdon"] : DateTime.Today;
                fp.fp_date_mode = sdr0["fp_date_mode"] != DBNull.Value ? (string)sdr0["fp_date_mode"] : null;
                fp.fp_branch = sdr0["fp_branch"] != DBNull.Value ? (int)sdr0["fp_branch"] : 0;

                fp.creator_fname = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                fp.creator_lname = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;
                fp.creator_email = sdr0["UEmail"] != DBNull.Value ? (string)sdr0["UEmail"] : null;

                fp.closer_fname = sdr0["cl_fname"] != DBNull.Value ? (string)sdr0["cl_fname"] : null;
                fp.closer_lname = sdr0["cl_lname"] != DBNull.Value ? (string)sdr0["cl_lname"] : null;
                fp.closer_lname = sdr0["cl_email"] != DBNull.Value ? (string)sdr0["cl_email"] : null;

                fp.approver_fname = sdr0["app_fname"] != DBNull.Value ? (string)sdr0["app_fname"] : null;
                fp.approver_lname = sdr0["app_lname"] != DBNull.Value ? (string)sdr0["app_lname"] : null;
                fp.approver_email = sdr0["app_email"] != DBNull.Value ? (string)sdr0["app_email"] : null;


            }
            cnn.Close();

            //success
            return Ok(new
            {
                FPData = fp
            });




        }



        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult closingfinancialperiod(string financial_ref)
        {
            if (string.IsNullOrEmpty(financial_ref))
            {
                return BadRequest(new { message = "undefined financial reference" });
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

            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occured while trying to get your branch details. Request failed" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            bool status = false;

            cnn.Open();
            string insertQ = "UPDATE financial_periods SET fp_active = false, fp_closedby = " + userId + " WHERE fp_id = " + financial_ref + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to process request." });
            }

            //success
                return Ok(new
            {
                message = "Request has been successfully processed"
            });


        }






        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult financialperiod_update(string financial_ref, [FromBody] FinancialPeriod recvData)
        {
            if (string.IsNullOrEmpty(financial_ref))
            {
                return BadRequest(new { message = "undefined financial reference" });
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

            //get user branch
            int user_branch = myDbconnection.GetStaffBranch(userId, db);
            if (user_branch == 0)
            {
                return BadRequest(new { message = "An error occured while trying to get your branch details. Request failed" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            cnn.Open();
            string insertQ = "UPDATE financial_periods SET fp_name = '" + recvData.fp_name + "',fp_date_mode = '" + recvData.fp_date_mode + "', fp_trans_date = '" + recvData.fp_trans_date + "' WHERE fp_ref = '" + financial_ref + "' ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to process request." });
            }

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"
            });


        }

    }

}
