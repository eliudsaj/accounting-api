using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Settings
{
    public class Departments
    {
        public int DpId { get; set; }
        public int DpBranch { get; set; }
        public string DpName { get; set; }
        public int DpHead { get; set; }
        public string DpRef { get; set; }

        public int BranchId { get; set; }
        public string BranchName { get; set; }

        public string DepartmentHead_firstname { get; set; }
        public string DepartmentHead_lastname { get; set; }
    }
}
