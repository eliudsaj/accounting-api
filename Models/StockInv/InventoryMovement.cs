using System;

namespace pyme_finance_api.Models.StockInv
{
    public class InventoryMovement
    {
        public string InvtName { get; set; }
        public DateTime ws_date { get; set; }

        public int openstock { get; set; }
        public int qty_issued { get; set; }

        public int qty_received { get; set; }

        public int qty_allocated { get; set; }

        public int qty_adjusted { get; set; }

        public int physical_qty { get; set; }   

    }
}
