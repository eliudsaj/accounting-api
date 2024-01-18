using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Vat
{
    public class VatAnalysis
    {
      public  string vatcode { get; set; }

       public decimal vat { get; set; }

        public decimal goods { get; set; }
    }

}
