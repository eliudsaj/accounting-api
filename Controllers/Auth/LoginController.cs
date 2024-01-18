using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using pyme_finance_api.Models.DBConn;
using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Authentication;
using Microsoft.Net.Http.Headers;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Service.AuthService;
using pyme_finance_api.Service.MailService;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.PermissionService;
using pyme_finance_api.Models.Permission;
using pyme_finance_api.Service.UserService;
using pyme_finance_api.Models.Settings;

namespace pyme_finance_api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly string _default_connectionString;
        private readonly ILogger<LoginController> _logger;
        dbconnection myDbconnection = new dbconnection();
        public LoginController(IConfiguration config, IWebHostEnvironment environment, ILogger<LoginController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _default_connectionString = config.GetConnectionString("LocalDatabase");
            _logger = logger;
        }
        //private bool Create_New_Database(string dbName)
        //{
        //    NpgsqlConnection cnn = new NpgsqlConnection(_default_connectionString);
        //    string qr = "CREATE DATABASE " + dbName + " WITH OWNER = postgres ENCODING = 'UTF8' ";
        //    cnn.Open();
        //    bool success = new NpgsqlCommand(qr, cnn).ExecuteNonQuery() > 0;
        //    cnn.Close();

        //    //check if success
        //    if (success == false)
        //    {
        //        return success;
        //    }

        //    cnn.Open();
        //    using var cmd = new NpgsqlCommand();
        //    cmd.Connection = cnn;
        //    string myquery = System.IO.File.ReadAllText(@"./SQLResources/clientExport.sql");
        //    cmd.CommandText = myquery;
        //    cmd.ExecuteNonQuery();
        //    cnn.Dispose();
        //    cnn.Close();
        //    return true;
        //}
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> CreateNewCompany(Company company)
        {
            _logger.LogInformation($"Creating Company {company.CpName}");
            if (string.IsNullOrEmpty(company.CpName))
            {
                _logger.LogError($"Company name is missing");
                return BadRequest(new { message = "Missing company name" });
            }
            else if (string.IsNullOrEmpty(company.CpAddress))
            {
                _logger.LogError($"Company address is missing");
                return BadRequest(new { message = "Missing company address" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminFirstname))
            {
                _logger.LogError($"Company admin firstname is missing");
                return BadRequest(new { message = "Missing Administrator firstname" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminLastname))
            {
                _logger.LogError($"Company admin last is missing");
                return BadRequest(new { message = "Missing Administrator lastname" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminEmail))
            {
                _logger.LogError($"Company email is missing");
                return BadRequest(new { message = "Missing Administrator email" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminContact))
            {
                return BadRequest(new { message = "Missing Administrator contact" });
            }
            NpgsqlConnection cnn_default = new NpgsqlConnection(_default_connectionString);
            _logger.LogInformation($"Checking if vat or pin  already exists ");
            //check KRA Pin exists
            cnn_default.Open();
            string qr_k = "SELECT * FROM \"Company\" WHERE \"KRAPin\" =  '" + company.KRAPin + "' ";
            NpgsqlCommand cmd_k = new NpgsqlCommand(qr_k, cnn_default);
            NpgsqlDataAdapter da_k = new NpgsqlDataAdapter(cmd_k);
            DataSet ds1_k = new DataSet();
            da_k.Fill(ds1_k);
            int ik = ds1_k.Tables[0].Rows.Count;
            cnn_default.Close();
            if (ik > 0)
            {
                return BadRequest(new { message = "Sorry, the company KRA certificate " + company.KRAPin.ToUpper() + " is already registered with PYME FINANCE. Please contact support for more details." });
            }
            //check if company admin email exists
            _logger.LogInformation($"Checking if adminemail exists ");
            cnn_default.Open();
            string qr = "SELECT * FROM \"Company\" WHERE \"CpAdminEmail\" =  '" + company.CpAdminEmail + "' ";
            NpgsqlCommand cmd = new NpgsqlCommand(qr, cnn_default);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            DataSet ds1 = new DataSet();
            da.Fill(ds1);
            int i = ds1.Tables[0].Rows.Count;
            cnn_default.Close();

            if (i > 0)
            {
                _logger.LogError($" adminemail already exists ");
                return BadRequest(new { message = "Sorry, the administrator email " + company.CpAdminEmail.ToLower() + " is already registered with PYME FINANCE. Please contact support for more details." });
            }
            //get last value
            int last_cp_number = 0;
            cnn_default.Open();
            NpgsqlDataReader sdrR = new NpgsqlCommand("Select MAX(\"CpId\") as sj From \"Company\" LIMIT 1 ", cnn_default).ExecuteReader();
            while (sdrR.Read())
            {
                last_cp_number = sdrR["sj"] != DBNull.Value ? (int)sdrR["sj"] : 0;
            }
            cnn_default.Close();
            //get last registered number
            //cnn_default.Open();
            //NpgsqlDataReader sdr2 = new NpgsqlCommand("SELECT COALESCE(MAX(\"CpId\"),0) as st FROM \"Company\" ", cnn_default).ExecuteReader();
            //while (sdr2.Read())
            //{
            //    last_cp_number = (int)sdr2["st"];
            //}
            //cnn_default.Close();
            //set companyID as the VAT reference
            string cpRef = company.KRAPin;
            string clientDBName = company.CpName + "_" + myDbconnection.Generate_RandomInt(5);
            string license = System.Guid.NewGuid().ToString("B");
            _logger.LogInformation($"Generating company random password ");
            //generate random account password
            string randomPassword = myDbconnection.Generate_RandomString(6) + myDbconnection.Generate_RandomInt(5);
            _logger.LogInformation($"Random Password for Company being created is  {randomPassword} ");
            //Encrypt random password
            string randomEncryptedPassword = CommonMethods.ConvertToEncrypt(randomPassword);
            _logger.LogInformation($"Saving company {company.CpName} data ");
            //save company details
            string create_q = "INSERT INTO \"Company\" ( \"CpName\", \"CpAddress\", \"CpAdminEmail\", \"CpRegisterDate\", \"CpRef\", \"CpConnString\", \"CpStatus\", \"CpLogo\", \"CpDocuments\", \"CpAdminContact\", \"CpAdminFirstname\", " +
                "\"CpAdminLastname\", \"CpLicenseType\", \"CpExpireDate\", \"CpLicense\", \"CpId\", \"KRAPin\" ) VALUES ('" + company.CpName + "', '" + company.CpAddress + "', '" + company.CpAdminEmail + "', '" + DateTime.Now + "'," +
                " '" + cpRef + "', '" + clientDBName + "', 'Active','" + null + "', '" + null + "', '" + company.CpAdminContact + "', '" + company.CpAdminFirstname + "', '" + company.CpAdminLastname + "', 'PREMIUM', '" + DateTime.Today.AddDays(30) + "', '" + license + "', " + (last_cp_number + 1) + ", '" + company.KRAPin + "' ) ";
            cnn_default.Open();
            bool success = new NpgsqlCommand(create_q, cnn_default).ExecuteNonQuery() > 0;
            cnn_default.Close();
            if (success == false)
            {
                return BadRequest(new { message = "Error trying to register your company to the database. Operation failed." });
            }

            //create database
            AuthService authService = new AuthService();
            _logger.LogInformation($"Creating company {company.CpName} database ");
            // bool create_db_res = Create_New_Database(clientDBName);
            bool create_db_res = authService.Create_New_Database(clientDBName, _default_connectionString);
            if (create_db_res == false)
            {
                _logger.LogError($"Error while creating company database");
                return BadRequest(new { message = "Sorry! An error occured while trying to create the database. (Multi - tenancy) database creation failed" });
            }
            //local db conn
            NpgsqlConnection cnn_local = new NpgsqlConnection(_default_connectionString);
            //get last user id
            int last_user_id = 0;
            cnn_local.Open();
            //NpgsqlDataReader sdr_usrid = await new NpgsqlCommand("SELECT COALESCE(MAX(\"UId\"),0) as st FROM \"Users\" ", cnn_local).ExecuteReaderAsync();
            //while (sdr_usrid.Read())
            //{
            //    last_user_id = (int)sdr_usrid["st"];
            //}
            //get database name
            string db = clientDBName.Trim().ToLower();
            string account_username = company.CpAdminFirstname + myDbconnection.Generate_RandomInt(5);
            _logger.LogInformation($"Creating saving  company {company.CpName}  admin details ");
            //create user account
            string create_u = "INSERT INTO \"Users\" (\"UFirstName\", \"ULastName\", \"UEmail\", \"UPassword\", \"UType\", \"UContact\", \"UStatus\", \"UCompany\", \"UFirst\", \"UProfile\", \"RegistrationDate\", \"UBranch\", \"UDepartment\", \"Username\", \"UVAT\", \"UIdnumber\" ) VALUES ('" + company.CpAdminFirstname + "', '" + company.CpAdminLastname + "', '" + company.CpAdminEmail + "', '" + randomEncryptedPassword + "', 'Administrator', '" + company.CpAdminContact + "', 'ACTIVE','" + cpRef + "', 't', 'default-user-icon-4.jpg', '" + DateTime.Today + "', " + 1 + ", " + 1 + ", '" + account_username + "', '" + null + "', " + 0 + " ) ";

            bool success1 = myDbconnection.UpdateDelInsert(create_u, db);
            if (success1 == false)
            {
                return BadRequest(new { message = "Error trying to register your user account." });
            }
            //get last license id
            //int last_ls_id = 0;
            //NpgsqlDataReader sdr_ls = await new NpgsqlCommand("SELECT COALESCE(MAX(\"LsId\"),0) as st FROM \"License\" ", cnn_local).ExecuteReaderAsync();
            //while (sdr_ls.Read())
            //{
            //    last_user_id = (int)sdr_ls["st"];
            //}
            cnn_local.Close();
            //get administrator id
            int admin_id = 1;
            cnn_local.Open();
            //NpgsqlDataReader sdr_adm = new NpgsqlCommand("SELECT * FROM \"Users\" WHERE \"UEmail\" = '" + company.CpAdminEmail + "' ", cnn_local).ExecuteReader();
            //while (sdr_adm.Read())
            //{
            //    admin_id = (int)sdr_adm["UId"];
            //}
            //cnn_local.Close();

            //if (admin_id == 0)
            //{
            //    return BadRequest(new { message = "Error trying to get registered administered administrator reference." });
            //}
            _logger.LogInformation($"Creating saving  company {company.CpName}  licence data");

            //Add License
            string create_ls = "INSERT INTO \"Licence\" ( \"LsType\", \"LsCode\", \"LsIssueDate\", \"LsExpireDate\", \"CompanyName\", \"CompanySlogan\", \"CompanyAdmin\", \"CompanyPostal\", \"CompanyContact\", \"CompanyVAT\", \"PhysicalAddress\", \"CompanyLogo\", \"CompanyCurrency\" ) " +
                "VALUES ( 'Trial', '" + license + "', '" + DateTime.Today + "', '" + DateTime.Today.AddDays(30) + "', '" + company.CpName + "', '" + null + "','" + admin_id + "', '" + company.CpAddress + "', '" + company.CpAdminContact + "', '" + company.KRAPin + "', '" + null + "', '" + null + "', " + 1 + " ) ";

            bool success2 = myDbconnection.UpdateDelInsert(create_ls, db);
            if (success2 == false)
            {
                return BadRequest(new { message = "Error trying to register your PYME FINANCE license." });
            }



            //Give administrative rights

            _logger.LogInformation($"Sending Registration Email to {account_username}");

            MailService mailService = new MailService();

            mailService.SendCompanyTrailRegistrationEmail(company, _hostingEnvironment.WebRootPath, license, cpRef, account_username, randomPassword);
            ////send Email
            //var path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplates", "Company_Trial_Register.html");

            //   var builder = new BodyBuilder();
            //   using (StreamReader sourceReader = System.IO.File.OpenText(path))
            //   {
            //       builder.HtmlBody = sourceReader.ReadToEnd();
            //   }
            //   //{0} : Subject
            //   //{1} : DateTime
            //   //{2} : Email
            //   //{3} : Password
            //   //{4} : Message
            //   //{5} : callbackURL

            //   string messageBody = string.Format(builder.HtmlBody,
            //       "Welcome to PYME Finance",
            //       string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
            //       System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(company.CpName.ToLower()),
            //       System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase((company.CpAdminFirstname + " " + company.CpAdminLastname).ToLower()),
            //       string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
            //       string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now.AddDays(30)),
            //       license,
            //       cpRef,
            //       account_username,
            //       randomPassword
            //   );


            //   //send email
            //   var email = new MimeMessage();
            //   email.From.Add(MailboxAddress.Parse("mwangijustus12@gmail.com"));
            //   email.To.Add(MailboxAddress.Parse(company.CpAdminEmail));
            //   email.Subject = "Welcome " + company.CpAdminFirstname + " " + company.CpAdminLastname;
            //   email.Body = new TextPart(TextFormat.Html)
            //   {
            //       Text =
            //      messageBody
            //   };

            //   // send email
            //   using var smtp = new SmtpClient();
            //   smtp.Connect("in-v3.mailjet.com", 587, SecureSocketOptions.StartTls);
            //   smtp.Authenticate("cab7a809219d11a0cae71576d16596a1", "5395b324384107eb1e80c3ffa076327b");
            //   smtp.Send(email);
            //   smtp.Disconnect(true);


            _logger.LogInformation($" Company {company.CpName}  was created successfully ");

            return Ok(new { message = "Account Created successfully. Credentials sent to " + company.CpAdminEmail + "." });
        }



        [Route("[action]")]
        [HttpPost]

        public ActionResult CreateTrial(Company company)
        {


            _logger.LogInformation($"Creating Company {company.CpName}");
            if (string.IsNullOrEmpty(company.CpName))
            {
                _logger.LogError($"Company name is missing");
                return BadRequest(new { message = "Missing company name" });
            }

            else if (string.IsNullOrEmpty(company.CpAddress))
            {
                _logger.LogError($"Company address is missing");
                return BadRequest(new { message = "Missing company address" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminFirstname))
            {
                _logger.LogError($"Company admin firstname is missing");
                return BadRequest(new { message = "Missing Administrator firstname" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminLastname))
            {
                _logger.LogError($"Company admin last is missing");
                return BadRequest(new { message = "Missing Administrator lastname" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminEmail))
            {
                _logger.LogError($"Company email is missing");
                return BadRequest(new { message = "Missing Administrator email" });
            }
            else if (string.IsNullOrEmpty(company.CpAdminContact))
            {
                return BadRequest(new { message = "Missing Administrator contact" });
            }

            NpgsqlConnection cnn_default = new NpgsqlConnection(_default_connectionString);
            //_logger.LogInformation($"Checking if vat or pin  already exists ");
            //check KRA Pin exists
            //cnn_default.Open();
            //string qr_k = "SELECT * FROM \"Company\" WHERE \"KRAPin\" =  '" + company.KRAPin + "' ";
            //NpgsqlCommand cmd_k = new NpgsqlCommand(qr_k, cnn_default);
            //NpgsqlDataAdapter da_k = new NpgsqlDataAdapter(cmd_k);
            //DataSet ds1_k = new DataSet();
            //da_k.Fill(ds1_k);
            //int ik = ds1_k.Tables[0].Rows.Count;
            //cnn_default.Close();

            //if (ik > 0)
            //{
            //    return BadRequest(new { message = "Sorry, the company KRA certificate " + company.KRAPin.ToUpper() + " is already registered with PYME FINANCE. Please contact support for more details." });
            //}

            //check if company admin email exists

            _logger.LogInformation($"Checking if adminemail exists ");
            cnn_default.Open();
            string qr = "SELECT * FROM \"Company\" WHERE \"CpAdminEmail\" =  '" + company.CpAdminEmail + "' ";
            NpgsqlCommand cmd = new NpgsqlCommand(qr, cnn_default);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            DataSet ds1 = new DataSet();
            da.Fill(ds1);
            int i = ds1.Tables[0].Rows.Count;
            cnn_default.Close();

            if (i > 0)
            {
                _logger.LogError($" adminemail already exists ");
                return BadRequest(new { message = "Sorry, the administrator email " + company.CpAdminEmail.ToLower() + " is already registered with PYME FINANCE. Please contact support for more details." });
            }
            int last_cp_number = 0;


            cnn_default.Open();
            NpgsqlDataReader sdrR = new NpgsqlCommand("Select MAX(\"CpId\") as sj From \"Company\" LIMIT 1 ", cnn_default).ExecuteReader();
            while (sdrR.Read())
            {
                last_cp_number = sdrR["sj"] != DBNull.Value ? (int)sdrR["sj"] : 0;

            }
            cnn_default.Close();



            string cpRef = company.KRAPin;
            string clientDBName = company.CpName + "_" + myDbconnection.Generate_RandomInt(5);
            string license = System.Guid.NewGuid().ToString("B");
            _logger.LogInformation($"Generating company random password ");
            //generate random account password
            string randomPassword = myDbconnection.Generate_RandomString(6) + myDbconnection.Generate_RandomInt(5);

            //Encrypt random password
            string randomEncryptedPassword = CommonMethods.ConvertToEncrypt(randomPassword);

            _logger.LogInformation($"Saving company {company.CpName} data ");
            //save company details
            string create_q = "INSERT INTO \"Company\" ( \"CpName\",\"CpAdminIP\",\"CpAdminMac\", \"CpAddress\", \"CpAdminEmail\", \"CpRegisterDate\", \"CpRef\", \"CpConnString\", \"CpStatus\", \"CpLogo\", \"CpDocuments\", \"CpAdminContact\", \"CpAdminFirstname\", \"CpAdminLastname\", " +
                "\"CpLicenseType\", \"CpExpireDate\", \"CpLicense\", \"CpId\", \"KRAPin\" ) VALUES ('" + company.CpName + "','" + company.CpAdminIP + "','" + company.CpAdminMac + "' ,'" + company.CpAddress + "', '" + company.CpAdminEmail + "', '" + DateTime.Now + "', '" + cpRef + "', '" + clientDBName + "', 'Active','" + null + "', '" + null + "', '" + company.CpAdminContact + "', '" + company.CpAdminFirstname + "', '" + company.CpAdminLastname + "', 'Trial', '" + DateTime.Today.AddDays(30) + "', '" + license + "', " + (last_cp_number + 1) + ", '" + null + "' ) ";
            cnn_default.Open();
            bool success = new NpgsqlCommand(create_q, cnn_default).ExecuteNonQuery() > 0;
            cnn_default.Close();
            if (success == false)
            {
                return BadRequest(new { message = "Error trying to register your company to the database. Operation failed." });
            }

            //create database
            AuthService authService = new AuthService();

            _logger.LogInformation($"Creating company {company.CpName} database ");

            // bool create_db_res = Create_New_Database(clientDBName);
            bool create_db_res = authService.Create_New_Database(clientDBName, _default_connectionString);
            if (create_db_res == false)
            {
                _logger.LogError($"Error while creating company database");

                return BadRequest(new { message = "Sorry! An error occured while trying to create the database. (Multi - tenancy) database creation failed" });

            }

            //local db conn
            NpgsqlConnection cnn_local = new NpgsqlConnection(_default_connectionString);

            //get last user id
            int last_user_id = 0;
            cnn_local.Open();



            //get database name
            string db = clientDBName.Trim().ToLower();

            string account_username = company.CpAdminFirstname + myDbconnection.Generate_RandomInt(5);

            _logger.LogInformation($"Creating saving  company {company.CpName}  admin details ");

            //create user account
            string create_u = "INSERT INTO \"Users\" (\"UFirstName\", \"ULastName\", \"UEmail\", \"UPassword\", \"UType\", \"UContact\", \"UStatus\", \"UCompany\", \"UFirst\", \"UProfile\", \"RegistrationDate\"," +
                " \"UBranch\", \"UDepartment\", \"Username\", \"UVAT\", \"UIdnumber\" ) VALUES ('" + company.CpAdminFirstname + "', '" + company.CpAdminLastname + "', '" + company.CpAdminEmail + "', '" + randomEncryptedPassword + "', 'Administrator', '" + company.CpAdminContact + "', 'ACTIVE','" + cpRef + "', 't', 'default-user-icon-4.jpg', '" + DateTime.Today + "', " + 1 + ", " + 1 + ", '" + account_username + "', '" + null + "', " + 0 + " ) ";

            bool success1 = myDbconnection.UpdateDelInsert(create_u, db);
            if (success1 == false)
            {
                return BadRequest(new { message = "Error trying to register your user account." });
            }
            //Give administrative rights
            _logger.LogInformation($"Sending Registration Email to {account_username}");

            MailService mailService = new MailService();

            mailService.SendCompanyTrailRegistrationEmail(company, _hostingEnvironment.WebRootPath, license, cpRef, account_username, randomPassword);

            _logger.LogInformation($" Company {company.CpName}  was created successfully ");

            return Ok(new { message = "Demo Account Created successfully. Credentials sent to " + company.CpAdminEmail + "." });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetUsers()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _logger.LogInformation($"Fetching uses");
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
            List<Users> users_list = new List<Users>();
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select \"Users\".*, \"Departments\".\"DpName\", \"Departments\".\"DpRef\", \"Branches\".\"BrName\" From \"Users\" LEFT Join \"Departments\" On \"Departments\".\"DpId\" = \"Users\".\"UDepartment\" LEFT Join \"Branches\" On \"Branches\".\"BrId\" = \"Users\".\"UBranch\" ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Users usr = new Users();
                usr.UFirstName = sdr["UFirstName"] != DBNull.Value ? (string)sdr["UFirstName"] : null;
                usr.ULastName = sdr["ULastName"] != DBNull.Value ? (string)sdr["ULastName"] : null;
                usr.UEmail = sdr["UEmail"] != DBNull.Value ? (string)sdr["UEmail"] : null;
                usr.UType = sdr["UType"] != DBNull.Value ? (string)sdr["UType"] : null;
                usr.UContact = sdr["UContact"] != DBNull.Value ? (string)sdr["UContact"] : null;
                usr.UStatus = sdr["UStatus"] != DBNull.Value ? (string)sdr["UStatus"] : null;
                usr.UCompany = sdr["UCompany"] != DBNull.Value ? (string)sdr["UCompany"] : null;
                usr.UFirst = sdr["UFirst"] != DBNull.Value ? (bool)sdr["UFirst"] : false;
                usr.UProfile = sdr["UProfile"] != DBNull.Value ? (string)sdr["UProfile"] : null;
                usr.RegistrationDate = sdr["RegistrationDate"] != DBNull.Value ? (DateTime)sdr["RegistrationDate"] : DateTime.Now;
                usr.UBranch = sdr["UBranch"] != DBNull.Value ? (int)sdr["UBranch"] : 0;
                usr.UDepartment = sdr["UDepartment"] != DBNull.Value ? (int)sdr["UDepartment"] : 0;
                usr.Username = sdr["Username"] != DBNull.Value ? (string)sdr["Username"] : null;
                usr.UId = sdr["UId"] != DBNull.Value ? (int)sdr["UId"] : 0;
                usr.UVAT = sdr["UVAT"] != DBNull.Value ? (string)sdr["UVAT"] : null;
                usr.UIdnumber = sdr["UIdnumber"] != DBNull.Value ? (int)sdr["UIdnumber"] : 0;
                usr.Department_name = sdr["DpName"] != DBNull.Value ? (string)sdr["DpName"] : null;
                usr.Department_ref = sdr["DpRef"] != DBNull.Value ? (string)sdr["DpRef"] : null;
                usr.Branch_name = sdr["BrName"] != DBNull.Value ? (string)sdr["BrName"] : null;
                users_list.Add(usr);
            }
            cnn.Close();
            return Ok(new
            {
                users = users_list
            });
        }
        private Company GetCompany_Connection(string companyRef)
        {
            NpgsqlConnection cnn_default = new NpgsqlConnection(_default_connectionString);
            Company cp = new Company();
            cnn_default.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select * From \"Company\" WHERE \"CpRef\" = '" + companyRef.Trim() + "'  ", cnn_default).ExecuteReader();
            while (sdr.Read())
            {
                cp.CpConnString = sdr["CpConnString"] != DBNull.Value ? (string)sdr["CpConnString"] : null;
                cp.CpName = sdr["CpName"] != DBNull.Value ? (string)sdr["CpName"] : null;
            }
            cnn_default.Close();
            return cp;
        }

        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult updateUserPermissions(PermissionRequest permissionRequest)
        {

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //check if company code exists
            var companyRes = "";
            int userId = 0;
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
            PermissionService permissionservice = new PermissionService(tokenData);
            var response = permissionservice.updateUserPermission(permissionRequest.settrue, permissionRequest.setfalse, permissionRequest.user_id);
            if (response.Httpcode == 400)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response });
        }


        [Route("[action]")]
        [HttpPost]
        public ActionResult CreateUserPermissions(CreateUserPermissionRequest createUserPermissionRequest)
        {
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //check if company code exists
            var companyRes = "";
            int userId = 0;
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
            PermissionService permissionservice = new PermissionService(tokenData);
            var response = permissionservice.createUserPermission(createUserPermissionRequest.settrue, createUserPermissionRequest.user_id);
            if (response.Httpcode == 400)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message });
        }


        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult getUserPermissions(int user_id)
        {
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            //check if company code exists
            var companyRes = "";
            int userId = 0;
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
            PermissionService permissionservice = new PermissionService(tokenData);
            var userPermission = permissionservice.getUsersPermissions(user_id);
            return Ok(new { message = userPermission });
        }

        [Route("[action]")]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login([FromBody] Users users)
        {
            _logger.LogInformation($"User {users.Username} is logging in ");
            try
            {
                _logger.LogInformation("Class:LoginController | Method:Login | Start method |Params {0}", users.ToString());
                //List<Users> clUsers;
                if (string.IsNullOrEmpty(users.UCompany))
                {
                    _logger.LogError($"User {users.Username} is login failed due to missing company ref ");
                    return BadRequest(new { message = "Missing company reference" });
                }
                else if (string.IsNullOrEmpty(users.Username))
                {
                    _logger.LogError($"User {users.Username} is login failed due to missing username ");
                    return BadRequest(new { message = "Missing required account username" });
                }
                else if (string.IsNullOrEmpty(users.UPassword))
                {
                    _logger.LogError($"User {users.Username} is login failed due to missing password ");
                    return BadRequest(new { message = "Missing required password" });
                }
                //check if company code exists
                var companyRes = "";
                var companyName = "";
                //check if cookie exists in Request
                var headers = Request.Headers.TryGetValue("JWTAuth", out var traceValue);
                string JWTHeader = Request.Headers["JWTAuth"];
                if (JWTHeader != null)
                {
                    //validate token
                    jwt_token jwt = new jwt_token(_configuration);
                    var tokenStatus = jwt.ValidateCurrentToken(JWTHeader, users.UEmail);
                    if (tokenStatus == true)
                    {
                        //valid tokens // get token data
                        var tokenData = jwt.GetClaim(JWTHeader, "DB");
                        if (tokenData != "")
                        {
                            //assign conpamyRef from header
                            companyRes = tokenData;
                        }
                    }
                }
                //check companyRes
                if (string.IsNullOrEmpty(companyRes))
                {
                    //check if the company ref exists
                    NpgsqlConnection cnn_online = new NpgsqlConnection(_default_connectionString);
                    cnn_online.Open();
                    NpgsqlDataReader sdr_live = new NpgsqlCommand("Select * From \"Company\" WHERE \"CpRef\" ='" + users.UCompany.Trim() + "'", cnn_online).ExecuteReader();
                    if (sdr_live.HasRows == false)
                    {
                        _logger.LogError($"User {users.Username} is login failed due The company reference " + users.UCompany + " was NOT found. ");
                        return BadRequest(new { message = "The company reference " + users.UCompany + " was NOT found." });
                    }
                    cnn_online.Close();

                    //check online pyme finance database
                    Company cp_res = GetCompany_Connection(users.UCompany);
                    if (string.IsNullOrEmpty(cp_res.CpConnString))
                    {
                        return BadRequest(new { message = "The connection data from PYME FINANCE source database could NOT be established." });
                    }
                    else
                    {
                        companyRes = cp_res.CpConnString;
                        companyName = cp_res.CpName;
                    }
                }
                //get database name
                string db = companyRes.Trim();
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
                //get user data where username
                Users usr = new Users();
                cnn.Open();
                NpgsqlDataReader sdr = new NpgsqlCommand("Select * From \"Users\" WHERE \"Username\" = '" + users.Username.Trim() + "' ", cnn).ExecuteReader();
                if (sdr.HasRows == false)
                {
                    return BadRequest(new { message = "Username account was NOT found" });
                }
                while (sdr.Read())
                {
                    usr.UFirstName = sdr["UFirstName"] != DBNull.Value ? (string)sdr["UFirstName"] : null;
                    usr.ULastName = sdr["ULastName"] != DBNull.Value ? (string)sdr["ULastName"] : null;
                    usr.UEmail = sdr["UEmail"] != DBNull.Value ? (string)sdr["UEmail"] : null;
                    usr.UType = sdr["UType"] != DBNull.Value ? (string)sdr["UType"] : null;
                    usr.UPassword = sdr["UPassword"] != DBNull.Value ? (string)sdr["UPassword"] : null;
                    usr.UContact = sdr["UContact"] != DBNull.Value ? (string)sdr["UContact"] : null;
                    usr.UStatus = sdr["UStatus"] != DBNull.Value ? (string)sdr["UStatus"] : null;
                    usr.UCompany = sdr["UCompany"] != DBNull.Value ? (string)sdr["UCompany"] : null;
                    usr.UFirst = sdr["UFirst"] != DBNull.Value ? (bool)sdr["UFirst"] : false;
                    usr.UProfile = sdr["UProfile"] != DBNull.Value ? (string)sdr["UProfile"] : null;
                    usr.RegistrationDate = sdr["RegistrationDate"] != DBNull.Value ? (DateTime)sdr["RegistrationDate"] : DateTime.Now;
                    usr.UBranch = sdr["UBranch"] != DBNull.Value ? (int)sdr["UBranch"] : 0;
                    usr.UDepartment = sdr["UDepartment"] != DBNull.Value ? (int)sdr["UDepartment"] : 0;
                    usr.Username = sdr["Username"] != DBNull.Value ? (string)sdr["Username"] : null;
                    usr.UId = sdr["UId"] != DBNull.Value ? (int)sdr["UId"] : 0;
                    usr.UVAT = sdr["UVAT"] != DBNull.Value ? (string)sdr["UVAT"] : null;
                    usr.UIdnumber = sdr["UIdnumber"] != DBNull.Value ? (int)sdr["UIdnumber"] : 0;
                }
                cnn.Close();
                //get license table
                License ls = new License();
                cnn.Open();
                NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
                if (sdr1.HasRows == false)
                {
                    //no license row found
                    return BadRequest(new { message = "No license was found in your System.Please contact PYME FINANCE support" });
                }
                while (sdr1.Read())
                {
                    ls.LsId = sdr1["LsId"] != DBNull.Value ? (int)sdr1["LsId"] : 0;
                    ls.LsType = sdr1["LsType"] != DBNull.Value ? (string)sdr1["LsType"] : null;
                    ls.LsCode = sdr1["LsCode"] != DBNull.Value ? (string)sdr1["LsCode"] : null;
                    ls.LsIssueDate = sdr1["LsIssueDate"] != DBNull.Value ? (DateTime)sdr1["LsIssueDate"] : DateTime.Now;
                    ls.LsExpireDate = sdr1["LsExpireDate"] != DBNull.Value ? (DateTime)sdr1["LsExpireDate"] : DateTime.Now;
                }
                cnn.Close();
                PermissionService permissionservice = new PermissionService(db);
                var userPermission = permissionservice.getUsersPermissions(usr.UId);
                UserService userService = new UserService(db);
                var grouppermission = userService.getUserGroupPermission(usr.UId);
                foreach (var groupP in grouppermission)
                {
                    if (userPermission.ContainsKey(groupP))
                    {
                        /// do nothing
                        if (userPermission[groupP] == false)
                        {
                            userPermission[groupP] = true;
                        }
                    }
                    else
                    {
                        userPermission.Add(groupP, true);
                    }
                }
                bool isactive;
                //get financial periods
                List<FinancialPeriod> fp_list = new List<FinancialPeriod>();
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand("Select financial_periods.*, cr.\"UFirstName\", cr.\"ULastName\", cr.\"UEmail\", cl.\"UFirstName\" As cl_fname, cl.\"ULastName\" As cl_lname, " +
                    "cl.\"UEmail\" As cl_email, ap.\"UFirstName\" As app_fname, ap.\"ULastName\" As app_lname, ap.\"UEmail\" As app_email, \"Branches\".\"BrName\" From financial_periods " +
                    "LEFT Join \"Users\" cr On cr.\"UId\" = financial_periods.fp_createdby LEFT Join \"Users\" cl On cl.\"UId\" = financial_periods.fp_closedby" +
                    " LEFT Join \"Users\" ap On ap.\"UId\" = financial_periods.fp_authorisedby" +
                    " LEFT Join \"Branches\" On \"Branches\".\"BrId\" = financial_periods.fp_branch WHERE fp_branch = " + usr.UBranch + " AND \"financial_periods\".\"fp_active\" = '" + true + "'   ORDER BY fp_id DESC ", cnn).ExecuteReader();
                DateTime? closingdate = null;
                while (sdr0.Read())
                {
                    FinancialPeriod fp = new FinancialPeriod();
                    fp.fp_openingdate = sdr0["fp_openingdate"] != DBNull.Value ? (DateTime)sdr0["fp_openingdate"] : DateTime.Today;
                    fp.fp_closingdate = sdr0["fp_closingdate"] != DBNull.Value ? (DateTime)sdr0["fp_closingdate"] : DateTime.Today;
                    closingdate = fp.fp_closingdate;
                }
                cnn.Close();
                //check if license is trial
                if (ls.LsType == "Trial")
                {
                    //check date
                    DateTime expireDate = Convert.ToDateTime(ls.LsExpireDate);
                    var todayDate = DateTime.Today;
                    int result = DateTime.Compare(expireDate, todayDate);
                    if (result < 0)
                    {
                        //exceeded renewal
                        return BadRequest(new { message = "You license is already expired. Please contact PYME FINANCE support to renew license", status = "expired" });
                    }
                }
                //validate account login
                if (usr.UStatus != "ACTIVE")
                {
                    _logger.LogError($"User {users.Username} user account is not active");
                    return BadRequest(new { Message = "Account " + usr.UEmail + " is " + usr.UStatus + ". Please contact your system administrator" });
                }
                else
                {
                    //Check password
                    if (usr.UPassword == CommonMethods.ConvertToEncrypt(users.UPassword))
                    {
                        //correct validation

                        ///GET Image
                        string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "profiles");
                        string full_imgPath = Path.Combine(img_path, usr.UProfile);
                        byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        if (!closingdate.HasValue)
                        {
                            closingdate = DateTime.Now.AddDays(-1);
                        }

                        //Create my object
                        var myJsondata = new
                        {
                            Firstname = usr.UFirstName,
                            Lastname = usr.ULastName,
                            Email = usr.UEmail,
                            UserType = usr.UType,
                            Profile = base64ImageRepresentation,
                            FirstTime = usr.UFirst,
                            BranchId = usr.UBranch,
                            Status = usr.UStatus,
                            UserId = usr.UId,
                            cName = companyName,
                            Closingdate = closingdate
                        };


                        jwt_token jwtCtrl = new jwt_token(_configuration);
                        string jwtToken = jwtCtrl.GenerateJsonWebToken(usr.UId, db, usr.UType);

                        _logger.LogInformation("Class:LoginController | Method:LoginSuccess | Complete method");

                        return Ok(new
                        {
                            message = "Login account " + usr.UEmail + " successful",
                            JWT = jwtToken,
                            Company = usr.UCompany,
                            UserProfile = myJsondata,
                            UFirst = usr.UFirst,
                            permissions = userPermission
                        });

                    }
                    else
                    {

                        _logger.LogInformation("Class:LoginController | Method:LoginFailed | Complete method");
                        //incorrect password
                        return BadRequest(new { Message = "Incorrect password for account " + users.Username });
                    }

                }
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = e.Message });
            }


        }



    }
}
