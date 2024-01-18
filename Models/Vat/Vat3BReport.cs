using System;

namespace pyme_finance_api.Models.Vat
{
    public class Vat3BReport
    {
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public string VATpin { get; set; }
        public int EN { get; set; }
        public string entrynumber { get; set; } 
        public string vatCode { get; set; }
        public string custname { get; set; }
        public string invdate { get; set; }
        public string inv { get; set; }
        public string period { get; set;}
        public int  PLJrnlNo { get; set; }
        public int SLJrnlNo { get; set; }
        public string origin_status { get; set; }
        public string invoicenumber { get; set; }
        public DateTime TransDate { get; set; }
        public string CustomerCode { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostAddress { get; set; }
        public string DocType { get; set; }
        public decimal Dr { get; set; }
        public decimal Cr { get; set; }
        public string Description { get; set; }
        public string PostNumber { get; set; }
        public string TransFrom { get; set; }
        public int Invoice_no { get; set; }
        public string Inv_Doc { get; set; }
    }
    public class DeliveryNoteReport
    {
        public int NoteId { get; set; }
        public string LpoNumber { get; set; }
        public DateTime DNoteDate { get; set; }
        public string StockDescription { get; set; }
        public int ProductQty { get; set; }
        public string CustomerName { get; set; }
        //public string CompanyName { get; set; }
        public string PostalAddress { get; set; }
        public string VatPin { get; set; }
    }
}
