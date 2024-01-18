using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.AuditTrail
{
    public class AuditTrail
    {

        public int Id { get; set; }

        public int userId { get; set; }

        public string module { get; set; }

        public string action { get; set; }

        public DateTime createdOn { get; set; }
    }
}
