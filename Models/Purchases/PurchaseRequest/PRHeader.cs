using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseRequest
{
    public class PRHeader
    {
        public int po_id { get; set; }
        public DateTime po_date { get; set; }
        public string po_prefix { get; set; }
        public int po_ref { get; set; }
        public int po_user { get; set; }
        public int po_receiver { get; set; }
        public decimal po_total { get; set; }
        public string po_status { get; set; }
        public int po_approvedby { get; set; }
        public int po_sender_signature { get; set; }
        public DateTime po_transdate { get; set; }
        public int po_approval_signature { get; set; }
        public bool po_has_lpo { get; set; }

        public PRDetails[] po_description { get; set; }

        public string SenderFirstName { get; set; }
        public string SenderLastName { get; set; }

        public string ApprovalFirstName { get; set; }
        public string ApprovalLastName { get; set; }

        public string SendSignatureData { get; set; }
        public string SendSignaturename { get; set; }
        public string ApproveSignatureData { get; set; }
        public string ApproveSignatureName { get; set; }
    }
}
