using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.ReportPages.Sales
{
    public class DeliveryCustomer
    {
        [Key]
        public int SLCustomerSerial { get; set; }
        public string CustFirstName { get; set; }
        public string CustLastName { get; set; }
        public string CustEmail { get; set; }
        public string CustCompany { get; set; }
        public string CustType { get; set; }

        public string Currency { get; set; }

        public string Tel { get; set; }

        public string Postal { get; set; }

        public string Address { get; set; }

    }
}
