using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.LPO
{
    public class LpoHeader
    {
        public int LID { get; set; }
        public int LPOCustID { get; set; }
        public DateTime LPODate { get; set; }
        public DateTime TransDate { get; set; }
        public string Prefix { get; set; }
        public int DocRef { get; set; }
        public int CurrencyID { get; set; }
        public string LDescription { get; set; }
        public int StaffID { get; set; }
        public decimal Totals { get; set; }
        public bool Invoiced { get; set; }

        public string CrCode { get; set; }
        public string PLCustCode { get; set; }

        public string Custtelephone { get; set; }

        public string custemail { get; set; }

        public string custaddress { get; set; }

        public string custpostal { get; set; }
        public string CustName { get; set; }

        public LpoDetails[] lpo_Details { get; set; }

        public string UFirstName { get; set; }
        public string ULastName { get; set; }
    }
}
