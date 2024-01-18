using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Controllers.GoodsReceivedNote;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.GoodReceivedNote;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Service.GoodsReceivedNoteService;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace pyme_finance_api.Controllers.GoodsReturnedNote
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsReturnedNoteController : ControllerBase
    {

        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;
        private IWebHostEnvironment _hostingEnvironment;
        readonly ILogger<GoodsReceivedNoteController> _log;

        public GoodsReturnedNoteController(IConfiguration config, IWebHostEnvironment environment, ILogger<GoodsReceivedNoteController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _log = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_all_goods_returned_note()
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


            GoodsReceivedNoteService goodsReceivedNoteService = new GoodsReceivedNoteService(tokenData);
            var data = goodsReceivedNoteService.getAllGoodsReturnedNote();

            return Ok(data);
        }


        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult add_goods_returned_note_details(Models.GoodReceivedNote.GoodsReceivedNote goodsReceivedNote)
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
            goodsReceivedNote.createdBy = userId;
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
            GoodsReceivedNoteService goodsReceivedNoteService = new GoodsReceivedNoteService(tokenData);
            var response = goodsReceivedNoteService.addGoodReturnedNote(goodsReceivedNote);


            return Ok(response);
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_goods_received_note_details(int id)
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
            string db = companyRes;
            GoodsReceivedNoteService goodsReceivedNoteService = new GoodsReceivedNoteService(db);
            var data = goodsReceivedNoteService.getGoodReturnedNoteDetail(id);            
            var lic = goodsReceivedNoteService.GettingCompanyDetail();
            return Ok(new { goodsReceivedNote = data, LicenseData = lic, Code = 200 });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult get_goods_returned_note_details(int id)
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
            string db = companyRes;
            GoodsReceivedNoteService goodsReceivedNoteService = new GoodsReceivedNoteService(tokenData);
            var data = goodsReceivedNoteService.getGoodReturnNoteDetail(id);
            var grnData = goodsReceivedNoteService.GetAllGoodReturnNoteType();
            var headerData = goodsReceivedNoteService.GetPurchaseHeaderSettings();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            License lic = new License();
            cnn.Open();
            string query = "Select * From \"Licence\"  ";
            NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (reader.Read())
            {
                lic.CompanyName = (string)reader["CompanyName"];
                lic.CompanySlogan = (string)reader["CompanySlogan"];
                lic.CompanyPostal = (string)reader["CompanyPostal"];
                lic.CompanyContact = (string)reader["CompanyContact"];
                lic.CompanyVAT = (string)reader["CompanyVAT"];
                lic.PhysicalAddress = (string)reader["PhysicalAddress"];
                lic.CompanyLogo = (string)reader["CompanyLogo"];
            }
            cnn.Close();
            string img_path = "";
            bool url_is_base64 = IsBase64String(lic.CompanyLogo);
            if (String.IsNullOrEmpty(lic.CompanyLogo))
            {
                //upload image
                //using ngenx.jpg for test purpose
                lic.CompanyLogo = "invoice_default.jpg";
                //  lic.CompanyLogo = "ngenx.jpg";
                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                lic.CompanyLogo = Convert.ToBase64String(imageArray); ;
            }
            else
            {
                img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                lic.CompanyLogo = Convert.ToBase64String(imageArray);
            }

            return Ok(new { goodsReceivedNote = data, grnList = grnData, HeaderSettings = headerData, LicenseData = lic, Code = 200 });
        }
        private static bool IsBase64String(string base64)
        {
            try
            {
                string result = Regex.Replace(base64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                byte[] data = Convert.FromBase64String(result);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
