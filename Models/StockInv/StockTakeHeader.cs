using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.StockInv
{
    public class StockTakeHeader
    {
        public int sth_id { get; set; }
        public DateTime? sth_date { get; set; }
        public string sth_ref { get; set; }
        public string sth_name { get; set; }
        public int sth_staff { get; set; }
        public bool sth_approved { get; set; }
        public int approved_by { get; set; }
        public DateTime? approval_date { get; set; } = null;

        public bool has_issue { get; set; }
        public string staff_firstname { get; set; }
        public string staff_lastname { get; set; }

        public string approver_firstname { get; set; }
        public string approver_lastname { get; set; }

        public int approver_signature { get; set; }

        public string ApproverSignatureData { get; set; }
        public string ApproverSignatureName { get; set; }

        public bool has_approval_permission { get; set; }

        public StockTakeDetails[] take_description { get; set; }
    }
}
