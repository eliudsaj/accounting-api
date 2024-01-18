using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseReceipt
{
    public class PurchaseReceipt
    {
        public int pr_id { get; set; }
        public DateTime pr_date { get; set; }
        public int pr_ref { get; set; }
        public string pr_prefix { get; set; }
        public int pr_customer { get; set; }
        public int pr_user { get; set; }
        public decimal pr_total { get; set; }
        public decimal pr_currency { get; set; }
        public string pr_additional { get; set; }
        public bool pr_invoiced { get; set; }
        public DateTime pr_transdate { get; set; }
        public bool pr_returned { get; set; }

        public string CrCode { get; set; }
        public string PLCustCode { get; set; }
        public string CustName { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public string VATNo { get; set; }


        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public PurchaseReceiptDetails[] pr_Details { get; set; }
    }
}
