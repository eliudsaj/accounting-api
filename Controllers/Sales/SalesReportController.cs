using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.JWT;
using pyme_finance_api.Service.AuthService;
using pyme_finance_api.Service.NlServices;
using pyme_finance_api.Service.SaleReport;

namespace pyme_finance_api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesReportController : ControllerBase
    {
        private IConfiguration _configuration;
        dbconnection myDbconnection = new dbconnection();
        public SalesReportController(IConfiguration config)
        {
            _configuration = config;
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult detailedaccountsReceivableAgeing(string custcode)
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
            string db = companyRes;
            SaleReport saleReport = new SaleReport(db);
            var data = saleReport.detaileduserAgeingReport(custcode);
            var lic = saleReport.GetCompanyDetails();
            return Ok(new { detailedaccountsreceivableageingreport = data, license = lic, Code = 200 });
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult accountsReceivableAgeing()
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
            string db = companyRes;
            SaleReport saleReport = new SaleReport(db);
            var data = saleReport.getAccountReceivableAgeingReport();
            var lic = saleReport.GetCompanyDetails();
            return Ok(new { accountsreceivableageingreport = data, license = lic, Code = 200});
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult sendCustomerStatement(int cust_id)
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
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}
            //get database name
            string db = companyRes;
            SaleReport saleReport = new SaleReport(db);
            NlService nlService = new NlService(db);
            AuthService authService = new AuthService();
           var data = saleReport.sendCusromerStatement(cust_id);
            var customer = nlService.getCustomerById(cust_id);
            var lic = saleReport.GetCompanyDetails();
            return Ok(new {customerstatement = data ,customerinfo = customer,companyinfo = lic });
        }


        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult sendCustomerLedgerCard(int cust_id, DateTime from, DateTime to)
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
            //bool perStatus = myDbconnection.CheckRights(companyRes, permissionName, userId);
            //if (perStatus == false)
            //{
            //    return BadRequest(new { message = "Sorry! you are denied access to this action or information. Please contact your system administrator for more information." });
            //}
            //get database name
            string db = companyRes;
            SaleReport saleReport = new SaleReport(db);
            NlService nlService = new NlService(db);
            AuthService authService = new AuthService();  
            var customer = nlService.getCustomerById(cust_id);
            var ledger = nlService.getCustomerLedgerCard(customer.CustCode,from,to,cust_id);
            var invBal = nlService.GetInvoiceBalanceBroughtForward(cust_id, from);
            var payBal = nlService.GetPaymentBalance(cust_id, from);
            var crnBal = nlService.GetCRNBalance(cust_id, from);
            var reversalBal = nlService.GetReversalBalance(cust_id, from);
            var lic = nlService.GetCompanyDetail();
            return Ok(new { customerledgercard = ledger, customerinfo = customer, infoCustomer = customer, BalanceData = invBal, PayBalanceData = payBal, CrnBalanceData = crnBal, ReversalBalanceData = reversalBal, companyinfo = lic });
        }

    }
}
