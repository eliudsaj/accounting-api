using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Npgsql;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Settings;
using pyme_finance_api.Models.StockInv;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.Purchases.Customers;
using pyme_finance_api.Models.ReusableCodes;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.PlService;
using pyme_finance_api.Service.InventoryService;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Common;
using pyme_finance_api.Service.MeasureofUnitService;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<InventoryController> _logger;
        public InventoryController(IConfiguration config, IWebHostEnvironment environment, ILogger<InventoryController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _logger = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult inventory_save_get_default()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _logger.LogInformation($"Fetching Inventory");
            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            //permission name
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }
            _logger.LogInformation($"Fetching InventoryCategory data");
            //get categories
            List<InventoryCategory> categories_list = new List<InventoryCategory>();
            cnn.Open();
            string query = "SELECT inventory_category.*, \"BrName\" FROM inventory_category LEFT JOIN \"Branches\" ON (\"BrId\" = cat_branch) WHERE cat_branch = " + staff_branch + " ORDER BY cat_id DESC";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                InventoryCategory invct = new InventoryCategory();
                invct.cat_id = sdr0["cat_id"] != DBNull.Value ? (int)sdr0["cat_id"] : 0;
                invct.cat_entry_date = sdr0["cat_entry_date"] != DBNull.Value ? (DateTime)sdr0["cat_entry_date"] : DateTime.Today;
                invct.cat_name = sdr0["cat_name"] != DBNull.Value ? (string)sdr0["cat_name"] : null;
                invct.cat_ref = sdr0["cat_ref"] != DBNull.Value ? (string)sdr0["cat_ref"] : null;
                invct.cat_branch = sdr0["cat_branch"] != DBNull.Value ? (int)sdr0["cat_branch"] : 0;
                invct.branch_name = sdr0["BrName"] != DBNull.Value ? (string)sdr0["BrName"] : null;
                categories_list.Add(invct);
            }
            cnn.Close();
            _logger.LogInformation($"Fetching VAT data");
            //get VAT data
            List<TaxSetup> vatList = new List<TaxSetup>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"VATs\" WHERE \"VtActive\" = 't' AND \"VtBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                TaxSetup tx = new TaxSetup();
                tx.VtId = sdr1["VtId"] != DBNull.Value ? (int)sdr1["VtId"] : 0;
                tx.VtRef = sdr1["VtRef"] != DBNull.Value ? (string)sdr1["VtRef"] : null;
                tx.VtPerc = sdr1["VtPerc"] != DBNull.Value ? (float)sdr1["VtPerc"] : 0;
                tx.VtActive = sdr1["VtActive"] != DBNull.Value ? (bool)sdr1["VtActive"] : false;
                vatList.Add(tx);
            }
            cnn.Close();
            _logger.LogInformation($"Fetching Suppliers  data");
            //suppliers
            List<PLCustomer> plcustomerList = new List<PLCustomer>();
            cnn.Open();
            NpgsqlDataReader sdr_pl = new NpgsqlCommand("Select * From \"PLCustomer\" WHERE \"CustBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr_pl.Read())
            {
                PLCustomer pl = new PLCustomer();
                pl.PLCustCode = sdr_pl["PLCustCode"] != DBNull.Value ? (string)sdr_pl["PLCustCode"] : null;
                pl.CustName = sdr_pl["CustName"] != DBNull.Value ? (string)sdr_pl["CustName"] : null;
                pl.PhysicalAddress = sdr_pl["PhysicalAddress"] != DBNull.Value ? (string)sdr_pl["PhysicalAddress"] : null;
                pl.PostalAddress = sdr_pl["PostalAddress"] != DBNull.Value ? (string)sdr_pl["PostalAddress"] : null;
                pl.CurrID = sdr_pl["CurrID"] != DBNull.Value ? (int)sdr_pl["CurrID"] : 0;
                pl.VATNo = sdr_pl["VATNo"] != DBNull.Value ? (string)sdr_pl["VATNo"] : null;
                pl.CustID = sdr_pl["CustID"] != DBNull.Value ? (int)sdr_pl["CustID"] : 0;
                pl.RegisterDate = sdr_pl["RegisterDate"] != DBNull.Value ? (DateTime)sdr_pl["RegisterDate"] : DateTime.Now;
                pl.StaffID = sdr_pl["StaffID"] != DBNull.Value ? (int)sdr_pl["StaffID"] : 0;
                pl.CustBranch = sdr_pl["CustBranch"] != DBNull.Value ? (int)sdr_pl["CustBranch"] : 0;
                plcustomerList.Add(pl);
            }
            cnn.Close();
            _logger.LogInformation($"Fetching Discounts data");
            //Get all discounts not expired
            List<Discounts> discList = new List<Discounts>();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Discounts\" WHERE \"DBranch\" = " + staff_branch + " AND \"DEndDate\" >= '" + DateTime.Now + "'  ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                Discounts disc = new Discounts();
                disc.DId = (int)sdr3["DId"];
                disc.DRef = (string)sdr3["DRef"];
                disc.DPerc = (float)sdr3["DPerc"];
                discList.Add(disc);
            }
            cnn.Close();
            _logger.LogInformation($"Fetching Warehouses data");
            //get warehouse data
            List<Warehouse> warehouseList = new List<Warehouse>();
            cnn.Open();
            NpgsqlDataReader sdr_wh = new NpgsqlCommand("Select * From warehouses WHERE wh_branch = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr_wh.Read())
            {
                Warehouse wh = new Warehouse();
                wh.wh_ref = sdr_wh["wh_ref"] != DBNull.Value ? (string)sdr_wh["wh_ref"] : null;
                wh.wh_code = sdr_wh["wh_code"] != DBNull.Value ? (string)sdr_wh["wh_code"] : null;
                wh.wh_desc = sdr_wh["wh_desc"] != DBNull.Value ? (string)sdr_wh["wh_desc"] : null;
                warehouseList.Add(wh);
            }
            cnn.Close();
            //check if user  can manage categories
            bool mngcategories = myDbconnection.CheckRights(companyRes, "ManageCategories", userId);
            _logger.LogInformation($"Fetching Units data");
            NlService nlService = new NlService(companyRes);
            PlService plService = new PlService(companyRes);
            MeasureofUnitService measureofUnitService = new MeasureofUnitService(companyRes);
            var unitsofmeasure = measureofUnitService.listofUnitofMeasure();
            var sLAnalysisCodes = nlService.GetSlanalysisCodes();
            var plcodes = plService.GetPlanalysisCodes();
            return Ok(new
            {
                VATData = vatList,
                CategoriesList = categories_list,
                ManageCategories = mngcategories,
                SuppliersList = plcustomerList,
                DiscountsList = discList,
                UnitofMeasureList = unitsofmeasure,
                Warehouses = warehouseList,
                SLAnalysisCodesList = sLAnalysisCodes,
                PLAnalysisCodes = plcodes
            });
        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult inventory_save_category(InventoryCategory recvData)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _logger.LogInformation($"Saving category {recvData.cat_name}");
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }
            //get last category id
            int last_cat_ID = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(cat_id) as sl From inventory_category LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_cat_ID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();
            //get guid
            string catRef = System.Guid.NewGuid().ToString("D");
            // create category
            _logger.LogInformation($"Saving Category {recvData.cat_name} ");
            cnn.Open();
            string insertQ = "INSERT INTO inventory_category (cat_id, cat_entry_date, cat_name, cat_ref, cat_branch) VALUES(" + (last_cat_ID + 1) + ", '" + DateTime.Today + "', '" + recvData.cat_name + "', '" + catRef + "' ," + staff_branch + " ); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                _logger.LogError($"An occurred while trying to save invoice details.");
                //failed
                return BadRequest(new { message = "An occurred while trying to save invoice details." });
            }
            recvData.cat_id = last_cat_ID;
            _logger.LogInformation($"Category {recvData.cat_name} has been added successfully");
            //success
            return Ok(new
            {
                message = "Request has been successfully processed",
                data = recvData
            });
        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult inventory_update_category(int categoryid, [FromBody] InventoryCategory recvData)
        {
            if (categoryid == 0)
            {
                return BadRequest(new { message = "Cannot find required category reference" });
            }
            else if (string.IsNullOrEmpty(recvData.cat_name))
            {
                return BadRequest(new { message = "Cannot find required category name" });
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }
            // update category
            _logger.LogInformation($"Updating Category {recvData.cat_name} ");
            cnn.Open();
            string insertQ = "UPDATE inventory_category SET cat_name = '" + recvData.cat_name + "' WHERE cat_id = " + categoryid + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                _logger.LogError($"An occurred while trying to process request");
                //failed
                return BadRequest(new { message = "An occurred while trying to process request" });
            }
            _logger.LogInformation($" Category {recvData.cat_name}  Updated successfully");
            //success
            return Ok(new
            {
                message = "Request has been successfully processed"
            });
        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult inventory_delete_category(int categoryid)
        {
            if (categoryid == 0)
            {
                return BadRequest(new { message = "Cannot find required category reference" });
            }
            _logger.LogInformation($" Deleting category");
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }


            // update category
            cnn.Open();
            string insertQ = "DELETE FROM inventory_category WHERE cat_id = " + categoryid + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request" });
            }

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }

        [Route("ListInventory")]
        [HttpGet]
        [Authorize]
        public ActionResult ListInventory()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
            _logger.LogInformation($"Fetching Inventory List");

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];

            //permission name
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

            //get database name
            string db = companyRes;

            //check permission
            //bool perStatus = myDbconnection.CheckRights(db, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry!, you are denied access to this action or information. Please contact your system administrator for more information." });
            //}

            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            List<Inventory> invList = new List<Inventory>();
            cnn.Open();
            string query = "SELECT \"Inventory\".*, " +
                "    reg.\"UFirstName\", reg.\"ULastName\", mod.\"UFirstName\" As m_fname, mod.\"ULastName\" As m_lname," +
                " \"Currencies\".\"CrName\", \"Currencies\".\"CrCode\", \"VATs\".\"VtRef\", \"VATs\".\"VtPerc\",warehouses.wh_code,warehouses.wh_desc " +
                "From \"Inventory\" " +
                //" LEFT Join inventory_category On inventory_category.cat_id = \"Inventory\".\"InvtCategory\" " +
                "LEFT Join \"Users\" reg On reg.\"UId\" = \"Inventory\".\"InvtAddedBy\"" +
                " LEFT Join \"Users\" mod On mod.\"UId\" = \"Inventory\".\"InvtModifiedBy\"" +
                " LEFT Join \"Currencies\" On \"Currencies\".\"CrId\" = \"Inventory\".\"InvtCurrency\"" +
                " LEFT Join \"VATs\" On \"VATs\".\"VtId\" = \"Inventory\".\"InvtVATId\" " +
                "LEFT Join warehouses On warehouses.wh_ref = \"Inventory\".\"WarehouseRef\" " +
                " WHERE \"Inventory\".\"InvtBranch\" = " + staff_branch + "  ";
            NpgsqlDataReader sdr = new NpgsqlCommand(query, cnn).ExecuteReader();

            _logger.LogError($"======================================={query}");

            while (sdr.Read())
            {
                try
                {
                    Inventory inv = new Inventory();

                    inv.InvtId = sdr["InvtId"] != DBNull.Value ? (int)sdr["InvtId"] : 0;
                    inv.InvtType = sdr["InvtType"] != DBNull.Value ? (string)sdr["InvtType"] : null;
                    inv.InvtName = sdr["InvtName"] != DBNull.Value ? (string)sdr["InvtName"] : null;
                    inv.InvtQty = sdr["InvtQty"] != DBNull.Value ? (int)sdr["InvtQty"] : 0;
                    inv.InvtReorderLevel = sdr["InvtReorderLevel"] != DBNull.Value ? (int)sdr["InvtReorderLevel"] : 0;
                    inv.InvtDateAdded = sdr["InvtDateAdded"] != DBNull.Value ? (DateTime)sdr["InvtDateAdded"] : DateTime.Now;
                    inv.InvtDateModified = sdr["InvtDateModified"] != DBNull.Value ? (DateTime)sdr["InvtDateModified"] : DateTime.Now;
                    inv.InvtAddedBy = sdr["InvtAddedBy"] != DBNull.Value ? (int)sdr["InvtAddedBy"] : 0;
                    inv.InvtModifiedBy = sdr["InvtModifiedBy"] != DBNull.Value ? (int)sdr["InvtModifiedBy"] : 0;
                    inv.InvtCurrency = sdr["InvtCurrency"] != DBNull.Value ? (int)sdr["InvtCurrency"] : 0;
                    inv.InvtVATId = sdr["InvtVATId"] != DBNull.Value ? (int)sdr["InvtVATId"] : 0;
                    inv.InvtBranch = sdr["InvtBranch"] != DBNull.Value ? (int)sdr["InvtBranch"] : 0;
                    inv.InvtCategory = sdr["InvtCategory"] != DBNull.Value ? (int)sdr["InvtCategory"] : 0;
                    inv.InvtProdCode = sdr["InvtProdCode"] != DBNull.Value ? (string)sdr["InvtProdCode"] : null;
                    inv.InvtRef = sdr["InvtRef"] != DBNull.Value ? (string)sdr["InvtRef"] : null;
                    inv.InvtBP = sdr["InvtBP"] != DBNull.Value ? (decimal)sdr["InvtBP"] : 0;
                    inv.InvtSP = sdr["InvtSP"] != DBNull.Value ? (decimal)sdr["InvtSP"] : 0;
                    inv.ProdDesc = sdr["ProdDesc"] != DBNull.Value ? (string)sdr["ProdDesc"] : null;
                    inv.UOM = sdr["UOM"] != DBNull.Value ? (int)sdr["UOM"] : 0;
                    inv.Obsolete = sdr["Obsolete"] != DBNull.Value ? (bool)sdr["Obsolete"] : false;
                    inv.NonStock = sdr["NonStock"] != DBNull.Value ? (bool)sdr["NonStock"] : false;
                    inv.ProdImage = sdr["ProdImage"] != DBNull.Value ? (string)sdr["ProdImage"] : null;
                    inv.BatchRef = sdr["BatchRef"] != DBNull.Value ? (string)sdr["BatchRef"] : null;
                    inv.BOM = sdr["BOM"] != DBNull.Value ? (bool)sdr["BOM"] : false;
                    inv.StkType = sdr["StkType"] != DBNull.Value ? (string)sdr["StkType"] : null;
                    inv.PartsPerUnit = sdr["PartsPerUnit"] != DBNull.Value ? (int)sdr["PartsPerUnit"] : 0;
                    inv.UnitSeparator = sdr["UnitSeparator"] != DBNull.Value ? (string)sdr["UnitSeparator"] : null;
                    inv.SupplierRef = sdr["SupplierRef"] != DBNull.Value ? (string)sdr["SupplierRef"] : null;
                    inv.LeadTime = sdr["LeadTime"] != DBNull.Value ? (int)sdr["LeadTime"] : 0;
                    inv.InventoryItem = sdr["InventoryItem"] != DBNull.Value ? (string)sdr["InventoryItem"] : "";
                    inv.SLProdGrpCode = sdr["SLProdGrpCode"] != DBNull.Value ? (string)sdr["SLProdGrpCode"] : String.Empty;
                    inv.PLProdGrpCode = sdr["PLProdGrpCode"] != DBNull.Value ? (string)sdr["PLProdGrpCode"] : null;

                    // inv.SLProdGrpCode = (string)sdr["SLProdGrpCode"];
                    //  inv.PLProdGrpCode = (string)sdr["PLProdGrpCode"];

                    inv.ProdDiscId = sdr["ProdDiscId"] != DBNull.Value ? (int)sdr["ProdDiscId"] : 0;
                    inv.ProdDiscPerc = sdr["ProdDiscPerc"] != DBNull.Value ? (decimal)sdr["ProdDiscPerc"] : 0;
                    inv.UdCostPrice = sdr["UdCostPrice"] != DBNull.Value ? (decimal)sdr["UdCostPrice"] : 0;
                    inv.AvgCostPrice = sdr["AvgCostPrice"] != DBNull.Value ? (decimal)sdr["AvgCostPrice"] : 0;
                    inv.LastPrice = sdr["LastPrice"] != DBNull.Value ? (decimal)sdr["LastPrice"] : 0;
                    //  inv.Weight = sdr["Weight"] != DBNull.Value ? (float)sdr["Weight"] : 0.00;
                    inv.LastMovDate = sdr["LastMovDate"] != DBNull.Value ? (DateTime)sdr["LastMovDate"] : DateTime.Now;
                    inv.LastIssueDate = sdr["LastIssueDate"] != DBNull.Value ? (DateTime)sdr["LastIssueDate"] : DateTime.Now;
                    inv.WarehouseRef = sdr["WarehouseRef"] != DBNull.Value ? (string)sdr["WarehouseRef"] : null;

                    inv.AddedFirstname = (string)sdr["UFirstName"];
                    inv.AddedLastname = (string)sdr["ULastName"];

                    //inv.ModifiedFirstname = (string)sdr["m_fname"];
                    //inv.ModifiedLastname = (string)sdr["m_lname"];

                    inv.InvtCurrency = (int)sdr["InvtCurrency"];

                    inv.CurrName = sdr["CrName"] != DBNull.Value ? (string)sdr["CrName"] : null; ;
                    inv.CurrCode = sdr["CrCode"] != DBNull.Value ? (string)sdr["CrCode"] : null;
                    inv.InvtVATId = sdr["InvtVATId"] != DBNull.Value ? (int)sdr["InvtVATId"] : 0;
                    inv.InvtCategory = sdr["InvtCategory"] != DBNull.Value ? (int)sdr["InvtCategory"] : 0;
                    //inv.category_name = (string)sdr["cat_name"];
                    //inv.category_ref = (string)sdr["cat_ref"];

                    inv.VATRef = (string)sdr["VtRef"];
                    inv.VATPerc = (float)sdr["VtPerc"];

                    inv.warehouse_code = sdr["wh_code"] != DBNull.Value ? (string)sdr["wh_code"] : null;
                    inv.warehouse_descr = sdr["wh_desc"] != DBNull.Value ? (string)sdr["wh_desc"] : null;

                    invList.Add(inv);
                }
                catch (Exception e)
                {
                    _logger.LogError($"======================================={e.StackTrace}");
                }
            }

            cnn.Close();

            _logger.LogInformation($"Fetching category List");
            //get categories
            List<InventoryCategory> categories_list = new List<InventoryCategory>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT inventory_category.*, \"BrName\" FROM inventory_category LEFT JOIN \"Branches\" ON (\"BrId\" = cat_branch) WHERE cat_branch = " + staff_branch + " ORDER BY cat_id DESC ", cnn).ExecuteReader();

            while (sdr0.Read())
            {
                InventoryCategory invct = new InventoryCategory();

                invct.cat_id = sdr0["cat_id"] != DBNull.Value ? (int)sdr0["cat_id"] : 0;
                invct.cat_entry_date = sdr0["cat_entry_date"] != DBNull.Value ? (DateTime)sdr0["cat_entry_date"] : DateTime.Today;
                invct.cat_name = sdr0["cat_name"] != DBNull.Value ? (string)sdr0["cat_name"] : null;
                invct.cat_ref = sdr0["cat_ref"] != DBNull.Value ? (string)sdr0["cat_ref"] : null;
                invct.cat_branch = sdr0["cat_branch"] != DBNull.Value ? (int)sdr0["cat_branch"] : 0;

                invct.branch_name = sdr0["BrName"] != DBNull.Value ? (string)sdr0["BrName"] : null;

                categories_list.Add(invct);
            }
            cnn.Close();

            _logger.LogInformation($"Fetching VAT List");
            //get VAT data
            List<TaxSetup> vatList = new List<TaxSetup>();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"VATs\" WHERE \"VtActive\" = 't' AND \"VtBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                TaxSetup tx = new TaxSetup();

                tx.VtId = sdr1["VtId"] != DBNull.Value ? (int)sdr1["VtId"] : 0;
                tx.VtRef = sdr1["VtRef"] != DBNull.Value ? (string)sdr1["VtRef"] : null;
                tx.VtPerc = sdr1["VtPerc"] != DBNull.Value ? (float)sdr1["VtPerc"] : 0;
                tx.VtActive = sdr1["VtActive"] != DBNull.Value ? (bool)sdr1["VtActive"] : false;

                vatList.Add(tx);

            }
            cnn.Close();






            //suppliers
            _logger.LogInformation($"Fetching Suppliers List");
            List<PLCustomer> plcustomerList = new List<PLCustomer>();
            cnn.Open();
            NpgsqlDataReader sdr_pl = new NpgsqlCommand("Select * From \"PLCustomer\" WHERE \"CustBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr_pl.Read())
            {
                PLCustomer pl = new PLCustomer();

                pl.PLCustCode = sdr_pl["PLCustCode"] != DBNull.Value ? (string)sdr_pl["PLCustCode"] : null;
                pl.CustName = sdr_pl["CustName"] != DBNull.Value ? (string)sdr_pl["CustName"] : null;
                pl.PhysicalAddress = sdr_pl["PhysicalAddress"] != DBNull.Value ? (string)sdr_pl["PhysicalAddress"] : null;
                pl.PostalAddress = sdr_pl["PostalAddress"] != DBNull.Value ? (string)sdr_pl["PostalAddress"] : null;
                pl.CurrID = sdr_pl["CurrID"] != DBNull.Value ? (int)sdr_pl["CurrID"] : 0;
                pl.VATNo = sdr_pl["VATNo"] != DBNull.Value ? (string)sdr_pl["VATNo"] : null;
                pl.CustID = sdr_pl["CustID"] != DBNull.Value ? (int)sdr_pl["CustID"] : 0;
                pl.RegisterDate = sdr_pl["RegisterDate"] != DBNull.Value ? (DateTime)sdr_pl["RegisterDate"] : DateTime.Now;
                pl.StaffID = sdr_pl["StaffID"] != DBNull.Value ? (int)sdr_pl["StaffID"] : 0;
                pl.CustBranch = sdr_pl["CustBranch"] != DBNull.Value ? (int)sdr_pl["CustBranch"] : 0;

                plcustomerList.Add(pl);
            }
            cnn.Close();










            //get warehouse data
            _logger.LogInformation($"Fetching Warehouse List");
            List<Warehouse> warehouseList = new List<Warehouse>();
            cnn.Open();
            NpgsqlDataReader sdr_wh = new NpgsqlCommand("Select * From warehouses WHERE wh_branch = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr_wh.Read())
            {
                Warehouse wh = new Warehouse();

                wh.wh_ref = sdr_wh["wh_ref"] != DBNull.Value ? (string)sdr_wh["wh_ref"] : null;
                wh.wh_code = sdr_wh["wh_code"] != DBNull.Value ? (string)sdr_wh["wh_code"] : null;
                wh.wh_desc = sdr_wh["wh_desc"] != DBNull.Value ? (string)sdr_wh["wh_desc"] : null;

                warehouseList.Add(wh);

            }
            cnn.Close();

            //check if user  can manage categories
            bool readinv = myDbconnection.CheckRights(companyRes, "ReadInventory", userId);
            bool modifyinv = myDbconnection.CheckRights(companyRes, "ModifyInventory", userId);
            bool addinv = myDbconnection.CheckRights(companyRes, "AddInventory", userId);

            NlService nlService = new NlService(db);
            MeasureofUnitService measureofUnitService = new MeasureofUnitService(db);
            var unitsofmeasure = measureofUnitService.listofUnitofMeasure();
            var analyiscodes = nlService.GetSlanalysisCodes();
            PlService plService = new PlService(companyRes);
            var plcodes = plService.GetPlanalysisCodes();

            return Ok(new
            {
                InventoryData = invList,
                VATData = vatList,
                CategoriesList = categories_list,
                ReadInvtPerm = readinv,
                ModifyInvtPerm = modifyinv,
                AddInvtPerm = addinv,
                UnitofMeasureList = unitsofmeasure,
                Warehouses = warehouseList,
                SLAnalysisCodesList = analyiscodes,
                SuppliersList = plcustomerList,
                PLAnalysisCodes = plcodes
            });

        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult update_inventory_working(int inventoryid, [FromBody] Inventory recvData)
        {

            _logger.LogInformation($"Updating inventory {recvData.InvtName}");
            if (inventoryid == 0)
            {
                return BadRequest(new { message = "Cannot find required inventory reference" });
            }
            else if (string.IsNullOrEmpty(recvData.InvtName))
            {
                return BadRequest(new { message = "Cannot find required inventory name" });
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            string img_path = null;
            //check if user profile has data
            if (string.IsNullOrEmpty(recvData.ProdImage))
            {
                img_path = "product_default.png";

            }

            //upload image if base64
            bool url_is_base64 = IsBase64String(recvData.ProdImage);

            if (url_is_base64 == false)
            {
                //upload image
                img_path = "product_default.png";
            }
            else
            {
                //remove prefix
                recvData.ProdImage = recvData.ProdImage.Substring(recvData.ProdImage.LastIndexOf(',') + 1);

                //upload image
                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "inventory");

                //Check if directory exist
                if (!System.IO.Directory.Exists(img_path))
                {
                    return BadRequest(new { message = "The path to upload account profile does NOT exist" });
                }

                string rand_imageName = System.Guid.NewGuid().ToString("D") + ".jpg";

                //set the image path
                string full_imgPath = Path.Combine(img_path, rand_imageName);

                //write file
                System.IO.File.WriteAllBytes(full_imgPath, Convert.FromBase64String(recvData.ProdImage));

                img_path = rand_imageName;

            }

            // update inventory
            cnn.Open();
            string insertQ = "UPDATE \"Inventory\" SET  \"InvtType\" = '" + recvData.InvtType + "', \"InvtName\" = '" + recvData.InvtName + "', \"InvtQty\" = " + recvData.InvtQty + ", \"InvtReorderLevel\" = " + recvData.InvtReorderLevel + ", \"InvtDateModified\" = '" + DateTime.Today + "', \"InvtModifiedBy\" = " + userId + ", \"InvtVATId\" = " + recvData.InvtVATId + ", \"InvtCategory\" = " + recvData.InvtCategory + ", \"InvtProdCode\" = '" + recvData.InvtProdCode + "', \"InvtBP\" = " + recvData.InvtBP + ", \"InvtSP\" = " + recvData.InvtSP + ", \"ProdDesc\" = '" + recvData.ProdDesc + "', \"UOM\" = '" + recvData.UOM + "', \"Obsolete\" = '" + recvData.Obsolete + "', \"NonStock\" = '" + recvData.NonStock + "', \"ProdImage\" = '" + img_path + "', \"BatchRef\" = '" + recvData.BatchRef + "', \"BOM\" = '" + recvData.BOM + "', \"StkType\" = '" + recvData.StkType + "', \"PartsPerUnit\" = " + recvData.PartsPerUnit + ", \"UnitSeparator\" = '" + recvData.UnitSeparator + "', \"SupplierRef\" = '" + recvData.SupplierRef + "', \"LeadTime\" = " + recvData.LeadTime + ", \"SLProdGrpCode\" = '" + recvData.SLProdGrpCode + "', \"PLProdGrpCode\" = '" + recvData.PLProdGrpCode + "', \"ProdDiscId\" = " + recvData.ProdDiscId + ", \"UdCostPrice\" = " + recvData.InvtSP + ", \"LastPrice\" = " + recvData.InvtSP + ", \"WarehouseRef\" = '" + recvData.WarehouseRef + "',\"InventoryItem\" = '" + recvData.InventoryItem + "' WHERE \"InvtId\" = " + inventoryid + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                _logger.LogError($"Update Failed");
                //failed
                return BadRequest(new { message = "An occurred while trying to process request" });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Updated item " + recvData.InvtName + " in inventory";
            auditTrail.module = "Inventory";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult update_inventory(int inventoryid, [FromBody] Inventory recvData)
        {
            if (inventoryid == 0)
            {
                return BadRequest(new { message = "Cannot find required inventory reference" });
            }
            else if (string.IsNullOrEmpty(recvData.InvtName))
            {
                return BadRequest(new { message = "Cannot find required inventory name" });
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }


            // update inventory
            cnn.Open();
            string insertQ = "UPDATE \"Inventory\" SET  \"InvtType\" = '" + recvData.InvtType + "', \"InvtName\" = '" + recvData.InvtName + "', \"InvtQty\" = " + recvData.InvtQty + ", \"InvtReorderLevel\" = " + recvData.InvtReorderLevel + ", \"InvtDateModified\" = '" + DateTime.Today + "', \"InvtModifiedBy\" = " + userId + ", \"InvtVATId\" = " + recvData.InvtVATId + ", \"InvtCategory\" = " + recvData.InvtCategory + ", \"InvtProdCode\" = '" + recvData.InvtProdCode + "', \"InvtBP\" = " + recvData.InvtBP + ", \"InventoryItem\" = '" + recvData.InventoryItem + "',\"InvtSP\" = " + recvData.InvtSP + " WHERE \"InvtId\" = " + inventoryid + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request" });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Updated item" + recvData.InvtName + " to inventory";
            auditTrail.module = "Inventory";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public ActionResult delete_inventory(int inventoryid)
        {
            if (inventoryid == 0)
            {
                return BadRequest(new { message = "Cannot find required inventory reference" });
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
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            bool inventory_exists = false;

            //check if item exists in SL details
            cnn.Open();
            NpgsqlDataReader sdrSL = new NpgsqlCommand("SELECT * FROM \"SLInvoiceDetail\" WHERE \"ItemId\" = " + inventoryid + "  ", cnn).ExecuteReader();
            if (sdrSL.HasRows == true)
            {
                inventory_exists = true;
            }
            cnn.Close();

            //check if item exists in PL details
            cnn.Open();
            NpgsqlDataReader sdrPL = new NpgsqlCommand("SELECT * FROM \"PLInvoiceDetail\" WHERE \"ProdId\" = " + inventoryid + "  ", cnn).ExecuteReader();
            if (sdrPL.HasRows == true)
            {
                inventory_exists = true;
            }
            cnn.Close();

            if (inventory_exists == true)
            {
                return BadRequest(new { message = "Sorry, the inventory item could NOT be removed because it is already part of a transaction that has already been carried out." });
            }

            // delete inventory
            cnn.Open();
            string insertQ = "DELETE FROM \"Inventory\" WHERE \"InvtId\" = " + inventoryid + " ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, db);
            cnn.Close();
            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occured while trying to process request" });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Deleted item in the inventory";
            auditTrail.module = "Inventory";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }


        [Route("[action]")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateInventory(Inventory inventoryData)
        {
            //check customer type
            if (string.IsNullOrEmpty(inventoryData.InvtName))
            {
                return BadRequest(new { message = "Missing inventory name" });

            }
            _logger.LogInformation($"Creating item {inventoryData.InvtName}");

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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            _logger.LogInformation($"Checking if item {inventoryData.InvtName} exists");
            //check if name exists
            cnn.Open();
            NpgsqlDataReader sdr78 = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtName\" = '" + inventoryData.InvtName + "' ", cnn).ExecuteReader();
            if (sdr78.HasRows == true)
            {
                return BadRequest(new { message = "The inventory name " + inventoryData.InvtName + " is already registered." });
            }
            cnn.Close();

            //check prod code
            _logger.LogInformation($"Checking if item  {inventoryData.InvtName} product code exists");
            cnn.Open();
            NpgsqlDataReader sdr79 = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtProdCode\" = '" + inventoryData.InvtProdCode + "' ", cnn).ExecuteReader();
            if (sdr79.HasRows == true)
            {
                return BadRequest(new { message = "The inventory code for the inventory " + inventoryData.InvtName + " is already registered." });
            }
            cnn.Close();

            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            //get last category id
            int last_invt_ID = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(\"InvtId\") as sl From \"Inventory\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_invt_ID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //get guid
            string int_Ref = System.Guid.NewGuid().ToString("D");



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
                lic.CompanyCurrency = (int)sdr3["CompanyCurrency"];
            }
            cnn.Close();


            string img_path = null;
            //check if user profile has data
            if (string.IsNullOrEmpty(inventoryData.ProdImage))
            {
                img_path = "product_default.png";

            }

            //upload image if base64
            bool url_is_base64 = IsBase64String(inventoryData.ProdImage);

            if (url_is_base64 == false)
            {
                //upload image
                img_path = "product_default.png";
            }
            else
            {
                //remove prefix
                inventoryData.ProdImage = inventoryData.ProdImage.Substring(inventoryData.ProdImage.LastIndexOf(',') + 1);

                //upload image
                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "inventory");

                //Check if directory exist
                if (!System.IO.Directory.Exists(img_path))
                {
                    return BadRequest(new { message = "The path to upload account profile does NOT exist" });
                }

                string rand_imageName = System.Guid.NewGuid().ToString("D") + ".jpg";

                //set the image path
                string full_imgPath = Path.Combine(img_path, rand_imageName);

                //write file
                System.IO.File.WriteAllBytes(full_imgPath, Convert.FromBase64String(inventoryData.ProdImage));

                img_path = rand_imageName;

            }


            _logger.LogInformation($"Saving item {inventoryData.InvtName}  details");
            Inventory inv = new Inventory();

            // create inventory
            cnn.Open();
            string insertQ = "INSERT INTO \"Inventory\" (\"InvtId\", \"InvtType\", \"InvtName\", \"InvtQty\", \"InvtReorderLevel\", \"InvtDateAdded\", \"InvtDateModified\", \"InvtAddedBy\", \"InvtModifiedBy\", \"InvtCurrency\", \"InvtVATId\", \"InvtBranch\", \"InvtCategory\", \"InvtProdCode\", \"InvtRef\", \"InvtBP\", \"InvtSP\",  \"ProdDesc\",  \"UOM\",  \"Obsolete\",  \"NonStock\",  \"ProdImage\",  \"BatchRef\", \"BOM\", \"StkType\", \"PartsPerUnit\", \"UnitSeparator\", \"SupplierRef\", \"LeadTime\", \"SLProdGrpCode\", \"PLProdGrpCode\", \"ProdDiscId\", \"UdCostPrice\", \"AvgCostPrice\", \"LastPrice\", \"LastMovDate\", \"LastIssueDate\", \"WarehouseRef\",\"InventoryItem\" ) VALUES(" + (last_invt_ID + 1) + ", '" + inventoryData.InvtType + "', '" + inventoryData.InvtName + "', " + inventoryData.InvtQty + ", " + inventoryData.InvtReorderLevel + " ,'" + DateTime.Today + "','" + DateTime.Today + "'," + userId + "," + userId + ", " + lic.CompanyCurrency + "," + inventoryData.InvtVATId + ", " + staff_branch + ", " + inventoryData.InvtCategory + ", '" + inventoryData.InvtProdCode + "','" + int_Ref + "', " + inventoryData.InvtBP + ", " + inventoryData.InvtSP + ", '" + inventoryData.ProdDesc + "', '" + inventoryData.UOM + "', '" + inventoryData.Obsolete + "', '" + inventoryData.NonStock + "', '" + img_path + "', '" + inventoryData.BatchRef + "', '" + inventoryData.BOM + "', '" + inventoryData.StkType + "', " + inventoryData.PartsPerUnit + ", '" + inventoryData.UnitSeparator + "', '" + inventoryData.SupplierRef + "', " + inventoryData.LeadTime + ", '" + inventoryData.SLProdGrpCode + "','" + inventoryData.PLProdGrpCode + "', " + inventoryData.ProdDiscId + ", " + inventoryData.InvtSP + "," + inventoryData.InvtSP + "," + inventoryData.InvtSP + ",'" + DateTime.Now + "','" + DateTime.Now + "','" + inventoryData.WarehouseRef + "','" + inventoryData.InventoryItem + "'  ); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, companyRes);
            cnn.Close();

            if (myReq2 == false)
            {
                //failed
                return BadRequest(new { message = "An occurred while trying to process request." });
            }

            //get inventory id
            Inventory inv_srch = new Inventory();
            cnn.Open();
            NpgsqlDataReader sdr_ins = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtRef\" = '" + int_Ref + "' ", cnn).ExecuteReader();
            while (sdr_ins.Read())
            {
                inv_srch.InvtId = (int)sdr_ins["InvtId"];

            }
            cnn.Close();

            //add to warehouse summary
            // if it goes to warehousw
            ManageWarehouseSummary mw_ = new ManageWarehouseSummary();
            bool mw_res = mw_.warehouse_summary_addstock(companyRes, inv_srch.InvtId, inventoryData.InvtQty, userId);
            if (mw_res == false)
            {
                _logger.LogError($"{inventoryData.InvtName} is not added to warehouse since it is not assigned to one.");
                //return BadRequest(new { message = "An occurred while trying to process warehouse summary request." });
            }

            _logger.LogInformation($" item {inventoryData.InvtName}  has been saved .");


            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = $"Created Item {inventoryData.InvtName}  at {DateTime.Now.ToString("dd/MM/yyyy")}";
            auditTrail.module = "Inventory";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });



        }



        /// <summary>
        /// USING FORMDATA TO CREATE I
        /// </summary>
        /// <param name="inventoryData"></param>
        /// <returns></returns>

        [Route("V1CreateInventory")]
        [Authorize]
        [HttpPost]
        public ActionResult CreateInventory([FromForm] InventoryRequest inventoryData)
        {
            _logger.LogInformation(MyConstants.InventoryIdLog, inventoryData.InvtName);
            //check customer type
            if (string.IsNullOrEmpty(inventoryData.InvtName))
            {
                _logger.LogInformation(MyConstants.InventoryIdLog, "Missing inventory name");
                return BadRequest(new { message = "Missing inventory name" });

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
                _logger.LogInformation(MyConstants.InventoryIdLog, "dont have permission");
                return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));


            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                _logger.LogError(MyConstants.InventoryIdLog, "cant identify user  branch");
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            //get last category id
            int last_invt_ID = 0;




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
                lic.CompanyCurrency = (int)sdr3["CompanyCurrency"];
            }
            cnn.Close();

            Inventory inv = new Inventory();
            InventoryService inventoryService = new InventoryService(companyRes, _hostingEnvironment);
            var response = inventoryService.AddInventory(inventoryData, userId, staff_branch, lic.CompanyCurrency);
            if (response.Httpcode == 400)
            {
                _logger.LogError(MyConstants.InventoryIdLog, response.Message);
                return BadRequest(new { message = response.Message });
            }
            var int_Ref = response.Message;



            //get inventory id
            Inventory inv_srch = new Inventory();
            cnn.Open();
            NpgsqlDataReader sdr_ins = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtRef\" = '" + int_Ref + "' ", cnn).ExecuteReader();
            while (sdr_ins.Read())
            {
                inv_srch.InvtId = (int)sdr_ins["InvtId"];

            }
            cnn.Close();

            //add to warehouse summary

            ManageWarehouseSummary mw_ = new ManageWarehouseSummary();
            bool mw_res = mw_.warehouse_summary_addstock(companyRes, inv_srch.InvtId, inventoryData.InvtQty, userId);
            if (mw_res == false)
            {
                //_logger.LogError(MyConstants.InventoryIdLog, "An occurred while trying to process warehouse summary request.");
                //return BadRequest(new { message = "An occurred while trying to process warehouse summary request." });

                _logger.LogError($"{inventoryData.InvtName} is not added to warehouse since it is not assigned to it.");
            }



            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Added item" + inventoryData.InvtName + " to inventory";
            auditTrail.module = "Inventory";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);

            //success
            return Ok(new
            {
                message = "Request has been successfully processed"

            });


        }























        /// <summary>
        /// check if it is a base 64
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>



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
        [HttpPost]
        public ActionResult CreateWarehouse(Warehouse recvData)
        {
            //check customer type
            if (string.IsNullOrEmpty(recvData.wh_code))
            {
                return BadRequest(new { message = "Missing warehouse code" });

            }
            else if (string.IsNullOrEmpty(recvData.wh_address_1) && string.IsNullOrEmpty(recvData.wh_address_2) && string.IsNullOrEmpty(recvData.wh_address_3) && string.IsNullOrEmpty(recvData.wh_address_4))
            {
                return BadRequest(new { message = "You need at least one address to attach to your warehouse" });
            }
            else if (string.IsNullOrEmpty(recvData.wh_desc))
            {
                return BadRequest(new { message = "Missing warehouse description" });
            }
            else if (string.IsNullOrEmpty(recvData.wh_type))
            {
                return BadRequest(new { message = "Missing warehouse type" });
            }
            else if (string.IsNullOrEmpty(recvData.wh_stage))
            {
                return BadRequest(new { message = "Missing warehouse stage" });
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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            //check if ref exists
            cnn.Open();
            NpgsqlDataReader sdr78 = new NpgsqlCommand("Select * From warehouses WHERE wh_code = '" + recvData.wh_code + "' ", cnn).ExecuteReader();
            if (sdr78.HasRows == true)
            {
                return BadRequest(new { message = "The warehouse code " + recvData.wh_code + " is already registered." });
            }
            cnn.Close();


            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }


            //get guid
            string unique_Ref = System.Guid.NewGuid().ToString("D");


            // create warehouse
            cnn.Open();
            string insertQ = "INSERT INTO warehouses (wh_ref, wh_code, wh_desc, wh_address_1, wh_address_2, wh_address_3, wh_address_4, wh_type, wh_stage, wh_modifiedon, wh_createdon, wh_branch ) VALUES('" + unique_Ref + "', '" + recvData.wh_code + "', '" + recvData.wh_desc + "', '" + recvData.wh_address_1 + "', '" + recvData.wh_address_2 + "','" + recvData.wh_address_3 + "','" + recvData.wh_address_4 + "','" + recvData.wh_type + "','" + recvData.wh_stage + "', '" + DateTime.Now + "', '" + DateTime.Now + "' , " + staff_branch + "); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, companyRes);
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
        [HttpGet]
        [Authorize]
        public ActionResult warehouse_load_default()
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

            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            List<Warehouse> warehouseList = new List<Warehouse>();
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select * FROM warehouses WHERE wh_branch = " + staff_branch + "  ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Warehouse w = new Warehouse();

                w.wh_ref = sdr["wh_ref"] != DBNull.Value ? (string)sdr["wh_ref"] : null;
                w.wh_code = sdr["wh_code"] != DBNull.Value ? (string)sdr["wh_code"] : null;
                w.wh_desc = sdr["wh_desc"] != DBNull.Value ? (string)sdr["wh_desc"] : null;
                w.wh_address_1 = sdr["wh_address_1"] != DBNull.Value ? (string)sdr["wh_address_1"] : null;
                w.wh_address_2 = sdr["wh_address_2"] != DBNull.Value ? (string)sdr["wh_address_2"] : null;
                w.wh_address_3 = sdr["wh_address_3"] != DBNull.Value ? (string)sdr["wh_address_3"] : null;
                w.wh_address_4 = sdr["wh_address_4"] != DBNull.Value ? (string)sdr["wh_address_4"] : null;
                w.wh_type = sdr["wh_type"] != DBNull.Value ? (string)sdr["wh_type"] : null;
                w.wh_stage = sdr["wh_stage"] != DBNull.Value ? (string)sdr["wh_stage"] : null;
                w.wh_modifiedon = sdr["wh_modifiedon"] != DBNull.Value ? (DateTime)sdr["wh_modifiedon"] : DateTime.Now;
                w.wh_createdon = sdr["wh_createdon"] != DBNull.Value ? (DateTime)sdr["wh_createdon"] : DateTime.Now;

                warehouseList.Add(w);
            }

            cnn.Close();

            return Ok(new
            {
                //manage_wahouses_permission = perStatus,
                warehouses_list = warehouseList
            });

        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult warehouse_edit_default(string warehouse_ref)
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

            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            Warehouse w = new Warehouse();
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select * FROM warehouses WHERE wh_branch = " + staff_branch + " AND wh_ref = '" + warehouse_ref + "'  ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                w.wh_ref = sdr["wh_ref"] != DBNull.Value ? (string)sdr["wh_ref"] : null;
                w.wh_code = sdr["wh_code"] != DBNull.Value ? (string)sdr["wh_code"] : null;
                w.wh_desc = sdr["wh_desc"] != DBNull.Value ? (string)sdr["wh_desc"] : null;
                w.wh_address_1 = sdr["wh_address_1"] != DBNull.Value ? (string)sdr["wh_address_1"] : null;
                w.wh_address_2 = sdr["wh_address_2"] != DBNull.Value ? (string)sdr["wh_address_2"] : null;
                w.wh_address_3 = sdr["wh_address_3"] != DBNull.Value ? (string)sdr["wh_address_3"] : null;
                w.wh_address_4 = sdr["wh_address_4"] != DBNull.Value ? (string)sdr["wh_address_4"] : null;
                w.wh_type = sdr["wh_type"] != DBNull.Value ? (string)sdr["wh_type"] : null;
                w.wh_stage = sdr["wh_stage"] != DBNull.Value ? (string)sdr["wh_stage"] : null;
                w.wh_modifiedon = sdr["wh_modifiedon"] != DBNull.Value ? (DateTime)sdr["wh_modifiedon"] : DateTime.Now;
                w.wh_createdon = sdr["wh_createdon"] != DBNull.Value ? (DateTime)sdr["wh_createdon"] : DateTime.Now;

            }

            cnn.Close();

            return Ok(new
            {
                manage_wahouses_permission = perStatus,
                warehouse_data = w
            });

        }

        [Route("[action]")]
        [Authorize]
        [HttpPut]
        public ActionResult warehouse_update(string warehouse_ref, [FromBody] Warehouse recvData)
        {
            if (string.IsNullOrEmpty(warehouse_ref))
            {
                return BadRequest(new { message = "Missing warehouse reference" });
            }
            //check customer type
            if (string.IsNullOrEmpty(recvData.wh_code))
            {
                return BadRequest(new { message = "Missing warehouse code" });

            }
            else if (string.IsNullOrEmpty(recvData.wh_address_1) && string.IsNullOrEmpty(recvData.wh_address_2) && string.IsNullOrEmpty(recvData.wh_address_3) && string.IsNullOrEmpty(recvData.wh_address_4))
            {
                return BadRequest(new { message = "You need at least one address to attach to your warehouse" });
            }
            else if (string.IsNullOrEmpty(recvData.wh_desc))
            {
                return BadRequest(new { message = "Missing warehouse description" });
            }
            else if (string.IsNullOrEmpty(recvData.wh_type))
            {
                return BadRequest(new { message = "Missing warehouse type" });
            }
            else if (string.IsNullOrEmpty(recvData.wh_stage))
            {
                return BadRequest(new { message = "Missing warehouse stage" });
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

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            //check if ref exists
            cnn.Open();
            NpgsqlDataReader sdr78 = new NpgsqlCommand("Select * From warehouses WHERE wh_ref = '" + warehouse_ref + "' ", cnn).ExecuteReader();
            if (sdr78.HasRows == false)
            {
                return BadRequest(new { message = "The warehouse reference " + warehouse_ref + " was NOT found." });
            }
            cnn.Close();


            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            string upd_qr = "UPDATE warehouses SET wh_code = '" + recvData.wh_code + "',wh_desc = '" + recvData.wh_desc +
                            "', wh_address_1 = '" + recvData.wh_address_1 + "',wh_address_2 = '" +
                            recvData.wh_address_2 + "',wh_address_3 = '" + recvData.wh_address_3 +
                            "', wh_address_4 = '" + recvData.wh_address_4 + "', wh_type = '" + recvData.wh_type +
                            "', wh_stage = '" + recvData.wh_stage + "', wh_modifiedon = '" + DateTime.Now + "' WHERE wh_ref = '" + warehouse_ref + "' ";

            // update warehouse
            bool myReq2 = myDbconnection.UpdateDelInsert(upd_qr, companyRes);
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
        public ActionResult warehouse_delete(string warehouse_ref)
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


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

            //check if ref exists
            cnn.Open();
            NpgsqlDataReader sdr78 = new NpgsqlCommand("Select * From warehouses WHERE wh_ref = '" + warehouse_ref + "' ", cnn).ExecuteReader();
            if (sdr78.HasRows == false)
            {
                return BadRequest(new { message = "The warehouse reference " + warehouse_ref + " was NOT found." });
            }
            cnn.Close();


            //get user branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "We have problems identifying your branch" });
            }

            string upd_qr = "DELETE FROM warehouses WHERE wh_ref = '" + warehouse_ref + "' ";

            // update warehouse
            bool myReq2 = myDbconnection.UpdateDelInsert(upd_qr, companyRes);
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
        [Authorize]
        [HttpGet]
        public ActionResult StockReport_Basic(int trans_year)
        {
            try
            {
                //check customer type
                if (trans_year == 0)
                {
                    // return BadRequest(new { message = "Missing transaction year" });
                    trans_year = DateTime.Now.Year;
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

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(companyRes));

                //get user branch
                int staff_branch = myDbconnection.GetStaffBranch(userId, companyRes);
                if (staff_branch == 0)
                {
                    return BadRequest(new { message = "We have problems identifying your branch" });
                }

                //get warehouse summary for specific year
                List<WarehouseSummary> w_summaries_list = new List<WarehouseSummary>();
                ManageWarehouseSummary mng_summary = new ManageWarehouseSummary();

                w_summaries_list = mng_summary.GetAllWarehouseSummary(companyRes, trans_year, staff_branch);

                //get closing stocks per month


                List<WarehouseSummary> whcs_list = new List<WarehouseSummary>();
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(" Select Sum(physical_qty) As \"Sum_physical_qty\", Max(ws_date) As \"Max_ws_date\", Date_Part('month', ws_date) " +
                    "From warehouse_summary WHERE date_part('year', ws_date) = " + trans_year + " AND ws_branch = " + staff_branch + "" +
                    " Group By Date_Part('month', ws_date) Order By Date_Part('month', ws_date) ", cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    WarehouseSummary cs = new WarehouseSummary();

                    cs.sum_physical_qty = (long)sdr0["Sum_physical_qty"];
                    cs.max_ws_date = sdr0["Max_ws_date"] != DBNull.Value ? (DateTime)sdr0["Max_ws_date"] : DateTime.Now;
                    cs.date_Part = (double)sdr0["Date_Part"];

                    whcs_list.Add(cs);
                }
                cnn.Close();


                //success
                return Ok(new
                {
                    message = "Request has been successfully processed",
                    warehouse_summary_list_specific_year = w_summaries_list,
                    year_closing_stocks = whcs_list
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e });
                // Console.WriteLine(e);
                //throw;
            }


        }

    }
}
