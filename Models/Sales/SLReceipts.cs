using pyme_finance_api.Models.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class SLReceipts
    {
        [Key]
        public int pyID { get; set; }
        public int cust_id { get; set; }
        public int pyRef { get; set; }
        public DateTime pyDate { get; set; }
        public int pyInvRef { get; set; }
        public decimal pyPayable { get; set; }
        public decimal pyPaid { get; set; }
        public decimal pyBalance { get; set; }
        public string pyMode { get; set; }
        public string pyChequeNumber { get; set; }
        public string pyReceivedFrom { get; set; }
        public string pyAdditionalDetails { get; set; }
        public DateTime pyProcessDate { get; set; }
        public int pyUser { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string CustFirstName { get; set; }
        public string CustLastName { get; set; }
        public string CustEmail { get; set; }
        public string CustCompany { get; set; }
        public string CustAddress { get; set; }
        public string currentCustName { get; set; }
        public string CustType { get; set; }
        public int DocRef { get; set; }
        public int SLJrnlNo { get; set; }
        public string CrCode { get; set; }
        public string DocPrefix { get; set; }
        public decimal invAmount { get; set; }
        public DateTime invDate { get; set; }
        public DateTime dueDate { get; set; }
        public int paymentdays { get; set; }
        public string fname { get; set; }
        public decimal rate { get; set; }
        public decimal allocation_remainder { get; set; }
    }
    public class ReceivePaymentRequest
    {
        public int CustId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string AdditionalDetails { get; set; }
        public string PaymentDate { get; set; }
        public int ReceiptId { get; set; }
        //public decimal TotalAllocated { get; set; }
        public License license { get; set; }
        public string Nominal { get; set; }
        public string ReceivedFrom { get; set; }
        public string CurrenctCustomerName { get; set; }
        public string ChequeNumber { get; set; }
        public decimal rate { get; set; }
        public string Period { get; set; }
        public List<AllocatedInvoice> AllocatedInvoices { get; set; }        
    }
    public class AllocatedInvoice
    {
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        //public decimal AR { get; set; }
        public int InvoiceRef { get; set; }
        public string Currency { get; set; }
        public decimal AR { get; set; }
        public decimal ALLOCATE { get; set; }
    }
    public class RemitanceReport
    {
        public DateTime ReceiptDate { get; set; }
        public string CustName { get; set; }
        public string Pin { get; set; }
        public string PostalAddress { get; set; }
        public int PyId { get; set; }
        public string PaymentMode { get; set; }
        public License License { get; set; }
        public string ChequeNumber { get; set; }
        public float ReceiptAmount { get; set; }
        public float ReceiptBalance { get; set; }
        public List<RemitanceData> RemitanceData { get; set; }
    }

    public class RemitanceData
    {
        public string InvoiceRef { get; set; }
        public int InvoiceId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public DateTime AllocatedOn { get; set; }
        public decimal InvoiceBalance { get; set; }
    }
}
