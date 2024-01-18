using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseReturn
{
    public class PurchaseReturnHeader
    {
        public string prh_ref { get; set; }
        public DateTime prh_date { get; set; }
        public int prh_pljrnl { get; set; }
        public int returnedby { get; set; }
        public int returner_signature { get; set; }
        public int approvedby { get; set; }
        public int approver_signature { get; set; }
        public int prh_staff { get; set; }
        public string status { get; set; }

        public int docref { get; set; }
        public string docprefix { get; set; }
        public DateTime InvDate { get; set; }
        public DateTime due_date { get; set; }
        public decimal totals { get; set; }
        public decimal balance { get; set; }

        public string Returner_firstname { get; set; }
        public string Returner_lastname { get; set; }
        public string Returner_signaturename { get; set; }
        public string Returner_signaturedata { get; set; }

        public string Approver_firstname { get; set; }
        public string Approver_lastname { get; set; }
        public string Approver_signaturename { get; set; }
        public string Approver_signaturedata { get; set; }

        public string CrCode { get; set; }

        public string CustCode { get; set; }
        public string InvCustomerName { get; set; }
    }
}
