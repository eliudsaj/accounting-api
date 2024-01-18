using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.Invoices
{
    [DataContract]
    public class PLinvoiceAnalysis
    {

        [DataMember(Name = "date")] public string date { get; set; }
        [DataMember(Name = "balance")] public float balance { get; set; }

        [DataMember(Name = "dueDate")] public DateTime dueDate { get; set; }
    }



    public class PLinvoiceResponse
    {

        public Dictionary<string,List<PLinvoiceAnalysis>> list { get; set; }
    }
}
