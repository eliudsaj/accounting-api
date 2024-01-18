using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class Invoice
    {
        [Key]
        public int SLJrnlNo { get; set; }
        public int NlJrnlNo { get; set; }
        public string CustCode { get; set; }
        public string Period { get; set; }
        public int DocRef { get; set; }
        public string DocPrefix { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime formatedduedate { get; set; }
        public string Deliverto { get; set; }
        public string InvoiceNumber { get; set; }
        public string status { get; set; }   
        public decimal ExchangeRate { get; set; }
        public DateTime TransDate { get; set; }
        public DateTime CRNDate { get; set; }
        public decimal CRNvat { get; set; }
        public int DeliveryCust { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime DeliveryDue { get; set; }
        public DateTime INVDate { get; set; }
        public int CustId { get; set; }
        public int StaffID { get; set; }
        public int PaymentDays { get; set; }
        public string CustomRef { get; set; }
        public int CurrencyId { get; set; }
        public DateTime DueDate { get; set; }
        public string StatementDescription { get; set; }
        public string SLDescription { get; set; }
        public string CRNReason { get; set; }
        public string CrnPercent { get; set; }
        public decimal ItemTotals { get; set; }
        public string INVTypeRef { get; set; }
        public bool Dispute { get; set; }
        public bool HasInvoice { get; set; }
        public bool IsReversed { get; set; }
        public bool HasCreditNote { get; set; }
        public int BranchRef { get; set; }
        public int journalRef { get; set; }
        public int PeriodNumber { get; set; }
        public int PeriodYear { get; set; }
        public string  test { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string AccName { get; set; }
        public string AccNumber { get; set; }
        public string SwiftCode { get; set; }
        public IFormFile lpoFile { get; set; }
        public List<Inventorylist> InvoiceDetailsList { get; set; }
        public virtual string CustFirstName { get; set; }
        public virtual string CustLastname { get; set; }
        public virtual string CustCompany { get; set; }
        public virtual string CustType { get; set; }
        public virtual string CrCode { get; set; }
        public virtual string CrName { get; set; }      
        public virtual string Address { get; set; }
        public virtual string PostalAddress { get; set; }
        public virtual string CustEmail { get; set; }
        public virtual string CustContact { get; set; }
        public virtual string UFirstName { get; set; }
        public virtual string ULastName { get; set; }
        public virtual string UEmail { get; set; }
        public decimal CreditNoteAmount { get; set; }   
        public int Branch { get; set; }
        public string fp_name { get; set; }
        public bool InvPrinted { get; set; }
        public bool InvSettled { get; set; }
        public decimal TotalBalance { get; set; }
        public DeliverTo DeliverToData { get; set; }
        public int NewInvoiceNumber { get; set; }
    }

    public class DeliverTo
    {
        public virtual string Address { get; set; }
        public virtual string PostalAddress { get; set; }
        public virtual string CustFirstName { get; set; }
        public virtual string CustLastname { get; set; }
        public virtual string CustCompany { get; set; }
        public virtual string CustType { get; set; }
    }
}
