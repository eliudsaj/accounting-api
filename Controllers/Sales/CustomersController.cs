using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.Dashboard;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.ReportPages.Sales;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.VatService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        readonly ILogger<CustomersController> _log;
        public CustomersController(IConfiguration config, ILogger<CustomersController> logger)
        {
            _configuration = config;
            _log = logger;

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult CustomerActivity( int cust_id)

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


            //get database name
            string db = companyRes;

            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }



            return null;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult ListDefaults()
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

            
            //get database name
            string db = companyRes;

            //create connection
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }

            _log.LogInformation("fetching Currencies");
            //Get all currencies
            List<Currencies> currencyList = new List<Currencies>();

            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Currencies\" ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                Currencies curr = new Currencies();

                curr.CrId = sdr0["CrId"] != DBNull.Value ? (int)sdr0["CrId"] : 0;
                curr.CrName = sdr0["CrName"] != DBNull.Value ? (string)sdr0["CrName"] : null;
                curr.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                curr.CrCountry = sdr0["CrCountry"] != DBNull.Value ? (string)sdr0["CrCountry"] : null;
                curr.CrStatus = sdr0["CrStatus"] != DBNull.Value ? (string)sdr0["CrStatus"] : null;
                curr.CrCreatedDate = sdr0["CrCreatedDate"] != DBNull.Value ? (DateTime)sdr0["CrCreatedDate"] : DateTime.Now;
                curr.CrModifiedDate = sdr0["CrModifiedDate"] != DBNull.Value ? (DateTime)sdr0["CrModifiedDate"] : DateTime.Now;
                
                currencyList.Add(curr);
            }
            cnn.Close();

            
            //count currency list
            var count = currencyList.Count();

            //Get all customer types          
            _log.LogInformation("fetching Customer Types");
            List<CustomerTypes> customerTypes_List = new List<CustomerTypes>();

            cnn.Open();
            NpgsqlDataReader sdr_ct = new NpgsqlCommand("SELECT * FROM \"SLCustomerTypes\" ", cnn).ExecuteReader();
            while (sdr_ct.Read())
            {
                CustomerTypes ct = new CustomerTypes();

                ct.SLCTypeID = sdr_ct["SLCTypeID"] != DBNull.Value ? (int)sdr_ct["SLCTypeID"] : 0;
                ct.TypeName = sdr_ct["TypeName"] != DBNull.Value ? (string)sdr_ct["TypeName"] : null;

                customerTypes_List.Add(ct);
            }
            cnn.Close();


            _log.LogInformation("fetching Customers");
            //Get all customers
            string myQuery = "SELECT \"SLCustomer\".*,\"CrId\",\"CrCode\" FROM \"SLCustomer\" LEFT JOIN \"Currencies\" ON ( \"CurCode\" = \"CrId\") WHERE \"CustBranch\" = " + staff_branch + "  ";

            List<AddCustomer> customerList = new List<AddCustomer>();
           
            cnn.Open();

            NpgsqlDataReader sdr = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr.Read())
            {
                AddCustomer addCust = new AddCustomer();

                addCust.SLCustomerSerial = (int)sdr["SLCustomerSerial"];
                addCust.CustCode = sdr["CustCode"].ToString();
                addCust.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                addCust.Address = sdr["Address"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.CurCode = (int)sdr["CurCode"];
                addCust.CustEmail = sdr["CustEmail"].ToString();
                addCust.CustContact = sdr["CustContact"].ToString();
                addCust.SLCTypeID = (int)sdr["SLCTypeID"];
                addCust.CustLastName = sdr["CustLastName"] != DBNull.Value ? (string)sdr["CustLastName"] : null;
                addCust.CustType = sdr["CustType"].ToString();
                addCust.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                addCust.VATNo = sdr["VATNo"] != DBNull.Value ? (string)sdr["VATNo"] : null;
                addCust.CustCreditLimit = (float)sdr["CustCreditLimit"];
                addCust.VATpin = sdr["VATpin"].ToString();
                addCust.CreditTerms = (int)sdr["CreditTerms"];
                addCust.CurrCode = sdr["CrCode"].ToString();
                addCust.CustBranch = sdr["CustBranch"] != DBNull.Value ? (int)sdr["CustBranch"] : 0;
                addCust.CustRef = sdr["CustRef"] != DBNull.Value ? (string)sdr["CustRef"] : null;
                addCust.CustomerDept = sdr["CustomerDept"] != DBNull.Value ? (decimal)sdr["CustomerDept"] : 0;
                addCust.Status = sdr["Status"].ToString();
                addCust.OpeningBalance = sdr["OpeningBalance"] != DBNull.Value ? (decimal)sdr["OpeningBalance"] : 0;
                addCust.HasOpeningBalance = (bool)sdr["HasOpeningBalance"];
                addCust.OpeningBalanceDate = (DateTime?)(sdr["OpeningBalanceDate"] != DBNull.Value ? (DateTime?)sdr["OpeningBalanceDate"] : null);
                customerList.Add(addCust);
            }

            cnn.Close();

            string inventoryquery = "SELECT  COUNT(*) FROM \"Inventory\"  ";
            int inventorycount = 0;


            cnn.Open();
            NpgsqlDataReader sdr21 = new NpgsqlCommand(inventoryquery, cnn).ExecuteReader();

            while (sdr21.Read())
            {
                inventorycount = (int)(long)sdr21["count"];
            }
            cnn.Dispose();
            cnn.Close();


            //Get All customers
            return Ok(new { CurrencyCount = count, CurrencyData = currencyList, CustomerTypeData = customerTypes_List, CustomersListData = customerList, InventoryCount = inventorycount });


        }

        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateCustomer(AddCustomer customer)
        {
            //check customer type
            if (string.IsNullOrEmpty(customer.CustType))
            {
                return BadRequest(new { message = "Missing customer type" });
            }

            _log.LogInformation($"Creating Customer  {customer.CustFirstName} ");
            //validate personal details
            if (customer.CustType == "Personal")
            {
                //validate
                if (string.IsNullOrEmpty(customer.CustFirstName))
                {

                    _log.LogError($" Customer firstname is missing cant complete request ");
                    return BadRequest(new { message = "Missing customer firstname" });
                }
                else if (string.IsNullOrEmpty(customer.CustLastName))
                {
                    _log.LogError($" Customer lastname is missing cant complete request ");
                    return BadRequest(new { message = "Missing customer lastname" });
                }
                //else if (string.IsNullOrEmpty(customer.CustEmail))
                //{
                //    _log.LogError($" Customer email  is missing cant complete request ");
                //    return BadRequest(new { message = "Missing customer email" });
                //}
                else if (string.IsNullOrEmpty(customer.CustContact))
                {
                    _log.LogError($" Customer contact is missing cant complete request ");
                    return BadRequest(new { message = "Missing customer contact" });
                }
                else if (string.IsNullOrEmpty(customer.Address))
                {
                    _log.LogError($" Customer address is missing cant complete request ");
                    return BadRequest(new { message = "Missing customer physical address" });
                }
                //else if (string.IsNullOrEmpty(customer.PostalAddress))
                //{
                //    return BadRequest(new { message = "Missing customer postal address" });
                //}
                //else if (string.IsNullOrEmpty(customer.SLCTypeID.ToString()))
                //{
                //    return BadRequest(new { message = "Missing customer SLC type" });
                //}
                //else if (string.IsNullOrEmpty(customer.VATpin))
                //{
                //    _log.LogError($" Customer vat/pin is missing cant complete request ");
                //    return BadRequest(new { message = "Missing customer VAT pin" });
                //}

            }
            else if (customer.CustType == "Business")
            {
                //validate
                if (string.IsNullOrEmpty(customer.CustCompany))
                {
                    return BadRequest(new { message = "Missing company/business name" });
                }

                //else if (string.IsNullOrEmpty(customer.CustEmail))
                //{
                //    return BadRequest(new { message = "Missing customer email" });
                //}
                else if (string.IsNullOrEmpty(customer.CustContact))
                {
                    return BadRequest(new { message = "Missing customer contact" });
                }
                else if (string.IsNullOrEmpty(customer.Address))
                {
                    return BadRequest(new { message = "Missing customer physical address" });
                }
                //else if (string.IsNullOrEmpty(customer.PostalAddress))
                //{
                //    return BadRequest(new { message = "Missing customer postal address" });
                //}
                //else if (string.IsNullOrEmpty(customer.SLCTypeID.ToString()))
                //{
                //    return BadRequest(new { message = "Missing customer SLC type" });
                //}
                else if (string.IsNullOrEmpty(customer.CurCode.ToString()))
                {
                    return BadRequest(new { message = "Missing customer currency code" });
                }

                else if (string.IsNullOrEmpty(customer.VATpin))
                {
                    return BadRequest(new { message = "Missing customer VAT PIN" });
                }
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

          
            //Get customer serial
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            int lastCustserial = 0;
            cnn.Open();
            NpgsqlDataReader sdrR = new NpgsqlCommand("Select MAX(\"SLCustomerSerial\") as sj From \"SLCustomer\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrR.Read())
            {
                lastCustserial = sdrR["sj"] != DBNull.Value ? (int)sdrR["sj"] : 0;

            }
            cnn.Close();

            //create customer code

            customer.CustCode = "CUST" + ((lastCustserial + 1).ToString("0000"));
            customer.Status = "Active";

            string customer_reference = System.Guid.NewGuid().ToString("D"); ;
       
            //check if cust email exists
            if (!String.IsNullOrEmpty(customer.CustEmail))
            {
                _log.LogInformation($" Checking if customer email  {customer.CustEmail} exist ");
                string check_query = "SELECT * FROM \"SLCustomer\" WHERE \"CustEmail\" = '" + customer.CustEmail + "' ";
                int res_count = myDbconnection.CheckRowExists(check_query, db);
                if (res_count > 0)
                {
                    return BadRequest(new { message = "The customer email " + customer.CustEmail.ToLower() + " is already registered by another customer account" });
                }

            }
        

            //string checkifkrapinexist = "SELECT * FROM \"SLCustomer\" WHERE \"VATpin\" = '" + customer.VATpin + "' ";

            ////get staff branch
            //int pincount = myDbconnection.CheckRowExists(checkifkrapinexist, db);

            //if(pincount > 0)
            //{
            //    return BadRequest(new { message = "A customer with KRA PIN "+customer.VATpin+ "already exists"});
            //}




            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            _log.LogInformation($"Saving customer  {customer.CustFirstName}  {customer.CustLastName} or{customer.CustCompany} details");

            //insert customer
            string inst_customer = "INSERT INTO \"SLCustomer\"(\"SLCustomerSerial\", \"CustCode\", \"CustFirstName\", \"Address\", \"PostalAddress\", \"PostalCode\", \"CurCode\", \"CustEmail\", \"CustContact\", \"SLCTypeID\", \"CustLastName\", \"CustType\", \"CustCompany\", \"VATNo\", \"CustCreditLimit\", \"VATpin\", \"CreditTerms\", \"Status\", \"CustRef\", \"CustBranch\", \"CustomerDept\" ,\"HasOpeningBalance\")" +
                " VALUES ( "+(lastCustserial+1)+", '"+customer.CustCode+ "', '" + customer.CustFirstName + "', '" + customer.Address + "', '" + customer.PostalAddress + "', '" + null + "', " + customer.CurCode + ", '" + customer.CustEmail + "','" + customer.CustContact + "'," + customer.SLCTypeID + ", '" + customer.CustLastName + "','" + customer.CustType + "', '" + customer.CustCompany + "', '" + customer.VATNo + "', " + customer.CustCreditLimit + ", '" + customer.VATpin + "', " + customer.CreditTerms + ", 'Active', '" + customer_reference + "', " + staff_branch + ","+0+","+false+" );";

            bool inst_res = myDbconnection.UpdateDelInsert(inst_customer, db);
            if (inst_res == false)
            {
                return BadRequest(new { message = $"Sorry! An error occurred while trying to create a new {customer.CustFirstName}  {customer.CustLastName} or{customer.CustCompany}. Please contact support for more details." });
            }


            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Customer "+customer.CustFirstName+" is created successfully  ";
            auditTrail.module = "Invoice";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);




            _log.LogInformation($" Customer {customer.CustFirstName}  {customer.CustLastName} or{customer.CustCompany} is created successfully ");
            return Ok(new
            {
                message = "Customer has been successfully registered."
            });


        }


        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult viewCustomerActivity(int custId,DateTime from, DateTime to)
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

            VatAnalysisService vatAnalysisService = new VatAnalysisService(tokenData);
            var data = vatAnalysisService.CustomerActivityForSales(custId,from,to);
            NlService nlService = new NlService(tokenData);
            var customer = nlService.getCustomerById(custId);
            var invBal = nlService.GetInvoiceBalanceBroughtForward(custId, from);
            var payBal = nlService.GetPaymentBalance(custId, from);
            var crnBal = nlService.GetCRNBalance(custId, from);
            var reversalBal = nlService.GetReversalBalance(custId, from);
            var lic = nlService.GetCompanyDetail();            
            return Ok(new { customerledgercard = data, companylicense = lic, customerinfo = customer, infoCustomer = customer, BalanceData = invBal, PayBalanceData = payBal, CrnBalanceData = crnBal, ReversalBalanceData = reversalBal });
        }

        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult setCustomerOnHold(int custId)
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




            string status = "Onhold";

          string  cust_upd = "UPDATE \"SLCustomer\" SET \"Status\" = '" + status + "'  WHERE  \"SLCustomerSerial\" = '" + custId + "' ;    ";

            string myQuery = "SELECT \"SLCustomer\".*,\"CrId\",\"CrCode\" FROM \"SLCustomer\" LEFT JOIN \"Currencies\" ON ( \"CurCode\" = \"CrId\") WHERE \"SLCustomerSerial\" = " + custId + "  ";

            AddCustomer addCust = new AddCustomer();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            cnn.Open();

            NpgsqlDataReader sdr = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr.Read())
            {
             

                addCust.SLCustomerSerial = (int)sdr["SLCustomerSerial"];
                addCust.CustCode = sdr["CustCode"].ToString();
                addCust.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? (string)sdr["CustFirstName"] : null;
                addCust.Address = sdr["Address"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.PostalAddress = sdr["PostalAddress"].ToString();
                addCust.CurCode = (int)sdr["CurCode"];
                addCust.CustEmail = sdr["CustEmail"].ToString();
                addCust.CustContact = sdr["CustContact"].ToString();
                addCust.SLCTypeID = (int)sdr["SLCTypeID"];
                addCust.CustLastName = sdr["CustLastName"] != DBNull.Value ? (string)sdr["CustLastName"] : null;
                addCust.CustType = sdr["CustType"].ToString();
                addCust.CustCompany = sdr["CustCompany"] != DBNull.Value ? (string)sdr["CustCompany"] : null;
                addCust.VATNo = sdr["VATNo"] != DBNull.Value ? (string)sdr["VATNo"] : null;
                addCust.CustCreditLimit = (float)sdr["CustCreditLimit"];
                addCust.VATpin = sdr["VATpin"].ToString();
                addCust.CreditTerms = (int)sdr["CreditTerms"];
                addCust.CurrCode = sdr["CrCode"].ToString();
                addCust.CustBranch = sdr["CustBranch"] != DBNull.Value ? (int)sdr["CustBranch"] : 0;
                addCust.CustRef = sdr["CustRef"] != DBNull.Value ? (string)sdr["CustRef"] : null;
                addCust.CustomerDept = sdr["CustomerDept"] != DBNull.Value ? (decimal)sdr["CustomerDept"] : 0;
                addCust.Status = sdr["Status"].ToString();

       
            }

            cnn.Close();
            if(addCust.Status == status)
            {
                return Ok(new
                {
                    message = "This customer account is already on hold",
                    status = 400
                });
            }

            bool upd_res = myDbconnection.UpdateDelInsert(cust_upd, companyRes);
            if (upd_res == false)
            {
                return Ok(new { message = "Sorry an error occured please try again later.", status = 400 });
            }

            string resMessage = addCust.CustType != "Business" ? $"{addCust.CustFirstName} {addCust.CustFirstName} account has been set on hold" : $"{addCust.CustCompany} account has been set on hold";



            return Ok(new { message = resMessage, status = 200 });

        }

        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult UpdateCustomer(AddCustomer customer)
        {
            //check customer type
            if (string.IsNullOrEmpty(customer.CustType))
            {
                return BadRequest(new { message = "Missing customer type" });
            }
            _log.LogInformation($" Customer {customer.CustFirstName}  details is being updated ");
            //validate personal details
            if (customer.CustType == "Personal")
            {
                //validate
                if (string.IsNullOrEmpty(customer.CustCode))
                {
                    return BadRequest(new { message = "Missing customer code" });
                }
                else if (string.IsNullOrEmpty(customer.CustFirstName))
                {
                    return BadRequest(new { message = "Missing customer firstname" });
                }
                else if (string.IsNullOrEmpty(customer.CustLastName))
                {
                    return BadRequest(new { message = "Missing customer lastname" });
                }
                else if (string.IsNullOrEmpty(customer.CustEmail))
                {
                    return BadRequest(new { message = "Missing customer email" });
                }
                else if (string.IsNullOrEmpty(customer.CustContact))
                {
                    return BadRequest(new { message = "Missing customer contact" });
                }
                else if (string.IsNullOrEmpty(customer.Address))
                {
                    return BadRequest(new { message = "Missing customer physical address" });
                }
                //else if (string.IsNullOrEmpty(customer.PostalAddress))
                //{
                //    return BadRequest(new { message = "Missing customer postal address" });
                //}
                else if (string.IsNullOrEmpty(customer.SLCTypeID.ToString()))
                {
                    return BadRequest(new { message = "Missing customer SLC type" });
                }
                else if (string.IsNullOrEmpty(customer.CurCode.ToString()))
                {
                    return BadRequest(new { message = "Missing customer currency code" });
                }
                //else if (string.IsNullOrEmpty(customer.VATNo))
                //{
                //    return BadRequest(new { message = "Missing customer VAT number" });
                //}
                else if (string.IsNullOrEmpty(customer.VATpin.ToString()))
                {
                    return BadRequest(new { message = "Missing VAT PIN  " });
                }

            }
            else if (customer.CustType == "Business")
            {
                //validate
                if (string.IsNullOrEmpty(customer.CustCompany))
                {
                    return BadRequest(new { message = "Missing company/business name" });
                }
                else if (string.IsNullOrEmpty(customer.CustCode))
                {
                    return BadRequest(new { message = "Missing customer code" });
                }

                //else if (string.IsNullOrEmpty(customer.CustEmail))
                //{
                //    return BadRequest(new { message = "Missing customer email" });
                //}
                else if (string.IsNullOrEmpty(customer.CustContact))
                {
                    return BadRequest(new { message = "Missing customer contact" });
                }
                else if (string.IsNullOrEmpty(customer.Address))
                {
                    return BadRequest(new { message = "Missing customer physical address" });
                }
                //else if (string.IsNullOrEmpty(customer.PostalAddress))
                //{
                //    return BadRequest(new { message = "Missing customer postal address" });
                //}
                else if (string.IsNullOrEmpty(customer.SLCTypeID.ToString()))
                {
                    return BadRequest(new { message = "Missing customer SLC type" });
                }
                else if (string.IsNullOrEmpty(customer.CurCode.ToString()))
                {
                    return BadRequest(new { message = "Missing customer currency code" });
                }
                else if (string.IsNullOrEmpty(customer.VATpin.ToString()))
                {
                    return BadRequest(new { message = "Missing VAT PIN  " });
                }
                //else if (string.IsNullOrEmpty(customer.VATNo))
                //{
                //    return BadRequest(new { message = "Missing customer VAT number" });
                //}
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
            _log.LogInformation($" Checking if customer with code {customer.CustCode} exists ");
            //check if customer code exists
            string check_query = "SELECT * FROM \"SLCustomer\" WHERE \"CustCode\" = '" + customer.CustCode + "' ";
            int res_count = myDbconnection.CheckRowExists(check_query, db);
            if (res_count == 0)
            {
                return BadRequest(new { message = "No customer exists with the code '<span class='text-warning'>" + customer.CustCode + "</span>'. Request cancelled." });
            }
            

            string cust_upd = null;

            //assign personal account
            if (customer.CustType == "Personal")
            {
                _log.LogInformation($" Customer {customer.CustFirstName}  details is being updated ");
                cust_upd = "UPDATE \"SLCustomer\" SET \"CustFirstName\" = '" + customer.CustFirstName + "'," +
                    "\"CustLastName\" = '"+customer.CustLastName+ "', \"Address\" = '"+customer.Address+ "',  \"CreditTerms\" ='"+customer.CreditTerms+"', " +
                    "\"PostalAddress\" = '"+customer.PostalAddress+ "',\"CurCode\" = "+customer.CurCode+ ",\"CustCreditLimit\" = "+customer.CustCreditLimit+" ," +
                    "\"VATNo\" = '"+customer.VATNo+ "',\"CustEmail\" = '"+customer.CustEmail+ "',\"CustContact\" = '"+customer.CustContact+ "', \"VATpin\" = '"+customer.VATpin+"',    " +
                    "\"SLCTypeID\" = "+customer.SLCTypeID+ ",\"CustType\" = '"+customer.CustType+ "',\"CustCompany\" = '"+null+ "'  WHERE  \"CustCode\" = '" + customer.CustCode + "' ;    ";

            }
            //assign business account
            else if (customer.CustType == "Business")
            {
                _log.LogInformation($" Customer {customer.CustFirstName}  details is being updated ");
                cust_upd = "UPDATE \"SLCustomer\" SET \"CustFirstName\" = '" + null + "',\"CustLastName\" = '" + null + "',\"Address\" = '" + customer.Address + "',  \"CreditTerms\" ='" + customer.CreditTerms + "'," +
                    "\"PostalAddress\" = '" + customer.PostalAddress + "',\"CurCode\" = " + customer.CurCode + ",\"VATNo\" = '" + customer.VATNo + "', \"CustCreditLimit\" = " + customer.CustCreditLimit + " , " +
                    "\"CustEmail\" = '" + customer.CustEmail + "',\"CustContact\" = '" + customer.CustContact + "',\"SLCTypeID\" = " + customer.SLCTypeID + ",\"VATpin\" = '"+customer.VATpin+"', " +
                    "\"CustType\" = '" + customer.CustType + "',\"CustCompany\" = '" + customer.CustCompany + "' WHERE  \"CustCode\" = '"+customer.CustCode+"' ;   ";

                
            }

            //update query
            bool upd_res = myDbconnection.UpdateDelInsert(cust_upd, db);
            if (upd_res == false)
            {
                return BadRequest(new { message = "Sorry NO changes were made." });
            }


            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Customer " + customer.CustFirstName + " is updated successfully  ";
            auditTrail.module = "Invoice";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);




            _log.LogInformation($" Customer {customer.CustFirstName} is updated successfully ");



            return Ok(new
            {
                message = "Customer has been successfully Updated.",
                status = 200
            });


        }



    }

}
