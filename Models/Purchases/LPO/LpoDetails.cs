using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.LPO
{
    public class LpoDetails
    {
        public int PldID { get; set; }
        public int PldRef { get; set; }
        public string VatPerc { get; set; }
        public decimal VatAmt { get; set; }
        public string StkDesc { get; set; }

        public string StkInfo { get; set; }

        public int UserID { get; set; }
        public int ProdQty { get; set; }
        public decimal Total { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime PldDate { get; set; }

        public string InvtName { get; set; }
    }
}
