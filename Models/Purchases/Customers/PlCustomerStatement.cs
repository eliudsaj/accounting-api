using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.Customers
{
    public class PlCustomerStatement
    {
        public DateTime transactionDate { get; set; }
        public string documentRef { get; set; }
        public string description { get; set; }
        public string transtype { get; set; }
        public string Pymode { get; set; }
        public decimal Dr { get; set; }
        public decimal Balance { get; set; }
        public decimal Cr { get; set;  }
        public int JournalId { get; set; }
        public DateTime invDate { get; set; }
        public decimal InvoiceBalance { get; set; }
        public decimal PayBalance { get; set; }
        public decimal CrnBalance { get; set; }
    }
}
