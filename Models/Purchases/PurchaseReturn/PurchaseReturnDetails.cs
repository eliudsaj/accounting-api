using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseReturn
{
    public class PurchaseReturnDetails
    {
        public string pr_ref { get; set; }
        public int pr_pl_invref { get; set; }
        public string pr_item_name { get; set; }
        public int pr_item_qty { get; set; }
        public string pr_reason { get; set; }
    }
}
