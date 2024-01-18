using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseReceipt
{
    public class PurchaseReceiptDetails
    {
        public int pd_id { get; set; }
        public DateTime pd_date { get; set; }
        public int pd_ref { get; set; }
        public int pd_item { get; set; }
        public int pd_qty { get; set; }
        public decimal pd_unitprice { get; set; }
        public string pd_vat_perc { get; set; }
        public decimal pd_vat_amt { get; set; }
        public decimal pd_totals { get; set; }
        public string StkDesc { get; set; }
    }
}
