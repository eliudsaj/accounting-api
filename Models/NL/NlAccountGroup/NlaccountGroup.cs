using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.NL.NlAccountGroup
{
    public class NlaccountGroup
    {

        public string GroupCode { get; set; }

        public string GroupName { get; set; }

        public string PriGroupName { get; set; }

        public string GroupType { get; set; }

        public string GroupSubType { get; set; }

        public int? GroupLevel { get; set; }

        public int? UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string SwverNo  { get; set; }

        public int? DefaultGroup  { get; set; }
    }


    public class AccountGroupReport
    {

        public string account { get; set; }

        public int dr { get; set; }
        public int cr { get; set; }
    }
}
