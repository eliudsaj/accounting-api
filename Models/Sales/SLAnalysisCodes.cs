using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class SLAnalysisCodes
    {

        public int Id { get; set; }
        public string AnalType { get; set; }

        public string AnalCode { get; set; }

        public string AnalDesc { get; set; }

        public string NLAccCode { get; set; }

        public DateTime ModifiedOn { get; set; }

    }
}
