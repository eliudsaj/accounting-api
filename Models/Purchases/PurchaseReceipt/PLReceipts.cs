using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.PurchaseReceipt
{
    public class PLReceipts
    {
        public int pyID { get; set; }
        public int pyRef { get; set; }
        public DateTime pyDate { get; set; }
        public int pyInvRef { get; set; }
        public float pyPayable { get; set; }
        public float pyPaid { get; set; }
        public float pyBalance { get; set; }
        public string pyMode { get; set; }
        public string pyChequeNumber { get; set; }
        public string pyReceivedBy { get; set; }
        public string pyAdditionalDetails { get; set; }
        public DateTime pyProcessDate { get; set; }
        public int pyUser { get; set; }
        public bool pyCancelled { get; set; }
        public string pyCancelReason { get; set; }
        public int pyBranch { get; set; }
        public string Attachment_Image { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string pyCurr { get; set; }
        public string pyPrefix { get; set; }
        public string PLCustomer { get; set; }
        public int PLJrnlNo { get; set; }
        public string DocRef { get; set; }
        public string DocPrefix { get; set; }
        public string CrCode { get; set; }
        public string PhysicalAddress { get; set; }
        public string CustName { get; set; }
        public int SupplierID { get; set; }
        public decimal rate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal AllocationRemainder { get; set; }
    }

    public class PLReceiptsHeader
    {
        public int pyID { get; set; }
        public int pyRef { get; set; }
        public DateTime pyDate { get; set; }
        public int pyInvRef { get; set; }
        public float pyPayable { get; set; }
        public float pyPaid { get; set; }
        public float pyBalance { get; set; }
        public string pyMode { get; set; }
        public string pyChequeNumber { get; set; }
        public string pyReceivedBy { get; set; }
        public string pyAdditionalDetails { get; set; }
        public DateTime pyProcessDate { get; set; }
        public int pyUser { get; set; }
        public bool pyCancelled { get; set; }
        public string pyCancelReason { get; set; }
        public int pyBranch { get; set; }
        public string Attachment_Image { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string pyCurr { get; set; }
        public string pyPrefix { get; set; }
        public string PLCustomer { get; set; }
        public string PhysicalAddress { get; set; }
        public string DocPrefix { get; set; }
        public string DocRef { get; set; }
    }

    public class paymentReceiptReversal
    {
        public int ReceiptNumber { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class paymentReceiptReversalList
    {
        public int ReceiptNumber { get; set; }
        public string CreatedBy { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ReversedOn { get; set; }
        public int MyProperty { get; set; }
        public string ReversedBy { get; set; }
        public bool IsReversed { get; set; }
    }
}
