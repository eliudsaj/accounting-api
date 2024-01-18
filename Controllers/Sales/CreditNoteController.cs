using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.ReportPages.Sales;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using pyme_finance_api.Models.Sales.Terms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditNoteController : ControllerBase
    {
      
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        readonly ILogger<CreditNoteController> _log;
        private readonly IWebHostEnvironment environment;
        public CreditNoteController(IConfiguration config, ILogger<CreditNoteController> logger, IWebHostEnvironment environment)
        {
            _configuration = config;
            _log = logger;
            this.environment=environment;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetAll()
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;

            //check if cookie exists in Request
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            _log.LogInformation($"Fetching CreditNotes");

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

            //get all Credit Notes
            List<Invoice> invList = new List<Invoice>();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();

            NpgsqlDataReader sdr = new NpgsqlCommand("Select \"SLInvoiceHeader\".*, \"SLCustomerSerial\", \"CustFirstName\", \"CustLastName\",  \"CustCompany\", \"CustType\", " +
                " \"CrId\", \"CrCode\", fp_name From \"SLInvoiceHeader\" LEFT JOIN  \"SLCustomer\" ON ( \"CustId\" =  \"SLCustomerSerial\" ) " +
                "LEFT JOIN  \"Currencies\" ON ( \"CurrencyId\" =  \"CrId\" ) LEFT JOIN financial_periods ON ( fp_ref =  \"Period\" )" +
                " WHERE \"INVTypeRef\" = 'CRN'  ", cnn).ExecuteReader();
            while (sdr.Read())
            {
                Invoice inv = new Invoice();
                inv.SLJrnlNo = (int)sdr["SLJrnlNo"];
                inv.NlJrnlNo = (int)sdr["NlJrnlNo"];
                inv.CustCode = (string)sdr["CustCode"];
                inv.TransDate = (DateTime)sdr["TransDate"];
                inv.Period = (string)sdr["Period"];
                inv.DocPrefix = sdr["DocPrefix"].ToString();
                inv.DocRef = (int)sdr["DocRef"];
                inv.TotalAmount = (decimal)sdr["TotalAmount"];
                inv.INVTypeRef = (string)sdr["INVTypeRef"];
                inv.Dispute = (bool)sdr["Dispute"];
                inv.DeliveryCust = (int)sdr["DeliveryCust"];
                // inv.DeliveryAddress = (string)sdr["DeliveryAddress"];
                inv.DeliveryDue = (DateTime)sdr["DeliveryDue"];
                inv.INVDate = (DateTime)sdr["INVDate"];
                inv.CustId = (int)sdr["CustId"];
                inv.PaymentDays = (int)sdr["PaymentDays"];
                inv.CustomRef = sdr["CustomRef"].ToString();
                inv.CurrencyId = (int)sdr["CurrencyId"];
                inv.DueDate = (DateTime)sdr["DueDate"];
                inv.CreditNoteAmount = sdr["CreditNoteAmount"] != DBNull.Value ? (decimal)sdr["CreditNoteAmount"] : 0;
                inv.HasCreditNote = (bool)sdr["HasCreditNote"];
                inv.CustFirstName = sdr["CustFirstName"].ToString();
                inv.CustLastname = sdr["CustLastname"].ToString();
                inv.CustCompany = sdr["CustCompany"].ToString();
                inv.CustType = sdr["CustType"].ToString();
                inv.CrCode = sdr["CrCode"].ToString();
                inv.fp_name = sdr["fp_name"].ToString();
                inv.CRNReason = sdr["CRNReason"] != DBNull.Value ? (string)sdr["CRNReason"] : "";
                inv.TotalBalance = sdr["TotalBalance"] != DBNull.Value ? (decimal)sdr["TotalBalance"] : 0;
                inv.CRNDate = sdr["CRNDate"] != DBNull.Value ? (DateTime)sdr["CRNDate"] : (DateTime)sdr["INVDate"];
                invList.Add(inv);
            }
            cnn.Close();
            return Ok(new { CRNData = invList });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult<Invoice> GetCRNPageReport(string CRNJRN , string SL)
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

            //get credit note
            Invoice inv = new Invoice();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("Select \"SLInvoiceHeader\".*, \"SLCustomerSerial\", \"CustFirstName\", \"CustLastName\"," +
                "\"Address\",\"PostalAddress\",\"CustEmail\",\"CustContact\",\"CustCompany\", \"CustType\", \"CrId\", \"CrCode\" From \"SLInvoiceHeader\"" +
                " LEFT JOIN  \"SLCustomer\" ON ( \"CustId\" =  \"SLCustomerSerial\" ) " +
                "LEFT JOIN  \"Currencies\" ON ( \"CurrencyId\" =  \"CrId\" ) " +
                "WHERE \"DocRef\" = '" + CRNJRN + "'  AND \"SLJrnlNo\" ='"+Int32.Parse(SL)+"'    AND  \"INVTypeRef\" = 'CRN' LIMIT 1  ", cnn).ExecuteReader();
            if (sdr.Read())
            {
                inv.SLJrnlNo = (int)sdr["SLJrnlNo"];
                inv.NlJrnlNo = (int)sdr["NlJrnlNo"];
                inv.CustCode = (string)sdr["CustCode"];
                inv.TransDate = (DateTime)sdr["TransDate"];
                inv.Period = (string)sdr["Period"];

                inv.DocPrefix = sdr["DocPrefix"].ToString();
                inv.DocRef = (int)sdr["DocRef"];
                inv.TotalAmount = (decimal)sdr["TotalAmount"];
                inv.INVTypeRef = (string)sdr["INVTypeRef"];
                inv.Dispute = (bool)sdr["Dispute"];
                inv.DeliveryCust = (int)sdr["DeliveryCust"];
                // inv.DeliveryAddress = (string)sdr["DeliveryAddress"];
                inv.DeliveryDue = (DateTime)sdr["DeliveryDue"];
                inv.INVDate = (DateTime)sdr["INVDate"];
                inv.CustId = (int)sdr["CustId"];
                inv.PaymentDays = (int)sdr["PaymentDays"];
                inv.CustomRef = sdr["CustomRef"].ToString();
                inv.CurrencyId = (int)sdr["CurrencyId"];
                inv.DueDate = (DateTime)sdr["DueDate"];
                inv.CustFirstName = sdr["CustFirstName"].ToString();
                inv.CustLastname = sdr["CustLastname"].ToString();
                inv.CustCompany = sdr["CustCompany"].ToString();
                inv.CustType = sdr["CustType"].ToString();
                inv.CrCode = sdr["CrCode"].ToString();
                inv.Address = sdr["Address"].ToString();
                inv.PostalAddress = sdr["PostalAddress"].ToString();
                inv.CustEmail = sdr["CustEmail"].ToString();
                inv.CustContact = sdr["CustContact"].ToString();
                inv.CreditNoteAmount = sdr["CreditNoteAmount"] != DBNull.Value ? (decimal)sdr["CreditNoteAmount"] : 0;

                inv.DeliveryAddress = sdr["DeliveryAddress"].ToString();
                inv.CRNReason = sdr["CRNReason"].ToString();
                inv.CRNvat = sdr["CRNVat"] != DBNull.Value ? (decimal)sdr["CRNVat"] : 0;
                inv.CRNDate = sdr["CRNDate"] != DBNull.Value ? (DateTime)sdr["CRNDate"] : (DateTime)sdr["INVDate"];

            }
            cnn.Close();

            //Get original invoice
            Invoice orig_inv = new Invoice();
            cnn.Open();
            NpgsqlDataReader sdr12 = new NpgsqlCommand("SELECT * From \"SLInvoiceHeader\" WHERE \"SLJrnlNo\" = '" + Int32.Parse(SL) + "' AND  \"INVTypeRef\" = 'CRN' LIMIT 1  ", cnn).ExecuteReader();
            if (sdr12.Read())
            {
                orig_inv.SLJrnlNo = (int)sdr12["SLJrnlNo"];
                orig_inv.NlJrnlNo = (int)sdr12["NlJrnlNo"];
                orig_inv.CustCode = (string)sdr12["CustCode"];
                orig_inv.TransDate = (DateTime)sdr12["TransDate"];
                orig_inv.DocPrefix = (string)sdr12["DocPrefix"];
                orig_inv.DocRef = (int)sdr12["DocRef"];
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
            bool url_is_base64 = IsBase64String(lic.CompanyLogo);
            if (String.IsNullOrEmpty(lic.CompanyLogo))
            {
                //upload image
                //using ngenx.jpg for test purpose
                lic.CompanyLogo = "invoice_default.jpg";
                //  lic.CompanyLogo = "ngenx.jpg";
                img_path = Path.Combine(environment.WebRootPath, "Images", "company_profile");
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                lic.CompanyLogo = Convert.ToBase64String(imageArray); ;
            }
            else
            {
                img_path = Path.Combine(environment.WebRootPath, "Images", "company_profile");
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                lic.CompanyLogo = Convert.ToBase64String(imageArray);
            }

            //get Delivery customer details
            DeliveryCustomer dc = new DeliveryCustomer();
            cnn.Open();
            NpgsqlDataReader sdr2 = new NpgsqlCommand("Select * From \"SLCustomer\" WHERE \"SLCustomerSerial\" = " + inv.DeliveryCust + " ", cnn).ExecuteReader();
            if (sdr2.Read())
            {
                dc.CustFirstName = sdr2["CustFirstName"].ToString();
                dc.CustLastName = sdr2["CustLastName"].ToString();
                dc.CustEmail = sdr2["CustEmail"].ToString();
                dc.CustCompany = sdr2["CustCompany"].ToString();
                dc.CustType = sdr2["CustType"].ToString();
            }
            cnn.Close();

            //get invoice list details
            List<InvoiceDetails> invDetailsList = new List<InvoiceDetails>();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"SLInvoiceDetail\" WHERE \"SLJrnlNo\" = " + Int32.Parse(SL) + " ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                InvoiceDetails invDetail = new InvoiceDetails();
                invDetail.SLJrnlNo = (int)sdr3["SLJrnlNo"];
                invDetail.JrnlSLNo = (int)sdr3["JrnlSLNo"];
                invDetail.VatCode = (string)sdr3["VatCode"];
                invDetail.VatAmt = (float)(decimal)sdr3["VatAmt"];
                invDetail.ItemQty = (int)sdr3["ItemQty"];
                invDetail.ItemTotals = (float)(decimal)sdr3["ItemTotals"];
                invDetail.ItemUnitPrice = (float)(decimal)sdr3["ItemUnitPrice"];
                invDetail.Currency = inv.CrCode;
                invDetail.DiscountPerc = sdr3["DiscountPerc"] != DBNull.Value ? (float)(decimal)sdr3["DiscountPerc"] : 0;
                invDetail.DiscountAmt = sdr3["DiscountAmt"] != DBNull.Value ? (float)(decimal)sdr3["DiscountAmt"] : 0;
                invDetail.AdditionalDetails = sdr3["AdditionalDetails"] != DBNull.Value ? (string)sdr3["AdditionalDetails"] : null;
                invDetail.StkDesc = sdr3["ItemName"] != DBNull.Value ? (string)sdr3["ItemName"] : null;
                invDetail.ProdGroupCode = sdr3["ItemCode"] != DBNull.Value ? (string)sdr3["ItemCode"] : null;
                invDetailsList.Add(invDetail);
            }
            cnn.Close();
            //Get invoice terms and conditions
            Allterms invterms = new Allterms();
            cnn.Open();

            NpgsqlDataReader sdr4 = new NpgsqlCommand("SELECT * FROM \"AllSystemTerms\" WHERE \"tosType\" = 'inv_terms' ", cnn).ExecuteReader();
            while (sdr4.Read())
            {
                invterms.tosID = sdr4["tosID"] != DBNull.Value ? (int)sdr4["tosID"] : 0;
                invterms.tosType = sdr4["tosType"] != DBNull.Value ? (string)sdr4["tosType"] : null;
                invterms.terms = sdr4["terms"] != DBNull.Value ? sdr4["terms"].ToString() : null;

            }
            cnn.Close();

            //getting Credit Note Type
            List<CreditNoteType> creditNoteTypes = new List<CreditNoteType>();
            cnn.Open();
            NpgsqlDataReader reader = new NpgsqlCommand("SELECT * FROM \"CreditNoteTypes\" ", cnn).ExecuteReader();
            if(reader.HasRows == false)
            {
                return BadRequest(new { message = "Sorry! NO Credit Note types were found in your branch. Request terminated prematurely " });
            }
            while (reader.Read())
            {
                CreditNoteType credit = new CreditNoteType();
                credit.CRNId = reader["CRNId"] != DBNull.Value ? (int)reader["CRNId"] : 0;
                credit.CRNType = reader["CRNType"] != DBNull.Value ? (string)reader["CRNType"] : null;
                credit.CRNComment = reader["CRNComment"] != DBNull.Value ? (string)reader["CRNComment"] : null;
                creditNoteTypes.Add(credit);
            }
            cnn.Close();

            // Decode the content for showing on Web page.
            invterms.terms = WebUtility.HtmlDecode(invterms.terms);
            return Ok(new { InvoiceHeader = inv, CompanyData = lic, DeliveryCust = dc, InvoiceDetails = invDetailsList, OriginalInvoice = orig_inv, InvTerms = invterms, CRNTypeList = creditNoteTypes });
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
