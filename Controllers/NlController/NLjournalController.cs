using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Service.NlServices;

namespace pyme_finance_api.Controllers.NlController
{
    [Route("api/[controller]")]
    [ApiController]
    public class NLjournalController : ControllerBase
    {
        dbconnection myDbconnection = new dbconnection();
        private IConfiguration _configuration;

        public NLjournalController(IConfiguration config)
        {
            _configuration = config;
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_NljournalsDetailsByJournalNo(int journalNo)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nljournaldetails = nlService.GetNLJournalByJournalNo(journalNo);
            return Ok(new { data = nljournaldetails });
        }
        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult AddJournalDetails(MyJournalInputRequest myJournalInputRequest)
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
            string db = companyRes;
            NlService nlService = new NlService(db);
            var data = nlService.customJournalEntry(myJournalInputRequest.JournalList, myJournalInputRequest.TransactionDate, myJournalInputRequest.PeriodMonth, myJournalInputRequest.PeriodYear);
            return Ok(data);
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult NljournalsDetails(string code)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var result = nlService.NLReportDetails(code);
            return Ok(result);
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult AccountGroupPerDate(string code, DateTime from, DateTime to)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var result = nlService.AccountGroupReportDetailsByDate(code, from, to);
            //Getting Company Details
            var lic = nlService.GetCompanyDetail();
            return Ok(new { nlReport = result , licenseData = lic, Code = 200});
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult AccountGroupPerPeriod(string code, string periodfrom, string periodto)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var result = nlService.AccountGroupReportDetailsByPeriod(code, periodfrom, periodto);
            //Getting Company Details
            var lic = nlService.GetCompanyDetail();
            return Ok(new { nlReport = result, licenseData = lic, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_Nljournals()
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

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService NlService = new NlService(db);
            var nljournals = NlService.GettingNlJournal();
            return Ok(new { data = nljournals, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult Get_NljournalsDetails(int journalId)
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
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nljournaldetails = nlService.GetNLJournalDetails(journalId);
            var lic = nlService.GetCompanyDetail();
            return Ok(new { data = nljournaldetails, license = lic, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetAllBanks()
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

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService NlService = new NlService(db);
            var nljournals = NlService.GettingAllBankList();
            return Ok(new { nlaccountData = nljournals, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetBankDetailsPerPeriod(string code, string periodFrom, string periodTo)
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

            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nljournals = nlService.GettingBankDetailsPerPeriod(code, periodFrom, periodTo);
            return Ok(new { BankResponse = nljournals, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult GetBankPerDate(string code, DateTime from, DateTime to)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var nljournals = nlService.GettingBankDetailsPerDate(code, from, to);
            return Ok(new { BankResponse = nljournals, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult BankReconciliation(string code)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var cashBook = nlService.GetBankBalance(code);
            var bankReconcilations = nlService.BankReconciliationReports(code);
            return Ok(new { BankResponse = bankReconcilations, CashBook = cashBook,  Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult CalculateCashBookBalancePerStatementDate(string code, DateTime statementDate)
        {
            var companyRes = "";
            int userId = 0;
            string authHeader = Request.Headers[HeaderNames.Authorization];
            string jwtHeader = authHeader.Split(' ')[1];
            jwt_token jwt = new jwt_token(_configuration);
            var tokenData = jwt.GetClaim(jwtHeader, "DB");
            userId = Int32.Parse(jwt.GetClaim(jwtHeader, "Id"));
            if (tokenData != "")
            {
                companyRes = tokenData;
            }
            if(userId == 0)
            {
                return BadRequest(new { message = "Missing User Reference. Page verification Failed!!" });
            }
            string db = companyRes;
            NlService NlService = new NlService(db);
            var result = NlService.GetCashBookBalancePerDate(code, statementDate);
            return Ok(new { BalanceList = result, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult UnclearBankReconciliation(string code)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var result = nlService.GettingUnclearedReconciliationRecord(code);
            //Getting Company Details
            var lic = nlService.GetCompanyDetail();
            return Ok(new { BankResponse = result, LicenseData = lic, Code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult ClearedBankReconciliation(string code)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var result = nlService.GettingClearedReconciliation(code);
            //Getting company Details
            var lic = nlService.GetCompanyDetail();
            return Ok(new { BankResponse = result, LicenseData = lic, code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult BankHistoryByDate(string code, DateTime from, DateTime to)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var records = nlService.GetbankHistory(code, from, to);
            return Ok(new { BankHistory = records, code = 200 });
        }
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public ActionResult BankHistoryByPeriod(string code, string periodFrom, string periodTo)
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
            //check user id
            if (userId == 0)
            {
                return BadRequest(new { message = "Missing user reference. Page verification failed" });
            }
            string db = companyRes;
            NlService nlService = new NlService(db);
            var bankReconciliation = nlService.GetBankHistoryByPeriod(code, periodFrom, periodTo);
            return Ok(new { BankHistory = bankReconciliation, code = 200 });
        }
        [Route("[action]")]
        [HttpPost]
        [Authorize]
        public ActionResult PostBankStatements(CashBookHeader cashBook)
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
            string db = companyRes;
            NlService nlService = new NlService(db);
            var data = nlService.BankStatementAdding(cashBook.CashBookDetails, userId, cashBook.CreatedOn, cashBook.bankBalance, cashBook.UnclearedBalance, cashBook.abBalance, cashBook.CashBookBalance, cashBook.cdBalance, cashBook.PeriodFrom, cashBook.PeriodTo, cashBook.DateFrom, cashBook.DateTo);
            if(data.Httpcode == 400)
            {
                return BadRequest(data);
            }
            return Ok(data);
        }
    }
}
