using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.Invoices
{
    public class NLaccounts
    {
        public int NLAccCode { get; set; }
        public string NLAccName { get; set; }
        public string GroupCode { get; set; }
        public string CurCode { get; set; }
    }
}
