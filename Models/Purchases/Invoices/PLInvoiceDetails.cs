using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.Invoices
{
    public class PLInvoiceDetails
    {
        public int PLJrnlNo { get; set; }
        public int JrnlPLNo { get; set; }
        public decimal UnitPrice { get; set; }
        public string VatPerc { get; set; }
        public decimal VatAmt { get; set; }
        public string ProdGroupCode { get; set; }
        public string NLAccCode { get; set; }
        public string StkDesc { get; set; }
        public int UserID { get; set; }
        public int ProdQty { get; set; }
        public string EditQty { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal Total { get; set; }
        public int PLInvId { get; set; }
        public string ReturnReason { get; set; }
        public string chk { get; set; }
        public int ProdId { get; set; }
       

    }
}
