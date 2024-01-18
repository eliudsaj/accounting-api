using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseRequest
{
    public class PRDetails
    {
        public int pod_ref { get; set; }
        public string pod_itemname { get; set; }
        public int pod_qty { get; set; }
        public decimal pod_unitprice { get; set; }
        public decimal pod_total { get; set; }
        public decimal pod_vat_perc { get; set; }
        public decimal pod_vat_amt { get; set; }
        public int pod_itemid { get; set; }
    }
}
