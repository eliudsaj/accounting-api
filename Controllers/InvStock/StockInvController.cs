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
using pyme_finance_api.Models.Purchases.Invoices;
using pyme_finance_api.Models.Purchases.PurchaseReturn;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.StockInv;
using pyme_finance_api.Models.UserProfile;
using pyme_finance_api.Service.MailService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.InventoryService;
using System.IO;
using System.Text.RegularExpressions;

namespace pyme_finance_api.Controllers.InvStock
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockInvController : ControllerBase
    {
        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;
        private IWebHostEnvironment _hostingEnvironment;
        readonly ILogger<StockInvController> _log;

        public StockInvController(IConfiguration config, IWebHostEnvironment environment, ILogger<StockInvController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _log = logger;
        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult purchaserequest_add_get_default()
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

            //get signatures
            List<usersignature> users_Signatures = new List<usersignature>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM signatures WHERE sign_user = " + userId + " ", cnn).ExecuteReader();
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

            //get company data
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

            //get inventory data
            List<Inventory> invList = new List<Inventory>();
            cnn.Open();
            NpgsqlDataReader sdrInv = new NpgsqlCommand("SELECT \"Inventory\".*,\"VtRef\",\"VtPerc\" FROM \"Inventory\" LEFT JOIN \"VATs\" ON (\"VtId\" = \"InvtVATId\") ", cnn).ExecuteReader();
            try
            {
                while (sdrInv.Read())
                {
                    Inventory inv = new Inventory();

                    inv.InvtId = sdrInv["InvtId"] != DBNull.Value ? (int)sdrInv["InvtId"] : 0;
                    _log.LogInformation($"Id===================================={sdrInv["InvtId"]}");

                    inv.InvtType = sdrInv["InvtType"] != DBNull.Value ? (string)sdrInv["InvtType"] : null;
                    _log.LogInformation($"TYPE===================================={sdrInv["InvtType"]}");

                    inv.InvtName = sdrInv["InvtName"] != DBNull.Value ? (string)sdrInv["InvtName"] : null;
                    _log.LogInformation($"Name===================================={sdrInv["InvtType"]}");

                    inv.InvtQty = sdrInv["InvtQty"] != DBNull.Value ? (int)sdrInv["InvtQty"] : 0;
                    _log.LogInformation($"QTY===================================={(int)sdrInv["InvtQty"]}");

                    inv.InvtBP = sdrInv["InvtBP"] != DBNull.Value ? (decimal)sdrInv["InvtBP"] : 0;
                    _log.LogInformation($"bp===================================={(decimal)sdrInv["InvtBP"]}");

                    inv.InvtSP = sdrInv["InvtSP"] != DBNull.Value ? (decimal)sdrInv["InvtSP"] : 0;
                    _log.LogInformation($"SP===================================={sdrInv["InvtSP"]}");

                    inv.InvtReorderLevel = sdrInv["InvtReorderLevel"] != DBNull.Value ? (int)sdrInv["InvtReorderLevel"] : 0;
                    _log.LogInformation($"rEORDER===================================={(int)sdrInv["InvtReorderLevel"]}");

                    inv.InvtVATId = sdrInv["InvtVATId"] != DBNull.Value ? (int)sdrInv["InvtVATId"] : 0;
                    _log.LogInformation($"vatId===================================={sdrInv["InvtVATId"]}");

                    inv.VATPerc = sdrInv["VtPerc"] != DBNull.Value ? (float)sdrInv["VtPerc"] : 0;
                    _log.LogInformation($"VATPERCENTAGE===================================={(float)sdrInv["VtPerc"]}");

                    inv.VATRef = sdrInv["VtRef"] != DBNull.Value ? (string)sdrInv["VtRef"] : null;
                    _log.LogInformation($"vTFEF===================================={(string)sdrInv["VtRef"]}");

                    invList.Add(inv);
                }
                cnn.Close();

                return Ok(new { MyCompany = lic, CurrUserSignatures = users_Signatures, InventData = invList });

            }
            catch (Exception E)
            {
                return BadRequest(E.Message);
            }

        }



        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Inventory_Movement_Details(int inventoryId)
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
            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT b.\"InvtName\", a.\"openstock\",a.\"qty_issued\",a.\"qty_received\",a.\"qty_adjusted\",a.\"qty_allocated\",a.\"physical_qty\",a.\"ws_date\" " +
            "FROM warehouse_summary a LEFT JOIN public.\"Inventory\" b on b.\"InvtId\" = a.\"prod_id\" WHERE a.\"prod_id\" =" + inventoryId, cnn).ExecuteReader();
            List<InventoryMovement> inventoryMovements = new List<InventoryMovement>();
            while (sdr0.Read())
            {
                InventoryMovement sth = new InventoryMovement();


                sth.InvtName = sdr0["InvtName"] != DBNull.Value ? (string)sdr0["InvtName"] : null;
                sth.openstock = sdr0["openstock"] != DBNull.Value ? (int)sdr0["openstock"] : 0;
                sth.qty_issued = sdr0["qty_issued"] != DBNull.Value ? (int)sdr0["qty_issued"] : 0;
                sth.qty_received = sdr0["qty_received"] != DBNull.Value ? (int)sdr0["qty_received"] : 0;
                sth.qty_adjusted = sdr0["qty_adjusted"] != DBNull.Value ? (int)sdr0["qty_adjusted"] : 0;
                sth.qty_allocated = sdr0["qty_allocated"] != DBNull.Value ? (int)sdr0["qty_allocated"] : 0;
                sth.physical_qty = sdr0["physical_qty"] != DBNull.Value ? (int)sdr0["physical_qty"] : 0;
                sth.ws_date = (DateTime)sdr0["ws_date"];

                inventoryMovements.Add(sth);
            }
            cnn.Close();



            return Ok(inventoryMovements);
        }



        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult stock_take_listall()
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

            //get stock headers
            List<StockTakeHeader> stocks_header = new List<StockTakeHeader>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select stock_take_header.*, s.\"UFirstName\", s.\"ULastName\", app.\"UFirstName\" As appfname, app.\"ULastName\" As applname From stock_take_header LEFT Join \"Users\" s On s.\"UId\" = stock_take_header.sth_staff LEFT Join \"Users\" app On app.\"UId\" = stock_take_header.approved_by", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                StockTakeHeader sth = new StockTakeHeader();

                sth.sth_id = sdr0["sth_id"] != DBNull.Value ? (int)sdr0["sth_id"] : 0;
                sth.sth_date = sdr0["sth_date"] != DBNull.Value ? (DateTime)sdr0["sth_date"] : DateTime.Today;
                sth.sth_ref = sdr0["sth_ref"] != DBNull.Value ? (string)sdr0["sth_ref"] : null;
                sth.sth_name = sdr0["sth_name"] != DBNull.Value ? (string)sdr0["sth_name"] : null;
                sth.sth_staff = sdr0["sth_staff"] != DBNull.Value ? (int)sdr0["sth_staff"] : 0;
                sth.sth_approved = sdr0["sth_approved"] != DBNull.Value ? (bool)sdr0["sth_approved"] : false;
                sth.approved_by = sdr0["approved_by"] != DBNull.Value ? (int)sdr0["approved_by"] : 0;
                sth.approval_date = (sdr0["approval_date"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(sdr0["approval_date"]);
                sth.has_issue = sdr0["has_issue"] != DBNull.Value ? (bool)sdr0["has_issue"] : false;


                sth.staff_firstname = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                sth.staff_lastname = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;

                sth.approver_firstname = sdr0["appfname"] != DBNull.Value ? (string)sdr0["appfname"] : null;
                sth.approver_lastname = sdr0["applname"] != DBNull.Value ? (string)sdr0["applname"] : null;

                stocks_header.Add(sth);
            }
            cnn.Close();

            return Ok(new { StockH_data = stocks_header });

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult stock_take_add_defaults()
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

            //get stock details for goods
            List<Inventory> inv_details = new List<Inventory>();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Inventory\" WHERE \"InvtType\" = 'GOODS'  ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                Inventory inv = new Inventory();

                inv.InvtId = sdr0["InvtId"] != DBNull.Value ? (int)sdr0["InvtId"] : 0;
                inv.InvtType = sdr0["InvtType"] != DBNull.Value ? (string)sdr0["InvtType"] : null;
                inv.InvtName = sdr0["InvtName"] != DBNull.Value ? (string)sdr0["InvtName"] : null;
                inv.InvtQty = sdr0["InvtQty"] != DBNull.Value ? (int)sdr0["InvtQty"] : 0;
                inv.InvtBP = sdr0["InvtBP"] != DBNull.Value ? (decimal)sdr0["InvtBP"] : 0;
                inv.InvtSP = sdr0["InvtSP"] != DBNull.Value ? (decimal)sdr0["InvtSP"] : 0;
                inv.InvtReorderLevel = sdr0["InvtReorderLevel"] != DBNull.Value ? (int)sdr0["InvtReorderLevel"] : 0;

                inv_details.Add(inv);
            }
            cnn.Close();

            return Ok(new { InventData = inv_details });

        }






        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult getAllQuotations()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));

            var data = inventoryService.getAllQuotation();


            return Ok(data);

        }


        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult getAllDeliveryNotes()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));

            var data = inventoryService.getAllDeliveryNotes();


            return Ok(data);

        }


        private static bool IsBase64String(string base64)
        {
            try
            {
                //remove header from base 64
                string result = Regex.Replace(base64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);

                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(result);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                // return (base64.Replace(" ", "").Length % 4 == 0);
                return true;
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }


        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult DeliveryNoteDetails(int id)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));

            var data = inventoryService.getDeliveryNoteDetail(id);




            //Get all customers
            string myQuery = "SELECT \"SLCustomer\".* FROM \"SLCustomer\"  WHERE \"SLCustomerSerial\" = " + data.ActionBy + "  ";

            List<AddCustomer> customerList = new List<AddCustomer>();
            AddCustomer addCust = new AddCustomer();

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

            }

            cnn.Close();










            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr1.Read())
            {
                lic.CompanyName = (string)sdr1["CompanyName"];
                lic.CompanySlogan = (string)sdr1["CompanySlogan"];
                lic.CompanyPostal = (string)sdr1["CompanyPostal"];
                lic.CompanyContact = (string)sdr1["CompanyContact"];
                lic.CompanyVAT = (string)sdr1["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr1["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr1["CompanyLogo"];
            }

            cnn.Close();
            string img_path = "";

            //upload image if base64
            bool url_is_base64 = IsBase64String(lic.CompanyLogo);

            if (String.IsNullOrEmpty(lic.CompanyLogo))
            {
                //upload image
                //using ngenx.jpg for test purpose
                lic.CompanyLogo = "invoice_default.jpg";
                //  lic.CompanyLogo = "ngenx.jpg";

                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");



                //set the image path
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);

                //write file
                //       System.IO.File.WriteAllBytes(full_imgPath, Convert.FromBase64String(lic.CompanyLogo));

                //      img_path = rand_imageName;
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                lic.CompanyLogo = Convert.ToBase64String(imageArray); ;
            }
            else
            {
                //remove prefix


                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");


                //set the image path
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);


                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                lic.CompanyLogo = Convert.ToBase64String(imageArray); ;


            }





            return Ok(new { DeliveryNote = data, Lic = lic, Customer = addCust });

        }















        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult ApproveQuotation(ApproveQuotation recvData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            var data = inventoryService.ApproveQuotation(recvData.quotationId, recvData.userId);



            return Ok(data);

        }


        [Route("[action]")]
        [Authorize]
        [HttpGet]
        public ActionResult GetQuotationDetails(int quotationid)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            var data = inventoryService.getQuotationdetails(quotationid);



            return Ok(data);

        }


        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult RejectQuotation(ApproveQuotation recvData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));



            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            var data = inventoryService.RejectQuotation(recvData.quotationId, recvData.userId);



            return Ok(data);

        }



        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult saveDeliveryNoteNonStock(DeliveryNoteRequest recvData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            recvData.pr_createdby = userId;


            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));

            foreach (var item in recvData.pr_details)
            {
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Inventory\"  WHERE  \"InvtId\" = " + item.pd_item + " ", cnn).ExecuteReader();
                while (sdr0.Read())
                {

                    item.pd_item_name = sdr0["InvtName"] != DBNull.Value ? (string)sdr0["InvtName"] : null;

                }
                cnn.Close();


            }




            var response = inventoryService.DeliveryNotenonstock(recvData);



            return Ok(new
            {
                message = response.Message,
                code = response.Httpcode

            });

        }

















        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult saveQuotationtoCustomer(QuotationRequest recvData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            recvData.pr_createdby = userId;


            InventoryService inventoryService = new InventoryService(tokenData, _hostingEnvironment);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));

            foreach (var item in recvData.pr_details)
            {
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Inventory\"  WHERE  \"InvtId\" = " + item.pd_item + " ", cnn).ExecuteReader();
                while (sdr0.Read())
                {

                    item.pd_item_name = sdr0["InvtName"] != DBNull.Value ? (string)sdr0["InvtName"] : null;

                }
                cnn.Close();


            }




            var response = inventoryService.saveQuotation(recvData);



            return Ok(new
            {
                message = response.Message,
                code = response.Httpcode

            });

        }


        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult emailQuotationtoCustomer(QuotationRequest recvData)
        {

            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            ////permission name
            //string permissionName = Request.Headers["PermName"];


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


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));


            //get stock details for goods


            foreach (var item in recvData.pr_details)
            {
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Inventory\"  WHERE  \"InvtId\" = " + item.pd_item + " ", cnn).ExecuteReader();
                while (sdr0.Read())
                {


                    item.pd_item_name = sdr0["InvtName"] != DBNull.Value ? (string)sdr0["InvtName"] : null;
                    item.image_path = sdr0["ProdImage"] != DBNull.Value ? (string)sdr0["ProdImage"] : null;



                }
                cnn.Close();


            }
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
            if (sdr1.Read())
            {
                lic.CompanyName = (string)sdr1["CompanyName"];
                lic.CompanySlogan = (string)sdr1["CompanySlogan"];
                lic.CompanyPostal = (string)sdr1["CompanyPostal"];
                lic.CompanyContact = (string)sdr1["CompanyContact"];
                lic.CompanyVAT = (string)sdr1["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr1["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr1["CompanyLogo"];
            }
            cnn.Close();

            MailService mailService = new MailService();

            mailService.EmailCustomerQuotation(recvData.pr_details, _hostingEnvironment.WebRootPath, lic, recvData.pr_customer);





            //success
            return Ok(new
            {
                message = "Quotation has been sent successfully"

            });


        }










        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult stock_take_add_new(StockTakeHeader recvData)
        {
            //check data
            if (string.IsNullOrEmpty(recvData.sth_name))
            {
                return BadRequest(new { message = "Missing stock take save name " });

            }
            else if (recvData.take_description.Length == 0)
            {
                return BadRequest(new { message = "Missing stock details" });

            }

            //set Date
            DateTime today = DateTime.Today;


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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //get last id
            int lastSTHID = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(sth_id) as sl From stock_take_header LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastSTHID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();


            //get sth_ref guid
            string sthReference = System.Guid.NewGuid().ToString("D");

            bool details_have_issue = false;


            if (recvData.take_description.Length > 0)
            {
                for (int i = 0; i < recvData.take_description.Length; i++)
                {


                    NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                    cnn1.Open();

                    if (recvData.take_description[i].store_qty == recvData.take_description[i].curr_qty)
                    {
                        _ = details_have_issue == false;
                    }
                    else
                    {
                        _ = details_have_issue == true;
                    }

                    string insertQuery1 = "INSERT INTO stock_take_details (stk_id, stk_date, stk_item_id, stk_item_name, store_qty, curr_qty, stk_ref, stk_has_issue) VALUES(" + recvData.take_description[i].stk_id + ", '" + DateTime.Today + "', " + recvData.take_description[i].stk_item_id + ", '" + recvData.take_description[i].stk_item_name + "', " + recvData.take_description[i].store_qty + "," + recvData.take_description[i].curr_qty + ",'" + sthReference + "','" + details_have_issue + "' );";

                    bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);

                    cnn1.Close();

                    if (myReq1 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occured while trying to save details." });
                    }



                }
            }


            //set header details
            cnn.Open();
            string insertQ = "INSERT INTO stock_take_header (sth_id, sth_date, sth_ref, sth_name, sth_staff, sth_approved, has_issue) VALUES(" + (lastSTHID + 1) + ", '" + DateTime.Today + "', '" + sthReference + "', '" + recvData.sth_name + "'," + userId + ",'f','" + details_have_issue + "');";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);

            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to save details." });
            }



            //success
            return Ok(new
            {
                message = "Request has been successfully saved"

            });


        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult stock_take_report_review(string parsed_stockref)
        {

            if (string.IsNullOrEmpty(parsed_stockref))
            {
                return BadRequest(new { message = "Cannot find the required reference." });
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
                return BadRequest(new
                {
                    message =
                        "Cannot find required permission parameters. Request terminated.Security verification failed."
                });
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
                return BadRequest(new
                {
                    message =
                        "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
                });
            }

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            //get header
            StockTakeHeader stk_header = new StockTakeHeader();
            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand(
                "Select stock_take_header.*, s.\"UFirstName\", s.\"ULastName\", app.\"UFirstName\" As appfname, app.\"ULastName\" As applname, sn.sign_data, sn.sign_name, rcv.sign_data As rcvdata, rcv.sign_name As rcvname From stock_take_header Left Join \"Users\" s On s.\"UId\" = stock_take_header.sth_staff LEFT Join \"Users\" app On app.\"UId\" = stock_take_header.approved_by LEFT Join signatures sn On sn.sign_user = stock_take_header.sth_staff LEFT Join signatures rcv On rcv.sign_user = stock_take_header.approved_by WHERE sth_ref = '" +
                parsed_stockref + "' ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                return BadRequest(new { message = "Could NOT find parsed reference header details" });
            }

            while (sdr0.Read())
            {
                stk_header.sth_id = sdr0["sth_id"] != DBNull.Value ? (int)sdr0["sth_id"] : 0;
                stk_header.sth_date = (sdr0["sth_date"] == DBNull.Value)
                    ? (DateTime?)null
                    : Convert.ToDateTime(sdr0["sth_date"]);
                stk_header.sth_ref = sdr0["sth_ref"] != DBNull.Value ? (string)sdr0["sth_ref"] : null;
                stk_header.sth_name = sdr0["sth_name"] != DBNull.Value ? (string)sdr0["sth_name"] : null;
                stk_header.sth_staff = sdr0["sth_staff"] != DBNull.Value ? (int)sdr0["sth_staff"] : 0;
                stk_header.sth_approved = sdr0["sth_approved"] != DBNull.Value ? (bool)sdr0["sth_approved"] : false;
                stk_header.approved_by = sdr0["approved_by"] != DBNull.Value ? (int)sdr0["approved_by"] : 0;
                stk_header.approval_date = (sdr0["approval_date"] == DBNull.Value)
                    ? (DateTime?)null
                    : Convert.ToDateTime(sdr0["approval_date"]);
                stk_header.has_issue = sdr0["has_issue"] != DBNull.Value ? (bool)sdr0["has_issue"] : false;
                stk_header.approver_signature =
                    sdr0["approver_signature"] != DBNull.Value ? (int)sdr0["approver_signature"] : 0;

                stk_header.staff_firstname = sdr0["UFirstName"] != DBNull.Value ? (string)sdr0["UFirstName"] : null;
                stk_header.staff_lastname = sdr0["ULastName"] != DBNull.Value ? (string)sdr0["ULastName"] : null;

                stk_header.approver_firstname = sdr0["appfname"] != DBNull.Value ? (string)sdr0["appfname"] : null;
                stk_header.approver_lastname = sdr0["applname"] != DBNull.Value ? (string)sdr0["applname"] : null;

            }

            cnn.Close();

            cnn.Open();
            List<StockTakeDetails> stk_detailsList = new List<StockTakeDetails>();
            NpgsqlDataReader sdr1 =
                new NpgsqlCommand("SELECT * FROM stock_take_details  WHERE stk_ref = '" + parsed_stockref + "'  ", cnn)
                    .ExecuteReader();
            if (sdr1.HasRows == false)
            {
                return BadRequest(new { message = "Could NOT find attached details from parsed reference" });
            }

            while (sdr1.Read())
            {
                StockTakeDetails sdetails = new StockTakeDetails();

                sdetails.stk_id = sdr1["stk_id"] != DBNull.Value ? (int)sdr1["stk_id"] : 0;
                sdetails.stk_date = (sdr1["stk_date"] == DBNull.Value)
                    ? (DateTime?)null
                    : Convert.ToDateTime(sdr1["stk_date"]);
                sdetails.stk_item_id = sdr1["stk_item_id"] != DBNull.Value ? (int)sdr1["stk_item_id"] : 0;
                sdetails.stk_item_name = sdr1["stk_item_name"] != DBNull.Value ? (string)sdr1["stk_item_name"] : null;
                sdetails.store_qty = sdr1["store_qty"] != DBNull.Value ? (int)sdr1["store_qty"] : 0;
                sdetails.curr_qty = sdr1["curr_qty"] != DBNull.Value ? (int)sdr1["curr_qty"] : 0;
                sdetails.stk_ref = sdr1["stk_ref"] != DBNull.Value ? (string)sdr1["stk_ref"] : null;
                sdetails.stk_has_issue = sdr1["stk_has_issue"] != DBNull.Value ? (bool)sdr1["stk_has_issue"] : false;

                stk_detailsList.Add(sdetails);
            }

            cnn.Close();

            //get company data
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

            //permissions
            bool take_action_permission = myDbconnection.CheckRights(companyRes, "stockTakeAction", userId);
            stk_header.has_approval_permission = take_action_permission;

            //signatures
            List<usersignature> users_signature_list = new List<usersignature>();
            cnn.Open();
            NpgsqlDataReader sdr31 =
                new NpgsqlCommand("Select * From signatures where sign_user = " + userId + " ", cnn).ExecuteReader();
            while (sdr31.Read())
            {
                usersignature usg = new usersignature();
                usg.sign_id = sdr31["sign_id"] != DBNull.Value ? (int)sdr31["sign_id"] : 0;
                usg.sign_data = sdr31["sign_data"] != DBNull.Value ? (string)sdr31["sign_data"] : null;
                usg.sign_name = sdr31["sign_name"] != DBNull.Value ? (string)sdr31["sign_name"] : null;
                users_signature_list.Add(usg);

            }

            cnn.Close();

            //get approver signture if it exists
            usersignature approver_signature = new usersignature();
            if (stk_header.approver_signature != 0)
            {
                cnn.Open();
                NpgsqlDataReader sdr32 =
                    new NpgsqlCommand("Select * From signatures where sign_id = " + stk_header.approver_signature + "  ",
                        cnn).ExecuteReader();
                if (sdr32.Read())
                {
                    approver_signature.sign_name = sdr32["sign_name"] != DBNull.Value ? (string)sdr32["sign_name"] : null;
                    approver_signature.sign_data = sdr32["sign_data"] != DBNull.Value ? (string)sdr32["sign_data"] : null;
                }
                cnn.Close();
            }


            return Ok(new
            {
                Report_Stock_take_Header = stk_header,
                Report_stock_take_details = stk_detailsList,
                myCompany = lic,
                CurrUserSignatures = users_signature_list,
                approverSignatureDetails = approver_signature
            });
        }


        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult stock_take_approve(string parsed_stockref, [FromBody] StockTakeHeader rcvData)
        {

            if (string.IsNullOrEmpty(parsed_stockref))
            {
                return BadRequest(new { message = "Cannot find the required reference." });
            }
            else if (rcvData.approver_signature == 0)
            {
                return BadRequest(new { message = "Please ensure you attach a signature " });
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
                return BadRequest(new
                {
                    message =
                        "Cannot find required permission parameters. Request terminated.Security verification failed."
                });
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
                return BadRequest(new
                {
                    message =
                        "Sorry! you are denied access to this action or information. Please contact your system administrator for more information."
                });
            }

            //get database name
            string db = companyRes;


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));


            //update header
            string updHeader0 = "UPDATE stock_take_header SET approved_by = " + userId + ", approval_date = '" + DateTime.Today + "', approver_signature = " + rcvData.approver_signature + " WHERE sth_ref = '" + parsed_stockref + "' ";
            bool myReq1 = myDbconnection.UpdateDelInsert(updHeader0, db);
            if (myReq1 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process receipt header request." });
            }


            return Ok(new
            {
                message = "Request has been successfully received"
            });
        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult purchase_return_INV_details(int PLJrnlNo)
        {

            if (PLJrnlNo == 0)
            {
                return BadRequest(new { message = "Cannot find the required reference." });
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

            //get PL Header
            PLInvoice plInv = new PLInvoice();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT h.\"PLJrnlNo\", h.\"NlJrnlNo\", h.\"PLCustID\", h.\"TranDate\", h.\"Period\", h.\"DocRef\",  h.\"InvDate\", h.\"CurrencyId\", h.\"PLDescription\", h.\"StaffId\", h.\"DocPrefix\", h.\"HasCreditNote\", h.\"DueDate\", h.\"Totals\", h.\"Balance\", c.\"CrCode\", pc.\"PLCustCode\", pc.\"CustName\" " +
                "FROM \"PLInvoiceHeader\" h " +
                "INNER JOIN  \"Currencies\" c ON c.\"CrId\" = h.\"CurrencyId\"" +
                " INNER JOIN  \"PLCustomer\" pc ON pc.\"CustID\" = h.\"PLCustID\"" +
                " WHERE h.\"PLJrnlNo\" = " + PLJrnlNo + " ", cnn).ExecuteReader();
            if (sdr0.Read())
            {

                plInv.PLJrnNo = sdr0["PLJrnlNo"] != DBNull.Value ? (int)sdr0["PLJrnlNo"] : 0;
                plInv.NlJrnlNo = sdr0["NlJrnlNo"] != DBNull.Value ? (int)sdr0["NlJrnlNo"] : 0;
                plInv.PLCustID = sdr0["PLCustID"] != DBNull.Value ? (int)sdr0["PLCustID"] : 0;
                plInv.TranDate = sdr0["TranDate"] != DBNull.Value ? (DateTime)sdr0["TranDate"] : DateTime.Now;
                plInv.Period = sdr0["Period"] != DBNull.Value ? (string)sdr0["Period"] : null;
                //plInv.Year = sdr0["Year"] != DBNull.Value ? (int)sdr0["Year"] : 0;
                plInv.DocRef = sdr0["DocRef"] != DBNull.Value ? (string)sdr0["DocRef"] : "";
                plInv.InvDate = sdr0["InvDate"] != DBNull.Value ? (DateTime)sdr0["InvDate"] : DateTime.Now;
                plInv.CurrencyId = sdr0["CurrencyId"] != DBNull.Value ? (int)sdr0["CurrencyId"] : 0;
                plInv.PLDescription = sdr0["PLDescription"] != DBNull.Value ? (string)sdr0["PLDescription"] : null;
                plInv.StaffId = sdr0["StaffId"] != DBNull.Value ? (int)sdr0["StaffId"] : 0;
                plInv.DocPrefix = sdr0["DocPrefix"] != DBNull.Value ? (string)sdr0["DocPrefix"] : null;
                plInv.HasCreditNote = sdr0["HasCreditNote"] != DBNull.Value ? (bool)sdr0["HasCreditNote"] : false;
                plInv.DueDate = sdr0["DueDate"] != DBNull.Value ? (DateTime)sdr0["DueDate"] : DateTime.Today;
                plInv.Totals = sdr0["Totals"] != DBNull.Value ? (decimal)sdr0["Totals"] : 0;
                plInv.Balance = sdr0["Balance"] != DBNull.Value ? (decimal)sdr0["Balance"] : 0;

                plInv.CrCode = sdr0["CrCode"] != DBNull.Value ? (string)sdr0["CrCode"] : null;
                plInv.PLCustCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                plInv.CustName = sdr0["CustName"] != DBNull.Value ? (string)sdr0["CustName"] : null;

            }
            cnn.Close();



            cnn.Open();
            List<PLInvoiceDetails> myPLDetails = new List<PLInvoiceDetails>();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"PLInvoiceDetail\" WHERE \"PLJrnlNo\" = " + PLJrnlNo + "  ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                PLInvoiceDetails lpdet = new PLInvoiceDetails();

                lpdet.PLJrnlNo = sdr1["PLJrnlNo"] != DBNull.Value ? (int)sdr1["PLJrnlNo"] : 0;
                lpdet.JrnlPLNo = sdr1["JrnlPLNo"] != DBNull.Value ? (int)sdr1["JrnlPLNo"] : 0;
                lpdet.UnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;
                lpdet.VatPerc = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : null;
                lpdet.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                lpdet.NLAccCode = sdr1["NLAccCode"] != DBNull.Value ? (string)sdr1["NLAccCode"] : null;
                lpdet.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : null;
                lpdet.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                lpdet.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;

                myPLDetails.Add(lpdet);
            }
            cnn.Close();

            //get company data
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
            }
            cnn.Close();

            //get license details
            List<usersignature> users_signature_list = new List<usersignature>();
            cnn.Open();
            NpgsqlDataReader sdr31 =
                new NpgsqlCommand("Select * From signatures where sign_user = " + userId + " ", cnn).ExecuteReader();
            while (sdr31.Read())
            {
                usersignature usg = new usersignature();
                usg.sign_id = sdr31["sign_id"] != DBNull.Value ? (int)sdr31["sign_id"] : 0;
                usg.sign_data = sdr31["sign_data"] != DBNull.Value ? (string)sdr31["sign_data"] : null;
                usg.sign_name = sdr31["sign_name"] != DBNull.Value ? (string)sdr31["sign_name"] : null;
                users_signature_list.Add(usg);

            }

            return Ok(new { PLHeader = plInv, PLDetails = myPLDetails, myCompany = lic, CurrUserSignatures = users_signature_list });

        }

        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult stock_take_return(int parsed_pljrn_no, [FromBody] PLInvoice recvData)
        {
            //check data
            if (recvData.ReturnerSignature == 0)
            {
                return BadRequest(new { message = "Missing signature details" });

            }
            else if (recvData.pl_inv_details.Length == 0)
            {
                return BadRequest(new { message = "Missing attached invoice details" });

            }

            //set Date
            DateTime today = DateTime.Today;


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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            int count_checked = 0;
            //check if any details is checked
            foreach (var j in recvData.pl_inv_details)
            {
                //convert edited qty to int
                j.ProdQty = Int32.Parse(j.EditQty);
                //count on
                if (j.chk == "on")
                {
                    count_checked += 1;
                }
                else if (j.chk == "on" && string.IsNullOrEmpty(j.ReturnReason))
                {
                    return BadRequest(new { message = "Sorry! please state the reason as to why you are returning " + j.StkDesc });
                }

            }

            if (count_checked == 0)
            {
                return BadRequest(new { message = "Please ensure you have some items checked in your list to continue. You have " + count_checked + " item(s) checked" });
            }

            //send email


            //insert PL inv details
            foreach (var i in recvData.pl_inv_details)
            {
                NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn1.Open();
                if (i.chk == "on")
                {
                    string det_ref = System.Guid.NewGuid().ToString("D");
                    string insertQuery1 = "INSERT INTO purchase_return_details (pr_ref,pr_pl_invref,pr_item_name,pr_item_qty,pr_reason) VALUES ('" + det_ref + "'," + parsed_pljrn_no + ",'" + i.StkDesc + "', " + i.ProdQty + ",'" + i.ReturnReason + "' ) ";
                    bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, db);
                    cnn1.Close();
                    if (myReq1 == false)
                    {
                        //failed
                        return BadRequest(new { message = "An occured while trying to save details." });
                    }
                }

            }

            string hdr_ref = System.Guid.NewGuid().ToString("D");

            //insert header details
            cnn.Open();
            string insertQ = "INSERT INTO purchase_return_header (prh_ref,prh_date,prh_pljrnl,returnedby,returner_signature,approvedby,approver_signature,status,prh_staff) VALUES ('" + hdr_ref + "','" + DateTime.Today + "'," + parsed_pljrn_no + ", " + userId + "," + recvData.ReturnerSignature + "," + 0 + "," + 0 + ",'Pending Approval'," + userId + " ) ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to save details." });
            }

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }

        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult stock_take_action_approve_decline(int parsed_pljrn_no, [FromBody] PurchaseReturnHeader recvData)
        {
            //check data
            if (parsed_pljrn_no == 0)
            {
                return BadRequest(new { message = "Missing required reference details" });

            }
            else if (recvData.approver_signature == 0)
            {
                return BadRequest(new { message = "Cannot find the approved signature to be attached" });
            }
            else if (recvData.status == "")
            {
                return BadRequest(new { message = "Cannot find the request status" });
            }

            //set Date
            DateTime today = DateTime.Today;


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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            string hdr_ref = System.Guid.NewGuid().ToString("D");

            //update purchase return header
            cnn.Open();
            string updtQ = "UPDATE purchase_return_header SET approvedby = " + userId + ", approver_signature = " + recvData.approver_signature + ", status = '" + recvData.status + "' WHERE prh_pljrnl = " + parsed_pljrn_no + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(updtQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to update details." });
            }

            //update PL Invoice
            cnn.Open();
            string updt1Q = "UPDATE \"PLInvoiceHeader\" SET \"ReturnStatus\" = '" + recvData.status + "' WHERE \"PLJrnlNo\" = " + parsed_pljrn_no + " ";
            bool myReq3 = myDbconnection.UpdateDelInsert(updt1Q, db);
            cnn.Close();
            if (myReq3 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to update PL details." });
            }

            //get all items in details
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM purchase_return_details WHERE pr_pl_invref = " + parsed_pljrn_no + "  ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                //update inventory
                NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(db));
                cnn1.Open();
                string updt2Q = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " + sdr1["pr_item_qty"] + " WHERE \"InvtName\" = '" + sdr1["pr_item_name"] + "' ";
                bool myReq5 = myDbconnection.UpdateDelInsert(updt2Q, db);
                cnn1.Close();
                if (myReq5 == false)
                {
                    //failed
                    return BadRequest(new { message = "An occured while trying to update Inventory details." });
                }

            }
            cnn.Close();

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }


    }
}
