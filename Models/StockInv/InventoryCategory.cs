using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.StockInv
{
    public class InventoryCategory
    {
        public int cat_id { get; set; }
        public DateTime cat_entry_date { get; set; }
        public string cat_name { get; set; }
        public string cat_ref { get; set; }
        public int cat_branch { get; set; }

        public string branch_name { get; set; }
    }
}
