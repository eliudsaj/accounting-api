using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class CustomerStatementModel
    {
        public DateTime Invoicedate { get; set; }
        public decimal Invoicetotalamount { get; set; }
        public string Docref { get; set; }
        public int DocumentRef { get; set; }
        public int SLJrnlNo { get; set; }
        public bool HasCreditNote { get; set; }
        public string NlaccCode { get; set; }
        public string origin_status { get; set; }
        public string invoicenumber { get; set; }
        public string description { get; set; }
        public string StatementDescription { get; set; }
        public string Pymode { get; set; }
        public DateTime TranDate { get; set; }
        public string NlAccName { get; set; }
        public string TranFrom { get; set; }
        public int journalId { get; set; }
        public int paymentId { get; set; }
        public DateTime? paymentDate { get; set; }
        public Decimal Cr { get; set; }
        public Decimal Dr { get; set; }
    }

    public class AccountsReceivableAgeingReport
    {
        public string CustCode { get; set; }
        public string customer { get; set; }
        public Decimal Over90Days { get; set; }
        public Decimal sixtoneto90days { get; set; }
        public Decimal thirtyoneto60days { get; set; }
        public Decimal oneto30days { get; set; }
        public Decimal Current { get; set; }
    }

    public class DetailedAccountsReceivableAgeingReport
    {
        public Decimal Balance { get; set; }
        public string Invoice { get; set; }
        public DateTime DueDate { get; set; }
        public string TimeFrame { get; set; }
        public DateTime TransDate { get; set; } 
    }
}
