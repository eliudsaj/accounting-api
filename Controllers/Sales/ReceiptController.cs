using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MimeKit;
using MimeKit.Text;
using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Models.ReportPages.Sales;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.Sales.Terms;
using MailKit.Net.Smtp;
using pyme_finance_api.Models.Email;
using pyme_finance_api.Models.Purchases.PurchaseReceipt;
using Microsoft.Extensions.Logging;
using pyme_finance_api.Service.AuditTrailService;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Controllers.NlController;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.PaymentService;
using pyme_finance_api.Common;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _hostingEnvironment;
        dbconnection myDbconnection = new dbconnection();
        private readonly ILogger<ReceiptController> _logger;

        public ReceiptController(IWebHostEnvironment environment, IConfiguration config, ILogger<ReceiptController> logger)
        {
            _configuration = config;
            _hostingEnvironment = environment;
            _logger = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult ReceiptGetInvoice(int parsedinvref)
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
            //get all Invoices
            Invoice inv = new Invoice();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("SELECT \"SLInvoiceHeader\".*, \"SLCustomerSerial\", \"CustFirstName\", \"CustLastName\",\"Address\",\"PostalAddress\",\"CustEmail\",\"CustContact\",\"CustCompany\", \"CustType\", \"CrId\", \"CrCode\" FROM \"SLInvoiceHeader\" " +
                " LEFT JOIN  \"SLCustomer\" ON ( \"CustId\" =  \"SLCustomerSerial\" ) " +
                " LEFT JOIN  \"Currencies\" ON ( \"CurrencyId\" =  \"CrId\" ) WHERE \"SLJrnlNo\" = " + parsedinvref + "  ", cnn).ExecuteReader();
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
                inv.DeliveryAddress = sdr["DeliveryAddress"].ToString();
            }
            cnn.Close();
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
            }
            cnn.Close();
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
            //get other receipts attached to that invoice
            List<SLReceipts> SalesreceiptsList = new List<SLReceipts>();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT \"pyID\",\"pyRef\",\"pyDate\",\"pyInvRef\",\"pyPayable\",\"pyPaid\",\"pyBalance\",\"pyMode\"," +
                "\"pyChequeNumber\",\"pyReceivedFrom\",\"pyAdditionalDetails\",\"pyProcessDate\",\"pyUser\",\"UFirstName\",\"ULastName\"," +
                "\"UId\" FROM \"SLReceipts\" LEFT JOIN \"Users\" ON (\"SLReceipts\".\"pyUser\" = \"Users\".\"UId\") " +
                "WHERE \"pyInvRef\" = " + parsedinvref + " ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                SLReceipts rcpDetail = new SLReceipts();
                rcpDetail.pyID = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyRef = sdr3["pyRef"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyDate = sdr3["pyDate"] != DBNull.Value ? (DateTime)sdr3["pyDate"] : DateTime.Now;
                rcpDetail.pyInvRef = sdr3["pyInvRef"] != DBNull.Value ? (int)sdr3["pyInvRef"] : 0;
                rcpDetail.pyPayable = sdr3["pyPayable"] != DBNull.Value ? (decimal)sdr3["pyPayable"] : 0;
                rcpDetail.pyPaid = sdr3["pyPaid"] != DBNull.Value ? (decimal)sdr3["pyPaid"] : 0;
                rcpDetail.pyBalance = sdr3["pyBalance"] != DBNull.Value ? (decimal)sdr3["pyBalance"] : 0;
                rcpDetail.pyMode = sdr3["pyMode"] != DBNull.Value ? (string)sdr3["pyMode"] : null;
                rcpDetail.pyChequeNumber = sdr3["pyChequeNumber"] != DBNull.Value ? (string)sdr3["pyChequeNumber"] : null;
                rcpDetail.pyReceivedFrom = sdr3["pyReceivedFrom"] != DBNull.Value ? (string)sdr3["pyReceivedFrom"] : null;
                rcpDetail.pyAdditionalDetails = sdr3["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3["pyAdditionalDetails"] : null;
                rcpDetail.pyProcessDate = sdr3["pyProcessDate"] != DBNull.Value ? (DateTime)sdr3["pyProcessDate"] : DateTime.Now;
                rcpDetail.pyUser = sdr3["pyUser"] != DBNull.Value ? (int)sdr3["pyUser"] : 0;
                rcpDetail.UFirstName = sdr3["UFirstName"] != DBNull.Value ? (string)sdr3["UFirstName"] : null;
                rcpDetail.ULastName = sdr3["ULastName"] != DBNull.Value ? (string)sdr3["ULastName"] : null;
                SalesreceiptsList.Add(rcpDetail);
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
            // Decode the content for showing on Web page.
            invterms.terms = WebUtility.HtmlDecode(invterms.terms);
            Console.WriteLine(SalesreceiptsList);
            _logger.LogInformation(SalesreceiptsList.ToString());
            return Ok(new { InvoiceHeader = inv, CompanyData = lic, DeliveryCust = dc, ReceiptDetails = SalesreceiptsList, InvTerms = invterms });
        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult addSalesReceipt(SLReceipts recvData)
        {
            _logger.LogInformation("Saving sales receipt");
            //validate data
            if (string.IsNullOrEmpty(recvData.pyDate.ToString()))
            {
                return BadRequest(new { message = "Missing receipt payment date." });
            }
            else if (string.IsNullOrEmpty(recvData.pyInvRef.ToString()) || recvData.pyInvRef == 0)
            {
                return BadRequest(new { message = "Missing or invalid receipt invoice reference." });
            }
            else if (string.IsNullOrEmpty(recvData.pyMode))
            {
                return BadRequest(new { message = "Missing receipt payment mode." });
            }
            else if (string.IsNullOrEmpty(recvData.pyChequeNumber) && recvData.pyMode == "cheque")
            {
                return BadRequest(new { message = "Missing cheque number for cheque payment" });
            }
            //else if (string.IsNullOrEmpty(recvData.pyReceivedFrom))
            //{
            //    return BadRequest(new { message = "Missing receipt received from details." });
            //}
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
            NlService nlService = new NlService(db);
            if (nlService.GetNLAccountAccountCodeUsingName(recvData.pyMode.ToUpper()).NlaccCode == null)
            {
                return BadRequest(new { message = $"Cant complete this transaction .Create account {recvData.pyMode.ToUpper()} in Nl Account module ." });
            }
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, db);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get last pyid
            int lastpyid = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select COALESCE(MAX(\"pyID\"), 0) as ref From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                lastpyid = (int)sdra["ref"];
            }
            cnn.Close();
            //get lat pyref
            int lastpyRef = 0;
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select COALESCE(MAX(\"pyRef\"), 0) as ref From \"SLReceipts\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                lastpyRef = (int)sdrb["ref"];
            }
            cnn.Close();
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdrLc = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
            if (sdrLc.Read())
            {
                lic.CompanyName = (string)sdrLc["CompanyName"];
                lic.CompanySlogan = (string)sdrLc["CompanySlogan"];
                lic.CompanyPostal = (string)sdrLc["CompanyPostal"];
                lic.CompanyContact = (string)sdrLc["CompanyContact"];
                lic.CompanyVAT = (string)sdrLc["CompanyVAT"];
                lic.PhysicalAddress = (string)sdrLc["PhysicalAddress"];
                lic.CompanyLogo = (string)sdrLc["CompanyLogo"];
            }
            cnn.Close();
            //get invoice data from inv ref
            Invoice inv = new Invoice();
            cnn.Open();
            NpgsqlDataReader sdr = new NpgsqlCommand("SELECT \"SLInvoiceHeader\". * ,\"Currencies\".\"CrCode\", \"SLCustomer\".\"CustFirstName\",\"SLCustomer\".\"CustLastName\",\"SLCustomer\".\"CustEmail\",\"Users\".\"UFirstName\",\"Users\".\"ULastName\",\"SLCustomer\".\"CustCompany\",\"SLCustomer\".\"CustType\" FROM \"SLInvoiceHeader\" LEFT JOIN \"Currencies\" ON (\"Currencies\".\"CrId\" = \"SLInvoiceHeader\".\"CurrencyId\") LEFT JOIN \"SLCustomer\" ON (\"SLCustomer\".\"SLCustomerSerial\" = \"SLInvoiceHeader\".\"CustId\") LEFT JOIN \"Users\" ON (\"Users\".\"UId\" = \"SLInvoiceHeader\".\"StaffID\")  WHERE \"SLJrnlNo\" = " + recvData.pyInvRef + "  ", cnn).ExecuteReader();
            if (sdr.Read())
            {
                //invoice
                inv.SLJrnlNo = (int)sdr["SLJrnlNo"];
                inv.TotalAmount = (decimal)sdr["TotalAmount"];
                inv.DocPrefix = (string)sdr["DocPrefix"];
                inv.DocRef = (int)sdr["DocRef"];
                inv.INVDate = (DateTime)sdr["INVDate"];
                inv.PaymentDays = (int)sdr["PaymentDays"];
                inv.DueDate = (DateTime)sdr["DueDate"];
                inv.DeliveryAddress = sdr["DeliveryAddress"] != DBNull.Value ? sdr["DeliveryAddress"].ToString() : null;
                //currency
                inv.CrCode = (string)sdr["CrCode"];
                //customer
                inv.CustType = (string)sdr["CustType"];
                inv.CustFirstName = sdr["CustFirstName"] != DBNull.Value ? sdr["CustFirstName"].ToString() : null;
                inv.CustLastname = sdr["CustLastname"] != DBNull.Value ? sdr["CustLastname"].ToString() : null;
                inv.CustCompany = sdr["CustCompany"] != DBNull.Value ? sdr["CustCompany"].ToString() : null;
                inv.CustEmail = (string)sdr["CustEmail"];
                //staff
                inv.UFirstName = (string)sdr["UFirstName"];
                inv.ULastName = (string)sdr["ULastName"];
            }
            cnn.Close();
            //get staff details
            Users staff = new Users();
            cnn.Open();
            NpgsqlDataReader sdrStff = new NpgsqlCommand("SELECT * From \"Users\" WHERE \"UId\" = " + userId + "  ", cnn).ExecuteReader();
            if (sdrStff.Read())
            {
                staff.UFirstName = (string)sdrStff["UFirstName"];
                staff.ULastName = (string)sdrStff["ULastName"];
            }
            cnn.Close();
            //get all receipts paid if exists
            decimal totalsReceiptsPaid = 0;
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT * FROM \"SLReceipts\" WHERE \"pyInvRef\" = " + recvData.pyInvRef + " AND \"pyBranch\" = " + staff_branch + " ", cnn).ExecuteReader();
            while (sdr3.Read())
            {
                totalsReceiptsPaid += sdr3["pyPaid"] != DBNull.Value ? (decimal)(float)sdr3["pyPaid"] : 0;
            }
            cnn.Close();
            //get amount payable
            decimal amtPayable = inv.TotalAmount - totalsReceiptsPaid;
            //compute balance
            decimal rcpBalance = amtPayable - (decimal)recvData.pyPaid;
            string chqDetails = "";
            if (recvData.pyMode == "cheque")
            {
                chqDetails = "Cheque number " + recvData.pyChequeNumber;
            }
            else
            {
                chqDetails = "";
            }
            string myQuery = "SELECT \"SLCustomer\".*,\"CrId\",\"CrCode\" FROM \"SLCustomer\" LEFT JOIN \"Currencies\" ON ( \"CurCode\" = \"CrId\") WHERE \"CustEmail\" = '" + inv.CustEmail + "'; ";
            AddCustomer addCust = new AddCustomer();
            cnn.Open();
            NpgsqlDataReader sdr4 = new NpgsqlCommand(myQuery, cnn).ExecuteReader();
            while (sdr4.Read())
            {
                addCust.SLCustomerSerial = (int)sdr4["SLCustomerSerial"];
                addCust.CustCode = sdr4["CustCode"].ToString();
                addCust.CustFirstName = sdr4["CustFirstName"] != DBNull.Value ? (string)sdr4["CustFirstName"] : null;
                addCust.Address = sdr4["Address"].ToString();
                addCust.PostalAddress = sdr4["PostalAddress"].ToString();
                addCust.PostalAddress = sdr4["PostalAddress"].ToString();
                addCust.CurCode = (int)sdr4["CurCode"];
                addCust.CustEmail = sdr4["CustEmail"].ToString();
                addCust.CustContact = sdr4["CustContact"].ToString();
                addCust.SLCTypeID = (int)sdr4["SLCTypeID"];
                addCust.CustLastName = sdr4["CustLastName"] != DBNull.Value ? (string)sdr4["CustLastName"] : null;
                addCust.CustType = sdr4["CustType"].ToString();
                addCust.CustCompany = sdr4["CustCompany"] != DBNull.Value ? (string)sdr4["CustCompany"] : null;
                addCust.VATNo = sdr4["VATNo"] != DBNull.Value ? (string)sdr4["VATNo"] : null;
                addCust.CustCreditLimit = (float)sdr4["CustCreditLimit"];
                addCust.VATpin = sdr4["VATpin"].ToString();
                addCust.CreditTerms = (int)sdr4["CreditTerms"];
                addCust.CurrCode = sdr4["CrCode"].ToString();
                addCust.CustBranch = sdr4["CustBranch"] != DBNull.Value ? (int)sdr4["CustBranch"] : 0;
                addCust.CustRef = sdr4["CustRef"] != DBNull.Value ? (string)sdr4["CustRef"] : null;
                addCust.CustomerDept = sdr4["CustomerDept"] != DBNull.Value ? (decimal)sdr4["CustomerDept"] : 0;
            }

            cnn.Close();
            _logger.LogInformation($"Sending receipt to customer's email {inv.CustEmail}");
            //Email
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplates", "SalesReceiptPayment.html");
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
            string receiptRef = "PYT" + (lastpyRef + 1).ToString("D4");
            string desc = "";
            if (inv.TotalAmount > (decimal)recvData.pyPaid)
            {
                desc = "partial payment of " + inv.DocPrefix + inv.SLJrnlNo;
            }
            else
            {
                desc = "payment of " + inv.DocPrefix + inv.SLJrnlNo;
            }
            string messageBody = string.Format(builder.HtmlBody,
                inv.DocPrefix + inv.SLJrnlNo,
                String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),

                lic.CompanyLogo,

                lic.CompanyName,
                lic.CompanyPostal,
                lic.CompanyContact,
                lic.PhysicalAddress,

                receiptRef,
                inv.CrCode + " " + recvData.pyPaid.ToString("N"),
                desc,
                inv.DocPrefix + inv.SLJrnlNo.ToString("D4"),
                inv.INVDate.ToString("dd/MM/yyyy"),
                inv.CrCode + " " + inv.TotalAmount,
                inv.PaymentDays,
                inv.DueDate.ToString("dd/MM/yyyy"),
                inv.UFirstName + " " + inv.ULastName,
                recvData.pyReceivedFrom,
                receiptRef,
                inv.DeliveryAddress,
                 inv.CrCode + " " + recvData.pyPaid,
                 desc,
                 inv.CustFirstName + " " + inv.CustLastname,
                 recvData.pyMode,
                 chqDetails,
                 inv.CrCode + " " + amtPayable,
                 inv.CrCode + " " + recvData.pyPaid,
                 inv.CrCode + " " + rcpBalance,
                 staff.UFirstName + " " + staff.ULastName
            );

            //prepare email
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("pymetest@ngenx.io"));
            email.To.Add(MailboxAddress.Parse(inv.CustEmail));
            email.Subject = "Payment Receipt " + receiptRef;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = messageBody
            };
            // send email
            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            smtp.Connect("smtp.munshiram.com", 587, SecureSocketOptions.Auto);
            smtp.Authenticate("pymetest@ngenx.io", "S-85d9v7");
            smtp.Send(email);
            smtp.Disconnect(true);

            _logger.LogInformation($"Saving receipt data of invoice   {inv.DocPrefix + inv.SLJrnlNo.ToString("D4")}");
            //save details
            string insertQuery = "INSERT INTO \"SLReceipts\" (\"pyID\", \"pyRef\", \"pyDate\", \"pyInvRef\", \"pyPayable\", \"pyPaid\", \"pyBalance\", \"pyMode\", \"pyChequeNumber\", \"pyReceivedFrom\", \"pyAdditionalDetails\", \"pyProcessDate\", \"pyUser\",\"pyBranch\" )  " +
                "VALUES(" + (lastpyid + 1) + ", " + (lastpyRef + 1) + ", '" + recvData.pyDate + "', " + inv.SLJrnlNo + ", " + amtPayable + ", " + recvData.pyPaid + "," + rcpBalance + ",'" + recvData.pyMode + "', '" + recvData.pyChequeNumber + "', '" + recvData.pyReceivedFrom + "','" + recvData.pyAdditionalDetails + "', '" + DateTime.Now + "'," + userId + "," + staff_branch + " ); ";

            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery, db);
            string update_inv;
            // reduce inv balance
            //Mark as dispute add reason update lastcrn invoicetypeRef and update reasons
            update_inv = "UPDATE \"SLInvoiceHeader\" SET   \"TotalBalance\" = \"TotalBalance\" - " + recvData.pyPaid + "  WHERE \"SLJrnlNo\" = " + recvData.pyInvRef + ";  ";
            bool myReq2 = myDbconnection.UpdateDelInsert(update_inv, db);
            if (myReq1 == true)
            {
                if (myReq2 == true)
                {
                    NlJournalHeader nlJournalHeader = new NlJournalHeader();
                    nlJournalHeader.NlJrnlDesc = "";
                    nlJournalHeader.TranDate = DateTime.Now;
                    nlJournalHeader.MEndDate = inv.DueDate;
                    nlJournalHeader.TranYear = DateTime.Now.Year;
                    nlJournalHeader.TranPeriod = DateTime.Now.Month;
                    nlJournalHeader.TranType = "";
                    nlJournalHeader.TranFrom = "";
                    nlJournalHeader.ModuleId = null;
                    nlJournalHeader.SlJrnlNo = recvData.pyInvRef;
                    _logger.LogInformation($"Updating Journal Accounts ");
                    var response = nlService.saveSalesReceiptsAccount(nlJournalHeader, recvData.pyMode.ToUpper(), (float)recvData.pyPaid, addCust);
                    if (response.Httpcode == 400)
                    {
                        _logger.LogError($"Error failed to update journals due to {response.Message} ");
                        return BadRequest(new { message = response.Message });
                    }
                    AuditTrailService auditTrailService = new AuditTrailService(tokenData);
                    AuditTrail auditTrail = new AuditTrail();
                    auditTrail.action = "Created  Sales Receipt from invoice  ref INV" + inv.SLJrnlNo + "  at " + DateTime.Now.ToString("dd-MM-yyyy") + " worth " + recvData.pyPaid + "from" + recvData.pyReceivedFrom;
                    auditTrail.module = "Sales Receipt";
                    auditTrail.userId = userId;
                    auditTrailService.createAuditTrail(auditTrail);
                    return Ok(new { message = "Receipt has been successfully created" });
                }
                else
                {
                    _logger.LogError($"Error in updating invoice {recvData.DocRef} balance");
                    //failed
                    return BadRequest(new { message = "An occured while trying to create your payment receipt." });
                }
            }
            else
            {
                _logger.LogError($"Error in saving receipt of invoice {recvData.DocRef}");
                //failed
                return BadRequest(new { message = "An occured while trying to create your payment receipt." });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> savePurchasePayment(ReceivePaymentRequest receivePaymentRequest)
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

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, tokenData);
            if (staff_branch == 0)
            {
                return BadRequest(new {  message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"});
            }
            PaymentService paymentService = new PaymentService(tokenData);
            // var response = paymentService.savepayment(receivePaymentRequest, userId, staff_branch);
            var response = await paymentService.allocatedPurchaseInvoicesV2(receivePaymentRequest, userId, staff_branch);
            if (response.Httpcode == 400)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> SavePayment(ReceivePaymentRequest receivePaymentRequest)
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
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, tokenData);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            PaymentService paymentService = new PaymentService(tokenData);
            // var response = paymentService.savepayment(receivePaymentRequest, userId, staff_branch);
            var response = await paymentService.allocatedInvoicesV3(receivePaymentRequest, userId, staff_branch);
            if (response.Httpcode == 400)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetReceiptDetailsById(int receipt_id)
        {
            //check if company code exists
            var companyRes = "";
            int userId = 0;
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
            //get all Invoices
            Invoice inv = new Invoice();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get other receipts attached to that invoice
            cnn.Open();
            string slquery = "SELECT \"SLReceipts\".*, \"SLCustomer\".\"CustFirstName\", \"SLCustomer\".\"CustLastName\",\"SLCustomer\".\"CustEmail\",\"SLInvoiceHeader\".\"INVDate\", \"SLInvoiceHeader\".\"TotalAmount\",  \"Users\".\"UFirstName\" as Fname , " +
                " \"SLCustomer\".\"CustCompany\",\"SLCustomer\".\"CustType\", \"SLCustomer\".\"Address\", \"SLInvoiceHeader\".\"PaymentDays\",  \"SLInvoiceHeader\".\"DueDate\", \"SLInvoiceHeader\".\"DocRef\",\"SLInvoiceHeader\".\"DocPrefix\",\"SLInvoiceHeader\".\"SLJrnlNo\",\"Currencies\".\"CrCode\"   " +
                "FROM \"SLReceipts\" LEFT JOIN \"SLInvoiceHeader\" ON \"SLInvoiceHeader\".\"SLJrnlNo\" = \"SLReceipts\".\"pyInvRef\" LEFT JOIN \"Users\" ON  \"Users\".\"UId\" = \"SLInvoiceHeader\".\"StaffID\"  INNER JOIN \"SLCustomer\" ON \"SLCustomer\".\"SLCustomerSerial\" = \"SLInvoiceHeader\".\"CustId\" " +
                "INNER JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"SLInvoiceHeader\".\"CurrencyId\"" +
                "WHERE \"SLReceipts\".\"pyID\" = " + receipt_id + "  ";

            //NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT \"pyID\",\"pyRef\",\"pyDate\",\"pyInvRef\",\"pyPayable\",\"pyPaid\",\"pyBalance\",\"pyMode\",\"pyChequeNumber\",\"pyReceivedFrom\",\"pyAdditionalDetails\",\"pyProcessDate\",\"pyUser\",\"UFirstName\",\"ULastName\",\"UId\" FROM \"SLReceipts\" LEFT JOIN \"Users\" ON (\"SLReceipts\".\"pyUser\" = \"Users\".\"UId\") ", cnn).ExecuteReader();
            NpgsqlDataReader sdr3 = new NpgsqlCommand(slquery, cnn).ExecuteReader();
            SLReceipts rcpDetail = new SLReceipts();
            while (sdr3.Read())
            {
                rcpDetail.pyID = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.cust_id = sdr3["cust_id"] != DBNull.Value ? (int)sdr3["cust_id"] : 0;
                rcpDetail.pyRef = sdr3["pyRef"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyDate = sdr3["pyDate"] != DBNull.Value ? (DateTime)sdr3["pyDate"] : DateTime.Now;
                rcpDetail.pyInvRef = sdr3["pyInvRef"] != DBNull.Value ? (int)sdr3["pyInvRef"] : 0;
                rcpDetail.pyPayable = sdr3["pyPayable"] != DBNull.Value ? (decimal)sdr3["pyPayable"] : 0;
                rcpDetail.pyPaid = sdr3["pyPaid"] != DBNull.Value ? (decimal)sdr3["pyPaid"] : 0;
                rcpDetail.pyBalance = sdr3["pyBalance"] != DBNull.Value ? (decimal)sdr3["pyBalance"] : 0;
                rcpDetail.pyMode = sdr3["pyMode"] != DBNull.Value ? (string)sdr3["pyMode"] : null;
                rcpDetail.pyChequeNumber = sdr3["pyChequeNumber"] != DBNull.Value ? (string)sdr3["pyChequeNumber"] : null;
                rcpDetail.pyReceivedFrom = sdr3["pyReceivedFrom"] != DBNull.Value ? (string)sdr3["pyReceivedFrom"] : null;
                rcpDetail.pyAdditionalDetails = sdr3["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3["pyAdditionalDetails"] : null;
                rcpDetail.pyProcessDate = sdr3["pyProcessDate"] != DBNull.Value ? (DateTime)sdr3["pyProcessDate"] : DateTime.Now;
                rcpDetail.pyUser = sdr3["pyUser"] != DBNull.Value ? (int)sdr3["pyUser"] : 0;
                rcpDetail.allocation_remainder = sdr3["allocation_remainder"] != DBNull.Value ? (decimal)sdr3["allocation_remainder"] : 0;
                rcpDetail.rate = sdr3["rate"] != DBNull.Value ? (decimal)sdr3["rate"] : 0;
                //customer details
                rcpDetail.CustFirstName = sdr3["CustFirstName"] != DBNull.Value ? (string)sdr3["CustFirstName"] : null;
                rcpDetail.CustLastName = sdr3["CustLastName"] != DBNull.Value ? (string)sdr3["CustLastName"] : null;
                rcpDetail.CustEmail = sdr3["CustEmail"] != DBNull.Value ? (string)sdr3["CustEmail"] : null;
                rcpDetail.CustCompany = sdr3["CustCompany"] != DBNull.Value ? (string)sdr3["CustCompany"] : null;
                rcpDetail.CustType = sdr3["CustType"] != DBNull.Value ? (string)sdr3["CustType"] : null;
                rcpDetail.CustAddress = sdr3["Address"] != DBNull.Value ? (string)sdr3["Address"] : null;
                rcpDetail.currentCustName = sdr3["currentCustName"] != DBNull.Value ? (string)sdr3["currentCustName"] : null;
                //currency
                rcpDetail.CrCode = sdr3["CrCode"] != DBNull.Value ? (string)sdr3["CrCode"] : null;
                //invoice
                rcpDetail.DocPrefix = sdr3["DocPrefix"] != DBNull.Value ? (string)sdr3["DocPrefix"] : null;
                rcpDetail.DocRef = sdr3["DocRef"] != DBNull.Value ? (int)sdr3["DocRef"] : 0;
                rcpDetail.SLJrnlNo = (int)sdr3["SLJrnlNo"];
                rcpDetail.invDate = (DateTime)sdr3["INVDate"];
                rcpDetail.invAmount = sdr3["TotalAmount"] != DBNull.Value ? (decimal)sdr3["TotalAmount"] : 0;
                rcpDetail.dueDate = (DateTime)sdr3["DueDate"];
                rcpDetail.paymentdays = (int)sdr3["PaymentDays"];
                rcpDetail.fname = (string)sdr3["Fname"];
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
            //string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "profiles");
            //string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
            //byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
            //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            //lic.CompanyLogo = base64ImageRepresentation;
            return Ok(new { receipt = rcpDetail, companydata = lic, Code = 200 });
        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetAll(int parsedinvref)
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
            //get all Invoices
            Invoice inv = new Invoice();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get other receipts attached to that invoice
            List<SLReceipts> SalesreceiptsList = new List<SLReceipts>();
            cnn.Open();
            string slquery = "SELECT \"SLReceipts\".*, \"SLCustomer\".\"CustFirstName\", \"SLCustomer\".\"CustLastName\",\"SLCustomer\".\"CustEmail\",\"SLCustomer\".\"CustCompany\",\"SLCustomer\".\"CustType\",   " +
                "\"SLInvoiceHeader\".\"DocRef\",\"SLInvoiceHeader\".\"DocPrefix\",\"SLInvoiceHeader\".\"SLJrnlNo\",\"Currencies\".\"CrCode\"    " +
                "FROM \"SLReceipts\" LEFT JOIN \"SLInvoiceHeader\" ON \"SLInvoiceHeader\".\"SLJrnlNo\" = \"SLReceipts\".\"pyInvRef\" LEFT JOIN \"SLCustomer\" ON \"SLCustomer\".\"CustCode\" = \"SLInvoiceHeader\".\"CustCode\"    " +
                "INNER JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"SLInvoiceHeader\".\"CurrencyId\"   ";

            //NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT \"pyID\",\"pyRef\",\"pyDate\",\"pyInvRef\",\"pyPayable\",\"pyPaid\",\"pyBalance\",\"pyMode\",\"pyChequeNumber\",\"pyReceivedFrom\",\"pyAdditionalDetails\",\"pyProcessDate\",\"pyUser\",\"UFirstName\",\"ULastName\",\"UId\" FROM \"SLReceipts\" LEFT JOIN \"Users\" ON (\"SLReceipts\".\"pyUser\" = \"Users\".\"UId\") ", cnn).ExecuteReader();
            NpgsqlDataReader sdr3 = new NpgsqlCommand(slquery, cnn).ExecuteReader();
            while (sdr3.Read())
            {
                SLReceipts rcpDetail = new SLReceipts();
                rcpDetail.pyID = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.cust_id = sdr3["cust_id"] != DBNull.Value ? (int)sdr3["cust_id"] : 0;
                rcpDetail.pyRef = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyDate = sdr3["pyDate"] != DBNull.Value ? (DateTime)sdr3["pyDate"] : DateTime.Now;
                rcpDetail.pyInvRef = sdr3["pyInvRef"] != DBNull.Value ? (int)sdr3["pyInvRef"] : 0;
                rcpDetail.pyPayable = sdr3["pyPayable"] != DBNull.Value ? (decimal)sdr3["pyPayable"] : 0;
                rcpDetail.pyPaid = sdr3["pyPaid"] != DBNull.Value ? (decimal)sdr3["pyPaid"] : 0;
                rcpDetail.pyBalance = sdr3["pyBalance"] != DBNull.Value ? (decimal)sdr3["pyBalance"] : 0;
                rcpDetail.pyMode = sdr3["pyMode"] != DBNull.Value ? (string)sdr3["pyMode"] : null;
                rcpDetail.pyChequeNumber = sdr3["pyChequeNumber"] != DBNull.Value ? (string)sdr3["pyChequeNumber"] : null;
                rcpDetail.pyReceivedFrom = sdr3["pyReceivedFrom"] != DBNull.Value ? (string)sdr3["pyReceivedFrom"] : null;
                rcpDetail.pyAdditionalDetails = sdr3["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3["pyAdditionalDetails"] : null;
                rcpDetail.pyProcessDate = sdr3["pyProcessDate"] != DBNull.Value ? (DateTime)sdr3["pyProcessDate"] : DateTime.Now;
                rcpDetail.pyUser = sdr3["pyUser"] != DBNull.Value ? (int)sdr3["pyUser"] : 0;
                rcpDetail.rate = sdr3["rate"] != DBNull.Value ? (decimal)sdr3["rate"] : 0;
                //customer details
                rcpDetail.CustFirstName = sdr3["CustFirstName"] != DBNull.Value ? (string)sdr3["CustFirstName"] : null;
                rcpDetail.CustLastName = sdr3["CustLastName"] != DBNull.Value ? (string)sdr3["CustLastName"] : null;
                rcpDetail.CustEmail = sdr3["CustEmail"] != DBNull.Value ? (string)sdr3["CustEmail"] : null;
                rcpDetail.CustCompany = sdr3["CustCompany"] != DBNull.Value ? (string)sdr3["CustCompany"] : null;
                rcpDetail.CustType = sdr3["CustType"] != DBNull.Value ? (string)sdr3["CustType"] : null;
                rcpDetail.currentCustName = sdr3["currentCustName"] != DBNull.Value ? (string)sdr3["currentCustName"] : null;
                //currency
                rcpDetail.CrCode = sdr3["CrCode"] != DBNull.Value ? (string)sdr3["CrCode"] : null;
                //invoice
                rcpDetail.DocPrefix = sdr3["DocPrefix"] != DBNull.Value ? (string)sdr3["DocPrefix"] : null;
                rcpDetail.DocRef = sdr3["DocRef"] != DBNull.Value ? (int)sdr3["DocRef"] : 0;
                rcpDetail.SLJrnlNo = sdr3["SLJrnlNo"] != DBNull.Value ? (int)sdr3["SLJrnlNo"] : 0;
                SalesreceiptsList.Add(rcpDetail);
            }
            cnn.Close();
            return Ok(new { AllReceipts = SalesreceiptsList });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult PL_GetAll_PLINV(int parsedinvref)
        {
            if (parsedinvref == 0)
            {
                return BadRequest(new { message = "Cannot finf required reference to process request" });
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
                return BadRequest(new { message ="Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator"});
            }
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get all receipts attached to that invoice
            List<PLReceipts> pl_receiptsList = new List<PLReceipts>();
            cnn.Open();
            NpgsqlDataReader sdr3_p = new NpgsqlCommand("SELECT \"PLReceipts\".*, \"UFirstName\",\"ULastName\",\"UId\" FROM \"PLReceipts\" LEFT JOIN \"Users\" ON (\"PLReceipts\".\"pyUser\" = \"Users\".\"UId\") WHERE \"pyInvRef\" = " + parsedinvref + " ", cnn).ExecuteReader();
            while (sdr3_p.Read())
            {
                PLReceipts rcpDetail = new PLReceipts();
                rcpDetail.pyID = sdr3_p["pyID"] != DBNull.Value ? (int)sdr3_p["pyID"] : 0;
                rcpDetail.pyRef = sdr3_p["pyRef"] != DBNull.Value ? (int)sdr3_p["pyRef"] : 0;
                rcpDetail.pyDate = sdr3_p["pyDate"] != DBNull.Value ? (DateTime)sdr3_p["pyDate"] : DateTime.Now;
                rcpDetail.pyInvRef = sdr3_p["pyInvRef"] != DBNull.Value ? (int)sdr3_p["pyInvRef"] : 0;
                rcpDetail.pyPayable = sdr3_p["pyPayable"] != DBNull.Value ? (float)sdr3_p["pyPayable"] : 0;
                rcpDetail.pyPaid = sdr3_p["pyPaid"] != DBNull.Value ? (float)sdr3_p["pyPaid"] : 0;
                rcpDetail.pyBalance = sdr3_p["pyBalance"] != DBNull.Value ? (float)sdr3_p["pyBalance"] : 0;
                rcpDetail.pyMode = sdr3_p["pyMode"] != DBNull.Value ? (string)sdr3_p["pyMode"] : null;
                rcpDetail.pyChequeNumber = sdr3_p["pyChequeNumber"] != DBNull.Value ? (string)sdr3_p["pyChequeNumber"] : null;
                rcpDetail.pyReceivedBy = sdr3_p["pyReceivedBy"] != DBNull.Value ? (string)sdr3_p["pyReceivedBy"] : null;
                rcpDetail.pyAdditionalDetails = sdr3_p["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3_p["pyAdditionalDetails"] : null;
                rcpDetail.pyProcessDate = sdr3_p["pyProcessDate"] != DBNull.Value ? (DateTime)sdr3_p["pyProcessDate"] : DateTime.Now;
                rcpDetail.pyUser = sdr3_p["pyUser"] != DBNull.Value ? (int)sdr3_p["pyUser"] : 0;
                rcpDetail.pyCancelled = sdr3_p["pyCancelled"] != DBNull.Value ? (bool)sdr3_p["pyCancelled"] : false;
                rcpDetail.pyCancelReason = sdr3_p["pyCancelReason"] != DBNull.Value ? (string)sdr3_p["pyCancelReason"] : null;
                rcpDetail.pyBranch = sdr3_p["pyBranch"] != DBNull.Value ? (int)sdr3_p["pyBranch"] : 0;
                rcpDetail.UFirstName = sdr3_p["UFirstName"] != DBNull.Value ? (string)sdr3_p["UFirstName"] : null;
                rcpDetail.ULastName = sdr3_p["ULastName"] != DBNull.Value ? (string)sdr3_p["ULastName"] : null;
                pl_receiptsList.Add(rcpDetail);
            }
            cnn.Close();
            return Ok(new { ReceiptDetails = pl_receiptsList });
        }

        [Route("[action]")]
        [HttpPost]

        public async Task<ActionResult> AllocatePurchaseInvoiceFromPayment(ReceivePaymentRequest receivePaymentRequest)
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
            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(userId, tokenData);
            if (staff_branch == 0)
            {
                return BadRequest(new { message = "Sorry! We have problems getting your branch details. Request failed. Please contact your system administrator" });
            }
            decimal sum = receivePaymentRequest.AllocatedInvoices.Sum(A => A.AR);
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(tokenData));
            //get other receipts attached to that invoice
            cnn.Open();
            string slquery = "SELECT  \"PLReceipts\".*  FROM \"PLReceipts\" WHERE \"PLReceipts\".\"pyID\" = " + receivePaymentRequest.ReceiptId + "  ";

            //NpgsqlDataReader sdr3 = new NpgsqlCommand("SELECT \"pyID\",\"pyRef\",\"pyDate\",\"pyInvRef\",\"pyPayable\",\"pyPaid\",\"pyBalance\",\"pyMode\",\"pyChequeNumber\",\"pyReceivedFrom\",\"pyAdditionalDetails\",\"pyProcessDate\",\"pyUser\",\"UFirstName\",\"ULastName\",\"UId\" FROM \"SLReceipts\" LEFT JOIN \"Users\" ON (\"SLReceipts\".\"pyUser\" = \"Users\".\"UId\") ", cnn).ExecuteReader();
            NpgsqlDataReader sdr3 = new NpgsqlCommand(slquery, cnn).ExecuteReader();
            SLReceipts rcpDetail = new SLReceipts();
            while (sdr3.Read())
            {
                rcpDetail.pyID = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.allocation_remainder = sdr3["allocation_remainder"] != DBNull.Value ? (decimal)sdr3["allocation_remainder"] : 0;
            }
            cnn.Close();
            if (sum > rcpDetail.allocation_remainder)
            {
                return BadRequest(new { message = "Allocation amount is greater than the receipt balance" });
            }
            PaymentService paymentService = new PaymentService(tokenData);
            MyResponse response = new MyResponse();
            // var response = paymentService.savepayment(receivePaymentRequest, userId, staff_branch);
            if (receivePaymentRequest.AllocatedInvoices == null)
            {
                return BadRequest(new { message = "Sorry ,No Invoices were found in this request" });
            }
            else
            {
                response = await paymentService.allocatedInvoicesFromPurchaseReceipt(receivePaymentRequest, userId, receivePaymentRequest.ReceiptId);
            }
            if (response.Httpcode == 400)
            {
                return BadRequest(new { message = response.Message });
            }
            AuditTrailService auditTrailService = new AuditTrailService(tokenData);
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.action = "Allocated Purchase Receipt " + receivePaymentRequest.ReceiptId + "  at " + DateTime.Now.ToString("dd/MM/yyyy") + " of value" + String.Format("{0:n}", receivePaymentRequest.Amount);
            auditTrail.module = "Purchase Receipt";
            auditTrail.userId = userId;
            auditTrailService.createAuditTrail(auditTrail);
            return Ok(new { message = response.Message });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPurchaseReceiptDetailsById(int receipt_id)
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

            //get all Invoices
            Invoice inv = new Invoice();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));
            //get other receipts attached to that invoice
            cnn.Open();
            string slquery = "SELECT \"PLReceipts\".*, \"Currencies\".\"CrCode\" ,\"PLCustomer\".\"CustName\" FROM \"PLReceipts\" LEFT JOIN \"PLCustomer\" ON \"PLCustomer\".\"CustID\" = \"PLReceipts\".\"supplier_id\" LEFT JOIN \"Currencies\" ON \"Currencies\".\"CrId\" = \"PLCustomer\".\"CurrID\" " +
                "WHERE \"PLReceipts\".\"pyID\" = " + receipt_id + "  ";

            NpgsqlDataReader sdr3 = new NpgsqlCommand(slquery, cnn).ExecuteReader();
            SLReceipts rcpDetail = new SLReceipts();
            while (sdr3.Read())
            {
                rcpDetail.pyID = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyRef = sdr3["pyID"] != DBNull.Value ? (int)sdr3["pyID"] : 0;
                rcpDetail.pyDate = sdr3["pyDate"] != DBNull.Value ? (DateTime)sdr3["pyDate"] : DateTime.Now;
                rcpDetail.pyInvRef = sdr3["pyInvRef"] != DBNull.Value ? (int)sdr3["pyInvRef"] : 0;
                rcpDetail.pyPayable = sdr3["pyPayable"] != DBNull.Value ? (decimal)(float)sdr3["pyPayable"] : 0;
                rcpDetail.cust_id = sdr3["supplier_id"] != DBNull.Value ? (int)sdr3["supplier_id"] : 0;
                rcpDetail.pyPaid = sdr3["pyPaid"] != DBNull.Value ? (decimal)(float)sdr3["pyPaid"] : 0;
                rcpDetail.pyBalance = sdr3["pyBalance"] != DBNull.Value ? (decimal)(float)sdr3["pyBalance"] : 0;
                rcpDetail.pyMode = sdr3["pyMode"] != DBNull.Value ? (string)sdr3["pyMode"] : null;
                rcpDetail.pyChequeNumber = sdr3["pyChequeNumber"] != DBNull.Value ? (string)sdr3["pyChequeNumber"] : null;
                //rcpDetail.pyReceivedFrom = sdr3["pyReceivedFrom"] != DBNull.Value ? (string)sdr3["pyReceivedFrom"] : null;
                rcpDetail.pyAdditionalDetails = sdr3["pyAdditionalDetails"] != DBNull.Value ? (string)sdr3["pyAdditionalDetails"] : null;
                rcpDetail.pyProcessDate = sdr3["pyDate"] != DBNull.Value ? (DateTime)sdr3["pyDate"] : DateTime.Now;

                rcpDetail.pyUser = sdr3["pyUser"] != DBNull.Value ? (int)sdr3["pyUser"] : 0;
                rcpDetail.currentCustName = sdr3["CustName"] != DBNull.Value ? (string)sdr3["CustName"] : null;
                //currency
                rcpDetail.CrCode = sdr3["CrCode"] != DBNull.Value ? (string)sdr3["CrCode"] : null;
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

            return Ok(new { receipt = rcpDetail, companydata = lic, Code = 200 } );
        }
    }
}
