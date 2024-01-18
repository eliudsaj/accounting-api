using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Controllers.NlController
{
    public class NLJournalDetails
    {
        public int SlJrnlNo { get; set; }
        public int? ModuleId { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int? NlJrnlNo { get; set; }
        public int JrnlSlNo { get; set; }
        public string NlAccCode { get; set; }
        public Decimal Dr { get; set; }
        public Decimal Cr { get; set; }
        public Decimal Amount { get; set; }
        public string Narration { get; set; }
        public string SLNarration { get; set; }
        public bool IsForex { get; set; }
        public string FolioNo { get; set; }
        public bool IsCleard { get; set; }
        public DateTime ClearDate { get; set; }
        public bool FCCleared { get; set; }
        public DateTime FCClearDate { get; set; }
        public Decimal VatAmount { get; set; }
        public string Period { get; set; }
    }
}
