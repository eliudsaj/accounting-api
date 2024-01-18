using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Controllers.NlController
{
    public class NlJournalHeader
    {
        public int NlJrnlNo { get; set; }
        public string NlJrnlDesc { get; set; }
        public DateTime TranDate { get; set; }
        public DateTime MEndDate { get; set; }
        public decimal TranPeriod { get; set; }
        public decimal TranYear { get; set; }
        public string TranFrom { get; set; }
        public string TranType { get; set; }
        public long SlJrnlNo { get; set; }
        public long PlJrnlNo { get; set; }
        public int? ModuleId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Period { get; set; }
    }
    public class JournalInputRequest
    {
        public string accountcode { get; set; }
        public string action { get; set; }
        public string description { get; set; }
        public decimal amount { get; set; }
        public string FolioNo { get; set; }
    }
    public class MyJournalInputRequest
    {
        public string Description { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime TransactionDate { get; set; }
        public List<JournalInputRequest> JournalList { get; set; }
    }

    public class AccountsReceivableBroughtForward
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public DateTime TransactionDate { get; set; }
        public string JournalRef { get; set; }
        public string AccountName { get; set; }
    }
}
