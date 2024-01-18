using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.StockInv
{
    public class WarehouseSummary
    {
        public int ws_id { get; set; }
        public int prod_id { get; set; }
        public string wh_ref { get; set; }
        public string bincode { get; set; }
        public int openstock { get; set; }
        public int qty_issued { get; set; }
        public int qty_received { get; set; }
        public int qty_adjusted { get; set; }
        public int qty_allocated { get; set; }
        public int rt_rct_qty { get; set; }
        public int rt_issue_qty { get; set; }
        public int qty_on_order { get; set; }
        public int physical_qty { get; set; }
        public int free_qty { get; set; }
        public int min_stock_qty { get; set; }
        public int max_stock_qty { get; set; }
        public DateTime modified_on { get; set; }
        public int ws_branch { get; set; }
        public DateTime ws_date { get; set; }

        public virtual long sum_physical_qty { get; set; }
        public virtual DateTime max_ws_date { get; set; }
        public virtual double date_Part { get; set; }
    }
}
