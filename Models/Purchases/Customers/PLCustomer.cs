using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.Customers
{
    public class PLCustomer
    {
        public int CustID { get; set; }
        public string PLCustCode { get; set; }
        public string CustName { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public int CurrID { get; set; }
        public string VATNo { get; set; }
        public DateTime RegisterDate { get; set; }
        public int StaffID { get; set; }
        public string CrCode { get; set; }
        public int CrId { get; set; }
        public int CustBranch { get; set; }
        public decimal AmtOwed { get; set; }
        public string CustEmail { get; set; }
        public string PrimaryContact { get; set; }
        public string SecondayContact { get; set; }

        public decimal OpeningBalance { get; set; }

        public bool HasOpeningBalance { get; set; }
        public string ContactPerson { get; set; }

        public string BankName { get; set; }

        public string Branch { get; set; }
        
        public string AccountNumber { get; set; }

        public string MpesaNo { get; set; }

        public string PayBillNo { get; set; }

        public string Comment { get; set; }
        public DateTime? OpeningBalanceDate { get; set; }

    }
    public class BalanceBroughtForwardUpdate
    {
        public decimal AllocatedAmount { get; set; }
        public int ReceiptId { get; set; }
        public string CustomerId { get; set; }
        public int CustId { get; set; }
    }
}
