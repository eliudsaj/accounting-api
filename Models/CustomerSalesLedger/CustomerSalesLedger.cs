using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.CustomerSalesLedger
{
    public class CustomerSalesLedger
    {
        public string DocRef { get; set; }
        public int DocumentRef { get; set; }
        public string Type { get; set; }
        public string PyChequeNumber { get; set; }
        public int SLJrnlNo { get; set; }
        public string TransactionType { get; set; }
        public string Pymode { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool HasCreditNote { get; set; }
        public DateTime DueDate { get; set; } 
        public DateTime Date { get; set; }
        public decimal Dr { get; set; }
        public string origin_status { get; set; }
        public string invoicenumber { get; set; }
        public int JournalId { get; set; }
        public decimal Cr { get; set; }
        public int CustId { get; set; }
        public decimal InvoiceBalance { get; set; }
        public decimal PaymentBalance { get; set; }
        public decimal CrnBalance { get; set; }
        public decimal ReversalBalance { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
    }
}
