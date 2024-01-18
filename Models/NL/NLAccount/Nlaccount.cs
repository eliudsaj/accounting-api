using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.NL.NLAccount
{
    public class Nlaccount
    {



        public string NlaccCode { get; set; }

        public string NlaccName { get; set; }


        public string GroupCode { get; set; }

 

        public string CurCode { get; set; }

        public bool IsMirrorAcc { get; set; }

        public string MaccCode { get; set; }

        public string AgroupCode { get; set; }

        public Decimal StatBalance { get; set; }

        public DateTime LastStatDate { get; set; }
    }
}
