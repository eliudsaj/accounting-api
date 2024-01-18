using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Settings
{
    public class FinancialPeriod
    {
        public int fp_id { get; set; }
        public string fp_ref { get; set; }
        public string fp_name { get; set; }
        public DateTime fp_trans_date { get; set; }
        public DateTime fp_openingdate { get; set; }
        public DateTime fp_closingdate { get; set; }
        public bool fp_active { get; set; }
        public DateTime fp_createdon { get; set; }
        public int fp_createdby { get; set; }
        public int fp_closedby { get; set; }
        public int fp_authorisedby { get; set; }
        public string fp_trans_option { get; set; }
        public string fp_date_mode { get; set; }
        public int fp_branch { get; set; }


        public string creator_fname { get; set; }
        public string creator_lname { get; set; }
        public string creator_email { get; set; }

        public string closer_fname { get; set; }
        public string closer_lname { get; set; }
        public string closer_email { get; set; }

        public string approver_fname { get; set; }
        public string approver_lname { get; set; }
        public string approver_email { get; set; }
    }
}
