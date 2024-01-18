using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Purchases.LPO
{
    public class LPOSettings
    {
        public int LPO_SID { get; set; }
        public string LPO_SPrefix { get; set; }
        public int LPO_StartNO { get; set; }
        public string LPO_NumberingType { get; set; }
    }
}
