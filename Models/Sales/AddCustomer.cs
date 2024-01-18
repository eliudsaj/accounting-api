using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace pyme_finance_api.Models.Sales
{
    public class AddCustomer
    {
        [Key]
        public int SLCustomerSerial { get; set; }
        public string CustCode { get; set; }
        public string CustFirstName { get; set; }
        public string Address { get; set; }
        public string PostalAddress { get; set; }
        public int CurCode { get; set; }
        public string VATNo { get; set; }
        public string VATpin { get; set; }
        public string CustEmail { get; set; }
        public string CustContact { get; set; }
        public int SLCTypeID { get; set; }
        public string CustLastName { get; set; }
        public string CustCompany { get; set; }
        public string CustType { get; set; }
        public int CreditTerms { get; set; }
        public double CustCreditLimit { get; set; }
        public string CurrCode { get; set; }
        public string Status { get; set; }
        public string CustRef { get; set; }
        public int CustBranch { get; set; }
        public decimal CustomerDept { get; set; }

        public decimal OpeningBalance { get; set; }

        public bool HasOpeningBalance { get; set; }
        public DateTime? OpeningBalanceDate { get; set; }
    }
}
