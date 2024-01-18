using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class Warehouse
    {
        public string wh_ref { get; set; }
        public string wh_code { get; set; }
        public string wh_desc { get; set; }
        public string wh_address_1 { get; set; }
        public string wh_address_2 { get; set; }
        public string wh_address_3 { get; set; }
        public string wh_address_4 { get; set; }
        public string wh_type { get; set; }
        public string wh_stage { get; set; }
        public DateTime wh_modifiedon { get; set; }
        public DateTime wh_createdon { get; set; }
        public int wh_branch { get; set; }
    }
}
