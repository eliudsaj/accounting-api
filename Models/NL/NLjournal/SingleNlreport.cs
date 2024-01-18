using System;
using System.Collections.Generic;

namespace pyme_finance_api.Models.NL.NLjournal
{
    public class SingleNlreport
    {
        public decimal Dr { get; set; }
        public decimal Cr { get; set; }
        public int Journal { get; set; }
        public string Description { get; set; }
        public string AccName { get; set; }
        public DateTime TranDate { get; set; }
        public decimal BalanceBroughtForward { get; set; }
        public string BalanceDetails { get; set; }
        public string Period { get; set; }
        public string Reference { get; set; }
        public string AccCode { get; set; }
        public string periodfrom { get; set; }
        public string periodto { get; set; }
    }
}
