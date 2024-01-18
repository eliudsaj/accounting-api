using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.StockInv
{
    public class StockTakeDetails
    {
        public int stk_id { get; set; }
        public DateTime? stk_date { get; set; }
        public int stk_item_id { get; set; }
        public string stk_item_name { get; set; }
        public int store_qty { get; set; }
        public int curr_qty { get; set; }
        public string stk_ref { get; set; }
        public bool stk_has_issue { get; set; }
    }
}
