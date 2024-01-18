using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales.Terms
{
    public class Allterms
    {
        [Key]
        public int tosID { get; set; }
        public string tosType { get; set; }
        public string terms { get; set; }
    }
}
