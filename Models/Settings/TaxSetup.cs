using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Settings
{
    public class TaxSetup
    {
        [Key]
        public int VtId { get; set; }
        public string VtRef { get; set; }
        public float VtPerc { get; set; }
        public DateTime VtSetDate { get; set; }
        public DateTime VtModifyDate { get; set; }
        public bool VtActive { get; set; }
        public int VtBranch { get; set; }
        public decimal VAT_Balance { get; set; }
    }
}
