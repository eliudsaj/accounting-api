using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.ReportPages.Sales
{
    public class InvoiceDetails
    {
        public int SLJrnlNo { get; set; }
        public int JrnlSLNo { get; set; }
        public float InvAmt { get; set; }
        public string VatCode { get; set; }
        public float VatAmt { get; set; }
        public string ProdGroupCode { get; set; }
        public string NLAccCode { get; set; }
        public string StkDesc { get; set; }
        public int UserID { get; set; }
        public string ItemSerial { get; set; }
        public int ItemQty { get; set; }
        public float ItemTotals { get; set; }
        public float ItemUnitPrice { get; set; }
        public virtual string Currency { get; set; }
        public float DiscountPerc { get; set; }
        public float DiscountAmt { get; set; }
        public string AdditionalDetails { get; set; }
        public bool InvPrinted { get; set; }
        public bool InvSettled { get; set; }
    }
}
