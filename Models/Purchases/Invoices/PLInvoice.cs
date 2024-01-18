using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pyme_finance_api.Models.Sales;

namespace pyme_finance_api.Models.Purchases.Invoices
{
    public class PLInvoice
    {
        public int PLJrnNo { get; set; }
        public int NlJrnlNo { get; set; }
        public int PLCustID { get; set; }
        public DateTime TranDate { get; set; }
        public string Period { get; set; }
        public string DocRef { get; set; }
        public DateTime InvDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Balance { get; set; }
        public int CurrencyId { get; set; }
        public string PLDescription { get; set; }
        public int StaffId { get; set; }
        public string DocPrefix { get; set; }
        public bool HasCreditNote { get; set; }
        public string Additionals { get; set; }
        public bool Returned { get; set; }
        public bool HasReturned { get; set; }
        public decimal ttl_discount { get; set; }
        public DateTime CRNDate { get; set; }
        public decimal CRNTotal { get; set; }
        public decimal CRNVat { get; set; }
        public string CrnVatPercent { get; set; }
        public string CRNReference { get; set; }
        public int ReturnerSignature { get; set; }
        public string CrCode { get; set; }
        public string LpoNumber { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string CustomerRef { get; set; }
        public string account { get; set; }
        public int journalRef { get; set; }
        public string PLCustCode { get; set; }
        public string CustName { get; set; }
        public string PostalAddress { get; set; }
        public string PhysicalAddress { get; set; }
        public decimal AmtOwed { get; set; }
        public string VATNo { get; set; }
        public decimal Totals { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string fp_name { get; set; }
        public string CustEmail { get; set; }
        public virtual PLInvoiceDetails[] pl_inv_details { get; set; }
        public virtual List<InvoiceListDetailsData> InvoiceDetailsList { get; set; }
    }
    public class PLInvoiceV2
    {
        public int PLJrnNo { get; set; }
        public int NlJrnlNo { get; set; }
        public int PLCustID { get; set; }
        public DateTime TranDate { get; set; }
        public string Period { get; set; }
        public int DocRef { get; set; }
        public DateTime InvDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Balance { get; set; }
        public int CurrencyId { get; set; }
        public string PLDescription { get; set; }
        public int StaffId { get; set; }
        public string DocPrefix { get; set; }
        public bool HasCreditNote { get; set; }
        public string Additionals { get; set; }
        public bool Returned { get; set; }
        public bool HasReturned { get; set; }
        public decimal ttl_discount { get; set; }
        public DateTime CRNDate { get; set; }
        public int ReturnerSignature { get; set; }
        public string CrCode { get; set; }
        public string LpoNumber { get; set; }
        public int journalRef { get; set; }
        public string PLCustCode { get; set; }
        public string CustName { get; set; }
        public string PostalAddress { get; set; }
        public string PhysicalAddress { get; set; }
        public decimal AmtOwed { get; set; }
        public string VATNo { get; set; }
        public decimal Totals { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string fp_name { get; set; }
        public string CustEmail { get; set; }
        public virtual PLInvoiceDetails[] pl_inv_details { get; set; }
        public virtual List<InvoiceDetailsV2> InvoiceDetailsList { get; set; }
    }
    public class InvoiceDetailsV2
    {
        public string AccountName { get; set; }
        public virtual string Description { get; set; }
        public virtual string additionalDetails { get; set; }
        public virtual decimal ItemTotals { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal VAT { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal Total { get; set; }
        public virtual decimal DiscountAmt { get; set; }
        public virtual decimal Discount { get; set; }
        public int ItemId { get; set; }
        public decimal VatCode { get; set; }
        public decimal VatAmt { get; set; }
        //saving from Pl create Invoice
        public int ItemQty { get; set; }
        public decimal ItemUnitPrice { get; set; }
    }
}
