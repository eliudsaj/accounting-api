using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MimeKit;
using MimeKit.Text;
using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Models.UserProfile;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Service.PermissionService;
using pyme_finance_api.Service.DashboardService;
using pyme_finance_api.Service.UserService;

namespace pyme_finance_api.Controllers.UserAccounts
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private IWebHostEnvironment _hostingEnvironment;

        public UserAccountController(IConfiguration config, IWebHostEnvironment environment)
        {
            _configuration = config;
            _hostingEnvironment = environment;

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult my_userprofile(int prp)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}



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
            //bool perStatus = _pageValidationController.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get signatures
            List<usersignature> users_Signatures = new List<usersignature>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM signatures WHERE sign_user = " + prp + " ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                usersignature sgn = new usersignature();

                sgn.sign_id = sdr0["sign_id"] != DBNull.Value ? (int)sdr0["sign_id"] : 0;
                sgn.sign_date = sdr0["sign_date"] != DBNull.Value ? (DateTime)sdr0["sign_date"] : DateTime.Today;
                sgn.sign_user = sdr0["sign_user"] != DBNull.Value ? (int)sdr0["sign_user"] : 0;
                sgn.sign_data = sdr0["sign_data"] != DBNull.Value ? (string)sdr0["sign_data"] : null;
                sgn.sign_name = sdr0["sign_name"] != DBNull.Value ? (string)sdr0["sign_name"] : null;

                users_Signatures.Add(sgn);
            }
            cnn.Close();

            //get logged in user data
            Users usr = new Users();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT \"Users\".*,\"BrName\",\"DpName\" FROM \"Users\" LEFT JOIN \"Branches\" ON (\"BrId\" = \"UBranch\") LEFT JOIN \"Departments\" ON (\"DpId\" = \"UDepartment\" ) WHERE \"UId\" = " + prp + " ", cnn).ExecuteReader();
            if (sdr1.Read())
            {
                usr.UId = sdr1["UId"] != DBNull.Value ? (int)sdr1["UId"] : 0;
                usr.UFirstName = sdr1["UFirstName"] != DBNull.Value ? (string)sdr1["UFirstName"] : null;
                usr.ULastName = sdr1["ULastName"] != DBNull.Value ? (string)sdr1["ULastName"] : null;
                usr.UEmail = sdr1["UEmail"] != DBNull.Value ? (string)sdr1["UEmail"] : null;
                usr.UContact = sdr1["UContact"] != DBNull.Value ? (string)sdr1["UContact"] : null;
                usr.UType = sdr1["UType"] != DBNull.Value ? (string)sdr1["UType"] : null;
                usr.UVAT = sdr1["UVAT"] != DBNull.Value ? (string)sdr1["UVAT"] : null;
                usr.UIdnumber = sdr1["UIdnumber"] != DBNull.Value ? (int)sdr1["UIdnumber"] : 0;
                usr.UProfile = sdr1["UProfile"] != DBNull.Value ? (string)sdr1["UProfile"] : null;

                usr.Department_name = sdr1["DpName"] != DBNull.Value ? (string)sdr1["DpName"] : null;
                usr.Branch_name = sdr1["BrName"] != DBNull.Value ? (string)sdr1["BrName"] : null;
            }

           string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "profiles");
            string full_imgPath = Path.Combine(img_path, usr.UProfile);
            byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            usr.UProfile = base64ImageRepresentation;
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);





            cnn.Close();

            PermissionService permissionservice = new PermissionService(tokenData);

            var userPermission = permissionservice.getUsersPermissions(prp);
            var allpermissions = permissionservice.getAllPermissions();




            UserService userService = new UserService(companyRes);
            var groups = userService.getUserGroups(prp);


            return Ok(new {
                CurrUserSignatures = users_Signatures, 
                LoggedInUseData = usr, 
                auditList = auditTrailService.GetUserAuditTrails(prp),
                systempermissions = allpermissions,
                usergroups = groups,
                userPermissions = userPermission });

        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult my_add_signature(usersignature signData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}



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
            //bool perStatus = _pageValidationController.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //remove prefix
            string convert = signData.sign_data.Replace("data:image/svg+xml;base64,", String.Empty);

            //upload
            //string rand_img_name = System.Guid.NewGuid().ToString() + ".svg";


            // System.IO.File.WriteAllBytes(Path.Combine(_hostingEnvironment.WebRootPath, "images", "signatures", rand_img_name), Convert.FromBase64String(convert));

            //check name

            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select * From signatures WHERE sign_name = '" + signData.sign_name + "' AND sign_user = " + signData.sign_user + "  ", cnn).ExecuteReader();
            if (sdrb.HasRows == true)
            {
                return BadRequest(new { message = "Sorry! you have another signature registered with the same name." });

            }
            cnn.Close();


            //check last id
            int lastsignatureId = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(sign_id), 0) as ref From signatures LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastsignatureId = (int)sdra["ref"];

            }
            cnn.Close();

            //save details
            cnn.Open();
            string insertQuery = "INSERT INTO signatures (sign_id, sign_date, sign_user, sign_data, sign_name) VALUES(" + (lastsignatureId + 1) + ", '" + DateTime.Today + "', " + signData.sign_user + ", '" + convert + "', '" + signData.sign_name + "' ); ";

            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery, db);
            if (myReq1 == true)
            {
                return Ok(new { message = "Signature has been successfully created" });
            }
            else
            {
                //failed
                return BadRequest(new { message = "An occured while trying to create your payment receipt." });
            }


        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult my_delete_signature(int sign)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}



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


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //delete details
            cnn.Open();
            string insertQuery = "DELETE FROM signatures WHERE sign_id = " + sign + " ";

            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery, db);
            if (myReq1 == true)
            {
                return Ok(new { message = "Signature has been successfully removed" });
            }
            else
            {
                //failed
                return BadRequest(new { message = "An occured while trying to create your payment receipt." });
            }


        }




        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult suspendUserId(int staffid)
        {
            //check if company code exists
            var companyRes = "";
    

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];


            //if (string.IsNullOrEmpty(permissionName))
            //{
            //    return BadRequest(new { message = "Cannot find required permission parameters. Request terminated.Security verification failed." });
            //}



            //get token data
            jwt_token jwt = new jwt_token(_configuration);

            var tokenData = jwt.GetClaim(jwtHeader, "DB");
           int userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
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


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            string status = "SUSPENDED";

            //delete details
            cnn.Open();
            string updateQuery = "UPDATE \"Users\" SET \"UStatus\" = '" + status + "' WHERE \"UId\" =  " + staffid + " ;";


       

            bool myReq1 = myDbconnection.UpdateDelInsert(updateQuery, db);
            if (myReq1 == true)
            {
                return Ok(new { message = "Account has been suspended successfuly", status = 200 });
            }
            else
            {
                //failed
                return Ok(new { message = "An occured while trying to suspend this account." ,status = 400});
            }


        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult dashboarddata()
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
            string permissionName = Request.Headers["PermName"];
           
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

            DashboardService dashboardService = new DashboardService(tokenData);
            var result = dashboardService.getDashboardService();


            return Ok(new { dashboarddata = result });

        }




        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public  ActionResult get_groups()
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

            UserService userService = new UserService(companyRes);
            var groups = userService.getGroups();



            return Ok(groups);

        }




        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_group_members(int groupid)
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

            UserService userService = new UserService(companyRes);
            var groups = userService.getGroupMembers(groupid);



            return Ok(groups);

        }



        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_group_data(int groupid)
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

            UserService userService = new UserService(companyRes);
            var data = userService.GetGroupData(groupid);



            return Ok(new { groupdata = data, status = 200 });

        }




        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult add_group_permission(AddPermissionToGroup addPermissionToGroup)
        {

            if (addPermissionToGroup.groupId == 0)
            {
                return BadRequest(new { message = "No group reference was selected" });
            }

            if (addPermissionToGroup.Permissions.Length == 0)
            {
                return BadRequest(new { message = "No permission was selected" });
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

            UserService userService = new UserService(companyRes);
            var data = userService.AddPermissionToGroup(addPermissionToGroup);




            return Ok(new { message = data.Message, status = data.Httpcode });

        }




        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult add_group_members(AddUserToGroup addUserToGroup)
        {

            if (addUserToGroup.groupId == 0)
            {
                return BadRequest(new { message = "No group reference was selected" });
            }

            if ( addUserToGroup.users.Length == 0)
            {
                return BadRequest(new { message = "please,select at least one users" });
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

            UserService userService = new UserService(companyRes);
            var data = userService.AddUserstoGroup(addUserToGroup);




            return Ok(new { message = data.Message, status = data.Httpcode });

        }


        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        public ActionResult remove_group_permission(GroupPermmissions groupPermmissions)
        {

            if(groupPermmissions.Permission.Length == 0)
            {
                return BadRequest(new { message = "No permission was selected" });
            }

            if(groupPermmissions.GroupId < 0)
            {
                return BadRequest(new { message = "No group reference was selected" });
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

            UserService userService = new UserService(companyRes);
            var data = userService.RemovePermissionFromGroup(groupPermmissions);




            return Ok(new { message = data.Message, status = data.Httpcode });

        }



        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        public ActionResult remove_group_member(UserGroups userGroups)
        {

            if (userGroups.UserId == 0)
            {
                return BadRequest(new { message = "No user was selected" });
            }

            if (userGroups.GroupId < 0)
            {
                return BadRequest(new { message = "No group reference was selected" });
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

            UserService userService = new UserService(companyRes);
            var data = userService.RemoveUserFromGroup(userGroups);




            return Ok(new { message = data.Message, status = data.Httpcode });

        }







        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_groups_permissions()
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

            UserService userService = new UserService(companyRes);
            var groups = userService.groupPermmissions();



            return Ok(groups);

        }






        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult deactivate_group(int groupid)
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

            UserService userService = new UserService(companyRes);
            var response = userService.deactivategroup(groupid);



            return Ok(new { message = response.Message, status = response.Httpcode });

        }


        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult activate_group(int groupid)
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

            UserService userService = new UserService(companyRes);
            var response = userService.activategroup(groupid);



            return Ok(new { message = response.Message, status = response.Httpcode });

        }





        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult add_user_group([FromBody] AddGroupRequest addGroupRequest)
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

            UserService userService = new UserService(companyRes);
            var response = userService.addUserGroup(addGroupRequest,userId);
       
                return Ok(new { message = response.Message , status = response.Httpcode});
            




        }



        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult users_all()
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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get user branch
            int userbranch = myDbconnection.GetStaffBranch(userId, db);
            if (userbranch == 0)
            {
                return BadRequest(new { message = "Sorry! An error occured while getting your branch details" });
            }
            //get logged in user data
            List<Users> userslist = new List<Users>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT \"Users\".*,\"BrName\",\"DpName\" FROM \"Users\" LEFT JOIN \"Branches\" ON (\"BrId\" = \"UBranch\") LEFT JOIN \"Departments\" ON (\"DpId\" = \"UDepartment\" ) WHERE \"UBranch\" = " + userbranch + "  ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                Users usr = new Users();

                usr.UId = sdr1["UId"] != DBNull.Value ? (int)sdr1["UId"] : 0;
                usr.UFirstName = sdr1["UFirstName"] != DBNull.Value ? (string)sdr1["UFirstName"] : null;
                usr.ULastName = sdr1["ULastName"] != DBNull.Value ? (string)sdr1["ULastName"] : null;
                usr.UEmail = sdr1["UEmail"] != DBNull.Value ? (string)sdr1["UEmail"] : null;
                usr.UContact = sdr1["UContact"] != DBNull.Value ? (string)sdr1["UContact"] : null;
                usr.UType = sdr1["UType"] != DBNull.Value ? (string)sdr1["UType"] : null;
                usr.UVAT = sdr1["UVAT"] != DBNull.Value ? (string)sdr1["UVAT"] : null;
                usr.UIdnumber = sdr1["UIdnumber"] != DBNull.Value ? (int)sdr1["UIdnumber"] : 0;
                usr.UProfile = sdr1["UProfile"] != DBNull.Value ? (string)sdr1["UProfile"] : null;
                usr.RegistrationDate = sdr1["RegistrationDate"] != DBNull.Value ? (DateTime)sdr1["RegistrationDate"] : DateTime.Today;
                usr.UStatus = sdr1["UStatus"] != DBNull.Value ? (string)sdr1["UStatus"] : null;

                usr.Department_name = sdr1["DpName"] != DBNull.Value ? (string)sdr1["DpName"] : null;
                usr.Branch_name = sdr1["BrName"] != DBNull.Value ? (string)sdr1["BrName"] : null;

                userslist.Add(usr);
            }
            cnn.Close();
            PermissionService permissionservice = new PermissionService(tokenData);

          
            var allpermissions = permissionservice.getAllPermissions();

            return Ok(new { usersList = userslist, systempermissions = allpermissions }); 

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult users_addNew_default()
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


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get user branch
            int userbranch = myDbconnection.GetStaffBranch(userId, db);
            if (userbranch == 0)
            {
                return BadRequest(new { message = "Sorry! An error occured while getting your branch details" });
            }

            //get departments
            List<Departments> DepList = new List<Departments>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"Departments\" WHERE \"DpBranch\" = " + userbranch + " ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                Departments dp = new Departments();

                dp.DpId = sdr1["DpId"] != DBNull.Value ? (int)sdr1["DpId"] : 0;
                dp.DpName = sdr1["DpName"] != DBNull.Value ? (string)sdr1["DpName"] : null;


                DepList.Add(dp);
            }
            cnn.Close();

            //get company details
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();

            //get current branch
            Branches br = new Branches();
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("Select * From \"Branches\" WHERE \"BrId\" = " + userbranch + "  ", cnn).ExecuteReader();
            if (sdr2.Read())
            {
                br.BrId = sdr2["BrId"] != DBNull.Value ? (int)sdr2["BrId"] : 0;
                br.BrName = sdr2["BrName"] != DBNull.Value ? (string)sdr2["BrName"] : null;
                br.BrLocation = sdr2["BrLocation"] != DBNull.Value ? (string)sdr2["BrLocation"] : null;
                br.BrName = sdr2["BrName"] != DBNull.Value ? (string)sdr2["BrName"] : null;
                br.BrName = sdr2["BrName"] != DBNull.Value ? (string)sdr2["BrName"] : null;
            }
            cnn.Close();



            return Ok(new { DepartmentsList = DepList, companyData = lic, CurrBranch = br });

        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult users_CreateNew(string parcCompany, [FromBody] Users recvData)
        {
            //check data
            if (string.IsNullOrEmpty(recvData.UFirstName))
            {
                return BadRequest(new { message = "Missing account firstname " });

            }
            else if (string.IsNullOrEmpty(recvData.ULastName))
            {
                return BadRequest(new { message = "Missing account lastname " });
            }
            else if (string.IsNullOrEmpty(recvData.UEmail))
            {
                return BadRequest(new { message = "Missing account email " });
            }
            else if (recvData.UDepartment == 0)
            {
                return BadRequest(new { message = "Missing account department" });

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


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get user branch
            int userbranch = myDbconnection.GetStaffBranch(userId, db);
            if (userbranch == 0)
            {
                return BadRequest(new { message = "Sorry! An error occured while getting your branch details" });
            }

            //check email
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select * FROM \"Users\" WHERE \"UEmail\" = '" + recvData.UEmail + "' ", cnn).ExecuteReader();
            if (sdr0.HasRows == true)
            {
                return BadRequest(new { message = "The email " + recvData.UEmail.ToLower() + " is already registered by another user." });
            }
            cnn.Close();

            //check username
            cnn.Open();
            NpgsqlDataReader sdr10 = new NpgsqlCommand("Select * FROM \"Users\" WHERE \"Username\" = '" + recvData.Username + "' ", cnn).ExecuteReader();
            if (sdr10.HasRows == true)
            {
                return BadRequest(new { message = "The username " + recvData.Username + " is already in use by another user." });
            }
            cnn.Close();

            //check ID number
            cnn.Open();
            NpgsqlDataReader sdr11 = new NpgsqlCommand("Select * FROM \"Users\" WHERE \"UIdnumber\" = " + recvData.UIdnumber + " ", cnn).ExecuteReader();
            if (sdr11.HasRows == true)
            {
                return BadRequest(new { message = "The ID number " + recvData.UIdnumber + " is already registered." });
            }
            cnn.Close();

            //get last purchase request
            int last_user_id = 0;
            cnn.Open();
            NpgsqlDataReader sdr_PRI = new NpgsqlCommand("Select MAX(\"UId\") as sl From \"Users\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdr_PRI.Read())
            {
                last_user_id = sdr_PRI["sl"] != DBNull.Value ? (int)sdr_PRI["sl"] : 0;
            }
            cnn.Close();

            string img_path = null;
            //check if user profile has data
            if (string.IsNullOrEmpty(recvData.UProfile))
            {
                img_path = "default-user-icon-4.jpg";
            }
            else
            {
                //remove prefix
                recvData.UProfile = recvData.UProfile.Substring(recvData.UProfile.LastIndexOf(',') + 1);

                //upload image
                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "profiles");

                //Check if directory exist
                if (!System.IO.Directory.Exists(img_path))
                {
                    return BadRequest(new { message = "The path to upload account profile does NOT exist" });
                }

                string rand_imageName = System.Guid.NewGuid().ToString("D") + ".jpg";

                //set the image path
                string full_imgPath = Path.Combine(img_path, rand_imageName);

                //write file
                System.IO.File.WriteAllBytes(full_imgPath, Convert.FromBase64String(recvData.UProfile));

                img_path = rand_imageName;

            }

            string random_password = GenRandomString(8);
            //encrypt password
            string randomEncryptedPassword = CommonMethods.ConvertToEncrypt(random_password);

            //get company details
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();

            //send email
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplates", "staff_welcome.html");
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(path))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }
            //{0} : Subject  
            //{1} : DateTime  
            //{2} : Email  
            //{3} : Password  
            //{4} : Message  
            //{5} : callbackURL  

            string messageBody = string.Format(builder.HtmlBody,
                "Account Registration",
                String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                recvData.UFirstName.ToUpper() + " " + recvData.ULastName.ToUpper(),
                parcCompany,
                recvData.Username,
                random_password,
                lic.CompanyName,
                lic.PhysicalAddress,
                lic.CompanyContact,
                lic.CompanyPostal
            );

            //prepare email
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetValue<string>("MailSettings:SendEmail")));
            email.To.Add(MailboxAddress.Parse(recvData.UEmail));
            email.Subject = "Staff Account Registration ";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text =
                    messageBody
            };
            // send email
            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            smtp.Connect(_configuration.GetValue<string>("MailSettings:Host"), _configuration.GetValue<int>("MailSettings:Port"), SecureSocketOptions.Auto);
            smtp.Authenticate(_configuration.GetValue<string>("MailSettings:SendEmail"), _configuration.GetValue<string>("MailSettings:Password"));
            smtp.Send(email);
            smtp.Disconnect(true);

            //add user
            cnn.Open();
            string insertQ = "INSERT INTO \"Users\" (\"UFirstName\", \"ULastName\", \"UEmail\", \"UPassword\", \"UType\", \"UContact\", \"UStatus\", \"UCompany\", \"UFirst\",\"UProfile\", \"RegistrationDate\", \"UBranch\", \"UDepartment\", \"Username\", \"UId\", \"UVAT\", \"UIdnumber\") VALUES('" + recvData.UFirstName + "','" + recvData.ULastName + "', '" + recvData.UEmail + "','" + randomEncryptedPassword + "','User','" + recvData.UContact + "','ACTIVE','" + parcCompany + "','t','" + img_path + "','" + DateTime.Today + "'," + userbranch + "," + recvData.UDepartment + ",'" + recvData.Username + "'," + (last_user_id + 1) + ",'" + recvData.UVAT + "'," + recvData.UIdnumber + " ); ";

            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);

            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to save details." });
            }



            return Ok(new { message = "Account has been successfully created" });

        }
        private static Random random = new Random();
        private static string GenRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
